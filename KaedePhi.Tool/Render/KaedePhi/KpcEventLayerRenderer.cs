using SkiaSharp;
using KpcEvents = KaedePhi.Core.KaedePhi.Events;

namespace KaedePhi.Tool.Render.KaedePhi;

/// <summary>
/// 已移除的 KPC 事件层渲染入口：渲染工具已被彻底移除，仅为二进制兼容保留。
/// </summary>
[Obsolete(RenderRemovedNotice.Message, true)]
public static class KpcEventLayerRenderer
{
    /// <summary>
    /// 已移除的事件层渲染入口，调用始终抛出未实现异常。
    /// </summary>
    /// <param name="layer">事件层。</param>
    /// <param name="opts">渲染配置。</param>
    /// <exception cref="NotImplementedException">渲染工具已被彻底移除，始终抛出。</exception>
    public static SKBitmap RenderEventLayer(KpcEvents.EventLayer layer, KpcRenderOptions opts) =>
        throw RenderRemovedNotice.CreateException();
}
