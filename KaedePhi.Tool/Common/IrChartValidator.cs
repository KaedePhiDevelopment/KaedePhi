using KaedePhi.Core.Intermediate.Model;

namespace KaedePhi.Tool.Common;

/// <summary>
/// 校验 IR 谱面的 BPM 数据与父子线索引。
/// </summary>
public static class IrChartValidator
{
    private const int RootFatherIndex = -1;
    private const byte Unvisited = 0;
    private const byte Visiting = 1;
    private const byte Visited = 2;

    /// <summary>
    /// 校验 IR 谱面的 BPM 节点和判定线 BPM 因子。
    /// </summary>
    /// <param name="chart">待校验的 IR 谱面。</param>
    /// <returns>无返回值。</returns>
    public static void ValidateBpmAndBpmFactors(Chart chart)
    {
        ArgumentNullException.ThrowIfNull(chart);
        ValidateChartCollections(chart);
        ValidateBpmItems(chart.BpmList);
        ValidateBpmFactors(chart.JudgeLineList);
    }

    /// <summary>
    /// 校验父子线索引与父线环路。
    /// </summary>
    /// <param name="judgeLines">待检查的判定线列表。</param>
    /// <returns>无返回值。</returns>
    public static void ValidateJudgeLineHierarchy(IReadOnlyList<Ir.JudgeLine> judgeLines)
    {
        ArgumentNullException.ThrowIfNull(judgeLines);
        ValidateFatherIndexes(judgeLines);
        ValidateAcyclicHierarchy(judgeLines);
    }

    private static void ValidateChartCollections(Chart chart)
    {
        if (chart.BpmList is null)
            throw new FormatException(ToolResources.ir_chart_bpm_list_null);
        if (chart.JudgeLineList is null)
            throw new FormatException(ToolResources.ir_chart_judge_line_list_null);
        if (chart.BlockAreaList is null)
            throw new FormatException(ToolResources.ir_chart_block_area_list_null);
    }

    private static void ValidateBpmItems(IReadOnlyList<BpmItem> bpmItems)
    {
        for (var index = 0; index < bpmItems.Count; index++)
        {
            var bpm = bpmItems[index];
            if (bpm is null || !float.IsFinite(bpm.Bpm) || bpm.Bpm <= 0)
                throw new FormatException(
                    string.Format(ToolResources.ir_chart_bpm_item_invalid, index)
                );
        }
    }

    private static void ValidateBpmFactors(IReadOnlyList<Ir.JudgeLine> judgeLines)
    {
        for (var index = 0; index < judgeLines.Count; index++)
        {
            var line = judgeLines[index];
            if (line is null || !float.IsFinite(line.BpmFactor) || line.BpmFactor <= 0)
                throw new FormatException(
                    string.Format(ToolResources.ir_chart_bpm_factor_invalid, index)
                );
        }
    }

    private static void ValidateFatherIndexes(IReadOnlyList<Ir.JudgeLine> judgeLines)
    {
        for (var index = 0; index < judgeLines.Count; index++)
        {
            var father = judgeLines[index].Father;
            if (father < RootFatherIndex || father >= judgeLines.Count)
                throw new FormatException(
                    string.Format(ToolResources.ir_chart_father_index_invalid, index, father)
                );
        }
    }

    private static void ValidateAcyclicHierarchy(IReadOnlyList<Ir.JudgeLine> judgeLines)
    {
        var states = new byte[judgeLines.Count];
        for (var start = 0; start < judgeLines.Count; start++)
            VisitFatherPath(judgeLines, states, start);
    }

    private static void VisitFatherPath(
        IReadOnlyList<Ir.JudgeLine> judgeLines,
        byte[] states,
        int start
    )
    {
        var current = start;
        while (current >= 0 && states[current] == Unvisited)
        {
            states[current] = Visiting;
            current = judgeLines[current].Father;
        }

        if (current >= 0 && states[current] == Visiting)
            throw new FormatException(
                string.Format(ToolResources.ir_chart_hierarchy_cycle, current)
            );

        current = start;
        while (current >= 0 && states[current] == Visiting)
        {
            states[current] = Visited;
            current = judgeLines[current].Father;
        }
    }
}
