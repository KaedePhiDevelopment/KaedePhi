using KaedePhi.Tool.App.Config;
using KaedePhi.Tool.Common;

namespace KaedePhi.Tests.App;

public sealed class AppConfigServiceTests : IDisposable
{
    private readonly string _configDir = Path.Combine(
        Path.GetTempPath(),
        $"KaedePhi-config-tests-{Guid.NewGuid():N}"
    );

    private string ConfigPath => Path.Combine(_configDir, "config.yaml");

    public static bool IsWindows => OperatingSystem.IsWindows();

    [Theory]
    [InlineData("180")]
    [InlineData("-1")]
    [InlineData(".nan")]
    [InlineData("not-a-number")]
    [InlineData("[120, 180]")]
    public void Load_RemovesObsoleteBpmAndPreservesExistingSettings(string obsoleteBpm)
    {
        WriteConfig(
            $$"""
            logLevel: 2
            maxLogFiles: 9
            unbind:
              precision: 8
            convert:
              targetType: 3
              phigrosDefaultBpm: {{obsoleteBpm}}
              phigrosEasingPrecision: 16
              phigrosSpeedCutPrecision: 32
              streamOutput: true
            """
        );

        var service = new AppConfigService(_configDir);

        service.Config.LogLevel.Should().Be(2);
        service.Config.MaxLogFiles.Should().Be(9);
        service.Config.Unbind.Precision.Should().Be(8d);
        service.Config.Convert.TargetType.Should().Be(ChartFormatIds.PhigrosV3);
        service.Config.Convert.PhigrosEasingPrecision.Should().Be(16d);
        service.Config.Convert.PhigrosSpeedCutPrecision.Should().Be(32d);
        service.Config.Convert.StreamOutput.Should().BeTrue();
        var migratedYaml = File.ReadAllText(ConfigPath);
        migratedYaml.Should().NotContain("phigrosDefaultBpm");
        Directory.GetFiles(_configDir, "config.yaml.invalid.*").Should().BeEmpty();

        var reloaded = new AppConfigService(_configDir);
        reloaded.Config.Should().BeEquivalentTo(service.Config);
        File.ReadAllText(ConfigPath).Should().Be(migratedYaml);
    }

    [Theory]
    [InlineData("peSpeedCutCompress", "false")]
    [InlineData("peSpeedCutTolerance", "0.3")]
    public void Load_ContinuesRemovingPreviouslyObsoleteOptions(string key, string value)
    {
        WriteConfig(
            $"""
            convert:
              {key}: {value}
              phigrosSpeedCutPrecision: 32
            """
        );

        var service = new AppConfigService(_configDir);

        service.Config.Convert.PhigrosSpeedCutPrecision.Should().Be(32d);
        File.ReadAllText(ConfigPath).Should().NotContain(key);
        Directory.GetFiles(_configDir, "config.yaml.invalid.*").Should().BeEmpty();
    }

    [Fact]
    public void Load_RemovesRemovedRenderSectionAndPreservesExistingSettings()
    {
        WriteConfig(
            """
            logLevel: 2
            render:
              pixelsPerBeat: 120
              outputDir: render-output
            fit:
              tolerance: 0.25
            convert:
              phigrosDefaultBpm: 180
              phigrosSpeedCutPrecision: 32
            """
        );

        var service = new AppConfigService(_configDir);

        service.Config.LogLevel.Should().Be(2);
        service.Config.Fit.Tolerance.Should().Be(0.25);
        service.Config.Convert.PhigrosSpeedCutPrecision.Should().Be(32d);
        var migratedYaml = File.ReadAllText(ConfigPath);
        migratedYaml.Should().NotContain("render").And.NotContain("phigrosDefaultBpm");
        Directory.GetFiles(_configDir, "config.yaml.invalid.*").Should().BeEmpty();

        var reloaded = new AppConfigService(_configDir);
        reloaded.Config.Should().BeEquivalentTo(service.Config);
        File.ReadAllText(ConfigPath).Should().Be(migratedYaml);
    }

    [Fact]
    public void NewConfiguration_DoesNotWriteRenderSection()
    {
        _ = new AppConfigService(_configDir);

        File.ReadAllText(ConfigPath).Should().NotContainEquivalentOf("render");
    }

    [Fact]
    public void Load_HandlesConvertSectionContainingOnlyObsoleteBpm()
    {
        WriteConfig("convert: { phigrosDefaultBpm: 180 }\n");

        var service = new AppConfigService(_configDir);

        service.Config.Convert.TargetType.Should().Be(ChartFormatIds.RePhiEdit);
        File.ReadAllText(ConfigPath).Should().NotContain("phigrosDefaultBpm");
        Directory.GetFiles(_configDir, "config.yaml.invalid.*").Should().BeEmpty();
    }

    [Fact]
    public void Load_DoesNotRewriteConfigurationWithoutObsoleteOptions()
    {
        const string yaml = "convert: { targetType: PhigrosV3, phigrosSpeedCutPrecision: 32 }\n";
        WriteConfig(yaml);

        var service = new AppConfigService(_configDir);

        service.Config.Convert.TargetType.Should().Be(ChartFormatIds.PhigrosV3);
        service.Config.Convert.PhigrosSpeedCutPrecision.Should().Be(32d);
        File.ReadAllText(ConfigPath).Should().Be(yaml);
    }

    [Fact]
    public void NewConfiguration_DoesNotWriteObsoleteBpm()
    {
        var service = new AppConfigService(_configDir);

        File.ReadAllText(ConfigPath).Should().NotContain("phigrosDefaultBpm");
        service.Config.Convert.PhigrosSpeedCutPrecision = 32d;
        service.Save().Should().BeTrue();

        File.ReadAllText(ConfigPath).Should().NotContain("phigrosDefaultBpm");
        new AppConfigService(_configDir).Config.Convert.PhigrosSpeedCutPrecision.Should().Be(32d);
    }

    [Fact]
    public void Load_StillBacksUpInvalidUnrecognizedOptions()
    {
        const string yaml = "convert: { unrecognizedOption: 1 }\n";
        WriteConfig(yaml);

        var service = new AppConfigService(_configDir);

        service.Config.Convert.PhigrosSpeedCutPrecision.Should().Be(64d);
        var backup = Directory
            .GetFiles(_configDir, "config.yaml.invalid.*")
            .Should()
            .ContainSingle()
            .Which;
        File.ReadAllText(backup).Should().Be(yaml);
        File.ReadAllText(ConfigPath).Should().NotContain("unrecognizedOption");
    }

    [Fact(
        SkipUnless = nameof(IsWindows),
        Skip = "仅 Windows 的文件共享锁会阻止替换已打开的配置文件。"
    )]
    public void Load_WhenMigrationWriteFails_KeepsValidSettingsAndOriginalFile()
    {
        const string yaml = """
            convert:
              phigrosDefaultBpm: 180
              phigrosSpeedCutPrecision: 32
            """;
        WriteConfig(yaml);
        AppConfigService service;

        using (
            var lockedFile = new FileStream(
                ConfigPath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read
            )
        )
        {
            service = new AppConfigService(_configDir);

            service.Config.Convert.PhigrosSpeedCutPrecision.Should().Be(32d);
            File.ReadAllText(ConfigPath).Should().Be(yaml);
            Directory.GetFiles(_configDir, "config.yaml.invalid.*").Should().BeEmpty();
            Directory.GetFiles(_configDir, "*.tmp").Should().BeEmpty();
        }

        service.Save().Should().BeTrue();
        File.ReadAllText(ConfigPath).Should().NotContain("phigrosDefaultBpm");
    }

    private void WriteConfig(string yaml)
    {
        Directory.CreateDirectory(_configDir);
        File.WriteAllText(ConfigPath, yaml);
    }

    public void Dispose()
    {
        if (Directory.Exists(_configDir))
            Directory.Delete(_configDir, true);
    }
}
