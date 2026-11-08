using Avalonia.Controls;
using Avalonia.Platform.Storage;
using KaedePhi.Tool.App.Gui.Services;
using KaedePhi.Tool.App.Gui.ViewModels;
using KaedePhi.Tool.App.Gui.Views;
using KaedePhi.Tool.App.Shared;
using KaedePhi.Tool.Common;
using KaedePhi.Tool.Converter;
using static KaedePhi.Tool.Localization.GuiLocalizationString;

namespace KaedePhi.Tool.App.Gui.Controllers;

internal sealed class ExportController : ControllerBase
{
    private readonly GuiNavigationService _navigation;
    private readonly GuiChartService _chart;

    public ExportController(
        GuiNavigationService navigation,
        GuiChartService chart,
        LogService logService,
        Window window,
        Action returnToImport
    )
        : base(logService.ForContext<ExportController>(), logService, window)
    {
        _navigation = navigation;
        _chart = chart;

        _navigation.Export.RequestExport += OnExportExecute;
        _navigation.Export.RequestReturnToImport += returnToImport;
        _navigation.Export.RequestCancelExport += OnCancelExport;
    }

    private async void OnExportExecute()
    {
        var exportVm = _navigation.Export;
        if (exportVm.IsExporting)
            return;

        try
        {
            var targetFormat =
                exportVm.SelectedFormat ?? throw new InvalidOperationException("未选择导出格式。");
            var outputPath = await PickOutputPathAsync(exportVm, targetFormat);
            if (outputPath == null)
                return;

            if (ChartFiles.IsSamePath(_chart.SourceFilePath, outputPath))
            {
                exportVm.StatusText = export_same_file_error;
                return;
            }

            await ExportAsync(exportVm, targetFormat, outputPath);
        }
        catch (OperationCanceledException)
        {
            ReportCancelled(exportVm);
        }
        catch (Exception ex)
        {
            exportVm.StatusText = ReportFailure(log_export_failed, export_error_title, ex);
        }
        finally
        {
            exportVm.IsExporting = false;
            EndOperation();
        }
    }

    private void OnCancelExport()
    {
        if (TryCancelOperation())
            ReportCancelled(_navigation.Export);
    }

    // 返回用户选择的输出路径；无法选择或用户取消时返回 null，并已更新状态文本
    private async Task<string?> PickOutputPathAsync(ExportViewModel exportVm, string targetFormat)
    {
        var topLevel = TopLevel.GetTopLevel(OwnerWindow);
        if (topLevel == null)
        {
            exportVm.StatusText = status_cannot_access_picker;
            return null;
        }

        var (ext, typeLabel) = GetFormatFileInfo(targetFormat);
        var file = await topLevel.StorageProvider.SaveFilePickerAsync(
            new FilePickerSaveOptions
            {
                Title = export_title,
                SuggestedFileName = $"export_{targetFormat}.{ext}",
                DefaultExtension = ext,
                FileTypeChoices = [new FilePickerFileType(typeLabel) { Patterns = [$"*.{ext}"] }],
            }
        );
        if (file == null)
        {
            ReportCancelled(exportVm);
            return null;
        }

        var outputPath = file.TryGetLocalPath();
        if (string.IsNullOrEmpty(outputPath))
            return null;

        // 若 OS 未自动附加扩展名，则手动补全
        var expectedExt = $".{ext}";
        return outputPath.EndsWith(expectedExt, StringComparison.OrdinalIgnoreCase)
            ? outputPath
            : outputPath + expectedExt;
    }

    private async Task ExportAsync(ExportViewModel exportVm, string targetFormat, string outputPath)
    {
        exportVm.IsExporting = true;
        exportVm.StatusText = status_exporting;
        var ct = BeginOperation();

        var exportOptions = ExportOptionsBuilder.Build(targetFormat, exportVm);
        var useStream = exportVm.UseStream;
        var indented = exportVm.IndentedOutput;

        // 在后台线程执行耗时的导出操作，避免阻塞 UI
        await Task.Run(
            () =>
                _chart.ExportChartAsync(
                    targetFormat,
                    outputPath,
                    useStream,
                    indented,
                    exportOptions,
                    ct
                ),
            ct
        );

        var message = string.Format(status_exported_to, outputPath);
        exportVm.StatusText = message;
        Logger.Information(log_export_done);
        MessageDialog.ShowSuccess(OwnerWindow, export_success_title, message);
    }

    private void ReportCancelled(ExportViewModel exportVm)
    {
        exportVm.StatusText = status_export_cancelled;
        Logger.Information(log_export_cancelled);
    }

    private static (string Extension, string TypeLabel) GetFormatFileInfo(string formatId)
    {
        var format = ChartFormatRegistry.Get(formatId);
        var label = format.FormatId switch
        {
            ChartFormatIds.PhiEdit => file_type_pe_chart,
            ChartFormatIds.RePhiEdit or ChartFormatIds.StellateRePhiEditExtended =>
                file_type_rpe_json,
            ChartFormatIds.PhigrosV3 => file_type_phigros_json,
            ChartFormatIds.PhiFans => file_type_phifans_json,
            ChartFormatIds.PhiChain => file_type_phichain_json,
            _ => format.FormatId,
        };
        return (format.FileExtension, label);
    }
}
