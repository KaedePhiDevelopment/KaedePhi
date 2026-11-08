using System.Collections.Generic;
using System.Linq;

namespace KaedePhi.Core.PhiEdit
{
    /// <summary>
    /// 表示谱面数据。
    /// </summary>
    [System.Obsolete("已弃用：请迁移至 KaedePhi.Core.Formats.PhiEdit.Model 命名空间下的同名类型。")]
    public partial class Chart
    {
        /// <summary>
        /// 谱面偏移，单位为毫秒
        /// </summary>
        public int Offset { get; set; }

        /// <summary>
        /// 坐标系边界
        /// </summary>
        public static class CoordinateSystem
        {
            /// <summary>
            /// 表示最大横坐标常量。
            /// </summary>
            public const float MaxX = 2048f;

            /// <summary>
            /// 表示最小横坐标常量。
            /// </summary>
            public const float MinX = 0f;

            /// <summary>
            /// 表示最大纵坐标常量。
            /// </summary>
            public const float MaxY = 1400f;

            /// <summary>
            /// 表示最小纵坐标常量。
            /// </summary>
            public const float MinY = 0f;

            /// <summary>
            /// 表示顺时针旋转常量。
            /// </summary>
            public const bool ClockwiseRotation = true;
        }

        /// <summary>
        /// 判定线列表
        /// </summary>
        public List<JudgeLine> JudgeLineList { get; set; } = new();

        /// <summary>
        /// BPM列表
        /// </summary>
        public List<BpmItem> BpmList { get; set; } = new();

        /// <summary>
        /// 创建当前对象的深克隆副本。
        /// </summary>
        /// <returns>当前对象的深克隆副本。</returns>
        public Chart Clone()
        {
            var clonedChart = new Chart
            {
                Offset = Offset,
                BpmList = BpmList.Select(b => b.Clone()).ToList(),
                JudgeLineList = JudgeLineList.Select(jl => jl.Clone()).ToList(),
            };
            return clonedChart;
        }
    }
}
