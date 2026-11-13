using Avalonia.Controls;
using KaedePhi.Tool.App.Gui.Services;
using KaedePhi.Tool.App.Gui.ViewModels;
using KaedePhi.Tool.Common;
using KaedePhi.Tool.Converter.PhiChain.Model;
using KaedePhi.Tool.Converter.PhiEdit.Model;
using static KaedePhi.Tool.Localization.GuiLocalizationString;

namespace KaedePhi.Tool.App.Gui.Controllers;

internal sealed class ImportController : ControllerBase
{
    private readonly GuiNavigationService _navigation;
    private readonly GuiChartService _chart;

    private bool _isFileProcessing;
    private string? _pendingFilePath;
    private bool _pendingUseStream;

    public ImportController(
        GuiNavigationService navigation,
        GuiChartService chart,
        LogService logService,
        Window window
    )
        : base(logService.ForContext<ImportController>(), logService, window)
    {
        _navigation = navigation;
        _chart = chart;

        _navigation.Import.FileSelected += OnFileSelected;
        _navigation.Import.RequestCancelLoading += Cancel;
        _navigation.ImportOptions.RequestConfirm += OnImportOptionsConfirm;
        _navigation.ImportOptions.RequestCancel += ReturnToImport;
        _navigation.ImportOptions.RequestCancelImport += Cancel;
    }

    public void ReturnToImport()
    {
        if (_isFileProcessing)
            return;
        _pendingFilePath = null;
        _navigation.ShowImport();
    }

    private async void OnFileSelected(string filePath, bool useStream)
    {
        if (_isFileProcessing)
            return;

        var importVm = _navigation.Import;
        await RunImportAsync(
            importVm.Progress,
            loading => importVm.IsLoading = loading,
            returnToImportOnFailure: false,
            async (progress, ct) =>
            {
                // 先检测格式，再判断是否需要用户补充导入选项
                var format = await Task.Run(
                    () => _chart.DetectChartFormatAsync(filePath, ct, progress),
                    ct
                );
                var formatId = format.FormatId;
                if (formatId is ChartFormatIds.PhiEdit or ChartFormatIds.PhiChain)
                {
                    _pendingFilePath = filePath;
                    _pendingUseStream = useStream;
                    _navigation.ShowImportOptions(formatId, Path.GetFileName(filePath));
                }
                else
                {
                    await LoadChartAsync(filePath, useStream, null, progress, ct);
                }
            }
        );
    }

    private async void OnImportOptionsConfirm()
    {
        if (_pendingFilePath == null || _isFileProcessing)
            return;

        var filePath = _pendingFilePath;
        var useStream = _pendingUseStream;
        var optionsVm = _navigation.ImportOptions;
        await RunImportAsync(
            optionsVm.Progress,
            loading => optionsVm.IsLoading = loading,
            returnToImportOnFailure: true,
            (progress, ct) =>
            {
                var formatId = optionsVm.DetectedFormat;
                return LoadChartAsync(
                    filePath,
                    useStream,
                    BuildImportOptions(formatId),
                    progress,
                    ct
                );
            }
        );
        _pendingFilePath = null;
    }

    // 统一管理导入流程的占用标记、加载状态、取消令牌与异常处理
    private async Task RunImportAsync(
        ImportProgressViewModel progressVm,
        Action<bool> setLoading,
        bool returnToImportOnFailure,
        Func<IProgress<double?>, CancellationToken, Task> operation
    )
    {
        _isFileProcessing = true;
        setLoading(true);
        progressVm.Reset();
        var ct = BeginOperation();
        var progress = new Progress<double?>(progressVm.SetProgress);

        try
        {
            await Task.Yield();
            await operation(progress, ct);
        }
        catch (OperationCanceledException)
        {
            Logger.Information(log_import_cancelled);
            _navigation.ShowImport();
        }
        catch (Exception ex)
        {
            ReportFailure(log_load_failed, load_error_title, ex);
            if (returnToImportOnFailure)
                _navigation.ShowImport();
        }
        finally
        {
            _isFileProcessing = false;
            setLoading(false);
            EndOperation();
        }
    }

    private async Task LoadChartAsync(
        string filePath,
        bool useStream,
        object? importOptions,
        IProgress<double?> progress,
        CancellationToken ct
    )
    {
        await Task.Run(
            () => _chart.LoadChartAsync(filePath, useStream, ct, importOptions, progress),
            ct
        );
        ct.ThrowIfCancellationRequested();
        _navigation.ShowToolForChart(filePath, _chart.SourceFormat);
    }

    private object? BuildImportOptions(string formatId)
    {
        var vm = _navigation.ImportOptions;
        return formatId switch
        {
            ChartFormatIds.PhiEdit => new PhiEditToIrConvertOptions
            {
                FrameDurationBeat = 1d / vm.PeFrameDurationBeat,
                TrailingBeatPadding = 1d / vm.PeTrailingBeatPadding,
            },
            ChartFormatIds.PhiChain => new PhiChainToIrConvertOptions
            {
                UnsupportedEasingPrecision = vm.PhiChainUnsupportedEasingPrecision,
            },
            _ => null,
        };
    }
}
