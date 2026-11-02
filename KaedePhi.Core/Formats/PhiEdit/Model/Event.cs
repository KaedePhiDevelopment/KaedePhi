using System;
using System.Globalization;

namespace KaedePhi.Core.Formats.PhiEdit.Model
{
    /// <summary>
    /// 表示事件数据。
    /// </summary>
    public class Event
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
        /// 获取或设置结束数值。
        /// </summary>
        public float EndValue { get; set; }

        /// <summary>
        /// 获取某个拍在这个事件中的值
        /// </summary>
        /// <param name="beat">指定拍</param>
        /// <param name="startValue">开始数值</param>
        /// <returns>当前数值</returns>
        public float GetValueAtBeat(float beat, float startValue)
        {
            //获得这个拍在这个事件的时间轴上的位置
            var t = (beat - StartBeat) / (EndBeat - StartBeat);
            return EasingType.Interpolate(startValue, EndValue, t);
        }

        /// <summary>
        /// 调试用方法，不要调用，请改用<see cref="ToString(int, string)"/>
        /// </summary>
        public override string ToString() =>
            $"Event(StartBeat={StartBeat}, EndBeat={EndBeat}, EasingType={EasingType}, EndValue={EndValue})";

        /// <summary>
        /// 用于将瞬时事件转换为PhiEditor Chart格式的字符串
        /// </summary>
        /// <param name="judgeLineIndex">判定线索引</param>
        /// <param name="head">格式头</param>
        /// <returns>PhiEditer Chart格式字符串</returns>
        public string ToString(int judgeLineIndex, string head)
        {
            if (head is "cm" or "cp")
                throw new ArgumentException(
                    "请使用 MoveEvent 或 MoveFrame 的 ToString 方法，这不是一个 MoveEvent 或 MoveFrame"
                );
            return head != "cf"
                ? string.Format(
                    CultureInfo.InvariantCulture,
                    "{0} {1} {2} {3} {4} {5}",
                    head,
                    judgeLineIndex,
                    StartBeat,
                    EndBeat,
                    EndValue,
                    (int)EasingType
                )
                : string.Format(
                    CultureInfo.InvariantCulture,
                    "{0} {1} {2} {3} {4}",
                    head,
                    judgeLineIndex,
                    StartBeat,
                    EndBeat,
                    EndValue
                );
        }

        /// <summary>
        /// 创建当前对象的深克隆副本。
        /// </summary>
        /// <returns>当前对象的深克隆副本。</returns>
        public Event Clone()
        {
            return new Event
            {
                StartBeat = StartBeat,
                EndBeat = EndBeat,
                EasingType = EasingType,
                EndValue = EndValue,
            };
        }
    }
}
