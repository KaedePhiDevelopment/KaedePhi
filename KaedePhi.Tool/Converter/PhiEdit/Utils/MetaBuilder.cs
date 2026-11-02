namespace KaedePhi.Tool.Converter.PhiEdit.Utils;

/// <summary>
/// PE 与 IR 元数据之间的转换工具。
/// </summary>
public static class MetaBuilder
{
    private const int OffsetOffset = 175;

    /// <summary>
    /// 将源数据转换为目标格式的数据。
    /// </summary>
    /// <param name="src">源对象</param>
    /// <returns>转换或构建得到的结果。</returns>
    public static Ir.Meta ConvertMeta(Pe.Chart src) => new() { Offset = src.Offset - OffsetOffset };

    /// <summary>
    /// 获取谱面编辑格式偏移量。
    /// </summary>
    /// <param name="src">源对象</param>
    /// <returns>计算得到的数值。</returns>
    public static int GetPeOffset(Ir.Meta src) => src.Offset + OffsetOffset;
}
