using KaedePhi.Tool.Common;

namespace KaedePhi.Tool.JudgeLine.Utils;

/// <summary>
/// 唯一标识一次判定线解绑计算的缓存参数。
/// </summary>
/// <param name="TargetJudgeLineIndex">目标判定线索引。</param>
/// <param name="Precision">采样精度。</param>
/// <param name="Tolerance">几何拟合容差。</param>
/// <param name="MergeTolerance">事件合并容差。</param>
/// <param name="Adaptive">是否使用自适应采样。</param>
/// <param name="RenderProfile">渲染坐标配置。</param>
public readonly record struct JudgeLineUnbindCacheKey(
    int TargetJudgeLineIndex,
    double Precision,
    double Tolerance,
    double MergeTolerance,
    bool Adaptive,
    CoordinateProfile RenderProfile
);
