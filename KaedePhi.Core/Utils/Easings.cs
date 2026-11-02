using System;
using KaedePhi.Core.Common;

namespace KaedePhi.Core.Utils
{
    /// <summary>
    /// 表示缓动函数数据。
    /// </summary>
    public static class Easings
    {
        /// <summary>
        /// 表示双精度数值回调。
        /// </summary>
        public delegate double EasingFunction(double t);

        // 线性
        /// <summary>
        /// 按线性方式映射输入进度。
        /// </summary>
        /// <param name="t">输入进度值。</param>
        /// <returns>映射后的进度值。</returns>
        public static double Linear(double t) => t;

        // 二次
        /// <summary>
        /// 使用二次曲线计算缓入缓动值。
        /// </summary>
        /// <param name="t">输入进度值。</param>
        /// <returns>映射后的进度值。</returns>
        public static double EaseInQuad(double t) => t * t;

        /// <summary>
        /// 使用二次曲线计算缓出缓动值。
        /// </summary>
        /// <param name="t">输入进度值。</param>
        /// <returns>映射后的进度值。</returns>
        public static double EaseOutQuad(double t) => t * (2 - t);

        /// <summary>
        /// 使用二次曲线计算缓入缓出缓动值。
        /// </summary>
        /// <param name="t">输入进度值。</param>
        /// <returns>映射后的进度值。</returns>
        public static double EaseInOutQuad(double t) => t < 0.5 ? 2 * t * t : -1 + (4 - 2 * t) * t;

        // 三次
        /// <summary>
        /// 使用三次曲线计算缓入缓动值。
        /// </summary>
        /// <param name="t">输入进度值。</param>
        /// <returns>映射后的进度值。</returns>
        public static double EaseInCubic(double t) => t * t * t;

        /// <summary>
        /// 使用三次曲线计算缓出缓动值。
        /// </summary>
        /// <param name="t">输入进度值。</param>
        /// <returns>映射后的进度值。</returns>
        public static double EaseOutCubic(double t)
        {
            t--;
            return t * t * t + 1;
        }

        /// <summary>
        /// 使用三次曲线计算缓入缓出缓动值。
        /// </summary>
        /// <param name="t">输入进度值。</param>
        /// <returns>映射后的进度值。</returns>
        public static double EaseInOutCubic(double t) =>
            t < 0.5 ? 4 * t * t * t : (t - 1) * (2 * t - 2) * (2 * t - 2) + 1;

        // 四次
        /// <summary>
        /// 使用四次曲线计算缓入缓动值。
        /// </summary>
        /// <param name="t">输入进度值。</param>
        /// <returns>映射后的进度值。</returns>
        public static double EaseInQuart(double t) => t * t * t * t;

        /// <summary>
        /// 使用四次曲线计算缓出缓动值。
        /// </summary>
        /// <param name="t">输入进度值。</param>
        /// <returns>映射后的进度值。</returns>
        public static double EaseOutQuart(double t)
        {
            t--;
            return 1 - t * t * t * t;
        }

        /// <summary>
        /// 使用四次曲线计算缓入缓出缓动值。
        /// </summary>
        /// <param name="t">输入进度值。</param>
        /// <returns>映射后的进度值。</returns>
        public static double EaseInOutQuart(double t) =>
            t < 0.5 ? 8 * t * t * t * t : 1 - 8 * --t * t * t * t;

        // 五次
        /// <summary>
        /// 使用五次曲线计算缓入缓动值。
        /// </summary>
        /// <param name="t">输入进度值。</param>
        /// <returns>映射后的进度值。</returns>
        public static double EaseInQuint(double t) => t * t * t * t * t;

        /// <summary>
        /// 使用五次曲线计算缓出缓动值。
        /// </summary>
        /// <param name="t">输入进度值。</param>
        /// <returns>映射后的进度值。</returns>
        public static double EaseOutQuint(double t)
        {
            t--;
            return t * t * t * t * t + 1;
        }

        /// <summary>
        /// 使用五次曲线计算缓入缓出缓动值。
        /// </summary>
        /// <param name="t">输入进度值。</param>
        /// <returns>映射后的进度值。</returns>
        public static double EaseInOutQuint(double t) =>
            t < 0.5 ? 16 * t * t * t * t * t : 1 + 16 * --t * t * t * t * t;

        // 正弦
        /// <summary>
        /// 使用正弦曲线计算缓入缓动值。
        /// </summary>
        /// <param name="t">输入进度值。</param>
        /// <returns>映射后的进度值。</returns>
        public static double EaseInSine(double t) => 1 - Math.Cos(t * Math.PI / 2);

        /// <summary>
        /// 使用正弦曲线计算缓出缓动值。
        /// </summary>
        /// <param name="t">输入进度值。</param>
        /// <returns>映射后的进度值。</returns>
        public static double EaseOutSine(double t) => Math.Sin(t * Math.PI / 2);

        /// <summary>
        /// 使用正弦曲线计算缓入缓出缓动值。
        /// </summary>
        /// <param name="t">输入进度值。</param>
        /// <returns>映射后的进度值。</returns>
        public static double EaseInOutSine(double t) => -0.5f * (Math.Cos(Math.PI * t) - 1);

        // 指数
        /// <summary>
        /// 使用指数曲线计算缓入缓动值。
        /// </summary>
        /// <param name="t">输入进度值。</param>
        /// <returns>映射后的进度值。</returns>
        public static double EaseInExpo(double t) =>
            Math.Abs(t) < CoreConstants.FloatEpsilon ? 0 : Math.Pow(2, 10 * (t - 1));

        /// <summary>
        /// 使用指数曲线计算缓出缓动值。
        /// </summary>
        /// <param name="t">输入进度值。</param>
        /// <returns>映射后的进度值。</returns>
        public static double EaseOutExpo(double t) =>
            Math.Abs(t - 1) < CoreConstants.FloatEpsilon ? 1 : 1 - Math.Pow(2, -10 * t);

        /// <summary>
        /// 使用指数曲线计算缓入缓出缓动值。
        /// </summary>
        /// <param name="t">输入进度值。</param>
        /// <returns>映射后的进度值。</returns>
        public static double EaseInOutExpo(double t)
        {
            if (
                Math.Abs(t) < CoreConstants.FloatEpsilon
                || Math.Abs(t - 1) < CoreConstants.FloatEpsilon
            )
                return t;
            return t < 0.5f
                ? 0.5f * Math.Pow(2, 20 * t - 10)
                : 1 - 0.5f * Math.Pow(2, -20 * t + 10);
        }

        // 圆形
        /// <summary>
        /// 使用圆弧曲线计算缓入缓动值。
        /// </summary>
        /// <param name="t">输入进度值。</param>
        /// <returns>映射后的进度值。</returns>
        public static double EaseInCirc(double t) => 1 - Math.Sqrt(1 - t * t);

        /// <summary>
        /// 使用圆弧曲线计算缓出缓动值。
        /// </summary>
        /// <param name="t">输入进度值。</param>
        /// <returns>映射后的进度值。</returns>
        public static double EaseOutCirc(double t) => Math.Sqrt(1 - --t * t);

        /// <summary>
        /// 使用圆弧曲线计算缓入缓出缓动值。
        /// </summary>
        /// <param name="t">输入进度值。</param>
        /// <returns>映射后的进度值。</returns>
        public static double EaseInOutCirc(double t) =>
            t < 0.5f
                ? 0.5f * (1 - Math.Sqrt(1 - 4 * t * t))
                : 0.5f * (Math.Sqrt(1 - 4 * --t * t) + 1);

        // 回弹
        /// <summary>
        /// 使用回退曲线计算缓入缓动值。
        /// </summary>
        /// <param name="t">输入进度值。</param>
        /// <returns>映射后的进度值。</returns>
        public static double EaseInBack(double t)
        {
            const double s = 1.70158d;
            return t * t * ((s + 1) * t - s);
        }

        /// <summary>
        /// 使用回退曲线计算缓出缓动值。
        /// </summary>
        /// <param name="t">输入进度值。</param>
        /// <returns>映射后的进度值。</returns>
        public static double EaseOutBack(double t)
        {
            const double s = 1.70158d;
            t--;
            return t * t * ((s + 1) * t + s) + 1;
        }

        /// <summary>
        /// 使用回退曲线计算缓入缓出缓动值。
        /// </summary>
        /// <param name="t">输入进度值。</param>
        /// <returns>映射后的进度值。</returns>
        public static double EaseInOutBack(double t)
        {
            const double s = 1.70158d * 1.525f;
            t *= 2;
            if (t < 1)
                return 0.5f * (t * t * ((s + 1) * t - s));
            t -= 2;
            return 0.5f * (t * t * ((s + 1) * t + s) + 2);
        }

        // 弹性
        /// <summary>
        /// 使用弹性曲线计算缓入缓动值。
        /// </summary>
        /// <param name="t">输入进度值。</param>
        /// <returns>映射后的进度值。</returns>
        public static double EaseInElastic(double t)
        {
            if (
                Math.Abs(t) < CoreConstants.FloatEpsilon
                || Math.Abs(t - 1) < CoreConstants.FloatEpsilon
            )
                return t;
            return -Math.Pow(2, 10 * (t - 1)) * Math.Sin((t - 1.1f) * 5 * Math.PI);
        }

        /// <summary>
        /// 使用弹性曲线计算缓出缓动值。
        /// </summary>
        /// <param name="t">输入进度值。</param>
        /// <returns>映射后的进度值。</returns>
        public static double EaseOutElastic(double t)
        {
            if (
                Math.Abs(t) < CoreConstants.FloatEpsilon
                || Math.Abs(t - 1) < CoreConstants.FloatEpsilon
            )
                return t;
            return Math.Pow(2, -10 * t) * Math.Sin((t - 0.1f) * 5 * Math.PI) + 1;
        }

        /// <summary>
        /// 使用弹性曲线计算缓入缓出缓动值。
        /// </summary>
        /// <param name="t">输入进度值。</param>
        /// <returns>映射后的进度值。</returns>
        public static double EaseInOutElastic(double t)
        {
            if (
                Math.Abs(t) < CoreConstants.FloatEpsilon
                || Math.Abs(t - 1) < CoreConstants.FloatEpsilon
            )
                return t;
            t *= 2;
            if (t < 1)
                return -0.5f * Math.Pow(2, 10 * (t - 1)) * Math.Sin((t - 1.1f) * 5 * Math.PI);
            t--;
            return Math.Pow(2, -10 * t) * Math.Sin((t - 0.1f) * 5 * Math.PI) * 0.5f + 1;
        }

        // 弹跳
        /// <summary>
        /// 使用弹跳曲线计算缓入缓动值。
        /// </summary>
        /// <param name="t">输入进度值。</param>
        /// <returns>映射后的进度值。</returns>
        public static double EaseInBounce(double t) => 1 - EaseOutBounce(1 - t);

        /// <summary>
        /// 使用弹跳曲线计算缓出缓动值。
        /// </summary>
        /// <param name="t">输入进度值。</param>
        /// <returns>映射后的进度值。</returns>
        public static double EaseOutBounce(double t)
        {
            const double n1 = 7.5625f;
            const double d1 = 2.75f;
            switch (t)
            {
                case < 1 / d1:
                    return n1 * t * t;
                case < 2 / d1:
                    t -= 1.5f / d1;
                    return n1 * t * t + 0.75f;
                case < 2.5 / d1:
                    t -= 2.25f / d1;
                    return n1 * t * t + 0.9375f;
                default:
                    t -= 2.625f / d1;
                    return n1 * t * t + 0.984375f;
            }
        }

        /// <summary>
        /// 使用弹跳曲线计算缓入缓出缓动值。
        /// </summary>
        /// <param name="t">输入进度值。</param>
        /// <returns>映射后的进度值。</returns>
        public static double EaseInOutBounce(double t) =>
            t < 0.5f
                ? (1 - EaseOutBounce(1 - 2 * t)) * 0.5f
                : (EaseOutBounce(2 * t - 1) + 1) * 0.5f;
    }
}
