namespace KaedePhi.Tool.App.Gui.ViewModels;

public sealed class ImportProgressViewModel : ViewModelBase
{
    public bool IsIndeterminate
    {
        get;
        private set
        {
            if (SetProperty(ref field, value))
                OnPropertyChanged(nameof(IsDeterminate));
        }
    } = true;

    public bool IsDeterminate => !IsIndeterminate;

    public double Value
    {
        get;
        private set => SetProperty(ref field, value);
    }

    public string PercentageText
    {
        get;
        private set => SetProperty(ref field, value);
    } = string.Empty;

    // 传入 null 表示进度未知，显示为不确定进度
    public void SetProgress(double? progress)
    {
        if (progress is null)
        {
            IsIndeterminate = true;
            PercentageText = string.Empty;
            return;
        }

        Value = Math.Clamp(progress.Value, 0.0, 1.0) * 100;
        PercentageText = $"{Math.Floor(Value):0}%";
        IsIndeterminate = false;
    }

    public void Reset() => SetProgress(null);
}
