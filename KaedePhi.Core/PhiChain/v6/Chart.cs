using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace KaedePhi.Core.PhiChain.v6
{
    [System.Obsolete(
        "已弃用：请迁移至 KaedePhi.Core.Formats.PhiChain.v6.Model 命名空间下的同名类型。"
    )]
    public sealed partial class Chart
    {
        /// <summary>
        /// 谱面格式标识。
        /// </summary>
        [JsonProperty("format")]
        public ulong Format { get; set; } = Constants.CurrentFormat;

        /// <summary>
        /// 谱面播放时使用的音频偏移量。
        /// </summary>
        [JsonProperty("offset")]
        public float Offset { get; set; }

        /// <summary>
        /// 谱面中的节拍速度变化列表。
        /// </summary>
        [JsonProperty("bpm_list")]
        public BpmList BpmList { get; set; } = new();

        /// <summary>
        /// 谱面包含的判定线列表。
        /// </summary>
        [JsonProperty("lines")]
        public List<SerializedLine> Lines { get; set; } = new();

        /// <summary>
        /// 坐标系边界
        /// </summary>
        public static Common.CoordinateSystem CoordinateSystem { get; } =
            new()
            {
                MaxX = 675f,
                MinX = -675f,
                MaxY = 450f,
                MinY = -450f,
                ClockwiseRotation = false,
            };

        /// <summary>
        /// 深克隆当前 Chart 对象
        /// </summary>
        public Chart Clone()
        {
            return new Chart
            {
                Format = Format,
                Offset = Offset,
                BpmList = BpmList.Clone(),
                Lines = Lines.Select(l => l.Clone()).ToList(),
            };
        }
    }
}
