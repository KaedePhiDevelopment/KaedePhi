using Newtonsoft.Json;

namespace KaedePhi.Core.Formats.Phigros.v3.Model
{
    /// <summary>
    /// 速度事件，描述一段时间的速度变化。
    /// </summary>
    public readonly struct SpeedEvent
    {
        /// <summary>
        /// 事件开始时间
        /// </summary>
        [JsonProperty("startTime")]
        public float StartTime { get; init; }

        /// <summary>
        /// 事件结束时间
        /// </summary>
        [JsonProperty("endTime")]
        public float EndTime { get; init; }

        /// <summary>
        /// 事件值
        /// </summary>
        [JsonProperty("value")]
        public float Value { get; init; }

        /// <summary>
        /// 创建速度事件。
        /// </summary>
        /// <param name="startTime">事件开始时间</param>
        /// <param name="endTime">事件结束时间</param>
        /// <param name="value">事件值</param>
        public SpeedEvent(float startTime, float endTime, float value)
        {
            StartTime = startTime;
            EndTime = endTime;
            Value = value;
        }
    }
}
