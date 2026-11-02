using System.Collections.Generic;
using Newtonsoft.Json;

namespace KaedePhi.Core.RePhiEdit.Controls
{
    /// <summary>
    /// 表示横坐标控制数据。
    /// </summary>
    public class XControl : ControlBase
    {
        /// <summary>
        /// 获取或设置位置。
        /// </summary>
        [JsonProperty("pos")]
        public float Pos { get; set; } = 1.0f;

        private static readonly List<XControl> DefaultInstance = new()
        {
            new XControl
            {
                Easing = Easing.Linear,
                Pos = 1.0f,
                X = 0.0f,
            },
            new XControl
            {
                Easing = Easing.Linear,
                Pos = 1.0f,
                X = 9999999.0f,
            },
        };

        /// <summary>
        /// 获取默认。
        /// </summary>
        [JsonIgnore]
        public static List<XControl> Default =>
            DefaultInstance.ConvertAll(input => (XControl)input.Clone());

        /// <summary>
        /// 创建当前对象的深克隆副本。
        /// </summary>
        /// <returns>当前对象的深克隆副本。</returns>
        public override ControlBase Clone()
        {
            return new XControl
            {
                Easing = new Easing(Easing),
                X = X,
                Pos = Pos,
            };
        }
    }
}
