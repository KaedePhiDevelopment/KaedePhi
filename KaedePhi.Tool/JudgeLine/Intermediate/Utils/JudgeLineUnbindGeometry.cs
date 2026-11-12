using KaedePhi.Core.Primitives;
using KaedePhi.Tool.Common;
using KaedePhi.Tool.JudgeLine.Utils;

namespace KaedePhi.Tool.JudgeLine.Intermediate.Utils;

/// <summary>
/// 提供判定线坐标转换与解绑误差计算。
/// </summary>
internal static class JudgeLineUnbindGeometry
{
    /// <summary>
    /// 根据父线绝对坐标和旋转角度计算子线绝对坐标。
    /// </summary>
    internal static (double X, double Y) GetLinePos(
        double fatherLineX,
        double fatherLineY,
        double angleDegrees,
        double lineX,
        double lineY
    )
    {
        return CoordinateGeometry.GetIrAbsolutePos(
            fatherLineX,
            fatherLineY,
            angleDegrees,
            lineX,
            lineY,
            JudgeLineUnbindContext.CurrentRenderProfile
        );
    }

    /// <summary>
    /// 计算两个位置之间相对于 IR 坐标范围的距离。
    /// </summary>
    internal static double GetNormalizedDistance(
        (double X, double Y) left,
        (double X, double Y) right
    )
    {
        var profile = CoordinateProfile.IrProfile;
        var normalizedX = (left.X - right.X) / (profile.MaxX - profile.MinX);
        var normalizedY = (left.Y - right.Y) / (profile.MaxY - profile.MinY);
        return Math.Sqrt(normalizedX * normalizedX + normalizedY * normalizedY);
    }

    /// <summary>
    /// 计算四个位置覆盖范围相对于 IR 坐标范围的大小。
    /// </summary>
    internal static double GetNormalizedRange(
        (double X, double Y) first,
        (double X, double Y) second,
        (double X, double Y) third,
        (double X, double Y) fourth
    )
    {
        var minX = Math.Min(Math.Min(first.X, second.X), Math.Min(third.X, fourth.X));
        var maxX = Math.Max(Math.Max(first.X, second.X), Math.Max(third.X, fourth.X));
        var minY = Math.Min(Math.Min(first.Y, second.Y), Math.Min(third.Y, fourth.Y));
        var maxY = Math.Max(Math.Max(first.Y, second.Y), Math.Max(third.Y, fourth.Y));
        return GetNormalizedDistance((maxX, maxY), (minX, minY));
    }

    /// <summary>
    /// 判断线段相对原始运动范围的误差是否需要进一步切割。
    /// </summary>
    internal static bool NeedsAdaptiveCut(
        (double X, double Y) segmentStart,
        (double X, double Y) next,
        (double X, double Y) intervalEnd,
        Beat segmentStartBeat,
        Beat intervalEndBeat,
        Beat nextBeat,
        double tolerance,
        double originalMovementRange = 0d
    )
    {
        var segmentLength = (double)(intervalEndBeat - segmentStartBeat);
        var progress = segmentLength > 1e-12 ? (nextBeat - segmentStartBeat) / segmentLength : 1.0;
        var predicted = (
            X: segmentStart.X + (intervalEnd.X - segmentStart.X) * progress,
            Y: segmentStart.Y + (intervalEnd.Y - segmentStart.Y) * progress
        );

        var normalizedError = GetNormalizedDistance(next, predicted);
        var movementRange =
            originalMovementRange > 1e-12
                ? originalMovementRange
                : GetNormalizedDistance(segmentStart, intervalEnd);
        if (movementRange <= 1e-12)
            return normalizedError > 1e-12;

        return normalizedError / movementRange > tolerance / 100.0;
    }
}
