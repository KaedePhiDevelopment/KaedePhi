namespace KaedePhi.Core.KaedePhi
{
    public partial class Chart
    {
        /// <summary>
        /// 创建当前对象的深克隆副本。
        /// </summary>
        /// <returns>当前对象的深克隆副本。</returns>
        public Chart Clone()
        {
            return new Chart
            {
                BpmList = BpmList.ConvertAll(bpm => bpm.Clone()),
                Meta = Meta.Clone(),
                // 跳过空判定线，将空集合视为正常谱面
                JudgeLineList = JudgeLineList
                    .FindAll(judgeLine => judgeLine is not null)
                    .ConvertAll(judgeLine => judgeLine.Clone()),
            };
        }
    }
}
