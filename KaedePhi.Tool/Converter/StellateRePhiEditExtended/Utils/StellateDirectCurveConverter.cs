using KaedePhi.Core.Primitives;
using IrEvent = KaedePhi.Core.Intermediate.Model.Events.Event<double>;
using RpeEasingConverter = KaedePhi.Tool.Converter.RePhiEdit.Utils.EasingConverter;
using RpeEvent = KaedePhi.Core.Formats.RePhiEdit.Model.Events.Event<float>;
using RpeEventBuilder = KaedePhi.Tool.Converter.RePhiEdit.Utils.EventBuilder;
using RpeEventLayer = KaedePhi.Core.Formats.RePhiEdit.Model.Events.EventLayer;
using RpeJudgeLine = KaedePhi.Core.Formats.RePhiEdit.Model.JudgeLine;

namespace KaedePhi.Tool.Converter.StellateRePhiEditExtended.Utils;

/// <summary>
/// 提供噪域变换曲线的筛选、验证和直接转换功能。
/// </summary>
public static class StellateDirectCurveConverter
{
    /// <summary>
    /// 将可直接保留的缩放曲线转换为 IR 事件。
    /// </summary>
    /// <param name="events">源缩放事件</param>
    /// <param name="baseBeat">用于计算基准缩放的拍数</param>
    /// <param name="canonicalBaseSize">规范化基准尺寸</param>
    /// <param name="geometrySampler">用于计算曲线值的几何采样器</param>
    /// <param name="converted">转换成功时接收 IR 事件</param>
    public static bool TryConvertDirectScale(
        List<RpeEvent>? events,
        Beat baseBeat,
        double canonicalBaseSize,
        StellateGeometrySampler geometrySampler,
        out List<IrEvent>? converted
    )
    {
        converted = null;
        if (events is not { Count: > 0 })
            return true;
        if (!CanConvertDirectScale(events))
            return false;

        var baseScale = StellateGeometryConstants.FiniteOr(
            geometrySampler.EvaluateCurveAtBeat(
                events,
                baseBeat,
                rightSide: true,
                defaultValue: 1d
            ),
            1d
        );
        var divisor =
            Math.Abs(baseScale) * canonicalBaseSize > StellateGeometryConstants.MinimumBaseSize
                ? Math.Abs(baseScale)
                : 1d;
        return TryConvertDirectCurve(events, value => Math.Abs(value) / divisor, 1d, out converted);
    }

    /// <summary>
    /// 将可直接保留的曲线事件转换为 IR 事件。
    /// </summary>
    /// <param name="events">源曲线事件</param>
    /// <param name="valueTransform">用于转换事件值的函数</param>
    /// <param name="initialValue">转换曲线的初始值</param>
    /// <param name="converted">转换成功时接收 IR 事件</param>
    public static bool TryConvertDirectCurve(
        List<RpeEvent>? events,
        Func<float, double> valueTransform,
        double initialValue,
        out List<IrEvent>? converted
    )
    {
        converted = null;
        if (events is not { Count: > 0 })
            return true;
        if (!CanConvertDirectCurve(events))
            return false;

        var result = OrderEvents(events).ConvertAll(evt => ConvertDirectEvent(evt, valueTransform));
        if (
            result.Any(evt =>
                Math.Abs(evt.StartValue - initialValue) > StellateGeometryConstants.ValueEpsilon
                || Math.Abs(evt.EndValue - initialValue) > StellateGeometryConstants.ValueEpsilon
            )
        )
            converted = result;
        return true;
    }

    /// <summary>
    /// 判断所有事件是否能保留为原生噪域缓动。
    /// </summary>
    /// <param name="events">待检查的曲线事件</param>
    public static bool CanPreserveNativeEasings(IEnumerable<RpeEvent> events) =>
        events.All(evt =>
            !StellateRePhiEditEasing.IsBezierEvent(evt)
            && IsFullEasingRange(evt)
            && StellateRePhiEditEasing.TryMapToBlockArea((int)evt.Easing, out _)
        );

    /// <summary>
    /// 判断事件的缓动范围是否覆盖完整区间。
    /// </summary>
    /// <param name="evt">待检查的曲线事件</param>
    public static bool IsFullEasingRange(RpeEvent evt) =>
        Math.Abs(evt.EasingLeft) <= StellateGeometryConstants.ValueEpsilon
        && Math.Abs(evt.EasingRight - 1f) <= StellateGeometryConstants.ValueEpsilon;

    /// <summary>
    /// 判断曲线能否直接转换为 IR 事件。
    /// </summary>
    /// <param name="events">待检查的曲线事件</param>
    public static bool CanConvertDirectCurve(List<RpeEvent>? events)
    {
        if (events is not { Count: > 0 })
            return true;
        if (!CanPreserveNativeEasings(events))
            return false;

        var ordered = OrderEvents(events);
        if (ordered[0].StartBeat > StellateBlockAreaConstants.ZeroBeat)
            return false;

        for (var index = 0; index < ordered.Count; index++)
        {
            var current = ordered[index];
            if (current.StartBeat > current.EndBeat)
                return false;
            if (index > 0 && current.StartBeat < ordered[index - 1].EndBeat)
                return false;
        }

        return true;
    }

    /// <summary>
    /// 判断缩放曲线能否直接转换为非负尺寸事件。
    /// </summary>
    /// <param name="events">待检查的缩放事件</param>
    public static bool CanConvertDirectScale(List<RpeEvent>? events) =>
        CanConvertDirectCurve(events)
        && (
            events is not { Count: > 0 }
            || !events.Any(evt => evt.StartBeat < evt.EndBeat && evt.StartValue * evt.EndValue < 0f)
        );

    /// <summary>
    /// 移除被后续同值常量事件覆盖的冗余事件。
    /// </summary>
    /// <param name="events">待处理的曲线事件</param>
    public static List<RpeEvent>? RemoveRedundantConstantEvents(List<RpeEvent>? events)
    {
        if (events is not { Count: > 1 })
            return events;

        var ordered = OrderEvents(events);
        var result = new List<RpeEvent>(ordered.Count);
        for (var index = 0; index < ordered.Count; index++)
        {
            var current = ordered[index];
            var isRedundantConstant =
                Math.Abs(current.StartValue - current.EndValue)
                    <= StellateGeometryConstants.ValueEpsilon
                && ordered
                    .Skip(index + 1)
                    .Any(later =>
                        later.StartBeat == current.StartBeat
                        && later.EndBeat >= current.EndBeat
                        && Math.Abs(later.StartValue - current.StartValue)
                            <= StellateGeometryConstants.ValueEpsilon
                        && Math.Abs(later.EndValue - current.EndValue)
                            <= StellateGeometryConstants.ValueEpsilon
                    );
            if (!isRedundantConstant)
                result.Add(current);
        }

        return result;
    }

    /// <summary>
    /// 判断多条曲线是否在其他曲线的事件区间内存在边界。
    /// </summary>
    /// <param name="curves">待检查的曲线列表</param>
    public static bool HasInteriorCurveBoundary(params List<RpeEvent>?[] curves)
    {
        var events = curves
            .Where(curve => curve is { Count: > 0 })
            .SelectMany(curve => curve!)
            .ToList();
        if (events.Count == 0)
            return false;

        var boundaries = new SortedSet<Beat> { StellateBlockAreaConstants.ZeroBeat };
        foreach (var evt in events)
        {
            boundaries.Add(evt.StartBeat);
            boundaries.Add(evt.EndBeat);
        }

        return events.Any(evt =>
            evt.StartBeat < evt.EndBeat
            && StellateBlockAreaPathBuilder.HasInteriorBoundary(
                boundaries,
                evt.StartBeat,
                evt.EndBeat
            )
        );
    }

    /// <summary>
    /// 判断判定线是否使用居中锚点。
    /// </summary>
    /// <param name="line">待检查的判定线</param>
    public static bool IsCenteredAnchor(RpeJudgeLine line) =>
        line.Anchor is { Length: >= 2 }
        && Math.Abs(line.Anchor[0] - 0.5f) <= StellateGeometryConstants.ValueEpsilon
        && Math.Abs(line.Anchor[1] - 0.5f) <= StellateGeometryConstants.ValueEpsilon;

    /// <summary>
    /// 获取判定线中符合条件的非空曲线层数量。
    /// </summary>
    /// <param name="line">待检查的判定线</param>
    /// <param name="selector">选择事件列表的函数</param>
    public static int NonEmptyLayerCount(
        RpeJudgeLine line,
        Func<RpeEventLayer, List<RpeEvent>?> selector
    ) => line.EventLayers.Count(layer => layer is not null && selector(layer) is { Count: > 0 });

    /// <summary>
    /// 获取判定线中首条符合条件的非空曲线。
    /// </summary>
    /// <param name="line">待检查的判定线</param>
    /// <param name="selector">选择事件列表的函数</param>
    public static List<RpeEvent>? GetSingleCurve(
        RpeJudgeLine line,
        Func<RpeEventLayer, List<RpeEvent>?> selector
    ) =>
        line
            .EventLayers.Where(layer => layer is not null)
            .Select(selector)
            .FirstOrDefault(events => events is { Count: > 0 });

    /// <summary>
    /// 获取判定线的直接旋转曲线，并标记是否存在多条有效旋转曲线。
    /// </summary>
    /// <param name="line">待检查的判定线</param>
    /// <param name="hasMultipleRotationCurves">存在多条有效旋转曲线时为 <see langword="true"/></param>
    public static List<RpeEvent>? GetDirectRotationCurve(
        RpeJudgeLine line,
        out bool hasMultipleRotationCurves
    )
    {
        hasMultipleRotationCurves = false;
        List<RpeEvent>? result = null;
        foreach (var events in line.EventLayers.Select(layer => layer?.RotateEvents))
        {
            if (events is not { Count: > 0 } || IsNeutralRotationCurve(events))
                continue;
            if (result is not null)
            {
                hasMultipleRotationCurves = true;
                return null;
            }

            result = events;
        }

        return result;
    }

    /// <summary>
    /// 判断旋转曲线的所有事件是否均为零旋转。
    /// </summary>
    /// <param name="events">待检查的旋转事件</param>
    public static bool IsNeutralRotationCurve(IEnumerable<RpeEvent> events) =>
        events.All(evt =>
            Math.Abs(evt.StartValue) <= StellateGeometryConstants.ValueEpsilon
            && Math.Abs(evt.EndValue) <= StellateGeometryConstants.ValueEpsilon
        );

    // 按开始拍、结束拍排序；排序是稳定的，区间相同的事件保持原有顺序
    private static List<RpeEvent> OrderEvents(IEnumerable<RpeEvent> events) =>
        events.OrderBy(evt => evt.StartBeat).ThenBy(evt => evt.EndBeat).ToList();

    private static IrEvent ConvertDirectEvent(RpeEvent evt, Func<float, double> valueTransform)
    {
        if (evt.BezierPoints is null)
            evt = CopyWithEmptyBezierPoints(evt);

        var converted = RpeEventBuilder.ConvertFloatToDoubleEvent(evt, valueTransform);
        converted.IsBezier = false;
        converted.EasingLeft = 0f;
        converted.EasingRight = 1f;
        converted.Easing = new Ir.Easing(
            RpeEasingConverter.MapToIr(StellateRePhiEditEasing.NormalizeType((int)evt.Easing))
        );
        return converted;
    }

    private static RpeEvent CopyWithEmptyBezierPoints(RpeEvent evt) =>
        new()
        {
            IsBezier = false,
            BezierPoints = [],
            Easing = evt.Easing,
            EasingLeft = evt.EasingLeft,
            EasingRight = evt.EasingRight,
            StartValue = evt.StartValue,
            EndValue = evt.EndValue,
            StartBeat = new Beat((int[])evt.StartBeat),
            EndBeat = new Beat((int[])evt.EndBeat),
            Font = evt.Font,
        };
}
