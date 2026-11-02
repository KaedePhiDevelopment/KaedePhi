using System.Collections.Generic;
using Newtonsoft.Json;

namespace KaedePhi.Core.Formats.RePhiEdit.Model.Controls
{
    /// <summary>
    /// 表示尺寸控制数据。
    /// </summary>
    public class SizeControl : ControlBase
    {
        /// <summary>
        /// 获取或设置尺寸。
        /// </summary>
        [JsonProperty("size")]
        public float Size { get; set; } = 1.0f;

        private static readonly List<SizeControl> DefaultInstance = new()
        {
            new SizeControl
            {
                Easing = Easing.Linear,
                Size = 1.0f,
                X = 0.0f,
            },
            new SizeControl
            {
                Easing = Easing.Linear,
                Size = 1.0f,
                X = 9999999.0f,
            },
        };

        /// <summary>
        /// 获取默认。
        /// </summary>
        [JsonIgnore]
        public static List<SizeControl> Default =>
            DefaultInstance.ConvertAll(input => (SizeControl)input.Clone());

        /// <summary>
        /// 创建当前对象的深克隆副本。
        /// </summary>
        /// <returns>当前对象的深克隆副本。</returns>
        public override ControlBase Clone()
        {
            return new SizeControl
            {
                Easing = new Easing(Easing),
                X = X,
                Size = Size,
            };
        }
    }
}
