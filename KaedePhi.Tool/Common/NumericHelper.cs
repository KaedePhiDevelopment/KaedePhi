using System.Runtime.CompilerServices;

namespace KaedePhi.Tool.Common;

/// <summary>
/// 为 int/float/double 提供无装箱的算术与类型转换操作，替代 dynamic 分发。
/// </summary>
public static class NumericHelper
{
    /// <summary>
    /// 将两个数值相加，并返回计算结果。
    /// </summary>
    /// <typeparam name="T">事件载荷类型</typeparam>
    /// <param name="left">左侧数值。</param>
    /// <param name="right">右侧数值。</param>
    /// <returns>相加得到的数值。</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Add<T>(T? left, T? right)
    {
        if (left is null || right is null)
            throw new ArgumentNullException(
                $"NumericHelper.Add does not support null values for type {typeof(T)}."
            );
        if (typeof(T) == typeof(double))
            return (T)(object)((double)(object)left + (double)(object)right);
        if (typeof(T) == typeof(float))
            return (T)(object)((float)(object)left + (float)(object)right);
        if (typeof(T) == typeof(int))
            return (T)(object)((int)(object)left + (int)(object)right);
        throw new NotSupportedException($"NumericHelper.Add does not support type {typeof(T)}.");
    }

    /// <summary>
    /// 将数值转换为双精度浮点数。
    /// </summary>
    /// <typeparam name="T">事件载荷类型</typeparam>
    /// <param name="value">待转换的数值。</param>
    /// <returns>转换后的双精度浮点数。</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double ToDouble<T>(T? value)
    {
        if (value is null)
            throw new ArgumentNullException(
                $"NumericHelper.ToDouble does not support null values for type {typeof(T)}."
            );
        if (typeof(T) == typeof(double))
            return (double)(object)value;
        if (typeof(T) == typeof(float))
            return (float)(object)value;
        if (typeof(T) == typeof(int))
            return (int)(object)value;
        throw new NotSupportedException(
            $"NumericHelper.ToDouble does not support type {typeof(T)}."
        );
    }
}
