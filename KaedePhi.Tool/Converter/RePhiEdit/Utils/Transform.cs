using KaedePhi.Tool.Common;

namespace KaedePhi.Tool.Converter.RePhiEdit.Utils;

/// <summary>
/// RPE 坐标系与 IR 坐标系之间的坐标变换工具。
/// </summary>
public static class Transform
{
    /// <summary>
    /// 将 RePhiEdit 横坐标转换为 IR 横坐标。
    /// </summary>
    /// <param name="x">待转换的横坐标。</param>
    /// <returns>转换后的坐标值。</returns>
    public static double TransformToIrX(float x) => CoordinateGeometry.ToIrX(x);

    /// <summary>
    /// 将 RePhiEdit 纵坐标转换为 IR 纵坐标。
    /// </summary>
    /// <param name="y">待转换的纵坐标。</param>
    /// <returns>转换后的坐标值。</returns>
    public static double TransformToIrY(float y) => CoordinateGeometry.ToIrY(y);

    /// <summary>
    /// 将 RePhiEdit 角度转换为 IR 角度。
    /// </summary>
    /// <param name="angle">待转换的角度。</param>
    /// <returns>转换后的坐标值。</returns>
    public static double TransformToIrAngle(float angle) => CoordinateGeometry.ToIrAngle(angle);

    /// <summary>
    /// 将 IR 横坐标转换为 RePhiEdit 渲染坐标。
    /// </summary>
    /// <param name="x">待转换的横坐标。</param>
    /// <returns>转换后的坐标值。</returns>
    public static double TransformToRpeX(double x) => CoordinateGeometry.ToRenderX(x);

    /// <summary>
    /// 将 IR 纵坐标转换为 RePhiEdit 渲染坐标。
    /// </summary>
    /// <param name="y">待转换的纵坐标。</param>
    /// <returns>转换后的坐标值。</returns>
    public static double TransformToRpeY(double y) => CoordinateGeometry.ToRenderY(y);

    /// <summary>
    /// 将 IR 横坐标转换为 RePhiEdit 单精度渲染坐标。
    /// </summary>
    /// <param name="x">待转换的横坐标。</param>
    /// <returns>转换后的坐标值。</returns>
    public static float FloatTransformToRpeX(double x) => CoordinateGeometry.ToRenderXf(x);

    /// <summary>
    /// 将 IR 纵坐标转换为 RePhiEdit 单精度渲染坐标。
    /// </summary>
    /// <param name="y">待转换的纵坐标。</param>
    /// <returns>转换后的坐标值。</returns>
    public static float FloatTransformToRpeY(double y) => CoordinateGeometry.ToRenderYf(y);

    /// <summary>
    /// 将 IR 角度转换为 RePhiEdit 角度。
    /// </summary>
    /// <param name="angle">待转换的角度。</param>
    /// <returns>转换后的坐标值。</returns>
    public static double TransformToRpeAngle(double angle) =>
        CoordinateGeometry.ToRenderAngle(angle);
}
