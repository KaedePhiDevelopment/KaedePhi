using KaedePhi.Core.Primitives;
using KaedePhi.Tool.Converter.Phigros.v3.Utils;
using KaedePhi.Tool.Converter.StellateRePhiEditExtended.Model;
using GeometrySample = (
    KaedePhi.Core.Primitives.Beat Beat,
    KaedePhi.Tool.Converter.StellateRePhiEditExtended.Model.StellateBlockAreaGeometry Left,
    KaedePhi.Tool.Converter.StellateRePhiEditExtended.Model.StellateBlockAreaGeometry Right,
    bool Hard
);
using IrBpmItem = KaedePhi.Core.Intermediate.Model.BpmItem;
using IrEvent = KaedePhi.Core.Intermediate.Model.Events.Event<double>;
using PhigrosAreaEaseType = KaedePhi.Core.Formats.Phigros.v3.Model.AreaEaseType;
using RpeEvent = KaedePhi.Core.Formats.RePhiEdit.Model.Events.Event<float>;

namespace KaedePhi.Tool.Converter.StellateRePhiEditExtended.Utils;

/// <summary>
/// 根据采样几何生成并简化噪域转换路径。
/// </summary>
public sealed class StellateBlockAreaPathBuilder
{
    private const double TimeEpsilon = 1e-12d;
    private const double SpanEpsilon = 1e-12d;

    private readonly PhigrosV3TimeMapper _timeMapper;
    private readonly CancellationToken _ct;

    /// <summary>
    /// 根据谱面的 BPM 列表创建噪域路径构建器。
    /// </summary>
    /// <param name="bpmList">用于将拍数转换为音乐时间的 BPM 列表</param>
    /// <param name="cancellationToken">用于取消路径生成的令牌</param>
    /// <remarks>对简化曲线而言，BPM 列表会影响拍数到音乐时间的映射。</remarks>
    public StellateBlockAreaPathBuilder(
        IReadOnlyList<IrBpmItem> bpmList,
        CancellationToken cancellationToken
    )
        : this(new PhigrosV3TimeMapper(bpmList), cancellationToken) { }

    internal StellateBlockAreaPathBuilder(
        PhigrosV3TimeMapper timeMapper,
        CancellationToken cancellationToken
    )
    {
        _timeMapper = timeMapper;
        _ct = cancellationToken;
    }

    #region 采样点收集

    /// <summary>
    /// 将事件的开始拍和结束拍加入采样边界集合。
    /// </summary>
    /// <param name="events">待处理的曲线事件</param>
    /// <param name="beats">接收事件边界的拍数集合</param>
    public static void AddCurveBoundaries(List<RpeEvent>? events, SortedSet<Beat> beats)
    {
        if (events is null)
            return;
        foreach (var evt in events)
        {
            beats.Add(evt.StartBeat);
            beats.Add(evt.EndBeat);
        }
    }

    /// <summary>
    /// 为移动曲线补充采样切分点。
    /// </summary>
    /// <param name="sampleBeats">接收采样拍数的集合</param>
    /// <param name="moveXEvents">横向移动事件</param>
    /// <param name="moveYEvents">纵向移动事件</param>
    public void AddMovementCutPoints(
        SortedSet<Beat> sampleBeats,
        List<RpeEvent>? moveXEvents,
        List<RpeEvent>? moveYEvents
    )
    {
        var times = CreateCurveBoundarySet(moveXEvents, moveYEvents);
        for (var pass = 0; pass < 2; pass++)
        {
            AddCurveCutPoints(times, moveXEvents, splitSupportedIfInterrupted: true);
            AddCurveCutPoints(times, moveYEvents, splitSupportedIfInterrupted: true);
        }

        sampleBeats.UnionWith(times);
    }

    /// <summary>
    /// 为旋转曲线补充采样切分点。
    /// </summary>
    /// <param name="sampleBeats">接收采样拍数的集合</param>
    /// <param name="rotateEvents">旋转事件</param>
    public void AddRotationCutPoints(SortedSet<Beat> sampleBeats, List<RpeEvent>? rotateEvents)
    {
        var times = CreateCurveBoundarySet(rotateEvents);
        AddCurveCutPoints(times, rotateEvents, splitSupportedIfInterrupted: false);
        sampleBeats.UnionWith(times);
    }

    /// <summary>
    /// 为缩放曲线补充采样切分点。
    /// </summary>
    /// <param name="sampleBeats">接收采样拍数的集合</param>
    /// <param name="scaleXEvents">横向缩放事件</param>
    /// <param name="scaleYEvents">纵向缩放事件</param>
    public void AddScaleCutPoints(
        SortedSet<Beat> sampleBeats,
        List<RpeEvent>? scaleXEvents,
        List<RpeEvent>? scaleYEvents
    )
    {
        var times = CreateCurveBoundarySet(scaleXEvents, scaleYEvents);
        for (var pass = 0; pass < 2; pass++)
        {
            AddCurveCutPoints(times, scaleXEvents, splitSupportedIfInterrupted: true);
            AddScaleZeroCrossingCutPoints(times, scaleXEvents);
            AddCurveCutPoints(times, scaleYEvents, splitSupportedIfInterrupted: true);
            AddScaleZeroCrossingCutPoints(times, scaleYEvents);
        }

        sampleBeats.UnionWith(times);
    }

    /// <summary>
    /// 判断拍数区间内是否存在其他采样边界。
    /// </summary>
    /// <param name="times">已收集的拍数边界</param>
    /// <param name="startBeat">区间起始拍</param>
    /// <param name="endBeat">区间结束拍</param>
    public static bool HasInteriorBoundary(SortedSet<Beat> times, Beat startBeat, Beat endBeat) =>
        startBeat < endBeat
        && times.GetViewBetween(startBeat, endBeat).Any(beat => startBeat < beat && beat < endBeat);

    /// <summary>
    /// 将均匀采样网格和结束拍加入采样集合。
    /// </summary>
    /// <param name="beats">接收采样拍数的集合</param>
    /// <param name="endBeat">采样结束拍</param>
    public void AddUniformGrid(SortedSet<Beat> beats, Beat endBeat)
    {
        var count = (long)
            Math.Floor(Math.Max(0d, endBeat) * StellateGeometryConstants.SamplesPerBeat + 1e-9);
        for (long index = 0; index <= count; index++)
        {
            _ct.ThrowIfCancellationRequested();
            beats.Add(GridBeat(index));
        }

        beats.Add(endBeat);
    }

    internal static SortedSet<Beat> CreateCurveBoundarySet(params List<RpeEvent>?[] curves)
    {
        var times = new SortedSet<Beat> { StellateBlockAreaConstants.ZeroBeat };
        foreach (var curve in curves)
            AddCurveBoundaries(curve, times);
        return times;
    }

    private void AddCurveCutPoints(
        SortedSet<Beat> times,
        List<RpeEvent>? events,
        bool splitSupportedIfInterrupted
    )
    {
        if (events is not { Count: > 0 })
            return;

        foreach (var evt in events)
        {
            _ct.ThrowIfCancellationRequested();
            if (evt.StartBeat >= evt.EndBeat)
                continue;

            var isMapped = CanMapNativeEasing(evt, out var areaEaseType);
            if (
                !isMapped
                || (
                    splitSupportedIfInterrupted
                    && areaEaseType != (int)PhigrosAreaEaseType.Linear
                    && HasInteriorBoundary(times, evt.StartBeat, evt.EndBeat)
                )
            )
                AddUniformCutPoints(times, evt);
        }
    }

    private void AddScaleZeroCrossingCutPoints(SortedSet<Beat> times, List<RpeEvent>? events)
    {
        if (events is not { Count: > 0 })
            return;

        foreach (var evt in events)
        {
            _ct.ThrowIfCancellationRequested();
            if (evt.StartBeat < evt.EndBeat && (double)evt.StartValue * evt.EndValue < 0d)
                AddUniformCutPoints(times, evt);
        }
    }

    private void AddUniformCutPoints(SortedSet<Beat> times, RpeEvent evt)
    {
        var first =
            (long)Math.Floor((double)evt.StartBeat * StellateGeometryConstants.SamplesPerBeat) + 1L;
        var last =
            (long)Math.Ceiling((double)evt.EndBeat * StellateGeometryConstants.SamplesPerBeat) - 1L;
        for (var index = first; index <= last; index++)
        {
            _ct.ThrowIfCancellationRequested();
            var beat = GridBeat(index);
            if (evt.StartBeat < beat && beat < evt.EndBeat)
                times.Add(beat);
        }
    }

    private static Beat GridBeat(long index) =>
        new((double)index / StellateGeometryConstants.SamplesPerBeat);

    private static bool CanMapNativeEasing(RpeEvent evt, out int areaEaseType)
    {
        areaEaseType = -1;
        return !StellateRePhiEditEasing.IsBezierEvent(evt)
            && StellateDirectCurveConverter.IsFullEasingRange(evt)
            && StellateRePhiEditEasing.TryMapToBlockArea((int)evt.Easing, out areaEaseType);
    }

    #endregion

    #region 路径生成与简化

    /// <summary>
    /// 从采样几何生成二维路径点。
    /// </summary>
    /// <param name="samples">按拍数排列的左右侧几何采样结果</param>
    /// <param name="selector">从几何中提取二维坐标的函数</param>
    /// <returns>按拍数排列的二维路径点</returns>
    public List<(Beat Beat, double X, double Y, bool Hard)> BuildVectorPath(
        List<GeometrySample> samples,
        Func<StellateBlockAreaGeometry, (double X, double Y)> selector
    ) =>
        BuildPath(
            samples,
            selector,
            static (left, right) =>
                Math.Abs(left.X - right.X) > StellateGeometryConstants.ValueEpsilon
                || Math.Abs(left.Y - right.Y) > StellateGeometryConstants.ValueEpsilon,
            static (beat, value, hard) => (beat, value.X, value.Y, hard)
        );

    /// <summary>
    /// 从采样几何生成一维路径点。
    /// </summary>
    /// <param name="samples">按拍数排列的左右侧几何采样结果</param>
    /// <param name="selector">从几何中提取数值的函数</param>
    /// <returns>按拍数排列的一维路径点</returns>
    public List<(Beat Beat, double Value, bool Hard)> BuildScalarPath(
        List<GeometrySample> samples,
        Func<StellateBlockAreaGeometry, double> selector
    ) =>
        BuildPath(
            samples,
            selector,
            static (left, right) => Math.Abs(left - right) > StellateGeometryConstants.ValueEpsilon,
            static (beat, value, hard) => (beat, value, hard)
        );

    /// <summary>
    /// 按音乐时间简化二维路径，同时保留硬边界点。
    /// </summary>
    /// <param name="points">待简化的二维路径点</param>
    /// <returns>简化后的二维路径点</returns>
    public List<(Beat Beat, double X, double Y, bool Hard)> SimplifyVector(
        List<(Beat Beat, double X, double Y, bool Hard)> points
    )
    {
        if (points.Count <= 2)
            return points;

        var keep = SelectKeptPoints(
            points.ConvertAll(point => point.Beat),
            index => points[index].Hard,
            (start, end, index, progress) =>
                Math.Max(
                    RelativeDeviation(
                        points[index].X,
                        Lerp(points[start].X, points[end].X, progress),
                        points[start].X,
                        points[end].X
                    ),
                    RelativeDeviation(
                        points[index].Y,
                        Lerp(points[start].Y, points[end].Y, progress),
                        points[start].Y,
                        points[end].Y
                    )
                )
        );
        return points.Where((_, index) => keep[index]).ToList();
    }

    /// <summary>
    /// 按音乐时间简化一维路径，同时保留硬边界点。
    /// </summary>
    /// <param name="points">待简化的一维路径点</param>
    /// <returns>简化后的一维路径点</returns>
    public List<(Beat Beat, double Value, bool Hard)> SimplifyScalar(
        List<(Beat Beat, double Value, bool Hard)> points
    )
    {
        if (points.Count <= 2)
            return points;

        var keep = SelectKeptPoints(
            points.ConvertAll(point => point.Beat),
            index => points[index].Hard,
            (start, end, index, progress) =>
                RelativeDeviation(
                    points[index].Value,
                    Lerp(points[start].Value, points[end].Value, progress),
                    points[start].Value,
                    points[end].Value
                )
        );
        return points.Where((_, index) => keep[index]).ToList();
    }

    /// <summary>
    /// 将二维路径转换为 IR 数值事件。
    /// </summary>
    /// <param name="points">按拍数排列的二维路径点</param>
    /// <param name="initialX">横向初始值</param>
    /// <param name="initialY">纵向初始值</param>
    /// <param name="xAxis">为 <see langword="true"/> 时生成横向事件，否则生成纵向事件</param>
    public static List<IrEvent>? CreateVectorEvents(
        List<(Beat Beat, double X, double Y, bool Hard)> points,
        double initialX,
        double initialY,
        bool xAxis
    ) =>
        CreateScalarEvents(
            points.ConvertAll(point => (point.Beat, xAxis ? point.X : point.Y, point.Hard)),
            xAxis ? initialX : initialY
        );

    /// <summary>
    /// 将一维路径转换为 IR 数值事件。
    /// </summary>
    /// <param name="points">按拍数排列的一维路径点</param>
    /// <param name="initialValue">路径初始值</param>
    public static List<IrEvent>? CreateScalarEvents(
        List<(Beat Beat, double Value, bool Hard)> points,
        double initialValue
    )
    {
        var events = new List<IrEvent>();
        var previousBeat = StellateBlockAreaConstants.ZeroBeat;
        var previousValue = initialValue;
        foreach (var point in points)
        {
            if (point.Beat < previousBeat)
                continue;
            if (Math.Abs(point.Value - previousValue) > StellateGeometryConstants.ValueEpsilon)
                events.Add(
                    StellateBlockAreaEventConverter.CreateEvent(
                        previousBeat,
                        point.Beat,
                        previousValue,
                        point.Value
                    )
                );
            previousBeat = point.Beat;
            previousValue = point.Value;
        }

        return events.Count > 0 ? events : null;
    }

    // 每个采样点在左侧值之后追加右侧值，两侧不同时形成瞬时跳变
    private List<TPoint> BuildPath<TValue, TPoint>(
        List<GeometrySample> samples,
        Func<StellateBlockAreaGeometry, TValue> selector,
        Func<TValue, TValue, bool> differs,
        Func<Beat, TValue, bool, TPoint> createPoint
    )
    {
        var result = new List<TPoint>();
        for (var index = 0; index < samples.Count; index++)
        {
            _ct.ThrowIfCancellationRequested();
            var sample = samples[index];
            var left = selector(sample.Left);
            var right = selector(sample.Right);
            if (index > 0)
                result.Add(createPoint(sample.Beat, left, sample.Hard));
            if (index == 0 || differs(left, right))
                result.Add(createPoint(sample.Beat, right, index > 0 || sample.Hard));
        }

        return result;
    }

    // 类 Douglas-Peucker 简化：反复在偏差超过容差的最大偏差点处切分，返回保留的点
    private bool[] SelectKeptPoints(
        List<Beat> beats,
        Func<int, bool> isHard,
        Func<int, int, int, double, double> measureDeviation
    )
    {
        var times = beats.ConvertAll(_timeMapper.ToMusicTime);
        var keep = new bool[beats.Count];
        keep[0] = true;
        keep[^1] = true;
        for (var index = 1; index < beats.Count; index++)
        {
            if (!(times[index] <= times[index - 1] + TimeEpsilon))
                continue;
            keep[index - 1] = true;
            keep[index] = true;
        }

        var anchors = Enumerable.Range(0, keep.Length).Where(index => keep[index]).ToArray();
        var ranges = new Stack<(int Start, int End)>();
        for (var index = 0; index < anchors.Length - 1; index++)
            ranges.Push((anchors[index], anchors[index + 1]));

        while (ranges.TryPop(out var range))
        {
            var (startIndex, endIndex) = range;
            if (endIndex <= startIndex + 1 || times[endIndex] <= times[startIndex] + TimeEpsilon)
                continue;

            var largestDeviation = 1d;
            var splitIndex = -1;
            for (var index = startIndex + 1; index < endIndex; index++)
            {
                var progress =
                    (times[index] - times[startIndex]) / (times[endIndex] - times[startIndex]);
                var deviation = measureDeviation(startIndex, endIndex, index, progress);
                var normalizedDeviation = deviation / StellateGeometryConstants.SimplifyTolerance;
                if (isHard(index))
                    normalizedDeviation = Math.Max(
                        normalizedDeviation,
                        deviation / StellateGeometryConstants.HardPointEpsilon
                    );
                if (!(normalizedDeviation > largestDeviation))
                    continue;
                largestDeviation = normalizedDeviation;
                splitIndex = index;
            }

            if (splitIndex < 0)
                continue;
            keep[splitIndex] = true;
            ranges.Push((startIndex, splitIndex));
            ranges.Push((splitIndex, endIndex));
        }

        return keep;
    }

    private static double Lerp(double start, double end, double progress) =>
        start + (end - start) * progress;

    private static double RelativeDeviation(
        double actual,
        double expected,
        double start,
        double end
    )
    {
        var span = end - start;
        return Math.Abs(span) <= SpanEpsilon
            ? Math.Abs(actual - expected)
            : Math.Abs((actual - expected) / span);
    }

    #endregion
}
