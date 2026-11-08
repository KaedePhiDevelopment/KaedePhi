using System.Globalization;

namespace KaedePhi.Core.PhiEdit
{
    /// <summary>
    /// 表示移动事件数据。
    /// </summary>
    public class MoveEvent
    {
        /// <summary>
        /// 获取或设置开始拍。
        /// </summary>
        public float StartBeat { get; set; }

        /// <summary>
        /// 获取或设置结束拍。
        /// </summary>
        public float EndBeat { get; set; }

        /// <summary>
        /// 获取或设置缓动类型。
        /// </summary>
        public Easing EasingType { get; set; } = new(1);

        /// <summary>
        /// 获取或设置结束横坐标数值。
        /// </summary>
        public float EndXValue { get; set; }

        /// <summary>
        /// 获取或设置结束纵坐标数值。
        /// </summary>
        public float EndYValue { get; set; }

        /// <summary>
        /// 获取某个拍在这个事件中的值
        /// </summary>
        /// <param name="beat">指定拍</param>
        /// <param name="startXValue">开始X坐标</param>
        /// <param name="startYValue">开始Y坐标</param>
        /// <returns>当前坐标（x,y）</returns>
        public (float, float) GetValueAtBeat(float beat, float startXValue, float startYValue)
        {
            //获得这个拍在这个事件的时间轴上的位置
            var t = (beat - StartBeat) / (EndBeat - StartBeat);
            var xValue = EasingType.Interpolate(startXValue, EndXValue, t);
            var yValue = EasingType.Interpolate(startYValue, EndYValue, t);
            return (xValue, yValue);
        }

        /// <summary>
        /// 调试用方法，不要调用，请改用<see cref="ToString(int)"/>
        /// </summary>
        public override string ToString() =>
            $"MoveEvent(StartBeat={StartBeat}, EndBeat={EndBeat}, EasingType={EasingType}, EndXValue={EndXValue}, EndYValue={EndYValue})";

        /// <summary>
        /// 用于将瞬时事件转换为PhiEditor Chart格式的字符串
        /// </summary>
        /// <param name="judgeLineIndex">判定线索引</param>
        /// <returns>PhiEditor Chart格式字符串</returns>
        public string ToString(int judgeLineIndex) =>
            string.Format(
                CultureInfo.InvariantCulture,
                "{0} {1} {2} {3} {4} {5} {6}",
                "cm",
                judgeLineIndex,
                StartBeat,
                EndBeat,
                EndXValue,
                EndYValue,
                (int)EasingType
            );

        /// <summary>
        /// 创建当前对象的深克隆副本。
        /// </summary>
        /// <returns>当前对象的深克隆副本。</returns>
        public MoveEvent Clone()
        {
            return new MoveEvent
            {
                StartBeat = StartBeat,
                EndBeat = EndBeat,
                EasingType = EasingType,
                EndXValue = EndXValue,
                EndYValue = EndYValue,
            };
        }
    }
}
