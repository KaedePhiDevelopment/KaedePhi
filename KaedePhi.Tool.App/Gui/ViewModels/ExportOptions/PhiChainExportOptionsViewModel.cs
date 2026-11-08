using KaedePhi.Tool.App.Config;

namespace KaedePhi.Tool.App.Gui.ViewModels.ExportOptions;

public sealed class PhiChainExportOptionsViewModel : ViewModelBase
{
    public PhiChainExportOptionsViewModel() => ApplyDefaults(new ConvertDefaultsConfig());

    // 是否对 rotateWithFather 为 false 的判定线进行父子线解绑
    public bool UnbindNonRotatingChildren
    {
        get;
        set => SetProperty(ref field, value);
    } = true;

    public double UnbindPrecision
    {
        get;
        set => SetProperty(ref field, value);
    }

    // 解绑相对原始运动范围的容差
    public double UnbindTolerance
    {
        get;
        set => SetProperty(ref field, value);
    }

    // 父子线事件通道合并容差
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

    public double MultiLayerMergePrecision
    {
        get;
        set => SetProperty(ref field, value);
    }

    // 多层级合并的自适应采样容差
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

    public double EasingCutPrecision
    {
        get;
        set => SetProperty(ref field, value);
    } = 64d;

    // 该格式沿用通用解绑与合并配置；非旋转子线解绑与缓动截取精度没有对应的配置项
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
