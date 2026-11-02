using JetBrains.Annotations;
using static KaedePhi.Core.Utils.Easings;

namespace KaedePhi.Core.PhiEdit
{
    /// <summary>
    /// 表示缓动函数数据。
    /// </summary>
    public static class Easings
    {
        /// <summary>
        /// 根据 PhiEdit 缓动编号获取对应的缓动函数。
        /// </summary>
        public static EasingFunction GetFunction(int easingType) =>
            easingType switch
            {
                1 => Linear,
                2 => EaseOutSine,
                3 => EaseInSine,
                4 => EaseOutQuad,
                5 => EaseInQuad,
                6 => EaseInOutSine,
                7 => EaseInOutQuad,
                8 => EaseOutCubic,
                9 => EaseInCubic,
                10 => EaseOutQuart,
                11 => EaseInQuart,
                12 => EaseInOutCubic,
                13 => EaseInOutQuart,
                14 => EaseOutQuint,
                15 => EaseInQuint,
                16 => EaseOutExpo,
                17 => EaseInExpo,
                18 => EaseOutCirc,
                19 => EaseInCirc,
                20 => EaseOutBack,
                21 => EaseInBack,
                22 => EaseInOutCirc,
                23 => EaseInOutBack,
                24 => EaseOutElastic,
                25 => EaseInElastic,
                26 => EaseOutBounce,
                27 => EaseInBounce,
                28 => EaseInOutBounce,
                29 => EaseInOutElastic,
                _ => Linear,
            };

        // 在任意起点和终点之间评估缓动
        /// <summary>
        /// 计算缓动函数的结果。
        /// </summary>
        /// <param name="function">缓动值计算函数。</param>
        /// <param name="t">输入进度值。</param>
        /// <returns>计算得到的数值。</returns>
        [PublicAPI]
        public static double Evaluate(EasingFunction function, double t)
        {
            return function(t);
        }

        // 使用 int 指定对应的缓动函数
        /// <summary>
        /// 计算缓动函数的结果。
        /// </summary>
        /// <param name="easingType">缓动类型。</param>
        /// <param name="t">输入进度值。</param>
        /// <returns>计算得到的数值。</returns>
        public static double Evaluate(int easingType, double t)
        {
            return Evaluate(GetFunction(easingType), t);
        }
    }
}
