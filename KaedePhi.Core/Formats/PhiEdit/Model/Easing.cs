using JetBrains.Annotations;

namespace KaedePhi.Core.Formats.PhiEdit.Model
{
    /// <summary>
    /// 表示事件使用的缓动参数。
    /// </summary>
    public class Easing
    {
        private static readonly Easing?[] Cache = new Easing[30];

        /// <summary>
        /// 线性缓动（编号1）。
        /// </summary>
        public static Easing Linear { get; } = Get(1);

        private readonly int _easingNumber;
        private readonly Primitives.Mathematics.Easings.EasingFunction _function;

        /// <summary>获取缓存的 Easing 实例，避免重复创建。</summary>
        public static Easing Get(int easingNumber)
        {
            if (easingNumber is >= 1 and <= 29)
                return Cache[easingNumber] ??= new Easing(easingNumber);
            return new Easing(easingNumber);
        }

        /// <summary>
        /// 初始化缓动的新实例。
        /// </summary>
        /// <param name="easingNumber">缓动编号。</param>
        public Easing(int easingNumber)
        {
            _easingNumber = easingNumber;
            _function = Easings.GetFunction(easingNumber);
        }

        /// <summary>对 [start, end] 区间在 t 处进行插值</summary>
        [PublicAPI]
        public float Interpolate(float start, float end, float t)
        {
            var easedTime = _function(t);
            return (float)(start + (end - start) * easedTime);
        }

        /// <inheritdoc cref="Interpolate(float,float,float)"/>
        [PublicAPI]
        public double Interpolate(double start, double end, double t)
        {
            var easedTime = _function(t);
            return start + (end - start) * easedTime;
        }

        /// <summary>
        /// 将缓动配置转换为对应的编号。
        /// </summary>
        /// <param name="easing">缓动配置。</param>
        /// <returns>操作得到的结果。</returns>
        public static implicit operator int(Easing easing) => easing._easingNumber;

        /// <summary>
        /// 根据缓动编号获取缓动配置。
        /// </summary>
        /// <param name="easingNumber">缓动编号。</param>
        /// <returns>操作得到的结果。</returns>
        public static implicit operator Easing(int easingNumber) => Get(easingNumber);
    }
}
