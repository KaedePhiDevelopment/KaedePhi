namespace KaedePhi.Tool.Converter;

/// <summary>
/// 谱面格式及其检测配置。
/// </summary>
public sealed class ChartFormatRegistration
{
    /// <summary>
    /// 创建谱面格式注册信息。
    /// </summary>
    /// <param name="descriptor">格式描述。</param>
    /// <param name="detector">返回使用同一检测器注册的格式标识，不匹配时返回空；流可能不可定位，回调返回后不可继续使用。</param>
    /// <param name="priority">检测优先级，越大越优先。</param>
    public ChartFormatRegistration(
        ChartFormatDescriptor descriptor,
        Func<Stream, CancellationToken, ValueTask<string?>>? detector = null,
        int priority = 0
    )
    {
        ArgumentNullException.ThrowIfNull(descriptor);
        Descriptor = descriptor;
        Detector = detector;
        Priority = priority;
    }

    /// <summary>谱面格式描述。</summary>
    public ChartFormatDescriptor Descriptor { get; }

    /// <summary>返回匹配格式标识的检测器；多个格式可共用同一检测器，每次检测只执行一次。</summary>
    public Func<Stream, CancellationToken, ValueTask<string?>>? Detector { get; }

    /// <summary>检测优先级。</summary>
    public int Priority { get; }
}
