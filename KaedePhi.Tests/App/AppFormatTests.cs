using KaedePhi.Core.Intermediate.Model;
using KaedePhi.Tool.App.Cli.Commands;
using KaedePhi.Tool.App.Cli.Infrastructure;
using KaedePhi.Tool.App.Config;
using KaedePhi.Tool.App.Gui.Services;
using KaedePhi.Tool.App.Gui.ViewModels;
using KaedePhi.Tool.Common;
using KaedePhi.Tool.Converter;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace KaedePhi.Tests.App;

[Collection("共享格式注册表")]
public sealed class AppFormatTests
{
    [Fact]
    public void ConvertCommand_RemovesObsoletePhigrosBpmOption()
    {
        var command = ConvertCommand.Create();

        command
            .Parse(
                ["--input", "input.json", "--target", ChartFormatIds.PhigrosV3, "--phigros-bpm", "180"]
            )
            .Errors.Should().NotBeEmpty();
        command
            .Parse(
                [
                    "--input",
                    "input.json",
                    "--target",
                    ChartFormatIds.PhigrosV3,
                    "--phigros-easing-precision",
                    "32",
                ]
            )
            .Errors.Should().BeEmpty();
    }

    [Fact]
    public void ExportViewModel_PreservesRemainingPhigrosDefaults()
    {
        var config = new ConvertDefaultsConfig
        {
            PhigrosEasingPrecision = 16d,
            PhigrosMisalignedXyEventPrecision = 32d,
            PhigrosAlphaCutPrecision = 48d,
            PhigrosSpeedCutPrecision = 96d,
        };
        var vm = new ExportViewModel { SelectedFormat = ChartFormatIds.PhigrosV3 };

        vm.ApplyConversionDefaults(config);

        vm.ShowPhigrosOptions.Should().BeTrue();
        vm.PhigrosEasingPrecision.Should().Be(16d);
        vm.PhigrosMisalignedXyEventPrecision.Should().Be(32d);
        vm.PhigrosAlphaCutPrecision.Should().Be(48d);
        vm.PhigrosSpeedCutPrecision.Should().Be(96d);
    }

    [Fact]
    public void ConvertCommand_AcceptsAndCompletesRegisteredFormatIds()
    {
        const string id = "tests.app-cli";
        var format = ChartFormatDescriptor.Create(id, "bin", exporter: (_, _, _, _, _, _) => Task.CompletedTask);
        ChartFormatRegistry.Register(format);
        try
        {
            var command = ConvertCommand.Create();
            command.Parse(["--input", "input.bin", "--target", id]).Errors.Should().BeEmpty();
            const string commandLine = "--target tests.app-";
            command.Parse(commandLine).GetCompletions(commandLine.Length)
                .Select(completion => completion.Label).Should().Contain(id);
        }
        finally
        {
            ChartFormatRegistry.UnregisterFormat(id);
        }
    }

    [Fact]
    public async Task CliAndGuiServices_UseRegisteredStreamOnlyFormat()
    {
        const string id = "tests.app-stream";
        const string header = "APP_FORMAT_TEST";
        var format = ChartFormatDescriptor.Create(
            id, "kptest",
            streamImporter: async (stream, options, _, ct) =>
            {
                using var reader = new StreamReader(stream, leaveOpen: true);
                (await reader.ReadLineAsync(ct)).Should().Be(header);
                return new Chart { Meta = new Meta { Name = (string)options! + await reader.ReadToEndAsync(ct) } };
            },
            exporter: (chart, path, _, options, _, ct) =>
                File.WriteAllTextAsync(path, chart.Meta.Name + (string?)options, ct),
            importOptionsFactory: () => "import:"
        );
        ChartFormatRegistry.Register(format, async (stream, ct) =>
        {
            using var reader = new StreamReader(stream, leaveOpen: true);
            return await reader.ReadLineAsync(ct) == header;
        });
        var input = Path.Combine(Path.GetTempPath(), $"kaedephi-{Guid.NewGuid():N}.kptest");
        var output = Path.Combine(Path.GetTempPath(), $"kaedephi-{Guid.NewGuid():N}.kptest");
        try
        {
            var ct = TestContext.Current.CancellationToken;
            await File.WriteAllTextAsync(input, $"{header}\nchart", ct);
            var cli = new ChartService();
            var chart = await cli.LoadKpcAsync(input, null, ct);
            chart.Should().NotBeNull();
            chart!.Meta.Name.Should().Be("import:chart");
            await ChartService.SaveAsAsync(
                chart, output, id.ToUpperInvariant(), new SaveAsOptions { ExportOptions = ":cli" }, ct
            );
            (await File.ReadAllTextAsync(output, ct)).Should().Be("import:chart:cli");

            using var log = new LogService();
            var gui = new GuiChartService(log);
            (await gui.DetectChartFormatAsync(input, ct)).Should().BeSameAs(format);
            await gui.LoadChartAsync(input, stream: false, ct);
            gui.SourceFormat.Should().Be(id);
            gui.CurrentChart!.Meta.Name.Should().Be("import:chart");
            await gui.ExportChartAsync(id, output, stream: false, indented: false, exportOptions: ":gui", ct);
            (await File.ReadAllTextAsync(output, ct)).Should().Be("import:chart:gui");
            gui.Clear();
            gui.IsLoaded.Should().BeFalse();
            gui.SourceFormat.Should().BeEmpty();
        }
        finally
        {
            ChartFormatRegistry.UnregisterFormat(id);
            if (File.Exists(input))
                File.Delete(input);
            if (File.Exists(output))
                File.Delete(output);
        }
    }

    [Fact]
    public void ExportViewModel_RefreshesRegistryFormatsAndUsesRegisteredExtension()
    {
        const string id = "tests.app-binary";
        var vm = new ExportViewModel();
        vm.AvailableFormats.Should().NotContain(id);
        var format = ChartFormatDescriptor.Create(id, "bin", exporter: (_, _, _, _, _, _) => Task.CompletedTask);
        ChartFormatRegistry.Register(format);
        try
        {
            vm.RefreshAvailableFormats();
            vm.AvailableFormats.Should().Contain(id).And.NotContain(ChartFormatIds.PhigrosV1);
            vm.SelectedFormat = ChartFormatIds.PhigrosV3;
            vm.IndentedOutput = true;
            vm.SelectedFormat = id;
            vm.IsJsonFormat.Should().BeFalse();
            vm.IndentedOutput.Should().BeFalse();
            vm.ShowConversionOptions.Should().BeFalse();
            vm.CanExport.Should().BeTrue();
            var exports = 0;
            vm.RequestExport += () => exports++;
            vm.OnExportClicked();
            exports.Should().Be(1);
            vm.IsExporting = true;
            vm.CanExport.Should().BeFalse();
            vm.OnExportClicked();
            exports.Should().Be(1);
            vm.IsExporting = false;
        }
        finally
        {
            ChartFormatRegistry.UnregisterFormat(id);
        }

        vm.CanExport.Should().BeFalse();
        vm.RefreshAvailableFormats();
        vm.AvailableFormats.Should().NotContain(id);
        vm.SelectedFormat.Should().NotBe(id);
        vm.CanExport.Should().BeTrue();
    }

    [Theory]
    [InlineData(ChartFormatIds.RePhiEdit)]
    [InlineData(ChartFormatIds.StellateRePhiEditExtended)]
    [InlineData(ChartFormatIds.PhiEdit)]
    [InlineData(ChartFormatIds.PhigrosV3)]
    [InlineData(ChartFormatIds.PhiChain)]
    [InlineData(ChartFormatIds.PhiFans)]
    public void ExportViewModel_PreservesBuiltInOptions(string formatId)
    {
        var vm = new ExportViewModel { SelectedFormat = formatId };
        vm.ShowConversionOptions.Should().BeTrue();
        vm.ShowPeOptions.Should().Be(formatId == ChartFormatIds.PhiEdit);
        vm.ShowPhigrosOptions.Should().Be(formatId == ChartFormatIds.PhigrosV3);
        vm.ShowPhiChainOptions.Should().Be(formatId == ChartFormatIds.PhiChain);
        vm.ShowPhiFansOptions.Should().Be(formatId == ChartFormatIds.PhiFans);
        vm.ShowRePhiEditOptions.Should().Be(
            formatId is ChartFormatIds.RePhiEdit or ChartFormatIds.StellateRePhiEditExtended
        );
        vm.IsJsonFormat.Should().Be(formatId != ChartFormatIds.PhiEdit);
    }

    [Theory]
    [InlineData("RePhiEdit", ChartFormatIds.RePhiEdit)]
    [InlineData("PhiEdit", ChartFormatIds.PhiEdit)]
    [InlineData("0", ChartFormatIds.PhiEdit)]
    [InlineData("1", ChartFormatIds.RePhiEdit)]
    [InlineData("6", ChartFormatIds.StellateRePhiEditExtended)]
    [InlineData("example.format", "example.format")]
    public void Configuration_PreservesExistingTargetKeyAndSupportsExternalIds(string stored, string expected)
    {
        var deserializer = new DeserializerBuilder().WithNamingConvention(CamelCaseNamingConvention.Instance).Build();
        var serializer = new SerializerBuilder().WithNamingConvention(CamelCaseNamingConvention.Instance).Build();
        var config = deserializer.Deserialize<ConvertDefaultsConfig>($"targetType: {stored}");
        config.TargetType.Should().Be(expected);
        var yaml = serializer.Serialize(config);
        yaml.Should().Contain($"targetType: {expected}");
        deserializer.Deserialize<ConvertDefaultsConfig>(yaml).TargetType.Should().Be(expected);
    }
}
