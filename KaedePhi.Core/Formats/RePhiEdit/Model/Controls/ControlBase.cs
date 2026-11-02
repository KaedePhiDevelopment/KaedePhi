using Newtonsoft.Json;

namespace KaedePhi.Core.Formats.RePhiEdit.Model.Controls
{
    /// <summary>
    /// 表示控制基础数据。
    /// </summary>
    public abstract class ControlBase
    {
        /// <summary>
        /// 获取或设置缓动。
        /// </summary>
        [JsonProperty("easing")]
        public Easing Easing { get; set; } = new(1);

        /// <summary>
        /// 获取或设置横坐标。
        /// </summary>
        [JsonProperty("x")]
        public float X { get; set; }

        /// <summary>
        /// 创建当前对象的深克隆副本。
        /// </summary>
        /// <returns>当前对象的深克隆副本。</returns>
        public abstract ControlBase Clone();
    }
}
