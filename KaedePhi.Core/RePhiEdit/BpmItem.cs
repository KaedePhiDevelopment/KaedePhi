using KaedePhi.Core.Common;
using Newtonsoft.Json;

namespace KaedePhi.Core.RePhiEdit
{
    /// <summary>
    /// 表示节拍速度项数据。
    /// </summary>
    public class BpmItem
    {
        /// <summary>
        /// 获取或设置节拍速度。
        /// </summary>
        [JsonProperty("bpm")]
        public float Bpm { get; set; } = 120f;

        /// <summary>
        /// 获取或设置开始拍。
        /// </summary>
        [JsonProperty("startTime")]
        public Beat StartBeat { get; set; } = new(BeatArray);
        private static readonly int[] BeatArray = { 0, 0, 1 };

        /// <summary>
        /// 创建当前对象的深克隆副本。
        /// </summary>
        /// <returns>当前对象的深克隆副本。</returns>
        public BpmItem Clone()
        {
            return new BpmItem { Bpm = Bpm, StartBeat = new Beat((int[])StartBeat) };
        }
    }
}
