namespace KaedePhi.Tool.App.Gui.ViewModels;

public sealed class ImportViewModel : ViewModelBase
{
    public ImportProgressViewModel Progress { get; } = new();

    public bool UseStream
    {
        get;
        set => SetProperty(ref field, value);
    }

    public bool IsLoading
    {
        get;
        set => SetProperty(ref field, value);
    }

    public event Action<string, bool>? FileSelected;
    public event Action? RequestCancelLoading;

    public void OnFileSelected(string filePath) => FileSelected?.Invoke(filePath, UseStream);

    public void OnCancelLoadingClicked() => RequestCancelLoading?.Invoke();
}
