using System.ComponentModel;
using System.Runtime.CompilerServices;
using KaedePhi.Tool.App.Config;
using KaedePhi.Tool.Common;
using KaedePhi.Tool.Converter;

namespace KaedePhi.Tool.App.Gui.ViewModels;

public sealed class ExportViewModel : INotifyPropertyChanged
{
    private string? _selectedFormat = ChartFormatIds.RePhiEdit;

    public List<string> AvailableFormats { get; private set; } =
        ChartFormatRegistry.ExportableFormats.Select(format => format.FormatId).ToList();

    public string SourceFormat
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(ShowConversionOptions));
            OnPropertyChanged(nameof(ShowPeOptions));
            OnPropertyChanged(nameof(ShowPhigrosOptions));
            OnPropertyChanged(nameof(ShowGenericOptions));
        }
    } = string.Empty;

    public string? SelectedFormat
    {
        get => _selectedFormat;
        set
        {
            _selectedFormat = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(IsJsonFormat));
            OnPropertyChanged(nameof(CanExport));
            OnPropertyChanged(nameof(ShowConversionOptions));
            OnPropertyChanged(nameof(ShowPeOptions));
            OnPropertyChanged(nameof(ShowPhigrosOptions));
            OnPropertyChanged(nameof(ShowRePhiEditOptions));
            OnPropertyChanged(nameof(ShowPhiChainOptions));
            OnPropertyChanged(nameof(ShowPhiFansOptions));
            OnPropertyChanged(nameof(ShowGenericOptions));
            if (!IsJsonFormat)
                IndentedOutput = false;
        }
    }

    /// <summary>
    /// 当前选择的导出格式是否为 JSON 格式（true 时才显示格式化输出选项）
    /// </summary>
    public bool IsJsonFormat => !string.IsNullOrWhiteSpace(_selectedFormat)
        && string.Equals(ChartFormatRegistry.Find(_selectedFormat)?.FileExtension, "json", StringComparison.OrdinalIgnoreCase);

    public bool CanExport => !IsExporting
        && !string.IsNullOrWhiteSpace(_selectedFormat)
        && ChartFormatRegistry.Find(_selectedFormat) is { CanExport: true };

    public void RefreshAvailableFormats()
    {
        AvailableFormats = ChartFormatRegistry.ExportableFormats.Select(format => format.FormatId).ToList();
        OnPropertyChanged(nameof(AvailableFormats));
        SelectedFormat = _selectedFormat is not null && AvailableFormats.Contains(_selectedFormat)
            ? _selectedFormat
            : AvailableFormats.FirstOrDefault();
    }

    /// <summary>
    /// 是否需要显示转换选项面板（目标格式存在可用选项时显示）
    /// </summary>
    public bool ShowConversionOptions =>
        _selectedFormat
            is ChartFormatIds.PhiEdit
                or ChartFormatIds.PhigrosV3
                or ChartFormatIds.RePhiEdit
                or ChartFormatIds.StellateRePhiEditExtended
                or ChartFormatIds.PhiChain
                or ChartFormatIds.PhiFans;

    /// <summary>
    /// 是否显示 PhiEdit 专属转换选项（目标为 PhiEdit）
    /// </summary>
    public bool ShowPeOptions => ShowConversionOptions && _selectedFormat == ChartFormatIds.PhiEdit;

    /// <summary>
    /// 是否显示 PhigrosV3 专属转换选项（目标为 PhigrosV3）
    /// </summary>
    public bool ShowPhigrosOptions =>
        ShowConversionOptions && _selectedFormat == ChartFormatIds.PhigrosV3;

    /// <summary>
    /// 是否显示 RePhiEdit 专属转换选项（目标为 RePhiEdit 或其噪域扩展格式）
    /// </summary>
    public bool ShowRePhiEditOptions =>
        ShowConversionOptions
        && _selectedFormat is ChartFormatIds.RePhiEdit or ChartFormatIds.StellateRePhiEditExtended;

    /// <summary>
    /// 是否显示 PhiChain 专属转换选项（目标为 PhiChain）
    /// </summary>
    public bool ShowPhiChainOptions =>
        ShowConversionOptions && _selectedFormat == ChartFormatIds.PhiChain;

    /// <summary>
    /// 是否显示 PhiFans 专属转换选项（目标为 PhiFans）
    /// </summary>
    public bool ShowPhiFansOptions => ShowConversionOptions && _selectedFormat == ChartFormatIds.PhiFans;

    /// <summary>
    /// 是否显示通用转换选项（目标格式支持解绑/合并/线过滤）
    /// </summary>
    public bool ShowGenericOptions =>
        ShowConversionOptions && _selectedFormat is ChartFormatIds.PhiEdit or ChartFormatIds.PhigrosV3;

    public bool UseStream
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// 是否格式化（缩进）JSON 输出，仅对 JSON 格式生效
    /// </summary>
    public bool IndentedOutput
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public bool IsExporting
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(CanExport));
        }
    }

    public string StatusText
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(HasStatusText));
        }
    } = string.Empty;

    public bool HasStatusText => !string.IsNullOrEmpty(StatusText);

    #region PE 转换选项

    public double PeTrailingBeatPadding
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    } = 1d / 64d;

    public double PeUnsupportedEasingPrecision
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    } = 64d;

    public double PeMisalignedXyEventPrecision
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    } = 64d;

    public double PeAlphaCutPrecision
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    } = 64d;

    public double PeAlphaCutTolerance
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    } = 1d;

    public double PeSpeedCutPrecision
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    } = 64d;

    #endregion

    #region PhigrosV3 转换选项

    public float PhigrosDefaultBpm
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    } = 120f;

    public double PhigrosEasingPrecision
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    } = 64d;

    public double PhigrosMisalignedXyEventPrecision
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    } = 64d;

    public double PhigrosAlphaCutPrecision
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    } = 64d;

    public double PhigrosSpeedCutPrecision
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    } = 64d;

    #endregion

    #region 通用选项

    public double UnbindPrecision
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    } = 64d;

    public double UnbindTolerance
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    } = 1d;

    public double UnbindMergeTolerance
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    } = 0.1d;

    public bool UnbindClassicMode
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public double MultiLayerMergePrecision
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    } = 64d;

    public double MultiLayerMergeTolerance
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    } = 0.1d;

    public bool MultiLayerMergeClassicMode
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public bool UnbindCompress
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    } = true;

    public bool MultiLayerMergeCompress
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    } = true;

    public bool RemoveAttachUiLine
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public bool RemoveTextureLine
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public bool FilterFakeNotes
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public bool NegativeAlphaElevation
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public double NegativeAlphaStep
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    } = 4.0d;

    #endregion

    #region RePhiEdit 转换选项

    /// <summary>
    /// RePhiEdit 非支持缓动切割精度
    /// </summary>
    public int RePhiEditUnsupportedEasingPrecision
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    } = 64;

    #endregion

    #region PhiChain 转换选项

    /// <summary>
    /// PhiChain 是否对 rotateWithFather 为 false 的判定线进行父子线解绑
    /// </summary>
    public bool PhiChainUnbindNonRotatingChildren
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    } = true;

    /// <summary>
    /// PhiChain 父子线解绑精度
    /// </summary>
    public double PhiChainUnbindPrecision
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    } = 64d;

    /// <summary>
    /// PhiChain 父子线解绑相对原始运动范围的容差
    /// </summary>
    public double PhiChainUnbindTolerance
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    } = 1d;

    /// <summary>
    /// PhiChain 父子线事件通道合并容差
    /// </summary>
    public double PhiChainUnbindMergeTolerance
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    } = 0.1d;

    /// <summary>
    /// PhiChain 父子线解绑是否使用经典模式
    /// </summary>
    public bool PhiChainUnbindClassicMode
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// PhiChain 多层级合并精度
    /// </summary>
    public double PhiChainMultiLayerMergePrecision
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    } = 64d;

    /// <summary>
    /// PhiChain 多层级合并自适应采样容差
    /// </summary>
    public double PhiChainMultiLayerMergeTolerance
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    } = 0.1d;

    /// <summary>
    /// PhiChain 多层级合并是否使用经典模式
    /// </summary>
    public bool PhiChainMultiLayerMergeClassicMode
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// PhiChain 缓动截取切割精度
    /// </summary>
    public double PhiChainEasingCutPrecision
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    } = 64d;

    #endregion

    #region PhiFans 转换选项

    /// <summary>
    /// PhiFans 非支持缓动切割精度
    /// </summary>
    public int PhiFansUnsupportedEasingPrecision
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    } = 64;

    /// <summary>
    /// PhiFans 节点式格式中相邻事件不连续时后事件开始拍向后推迟的拍数精分
    /// </summary>
    public int PhiFansDiscontinuityBeatPrecision
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    } = 64;

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

    /// <summary>
    /// 从配置加载转换选项默认值
    /// </summary>
    public void ApplyConversionDefaults(ConvertDefaultsConfig config)
    {
        UseStream = config.StreamOutput;
        PeTrailingBeatPadding = config.PeTrailingBeatPadding;
        PeUnsupportedEasingPrecision = config.PeUnsupportedEasingPrecision;
        PeMisalignedXyEventPrecision = config.PeMisalignedXyEventPrecision;
        PeAlphaCutPrecision = config.PeAlphaCutPrecision;
        PeAlphaCutTolerance = config.PeAlphaCutTolerance;
        PeSpeedCutPrecision = config.PeSpeedCutPrecision;
        PhigrosDefaultBpm = config.PhigrosDefaultBpm;
        PhigrosEasingPrecision = config.PhigrosEasingPrecision;
        PhigrosMisalignedXyEventPrecision = config.PhigrosMisalignedXyEventPrecision;
        PhigrosAlphaCutPrecision = config.PhigrosAlphaCutPrecision;
        PhigrosSpeedCutPrecision = config.PhigrosSpeedCutPrecision;
        UnbindPrecision = config.UnbindPrecision;
        UnbindTolerance = config.UnbindTolerance;
        UnbindMergeTolerance = config.UnbindMergeTolerance;
        UnbindClassicMode = config.UnbindClassicMode;
        MultiLayerMergePrecision = config.MultiLayerMergePrecision;
        MultiLayerMergeTolerance = config.MultiLayerMergeTolerance;
        MultiLayerMergeClassicMode = config.MultiLayerMergeClassicMode;
        FilterFakeNotes = config.PhigrosFilterFakeNotes;
        NegativeAlphaElevation = config.PhigrosNegativeAlphaElevation;
        NegativeAlphaStep = config.PhigrosNegativeAlphaStep;

        // RePhiEdit 及其噪域扩展格式选项使用通用配置
        RePhiEditUnsupportedEasingPrecision = (int)config.UnbindPrecision;

        // PhiChain 选项使用通用配置
        PhiChainUnbindPrecision = config.UnbindPrecision;
        PhiChainUnbindTolerance = config.UnbindTolerance;
        PhiChainUnbindMergeTolerance = config.UnbindMergeTolerance;
        PhiChainUnbindClassicMode = config.UnbindClassicMode;
        PhiChainMultiLayerMergePrecision = config.MultiLayerMergePrecision;
        PhiChainMultiLayerMergeTolerance = config.MultiLayerMergeTolerance;
        PhiChainMultiLayerMergeClassicMode = config.MultiLayerMergeClassicMode;

        PhiFansUnsupportedEasingPrecision = config.PhiFansUnsupportedEasingPrecision;
        PhiFansDiscontinuityBeatPrecision = config.PhiFansDiscontinuityBeatPrecision;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? name = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
