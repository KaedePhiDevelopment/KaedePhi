using KaedePhi.Core.Primitives;
using EventLayer = KaedePhi.Core.Intermediate.Model.Events.EventLayer;

namespace KaedePhi.Tool.JudgeLine.Intermediate.Utils;

/// <summary>
/// 提供解绑用的事件取值、通道合并和范围统计。
/// </summary>
internal static class JudgeLineUnbindEvents
{
    /// <summary>
    /// 获取指定拍点上正在生效的事件插值。
    /// </summary>
    internal static double GetValIn(List<IrEvents.Event<double>> events, Beat beat)
    {
        if (events.Count == 0)
            return 0f;
        int lo = 0,
            hi = events.Count - 1,
            idx = -1;
        while (lo <= hi)
        {
            var mid = (lo + hi) >> 1;
            if (events[mid].StartBeat <= beat)
            {
                idx = mid;
                lo = mid + 1;
            }
            else
                hi = mid - 1;
        }

        if (idx < 0)
            return 0f;
        var e = events[idx];
        return e.EndBeat > beat ? e.GetValueAtBeat(beat) : e.EndValue;
    }

    /// <summary>
    /// 获取指定拍点上即将结束的事件插值。
    /// </summary>
    internal static double GetValOut(List<IrEvents.Event<double>> events, Beat beat)
    {
        if (events.Count == 0)
            return 0f;
        int lo = 0,
            hi = events.Count - 1,
            idx = -1;
        while (lo <= hi)
        {
            var mid = (lo + hi) >> 1;
            if (events[mid].StartBeat < beat)
            {
                idx = mid;
                lo = mid + 1;
            }
            else
                hi = mid - 1;
        }

        if (idx < 0)
            return 0f;
        var e = events[idx];
        return e.EndBeat >= beat ? e.GetValueAtBeat(beat) : e.EndValue;
    }

    /// <summary>
    /// 按层顺序将某一类型的事件列表串行叠加合并。
    /// </summary>
    internal static List<IrEvents.Event<double>> MergeLayerChannel(
        List<EventLayer> layers,
        Func<EventLayer, List<IrEvents.Event<double>>?> selector,
        Func<
            List<IrEvents.Event<double>>,
            List<IrEvents.Event<double>>,
            List<IrEvents.Event<double>>
        > merge
    )
    {
        var result = new List<IrEvents.Event<double>>();
        return layers
            .Select(selector)
            .Where(ch => ch is { Count: > 0 })
            .Aggregate(
                result,
                (current, ch) =>
                    merge(
                        current,
                        ch ?? throw new InvalidOperationException("Channel selector returned null")
                    )
            );
    }

    /// <summary>
    /// 获取事件列表的拍范围；列表为空时返回 (0, 0)。
    /// </summary>
    internal static (Beat Min, Beat Max) GetEventRange(List<IrEvents.Event<double>> events) =>
        events.Count == 0
            ? (Beat.Zero, Beat.Zero)
            : (events.Min(e => e.StartBeat), events.Max(e => e.EndBeat));

    /// <summary>
    /// 合并父子线五个通道事件并拼装为解绑采样输入。
    /// </summary>
    internal static JudgeLineUnbindEventChannels MergeChannels(
        List<EventLayer> targetLayers,
        List<EventLayer> fatherLayers,
        Func<
            List<IrEvents.Event<double>>,
            List<IrEvents.Event<double>>,
            List<IrEvents.Event<double>>
        > merge
    ) =>
        new(
            Fx: MergeLayerChannel(fatherLayers, layer => layer.MoveXEvents, merge),
            Fy: MergeLayerChannel(fatherLayers, layer => layer.MoveYEvents, merge),
            Fr: MergeLayerChannel(fatherLayers, layer => layer.RotateEvents, merge),
            Tx: MergeLayerChannel(targetLayers, layer => layer.MoveXEvents, merge),
            Ty: MergeLayerChannel(targetLayers, layer => layer.MoveYEvents, merge)
        );
}
