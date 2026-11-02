using System.Runtime.Serialization;
using KaedePhi.Core.Common;
using Newtonsoft.Json;

namespace KaedePhi.Core.PhiFans
{
    /// <summary>
    /// 表示音符数据。
    /// </summary>
    public class Note
    {
        private Beat _holdEndBeat = new(0);
        private bool _hasExplicitHoldEndBeat;

        /// <summary>
        /// 获取或设置类型。
        /// </summary>
        [JsonProperty("type")]
        public NoteType Type { get; set; } = NoteType.Tap;

        /// <summary>
        /// 获取或设置拍。
        /// </summary>
        [JsonProperty("beat")]
        public Beat Beat { get; set; } = new(0);

        /// <summary>
        /// 获取或设置位置横坐标。
        /// </summary>
        [JsonProperty("positionX")]
        public float PositionX { get; set; }

        /// <summary>
        /// 获取或设置速度。
        /// </summary>
        [JsonProperty("speed")]
        public float Speed { get; set; } = 1f;

        /// <summary>
        /// 获取或设置是否上方。
        /// </summary>
        [JsonProperty("isAbove")]
        public bool IsAbove { get; set; } = true;

        /// <summary>
        /// 表示长按结束拍。
        /// </summary>
        [JsonProperty("holdEndBeat")]
        public Beat HoldEndBeat
        {
            get => _holdEndBeat;
            set
            {
                _holdEndBeat = value;
                _hasExplicitHoldEndBeat = true;
            }
        }

        [JsonIgnore]
        internal bool HasExplicitHoldEndBeat => _hasExplicitHoldEndBeat;

        [OnDeserializing]
        private void OnDeserializing(StreamingContext context) => _hasExplicitHoldEndBeat = false;

        [OnDeserialized]
        private void OnDeserialized(StreamingContext context)
        {
            if (Type == NoteType.Hold && (!_hasExplicitHoldEndBeat || HoldEndBeat <= Beat))
                throw new JsonSerializationException("Hold 音符缺少有效的结束拍。");
        }
    }

    /// <summary>
    /// 定义谱面中音符的类型。
    /// </summary>
    public enum NoteType
    {
        /// <summary>
        /// 表示点击音符。
        /// </summary>
        Tap = 1,
        /// <summary>
        /// 表示长按音符。
        /// </summary>
        Hold = 3,
        /// <summary>
        /// 表示滑动音符。
        /// </summary>
        Flick = 4,
        /// <summary>
        /// 表示拖动音符。
        /// </summary>
        Drag = 2,
    }
}
