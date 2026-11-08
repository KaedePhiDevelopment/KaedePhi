using KaedePhi.Tool.App.Config;
using KaedePhi.Tool.App.Gui.Models;

namespace KaedePhi.Tool.App.Gui.ViewModels;

public sealed class ToolViewModel : ViewModelBase
{
    public string CurrentFileName
    {
        get;
        set => SetProperty(ref field, value);
    } = string.Empty;

    public string DetectedFormat
    {
        get;
        set => SetProperty(ref field, value);
    } = string.Empty;

    public IReadOnlyList<ToolOption> Tools { get; } = ToolCatalog.Create();

    public ToolOption? SelectedTool
    {
        get;
        set
        {
            if (EqualityComparer<ToolOption?>.Default.Equals(field, value))
                return;

            field = value;
            // 必须先重置参数再通知：监听者会在收到通知后写入配置默认值
            if (value != null)
                ResetParameters(value);

            OnPropertyChanged();
            OnPropertiesChanged(
                nameof(ShowPrecision),
                nameof(ShowTolerance),
                nameof(ShowMergeTolerance),
                nameof(ShowClassicMode),
                nameof(ShowDisableCompress),
                nameof(ShowGeneralOptions),
                nameof(ShowFitOptions),
                nameof(DisableCompressEnabled),
                nameof(CanRun)
            );
        }
    }

    #region 通用工具参数

    public double Precision
    {
        get;
        set => SetProperty(ref field, value);
    } = 64;

    public double Tolerance
    {
        get;
        set => SetProperty(ref field, value);
    } = 0.1;

    public double MergeTolerance
    {
        get;
        set => SetProperty(ref field, value);
    } = 0.1;

    public bool ClassicMode
    {
        get;
        set
        {
            if (!SetProperty(ref field, value))
                return;

            OnPropertyChanged(nameof(DisableCompressEnabled));
            // 经典模式关闭时，禁用压缩选项不可用，自动取消
            if (!value)
                DisableCompress = false;
        }
    }

    public bool DisableCompress
    {
        get;
        set => SetProperty(ref field, value);
    }

    // 仅当工具支持经典模式且当前已启用经典模式时，才允许修改“禁用压缩”选项
    public bool DisableCompressEnabled =>
        SelectedTool is { HasDisableCompress: true, HasClassicMode: true }
            ? ClassicMode
            : SelectedTool?.HasDisableCompress == true;

    #endregion

    #region 运行状态

    public string StatusText
    {
        get;
        set => SetProperty(ref field, value);
    } = string.Empty;

    public bool IsProcessing
    {
        get;
        set
        {
            if (SetProperty(ref field, value))
                OnPropertyChanged(nameof(CanRun));
        }
    }

    // 当前谱面是否含有行为暂不明确、无法处理的内容
    public bool HasUnclearContent
    {
        get;
        set => SetProperty(ref field, value);
    }

    public bool CanRun => SelectedTool != null && !IsProcessing;

    #endregion

    #region 选项可见性

    public bool ShowPrecision => SelectedTool?.HasPrecision == true;
    public bool ShowTolerance => SelectedTool?.HasTolerance == true;
    public bool ShowMergeTolerance => SelectedTool?.HasMergeTolerance == true;
    public bool ShowClassicMode => SelectedTool?.HasClassicMode == true;
    public bool ShowDisableCompress => SelectedTool?.HasDisableCompress == true;
    public bool ShowFitOptions => SelectedTool?.HasFitOptions == true;

    public bool ShowGeneralOptions =>
        ShowPrecision
        || ShowTolerance
        || ShowMergeTolerance
        || ShowClassicMode
        || ShowDisableCompress;

    #endregion

    public event Action? RequestRun;
    public event Action? RequestExport;
    public event Action? RequestReturnToImport;

    public void OnRunClicked() => RequestRun?.Invoke();

    public void OnExportClicked() => RequestExport?.Invoke();

    public void OnReturnToImportClicked() => RequestReturnToImport?.Invoke();

    public void ApplyConfigDefaults(AppConfig config)
    {
        switch (SelectedTool?.ToolId)
        {
            case ToolIds.Unbind:
                ApplyToolDefaults(config.Unbind);
                MergeTolerance = config.Unbind.MergeTolerance;
                break;
            case ToolIds.LayerMerge:
                ApplyToolDefaults(config.LayerMerge);
                break;
            case ToolIds.Cut:
                ApplyToolDefaults(config.Cut);
                break;
            case ToolIds.Fit:
                Tolerance = config.Fit.Tolerance;
                break;
        }
    }

    private void ResetParameters(ToolOption tool)
    {
        Precision = tool.DefaultPrecision;
        Tolerance = tool.DefaultTolerance;
        MergeTolerance = tool.DefaultMergeTolerance;
        ClassicMode = false;
        DisableCompress = false;
    }

    // 仅写入该工具实际提供的参数，其余保持当前值
    private void ApplyToolDefaults(ToolDefaultsConfig defaults)
    {
        Precision = defaults.Precision;
        Tolerance = defaults.Tolerance;
        if (SelectedTool is { HasClassicMode: true })
            ClassicMode = defaults.ClassicMode;
        DisableCompress = defaults.DisableCompress;
    }
}
