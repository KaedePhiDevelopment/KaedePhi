using Newtonsoft.Json;

namespace KaedePhi.Core.Formats.PhiFans.Model
{
    /// <summary>
    /// 表示信息数据。
    /// </summary>
    public class Info
    {
        /// <summary>
        /// 获取或设置名称。
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; } = ""; // 曲名

        /// <summary>
        /// 获取或设置谱师。
        /// </summary>
        [JsonProperty("artist")]
        public string Artist { get; set; } = ""; // 曲师

        /// <summary>
        /// 获取或设置插图。
        /// </summary>
        [JsonProperty("illustration")]
        public string Illustration { get; set; } = ""; // 插画

        /// <summary>
        /// 获取或设置层级。
        /// </summary>
        [JsonProperty("level")]
        public string Level { get; set; } = ""; // 等级

        /// <summary>
        /// 获取或设置设计器。
        /// </summary>
        [JsonProperty("designer")]
        public string Designer { get; set; } = ""; // 谱师
    }
}
