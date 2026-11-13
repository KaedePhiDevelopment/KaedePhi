using PhigrosJudgeLine = KaedePhi.Core.Formats.Phigros.v3.Model.JudgeLine;

namespace KaedePhi.Tool.Converter.Phigros.v3.Utils;

/// <summary>
/// PhigrosV3 判定线到 IR 判定线的构建器。
/// </summary>
public static class IrJudgeLineBuilder
{
    /// <summary>
    /// 将源数据转换为目标格式的数据。
    /// </summary>
    /// <param name="src">源对象</param>
    /// <param name="index">分量索引</param>
    /// <param name="defaultBpm">谱面默认节拍速度。</param>
    /// <returns>转换或构建得到的结果。</returns>
    public static Ir.JudgeLine ConvertJudgeLine(PhigrosJudgeLine src, int index, float defaultBpm)
    {
        var horizonBeat = EventBuilder.GetJudgeLineHorizonBeat(src);

        return new Ir.JudgeLine
        {
            Name = $"PhigrosLine_{index}",
            NoteList = NoteBuilder.ConvertNotes(src.NotesAbove, src.NotesBelow),
            EventLayers = [EventLayerBuilder.ConvertEventLayer(src, horizonBeat)],
            BpmFactor = defaultBpm / src.Bpm,
        };
    }
}
