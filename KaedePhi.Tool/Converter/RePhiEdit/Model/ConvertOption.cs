using KaedePhi.Tool.Common;

namespace KaedePhi.Tool.Converter.RePhiEdit.Model;

/// <summary>
/// 表示转换选项数据。
/// </summary>
public class ConvertOption
{
    /// <summary>
    /// 默认缓动切割精度
    /// </summary>
    public const int DefaultPrecision = Constants.DefaultPrecision;

    /// <summary>
    /// 获取或设置切割。
    /// </summary>
    public CuttingOptions Cutting { get; set; } = new();

    /// <summary>
    /// 提供切割相关的配置项。
    /// </summary>
    public class CuttingOptions
    {
        /// <summary>
        /// 非支持缓动切割精度，默认<see cref="DefaultPrecision"/>，值越大切割越精细，建议为2的倍数
        /// </summary>
        public int UnsupportedEasingPrecision { get; set; } = DefaultPrecision;
    }
}
