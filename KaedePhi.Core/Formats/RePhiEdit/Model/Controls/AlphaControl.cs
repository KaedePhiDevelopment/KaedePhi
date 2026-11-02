using System.Collections.Generic;
using Newtonsoft.Json;

namespace KaedePhi.Core.Formats.RePhiEdit.Model.Controls
{
    /// <summary>
    /// 表示不透明度控制数据。
    /// </summary>
    public class AlphaControl : ControlBase
    {
        /// <summary>
        /// 获取或设置不透明度。
        /// </summary>
        [JsonProperty("alpha")]
        public float Alpha { get; set; } = 1.0f;

        private static readonly List<AlphaControl> DefaultInstance = new()
        {
            new AlphaControl
            {
                Easing = Easing.Linear,
                Alpha = 1.0f,
                X = 0.0f,
            },
            new AlphaControl
            {
                Easing = Easing.Linear,
                Alpha = 1.0f,
                X = 9999999.0f,
            },
        };

        /// <summary>
        /// 获取默认。
        /// </summary>
        [JsonIgnore]
        public static List<AlphaControl> Default =>
            DefaultInstance.ConvertAll(input => (AlphaControl)input.Clone());

        /// <summary>
        /// 创建当前对象的深克隆副本。
        /// </summary>
        /// <returns>当前对象的深克隆副本。</returns>
        public override ControlBase Clone()
        {
            return new AlphaControl
            {
                Easing = new Easing(Easing),
                X = X,
                Alpha = Alpha,
            };
        }
    }
}
