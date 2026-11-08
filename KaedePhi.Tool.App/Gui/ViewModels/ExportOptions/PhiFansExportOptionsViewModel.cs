using KaedePhi.Tool.App.Config;

namespace KaedePhi.Tool.App.Gui.ViewModels.ExportOptions;

public sealed class PhiFansExportOptionsViewModel : ViewModelBase
{
    public PhiFansExportOptionsViewModel() => ApplyDefaults(new ConvertDefaultsConfig());

    public int UnsupportedEasingPrecision
    {
        get;
        set => SetProperty(ref field, value);
    }

    // 节点式格式中相邻事件不连续时，后事件开始拍向后推迟的拍数精分
    public int DiscontinuityBeatPrecision
    {
        get;
        set => SetProperty(ref field, value);
    }

    public void ApplyDefaults(ConvertDefaultsConfig config)
    {
        UnsupportedEasingPrecision = config.PhiFansUnsupportedEasingPrecision;
        DiscontinuityBeatPrecision = config.PhiFansDiscontinuityBeatPrecision;
    }
}
