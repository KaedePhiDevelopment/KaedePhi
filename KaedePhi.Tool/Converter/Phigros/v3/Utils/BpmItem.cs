using KaedePhi.Core.Primitives;
using PhigrosJudgeLine = KaedePhi.Core.Formats.Phigros.v3.Model.JudgeLine;

namespace KaedePhi.Tool.Converter.Phigros.v3.Utils;

/// <summary>
/// PhigrosV3 BPM 项到 IR BPM 项的转换工具。
/// </summary>
public static class BpmItemBuilder
{
    /// <summary>
    /// 将源数据转换为目标格式的数据。
    /// </summary>
    /// <param name="judgeLines">待转换的判定线列表。</param>
    /// <returns>转换或构建得到的结果。</returns>
    public static List<Ir.BpmItem> ConvertBpmList(List<PhigrosJudgeLine> judgeLines)
    {
        if (judgeLines is not { Count: > 0 })
            return [new Ir.BpmItem { Bpm = 120f, StartBeat = new Beat(0) }];

        return [new Ir.BpmItem { Bpm = judgeLines[0].Bpm, StartBeat = new Beat(0) }];
    }
}
