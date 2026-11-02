using Newtonsoft.Json;

namespace KaedePhi.Core.Formats.PhiChain.v6.Model
{
    /// <summary>
    /// 表示判定线数据。
    /// </summary>
    public sealed class Line
    {
        /// <summary>
        /// 获取或设置名称。
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; } = "Unnamed Line";
    }
}
