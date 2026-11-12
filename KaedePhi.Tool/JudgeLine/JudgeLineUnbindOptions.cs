using KaedePhi.Tool.Common;

namespace KaedePhi.Tool.JudgeLine;

/// <summary>
/// 指定判定线解绑所需的采样策略、采样精度和渲染坐标系。
/// </summary>
public sealed record JudgeLineUnbindOptions
{
    /// <summary>
    /// 获取或设置解绑时使用的采样策略。
    /// </summary>
    public JudgeLineUnbindSamplingMode SamplingMode { get; init; } =
        JudgeLineUnbindSamplingMode.EqualSpacing;

    /// <summary>
    /// 获取或设置每拍内的采样步数；自适应采样时表示最大采样步数。
    /// </summary>
    public double Precision { get; init; } = 64d;

    /// <summary>
    /// 获取或设置自适应采样的几何拟合容差百分比。
    /// </summary>
    public double Tolerance { get; init; } = 1d;

    /// <summary>
    /// 获取或设置自适应采样的事件通道合并容差百分比。
    /// </summary>
    public double MergeTolerance { get; init; } = 0.1d;

    /// <summary>
    /// 获取或设置解绑计算使用的渲染坐标系；未指定时使用默认坐标系。
    /// </summary>
    public CoordinateProfile? RenderProfile { get; init; }
}
