using KaedePhi.Tool.Common;

namespace KaedePhi.Tool.Converter;

/// <summary>
/// 管线式图表转换入口
/// </summary>
public static class ChartPipeline
{
    /// <summary>
    /// 使用已有 IR 谱面的副本开始转换管线。
    /// </summary>
    /// <param name="chart">源 IR 谱面。</param>
    /// <param name="ct">取消令牌。</param>
    /// <returns>等待处理或转换的管线。</returns>
    public static ChartPipelineSource FromIr(Ir.Chart chart, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        return new ChartPipelineSource(
            IrChartNormalizer.NormalizeAndValidateNoteEndBeats(chart),
            ct
        );
    }

    /// <summary>
    /// 使用源格式和转换器开始一条转换管线。
    /// </summary>
    /// <typeparam name="TIn">源谱面类型。</typeparam>
    /// <typeparam name="TInOptions">源转换器的导入选项类型。</typeparam>
    /// <typeparam name="TOutOptions">源转换器的导出选项类型。</typeparam>
    /// <param name="input">源谱面。</param>
    /// <param name="converter">源转换器。</param>
    /// <param name="inOptions">导入转换选项。</param>
    /// <param name="ct">取消令牌。</param>
    /// <returns>等待处理或转换的管线。</returns>
    public static ChartPipelineSource From<TIn, TInOptions, TOutOptions>(
        TIn input,
        IChartConverter<TIn, TInOptions, TOutOptions> converter,
        TInOptions inOptions,
        CancellationToken ct = default
    )
    {
        ct.ThrowIfCancellationRequested();
        var ir = converter.ToIr(input, inOptions);
        return FromIr(ir, ct);
    }
}
