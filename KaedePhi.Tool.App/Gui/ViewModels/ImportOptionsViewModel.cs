using KaedePhi.Tool.Common;

namespace KaedePhi.Tool.App.Gui.ViewModels;

public sealed class ImportOptionsViewModel : ViewModelBase
{
    public ImportProgressViewModel Progress { get; } = new();

    public string DetectedFormat
    {
        get;
        set
        {
            if (!SetProperty(ref field, value))
                return;
            OnPropertiesChanged(
                nameof(ShowPeOptions),
                nameof(ShowPhiChainOptions),
                nameof(FormatName)
            );
        }
    } = string.Empty;

    public string FileName
    {
        get;
        set => SetProperty(ref field, value);
    } = string.Empty;

    public bool IsLoading
    {
        get;
        set => SetProperty(ref field, value);
    }

    public string FormatName => DetectedFormat;

    public bool ShowPeOptions => DetectedFormat == ChartFormatIds.PhiEdit;

    public bool ShowPhiChainOptions => DetectedFormat == ChartFormatIds.PhiChain;

    #region PhiEdit 导入选项

    // 以下两项均为 1 / x 拍中的分母 x
    public int PeFrameDurationBeat
    {
        get;
        set => SetProperty(ref field, value);
    } = 64;

    public int PeTrailingBeatPadding
    {
        get;
        set => SetProperty(ref field, value);
    } = 64;

    #endregion

    #region PhiChain 导入选项

    public int PhiChainUnsupportedEasingPrecision
    {
        get;
        set => SetProperty(ref field, value);
    } = 64;

    #endregion

    public event Action? RequestConfirm;
    public event Action? RequestCancel;
    public event Action? RequestCancelImport;

    public void OnConfirmClicked() => RequestConfirm?.Invoke();

    public void OnCancelClicked() => RequestCancel?.Invoke();

    public void OnCancelImportClicked() => RequestCancelImport?.Invoke();
}
