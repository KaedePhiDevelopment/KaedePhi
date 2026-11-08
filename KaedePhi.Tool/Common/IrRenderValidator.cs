using KaedePhi.Core.Intermediate.Model;
using KaedePhi.Tool.Render;
using KaedePhi.Tool.Render.Intermediate;

namespace KaedePhi.Tool.Common;

/// <summary>
/// 已移除的渲染参数校验入口：渲染工具已被彻底移除，仅为二进制兼容保留。
/// </summary>
[Obsolete(RenderRemovedNotice.Message, true)]
public static class IrRenderValidator
{
    /// <summary>
    /// 已移除的渲染像素上限常量，仅为兼容保留。
    /// </summary>
    public const long MaximumRenderPixels = 200_000_000L;

    /// <summary>
    /// 已移除的渲染参数校验入口，调用始终抛出未实现异常。
    /// </summary>
    /// <param name="chart">待渲染谱面。</param>
    /// <param name="options">渲染配置。</param>
    /// <param name="lineIndex">可选判定线索引。</param>
    /// <param name="layerIndex">可选事件层索引。</param>
    /// <exception cref="NotImplementedException">渲染工具已被彻底移除，始终抛出。</exception>
    public static void Validate(
        Chart chart,
        IrRenderOptions options,
        int? lineIndex = null,
        int? layerIndex = null
    ) => throw RenderRemovedNotice.CreateException();

    /// <summary>
    /// 已移除的渲染配置校验入口，调用始终抛出未实现异常。
    /// </summary>
    /// <param name="options">渲染配置。</param>
    /// <exception cref="NotImplementedException">渲染工具已被彻底移除，始终抛出。</exception>
    public static void ValidateOptions(IrRenderOptions options) =>
        throw RenderRemovedNotice.CreateException();

    /// <summary>
    /// 已移除的事件层校验入口，调用始终抛出未实现异常。
    /// </summary>
    /// <param name="layer">待渲染的事件层。</param>
    /// <param name="options">渲染配置。</param>
    /// <exception cref="NotImplementedException">渲染工具已被彻底移除，始终抛出。</exception>
    public static void ValidateEventLayer(IrEvents.EventLayer layer, IrRenderOptions options) =>
        throw RenderRemovedNotice.CreateException();
}
