using KaedePhi.Tool.App.Config;

namespace KaedePhi.Tool.App.Gui.ViewModels.ExportOptions;

// RePhiEdit 及其噪域扩展格式的转换选项
public sealed class RePhiEditExportOptionsViewModel : ViewModelBase
{
    public RePhiEditExportOptionsViewModel() => ApplyDefaults(new ConvertDefaultsConfig());

    public int UnsupportedEasingPrecision
    {
        get;
        set => SetProperty(ref field, value);
    }

    // 该格式沿用通用解绑精度作为缓动切割精度
    public void ApplyDefaults(ConvertDefaultsConfig config) =>
        UnsupportedEasingPrecision = (int)config.UnbindPrecision;
}
