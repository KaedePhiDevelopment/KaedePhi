using System.Runtime.Serialization;
using KaedePhi.Core.Primitives;
using Newtonsoft.Json;

namespace KaedePhi.Core.Formats.PhiChain.v6.Model
{
    /// <summary>
    /// 表示曲线音符轨道数据。
    /// </summary>
    public sealed class CurveNoteTrack
    {
        private Beat? _holdBeat;
        private bool _hasExplicitHoldBeat;

        /// <summary>
        /// 获取或设置来源。
        /// </summary>
        [JsonProperty("from")]
        public int From { get; set; }

        /// <summary>
        /// 获取或设置目标。
        /// </summary>
        [JsonProperty("to")]
        public int To { get; set; }

        /// <summary>
        /// 获取或设置音符类型。
        /// </summary>
        [JsonProperty("kind")]
        public NoteType NoteType { get; set; } = NoteType.Drag;

        /// <summary>
        /// 表示长按持续拍数。
        /// </summary>
        [JsonProperty("hold_beat", NullValueHandling = NullValueHandling.Ignore)]
        public Beat? HoldBeat
        {
            get => _holdBeat;
            set
            {
                _holdBeat = value;
                _hasExplicitHoldBeat = true;
            }
        }

        [JsonIgnore]
        internal bool HasExplicitHoldBeat => _hasExplicitHoldBeat;

        /// <summary>
        /// 获取或设置密度。
        /// </summary>
        [JsonProperty("density")]
        public uint Density { get; set; } = 16;

        /// <summary>
        /// 获取或设置曲线。
        /// </summary>
        [JsonProperty("curve")]
        public Easing Curve { get; set; } = Easing.Linear;

        [OnDeserializing]
        private void OnDeserializing(StreamingContext context) => _hasExplicitHoldBeat = false;

        [OnDeserialized]
        private void OnDeserialized(StreamingContext context)
        {
            if (
                NoteType == NoteType.Hold
                && (!_hasExplicitHoldBeat || HoldBeat is null || HoldBeat <= new Beat(0))
            )
                throw new JsonSerializationException("Hold 曲线音符缺少有效的持续拍。");
        }

        /// <summary>
        /// 深克隆当前 CurveNoteTrack 对象
        /// </summary>
        public CurveNoteTrack Clone()
        {
            return new CurveNoteTrack
            {
                From = From,
                To = To,
                NoteType = NoteType,
                HoldBeat = HoldBeat != null ? new Beat((int[])HoldBeat) : null,
                Density = Density,
                Curve = Curve.Clone(),
                _hasExplicitHoldBeat = _hasExplicitHoldBeat,
            };
        }
    }
}
