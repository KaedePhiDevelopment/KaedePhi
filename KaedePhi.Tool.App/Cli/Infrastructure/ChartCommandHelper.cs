using Chart = KaedePhi.Core.Intermediate.Model.Chart;

namespace KaedePhi.Tool.App.Cli.Infrastructure;

// 各谱面命令共用的输入校验与“加载 - 处理 - 保存”流程
internal static class ChartCommandHelper
{
    // 输入文件与工作区至少需要指定一个，否则输出错误并返回 false
    public static bool TryGetInput(
        ParseResult result,
        Option<string?> inputOption,
        Option<string?> workspaceOption,
        out string? input,
        out string? workspace
    )
    {
        input = result.GetValue(inputOption);
        workspace = result.GetValue(workspaceOption);
        if (!string.IsNullOrWhiteSpace(input) || !string.IsNullOrWhiteSpace(workspace))
            return true;

        ConsoleWriter.Error(CliLocalizationString.err_input_required);
        return false;
    }

    // 在谱面副本上执行处理并以 RePhiEdit 格式保存，返回进程退出码
    public static async Task<int> RunToolAsync(
        string? input,
        string? workspace,
        string? output,
        bool dryRun,
        Action<Chart> process,
        CancellationToken ct
    )
    {
        var service = new ChartService();
        var chart = await service.LoadKpcAsync(input, workspace, ct);
        if (chart == null)
        {
            ConsoleWriter.Error(CliLocalizationString.err_unimplemented);
            return 1;
        }

        var workingChart = chart.Clone();
        process(workingChart);

        var savedPath = await ChartService.SaveAsRpeAsync(
            workingChart,
            service.ResolveOutputPath(input, output, workspace),
            dryRun,
            ct
        );
        ConsoleWriter.Info(string.Format(CliLocalizationString.msg_written, savedPath));
        return 0;
    }
}
