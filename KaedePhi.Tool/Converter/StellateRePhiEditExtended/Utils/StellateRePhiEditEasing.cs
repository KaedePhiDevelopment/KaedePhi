using KaedePhi.Tool.Converter.Phigros.v3.Utils;
using CoreBezier = KaedePhi.Core.Primitives.Mathematics.Bezier;
using IrEasing = KaedePhi.Core.Intermediate.Model.Easing;
using RpeCoreEasings = KaedePhi.Core.Formats.RePhiEdit.Model.Easings;
using RpeEasingConverter = KaedePhi.Tool.Converter.RePhiEdit.Utils.EasingConverter;
using RpeEvent = KaedePhi.Core.Formats.RePhiEdit.Model.Events.Event<float>;

namespace KaedePhi.Tool.Converter.StellateRePhiEditExtended.Utils;

/// <summary>
/// 提供 StellateRePhiEditExtended 缓动类型映射和曲线求值功能。
/// </summary>
public static class StellateRePhiEditEasing
{
    private const int MinimumType = 1;
    private const int MaximumType = 28;

    /// <summary>
    /// 将缓动编号规范到支持的取值范围。
    /// </summary>
    /// <param name="easingType">缓动编号</param>
    public static int NormalizeType(int easingType)
    {
        if (easingType is <= -MinimumType and >= -MaximumType)
            easingType = -easingType;
        return easingType is >= MinimumType and <= MaximumType ? easingType : MinimumType;
    }

    /// <summary>
    /// 判断事件是否包含有效的贝塞尔控制点。
    /// </summary>
    /// <param name="evt">待检查的 RePhiEdit 事件</param>
    public static bool IsBezierEvent(RpeEvent evt) =>
        evt is { IsBezier: true, BezierPoints.Length: 4 };

    /// <summary>
    /// 将 RePhiEdit 缓动编号映射为噪域缓动编号。
    /// </summary>
    /// <param name="easingType">RePhiEdit 缓动编号</param>
    /// <param name="areaEaseType">映射后的噪域缓动编号；未映射时为负数</param>
    public static bool TryMapToBlockArea(int easingType, out int areaEaseType)
    {
        var irEasing = new IrEasing(RpeEasingConverter.MapToIr(NormalizeType(easingType)));
        var mapped = PhigrosV3BlockAreaBuilder.TryToAreaEase(irEasing, out var mappedType);
        areaEaseType = mapped ? (int)mappedType : -1;
        return mapped;
    }

    /// <summary>
    /// 根据事件缓动设置计算指定进度的值。
    /// </summary>
    /// <param name="evt">包含缓动设置的 RePhiEdit 事件</param>
    /// <param name="progress">待求值的进度</param>
    /// <param name="escape">是否将区间外进度按端点规则处理</param>
    public static double Evaluate(RpeEvent evt, double progress, bool escape)
    {
        if (progress <= 0d && escape)
            return 0d;
        if (progress >= 1d)
            return 1d;

        return IsBezierEvent(evt)
            ? CoreBezier.Evaluate(evt.BezierPoints, progress)
            : RpeCoreEasings.Evaluate(
                NormalizeType((int)evt.Easing),
                evt.EasingLeft,
                evt.EasingRight,
                progress
            );
    }
}
