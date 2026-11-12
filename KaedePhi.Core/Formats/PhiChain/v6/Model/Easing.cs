using KaedePhi.Core.Formats.PhiChain.v6.Serialization.JsonConverter;
using Newtonsoft.Json;

namespace KaedePhi.Core.Formats.PhiChain.v6.Model
{
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
