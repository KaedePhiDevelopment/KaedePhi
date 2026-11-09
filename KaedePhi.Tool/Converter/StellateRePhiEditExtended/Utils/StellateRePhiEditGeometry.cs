using KaedePhi.Core.Primitives;
using KaedePhi.Tool.Common;
using KaedePhi.Tool.Converter.Phigros.v3.Utils;
using KaedePhi.Tool.Converter.StellateRePhiEditExtended.Model;
using static KaedePhi.Tool.Converter.StellateRePhiEditExtended.Utils.StellateDirectCurveConverter;
using static KaedePhi.Tool.Converter.StellateRePhiEditExtended.Utils.StellateGeometryConstants;
using GeometrySample = (
    KaedePhi.Core.Primitives.Beat Beat,
    KaedePhi.Tool.Converter.StellateRePhiEditExtended.Model.StellateBlockAreaGeometry Left,
    KaedePhi.Tool.Converter.StellateRePhiEditExtended.Model.StellateBlockAreaGeometry Right,
    bool Hard
);
using IrBpmItem = KaedePhi.Core.Intermediate.Model.BpmItem;
using IrEvent = KaedePhi.Core.Intermediate.Model.Events.Event<double>;
using RpeChart = KaedePhi.Core.Formats.RePhiEdit.Model.Chart;
using RpeEvent = KaedePhi.Core.Formats.RePhiEdit.Model.Events.Event<float>;
using RpeTransform = KaedePhi.Tool.Converter.RePhiEdit.Utils.Transform;

namespace KaedePhi.Tool.Converter.StellateRePhiEditExtended.Utils;

/// <summary>
/// 将 RePhiEdit 标记判定线转换为 IR 噪域几何。
/// </summary>
public sealed class StellateRePhiEditGeometry
{
    private readonly RpeChart _source;
    private readonly CancellationToken _ct;
    private readonly Beat _chartEndBeat;
    private readonly StellateGeometrySampler _sampler;
    private readonly StellateBlockAreaPathBuilder _pathBuilder;

    /// <summary>
    /// 根据源谱面和 BPM 列表创建几何转换器。
    /// </summary>
    /// <param name="source">包含噪域标记判定线的 RePhiEdit 谱面</param>
    /// <param name="bpmList">用于将拍数转换为音乐时间的 BPM 列表</param>
    /// <param name="cancellationToken">用于取消几何转换的令牌</param>
    public StellateRePhiEditGeometry(
        RpeChart source,
        IReadOnlyList<IrBpmItem> bpmList,
        CancellationToken cancellationToken
    )
        : this(source, new PhigrosV3TimeMapper(bpmList), cancellationToken) { }

    internal StellateRePhiEditGeometry(
        RpeChart source,
        PhigrosV3TimeMapper timeMapper,
        CancellationToken cancellationToken
    )
    {
        _source = source;
        _ct = cancellationToken;
        _chartEndBeat = StellateBlockAreaLifecycle.CalculateChartEndBeat(source);
        _sampler = new StellateGeometrySampler(source, timeMapper, cancellationToken);
        _pathBuilder = new StellateBlockAreaPathBuilder(timeMapper, cancellationToken);
    }

    /// <summary>
    /// 为指定判定线及其父级准备曲线索引。
    /// </summary>
    /// <param name="lineIndex">判定线索引</param>
    public void PrepareLine(int lineIndex) => _sampler.PrepareLine(lineIndex);

    /// <summary>
    /// 将指定判定线转换为零个或多个 IR 噪域。
    /// </summary>
    /// <param name="lineIndex">源判定线索引</param>
    /// <param name="maxDegreeOfParallelism">采样使用的最大并行度，零表示使用处理器数量</param>
    /// <returns>转换后的 IR 噪域列表</returns>
    public List<Ir.BlockArea> ConvertLine(int lineIndex, int maxDegreeOfParallelism = 0)
    {
        _ct.ThrowIfCancellationRequested();
        var source = _source.JudgeLineList[lineIndex];
        var markers = StellateBlockAreaLifecycle.GetMarkers(source, _ct);
        var lifeCycles = StellateBlockAreaLifecycle.BuildLifeCycles(markers, _chartEndBeat, _ct);
        if (lifeCycles.Count == 0)
            return [];

        _sampler.PrepareLine(lineIndex);
        var endBeat = markers.Aggregate(
            _chartEndBeat,
            (end, marker) => marker.Beat > end ? marker.Beat : end
        );

        var moveXSource = GetSingleCurve(source, static layer => layer.MoveXEvents);
        var moveYSource = GetSingleCurve(source, static layer => layer.MoveYEvents);
        var rotateSource = GetDirectRotationCurve(source, out var hasMultipleRotationCurves);
        var rotateDirectSource = RemoveRedundantConstantEvents(rotateSource);
        var scaleXSource = source.Extended?.ScaleXEvents;
        var scaleYSource = source.Extended?.ScaleYEvents;

        // 满足直通条件的曲线直接保留原生缓动，其余曲线通过几何采样重建
        var isDirectMoveShape =
            source.Father < 0
            && IsCenteredAnchor(source)
            && NonEmptyLayerCount(source, static layer => layer.MoveXEvents) <= 1
            && NonEmptyLayerCount(source, static layer => layer.MoveYEvents) <= 1;
        var canUseDirectMove =
            isDirectMoveShape
            && CanConvertDirectCurve(moveXSource)
            && CanConvertDirectCurve(moveYSource)
            && !HasInteriorCurveBoundary(moveXSource, moveYSource);
        var isDirectRotateShape =
            !(source.Father >= 0 && source.RotateWithFather) && !hasMultipleRotationCurves;
        var canUseDirectRotate =
            isDirectRotateShape
            && CanConvertDirectCurve(rotateDirectSource)
            && !HasInteriorCurveBoundary(rotateDirectSource);
        var canUseDirectScale =
            CanConvertDirectScale(scaleXSource)
            && CanConvertDirectScale(scaleYSource)
            && !HasInteriorCurveBoundary(scaleXSource, scaleYSource);

        // 去重时保留先加入的拍数表示，因此依次加入位置旋转边界和缩放边界
        var hardBeats = StellateBlockAreaPathBuilder.CreateCurveBoundarySet();
        _sampler.CollectPoseBreaks(lineIndex, hardBeats, []);
        StellateBlockAreaPathBuilder.AddCurveBoundaries(scaleXSource, hardBeats);
        StellateBlockAreaPathBuilder.AddCurveBoundaries(scaleYSource, hardBeats);
        var scaleHardBeats = StellateBlockAreaPathBuilder.CreateCurveBoundarySet(
            scaleXSource,
            scaleYSource
        );
        var rotateHardBeats = StellateBlockAreaPathBuilder.CreateCurveBoundarySet();
        _sampler.CollectRotationBreaks(lineIndex, rotateHardBeats, []);

        var sampleBeats = new SortedSet<Beat>(hardBeats);
        if (isDirectMoveShape && !canUseDirectMove)
            _pathBuilder.AddMovementCutPoints(sampleBeats, moveXSource, moveYSource);
        if (isDirectRotateShape && !canUseDirectRotate)
            _pathBuilder.AddRotationCutPoints(sampleBeats, rotateSource);
        _pathBuilder.AddScaleCutPoints(sampleBeats, scaleXSource, scaleYSource);
        _pathBuilder.AddUniformGrid(sampleBeats, endBeat);
        var sampleParallelism =
            maxDegreeOfParallelism > 0 ? maxDegreeOfParallelism : Environment.ProcessorCount;
        var samples = _sampler
            .SampleGeometry(lineIndex, sampleBeats, hardBeats, sampleParallelism)
            .ToList();

        var baseGeometry = _sampler.GetGeometryAtBeat(
            lineIndex,
            StellateBlockAreaConstants.ZeroBeat,
            rightSide: true
        );
        var baseWidth =
            baseGeometry.Width > MinimumBaseSize ? baseGeometry.Width : TextureWidth / RenderWidth;
        var baseHeight =
            baseGeometry.Height > MinimumBaseSize
                ? baseGeometry.Height
                : TextureHeight / RenderHeight;
        var baseCenterX = ToIrCoordinate(baseGeometry.CenterX);
        var baseCenterY = ToIrCoordinate(baseGeometry.CenterY);

        var (moveXEvents, moveYEvents) = canUseDirectMove
            ? (
                ConvertDirectCurve(
                    moveXSource,
                    RpeTransform.TransformToIrX,
                    baseCenterX,
                    ToolResources.stellate_track_move_x
                ),
                ConvertDirectCurve(
                    moveYSource,
                    RpeTransform.TransformToIrY,
                    baseCenterY,
                    ToolResources.stellate_track_move_y
                )
            )
            : BuildMoveEvents(samples, baseCenterX, baseCenterY);
        var rotateEvents = canUseDirectRotate
            ? ConvertDirectCurve(
                rotateDirectSource,
                RpeTransform.TransformToIrAngle,
                0d,
                ToolResources.stellate_track_rotate
            )
            : BuildRotateEvents(samples, rotateHardBeats);
        var (scaleXEvents, scaleYEvents) = canUseDirectScale
            ? (
                ConvertDirectScaleCurve(
                    scaleXSource,
                    TextureWidth / RenderWidth,
                    ToolResources.stellate_track_scale_x
                ),
                ConvertDirectScaleCurve(
                    scaleYSource,
                    TextureHeight * AspectWidth / (RenderWidth * AspectHeight),
                    ToolResources.stellate_track_scale_y
                )
            )
            : BuildScaleEvents(samples, scaleHardBeats, baseWidth, baseHeight);

        var geometryTemplate = new Ir.BlockArea
        {
            TopRightX = ToIrCoordinate(baseGeometry.CenterX + baseWidth / 2d),
            TopRightY = ToIrCoordinate(baseGeometry.CenterY + baseHeight / 2d),
            BottomLeftX = ToIrCoordinate(baseGeometry.CenterX - baseWidth / 2d),
            BottomLeftY = ToIrCoordinate(baseGeometry.CenterY - baseHeight / 2d),
            IsSubtract = StellateRePhiEditExtendedTexture.IsSubtractTexture(source.Texture),
            MoveXEvents = moveXEvents,
            MoveYEvents = moveYEvents,
            ScaleXEvents = scaleXEvents,
            ScaleYEvents = scaleYEvents,
            RotateEvents = rotateEvents,
        };

        var result = new List<Ir.BlockArea>(lifeCycles.Count);
        foreach (var lifeCycle in lifeCycles)
        {
            _ct.ThrowIfCancellationRequested();
            var area = geometryTemplate.Clone();
            area.AppearBeat = lifeCycle.AppearBeat;
            area.EnableBeat = lifeCycle.EnableBeat;
            area.DisableBeat = lifeCycle.DisableBeat;
            area.DisappearBeat = lifeCycle.DisappearBeat;
            result.Add(area);
        }

        return result;
    }

    private (List<IrEvent>? X, List<IrEvent>? Y) BuildMoveEvents(
        List<GeometrySample> samples,
        double baseCenterX,
        double baseCenterY
    )
    {
        var path = _pathBuilder
            .SimplifyVector(
                _pathBuilder.BuildVectorPath(
                    samples,
                    geometry => (geometry.CenterX, geometry.CenterY)
                )
            )
            .ConvertAll(point =>
                (point.Beat, ToIrCoordinate(point.X), ToIrCoordinate(point.Y), point.Hard)
            );
        return (
            StellateBlockAreaPathBuilder.CreateVectorEvents(path, baseCenterX, baseCenterY, true),
            StellateBlockAreaPathBuilder.CreateVectorEvents(path, baseCenterX, baseCenterY, false)
        );
    }

    private List<IrEvent>? BuildRotateEvents(
        List<GeometrySample> samples,
        SortedSet<Beat> rotateHardBeats
    )
    {
        var rotateSamples = WithHardBeats(samples, rotateHardBeats);
        var path = _pathBuilder.SimplifyScalar(
            _pathBuilder.BuildScalarPath(rotateSamples, geometry => geometry.Rotation)
        );
        return StellateBlockAreaPathBuilder.CreateScalarEvents(path, 0d);
    }

    private (List<IrEvent>? X, List<IrEvent>? Y) BuildScaleEvents(
        List<GeometrySample> samples,
        SortedSet<Beat> scaleHardBeats,
        double baseWidth,
        double baseHeight
    )
    {
        var scaleSamples = WithHardBeats(samples, scaleHardBeats);
        var path = _pathBuilder.SimplifyVector(
            _pathBuilder.BuildVectorPath(
                scaleSamples,
                geometry => (geometry.Width / baseWidth, geometry.Height / baseHeight)
            )
        );
        return (
            StellateBlockAreaPathBuilder.CreateVectorEvents(path, 1d, 1d, true),
            StellateBlockAreaPathBuilder.CreateVectorEvents(path, 1d, 1d, false)
        );
    }

    // 各轨道的硬边界不同，需要按轨道重新标记采样点
    private static List<GeometrySample> WithHardBeats(
        List<GeometrySample> samples,
        SortedSet<Beat> hardBeats
    ) => samples.ConvertAll(sample => sample with { Hard = hardBeats.Contains(sample.Beat) });

    private static List<IrEvent>? ConvertDirectCurve(
        List<RpeEvent>? events,
        Func<float, double> valueTransform,
        double initialValue,
        string trackName
    ) =>
        TryConvertDirectCurve(events, valueTransform, initialValue, out var converted)
            ? converted
            : throw StellateExceptions.DirectCurveConvertFailed(trackName);

    private List<IrEvent>? ConvertDirectScaleCurve(
        List<RpeEvent>? events,
        double canonicalBaseSize,
        string trackName
    ) =>
        TryConvertDirectScale(
            events,
            StellateBlockAreaConstants.ZeroBeat,
            canonicalBaseSize,
            _sampler,
            out var converted
        )
            ? converted
            : throw StellateExceptions.DirectCurveConvertFailed(trackName);
}
