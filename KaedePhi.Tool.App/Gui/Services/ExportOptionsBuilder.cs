using KaedePhi.Tool.App.Gui.ViewModels;
using KaedePhi.Tool.Common;
using KaedePhi.Tool.Converter.PhiChain.Model;
using KaedePhi.Tool.Converter.PhiEdit.Model;
using KaedePhi.Tool.Converter.PhiFans.Model;
using KaedePhi.Tool.Converter.Phigros.v3.Model;
using KaedePhi.Tool.Converter.RePhiEdit.Model;

namespace KaedePhi.Tool.App.Gui.Services;

// 将导出页的选项状态映射为各目标格式的转换选项
internal static class ExportOptionsBuilder
{
    // 没有可配置选项的格式返回 null，由格式自身使用默认选项
    public static object? Build(string targetFormat, ExportViewModel vm) =>
        targetFormat switch
        {
            ChartFormatIds.PhiEdit => BuildPhiEdit(vm),
            ChartFormatIds.PhigrosV3 => BuildPhigrosV3(vm),
            ChartFormatIds.PhiChain => BuildPhiChain(vm),
            ChartFormatIds.RePhiEdit or ChartFormatIds.StellateRePhiEditExtended => BuildRePhiEdit(
                vm
            ),
            ChartFormatIds.PhiFans => BuildPhiFans(vm),
            _ => null,
        };

    private static IrToPhiEditConvertOptions BuildPhiEdit(ExportViewModel vm)
    {
        var pe = vm.Pe;
        var common = vm.Common;
        return new IrToPhiEditConvertOptions
        {
            TrailingBeatPadding = pe.TrailingBeatPadding,
            Cutting = new IrToPhiEditConvertOptions.CuttingOptions
            {
                UnsupportedEasingPrecision = pe.UnsupportedEasingPrecision,
                MisalignedXyEventPrecision = pe.MisalignedXyEventPrecision,
            },
            Alpha = new IrToPhiEditConvertOptions.AlphaOptions
            {
                CutPrecision = pe.AlphaCutPrecision,
                CutTolerance = pe.AlphaCutTolerance,
            },
            Speed = new IrToPhiEditConvertOptions.SpeedOptions
            {
                CutPrecision = pe.SpeedCutPrecision,
            },
            FatherLineUnbind = new IrToPhiEditConvertOptions.FatherLineUnbindOptions
            {
                Precision = common.UnbindPrecision,
                Tolerance = common.UnbindTolerance,
                MergeTolerance = common.UnbindMergeTolerance,
                ClassicMode = common.UnbindClassicMode,
                Compress = common.UnbindCompress,
            },
            MultiLayerMerge = new IrToPhiEditConvertOptions.MultiLayerMergeOptions
            {
                Precision = common.MultiLayerMergePrecision,
                Tolerance = common.MultiLayerMergeTolerance,
                ClassicMode = common.MultiLayerMergeClassicMode,
                Compress = common.MultiLayerMergeCompress,
            },
            LineFilter = new IrToPhiEditConvertOptions.LineFilterOptions
            {
                RemoveAttachUiLine = common.RemoveAttachUiLine,
                RemoveTextureLine = common.RemoveTextureLine,
            },
        };
    }

    private static IrToPhigrosV3ConvertOptions BuildPhigrosV3(ExportViewModel vm)
    {
        var phigros = vm.Phigros;
        var common = vm.Common;
        return new IrToPhigrosV3ConvertOptions
        {
            Cutting = new IrToPhigrosV3ConvertOptions.CuttingOptions
            {
                EasingPrecision = phigros.EasingPrecision,
                MisalignedXyEventPrecision = phigros.MisalignedXyEventPrecision,
            },
            Alpha = new IrToPhigrosV3ConvertOptions.AlphaOptions
            {
                CutPrecision = phigros.AlphaCutPrecision,
            },
            Speed = new IrToPhigrosV3ConvertOptions.SpeedOptions
            {
                CutPrecision = phigros.SpeedCutPrecision,
            },
            FatherLineUnbind = new IrToPhigrosV3ConvertOptions.FatherLineUnbindOptions
            {
                Precision = common.UnbindPrecision,
                Tolerance = common.UnbindTolerance,
                MergeTolerance = common.UnbindMergeTolerance,
                ClassicMode = common.UnbindClassicMode,
                Compress = common.UnbindCompress,
            },
            MultiLayerMerge = new IrToPhigrosV3ConvertOptions.MultiLayerMergeOptions
            {
                Precision = common.MultiLayerMergePrecision,
                Tolerance = common.MultiLayerMergeTolerance,
                ClassicMode = common.MultiLayerMergeClassicMode,
                Compress = common.MultiLayerMergeCompress,
            },
            LineFilter = new IrToPhigrosV3ConvertOptions.LineFilterOptions
            {
                RemoveAttachUiLine = common.RemoveAttachUiLine,
                RemoveTextureLine = common.RemoveTextureLine,
            },
            NoteFilter = new IrToPhigrosV3ConvertOptions.NoteFilterOptions
            {
                FilterFakeNotes = phigros.FilterFakeNotes,
            },
            NegativeAlpha = new IrToPhigrosV3ConvertOptions.NegativeAlphaOptions
            {
                Enabled = phigros.NegativeAlphaElevation,
                ElevationStep = phigros.NegativeAlphaStep,
            },
        };
    }

    private static IrToPhiChainConvertOptions BuildPhiChain(ExportViewModel vm)
    {
        var phiChain = vm.PhiChain;
        return new IrToPhiChainConvertOptions
        {
            UnbindNonRotatingChildren = phiChain.UnbindNonRotatingChildren,
            UnbindPrecision = phiChain.UnbindPrecision,
            UnbindTolerance = phiChain.UnbindTolerance,
            UnbindMergeTolerance = phiChain.UnbindMergeTolerance,
            UnbindClassicMode = phiChain.UnbindClassicMode,
            MultiLayerMergePrecision = phiChain.MultiLayerMergePrecision,
            MultiLayerMergeTolerance = phiChain.MultiLayerMergeTolerance,
            MultiLayerMergeClassicMode = phiChain.MultiLayerMergeClassicMode,
            EasingCutPrecision = phiChain.EasingCutPrecision,
        };
    }

    private static ConvertOption BuildRePhiEdit(ExportViewModel vm) =>
        new()
        {
            Cutting = new ConvertOption.CuttingOptions
            {
                UnsupportedEasingPrecision = vm.RePhiEdit.UnsupportedEasingPrecision,
            },
        };

    // PhiFans 的多层级合并复用通用选项分组
    private static IrToPhiFansConvertOptions BuildPhiFans(ExportViewModel vm)
    {
        var phiFans = vm.PhiFans;
        var common = vm.Common;
        return new IrToPhiFansConvertOptions
        {
            Cutting = new IrToPhiFansConvertOptions.CuttingOptions
            {
                UnsupportedEasingPrecision = phiFans.UnsupportedEasingPrecision,
            },
            DiscontinuityBeatPrecision = phiFans.DiscontinuityBeatPrecision,
            MultiLayerMerge = new IrToPhiFansConvertOptions.MultiLayerMergeOptions
            {
                Precision = common.MultiLayerMergePrecision,
                Tolerance = common.MultiLayerMergeTolerance,
                ClassicMode = common.MultiLayerMergeClassicMode,
                Compress = common.MultiLayerMergeCompress,
            },
        };
    }
}
