using System.Collections.Generic;
using Newtonsoft.Json;

namespace KaedePhi.Core.RePhiEdit.Controls
{
    /// <summary>
    /// 表示倾斜控制数据。
    /// </summary>
    public class SkewControl : ControlBase
    {
        /// <summary>
        /// 获取或设置倾斜。
        /// </summary>
        [JsonProperty("skew")]
        public float Skew { get; set; } = 1.0f;

        private static readonly List<SkewControl> DefaultInstance = new()
        {
            new SkewControl
            {
                Easing = Easing.Linear,
                Skew = 0.0f,
                X = 0.0f,
            },
            new SkewControl
            {
                Easing = Easing.Linear,
                Skew = 0.0f,
                X = 9999999.0f,
            },
        };

        /// <summary>
        /// 获取默认。
        /// </summary>
        [JsonIgnore]
        public static List<SkewControl> Default =>
            DefaultInstance.ConvertAll(input => (SkewControl)input.Clone());

        /// <summary>
        /// 创建当前对象的深克隆副本。
        /// </summary>
        /// <returns>当前对象的深克隆副本。</returns>
        public override ControlBase Clone()
        {
            return new SkewControl
            {
                Easing = new Easing(Easing),
                X = X,
                Skew = Skew,
            };
        }
    }
}
