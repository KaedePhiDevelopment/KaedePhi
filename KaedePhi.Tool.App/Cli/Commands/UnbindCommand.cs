using KaedePhi.Tool.App.Cli.Infrastructure;
using KaedePhi.Tool.App.Config;
using KaedePhi.Tool.App.Shared;

namespace KaedePhi.Tool.App.Cli.Commands;

[CliCommand("unbind-father", Aliases = ["unbind"])]
public static partial class UnbindFatherCommand
{
    private static string Description => CliLocalizationString.cmd_unbind_father_desc;

    public static readonly Option<string?> InputOpt = SharedOptions.CreateInputRpeOption();
    public static readonly Option<string?> OutputOpt = SharedOptions.CreateOutputAutoOption();
    public static readonly Option<string?> WorkspaceOpt = SharedOptions.CreateWorkspaceRpeOption();
    public static readonly Option<double> PrecisionOpt = SharedOptions.PrecisionOption;
    public static readonly Option<double> ToleranceOpt = SharedOptions.ToleranceOption;

    public static readonly Option<double> MergeToleranceOpt = new("--merge-tolerance")
    {
        Description = CliLocalizationString.cli_opt_unbind_merge_tolerance_desc,
        Arity = ArgumentArity.ExactlyOne,
    };

    public static readonly Option<bool> ClassicOpt = SharedOptions.ClassicOption;
    public static readonly Option<bool> NoCompressOpt = SharedOptions.NoCompressOption;
    public static readonly Option<bool> DryRunOpt = SharedOptions.DryRunOption;

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

        var c = AppConfigService.Instance.Config.Unbind;
        var precision = SharedOptions.GetIfSpecified(result, PrecisionOpt) ?? c.Precision;
        var tolerance = SharedOptions.GetIfSpecified(result, ToleranceOpt) ?? c.Tolerance;
        var mergeTolerance =
            SharedOptions.GetIfSpecified(result, MergeToleranceOpt) ?? c.MergeTolerance;
        var classic = SharedOptions.GetIfSpecified(result, ClassicOpt) ?? c.ClassicMode;
        var disableCompress =
            SharedOptions.GetIfSpecified(result, NoCompressOpt) ?? c.DisableCompress;
        var dryRun = SharedOptions.GetIfSpecified(result, DryRunOpt) ?? c.DryRun;

        if (disableCompress && !classic)
        {
            ConsoleWriter.Error(CliLocalizationString.err_classic_disablsed);
            return 1;
        }

        return await ChartCommandHelper.RunToolAsync(
            input,
            workspace,
            result.GetValue(OutputOpt),
            dryRun,
            chart =>
                ChartProcessor.UnbindFather(
                    chart,
                    precision,
                    tolerance,
                    classic,
                    disableCompress: disableCompress,
                    mergeTolerance: mergeTolerance,
                    log: ConsoleWriter.Sink,
                    progress: ConsoleWriter.CreateProgress(),
                    ct: ct
                ),
            ct
        );
    }
}
