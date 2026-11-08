using KaedePhi.Tool.App.Config;

namespace KaedePhi.Tool.App.Gui.ViewModels.ExportOptions;

public sealed class PhigrosV3ExportOptionsViewModel : ViewModelBase
{
    public PhigrosV3ExportOptionsViewModel() => ApplyDefaults(new ConvertDefaultsConfig());

    public double EasingPrecision
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

    public double SpeedCutPrecision
    {
        get;
        set => SetProperty(ref field, value);
    }

    public bool FilterFakeNotes
    {
        get;
        set => SetProperty(ref field, value);
    }

    public bool NegativeAlphaElevation
    {
        get;
        set => SetProperty(ref field, value);
    }

    public double NegativeAlphaStep
    {
        get;
        set => SetProperty(ref field, value);
    }

    public void ApplyDefaults(ConvertDefaultsConfig config)
    {
        EasingPrecision = config.PhigrosEasingPrecision;
        MisalignedXyEventPrecision = config.PhigrosMisalignedXyEventPrecision;
        AlphaCutPrecision = config.PhigrosAlphaCutPrecision;
        SpeedCutPrecision = config.PhigrosSpeedCutPrecision;
        FilterFakeNotes = config.PhigrosFilterFakeNotes;
        NegativeAlphaElevation = config.PhigrosNegativeAlphaElevation;
        NegativeAlphaStep = config.PhigrosNegativeAlphaStep;
    }
}
