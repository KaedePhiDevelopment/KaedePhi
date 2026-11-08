using KaedePhi.Tool.App.Config;

namespace KaedePhi.Tool.App.Gui.ViewModels.ExportOptions;

// 目标格式为 PhiEdit 与 PhigrosV3 时共用的父子线解绑、多层级合并与判定线过滤选项
public sealed class CommonExportOptionsViewModel : ViewModelBase
{
    public CommonExportOptionsViewModel() => ApplyDefaults(new ConvertDefaultsConfig());

    public double UnbindPrecision
    {
        get;
        set => SetProperty(ref field, value);
    }

    public double UnbindTolerance
    {
        get;
        set => SetProperty(ref field, value);
    }

    public double UnbindMergeTolerance
    {
        get;
        set => SetProperty(ref field, value);
    }

    public bool UnbindClassicMode
    {
        get;
        set => SetProperty(ref field, value);
    }

    public bool UnbindCompress
    {
        get;
        set => SetProperty(ref field, value);
    } = true;

    public double MultiLayerMergePrecision
    {
        get;
        set => SetProperty(ref field, value);
    }

    public double MultiLayerMergeTolerance
    {
        get;
        set => SetProperty(ref field, value);
    }

    public bool MultiLayerMergeClassicMode
    {
        get;
        set => SetProperty(ref field, value);
    }

    public bool MultiLayerMergeCompress
    {
        get;
        set => SetProperty(ref field, value);
    } = true;

    public bool RemoveAttachUiLine
    {
        get;
        set => SetProperty(ref field, value);
    }

    public bool RemoveTextureLine
    {
        get;
        set => SetProperty(ref field, value);
    }

    // 压缩与线过滤选项没有对应的配置项，不随配置重置
    public void ApplyDefaults(ConvertDefaultsConfig config)
    {
        UnbindPrecision = config.UnbindPrecision;
        UnbindTolerance = config.UnbindTolerance;
        UnbindMergeTolerance = config.UnbindMergeTolerance;
        UnbindClassicMode = config.UnbindClassicMode;
        MultiLayerMergePrecision = config.MultiLayerMergePrecision;
        MultiLayerMergeTolerance = config.MultiLayerMergeTolerance;
        MultiLayerMergeClassicMode = config.MultiLayerMergeClassicMode;
    }
}
