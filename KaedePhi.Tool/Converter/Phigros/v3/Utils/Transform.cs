using KaedePhi.Tool.Common;
using PhigrosV3 = KaedePhi.Core.Formats.Phigros.v3.Model.Chart;

namespace KaedePhi.Tool.Converter.Phigros.v3.Utils;

/// <summary>
/// PhigrosV3 坐标系与 IR 坐标系之间的坐标变换工具。
/// </summary>
public static class Transform
{
    private static readonly CoordinateProfile PhigrosV3Profile = new(
        PhigrosV3.CoordinateSystem.MinX,
        PhigrosV3.CoordinateSystem.MaxX,
        PhigrosV3.CoordinateSystem.MinY,
        PhigrosV3.CoordinateSystem.MaxY,
        PhigrosV3.CoordinateSystem.ClockwiseRotation
    );

    /// <summary>
    /// 将 Phigros V3 横坐标转换为 IR 横坐标。
    /// </summary>
    /// <param name="x">待转换的横坐标。</param>
    /// <returns>转换后的坐标值。</returns>
    public static double ToIrX(float x) => CoordinateGeometry.ToIrX(x, PhigrosV3Profile);

    /// <summary>
    /// 将 Phigros V3 纵坐标转换为 IR 纵坐标。
    /// </summary>
    /// <param name="y">待转换的纵坐标。</param>
    /// <returns>转换后的坐标值。</returns>
    public static double ToIrY(float y) => CoordinateGeometry.ToIrY(y, PhigrosV3Profile);

    /// <summary>
    /// 将 Phigros V3 角度转换为 IR 角度。
    /// </summary>
    /// <param name="angle">待转换的角度。</param>
    /// <returns>转换后的坐标值。</returns>
    public static double ToIrAngle(float angle) =>
        CoordinateGeometry.ToIrAngle(angle, PhigrosV3Profile);

    /// <summary>
    /// 将 IR 横坐标转换为 Phigros V3 横坐标。
    /// </summary>
    /// <param name="x">待转换的横坐标。</param>
    /// <returns>转换后的坐标值。</returns>
    public static float ToPhigrosV3X(double x) =>
        CoordinateGeometry.ToTargetXf(x, PhigrosV3Profile);

    /// <summary>
    /// 将 IR 纵坐标转换为 Phigros V3 纵坐标。
    /// </summary>
    /// <param name="y">待转换的纵坐标。</param>
    /// <returns>转换后的坐标值。</returns>
    public static float ToPhigrosV3Y(double y) =>
        CoordinateGeometry.ToTargetYf(y, PhigrosV3Profile);

    /// <summary>
    /// 将 IR 角度转换为 Phigros V3 角度。
    /// </summary>
    /// <param name="angle">待转换的角度。</param>
    /// <returns>转换后的坐标值。</returns>
    public static float ToPhigrosV3Angle(double angle) =>
        (float)CoordinateGeometry.ToTargetAngle(angle, PhigrosV3Profile);
}
