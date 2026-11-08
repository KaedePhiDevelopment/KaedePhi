using KaedePhi.Tool.Render;
using KaedePhi.Tool.Render.KaedePhi;
using Kpc = KaedePhi.Core.KaedePhi;
using KpcEvents = KaedePhi.Core.KaedePhi.Events;

namespace KaedePhi.Tool.Common;

/// <summary>
/// 已移除的 KPC 渲染参数校验入口：渲染工具已被彻底移除，仅为二进制兼容保留。
/// </summary>
[Obsolete(RenderRemovedNotice.Message, true)]
public static class KpcRenderValidator
{
    /// <summary>
    /// 已移除的渲染像素上限常量，仅为兼容保留。
    /// </summary>
    public const long MaximumRenderPixels = IrRenderValidator.MaximumRenderPixels;

    /// <summary>
    /// 已移除的渲染参数校验入口，调用始终抛出未实现异常。
    /// </summary>
    /// <param name="chart">待渲染的 KPC 谱面。</param>
    /// <param name="options">渲染配置。</param>
    /// <param name="lineIndex">可选判定线索引。</param>
    /// <param name="layerIndex">可选事件层索引。</param>
    /// <exception cref="NotImplementedException">渲染工具已被彻底移除，始终抛出。</exception>
    public static void Validate(
        Kpc.Chart chart,
        KpcRenderOptions options,
        int? lineIndex = null,
        int? layerIndex = null
    ) => throw RenderRemovedNotice.CreateException();

    /// <summary>
    /// 已移除的渲染配置校验入口，调用始终抛出未实现异常。
    /// </summary>
    /// <param name="options">渲染配置。</param>
    /// <exception cref="NotImplementedException">渲染工具已被彻底移除，始终抛出。</exception>
    public static void ValidateOptions(KpcRenderOptions options) =>
        throw RenderRemovedNotice.CreateException();

    /// <summary>
    /// 已移除的事件层校验入口，调用始终抛出未实现异常。
    /// </summary>
    /// <param name="layer">待渲染的事件层。</param>
    /// <param name="options">渲染配置。</param>
    /// <exception cref="NotImplementedException">渲染工具已被彻底移除，始终抛出。</exception>
    public static void ValidateEventLayer(KpcEvents.EventLayer layer, KpcRenderOptions options) =>
        throw RenderRemovedNotice.CreateException();
}
