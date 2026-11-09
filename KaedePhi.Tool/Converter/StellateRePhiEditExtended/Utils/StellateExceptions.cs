using KaedePhi.Tool.Common;

namespace KaedePhi.Tool.Converter.StellateRePhiEditExtended.Utils;

internal static class StellateExceptions
{
    public static FormatException BlockAreaListContainsNull() =>
        new(ToolResources.stellate_error_block_area_null);

    public static FormatException InvalidFatherIndex(int lineIndex) =>
        Format(ToolResources.stellate_error_father_index_invalid, lineIndex);

    public static FormatException FatherCycle() => new(ToolResources.stellate_error_father_cycle);

    public static FormatException FatherUnmapped(int lineIndex) =>
        Format(ToolResources.stellate_error_father_unmapped, lineIndex);

    public static FormatException AnchorPositionNotFinite(int blockAreaIndex) =>
        Format(ToolResources.stellate_error_anchor_position_not_finite, blockAreaIndex);

    public static FormatException AnchorTimeInvalid(int blockAreaIndex) =>
        Format(ToolResources.stellate_error_anchor_time_invalid, blockAreaIndex);

    public static FormatException AnchorStepStalled(int blockAreaIndex) =>
        Format(ToolResources.stellate_error_anchor_step_stalled, blockAreaIndex);

    public static FormatException AnchorSamplesExceeded(int blockAreaIndex, int limit) =>
        Format(ToolResources.stellate_error_anchor_samples_exceeded, blockAreaIndex, limit);

    public static FormatException TrackEventNull(int blockAreaIndex, string trackName) =>
        Format(ToolResources.stellate_error_track_event_null, blockAreaIndex, trackName);

    public static FormatException EventEndBeforeStart(int blockAreaIndex, string trackName) =>
        Format(ToolResources.stellate_error_event_end_before_start, blockAreaIndex, trackName);

    public static FormatException EventValueNotFinite(int blockAreaIndex, string trackName) =>
        Format(ToolResources.stellate_error_event_value_not_finite, blockAreaIndex, trackName);

    public static FormatException EventEasingRangeNotFinite(int blockAreaIndex, string trackName) =>
        Format(
            ToolResources.stellate_error_event_easing_range_not_finite,
            blockAreaIndex,
            trackName
        );

    public static FormatException EventEasingNull(int blockAreaIndex, string trackName) =>
        Format(ToolResources.stellate_error_event_easing_null, blockAreaIndex, trackName);

    public static FormatException EventBezierInvalid(int blockAreaIndex, string trackName) =>
        Format(ToolResources.stellate_error_event_bezier_invalid, blockAreaIndex, trackName);

    public static FormatException EventValueOutOfRange(int blockAreaIndex, string trackName) =>
        Format(ToolResources.stellate_error_event_value_out_of_range, blockAreaIndex, trackName);

    public static FormatException ValueOutOfRange(int blockAreaIndex, string valueName) =>
        Format(ToolResources.stellate_error_value_out_of_range, blockAreaIndex, valueName);

    public static FormatException CenterNotConvertible(int blockAreaIndex) =>
        Format(ToolResources.stellate_error_center_not_convertible, blockAreaIndex);

    public static FormatException CoordinatesNotFinite() =>
        new(ToolResources.stellate_error_coordinates_not_finite);

    public static FormatException SizeOutOfRange() =>
        new(ToolResources.stellate_error_size_out_of_range);

    public static FormatException LifecycleOrderInvalid(int blockAreaIndex) =>
        Format(ToolResources.stellate_error_lifecycle_order_invalid, blockAreaIndex);

    public static InvalidOperationException CurveIndexMissing() =>
        new(ToolResources.stellate_error_curve_index_missing);

    public static InvalidOperationException DirectCurveConvertFailed(string trackName) =>
        new(string.Format(ToolResources.stellate_error_direct_curve_convert_failed, trackName));

    private static FormatException Format(string template, params object[] arguments) =>
        new(string.Format(template, arguments));
}
