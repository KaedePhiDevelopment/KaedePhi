using KaedePhi.Tool.Common;
using KaedePhi.Tool.Converter.RePhiEdit.Model;
using static KaedePhi.Tool.Converter.StellateRePhiEditExtended.Utils.StellateGeometryConstants;
using IrBlockArea = KaedePhi.Core.Intermediate.Model.BlockArea;
using IrEvent = KaedePhi.Core.Intermediate.Model.Events.Event<double>;
using RpeEvent = KaedePhi.Core.Formats.RePhiEdit.Model.Events.Event<float>;
using RpeEventLayer = KaedePhi.Core.Formats.RePhiEdit.Model.Events.EventLayer;
using RpeExtendLayer = KaedePhi.Core.Formats.RePhiEdit.Model.Events.ExtendLayer;
using RpeJudgeLine = KaedePhi.Core.Formats.RePhiEdit.Model.JudgeLine;
using RpeTransform = KaedePhi.Tool.Converter.RePhiEdit.Utils.Transform;

namespace KaedePhi.Tool.Converter.StellateRePhiEditExtended.Utils;

/// <summary>
/// 将 IR 噪域构建为 StellateRePhiEditExtended 标记判定线。
/// </summary>
public static class StellateRePhiEditExtendedBlockAreaBuilder
{
    /// <summary>
    /// 将一个 IR 噪域转换为 RePhiEdit 标记判定线。
    /// </summary>
    /// <param name="source">待转换的 IR 噪域</param>
    /// <param name="index">噪域在谱面中的索引</param>
    /// <param name="options">事件切分选项</param>
    /// <param name="warnLogger">接收转换警告的回调</param>
    /// <param name="cancellationToken">用于取消转换的令牌</param>
    /// <returns>表示该噪域的 RePhiEdit 判定线</returns>
    public static RpeJudgeLine ConvertBlockArea(
        IrBlockArea source,
        int index,
        ConvertOption.CuttingOptions options,
        Action<string>? warnLogger,
        CancellationToken cancellationToken
    )
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(options);
        cancellationToken.ThrowIfCancellationRequested();

        ValidateCoordinates(source);
        ValidateLifecycle(source, index);

        var centerX = source.TopRightX / 2d + source.BottomLeftX / 2d;
        var centerY = source.TopRightY / 2d + source.BottomLeftY / 2d;
        var width = Math.Abs(source.TopRightX - source.BottomLeftX);
        var height = Math.Abs(source.TopRightY - source.BottomLeftY);
        if (!double.IsFinite(centerX) || !double.IsFinite(centerY))
            throw StellateExceptions.CenterNotConvertible(index);

        var baseScaleX = StellateBlockAreaEventConverter.ToRpeFloat(
            width * RenderWidth / (IrCoordinateSpan * TextureWidth),
            index,
            ToolResources.stellate_value_width
        );
        var baseScaleY = StellateBlockAreaEventConverter.ToRpeFloat(
            height * RenderHeight / (IrCoordinateSpan * TextureHeight),
            index,
            ToolResources.stellate_value_height
        );

        var moveXEvents = source.MoveXEvents;
        var moveYEvents = source.MoveYEvents;
        var moveDefaultX = centerX;
        var moveDefaultY = centerY;
        if (HasAnchorEvents(source))
        {
            var bakedMotion = StellateBlockAreaAnchorMotionBaker.BakeAnchorMotion(
                source,
                index,
                options.UnsupportedEasingPrecision,
                cancellationToken
            );
            moveXEvents = bakedMotion.MoveXEvents;
            moveYEvents = bakedMotion.MoveYEvents;
            moveDefaultX = bakedMotion.CenterX;
            moveDefaultY = bakedMotion.CenterY;
            warnLogger?.Invoke(
                string.Format(
                    ToolResources.stellate_warning_anchor_baked,
                    index,
                    options.UnsupportedEasingPrecision
                )
            );
        }

        List<RpeEvent>? ConvertTrack(
            List<IrEvent>? events,
            double defaultValue,
            Func<double, double> valueTransformer,
            string trackName
        ) =>
            StellateBlockAreaEventConverter.ConvertEvents(
                events,
                defaultValue,
                valueTransformer,
                options,
                index,
                trackName,
                ensureBaseline: true,
                cancellationToken
            );

        var eventLayer = new RpeEventLayer
        {
            MoveXEvents = ConvertTrack(
                moveXEvents,
                moveDefaultX,
                RpeTransform.TransformToRpeX,
                ToolResources.stellate_track_move_x
            ),
            MoveYEvents = ConvertTrack(
                moveYEvents,
                moveDefaultY,
                RpeTransform.TransformToRpeY,
                ToolResources.stellate_track_move_y
            ),
            RotateEvents = ConvertTrack(
                source.RotateEvents,
                0d,
                RpeTransform.TransformToRpeAngle,
                ToolResources.stellate_track_rotate
            ),
            AlphaEvents = StellateBlockAreaEventConverter.CreateAlphaEvents(source),
            SpeedEvents =
            [
                StellateBlockAreaEventConverter.CreateMarker(source.AppearBeat, 1),
                StellateBlockAreaEventConverter.CreateMarker(source.EnableBeat, 2),
                StellateBlockAreaEventConverter.CreateMarker(source.DisableBeat, 3),
                StellateBlockAreaEventConverter.CreateMarker(source.DisappearBeat, 4),
            ],
        };

        return new RpeJudgeLine
        {
            Name = $"BlockArea_{index + 1}",
            Texture = StellateRePhiEditExtendedTexture.GetMarkerTexture(source.IsSubtract),
            Anchor = [0.5f, 0.5f],
            Father = -1,
            RotateWithFather = false,
            EventLayers = [eventLayer],
            Extended = new RpeExtendLayer
            {
                ScaleXEvents = ConvertTrack(
                    source.ScaleXEvents,
                    1d,
                    value => baseScaleX * value,
                    ToolResources.stellate_track_scale_x
                ),
                ScaleYEvents = ConvertTrack(
                    source.ScaleYEvents,
                    1d,
                    value => baseScaleY * value,
                    ToolResources.stellate_track_scale_y
                ),
            },
        };
    }

    private static bool HasAnchorEvents(IrBlockArea source) =>
        StellateBlockAreaEventConverter.HasEvents(source.RotateAnchorXEvents)
        || StellateBlockAreaEventConverter.HasEvents(source.RotateAnchorYEvents)
        || StellateBlockAreaEventConverter.HasEvents(source.ScaleAnchorXEvents)
        || StellateBlockAreaEventConverter.HasEvents(source.ScaleAnchorYEvents);

    private static void ValidateCoordinates(IrBlockArea source)
    {
        double[] coordinates =
        [
            source.TopRightX,
            source.TopRightY,
            source.BottomLeftX,
            source.BottomLeftY,
        ];
        if (!coordinates.All(double.IsFinite))
            throw StellateExceptions.CoordinatesNotFinite();

        if (
            !double.IsFinite(Math.Abs(source.TopRightX - source.BottomLeftX))
            || !double.IsFinite(Math.Abs(source.TopRightY - source.BottomLeftY))
        )
            throw StellateExceptions.SizeOutOfRange();
    }

    private static void ValidateLifecycle(IrBlockArea source, int index)
    {
        if (
            source.AppearBeat > source.EnableBeat
            || source.EnableBeat > source.DisableBeat
            || source.DisableBeat > source.DisappearBeat
        )
            throw StellateExceptions.LifecycleOrderInvalid(index);
    }
}
