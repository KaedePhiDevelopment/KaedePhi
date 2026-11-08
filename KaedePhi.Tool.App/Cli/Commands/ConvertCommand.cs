using System.CommandLine.Completions;
using KaedePhi.Tool.App.Cli.Infrastructure;
using KaedePhi.Tool.App.Config;
using KaedePhi.Tool.Converter;
using static KaedePhi.Tool.App.Cli.Infrastructure.SharedOptions;

namespace KaedePhi.Tool.App.Cli.Commands;

[CliCommand("convert")]
public static partial class ConvertCommand
{
    private static string Description => CliLocalizationString.convert_command_desc;

    static ConvertCommand()
    {
        TargetTypeOpt.CompletionSources.Add(_ =>
            ChartFormatRegistry.ExportableFormats.Select(format => new CompletionItem(
                format.FormatId
            ))
        );
    }

    #region 共享选项

    private static readonly Option<string?> InputOpt = CreateInputRpeOption();
    private static readonly Option<string?> OutputOpt = CreateOutputAutoOption();
    private static readonly Option<string?> WorkspaceOpt = CreateWorkspaceRpeOption();
    private static readonly Option<double> PrecisionOpt = PrecisionOption;
    private static readonly Option<double> ToleranceOpt = ToleranceOption;
    private static readonly Option<bool> ClassicOpt = ClassicOption;
    private static readonly Option<bool> DryRunOpt = DryRunOption;
    private static readonly Option<bool> StreamOpt = StreamOutputOption;
    private static readonly Option<bool> FormatOpt = FormatOutputOption;

    #endregion

    #region 目标格式选项

    private static readonly Option<string?> TargetTypeOpt = new("--target")
    {
        Description = CliLocalizationString.convert_command_opt_target,
        Arity = ArgumentArity.ZeroOrOne,
    };

    #endregion

    #region PhiEdit 选项

    private static readonly Option<double> PeTrailingPaddingOpt = CreateValueOption<double>(
        "--pe-trailing-padding",
        CliLocalizationString.convert_opt_pe_trailing_padding
    );

    private static readonly Option<double> PeEasingPrecisionOpt = CreateValueOption<double>(
        "--pe-easing-precision",
        CliLocalizationString.convert_opt_pe_easing_precision
    );

    private static readonly Option<double> PeXyPrecisionOpt = CreateValueOption<double>(
        "--pe-xy-precision",
        CliLocalizationString.convert_opt_pe_xy_precision
    );

    private static readonly Option<double> PeAlphaPrecisionOpt = CreateValueOption<double>(
        "--pe-alpha-precision",
        CliLocalizationString.convert_opt_pe_alpha_precision
    );

    private static readonly Option<double> PeAlphaToleranceOpt = CreateValueOption<double>(
        "--pe-alpha-tolerance",
        CliLocalizationString.convert_opt_pe_alpha_tolerance
    );

    private static readonly Option<double> PeSpeedPrecisionOpt = CreateValueOption<double>(
        "--pe-speed-precision",
        CliLocalizationString.convert_opt_pe_speed_precision
    );

    #endregion

    #region PhigrosV3 选项

    private static readonly Option<double> PhigrosEasingPrecisionOpt = CreateValueOption<double>(
        "--phigros-easing-precision",
        CliLocalizationString.convert_opt_phigros_easing_precision
    );

    private static readonly Option<double> PhigrosXyPrecisionOpt = CreateValueOption<double>(
        "--phigros-xy-precision",
        CliLocalizationString.convert_opt_phigros_xy_precision
    );

    private static readonly Option<double> PhigrosAlphaPrecisionOpt = CreateValueOption<double>(
        "--phigros-alpha-precision",
        CliLocalizationString.convert_opt_phigros_alpha_precision
    );

    private static readonly Option<double> PhigrosSpeedPrecisionOpt = CreateValueOption<double>(
        "--phigros-speed-precision",
        CliLocalizationString.convert_opt_phigros_speed_precision
    );

    #endregion

    #region PhiFans 选项

    private static readonly Option<int> PhiFansEasingPrecisionOpt = CreateValueOption<int>(
        "--phifans-easing-precision",
        CliLocalizationString.convert_opt_phifans_easing_precision
    );

    private static readonly Option<int> PhiFansDiscontinuityPrecisionOpt = CreateValueOption<int>(
        "--phifans-discontinuity-precision",
        CliLocalizationString.convert_opt_phifans_discontinuity_precision
    );

    #endregion

    #region 解绑与合并选项

    private static readonly Option<double> UnbindPrecisionOpt = CreateValueOption<double>(
        "--unbind-precision",
        CliLocalizationString.convert_opt_unbind_precision
    );

    private static readonly Option<double> UnbindToleranceOpt = CreateValueOption<double>(
        "--unbind-tolerance",
        CliLocalizationString.convert_opt_unbind_tolerance
    );

    private static readonly Option<double> UnbindMergeToleranceOpt = CreateValueOption<double>(
        "--unbind-merge-tolerance",
        CliLocalizationString.convert_opt_unbind_merge_tolerance
    );

    private static readonly Option<bool> UnbindClassicOpt = CreateFlagOption(
        "--unbind-classic",
        CliLocalizationString.convert_opt_unbind_classic
    );

    private static readonly Option<double> MergePrecisionOpt = CreateValueOption<double>(
        "--merge-precision",
        CliLocalizationString.convert_opt_merge_precision
    );

    private static readonly Option<double> MergeToleranceOpt = CreateValueOption<double>(
        "--merge-tolerance",
        CliLocalizationString.convert_opt_merge_tolerance
    );

    private static readonly Option<bool> MergeClassicOpt = CreateFlagOption(
        "--merge-classic",
        CliLocalizationString.convert_opt_merge_classic
    );

    private static readonly Option<bool> NoUnbindCompressOpt = CreateFlagOption(
        "--no-unbind-compress",
        CliLocalizationString.convert_opt_no_unbind_compress
    );

    private static readonly Option<bool> NoMergeCompressOpt = CreateFlagOption(
        "--no-merge-compress",
        CliLocalizationString.convert_opt_no_merge_compress
    );

    #endregion

    #region 判定线过滤选项

    private static readonly Option<bool> RemoveAttachUiOpt = CreateFlagOption(
        "--remove-attach-ui",
        CliLocalizationString.convert_opt_remove_attach_ui
    );

    private static readonly Option<bool> RemoveTextureOpt = CreateFlagOption(
        "--remove-texture",
        CliLocalizationString.convert_opt_remove_texture
    );

    #endregion

    #region PhigrosV3 音符与透明度选项

    private static readonly Option<bool> FilterFakeNotesOpt = CreateFlagOption(
        "--filter-fake-notes",
        CliLocalizationString.convert_opt_filter_fake_notes
    );

    private static readonly Option<bool> NegativeAlphaElevationOpt = CreateFlagOption(
        "--negative-alpha-elevation",
        CliLocalizationString.convert_opt_negative_alpha_elevation
    );

    private static readonly Option<double> NegativeAlphaStepOpt = CreateValueOption<double>(
        "--negative-alpha-step",
        CliLocalizationString.convert_opt_negative_alpha_step
    );

    #endregion

    [CliHandler]
    private static async Task<int> HandleAsync(ParseResult result, CancellationToken ct)
    {
        if (
            !ChartCommandHelper.TryGetInput(
                result,
                InputOpt,
                WorkspaceOpt,
                out var input,
                out var workspace
            )
        )
            return 1;

        var c = AppConfigService.Instance.Config.Convert;
        var target = ChartFormatRegistry.Get(result.GetValue(TargetTypeOpt) ?? c.TargetType);
        if (!target.CanExport)
        {
            ConsoleWriter.Error(CliLocalizationString.err_unimplemented);
            return 1;
        }

        var service = new ChartService();
        var chart = await service.LoadKpcAsync(input, workspace, ct);
        if (chart == null)
        {
            ConsoleWriter.Error(CliLocalizationString.err_unimplemented);
            return 1;
        }

        var output = service.ResolveOutputPath(
            input,
            result.GetValue(OutputOpt),
            workspace,
            "." + target.FileExtension
        );
        var savedPath = await ChartService.SaveAsAsync(
            chart,
            output,
            target.FormatId,
            new SaveAsOptions
            {
                Stream = result.ValueOr(StreamOpt, c.StreamOutput),
                Format = result.ValueOr(FormatOpt, c.FormatOutput),
                DryRun = result.ValueOr(DryRunOpt, c.DryRun),
                PhiEditOptions = BuildPhiEditOptions(result, c),
                PhigrosOptions = BuildPhigrosOptions(result, c),
                PhiFansOptions = BuildPhiFansOptions(result, c),
            },
            ct
        );

        if (savedPath is null)
        {
            ConsoleWriter.Error(CliLocalizationString.err_unimplemented);
            return 2;
        }

        ConsoleWriter.Info(string.Format(CliLocalizationString.msg_written, savedPath));
        return 0;
    }
}
