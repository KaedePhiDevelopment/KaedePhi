using KaedePhi.Core.Formats.RePhiEdit.Serialization.JsonConverter;
using Newtonsoft.Json;

namespace KaedePhi.Core.Formats.RePhiEdit.Model
{
    /// <summary>
    /// 表示事件使用的缓动参数。
    /// </summary>
    [JsonConverter(typeof(EasingJsonConverter))]
    public class Easing
    {
        /// <summary>
        /// 线性缓动（编号1）。
        /// </summary>
        public static Easing Linear { get; } = new(1);

        /// <summary>
        /// 初始化缓动的新实例。
        /// </summary>
        /// <param name="easingNumber">缓动编号。</param>
        public Easing(int easingNumber)
        {
            _easingNumber = easingNumber;
        }

        [JsonIgnore]
        private readonly int _easingNumber;

        /// <summary>
        /// 计算缓动的插值结果。
        /// </summary>
        /// <param name="minLim">缓动输入下界。</param>
        /// <param name="maxLim">缓动输入上界。</param>
        /// <param name="start">起始数值</param>
        /// <param name="end">结束数值</param>
        /// <param name="t">输入进度值。</param>
        /// <returns>计算得到的数值。</returns>
        public float Interpolate(float minLim, float maxLim, float start, float end, float t)
        {
            var easedTime = Easings.Evaluate(_easingNumber, minLim, maxLim, t);
            // 插值后返回
            return (float)(start + (end - start) * easedTime);
        }

        /// <summary>
        /// 计算缓动的插值结果。
        /// </summary>
        /// <param name="minLim">缓动输入下界。</param>
        /// <param name="maxLim">缓动输入上界。</param>
        /// <param name="start">起始数值</param>
        /// <param name="end">结束数值</param>
        /// <param name="t">输入进度值。</param>
        /// <returns>计算得到的数值。</returns>
        public double Interpolate(float minLim, float maxLim, double start, double end, double t)
        {
            var easedTime = Easings.Evaluate(_easingNumber, minLim, maxLim, t);
            // 插值后返回
            return start + (end - start) * easedTime;
        }

        /// <summary>
        /// 计算缓动的插值结果。
        /// </summary>
        /// <param name="minLim">缓动输入下界。</param>
        /// <param name="maxLim">缓动输入上界。</param>
        /// <param name="start">起始数值</param>
        /// <param name="end">结束数值</param>
        /// <param name="t">输入进度值。</param>
        /// <returns>计算得到的数值。</returns>
        public int Interpolate(float minLim, float maxLim, int start, int end, float t)
        {
            var easedTime = Easings.Evaluate(_easingNumber, minLim, maxLim, t);
            // 插值后返回
            return (int)(start + (end - start) * easedTime);
        }

        /// <summary>
        /// 计算缓动的插值结果。
        /// </summary>
        /// <param name="minLim">缓动输入下界。</param>
        /// <param name="maxLim">缓动输入上界。</param>
        /// <param name="start">起始数值</param>
        /// <param name="end">结束数值</param>
        /// <param name="t">输入进度值。</param>
        /// <returns>操作得到的结果。</returns>
        public byte Interpolate(float minLim, float maxLim, byte start, byte end, float t)
        {
            var easedTime = Easings.Evaluate(_easingNumber, minLim, maxLim, t);
            // 插值后返回
            return (byte)(start + (end - start) * easedTime);
        }

        // 以int访问时，返回缓动编号
        /// <summary>
        /// 将缓动配置转换为对应的编号。
        /// </summary>
        /// <param name="easing">缓动配置。</param>
        /// <returns>操作得到的结果。</returns>
        public static implicit operator int(Easing easing) => easing._easingNumber;
    }
}
