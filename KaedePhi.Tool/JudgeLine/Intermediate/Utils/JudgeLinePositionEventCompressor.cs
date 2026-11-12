using KaedePhi.Tool.Common;

namespace KaedePhi.Tool.JudgeLine.Intermediate.Utils;

/// <summary>
/// 根据二维运动范围联合压缩判定线的位置事件。
/// </summary>
internal static class JudgeLinePositionEventCompressor
{
    /// <summary>
    /// 使用原始运动范围的规范化相对误差联合压缩 X/Y 位置事件。
    /// </summary>
    internal static (
        List<IrEvents.Event<double>> X,
        List<IrEvents.Event<double>> Y
    ) CompressPositionEvents(
        List<IrEvents.Event<double>> xEvents,
        List<IrEvents.Event<double>> yEvents,
        double tolerance
    )
    {
        NumericParameterValidator.ValidateTolerance(tolerance);
        if (xEvents.Count == 0 || yEvents.Count == 0 || !ArePositionEventsAligned(xEvents, yEvents))
            return (xEvents, yEvents);

        var relativeTolerance = tolerance / 100.0;
        var compressedX = new List<IrEvents.Event<double>> { xEvents[0].Clone() };
        var compressedY = new List<IrEvents.Event<double>> { yEvents[0].Clone() };

        for (var i = 1; i < xEvents.Count; i++)
        {
            var lastX = compressedX[^1];
            var lastY = compressedY[^1];
            var currentX = xEvents[i];
            var currentY = yEvents[i];

            if (CanMergePositionSegments(lastX, lastY, currentX, currentY, relativeTolerance))
            {
                lastX.EndBeat = currentX.EndBeat;
                lastX.EndValue = currentX.EndValue;
                lastY.EndBeat = currentY.EndBeat;
                lastY.EndValue = currentY.EndValue;
                continue;
            }

            compressedX.Add(currentX.Clone());
            compressedY.Add(currentY.Clone());
        }

        return (compressedX, compressedY);
    }

    private static bool ArePositionEventsAligned(
        List<IrEvents.Event<double>> xEvents,
        List<IrEvents.Event<double>> yEvents
    )
    {
        if (xEvents.Count != yEvents.Count)
            return false;

        for (var i = 0; i < xEvents.Count; i++)
        {
            if (
                xEvents[i].StartBeat != yEvents[i].StartBeat
                || xEvents[i].EndBeat != yEvents[i].EndBeat
            )
                return false;
        }

        return true;
    }

    private static bool CanMergePositionSegments(
        IrEvents.Event<double> lastX,
        IrEvents.Event<double> lastY,
        IrEvents.Event<double> currentX,
        IrEvents.Event<double> currentY,
        double relativeTolerance
    )
    {
        if (
            !IsPositionEventMergeable(lastX)
            || !IsPositionEventMergeable(lastY)
            || !IsPositionEventMergeable(currentX)
            || !IsPositionEventMergeable(currentY)
            || lastX.EndBeat != currentX.StartBeat
            || lastY.EndBeat != currentY.StartBeat
        )
            return false;

        var actualJunction = (X: lastX.EndValue, Y: lastY.EndValue);
        var currentStart = (X: currentX.StartValue, Y: currentY.StartValue);
        var originalMovementRange = JudgeLineUnbindGeometry.GetNormalizedRange(
            (lastX.StartValue, lastY.StartValue),
            actualJunction,
            currentStart,
            (currentX.EndValue, currentY.EndValue)
        );
        if (originalMovementRange <= Constants.StrictFloatEpsilon)
            return true;

        if (
            JudgeLineUnbindGeometry.GetNormalizedDistance(actualJunction, currentStart)
            / originalMovementRange
            > relativeTolerance
        )
            return false;

        var startBeat = (double)lastX.StartBeat;
        var junctionBeat = (double)lastX.EndBeat;
        var endBeat = (double)currentX.EndBeat;
        var totalBeatSpan = endBeat - startBeat;
        if (totalBeatSpan < Constants.StrictFloatEpsilon)
            return true;

        var progress = (junctionBeat - startBeat) / totalBeatSpan;
        var predictedJunction = (
            X: lastX.StartValue + (currentX.EndValue - lastX.StartValue) * progress,
            Y: lastY.StartValue + (currentY.EndValue - lastY.StartValue) * progress
        );
        return JudgeLineUnbindGeometry.GetNormalizedDistance(actualJunction, predictedJunction)
            / originalMovementRange
            <= relativeTolerance;
    }

    private static bool IsPositionEventMergeable(IrEvents.Event<double> evt) =>
        !evt.IsBezier && evt.Easing == 1 && evt.Font is null;
}
