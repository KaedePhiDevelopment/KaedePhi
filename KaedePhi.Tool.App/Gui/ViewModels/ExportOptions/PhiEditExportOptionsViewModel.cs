using KaedePhi.Tool.App.Config;

namespace KaedePhi.Tool.App.Gui.ViewModels.ExportOptions;

public sealed class PhiEditExportOptionsViewModel : ViewModelBase
{
    public PhiEditExportOptionsViewModel() => ApplyDefaults(new ConvertDefaultsConfig());

    public double TrailingBeatPadding
    {
        get;
        set => SetProperty(ref field, value);
    }

    public double UnsupportedEasingPrecision
    {
        get;
        set => SetProperty(ref field, value);
    }

    public double MisalignedXyEventPrecision
    {
        get;
        set => SetProperty(ref field, value);
    }

    public double AlphaCutPrecision
    {
        get;
        set => SetProperty(ref field, value);
    }

    public double AlphaCutTolerance
    {
        get;
        set => SetProperty(ref field, value);
    }

    public double SpeedCutPrecision
    {
        get;
        set => SetProperty(ref field, value);
    }

    public void ApplyDefaults(ConvertDefaultsConfig config)
    {
        TrailingBeatPadding = config.PeTrailingBeatPadding;
        UnsupportedEasingPrecision = config.PeUnsupportedEasingPrecision;
        MisalignedXyEventPrecision = config.PeMisalignedXyEventPrecision;
        AlphaCutPrecision = config.PeAlphaCutPrecision;
        AlphaCutTolerance = config.PeAlphaCutTolerance;
        SpeedCutPrecision = config.PeSpeedCutPrecision;
    }
}
