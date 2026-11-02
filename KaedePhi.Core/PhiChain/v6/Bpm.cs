using System;
using System.Collections.Generic;
using System.Linq;
using KaedePhi.Core.Common;
using KaedePhi.Core.PhiChain.v6.JsonConverter;
using Newtonsoft.Json;

namespace KaedePhi.Core.PhiChain.v6
{
    /// <summary>
    /// 表示节拍速度数据点。
    /// </summary>
    public sealed class BpmPoint
    {
        /// <summary>
        /// 此节拍速度点生效的拍位置。
        /// </summary>
        [JsonProperty("beat")]
        public Beat Beat { get; set; } = new(new[] { 0, 0, 1 });

        /// <summary>
        /// 此节拍位置生效的每分钟节拍数。
        /// </summary>
        [JsonProperty("bpm")]
        public float Bpm { get; set; } = 120f;

        /// <summary>
        /// 从谱面起始位置到该节拍点经过的时间。
        /// </summary>
        [JsonIgnore]
        public float Time { get; internal set; }

        /// <summary>
        /// 深克隆当前 BpmPoint 对象
        /// </summary>
        public BpmPoint Clone()
        {
            return new BpmPoint
            {
                Beat = new Beat((int[])Beat),
                Bpm = Bpm,
                Time = Time,
            };
        }
    }

    /// <summary>
    /// 表示节拍速度列表。
    /// </summary>
    [JsonConverter(typeof(BpmListJsonConverter))]
    public sealed class BpmList : List<BpmPoint>
    {
        /// <summary>
        /// 初始化节拍速度列表的新实例。
        /// </summary>
        public BpmList()
        {
            Add(new BpmPoint());
        }

        /// <summary>
        /// 初始化节拍速度列表的新实例。
        /// </summary>
        /// <param name="points">节拍速度点列表</param>
        public BpmList(IEnumerable<BpmPoint> points)
            : base(points)
        {
            if (Count == 0)
            {
                Add(new BpmPoint());
            }

            ComputeTimes();
        }

        /// <summary>
        /// 根据各节拍速度点计算对应的时间。
        /// </summary>
        public void ComputeTimes()
        {
            var time = 0f;
            var lastBeat = 0f;
            var lastBpm = -1f;
            for (var i = 0; i < Count; i++)
            {
                var point = this[i];
                if (Math.Abs(lastBpm - -1f) > float.Epsilon)
                {
                    time += (point.Beat - lastBeat) * (60f / lastBpm);
                }

                lastBeat = point.Beat;
                lastBpm = point.Bpm;
                point.Time = time;
            }
        }

        /// <summary>
        /// 深克隆当前 BpmList 对象
        /// </summary>
        public BpmList Clone()
        {
            var cloned = new BpmList(this.Select(p => p.Clone()));
            cloned.ComputeTimes();
            return cloned;
        }
    }
}
