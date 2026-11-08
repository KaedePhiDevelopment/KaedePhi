using KaedePhi.Tool.App.Gui.Models;
using KaedePhi.Tool.App.Gui.Services;
using static KaedePhi.Tool.Localization.GuiLocalizationString;

namespace KaedePhi.Tool.App.Gui.ViewModels;

public sealed class MainViewModel : ViewModelBase
{
    private const double ExpandedSidebarWidth = 236;
    private const double CollapsedSidebarWidth = 72;

    public MainViewModel()
        : this(new BackgroundViewModel()) { }

    internal MainViewModel(BackgroundSceneService backgroundScene)
        : this(new BackgroundViewModel(backgroundScene)) { }

    private MainViewModel(BackgroundViewModel background)
    {
        Background = background;
    }

    public BackgroundViewModel Background { get; }

    #region 侧边栏

    public bool IsSidebarExpanded
    {
        get;
        private set
        {
            if (!SetProperty(ref field, value))
                return;
            OnPropertiesChanged(
                nameof(SidebarWidth),
                nameof(SidebarToggleIcon),
                nameof(SidebarToggleDescription)
            );
        }
    } = true;

    public double SidebarWidth => IsSidebarExpanded ? ExpandedSidebarWidth : CollapsedSidebarWidth;

    public string SidebarToggleIcon => IsSidebarExpanded ? "\uf053" : "\uf054";

    public string SidebarToggleDescription =>
        IsSidebarExpanded ? ui_sidebar_collapse : ui_sidebar_expand;

    public void ToggleSidebar() => IsSidebarExpanded = !IsSidebarExpanded;

    #endregion

    #region 页面外壳状态

    public object? CurrentPage
    {
        get;
        set => SetProperty(ref field, value);
    }

    public ShellPageKey CurrentPageKey
    {
        get;
        private set
        {
            if (!SetProperty(ref field, value))
                return;
            OnPropertiesChanged(
                nameof(IsImportActive),
                nameof(IsToolActive),
                nameof(IsExportActive),
                nameof(IsSettingsActive)
            );
        }
    } = ShellPageKey.Import;

    public string CurrentPageTitle
    {
        get;
        private set => SetProperty(ref field, value);
    } = import_button;

    public string CurrentPageDescription
    {
        get;
        private set => SetProperty(ref field, value);
    } = app_subtitle;

    public int CurrentStep
    {
        get;
        private set => SetProperty(ref field, value);
    } = 1;

    public bool IsImportActive =>
        CurrentPageKey is ShellPageKey.Import or ShellPageKey.ImportOptions;

    public bool IsToolActive => CurrentPageKey is ShellPageKey.Tool or ShellPageKey.Processing;

    public bool IsExportActive => CurrentPageKey == ShellPageKey.Export;

    public bool IsSettingsActive => CurrentPageKey == ShellPageKey.Settings;

    #endregion

    #region 当前谱面上下文

    public string CurrentFileName
    {
        get;
        private set => SetProperty(ref field, value);
    } = string.Empty;

    public string CurrentFormat
    {
        get;
        private set => SetProperty(ref field, value);
    } = string.Empty;

    public bool HasChart
    {
        get;
        private set
        {
            if (!SetProperty(ref field, value))
                return;
            OnPropertiesChanged(nameof(CanOpenTools), nameof(CanOpenExport));
        }
    }

    public bool CanOpenTools => HasChart;

    public bool CanOpenExport => HasChart;

    public void SetShellState(
        ShellPageKey pageKey,
        string pageTitle,
        string pageDescription,
        int step,
        string? fileName = null,
        string? format = null,
        bool? hasChart = null
    )
    {
        CurrentPageKey = pageKey;
        CurrentPageTitle = pageTitle;
        CurrentPageDescription = pageDescription;
        CurrentStep = step;

        if (fileName != null)
            CurrentFileName = fileName;
        if (format != null)
            CurrentFormat = format;
        if (hasChart.HasValue)
            HasChart = hasChart.Value;
    }

    public void ClearChartContext()
    {
        HasChart = false;
        CurrentFileName = string.Empty;
        CurrentFormat = string.Empty;
    }

    #endregion

    #region 导航请求

    public event Action? RequestImport;
    public event Action? RequestTools;
    public event Action? RequestExport;
    public event Action? RequestSettings;

    public void OnImportNavigationClicked() => RequestImport?.Invoke();

    public void OnToolsNavigationClicked() => RequestTools?.Invoke();

    public void OnExportNavigationClicked() => RequestExport?.Invoke();

    public void OnSettingsNavigationClicked() => RequestSettings?.Invoke();

    #endregion
}
