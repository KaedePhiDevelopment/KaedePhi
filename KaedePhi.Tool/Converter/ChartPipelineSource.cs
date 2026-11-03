using KaedePhi.Tool.Common;

namespace KaedePhi.Tool.Converter;

/// <summary>
/// 持有 IR 谱面并等待处理或转换的管线。
/// </summary>
public sealed class ChartPipelineSource
{
    private readonly Ir.Chart _ir;
    private readonly CancellationToken _ct;

    internal ChartPipelineSource(Ir.Chart ir, CancellationToken ct)
    {
        _ir = ir;
        _ct = ct;
    }

    /// <summary>
    /// 对 IR 谱面的副本应用处理，并创建新的管线。
    /// </summary>
    /// <param name="transform">接收 IR 副本并返回处理结果的回调。</param>
    /// <returns>持有处理结果副本的新管线。</returns>
    public ChartPipelineSource Transform(Func<Ir.Chart, Ir.Chart> transform)
    {
        ArgumentNullException.ThrowIfNull(transform);
        _ct.ThrowIfCancellationRequested();
        var input = IrChartNormalizer.NormalizeAndValidateNoteEndBeats(_ir);
        var result = transform(input);
        return ChartPipeline.FromIr(result, _ct);
    }

    /// <summary>
    /// 获取当前管线的 IR 谱面副本。
    /// </summary>
    /// <returns>IR 谱面副本。</returns>
    public Ir.Chart ToIr()
    {
        _ct.ThrowIfCancellationRequested();
        return IrChartNormalizer.NormalizeAndValidateNoteEndBeats(_ir);
    }

    /// <summary>
    /// 使用目标转换器和选项完成管线。
    /// </summary>
    /// <typeparam name="TOut">目标谱面类型。</typeparam>
    /// <typeparam name="TInOptions">目标转换器的导入选项类型。</typeparam>
    /// <typeparam name="TOutOptions">目标转换器的导出选项类型。</typeparam>
    /// <param name="toConverter">目标转换器。</param>
    /// <param name="outOptions">导出转换选项。</param>
    /// <returns>目标格式谱面。</returns>
    public TOut To<TOut, TInOptions, TOutOptions>(
        IChartConverter<TOut, TInOptions, TOutOptions> toConverter,
        TOutOptions outOptions
    )
    {
        ArgumentNullException.ThrowIfNull(toConverter);
        return toConverter.FromIr(ToIr(), outOptions);
    }
}
