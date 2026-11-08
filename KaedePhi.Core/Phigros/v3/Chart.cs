using System.Collections.Generic;
using Newtonsoft.Json;

namespace KaedePhi.Core.Phigros.v3
{
    /// <summary>
    /// 表示谱面数据。
    /// </summary>
    [System.Obsolete(
        "已弃用：请迁移至 KaedePhi.Core.Formats.Phigros.v3.Model 命名空间下的同名类型。"
    )]
    public partial class Chart
    {
        /// <summary>
        /// 格式版本号
        /// </summary>
        [JsonProperty("formatVersion")]
        public uint FormatVersion { get; set; } = 3;

        /// <summary>
        /// 谱面偏移，单位为秒
        /// </summary>
        [JsonProperty("offset")]
        public float Offset { get; set; }

        /// <summary>
        /// 判定线列表
        /// </summary>
        [JsonProperty("judgeLineList")]
        public List<JudgeLine> JudgeLineList { get; set; } = new();

        /// <summary>
        /// 坐标系边界
        /// </summary>
        public static class CoordinateSystem
        {
            /// <summary>
            /// 表示顺时针旋转常量。
            /// </summary>
            public const bool ClockwiseRotation = false;

            /// <summary>
            /// 表示最大横坐标常量。
            /// </summary>
            public const float MaxX = 1f;

            /// <summary>
            /// 表示最小横坐标常量。
            /// </summary>
            public const float MinX = 0f;

            /// <summary>
            /// 表示最大纵坐标常量。
            /// </summary>
            public const float MaxY = 1f;

            /// <summary>
            /// 表示最小纵坐标常量。
            /// </summary>
            public const float MinY = 0f;
        }
    }
}
