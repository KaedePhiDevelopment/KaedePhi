using KaedePhi.Tool.Common;
using KaedePhi.Tool.Converter.PhiEdit.Model;
using KaedePhi.Tool.Converter.PhiFans.Model;
using KaedePhi.Tool.Converter.Phigros.v3.Model;

namespace KaedePhi.Tool.App.Cli.Infrastructure;

/// <summary>
/// <see cref="ChartService.SaveAsAsync"/> 的导出选项。
/// </summary>
public sealed record SaveAsOptions
{
    /// <summary>是否使用流式写入（适合大文件）。</summary>
    public bool Stream { get; init; }

    /// <summary>是否格式化 JSON 输出。</summary>
    public bool Format { get; init; }

    /// <summary>演习模式：不实际写文件，仅返回目标路径。</summary>
    public bool DryRun { get; init; }

    /// <summary>PhiEdit 转换选项。</summary>
    public IrToPhiEditConvertOptions? PhiEditOptions { get; init; }

    /// <summary>Phigros v3 转换选项。</summary>
    public IrToPhigrosV3ConvertOptions? PhigrosOptions { get; init; }

    /// <summary>PhiFans 转换选项。</summary>
    public IrToPhiFansConvertOptions? PhiFansOptions { get; init; }

    /// <summary>其他格式的转换选项，按目标格式自行匹配类型。</summary>
    public object? ExportOptions { get; init; }

    /// <summary>
    /// 挑选与目标格式匹配的转换选项。
    /// </summary>
    /// <param name="target">目标格式</param>
    /// <returns>选项实例，无匹配时返回 <see langword="null"/> 以使用格式默认值</returns>
    internal object? ResolveFor(string target) =>
        target switch
        {
            ChartFormatIds.PhiEdit => PhiEditOptions ?? ExportOptions,
            ChartFormatIds.PhigrosV3 => PhigrosOptions ?? ExportOptions,
            ChartFormatIds.PhiFans => PhiFansOptions ?? ExportOptions,
            _ => ExportOptions,
        };
}
