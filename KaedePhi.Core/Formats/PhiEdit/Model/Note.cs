using System;
using System.Globalization;

namespace KaedePhi.Core.Formats.PhiEdit.Model
{
    /// <summary>
    /// 表示音符数据。
    /// </summary>
    public class Note
    {
        /// <summary>
        /// 获取或设置类型。
        /// </summary>
        public NoteType Type { get; set; } = NoteType.Tap;
        /// <summary>
        /// 获取或设置开始拍。
        /// </summary>
        public float StartBeat { get; set; }
        /// <summary>
        /// 获取或设置结束拍。
        /// </summary>
        public float EndBeat { get; set; } // 仅 Hold 使用
        /// <summary>
        /// 获取或设置是否启用上方。
        /// </summary>
        public bool Above { get; set; } = true;
        /// <summary>
        /// 获取或设置位置横坐标。
        /// </summary>
        public float PositionX { get; set; }
        /// <summary>
        /// 获取或设置宽度比例。
        /// </summary>
        public float WidthRatio { get; set; } = 1.0f;
        /// <summary>
        /// 获取或设置是否伪。
        /// </summary>
        public bool IsFake { get; set; }
        /// <summary>
        /// 获取或设置速度倍率。
        /// </summary>
        public float SpeedMultiplier { get; set; } = 1.0f;

        /// <summary>
        /// 调试用方法，不要调用，请改用<see cref="ToString(int)"/>
        /// </summary>
        public override string ToString() =>
            $"Note(Type: {Type}, StartBeat: {StartBeat}, EndBeat: {EndBeat}, Above: {Above}, PositionX: {PositionX}, WidthRatio: {WidthRatio}, IsFake: {IsFake}, SpeedMultiplier: {SpeedMultiplier})";

        /// <summary>
        /// 用于将瞬时事件转换为PhiEditor Chart格式的字符串
        /// </summary>
        /// <param name="judgeLineIndex">判定线索引</param>
        /// <returns>PhiEditor Chart格式字符串</returns>
        /// <exception cref="ArgumentException">存储的数值有误</exception>
        public string ToString(int judgeLineIndex)
        {
            var (noteLine, speedLine, widthLine) = GetExportParts(judgeLineIndex);
            return string.Join(Environment.NewLine, noteLine, speedLine, widthLine);
        }

        /// <summary>
        /// 获取 Note 导出的三条物理文本行，供字符串导出和流式导出共享。
        /// </summary>
        /// <param name="judgeLineIndex">判定线索引。</param>
        /// <returns>Note 主指令、速度倍率行和宽度比例行。</returns>
        /// <exception cref="ArgumentException">存储的数值有误。</exception>
        internal (string noteLine, string speedLine, string widthLine) GetExportParts(
            int judgeLineIndex
        )
        {
            const int fakeNote = 1;
            const int realNote = 0;
            const int aboveNote = 1;
            const int belowNote = 2;

            if (Type != NoteType.Hold && Math.Abs(StartBeat - EndBeat) > 0.0001f)
                throw new ArgumentException("非Hold音符的开始拍与结束拍应相等");

            var aboveNumber = Above ? aboveNote : belowNote;
            var isFakeNumber = IsFake ? fakeNote : realNote;
            var noteLine =
                Type == NoteType.Hold
                    ? string.Format(
                        CultureInfo.InvariantCulture,
                        "{0} {1} {2} {3} {4} {5} {6}",
                        $"n{(int)Type}",
                        judgeLineIndex,
                        StartBeat,
                        EndBeat,
                        PositionX,
                        aboveNumber,
                        isFakeNumber
                    )
                    : string.Format(
                        CultureInfo.InvariantCulture,
                        "{0} {1} {2} {3} {4} {5}",
                        $"n{(int)Type}",
                        judgeLineIndex,
                        StartBeat,
                        PositionX,
                        aboveNumber,
                        isFakeNumber
                    );

            return (
                noteLine,
                string.Format(CultureInfo.InvariantCulture, "{0} {1}", "#", SpeedMultiplier),
                string.Format(CultureInfo.InvariantCulture, "{0} {1}", "&", WidthRatio)
            );
        }

        /// <summary>
        /// 创建当前对象的深克隆副本。
        /// </summary>
        /// <returns>当前对象的深克隆副本。</returns>
        public Note Clone()
        {
            return new Note
            {
                Type = Type,
                StartBeat = StartBeat,
                EndBeat = EndBeat,
                Above = Above,
                PositionX = PositionX,
                WidthRatio = WidthRatio,
                IsFake = IsFake,
                SpeedMultiplier = SpeedMultiplier,
            };
        }
    }

    /// <summary>
    /// 定义谱面中音符的类型。
    /// </summary>
    public enum NoteType
    {
        /// <summary>
        /// 表示点击音符。
        /// </summary>
        Tap = 1,
        /// <summary>
        /// 表示长按音符。
        /// </summary>
        Hold = 2,
        /// <summary>
        /// 表示滑动音符。
        /// </summary>
        Flick = 3,
        /// <summary>
        /// 表示拖动音符。
        /// </summary>
        Drag = 4,
    }
}
