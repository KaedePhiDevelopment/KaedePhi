using System.Runtime.Serialization;
using KaedePhi.Core.Formats.PhiChain.v6.Serialization.JsonConverter;
using KaedePhi.Core.Primitives;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace KaedePhi.Core.Formats.PhiChain.v6.Model
{
    /// <summary>
    /// 定义谱面中音符的类型。
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum NoteType
    {
        /// <summary>
        /// 表示点击音符。
        /// </summary>
        [EnumMember(Value = "tap")]
        Tap,

        /// <summary>
        /// 表示拖动音符。
        /// </summary>
        [EnumMember(Value = "drag")]
        Drag,

        /// <summary>
        /// 表示长按音符。
        /// </summary>
        [EnumMember(Value = "hold")]
        Hold,

        /// <summary>
        /// 表示滑动音符。
        /// </summary>
        [EnumMember(Value = "flick")]
        Flick,
    }

    /// <summary>
    /// 表示音符数据。
    /// </summary>
    [JsonConverter(typeof(NoteJsonConverter))]
    public sealed class Note
    {
        /// <summary>
        /// 获取或设置类型。
        /// </summary>
        [JsonIgnore]
        public NoteType Type { get; set; } = NoteType.Tap;

        /// <summary>
        /// 获取或设置长按持续拍数。
        /// </summary>
        [JsonIgnore]
        public Beat HoldBeat { get; set; } = new(new[] { 0, 0, 1 });

        /// <summary>
        /// 获取或设置是否启用上方。
        /// </summary>
        [JsonProperty("above")]
        public bool Above { get; set; }

        /// <summary>
        /// 获取或设置拍。
        /// </summary>
        [JsonProperty("beat")]
        public Beat Beat { get; set; } = new(new[] { 0, 0, 1 });

        /// <summary>
        /// 获取或设置横坐标。
        /// </summary>
        [JsonProperty("x")]
        public float X { get; set; }

        /// <summary>
        /// 获取或设置速度。
        /// </summary>
        [JsonProperty("speed")]
        public float Speed { get; set; } = 1f;

        /// <summary>
        /// 深克隆当前 Note 对象
        /// </summary>
        public Note Clone()
        {
            return new Note
            {
                Type = Type,
                HoldBeat = new Beat((int[])HoldBeat),
                Above = Above,
                Beat = new Beat((int[])Beat),
                X = X,
                Speed = Speed,
            };
        }
    }
}
