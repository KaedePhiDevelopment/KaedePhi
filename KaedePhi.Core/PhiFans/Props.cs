using System.Collections.Generic;
using Newtonsoft.Json;

namespace KaedePhi.Core.PhiFans
{
    /// <summary>
    /// 表示属性数据。
    /// </summary>
    public class Props
    {
        /// <summary>
        /// 获取或设置速度。
        /// </summary>
        [JsonProperty("speed")]
        public List<Event> Speed { get; set; } = new();

        /// <summary>
        /// 获取或设置位置横坐标。
        /// </summary>
        [JsonProperty("positionX")]
        public List<Event> PositionX { get; set; } = new();

        /// <summary>
        /// 获取或设置位置纵坐标。
        /// </summary>
        [JsonProperty("positionY")]
        public List<Event> PositionY { get; set; } = new();

        /// <summary>
        /// 获取或设置旋转。
        /// </summary>
        [JsonProperty("rotate")]
        public List<Event> Rotate { get; set; } = new();

        /// <summary>
        /// 获取或设置不透明度。
        /// </summary>
        [JsonProperty("alpha")]
        public List<Event> Alpha { get; set; } = new();
    }
}
