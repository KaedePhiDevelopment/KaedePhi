using System.Runtime.Serialization;
using KaedePhi.Core.Formats.PhiChain.v6.Serialization.JsonConverter;
using KaedePhi.Core.Primitives;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace KaedePhi.Core.Formats.PhiChain.v6.Model
{
    /// <summary>
    /// 定义判定线事件类型的可选值。
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum LineEventType
    {
        /// <summary>
        /// 表示横坐标类型。
        /// </summary>
        [EnumMember(Value = "x")]
        X,

        /// <summary>
        /// 表示纵坐标类型。
        /// </summary>
        [EnumMember(Value = "y")]
        Y,

        /// <summary>
        /// 表示旋转类型。
        /// </summary>
        [EnumMember(Value = "rotation")]
        Rotation,

        /// <summary>
        /// 表示不透明度类型。
        /// </summary>
        [EnumMember(Value = "opacity")]
        Opacity,

        /// <summary>
        /// 表示速度类型。
        /// </summary>
        [EnumMember(Value = "speed")]
        Speed,
    }

    /// <summary>
    /// 定义判定线事件数值类型的可选值。
    /// </summary>
    public enum LineEventValueType
    {
        /// <summary>
        /// 表示过渡类型。
        /// </summary>
        Transition,

        /// <summary>
        /// 表示常量类型。
        /// </summary>
        Constant,
    }

    /// <summary>
    /// 表示判定线事件数值数据。
    /// </summary>
    [JsonConverter(typeof(LineEventValueJsonConverter))]
    public sealed class LineEventValue
    {
        /// <summary>
        /// 获取或设置类型。
        /// </summary>
        [JsonIgnore]
        public LineEventValueType Type { get; set; } = LineEventValueType.Constant;

        /// <summary>
        /// 获取或设置开始。
        /// </summary>
        [JsonIgnore]
        public float Start { get; set; }

        /// <summary>
        /// 获取或设置结束。
        /// </summary>
        [JsonIgnore]
        public float End { get; set; }

        /// <summary>
        /// 获取或设置缓动。
        /// </summary>
        [JsonIgnore]
        public Easing Easing { get; set; } = Easing.Linear;

        /// <summary>
        /// 获取或设置数值。
        /// </summary>
        [JsonIgnore]
        public float Value { get; set; }

        /// <summary>
        /// 执行过渡操作。
        /// </summary>
        /// <param name="start">起始数值</param>
        /// <param name="end">结束数值</param>
        /// <param name="easing">缓动配置。</param>
        /// <returns>操作得到的结果。</returns>
        public static LineEventValue Transition(float start, float end, Easing? easing)
        {
            return new LineEventValue
            {
                Type = LineEventValueType.Transition,
                Start = start,
                End = end,
                Easing = easing ?? Easing.Linear,
            };
        }

        /// <summary>
        /// 执行常量操作。
        /// </summary>
        /// <param name="value">待处理的数值</param>
        /// <returns>操作得到的结果。</returns>
        public static LineEventValue Constant(float value)
        {
            return new LineEventValue { Type = LineEventValueType.Constant, Value = value };
        }

        /// <summary>
        /// 深克隆当前 LineEventValue 对象
        /// </summary>
        public LineEventValue Clone()
        {
            return new LineEventValue
            {
                Type = Type,
                Start = Start,
                End = End,
                Easing = Easing.Clone(),
                Value = Value,
            };
        }
    }

    /// <summary>
    /// 表示判定线事件数据。
    /// </summary>
    public sealed class LineEvent
    {
        /// <summary>
        /// 获取或设置类型。
        /// </summary>
        [JsonProperty("kind")]
        public LineEventType Type { get; set; }

        /// <summary>
        /// 获取或设置开始拍。
        /// </summary>
        [JsonProperty("start_beat")]
        public Beat StartBeat { get; set; } = new(new[] { 0, 0, 1 });

        /// <summary>
        /// 获取或设置结束拍。
        /// </summary>
        [JsonProperty("end_beat")]
        public Beat EndBeat { get; set; } = new(new[] { 1, 0, 1 });

        /// <summary>
        /// 获取或设置数值。
        /// </summary>
        [JsonProperty("value")]
        public LineEventValue Value { get; set; } = LineEventValue.Constant(0f);

        /// <summary>
        /// 深克隆当前 LineEvent 对象
        /// </summary>
        public LineEvent Clone()
        {
            return new LineEvent
            {
                Type = Type,
                StartBeat = new Beat((int[])StartBeat),
                EndBeat = new Beat((int[])EndBeat),
                Value = Value.Clone(),
            };
        }
    }
}
