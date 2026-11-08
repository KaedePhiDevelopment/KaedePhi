using KaedePhi.Tool.App.Config;
using KaedePhi.Tool.App.Gui.Models;
using KaedePhi.Tool.App.Gui.Services;
using KaedePhi.Tool.App.Gui.ViewModels;
using KaedePhi.Tool.App.Shared;
using KaedePhi.Tool.Common;
using KaedePhi.Tool.Converter.PhiEdit.Model;
using KaedePhi.Tool.Converter.PhiFans.Model;
using KaedePhi.Tool.Converter.Phigros.v3.Model;

namespace KaedePhi.Tests.App;

[Collection("共享格式注册表")]
public sealed class GuiViewModelTests
{
    [Fact]
    public void ToolViewModel_ConfigDefaultsAppliedOnSelectionSurviveParameterReset()
    {
        var config = new AppConfig();
        config.Unbind.Precision = 8;
        config.Unbind.MergeTolerance = 0.7;
        config.Unbind.ClassicMode = true;
        var vm = new ToolViewModel();
        // 与控制器一致：收到选中工具变化通知后写入配置默认值
        vm.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(ToolViewModel.SelectedTool) && vm.SelectedTool != null)
                vm.ApplyConfigDefaults(config);
        };

        vm.SelectedTool = vm.Tools.Single(tool => tool.ToolId == ToolIds.Unbind);

        vm.Precision.Should().Be(8d);
        vm.MergeTolerance.Should().Be(0.7);
        vm.ClassicMode.Should().BeTrue();
        vm.CanRun.Should().BeTrue();
        vm.ShowMergeTolerance.Should().BeTrue();
    }

    [Fact]
    public void ToolViewModel_DisableCompressRequiresClassicModeOnlyForToolsWithClassicMode()
    {
        var vm = new ToolViewModel();

        vm.SelectedTool = vm.Tools.Single(tool => tool.ToolId == ToolIds.LayerMerge);
        vm.DisableCompressEnabled.Should().BeFalse();
        vm.ClassicMode = true;
        vm.DisableCompressEnabled.Should().BeTrue();
        vm.DisableCompress = true;
        vm.ClassicMode = false;
        vm.DisableCompress.Should().BeFalse();

        vm.SelectedTool = vm.Tools.Single(tool => tool.ToolId == ToolIds.Cut);
        vm.DisableCompressEnabled.Should().BeTrue();
    }

    [Fact]
    public void ExportViewModel_PrepareResetsPageStateAndLoadsDefaults()
    {
        var defaults = new ConvertDefaultsConfig
        {
            StreamOutput = true,
            PeAlphaCutTolerance = 0.5,
            PhiFansDiscontinuityBeatPrecision = 16,
            UnbindPrecision = 32,
        };
        var vm = new ExportViewModel
        {
            StatusText = "旧状态",
            IndentedOutput = true,
            IsExporting = true,
        };

        vm.Prepare(ChartFormatIds.PhiEdit, defaults);

        vm.SourceFormat.Should().Be(ChartFormatIds.PhiEdit);
        vm.SelectedFormat.Should().Be(ChartFormatIds.RePhiEdit);
        vm.StatusText.Should().BeEmpty();
        vm.IndentedOutput.Should().BeFalse();
        vm.IsExporting.Should().BeFalse();
        vm.UseStream.Should().BeTrue();
        vm.Pe.AlphaCutTolerance.Should().Be(0.5);
        vm.PhiFans.DiscontinuityBeatPrecision.Should().Be(16);
        vm.Common.UnbindPrecision.Should().Be(32d);
        vm.RePhiEdit.UnsupportedEasingPrecision.Should().Be(32);
        vm.PhiChain.UnbindPrecision.Should().Be(32d);
    }

    [Fact]
    public void ExportViewModel_ApplyDefaultsKeepsOptionsWithoutConfigurationEntries()
    {
        var vm = new ExportViewModel();
        vm.Common.UnbindCompress = false;
        vm.Common.RemoveTextureLine = true;
        vm.PhiChain.EasingCutPrecision = 8;

        vm.ApplyConversionDefaults(new ConvertDefaultsConfig());

        vm.Common.UnbindCompress.Should().BeFalse();
        vm.Common.RemoveTextureLine.Should().BeTrue();
        vm.PhiChain.EasingCutPrecision.Should().Be(8d);
    }

    [Fact]
    public void ExportOptionsBuilder_MapsPhiEditAndPhigrosOptionsFromGroups()
    {
        var vm = new ExportViewModel();
        vm.Pe.TrailingBeatPadding = 0.5;
        vm.Pe.SpeedCutPrecision = 12;
        vm.Phigros.EasingPrecision = 24;
        vm.Phigros.FilterFakeNotes = true;
        vm.Phigros.NegativeAlphaElevation = true;
        vm.Phigros.NegativeAlphaStep = 2.5;
        vm.Common.UnbindPrecision = 48;
        vm.Common.UnbindCompress = false;
        vm.Common.MultiLayerMergeClassicMode = true;
        vm.Common.RemoveAttachUiLine = true;

        var pe = ExportOptionsBuilder
            .Build(ChartFormatIds.PhiEdit, vm)
            .Should()
            .BeOfType<IrToPhiEditConvertOptions>()
            .Subject;
        pe.TrailingBeatPadding.Should().Be(0.5);
        pe.Speed.CutPrecision.Should().Be(12d);
        pe.FatherLineUnbind.Precision.Should().Be(48d);
        pe.FatherLineUnbind.Compress.Should().BeFalse();
        pe.MultiLayerMerge.ClassicMode.Should().BeTrue();
        pe.LineFilter.RemoveAttachUiLine.Should().BeTrue();

        var phigros = ExportOptionsBuilder
            .Build(ChartFormatIds.PhigrosV3, vm)
            .Should()
            .BeOfType<IrToPhigrosV3ConvertOptions>()
            .Subject;
        phigros.Cutting.EasingPrecision.Should().Be(24d);
        phigros.NoteFilter.FilterFakeNotes.Should().BeTrue();
        phigros.NegativeAlpha.Enabled.Should().BeTrue();
        phigros.NegativeAlpha.ElevationStep.Should().Be(2.5);
        phigros.FatherLineUnbind.Precision.Should().Be(48d);
    }

    [Fact]
    public void ExportOptionsBuilder_PhiFansSharesMergeOptionsAndUnknownFormatHasNoOptions()
    {
        var vm = new ExportViewModel();
        vm.PhiFans.UnsupportedEasingPrecision = 20;
        vm.Common.MultiLayerMergePrecision = 40;

        var phiFans = ExportOptionsBuilder
            .Build(ChartFormatIds.PhiFans, vm)
            .Should()
            .BeOfType<IrToPhiFansConvertOptions>()
            .Subject;
        phiFans.Cutting.UnsupportedEasingPrecision.Should().Be(20);
        phiFans.MultiLayerMerge.Precision.Should().Be(40d);

        ExportOptionsBuilder.Build("tests.unknown-format", vm).Should().BeNull();
    }

    [Fact]
    public void MainViewModel_TracksShellStateAndChartContext()
    {
        var vm = new MainViewModel();
        var changed = new List<string?>();
        vm.PropertyChanged += (_, e) => changed.Add(e.PropertyName);

        vm.IsImportActive.Should().BeTrue();
        vm.CanOpenTools.Should().BeFalse();

        vm.SetShellState(ShellPageKey.Processing, "处理中", "说明", 2, "a.json", "RePhiEdit", true);

        vm.IsToolActive.Should().BeTrue();
        vm.IsImportActive.Should().BeFalse();
        vm.CanOpenTools.Should().BeTrue();
        vm.CanOpenExport.Should().BeTrue();
        vm.CurrentFileName.Should().Be("a.json");
        changed
            .Should()
            .Contain([nameof(MainViewModel.CanOpenTools), nameof(MainViewModel.IsToolActive)]);

        vm.SetShellState(ShellPageKey.ImportOptions, "选项", "说明", 1);
        vm.IsImportActive.Should().BeTrue();
        vm.HasChart.Should().BeTrue();

        vm.ClearChartContext();
        vm.HasChart.Should().BeFalse();
        vm.CurrentFileName.Should().BeEmpty();
        vm.CurrentFormat.Should().BeEmpty();

        vm.ToggleSidebar();
        vm.IsSidebarExpanded.Should().BeFalse();
        vm.SidebarWidth.Should().BeLessThan(100);
    }

    [Fact]
    public void ProcessingViewModel_ResetClearsPreviousRun()
    {
        var vm = new ProcessingViewModel();
        vm.SetStep(0, "运行");
        vm.SetToolProgress(0.5, 0.5, "进行中");
        vm.SetError("失败", "log.txt");

        vm.Reset();

        vm.Progress.Should().Be(0);
        vm.ToolProgressValue.Should().Be(0);
        vm.IsCompleted.Should().BeFalse();
        vm.HasError.Should().BeFalse();
        vm.ErrorMessage.Should().BeEmpty();
        vm.LogFilePath.Should().BeEmpty();
        vm.StatusMessage.Should().BeEmpty();
    }

    [Fact]
    public void ProcessingViewModel_ProgressNeverMovesBackward()
    {
        var vm = new ProcessingViewModel();

        vm.SetToolProgress(0.6, 0.6);
        var forward = vm.Progress;
        vm.SetToolProgress(0.2, 0.2);

        forward.Should().BeGreaterThan(0);
        vm.Progress.Should().Be(forward);
    }

    [Fact]
    public void ChartFiles_ComparesPathsIgnoringCaseAndRelativeSegments()
    {
        var path = Path.Combine(Path.GetTempPath(), "KaedePhi", "chart.json");
        var variant = Path.Combine(Path.GetTempPath(), "KaedePhi", "sub", "..", "CHART.JSON");

        ChartFiles.IsSamePath(path, variant).Should().BeTrue();
        ChartFiles.IsSamePath(path, path + ".bak").Should().BeFalse();
        ChartFiles.IsSamePath(null, path).Should().BeFalse();
        ChartFiles.IsSamePath(path, " ").Should().BeFalse();
    }
}
