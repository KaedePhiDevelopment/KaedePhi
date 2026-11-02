using KaedePhi.Core.Common;
using Newtonsoft.Json;

namespace KaedePhi.Core.PhiFans
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
