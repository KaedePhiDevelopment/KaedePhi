using System;

namespace KaedePhi.Tool.Common;

/// <summary>
/// 内置谱面格式的兼容枚举；格式注册与扩展使用字符串标识。
/// </summary>
[Obsolete(
    "将在下一个大版本移除；请使用 ChartFormatDescriptor.FormatId 和 ChartFormatCatalog 管理格式。"
)]
public enum ChartType
{
    /// <summary>
    /// PhiEdit 格式。
    /// </summary>
    PhiEdit,

    /// <summary>
    /// RePhiEdit 格式。
    /// </summary>
    RePhiEdit,

    /// <summary>
    /// Phigros V1 格式。
    /// </summary>
    PhigrosV1,

    /// <summary>
    /// Phigros V3 格式。
    /// </summary>
    PhigrosV3,

    /// <summary>
    /// PhiChain 格式。
    /// </summary>
    PhiChain,

    /// <summary>
    /// PhiFans 格式。
    /// </summary>
    PhiFans,

    /// <summary>
    /// StellateRePhiEditExtended 格式。
    /// </summary>
    StellateRePhiEditExtended,
}
