using KaedePhi.Tool.App.Config;
using KaedePhi.Tool.App.Gui.ViewModels;
using KaedePhi.Tool.Common;

namespace KaedePhi.Tests.App;

public sealed class SettingsViewModelTests : IDisposable
{
    private readonly string _configDir = Path.Combine(
        Path.GetTempPath(),
        $"KaedePhi-settings-tests-{Guid.NewGuid():N}"
    );

    [Fact]
    public void Save_PreservesSettingsNotShownOnPage()
    {
        var service = new AppConfigService(_configDir);
        service.Config.LogLevel = 1;
        service.Config.Render.OutputDir = "render-output";
        service.Config.Unbind.DryRun = true;
        service.Config.Convert.TargetType = ChartFormatIds.PhigrosV3;
        service.Config.Convert.PhiFansDiscontinuityBeatPrecision = 32;
        service.Config.Convert.PhigrosNegativeAlphaStep = 8d;
        service.Save().Should().BeTrue();
        var vm = new SettingsViewModel(service);

        vm.Draft.Fit.Tolerance = 0.25;
        vm.Cut.Precision = 16;
        vm.OnSaveClicked();

        var reloaded = new AppConfigService(_configDir).Config;
        reloaded.Fit.Tolerance.Should().Be(0.25);
        reloaded.Cut.Precision.Should().Be(16d);
        reloaded.LogLevel.Should().Be(1u);
        reloaded.Render.OutputDir.Should().Be("render-output");
        reloaded.Unbind.DryRun.Should().BeTrue();
        reloaded.Convert.TargetType.Should().Be(ChartFormatIds.PhigrosV3);
        reloaded.Convert.PhiFansDiscontinuityBeatPrecision.Should().Be(32);
        reloaded.Convert.PhigrosNegativeAlphaStep.Should().Be(8d);
        service.Config.Should().BeEquivalentTo(reloaded);
    }

    [Fact]
    public void Edit_DoesNotAffectRunningConfigurationUntilSaved()
    {
        var service = new AppConfigService(_configDir);
        var vm = new SettingsViewModel(service);

        vm.Unbind.Precision = 8;

        service.Config.Unbind.Precision.Should().NotBe(8d);
        vm.OnSaveClicked();
        service.Config.Unbind.Precision.Should().Be(8d);

        vm.Unbind.Precision = 4;
        service.Config.Unbind.Precision.Should().Be(8d);
    }

    [Fact]
    public void Save_WithInvalidValue_KeepsRunningConfigurationAndReportsFailure()
    {
        var service = new AppConfigService(_configDir);
        var vm = new SettingsViewModel(service);
        var original = service.Config.Unbind.Precision;

        vm.Unbind.Precision = 0;
        vm.OnSaveClicked();

        service.Config.Unbind.Precision.Should().Be(original);
        new AppConfigService(_configDir).Config.Unbind.Precision.Should().Be(original);
        vm.StatusText.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void Reset_RestoresDefaultsAndRefreshesBindings()
    {
        var service = new AppConfigService(_configDir);
        var vm = new SettingsViewModel(service);
        vm.Unbind.Precision = 8;
        vm.OnSaveClicked();
        var raisedAll = false;
        vm.PropertyChanged += (_, e) => raisedAll |= string.IsNullOrEmpty(e.PropertyName);

        vm.OnResetClicked();

        raisedAll.Should().BeTrue();
        vm.Unbind.Precision.Should().Be(new AppConfig().Unbind.Precision);
        service.Config.Unbind.Precision.Should().Be(new AppConfig().Unbind.Precision);
    }

    [Fact]
    public void ToolDefaults_ClearingClassicModeClearsDisableCompress()
    {
        var model = new ToolDefaultsConfig { ClassicMode = true, DisableCompress = true };
        var vm = new ToolDefaultsViewModel(model, compressRequiresClassicMode: true);
        var changed = new List<string?>();
        vm.PropertyChanged += (_, e) => changed.Add(e.PropertyName);

        vm.DisableCompressEnabled.Should().BeTrue();
        vm.ClassicMode = false;

        model.ClassicMode.Should().BeFalse();
        model.DisableCompress.Should().BeFalse();
        vm.DisableCompressEnabled.Should().BeFalse();
        changed
            .Should()
            .Contain([
                nameof(ToolDefaultsViewModel.ClassicMode),
                nameof(ToolDefaultsViewModel.DisableCompressEnabled),
                nameof(ToolDefaultsViewModel.DisableCompress),
            ]);
    }

    [Fact]
    public void ToolDefaults_WithoutClassicModeRequirementAlwaysAllowsCompressToggle()
    {
        var vm = new ToolDefaultsViewModel(
            new ToolDefaultsConfig(),
            compressRequiresClassicMode: false
        );

        vm.DisableCompressEnabled.Should().BeTrue();
    }

    public void Dispose()
    {
        if (Directory.Exists(_configDir))
            Directory.Delete(_configDir, true);
    }
}
