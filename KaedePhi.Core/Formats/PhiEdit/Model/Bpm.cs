using System;
using System.Globalization;

namespace KaedePhi.Core.Formats.PhiEdit.Model
{
    /// <summary>
    /// 单个BPM变化点
    /// </summary>
    public class BpmItem
    {
        private float _bpm = 120f;

        /// <summary>
        /// 表示节拍速度。
        /// </summary>
        public float Bpm
        {
            get => _bpm;
            set
            {
                if (value <= 0 || float.IsInfinity(value) || float.IsNaN(value))
                    throw new ArgumentException(Resources.illegal_bpm_infinity_or_negative);
                _bpm = value;
            }
        }

        /// <summary>
        /// 获取或设置开始拍。
        /// </summary>
        public float StartBeat { get; set; }

        /// <summary>
        /// 返回当前对象的字符串表示。
        /// </summary>
        /// <returns>当前对象的字符串表示。</returns>
        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "{0} {1} {2}", "bp", StartBeat, Bpm);
        }

        /// <summary>
        /// 创建当前对象的深克隆副本。
        /// </summary>
        /// <returns>当前对象的深克隆副本。</returns>
        public BpmItem Clone()
        {
            return new BpmItem { Bpm = Bpm, StartBeat = StartBeat };
        }
    }
}
