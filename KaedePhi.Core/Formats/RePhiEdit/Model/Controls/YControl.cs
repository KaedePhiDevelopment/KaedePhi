using System.Collections.Generic;
using Newtonsoft.Json;

namespace KaedePhi.Core.Formats.RePhiEdit.Model.Controls
{
    /// <summary>
    /// 表示纵坐标控制数据。
    /// </summary>
    public class YControl : ControlBase
    {
        /// <summary>
        /// 获取或设置纵坐标。
        /// </summary>
        [JsonProperty("y")]
        public float Y { get; set; } = 1.0f;

        private static readonly List<YControl> DefaultInstance = new()
        {
            new YControl
            {
                Easing = Easing.Linear,
                Y = 1.0f,
                X = 0.0f,
            },
            new YControl
            {
                Easing = Easing.Linear,
                Y = 1.0f,
                X = 9999999.0f,
            },
        };

        /// <summary>
        /// 获取默认。
        /// </summary>
        [JsonIgnore]
        public static List<YControl> Default =>
            DefaultInstance.ConvertAll(input => (YControl)input.Clone());

        /// <summary>
        /// 创建当前对象的深克隆副本。
        /// </summary>
        /// <returns>当前对象的深克隆副本。</returns>
        public override ControlBase Clone()
        {
            return new YControl
            {
                Easing = new Easing(Easing),
                X = X,
                Y = Y,
            };
        }
    }
}
