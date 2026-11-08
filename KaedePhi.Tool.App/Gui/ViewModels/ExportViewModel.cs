using KaedePhi.Tool.App.Config;
using KaedePhi.Tool.App.Gui.ViewModels.ExportOptions;
using KaedePhi.Tool.Common;
using KaedePhi.Tool.Converter;

namespace KaedePhi.Tool.App.Gui.ViewModels;

public sealed class ExportViewModel : ViewModelBase
{
    // 目标格式变化时需要重新求值的派生属性
    private static readonly string[] FormatStateProperties =
    [
        nameof(IsJsonFormat),
        nameof(CanExport),
        nameof(ShowConversionOptions),
        nameof(ShowPeOptions),
        nameof(ShowPhigrosOptions),
        nameof(ShowRePhiEditOptions),
        nameof(ShowPhiChainOptions),
        nameof(ShowPhiFansOptions),
        nameof(ShowGenericOptions),
    ];

    public List<string> AvailableFormats { get; private set; } = ListExportableFormats();

    #region 格式选项分组

    public PhiEditExportOptionsViewModel Pe { get; } = new();
    public PhigrosV3ExportOptionsViewModel Phigros { get; } = new();
    public CommonExportOptionsViewModel Common { get; } = new();
    public RePhiEditExportOptionsViewModel RePhiEdit { get; } = new();
    public PhiChainExportOptionsViewModel PhiChain { get; } = new();
    public PhiFansExportOptionsViewModel PhiFans { get; } = new();

    #endregion

    #region 目标格式与派生状态

    public string SourceFormat
    {
        get;
        set => SetProperty(ref field, value);
    } = string.Empty;

    public string? SelectedFormat
    {
        get;
        set
        {
            if (SetProperty(ref field, value))
                OnSelectedFormatChanged();
        }
    } = ChartFormatIds.RePhiEdit;

    // 仅 JSON 格式才显示格式化输出选项
    public bool IsJsonFormat =>
        !string.IsNullOrWhiteSpace(SelectedFormat)
        && string.Equals(
            ChartFormatRegistry.Find(SelectedFormat)?.FileExtension,
            "json",
            StringComparison.OrdinalIgnoreCase
        );

    public bool CanExport =>
        !IsExporting
        && !string.IsNullOrWhiteSpace(SelectedFormat)
        && ChartFormatRegistry.Find(SelectedFormat) is { CanExport: true };

    // 目标格式存在可配置的转换选项时显示选项面板
    public bool ShowConversionOptions =>
        SelectedFormat
            is ChartFormatIds.PhiEdit
                or ChartFormatIds.PhigrosV3
                or ChartFormatIds.RePhiEdit
                or ChartFormatIds.StellateRePhiEditExtended
                or ChartFormatIds.PhiChain
                or ChartFormatIds.PhiFans;

    public bool ShowPeOptions => SelectedFormat == ChartFormatIds.PhiEdit;

    public bool ShowPhigrosOptions => SelectedFormat == ChartFormatIds.PhigrosV3;

    public bool ShowRePhiEditOptions =>
        SelectedFormat is ChartFormatIds.RePhiEdit or ChartFormatIds.StellateRePhiEditExtended;

    public bool ShowPhiChainOptions => SelectedFormat == ChartFormatIds.PhiChain;

    public bool ShowPhiFansOptions => SelectedFormat == ChartFormatIds.PhiFans;

    // 支持解绑、合并与判定线过滤的目标格式
    public bool ShowGenericOptions =>
        SelectedFormat is ChartFormatIds.PhiEdit or ChartFormatIds.PhigrosV3;

    #endregion

    #region 输出设置与运行状态

    public bool UseStream
    {
        get;
        set => SetProperty(ref field, value);
    }

    // 仅对 JSON 格式生效
    public bool IndentedOutput
    {
        get;
        set => SetProperty(ref field, value);
    }

    public bool IsExporting
    {
        get;
        set
        {
            if (SetProperty(ref field, value))
                OnPropertyChanged(nameof(CanExport));
        }
    }

    public string StatusText
    {
        get;
        set
        {
            if (SetProperty(ref field, value))
                OnPropertyChanged(nameof(HasStatusText));
        }
    } = string.Empty;

    public bool HasStatusText => !string.IsNullOrEmpty(StatusText);

    #endregion

    public event Action? RequestExport;
    public event Action? RequestReturnToImport;
    public event Action? RequestCancelExport;

    public void OnExportClicked()
    {
        if (CanExport)
            RequestExport?.Invoke();
    }

    public void OnReturnClicked() => RequestReturnToImport?.Invoke();

    public void OnCancelExportClicked() => RequestCancelExport?.Invoke();

    public void RefreshAvailableFormats()
    {
        AvailableFormats = ListExportableFormats();
        OnPropertyChanged(nameof(AvailableFormats));
        SelectedFormat =
            SelectedFormat is not null && AvailableFormats.Contains(SelectedFormat)
                ? SelectedFormat
                : AvailableFormats.FirstOrDefault();
        // 格式注册表可能已变化，即使选中项未变也需要重新求值
        OnPropertiesChanged(FormatStateProperties);
    }

    // 进入导出页时重置页面状态并载入配置中的转换默认值
    public void Prepare(string sourceFormat, ConvertDefaultsConfig defaults)
    {
        SourceFormat = sourceFormat;
        RefreshAvailableFormats();
        if (AvailableFormats.Contains(ChartFormatIds.RePhiEdit))
            SelectedFormat = ChartFormatIds.RePhiEdit;
        IndentedOutput = false;
        StatusText = string.Empty;
        IsExporting = false;
        ApplyConversionDefaults(defaults);
    }

    public void ApplyConversionDefaults(ConvertDefaultsConfig config)
    {
        UseStream = config.StreamOutput;
        Pe.ApplyDefaults(config);
        Phigros.ApplyDefaults(config);
        Common.ApplyDefaults(config);
        RePhiEdit.ApplyDefaults(config);
        PhiChain.ApplyDefaults(config);
        PhiFans.ApplyDefaults(config);
    }

    private void OnSelectedFormatChanged()
    {
        OnPropertiesChanged(FormatStateProperties);
        if (!IsJsonFormat)
            IndentedOutput = false;
    }

    private static List<string> ListExportableFormats() =>
        ChartFormatRegistry.ExportableFormats.Select(format => format.FormatId).ToList();
}
