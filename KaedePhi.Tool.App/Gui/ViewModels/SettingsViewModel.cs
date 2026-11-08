using KaedePhi.Tool.App.Config;
using static KaedePhi.Tool.Localization.GuiLocalizationString;

namespace KaedePhi.Tool.App.Gui.ViewModels;

public sealed class SettingsViewModel : ViewModelBase
{
    private readonly AppConfigService _config;

    public SettingsViewModel(AppConfigService config)
    {
        _config = config;
        LoadDraft();
    }

    // 设置页编辑的是配置副本，保存成功前不会影响运行中的配置，也不会丢失页面未展示的配置项
    public AppConfig Draft { get; private set; } = null!;

    public ToolDefaultsViewModel Unbind { get; private set; } = null!;

    public ToolDefaultsViewModel LayerMerge { get; private set; } = null!;

    public ToolDefaultsViewModel Cut { get; private set; } = null!;

    public string StatusText
    {
        get;
        set => SetProperty(ref field, value);
    } = string.Empty;

    public event Action? RequestReturnToTools;

    public void OnReturnClicked() => RequestReturnToTools?.Invoke();

    public void OnSaveClicked() =>
        StatusText = _config.Commit(Draft) ? settings_saved : settings_save_failed;

    public void OnResetClicked()
    {
        var saved = _config.ResetToDefaults();
        LoadDraft();
        StatusText = saved ? settings_reset : settings_save_failed;
    }

    private void LoadDraft()
    {
        Draft = _config.CreateDraft();
        Unbind = new ToolDefaultsViewModel(Draft.Unbind, compressRequiresClassicMode: true);
        LayerMerge = new ToolDefaultsViewModel(Draft.LayerMerge, compressRequiresClassicMode: true);
        Cut = new ToolDefaultsViewModel(Draft.Cut, compressRequiresClassicMode: false);
        // 草稿整体被替换，通知所有绑定重新读取
        OnPropertyChanged(string.Empty);
    }
}
