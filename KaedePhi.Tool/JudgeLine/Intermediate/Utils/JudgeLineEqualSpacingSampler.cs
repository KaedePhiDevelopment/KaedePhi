using System.Collections.Concurrent;
using KaedePhi.Core.Primitives;

namespace KaedePhi.Tool.JudgeLine.Intermediate.Utils;

/// <summary>
/// 使用固定拍间隔生成解绑后的 X/Y 事件。
/// </summary>
internal static class JudgeLineEqualSpacingSampler
{
    /// <summary>
    /// 生成从最小拍到最大拍（不含）以指定步长递增的拍列表。
    /// </summary>
    internal static List<Beat> BuildBeatList(
        Beat min,
        Beat max,
        Beat step,
        CancellationToken ct = default
    )
    {
        var range = (double)(max - min);
        var stepSize = (double)step;
        var estimatedCount = stepSize > 0 ? (int)Math.Ceiling(range / stepSize) : 0;
        var beats = new List<Beat>(Math.Max(0, estimatedCount));
        for (var beat = min; beat < max; beat += step)
        {
            ct.ThrowIfCancellationRequested();
            beats.Add(beat);
        }

        return beats;
    }

    /// <summary>
    /// 对拍列表中的每个区间采样，并按原始拍序返回 X/Y 事件。
    /// </summary>
    internal static (List<IrEvents.Event<double>> X, List<IrEvents.Event<double>> Y) Sample(
        List<Beat> beats,
        Beat max,
        Beat step,
        JudgeLineUnbindEventChannels channels,
        CancellationToken ct = default
    )
    {
        var xBag = new ConcurrentBag<(int Index, IrEvents.Event<double> Event)>();
        var yBag = new ConcurrentBag<(int Index, IrEvents.Event<double> Event)>();

        Parallel.For(
            0,
            beats.Count,
            new ParallelOptions { CancellationToken = ct },
            index =>
            {
                ct.ThrowIfCancellationRequested();
                var beat = beats[index];
                var next = beat + step > max ? max : beat + step;
                var (xEvent, yEvent) = ComputeBeatSegment(beat, next, channels);
                xBag.Add((index, xEvent));
                yBag.Add((index, yEvent));
            }
        );

        return (
            xBag.OrderBy(item => item.Index).Select(item => item.Event).ToList(),
            yBag.OrderBy(item => item.Index).Select(item => item.Event).ToList()
        );
    }

    private static (IrEvents.Event<double> X, IrEvents.Event<double> Y) ComputeBeatSegment(
        Beat beat,
        Beat next,
        JudgeLineUnbindEventChannels channels
    )
    {
        var (startAbsX, startAbsY) = JudgeLineUnbindGeometry.GetLinePos(
            JudgeLineUnbindEvents.GetValIn(channels.Fx, beat),
            JudgeLineUnbindEvents.GetValIn(channels.Fy, beat),
            JudgeLineUnbindEvents.GetValIn(channels.Fr, beat),
            JudgeLineUnbindEvents.GetValIn(channels.Tx, beat),
            JudgeLineUnbindEvents.GetValIn(channels.Ty, beat)
        );
        var (endAbsX, endAbsY) = JudgeLineUnbindGeometry.GetLinePos(
            JudgeLineUnbindEvents.GetValOut(channels.Fx, next),
            JudgeLineUnbindEvents.GetValOut(channels.Fy, next),
            JudgeLineUnbindEvents.GetValOut(channels.Fr, next),
            JudgeLineUnbindEvents.GetValOut(channels.Tx, next),
            JudgeLineUnbindEvents.GetValOut(channels.Ty, next)
        );

        return (
            new IrEvents.Event<double>
            {
                StartBeat = beat,
                EndBeat = next,
                StartValue = startAbsX,
                EndValue = endAbsX,
            },
            new IrEvents.Event<double>
            {
                StartBeat = beat,
                EndBeat = next,
                StartValue = startAbsY,
                EndValue = endAbsY,
            }
        );
    }
}
