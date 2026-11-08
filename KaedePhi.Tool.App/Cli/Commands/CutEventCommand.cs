using KaedePhi.Tool.App.Cli.Infrastructure;
using KaedePhi.Tool.App.Config;
using KaedePhi.Tool.App.Shared;

namespace KaedePhi.Tool.App.Cli.Commands;

[CliCommand("cut", Aliases = ["cut-event", "cut-all"])]
public static partial class CutEventCommand
{
    private static string Description => CliLocalizationString.cmd_cut_event_desc;

    private static readonly Option<string?> InputOpt = SharedOptions.CreateInputRpeOption();
    private static readonly Option<string?> OutputOpt = SharedOptions.CreateOutputAutoOption();
    private static readonly Option<string?> WorkspaceOpt = SharedOptions.CreateWorkspaceRpeOption();
    private static readonly Option<double> PrecisionOpt = SharedOptions.PrecisionOption;
    private static readonly Option<double> ToleranceOpt = SharedOptions.ToleranceOption;
    private static readonly Option<bool> NoCompressOpt = SharedOptions.NoCompressOption;
    private static readonly Option<bool> DryRunOpt = SharedOptions.DryRunOption;

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

        var c = AppConfigService.Instance.Config.Cut;
        var precision = SharedOptions.GetIfSpecified(result, PrecisionOpt) ?? c.Precision;
        var tolerance = SharedOptions.GetIfSpecified(result, ToleranceOpt) ?? c.Tolerance;
        var disableCompress =
            SharedOptions.GetIfSpecified(result, NoCompressOpt) ?? c.DisableCompress;
        var dryRun = SharedOptions.GetIfSpecified(result, DryRunOpt) ?? c.DryRun;

        return await ChartCommandHelper.RunToolAsync(
            input,
            workspace,
            result.GetValue(OutputOpt),
            dryRun,
            chart =>
                ChartProcessor.CutEvent(
                    chart,
                    precision,
                    tolerance,
                    disableCompress,
                    ConsoleWriter.Sink,
                    ConsoleWriter.CreateProgress(),
                    ct
                ),
            ct
        );
    }
}
