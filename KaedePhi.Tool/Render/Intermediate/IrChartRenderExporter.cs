using KaedePhi.Tool.Common;
using Chart = KaedePhi.Core.Intermediate.Model.Chart;

namespace KaedePhi.Tool.Render.Intermediate;

/// <summary>
/// 已移除的 IR 谱面渲染导出器：渲染工具已被彻底移除，仅为二进制兼容保留。
/// </summary>
[Obsolete(RenderRemovedNotice.Message, true)]
public class IrChartRenderExporter : LoggableBase, IChartRenderExporter<Chart, IrRenderOptions>
{
    /// <summary>
    /// 已移除的渲染入口，调用始终抛出未实现异常。
    /// </summary>
    /// <param name="chart">谱面对象。</param>
    /// <param name="outputDir">输出目录。</param>
    /// <param name="opts">渲染配置。</param>
    /// <param name="lineIndex">判定线索引。</param>
    /// <param name="layerIndex">事件层索引。</param>
    /// <param name="progress">进度回调。</param>
    /// <param name="ct">取消令牌。</param>
    /// <returns>不会正常返回。</returns>
    /// <exception cref="NotImplementedException">渲染工具已被彻底移除，始终抛出。</exception>
    public IReadOnlyList<string> ExportChart(
        Chart chart,
        string outputDir,
        IrRenderOptions opts,
        int? lineIndex = null,
        int? layerIndex = null,
        IProgress<ToolProgress>? progress = null,
        CancellationToken ct = default
    ) => throw RenderRemovedNotice.CreateException();
}
