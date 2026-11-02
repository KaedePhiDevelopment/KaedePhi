using PhigrosChart = KaedePhi.Core.Formats.Phigros.v3.Model.Chart;

namespace KaedePhi.Tool.Converter.Phigros.v3.Utils;

/// <summary>
/// PhigrosV3 元数据到 IR 元数据的转换工具。
/// </summary>
public static class MetaBuilder
{
    /// <summary>
    /// 将源数据转换为目标格式的数据。
    /// </summary>
    /// <param name="src">源对象</param>
    /// <returns>转换或构建得到的结果。</returns>
    public static Ir.Meta ConvertMeta(PhigrosChart src) =>
        new() { Offset = (int)(src.Offset * 1000) };
}
