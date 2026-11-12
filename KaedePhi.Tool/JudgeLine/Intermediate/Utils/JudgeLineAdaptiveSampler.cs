using KaedePhi.Core.Primitives;

namespace KaedePhi.Tool.JudgeLine.Intermediate.Utils;

/// <summary>
/// 根据事件边界和几何误差自适应生成解绑后的 X/Y 事件。
/// </summary>
internal static class JudgeLineAdaptiveSampler
{
    /// <summary>
    /// 获取五个事件通道的总体拍范围；所有通道均为空时返回 null。
    /// </summary>
    internal static (Beat Min, Beat Max)? TryGetOverallRange(
        JudgeLineUnbindEventChannels channels
    )
    {
        Beat overallMin = new(0),
            overallMax = new(0);
        var hasEvents = false;
        foreach (var events in new[] { channels.Tx, channels.Ty, channels.Fx, channels.Fy, channels.Fr })
        {
            if (events.Count == 0)
                continue;
            var (min, max) = JudgeLineUnbindEvents.GetEventRange(events);
            if (!hasEvents)
            {
                overallMin = min;
                overallMax = max;
                hasEvents = true;
            }
            else
            {
                if (min < overallMin)
                    overallMin = min;
                if (max > overallMax)
                    overallMax = max;
            }
        }

        return hasEvents ? (overallMin, overallMax) : null;
    }

    /// <summary>
    /// 收集所有通道事件的起止拍作为自适应采样的强制切割点。
    /// </summary>
    internal static List<Beat> CollectKeyBeats(
        Beat overallMin,
        Beat overallMax,
        JudgeLineUnbindEventChannels channels
    )
    {
        var keyBeats = new List<Beat> { overallMin, overallMax };
        foreach (var events in new[] { channels.Tx, channels.Ty, channels.Fx, channels.Fy, channels.Fr })
        {
            foreach (var evt in events)
            {
                if (evt.StartBeat >= overallMin && evt.StartBeat <= overallMax)
                    keyBeats.Add(evt.StartBeat);
                if (evt.EndBeat >= overallMin && evt.EndBeat <= overallMax)
                    keyBeats.Add(evt.EndBeat);
            }
        }

        return keyBeats.Distinct().OrderBy(beat => beat).ToList();
    }

    /// <summary>
    /// 并行处理关键拍区间并按区间顺序合并采样结果。
    /// </summary>
    internal static (
        List<IrEvents.Event<double>> X,
        List<IrEvents.Event<double>> Y
    ) Sample(
        List<Beat> keyBeats,
        Beat step,
        double tolerance,
        JudgeLineUnbindEventChannels channels,
        CancellationToken ct = default
    )
    {
        var segmentCount = keyBeats.Count - 1;
        var segmentsX = new List<IrEvents.Event<double>>[segmentCount];
        var segmentsY = new List<IrEvents.Event<double>>[segmentCount];
        for (var i = 0; i < segmentCount; i++)
        {
            segmentsX[i] = [];
            segmentsY[i] = [];
        }

        Parallel.For(
            0,
            segmentCount,
            new ParallelOptions { CancellationToken = ct },
            index =>
            {
                ct.ThrowIfCancellationRequested();
                if (keyBeats[index] >= keyBeats[index + 1])
                    return;
                var (xEvents, yEvents) = SampleInterval(
                    keyBeats[index],
                    keyBeats[index + 1],
                    step,
                    tolerance,
                    PositionIn,
                    PositionOut,
                    ct
                );
                segmentsX[index].AddRange(xEvents);
                segmentsY[index].AddRange(yEvents);
            }
        );

        var resultX = new List<IrEvents.Event<double>>();
        var resultY = new List<IrEvents.Event<double>>();
        foreach (var segment in segmentsX)
            resultX.AddRange(segment);
        foreach (var segment in segmentsY)
            resultY.AddRange(segment);
        return (resultX, resultY);

        (double X, double Y) PositionIn(Beat beat) =>
            JudgeLineUnbindGeometry.GetLinePos(
                JudgeLineUnbindEvents.GetValIn(channels.Fx, beat),
                JudgeLineUnbindEvents.GetValIn(channels.Fy, beat),
                JudgeLineUnbindEvents.GetValIn(channels.Fr, beat),
                JudgeLineUnbindEvents.GetValIn(channels.Tx, beat),
                JudgeLineUnbindEvents.GetValIn(channels.Ty, beat)
            );

        (double X, double Y) PositionOut(Beat beat) =>
            JudgeLineUnbindGeometry.GetLinePos(
                JudgeLineUnbindEvents.GetValOut(channels.Fx, beat),
                JudgeLineUnbindEvents.GetValOut(channels.Fy, beat),
                JudgeLineUnbindEvents.GetValOut(channels.Fr, beat),
                JudgeLineUnbindEvents.GetValOut(channels.Tx, beat),
                JudgeLineUnbindEvents.GetValOut(channels.Ty, beat)
            );
    }

    private static (
        List<IrEvents.Event<double>> X,
        List<IrEvents.Event<double>> Y
    ) SampleInterval(
        Beat intervalStart,
        Beat intervalEnd,
        Beat step,
        double tolerance,
        Func<Beat, (double X, double Y)> positionIn,
        Func<Beat, (double X, double Y)> positionOut,
        CancellationToken ct
    )
    {
        var localX = new List<IrEvents.Event<double>>();
        var localY = new List<IrEvents.Event<double>>();

        var end = positionOut(intervalEnd);
        var originalMovementRange = GetOriginalMovementRange(
            intervalStart,
            intervalEnd,
            step,
            positionIn,
            positionOut,
            ct
        );
        var segmentStart = intervalStart;
        var segmentPosition = positionIn(intervalStart);

        for (var current = intervalStart; current < intervalEnd; )
        {
            ct.ThrowIfCancellationRequested();
            var next = current + step > intervalEnd ? intervalEnd : current + step;
            var isLast = next >= intervalEnd;
            var nextPosition = isLast ? end : positionIn(next);

            if (
                isLast
                || JudgeLineUnbindGeometry.NeedsAdaptiveCut(
                    segmentPosition,
                    nextPosition,
                    end,
                    segmentStart,
                    intervalEnd,
                    next,
                    tolerance,
                    originalMovementRange
                )
            )
            {
                localX.Add(
                    new IrEvents.Event<double>
                    {
                        StartBeat = segmentStart,
                        EndBeat = next,
                        StartValue = segmentPosition.X,
                        EndValue = nextPosition.X,
                    }
                );
                localY.Add(
                    new IrEvents.Event<double>
                    {
                        StartBeat = segmentStart,
                        EndBeat = next,
                        StartValue = segmentPosition.Y,
                        EndValue = nextPosition.Y,
                    }
                );
                segmentStart = next;
                segmentPosition = nextPosition;
            }

            current = next;
        }

        return (localX, localY);
    }

    private static double GetOriginalMovementRange(
        Beat intervalStart,
        Beat intervalEnd,
        Beat step,
        Func<Beat, (double X, double Y)> positionIn,
        Func<Beat, (double X, double Y)> positionOut,
        CancellationToken ct
    )
    {
        var first = positionIn(intervalStart);
        var minX = first.X;
        var maxX = first.X;
        var minY = first.Y;
        var maxY = first.Y;

        for (var current = intervalStart; current < intervalEnd; )
        {
            ct.ThrowIfCancellationRequested();
            var next = current + step > intervalEnd ? intervalEnd : current + step;
            var point = next >= intervalEnd ? positionOut(intervalEnd) : positionIn(next);
            minX = Math.Min(minX, point.X);
            maxX = Math.Max(maxX, point.X);
            minY = Math.Min(minY, point.Y);
            maxY = Math.Max(maxY, point.Y);
            if (next <= current)
                break;
            current = next;
        }

        return JudgeLineUnbindGeometry.GetNormalizedDistance((maxX, maxY), (minX, minY));
    }
}
