namespace KaedePhi.Tool.Converter.RePhiEdit.Utils;

/// <summary>
/// RPE 与 IR 元数据之间的双向转换工具。
/// </summary>
public static class MetaBuilder
{
    /// <summary>
    /// 将源数据转换为目标格式的数据。
    /// </summary>
    /// <param name="src">源对象</param>
    /// <returns>转换或构建得到的结果。</returns>
    public static Ir.Meta ConvertMeta(Rpe.Meta src) =>
        new()
        {
            Background = src.Background,
            Author = src.Charter,
            Composer = src.Composer,
            Artist = src.Illustration,
            Level = src.Level,
            Name = src.Name,
            Offset = src.Offset,
            Song = src.Song,
        };

    /// <summary>
    /// 将源数据转换为目标格式的数据。
    /// </summary>
    /// <param name="src">源对象</param>
    /// <returns>转换或构建得到的结果。</returns>
    public static Rpe.Meta ConvertMeta(Ir.Meta src) =>
        new()
        {
            Background = src.Background,
            Charter = src.Author,
            Composer = src.Composer,
            Illustration = src.Artist,
            Level = src.Level,
            Name = src.Name,
            Offset = src.Offset,
            Song = src.Song,
        };
}
