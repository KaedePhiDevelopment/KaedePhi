using Avalonia.Controls;
using KaedePhi.Tool.App.Gui.Services;
using KaedePhi.Tool.App.Gui.Views;
using Serilog;
using static KaedePhi.Tool.Localization.GuiLocalizationString;

namespace KaedePhi.Tool.App.Gui.Controllers;

internal abstract class ControllerBase
{
    private readonly LogService _logService;
    private CancellationTokenSource? _cts;

    protected ControllerBase(ILogger logger, LogService logService, Window ownerWindow)
    {
        Logger = logger;
        OwnerWindow = ownerWindow;
        _logService = logService;
    }

    protected ILogger Logger { get; }

    protected Window OwnerWindow { get; }

    public void Cancel() => TryCancelOperation();

    protected CancellationToken BeginOperation()
    {
        _cts?.Dispose();
        _cts = new CancellationTokenSource();
        return _cts.Token;
    }

    protected void EndOperation()
    {
        _cts?.Dispose();
        _cts = null;
    }

    // 返回本次调用是否真正触发了取消
    protected bool TryCancelOperation()
    {
        if (_cts is not { IsCancellationRequested: false })
            return false;

        _cts.Cancel();
        return true;
    }

    // 记录异常并弹出带日志路径的错误对话框，返回展示给用户的错误文本
    protected string ReportFailure(string logMessage, string dialogTitle, Exception exception)
    {
        Logger.Error(exception, logMessage);
        var message = string.Format(
            status_error_with_log,
            exception.Message,
            _logService.CurrentLogFile
        );
        MessageDialog.ShowError(OwnerWindow, dialogTitle, message);
        return message;
    }
}
