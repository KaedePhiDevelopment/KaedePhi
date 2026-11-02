using KaedePhi.Core.Common;
using Newtonsoft.Json;

namespace KaedePhi.Core.PhiFans
{
    /// <summary>
    /// 表示事件数据。
    /// </summary>
    public class Event
    {
        /// <summary>
        /// 获取或设置拍。
        /// </summary>
        [JsonProperty("beat")]
        public Beat Beat { get; set; } = new(0);

        /// <summary>
        /// 获取或设置数值。
        /// </summary>
        [JsonProperty("value")]
        public float Value { get; set; }

        /// <summary>
        /// 获取或设置是否启用连续。
        /// </summary>
        [JsonProperty("continuous")]
        public bool Continuous { get; set; }

        /// <summary>
        /// 获取或设置缓动。
        /// </summary>
        [JsonProperty("easing")]
        public Easing Easing { get; set; } = new(0);
    }
}
