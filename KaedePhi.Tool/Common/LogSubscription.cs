namespace KaedePhi.Tool.Common;

/// <summary>
/// 日志订阅句柄，释放时自动取消订阅。
/// </summary>
public sealed class LogSubscription : IDisposable
{
    private readonly Action _unsubscribe;
    private bool _disposed;

    /// <summary>
    /// 初始化日志订阅的新实例。
    /// </summary>
    /// <param name="unsubscribe">输入值。</param>
    public LogSubscription(Action unsubscribe)
    {
        _unsubscribe = unsubscribe;
    }

    /// <summary>
    /// 释放当前对象占用的资源。
    /// </summary>
    public void Dispose()
    {
        if (_disposed)
            return;
        _unsubscribe();
        _disposed = true;
    }
}
