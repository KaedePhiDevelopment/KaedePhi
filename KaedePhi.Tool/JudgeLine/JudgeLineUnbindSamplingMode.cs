namespace KaedePhi.Tool.JudgeLine;

/// <summary>
/// 判定线解绑时使用的采样策略。
/// </summary>
public enum JudgeLineUnbindSamplingMode
{
    /// <summary>
    /// 按固定拍间隔采样。
    /// </summary>
    EqualSpacing,

    /// <summary>
    /// 根据运动误差自适应采样。
    /// </summary>
    Adaptive,
}
