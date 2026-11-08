using static KaedePhi.Tool.Localization.GuiLocalizationString;

namespace KaedePhi.Tool.App.Gui.ViewModels;

public sealed class ProcessingViewModel : ViewModelBase
{
    private const int ToolStepIndex = 0;

    public List<string> Steps { get; } = [step_running_tool];

    public double Progress
    {
        get;
        set => SetProperty(ref field, value);
    }

    public double ToolProgressValue
    {
        get;
        set => SetProperty(ref field, value);
    }

    public string CurrentStep
    {
        get;
        set => SetProperty(ref field, value);
    } = string.Empty;

    public string StatusMessage
    {
        get;
        set => SetProperty(ref field, value);
    } = string.Empty;

    public bool IsCompleted
    {
        get;
        set
        {
            if (SetProperty(ref field, value))
                OnPropertyChanged(nameof(ShowSuccessIcon));
        }
    }

    public bool HasError
    {
        get;
        set
        {
            if (SetProperty(ref field, value))
                OnPropertyChanged(nameof(ShowSuccessIcon));
        }
    }

    public string ErrorMessage
    {
        get;
        set => SetProperty(ref field, value);
    } = string.Empty;

    public string LogFilePath
    {
        get;
        set => SetProperty(ref field, value);
    } = string.Empty;

    public bool ShowSuccessIcon => IsCompleted && !HasError;

    public event Action? RequestReturnToTools;
    public event Action? RequestReturnToImport;
    public event Action? RequestGoToExport;
    public event Action? RequestCancel;

    public void OnReturnToToolsClicked() => RequestReturnToTools?.Invoke();

    public void OnReturnToImportClicked() => RequestReturnToImport?.Invoke();

    public void OnGoToExportClicked() => RequestGoToExport?.Invoke();

    public void OnCancelClicked() => RequestCancel?.Invoke();

    public void Reset()
    {
        Progress = 0;
        ToolProgressValue = 0;
        CurrentStep = string.Empty;
        StatusMessage = string.Empty;
        IsCompleted = false;
        HasError = false;
        ErrorMessage = string.Empty;
        LogFilePath = string.Empty;
    }

    public void SetStep(int index, string detail = "")
    {
        if (index < 0 || index >= Steps.Count)
            return;
        CurrentStep = Steps[index];
        StatusMessage = string.IsNullOrEmpty(detail) ? Steps[index] : detail;
        Progress = (double)index / Steps.Count * 100;
        if (index != ToolStepIndex)
            ToolProgressValue = 0;
    }

    public void SetToolProgress(double toolProgress, double overallProgress, string? detail = null)
    {
        ToolProgressValue = Math.Clamp(toolProgress, 0, 1) * 100;
        if (overallProgress >= 0)
        {
            var stepStart = (double)ToolStepIndex / Steps.Count * 100;
            var stepEnd = (double)(ToolStepIndex + 1) / Steps.Count * 100;
            var next = stepStart + Math.Clamp(overallProgress, 0, 1) * (stepEnd - stepStart);
            // 单调递增，避免异步回调乱序导致进度回退
            if (next > Progress)
                Progress = next;
        }

        if (!string.IsNullOrEmpty(detail))
            StatusMessage = detail;
    }

    public void SetCompleted(string message)
    {
        IsCompleted = true;
        HasError = false;
        StatusMessage = message;
        Progress = 100;
    }

    public void SetError(string message, string logPath)
    {
        IsCompleted = true;
        HasError = true;
        ErrorMessage = message;
        LogFilePath = logPath;
        StatusMessage = processing_error;
    }
}
