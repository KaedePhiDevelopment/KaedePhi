namespace KaedePhi.Tool.Common;

/// <summary>
/// 表示工具进度数据。
/// </summary>
public readonly struct ToolProgress
{
    /// <summary>
    /// 获取当前操作的完成比例。
    /// </summary>
    public double Percentage { get; }

    /// <summary>
    /// 获取整体处理流程的完成比例。
    /// </summary>
    public double OverallPercentage { get; }

    /// <summary>
    /// 获取当前进度的说明文字。
    /// </summary>
    public string? Detail { get; }

    /// <summary>
    /// 使用当前操作的完成比例和说明文字初始化进度信息。
    /// </summary>
    /// <param name="percentage">当前操作的完成比例。</param>
    /// <param name="detail">当前进度的说明文字。</param>
    public ToolProgress(double percentage, string? detail = null)
        : this(percentage, -1, detail) { }

    /// <summary>
    /// 使用当前操作和整体处理流程的完成比例初始化进度信息。
    /// </summary>
    /// <param name="percentage">当前操作的完成比例。</param>
    /// <param name="overallPercentage">整体处理流程的完成比例。</param>
    /// <param name="detail">当前进度的说明文字。</param>
    public ToolProgress(double percentage, double overallPercentage, string? detail = null)
    {
        Percentage = Math.Clamp(percentage, 0.0, 1.0);
        OverallPercentage = overallPercentage < 0 ? -1 : Math.Clamp(overallPercentage, 0.0, 1.0);
        Detail = detail;
    }
}
