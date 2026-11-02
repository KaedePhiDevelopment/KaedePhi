using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace KaedePhi.Core.PhiChain.v6
{
    /// <summary>
    /// 表示序列化判定线数据。
    /// </summary>
    public sealed class SerializedLine
    {
        // Rust uses serde(flatten) for line; v6 currently only contains name.
        /// <summary>
        /// 获取或设置名称。
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; } = "PhiChain Line";

        /// <summary>
        /// 表示判定线。
        /// </summary>
        [JsonIgnore]
        public Line Line
        {
            get => new() { Name = Name };
            set => Name = value.Name;
        }

        /// <summary>
        /// 获取或设置音符列表。
        /// </summary>
        [JsonProperty("notes")]
        public List<Note> Notes { get; set; } = new();

        /// <summary>
        /// 获取或设置事件列表。
        /// </summary>
        [JsonProperty("events")]
        public List<LineEvent> Events { get; set; } = new();

        /// <summary>
        /// 获取或设置子项。
        /// </summary>
        [JsonProperty("children")]
        public List<SerializedLine> Children { get; set; } = new();

        /// <summary>
        /// 获取或设置曲线音符轨道列表。
        /// </summary>
        [JsonProperty("curve_note_tracks")]
        public List<CurveNoteTrack> CurveNoteTracks { get; set; } = new();

        /// <summary>
        /// 深克隆当前 SerializedLine 对象
        /// </summary>
        public SerializedLine Clone()
        {
            return new SerializedLine
            {
                Name = Name,
                Notes = Notes.Select(n => n.Clone()).ToList(),
                Events = Events.Select(e => e.Clone()).ToList(),
                Children = Children.Select(c => c.Clone()).ToList(),
                CurveNoteTracks = CurveNoteTracks.Select(t => t.Clone()).ToList(),
            };
        }
    }
}
