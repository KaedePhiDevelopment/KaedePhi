using KaedePhi.Tool.App.Config;
using KaedePhi.Tool.Converter.PhiEdit.Model;
using KaedePhi.Tool.Converter.PhiFans.Model;
using KaedePhi.Tool.Converter.Phigros.v3.Model;
using static KaedePhi.Tool.App.Cli.Infrastructure.SharedOptions;

namespace KaedePhi.Tool.App.Cli.Commands;

// 命令行参数与配置默认值合并为各目标格式的转换选项
public static partial class ConvertCommand
{
    private static IrToPhiEditConvertOptions BuildPhiEditOptions(
        ParseResult result,
        ConvertDefaultsConfig c
    )
    {
        var unbind = ResolveUnbind(result, c);
        var merge = ResolveMerge(result, c);
        return new IrToPhiEditConvertOptions
        {
            TrailingBeatPadding = result.ValueOr(PeTrailingPaddingOpt, c.PeTrailingBeatPadding),
            Cutting = new IrToPhiEditConvertOptions.CuttingOptions
            {
                UnsupportedEasingPrecision = result.ValueOr(
                    PeEasingPrecisionOpt,
                    c.PeUnsupportedEasingPrecision
                ),
                MisalignedXyEventPrecision = result.ValueOr(
                    PeXyPrecisionOpt,
                    c.PeMisalignedXyEventPrecision
                ),
            },
            Alpha = new IrToPhiEditConvertOptions.AlphaOptions
            {
                CutPrecision = result.ValueOr(PeAlphaPrecisionOpt, c.PeAlphaCutPrecision),
                CutCompress = c.PeAlphaCutCompress,
                CutTolerance = result.ValueOr(PeAlphaToleranceOpt, c.PeAlphaCutTolerance),
            },
            Speed = new IrToPhiEditConvertOptions.SpeedOptions
            {
                CutPrecision = result.ValueOr(PeSpeedPrecisionOpt, c.PeSpeedCutPrecision),
            },
            FatherLineUnbind = new IrToPhiEditConvertOptions.FatherLineUnbindOptions
            {
                Precision = unbind.Precision,
                Tolerance = unbind.Tolerance,
                MergeTolerance = unbind.MergeTolerance,
                ClassicMode = unbind.ClassicMode,
                Compress = unbind.Compress,
            },
            MultiLayerMerge = new IrToPhiEditConvertOptions.MultiLayerMergeOptions
            {
                Precision = merge.Precision,
                Tolerance = merge.Tolerance,
                ClassicMode = merge.ClassicMode,
                Compress = merge.Compress,
            },
            LineFilter = new IrToPhiEditConvertOptions.LineFilterOptions
            {
                RemoveAttachUiLine = result.GetValue(RemoveAttachUiOpt),
                RemoveTextureLine = result.GetValue(RemoveTextureOpt),
            },
        };
    }

    private static IrToPhigrosV3ConvertOptions BuildPhigrosOptions(
        ParseResult result,
        ConvertDefaultsConfig c
    )
    {
        var unbind = ResolveUnbind(result, c);
        var merge = ResolveMerge(result, c);
        return new IrToPhigrosV3ConvertOptions
        {
            Cutting = new IrToPhigrosV3ConvertOptions.CuttingOptions
            {
                EasingPrecision = result.ValueOr(
                    PhigrosEasingPrecisionOpt,
                    c.PhigrosEasingPrecision
                ),
                MisalignedXyEventPrecision = result.ValueOr(
                    PhigrosXyPrecisionOpt,
                    c.PhigrosMisalignedXyEventPrecision
                ),
            },
            Alpha = new IrToPhigrosV3ConvertOptions.AlphaOptions
            {
                CutPrecision = result.ValueOr(PhigrosAlphaPrecisionOpt, c.PhigrosAlphaCutPrecision),
            },
            Speed = new IrToPhigrosV3ConvertOptions.SpeedOptions
            {
                CutPrecision = result.ValueOr(PhigrosSpeedPrecisionOpt, c.PhigrosSpeedCutPrecision),
            },
            FatherLineUnbind = new IrToPhigrosV3ConvertOptions.FatherLineUnbindOptions
            {
                Precision = unbind.Precision,
                Tolerance = unbind.Tolerance,
                MergeTolerance = unbind.MergeTolerance,
                ClassicMode = unbind.ClassicMode,
                Compress = unbind.Compress,
            },
            MultiLayerMerge = new IrToPhigrosV3ConvertOptions.MultiLayerMergeOptions
            {
                Precision = merge.Precision,
                Tolerance = merge.Tolerance,
                ClassicMode = merge.ClassicMode,
                Compress = merge.Compress,
            },
            LineFilter = new IrToPhigrosV3ConvertOptions.LineFilterOptions
            {
                RemoveAttachUiLine = result.GetValue(RemoveAttachUiOpt),
                RemoveTextureLine = result.GetValue(RemoveTextureOpt),
            },
            NoteFilter = new IrToPhigrosV3ConvertOptions.NoteFilterOptions
            {
                FilterFakeNotes = result.ValueOr(FilterFakeNotesOpt, c.PhigrosFilterFakeNotes),
            },
            NegativeAlpha = new IrToPhigrosV3ConvertOptions.NegativeAlphaOptions
            {
                Enabled = result.ValueOr(
                    NegativeAlphaElevationOpt,
                    c.PhigrosNegativeAlphaElevation
                ),
                ElevationStep = result.ValueOr(NegativeAlphaStepOpt, c.PhigrosNegativeAlphaStep),
            },
        };
    }

    private static IrToPhiFansConvertOptions BuildPhiFansOptions(
        ParseResult result,
        ConvertDefaultsConfig c
    )
    {
        var merge = ResolveMerge(result, c);
        return new IrToPhiFansConvertOptions
        {
            Cutting = new IrToPhiFansConvertOptions.CuttingOptions
            {
                UnsupportedEasingPrecision = result.ValueOr(
                    PhiFansEasingPrecisionOpt,
                    c.PhiFansUnsupportedEasingPrecision
                ),
            },
            DiscontinuityBeatPrecision = result.ValueOr(
                PhiFansDiscontinuityPrecisionOpt,
                c.PhiFansDiscontinuityBeatPrecision
            ),
            MultiLayerMerge = new IrToPhiFansConvertOptions.MultiLayerMergeOptions
            {
                Precision = merge.Precision,
                Tolerance = merge.Tolerance,
                ClassicMode = merge.ClassicMode,
                Compress = merge.Compress,
            },
        };
    }

    // 专属选项优先，其次是通用的 --precision/--tolerance/--classic，最后才是配置默认值
    private static (
        double Precision,
        double Tolerance,
        double MergeTolerance,
        bool ClassicMode,
        bool Compress
    ) ResolveUnbind(ParseResult result, ConvertDefaultsConfig c) =>
        (
            result.ValueOr(UnbindPrecisionOpt, result.ValueOr(PrecisionOpt, c.UnbindPrecision)),
            result.ValueOr(UnbindToleranceOpt, result.ValueOr(ToleranceOpt, c.UnbindTolerance)),
            result.ValueOr(UnbindMergeToleranceOpt, c.UnbindMergeTolerance),
            result.ValueOr(UnbindClassicOpt, result.ValueOr(ClassicOpt, c.UnbindClassicMode)),
            !result.GetValue(NoUnbindCompressOpt)
        );

    private static (
        double Precision,
        double Tolerance,
        bool ClassicMode,
        bool Compress
    ) ResolveMerge(ParseResult result, ConvertDefaultsConfig c) =>
        (
            result.ValueOr(MergePrecisionOpt, c.MultiLayerMergePrecision),
            result.ValueOr(MergeToleranceOpt, c.MultiLayerMergeTolerance),
            result.ValueOr(MergeClassicOpt, c.MultiLayerMergeClassicMode),
            !result.GetValue(NoMergeCompressOpt)
        );
}
