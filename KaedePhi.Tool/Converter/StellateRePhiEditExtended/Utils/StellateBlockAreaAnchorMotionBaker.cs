using KaedePhi.Core.Primitives;
using KaedePhi.Tool.Common;
using IrBlockArea = KaedePhi.Core.Intermediate.Model.BlockArea;
using IrEvent = KaedePhi.Core.Intermediate.Model.Events.Event<double>;

namespace KaedePhi.Tool.Converter.StellateRePhiEditExtended.Utils;

/// <summary>
/// 将 IR 噪域的锚点变换烘焙为位置事件。
/// </summary>
public static class StellateBlockAreaAnchorMotionBaker
{
    /// <summary>
    /// 根据锚点、缩放和旋转事件生成烘焙后的位置轨道。
    /// </summary>
    /// <param name="source">源 IR 噪域</param>
    /// <param name="blockAreaIndex">噪域索引</param>
    /// <param name="samplingPrecision">每拍使用的采样段数</param>
    /// <param name="cancellationToken">用于取消处理的令牌</param>
    public static (
        double CenterX,
        double CenterY,
        List<IrEvent> MoveXEvents,
        List<IrEvent> MoveYEvents
    ) BakeAnchorMotion(
        IrBlockArea source,
        int blockAreaIndex,
        int samplingPrecision,
        CancellationToken cancellationToken
    )
    {
        var baseCenterX = source.TopRightX / 2d + source.BottomLeftX / 2d;
        var baseCenterY = source.TopRightY / 2d + source.BottomLeftY / 2d;

        List<IrEvent> Prepare(List<IrEvent>? events, string trackName) =>
            PrepareSamplingEvents(events, blockAreaIndex, trackName, cancellationToken);

        var moveX = Prepare(source.MoveXEvents, ToolResources.stellate_track_move_x);
        var moveY = Prepare(source.MoveYEvents, ToolResources.stellate_track_move_y);
        var rotate = Prepare(source.RotateEvents, ToolResources.stellate_track_rotate);
        var rotateAnchorX = Prepare(
            source.RotateAnchorXEvents,
            ToolResources.stellate_track_rotate_anchor_x
        );
        var rotateAnchorY = Prepare(
            source.RotateAnchorYEvents,
            ToolResources.stellate_track_rotate_anchor_y
        );
        var scaleX = Prepare(source.ScaleXEvents, ToolResources.stellate_track_scale_x);
        var scaleY = Prepare(source.ScaleYEvents, ToolResources.stellate_track_scale_y);
        var scaleAnchorX = Prepare(
            source.ScaleAnchorXEvents,
            ToolResources.stellate_track_scale_anchor_x
        );
        var scaleAnchorY = Prepare(
            source.ScaleAnchorYEvents,
            ToolResources.stellate_track_scale_anchor_y
        );
        var tracks = new[]
        {
            moveX,
            moveY,
            rotate,
            rotateAnchorX,
            rotateAnchorY,
            scaleX,
            scaleY,
            scaleAnchorX,
            scaleAnchorY,
        };
        var sampleBeats = CreateAnchorSampleBeats(
            tracks,
            samplingPrecision,
            blockAreaIndex,
            cancellationToken
        );

        (double X, double Y) EvaluateCenter(Beat beat, bool rightSide)
        {
            var moveCenterX = EvaluateTrack(moveX, beat, baseCenterX, rightSide);
            var moveCenterY = EvaluateTrack(moveY, beat, baseCenterY, rightSide);
            var rotation = EvaluateTrack(rotate, beat, 0d, rightSide);
            var rotationAnchorValueX = EvaluateAnchor(rotateAnchorX, beat, baseCenterX, rightSide);
            var rotationAnchorValueY = EvaluateAnchor(rotateAnchorY, beat, baseCenterY, rightSide);
            var scaleValueX = EvaluateTrack(scaleX, beat, 1d, rightSide);
            var scaleValueY = EvaluateTrack(scaleY, beat, 1d, rightSide);
            var scaleAnchorValueX = EvaluateAnchor(scaleAnchorX, beat, baseCenterX, rightSide);
            var scaleAnchorValueY = EvaluateAnchor(scaleAnchorY, beat, baseCenterY, rightSide);

            // 锚点为绝对坐标：先变换原始中心，再叠加移动中心相对原始中心的偏移
            var scaledCenterX = scaleAnchorValueX + (baseCenterX - scaleAnchorValueX) * scaleValueX;
            var scaledCenterY = scaleAnchorValueY + (baseCenterY - scaleAnchorValueY) * scaleValueY;
            var rotatedOffset = CoordinateGeometry.RotateIrOffset(
                scaledCenterX - rotationAnchorValueX,
                scaledCenterY - rotationAnchorValueY,
                rotation
            );
            var centerX = rotationAnchorValueX + rotatedOffset.X + moveCenterX - baseCenterX;
            var centerY = rotationAnchorValueY + rotatedOffset.Y + moveCenterY - baseCenterY;
            if (!double.IsFinite(centerX) || !double.IsFinite(centerY))
                throw StellateExceptions.AnchorPositionNotFinite(blockAreaIndex);

            return (centerX, centerY);
        }

        var samples = new List<(
            Beat Beat,
            double LeftX,
            double LeftY,
            double RightX,
            double RightY
        )>(sampleBeats.Count);
        foreach (var beat in sampleBeats)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var right = EvaluateCenter(beat, rightSide: true);
            var left =
                beat == StellateBlockAreaConstants.ZeroBeat
                    ? right
                    : EvaluateCenter(beat, rightSide: false);
            samples.Add((beat, left.X, left.Y, right.X, right.Y));
        }

        return (
            samples[0].RightX,
            samples[0].RightY,
            CreatePositionEvents(samples, xAxis: true, cancellationToken),
            CreatePositionEvents(samples, xAxis: false, cancellationToken)
        );
    }

    private static List<Beat> CreateAnchorSampleBeats(
        IEnumerable<List<IrEvent>> tracks,
        int samplingPrecision,
        int blockAreaIndex,
        CancellationToken cancellationToken
    )
    {
        var beats = new SortedSet<Beat> { StellateBlockAreaConstants.ZeroBeat };
        foreach (var evt in tracks.SelectMany(events => events))
        {
            cancellationToken.ThrowIfCancellationRequested();
            AddBoundary(evt.StartBeat);
            AddBoundary(evt.EndBeat);
        }

        var endBeat = (double)beats.Max;
        if (!double.IsFinite(endBeat))
            throw StellateExceptions.AnchorTimeInvalid(blockAreaIndex);

        var step = 1d / samplingPrecision;
        var currentBeat = 0d;
        while (currentBeat < endBeat)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var nextBeat = Math.Min(currentBeat + step, endBeat);
            if (!double.IsFinite(nextBeat) || nextBeat <= currentBeat)
                throw StellateExceptions.AnchorStepStalled(blockAreaIndex);
            beats.Add(new Beat(nextBeat));
            EnsureSampleLimit();
            currentBeat = nextBeat;
        }

        return [.. beats];

        void AddBoundary(Beat beat)
        {
            if (beat > StellateBlockAreaConstants.ZeroBeat && beats.Add(beat))
                EnsureSampleLimit();
        }

        void EnsureSampleLimit()
        {
            if (beats.Count > StellateBlockAreaConstants.MaximumAnchorSamples)
                throw StellateExceptions.AnchorSamplesExceeded(
                    blockAreaIndex,
                    StellateBlockAreaConstants.MaximumAnchorSamples
                );
        }
    }

    private static List<IrEvent> CreatePositionEvents(
        IReadOnlyList<(
            Beat Beat,
            double LeftX,
            double LeftY,
            double RightX,
            double RightY
        )> samples,
        bool xAxis,
        CancellationToken cancellationToken
    )
    {
        var result = new List<IrEvent>();
        for (var index = 0; index < samples.Count; index++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var sample = samples[index];
            var leftValue = xAxis ? sample.LeftX : sample.LeftY;
            var rightValue = xAxis ? sample.RightX : sample.RightY;
            if (
                index > 0
                && Math.Abs(leftValue - rightValue) > StellateBlockAreaConstants.ValueEpsilon
            )
                result.Add(
                    StellateBlockAreaEventConverter.CreateEvent(
                        sample.Beat,
                        sample.Beat,
                        leftValue,
                        rightValue
                    )
                );

            if (index + 1 >= samples.Count)
                continue;

            var next = samples[index + 1];
            var endValue = xAxis ? next.LeftX : next.LeftY;
            if (
                next.Beat > sample.Beat
                && Math.Abs(rightValue - endValue) > StellateBlockAreaConstants.ValueEpsilon
            )
                result.Add(
                    StellateBlockAreaEventConverter.CreateEvent(
                        sample.Beat,
                        next.Beat,
                        rightValue,
                        endValue
                    )
                );
        }

        return result;
    }

    private static List<IrEvent> PrepareSamplingEvents(
        List<IrEvent>? source,
        int blockAreaIndex,
        string trackName,
        CancellationToken cancellationToken
    ) =>
        source is not { Count: > 0 }
            ? []
            : StellateBlockAreaEventConverter.ValidateAndSort(
                source,
                blockAreaIndex,
                trackName,
                evt =>
                    StellateBlockAreaEventConverter.ValidateEventFields(
                        evt,
                        blockAreaIndex,
                        trackName
                    ),
                thenByEndBeat: false,
                cancellationToken
            );

    private static double EvaluateAnchor(
        IReadOnlyList<IrEvent> events,
        Beat beat,
        double baseValue,
        bool includeStartAtBeat
    ) => events.Count == 0 ? baseValue : EvaluateTrack(events, beat, baseValue, includeStartAtBeat);

    private static double EvaluateTrack(
        IReadOnlyList<IrEvent> events,
        Beat beat,
        double defaultValue,
        bool includeStartAtBeat
    )
    {
        var low = 0;
        var high = events.Count - 1;
        var dominantIndex = -1;
        while (low <= high)
        {
            var middle = low + (high - low) / 2;
            var startsBeforeBeat = includeStartAtBeat
                ? events[middle].StartBeat <= beat
                : events[middle].StartBeat < beat;
            if (startsBeforeBeat)
            {
                dominantIndex = middle;
                low = middle + 1;
            }
            else
            {
                high = middle - 1;
            }
        }

        if (dominantIndex < 0)
            return defaultValue;

        var dominant = events[dominantIndex];
        return beat <= dominant.EndBeat ? dominant.GetValueAtBeatAsDouble(beat) : dominant.EndValue;
    }
}
