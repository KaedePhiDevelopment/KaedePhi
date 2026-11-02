using KaedePhi.Tool.Common;

namespace KaedePhi.Tool.Converter.PhiEdit.Utils;

/// <summary>
/// PE 与 IR 之间的坐标及速度值变换工具。
/// </summary>
public static class Transform
{
    // 该比例由 PE 与 IR 的速度单位定义，不能作为用户选项。
    private const float SpeedValueRatio = 14f / 9f;

    private static readonly CoordinateProfile PeCoordinateProfile = new(
        Pe.Chart.CoordinateSystem.MinX,
        Pe.Chart.CoordinateSystem.MaxX,
        Pe.Chart.CoordinateSystem.MinY,
        Pe.Chart.CoordinateSystem.MaxY,
        Pe.Chart.CoordinateSystem.ClockwiseRotation
    );

    /// <summary>
    /// 将 PhiEdit 横坐标转换为 IR 横坐标。
    /// </summary>
    /// <param name="x">待转换的横坐标。</param>
    /// <returns>转换后的坐标值。</returns>
    public static double TransformToIrX(float x) =>
        CoordinateGeometry.ToIrX(x, PeCoordinateProfile);

    /// <summary>
    /// 将 PhiEdit 纵坐标转换为 IR 纵坐标。
    /// </summary>
    /// <param name="y">待转换的纵坐标。</param>
    /// <returns>转换后的坐标值。</returns>
    public static double TransformToIrY(float y) =>
        CoordinateGeometry.ToIrY(y, PeCoordinateProfile);

    /// <summary>
    /// 将 PhiEdit 角度转换为 IR 角度。
    /// </summary>
    /// <param name="angle">待转换的角度。</param>
    /// <returns>转换后的坐标值。</returns>
    public static double TransformToIrAngle(float angle) =>
        CoordinateGeometry.ToIrAngle(angle, PeCoordinateProfile);

    /// <summary>
    /// 将 IR 横坐标转换为 PhiEdit 横坐标。
    /// </summary>
    /// <param name="x">待转换的横坐标。</param>
    /// <returns>转换后的坐标值。</returns>
    public static float TransformToPeX(double x) =>
        CoordinateGeometry.ToTargetXf(x, PeCoordinateProfile);

    /// <summary>
    /// 将 IR 纵坐标转换为 PhiEdit 纵坐标。
    /// </summary>
    /// <param name="y">待转换的纵坐标。</param>
    /// <returns>转换后的坐标值。</returns>
    public static float TransformToPeY(double y) =>
        CoordinateGeometry.ToTargetYf(y, PeCoordinateProfile);

    /// <summary>
    /// 将 IR 角度转换为 PhiEdit 角度。
    /// </summary>
    /// <param name="angle">待转换的角度。</param>
    /// <returns>转换后的坐标值。</returns>
    public static float TransformToPeAngle(double angle) =>
        (float)CoordinateGeometry.ToTargetAngle(angle, PeCoordinateProfile);

    internal static float TransformToIrSpeed(float speed) => speed / SpeedValueRatio;

    internal static float TransformToPeSpeed(float speed) => speed * SpeedValueRatio;
}
