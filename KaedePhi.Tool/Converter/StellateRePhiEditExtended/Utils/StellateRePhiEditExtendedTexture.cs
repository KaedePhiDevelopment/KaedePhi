namespace KaedePhi.Tool.Converter.StellateRePhiEditExtended.Utils;

/// <summary>
/// 提供 StellateRePhiEditExtended 噪域标记纹理的识别功能。
/// </summary>
public static class StellateRePhiEditExtendedTexture
{
    private const string AddTextureFileName = "isSubtract0.png";
    private const string SubtractTextureFileName = "isSubtract1.png";

    /// <summary>
    /// 判断纹理路径是否表示噪域标记。
    /// </summary>
    /// <param name="texture">待检查的纹理路径</param>
    public static bool IsBlockAreaTexture(string? texture) =>
        HasFileName(texture, AddTextureFileName) || HasFileName(texture, SubtractTextureFileName);

    /// <summary>
    /// 判断纹理路径是否表示减法噪域。
    /// </summary>
    /// <param name="texture">待检查的纹理路径</param>
    public static bool IsSubtractTexture(string? texture) =>
        HasFileName(texture, SubtractTextureFileName);

    internal static string GetMarkerTexture(bool isSubtract) =>
        @"Pictures\" + (isSubtract ? SubtractTextureFileName : AddTextureFileName);

    private static bool HasFileName(string? texture, string expectedFileName) =>
        string.Equals(GetFileName(texture), expectedFileName, StringComparison.OrdinalIgnoreCase);

    private static string? GetFileName(string? texture)
    {
        if (string.IsNullOrWhiteSpace(texture))
            return null;

        var normalized = texture.Replace('\\', '/');
        return normalized[(normalized.LastIndexOf('/') + 1)..];
    }
}
