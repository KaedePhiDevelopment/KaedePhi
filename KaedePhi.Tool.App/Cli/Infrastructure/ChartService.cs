using KaedePhi.Tool.App.Shared;
using KaedePhi.Tool.Common;
using KaedePhi.Tool.Converter;
using KaedePhi.Tool.Converter.RePhiEdit.Model;
using Chart = KaedePhi.Core.Intermediate.Model.Chart;

namespace KaedePhi.Tool.App.Cli.Infrastructure;

/// <summary>
/// 谱面加载、格式检测与导出服务。格式相关的转换细节委托给 <see cref="ChartFormatRegistry"/>。
/// </summary>
public sealed class ChartService
{
    private readonly WorkspaceService _workspace = new();

    /// <summary>从文件路径或工作区加载原始文本。</summary>
    public async Task<string> LoadChartTextAsync(
        string? input,
        string? workspace,
        CancellationToken ct = default
    )
    {
        var path = ResolveInputPath(input, workspace);
        InputFileValidator.Validate(path);
        return await File.ReadAllTextAsync(path, ct);
    }

    /// <summary>将输入谱面统一转换为中间类型，格式不支持导入时返回 <see langword="null"/>。</summary>
    public async Task<Chart?> LoadKpcAsync(
        string? input,
        string? workspace,
        CancellationToken ct = default
    )
    {
        var path = ResolveInputPath(input, workspace);
        InputFileValidator.Validate(path);

        var descriptor = await ChartImporter.DetectFormatAsync(path, null, ct);
        if (!descriptor.CanImport)
            return null;

        return await ChartImporter.ImportAsync(
            path,
            descriptor,
            preferStream: true,
            importOptions: null,
            log: null,
            progress: null,
            ct
        );
    }

    private string ResolveInputPath(string? input, string? workspace)
    {
        if (!string.IsNullOrWhiteSpace(input) && !string.IsNullOrWhiteSpace(workspace))
            throw new InvalidOperationException(CliLocalizationString.err_input_workspace_conflict);

        if (!string.IsNullOrWhiteSpace(workspace))
            return _workspace.GetChartPath(workspace)
                ?? throw new InvalidOperationException(
                    string.Format(CliLocalizationString.err_workspace_missing, workspace)
                );

        return input
            ?? throw new InvalidOperationException(CliLocalizationString.err_input_required);
    }

    /// <summary>根据输入路径或工作区自动计算输出路径。</summary>
    public string ResolveOutputPath(
        string? input,
        string? output,
        string? workspace,
        string extension = ".json"
    )
    {
        if (!string.IsNullOrWhiteSpace(input) && !string.IsNullOrWhiteSpace(workspace))
            throw new InvalidOperationException(CliLocalizationString.err_input_workspace_conflict);

        if (!string.IsNullOrWhiteSpace(output))
        {
            var sourcePath = !string.IsNullOrWhiteSpace(workspace)
                ? _workspace.GetChartPath(workspace)
                : input;
            if (ChartFiles.IsSamePath(sourcePath, output))
                throw new InvalidOperationException(CliLocalizationString.err_output_input_same);
            return output;
        }

        if (!string.IsNullOrWhiteSpace(workspace))
            return Path.Combine(_workspace.Root, workspace, "chart" + extension);
        if (string.IsNullOrEmpty(input))
            throw new InvalidOperationException(CliLocalizationString.err_input_required);
        return Path.Combine(
            Path.GetDirectoryName(input) ?? ".",
            Path.GetFileNameWithoutExtension(input) + "_KaedePhi" + extension
        );
    }

    /// <summary>将 KPC 谱面导出为 RPE 格式并写入。</summary>
    public static async Task<string> SaveAsRpeAsync(
        Chart chart,
        string outputPath,
        bool dryRun,
        CancellationToken ct = default
    )
    {
        await ChartFormatRegistry
            .Get(ChartFormatIds.RePhiEdit)
            .ExportAsync(
                chart,
                outputPath,
                new ChartWriteSettings { DryRun = dryRun },
                exportOptions: new ConvertOption(),
                ct: ct
            );
        return outputPath;
    }

    /// <summary>将 KPC 谱面导出为目标格式并写入，格式不支持导出时返回 <see langword="null"/>。</summary>
    public static async Task<string?> SaveAsAsync(
        Chart chart,
        string outputPath,
        string target,
        SaveAsOptions options,
        CancellationToken ct = default
    )
    {
        var descriptor = ChartFormatRegistry.Find(target);
        if (descriptor is not { CanExport: true })
            return null;

        await descriptor.ExportAsync(
            chart,
            outputPath,
            new ChartWriteSettings
            {
                UseStream = options.Stream,
                Indented = options.Format,
                DryRun = options.DryRun,
            },
            options.ResolveFor(descriptor.FormatId),
            ct: ct
        );
        return outputPath;
    }
}
