using KaedePhi.Core.PhiChain.v6.JsonConverter;
using Newtonsoft.Json;

namespace KaedePhi.Core.PhiChain.v6
{
    /// <summary>
    /// 定义缓动函数可使用的类型。
    /// </summary>
    public enum EasingKind
    {
        /// <summary>
        /// 表示线性缓动。
        /// </summary>
        Linear,

        /// <summary>
        /// 表示正弦缓入。
        /// </summary>
        EaseInSine,

        /// <summary>
        /// 表示正弦缓出。
        /// </summary>
        EaseOutSine,

        /// <summary>
        /// 表示正弦缓入缓出。
        /// </summary>
        EaseInOutSine,

        /// <summary>
        /// 表示二次缓入。
        /// </summary>
        EaseInQuad,

        /// <summary>
        /// 表示二次缓出。
        /// </summary>
        EaseOutQuad,

        /// <summary>
        /// 表示二次缓入缓出。
        /// </summary>
        EaseInOutQuad,

        /// <summary>
        /// 表示三次缓入。
        /// </summary>
        EaseInCubic,

        /// <summary>
        /// 表示三次缓出。
        /// </summary>
        EaseOutCubic,

        /// <summary>
        /// 表示三次缓入缓出。
        /// </summary>
        EaseInOutCubic,

        /// <summary>
        /// 表示四次缓入。
        /// </summary>
        EaseInQuart,

        /// <summary>
        /// 表示四次缓出。
        /// </summary>
        EaseOutQuart,

        /// <summary>
        /// 表示四次缓入缓出。
        /// </summary>
        EaseInOutQuart,

        /// <summary>
        /// 表示五次缓入。
        /// </summary>
        EaseInQuint,

        /// <summary>
        /// 表示五次缓出。
        /// </summary>
        EaseOutQuint,

        /// <summary>
        /// 表示五次缓入缓出。
        /// </summary>
        EaseInOutQuint,

        /// <summary>
        /// 表示指数缓入。
        /// </summary>
        EaseInExpo,

        /// <summary>
        /// 表示指数缓出。
        /// </summary>
        EaseOutExpo,

        /// <summary>
        /// 表示指数缓入缓出。
        /// </summary>
        EaseInOutExpo,

        /// <summary>
        /// 表示圆弧缓入。
        /// </summary>
        EaseInCirc,

        /// <summary>
        /// 表示圆弧缓出。
        /// </summary>
        EaseOutCirc,

        /// <summary>
        /// 表示圆弧缓入缓出。
        /// </summary>
        EaseInOutCirc,

        /// <summary>
        /// 表示回退缓入。
        /// </summary>
        EaseInBack,

        /// <summary>
        /// 表示回退缓出。
        /// </summary>
        EaseOutBack,

        /// <summary>
        /// 表示回退缓入缓出。
        /// </summary>
        EaseInOutBack,

        /// <summary>
        /// 表示弹性缓入。
        /// </summary>
        EaseInElastic,

        /// <summary>
        /// 表示弹性缓出。
        /// </summary>
        EaseOutElastic,

        /// <summary>
        /// 表示弹性缓入缓出。
        /// </summary>
        EaseInOutElastic,

        /// <summary>
        /// 表示弹跳缓入。
        /// </summary>
        EaseInBounce,

        /// <summary>
        /// 表示弹跳缓出。
        /// </summary>
        EaseOutBounce,

        /// <summary>
        /// 表示弹跳缓入缓出。
        /// </summary>
        EaseInOutBounce,

        /// <summary>
        /// 表示自定义缓动。
        /// </summary>
        Custom,

        /// <summary>
        /// 表示阶梯缓动。
        /// </summary>
        Steps,

        /// <summary>
        /// 表示弹性缓动。
        /// </summary>
        Elastic,
    }

    /// <summary>
    /// 表示事件使用的缓动参数。
    /// </summary>
    [JsonConverter(typeof(EasingJsonConverter))]
    public sealed class Easing
    {
        /// <summary>
        /// 获取线性缓动。
        /// </summary>
        public static Easing Linear => new() { EasingType = EasingKind.Linear };

        /// <summary>
        /// 获取或设置缓动类型。
        /// </summary>
        [JsonIgnore]
        public EasingKind EasingType { get; set; } = EasingKind.Linear;

        /// <summary>
        /// 获取或设置横坐标一。
        /// </summary>
        [JsonIgnore]
        public float X1 { get; set; }

        /// <summary>
        /// 获取或设置纵坐标一。
        /// </summary>
        [JsonIgnore]
        public float Y1 { get; set; }

        /// <summary>
        /// 获取或设置横坐标二。
        /// </summary>
        [JsonIgnore]
        public float X2 { get; set; }

        /// <summary>
        /// 获取或设置纵坐标二。
        /// </summary>
        [JsonIgnore]
        public float Y2 { get; set; }

        /// <summary>
        /// 获取或设置数量。
        /// </summary>
        [JsonIgnore]
        public int Count { get; set; }

        /// <summary>
        /// 获取或设置角频率。
        /// </summary>
        [JsonIgnore]
        public float Omega { get; set; }

        /// <summary>
        /// 深克隆当前 Easing 对象
        /// </summary>
        public Easing Clone()
        {
            return new Easing
            {
                EasingType = EasingType,
                X1 = X1,
                Y1 = Y1,
                X2 = X2,
                Y2 = Y2,
                Count = Count,
                Omega = Omega,
            };
        }
    }
}
