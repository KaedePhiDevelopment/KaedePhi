using KaedePhi.Core.Primitives;
using Newtonsoft.Json;

namespace KaedePhi.Core.Formats.PhiFans.Model
{
    /// <summary>
    /// 表示节拍速度数据。
    /// </summary>
    public class Bpm
    {
        /// <summary>
        /// 获取或设置开始拍。
        /// </summary>
        [JsonProperty("beat")]
        public Beat StartBeat { get; set; } = new(0);

        /// <summary>
        /// 获取或设置拍相关分钟。
        /// </summary>
        [JsonProperty("bpm")]
        public float BeatPerMinute { get; set; } = 120;
    }
}
