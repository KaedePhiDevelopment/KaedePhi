using System.Runtime.Serialization;
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
}
