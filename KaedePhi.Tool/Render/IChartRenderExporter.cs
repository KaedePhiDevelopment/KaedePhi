using KaedePhi.Tool.Common;

namespace KaedePhi.Tool.Render;

/// <summary>
/// 已移除的谱面渲染导出器接口：渲染工具已被彻底移除，该接口不可再被引用或实现。
/// </summary>
/// <typeparam name="TChart">谱面类型。</typeparam>
/// <typeparam name="TRenderOptions">渲染配置类型。</typeparam>
[Obsolete(RenderRemovedNotice.Message, true)]
public interface IChartRenderExporter<in TChart, in TRenderOptions> : ILoggable
{
    /// <summary>
    /// 已移除的渲染入口，任何实现均不再提供功能。
    /// </summary>
    /// <param name="chart">谱面对象。</param>
    /// <param name="outputDir">输出目录。</param>
    /// <param name="opts">渲染配置。</param>
    /// <param name="lineIndex">判定线索引。</param>
    /// <param name="layerIndex">事件层索引。</param>
    /// <param name="progress">进度回调。</param>
    /// <param name="ct">取消令牌。</param>
    /// <returns>该接口已移除，不会产生有效返回值。</returns>
    IReadOnlyList<string> ExportChart(
        TChart chart,
        string outputDir,
        TRenderOptions opts,
        int? lineIndex = null,
        int? layerIndex = null,
        IProgress<ToolProgress>? progress = null,
        CancellationToken ct = default
    );
}
