using KaedePhi.Tool.App.Cli.Infrastructure;
using KaedePhi.Tool.App.Config;
using KaedePhi.Tool.App.Shared;

namespace KaedePhi.Tool.App.Cli.Commands;

[CliCommand("layer-merge")]
public static partial class LayerMergeCommand
{
    private static string Description => CliLocalizationString.cmd_layer_merge_desc;

    private static readonly Option<string?> InputOpt = SharedOptions.CreateInputRpeOption();
    private static readonly Option<string?> OutputOpt = SharedOptions.CreateOutputAutoOption();
    private static readonly Option<string?> WorkspaceOpt = SharedOptions.CreateWorkspaceRpeOption();
    private static readonly Option<double> PrecisionOpt = SharedOptions.PrecisionOption;
    private static readonly Option<double> ToleranceOpt = SharedOptions.ToleranceOption;
    private static readonly Option<bool> ClassicOpt = SharedOptions.ClassicOption;
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

        var c = AppConfigService.Instance.Config.LayerMerge;
        var precision = SharedOptions.GetIfSpecified(result, PrecisionOpt) ?? c.Precision;
        var tolerance = SharedOptions.GetIfSpecified(result, ToleranceOpt) ?? c.Tolerance;
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
                ChartProcessor.LayerMerge(
                    chart,
                    precision,
                    tolerance,
                    classic,
                    disableCompress,
                    ConsoleWriter.Sink,
                    ConsoleWriter.CreateProgress(),
                    ct
                ),
            ct
        );
    }
}
