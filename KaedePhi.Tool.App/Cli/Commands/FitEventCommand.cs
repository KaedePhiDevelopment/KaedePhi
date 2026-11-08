using KaedePhi.Tool.App.Cli.Infrastructure;
using KaedePhi.Tool.App.Config;
using KaedePhi.Tool.App.Shared;

namespace KaedePhi.Tool.App.Cli.Commands;

[CliCommand("fit", Aliases = ["fit-event"])]
public static partial class FitEventCommand
{
    private static string Description => CliLocalizationString.fit_command_desc;

    public static readonly Option<string?> InputOpt = SharedOptions.CreateInputRpeOption();
    public static readonly Option<string?> OutputOpt = SharedOptions.CreateOutputAutoOption();
    public static readonly Option<string?> WorkspaceOpt = SharedOptions.CreateWorkspaceRpeOption();
    public static readonly Option<double> ToleranceOpt = SharedOptions.ToleranceOption;
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

        var c = AppConfigService.Instance.Config.Fit;
        var tolerance = SharedOptions.GetIfSpecified(result, ToleranceOpt) ?? c.Tolerance;
        var dryRun = SharedOptions.GetIfSpecified(result, DryRunOpt) ?? c.DryRun;

        return await ChartCommandHelper.RunToolAsync(
            input,
            workspace,
            result.GetValue(OutputOpt),
            dryRun,
            chart =>
                ChartProcessor.FitEvent(
                    chart,
                    tolerance,
                    ConsoleWriter.Sink,
                    ConsoleWriter.CreateProgress(),
                    ct
                ),
            ct
        );
    }
}
