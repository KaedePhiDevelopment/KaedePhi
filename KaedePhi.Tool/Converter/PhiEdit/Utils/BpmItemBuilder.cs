using KaedePhi.Core.Primitives;

namespace KaedePhi.Tool.Converter.PhiEdit.Utils;

/// <summary>
/// PE 与 IR BPM 项之间的双向转换工具。
/// </summary>
public static class BpmItemBuilder
{
    /// <summary>
    /// 将源数据转换为目标格式的数据。
    /// </summary>
    /// <param name="src">源对象</param>
    /// <returns>转换或构建得到的结果。</returns>
    public static Ir.BpmItem ConvertBpmItem(Pe.BpmItem src) =>
        new() { Bpm = src.Bpm, StartBeat = new Beat(src.StartBeat) };

    /// <summary>
    /// 将源数据转换为目标格式的数据。
    /// </summary>
    /// <param name="src">源对象</param>
    /// <returns>转换或构建得到的结果。</returns>
    public static Pe.BpmItem ConvertBpmItem(Ir.BpmItem src) =>
        new() { Bpm = src.Bpm, StartBeat = (float)(double)src.StartBeat };
}
