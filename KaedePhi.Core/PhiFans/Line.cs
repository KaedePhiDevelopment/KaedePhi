using System.Collections.Generic;
using Newtonsoft.Json;

namespace KaedePhi.Core.PhiFans
{
    /// <summary>
    /// 表示判定线数据。
    /// </summary>
    public class Line
    {
        /// <summary>
        /// 判定线事件组
        /// </summary>
        [JsonProperty("props")]
        public Props Props { get; set; } = new();

        /// <summary>
        /// 判定线音符列表
        /// </summary>
        [JsonProperty("notes")]
        public List<Note> NoteList { get; set; } = new();
    }
}
