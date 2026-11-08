using KaedePhi.Tool.App.Config;

namespace KaedePhi.Tool.App.Gui.ViewModels;

// 直接读写设置草稿中的某一个工具默认参数，并处理“经典模式”与“禁用压缩”的联动
public sealed class ToolDefaultsViewModel : ViewModelBase
{
    private readonly ToolDefaultsConfig _model;
    private readonly bool _compressRequiresClassicMode;

    public ToolDefaultsViewModel(ToolDefaultsConfig model, bool compressRequiresClassicMode)
    {
        _model = model;
        _compressRequiresClassicMode = compressRequiresClassicMode;
    }

    public double Precision
    {
        get => _model.Precision;
        set => SetProperty(_model.Precision, value, v => _model.Precision = v);
    }

    public double Tolerance
    {
        get => _model.Tolerance;
        set => SetProperty(_model.Tolerance, value, v => _model.Tolerance = v);
    }

    public double MergeTolerance
    {
        get => _model.MergeTolerance;
        set => SetProperty(_model.MergeTolerance, value, v => _model.MergeTolerance = v);
    }

    public bool ClassicMode
    {
        get => _model.ClassicMode;
        set
        {
            if (!SetProperty(_model.ClassicMode, value, v => _model.ClassicMode = v))
                return;

            OnPropertyChanged(nameof(DisableCompressEnabled));
            // 经典模式关闭后“禁用压缩”不可用，自动取消
            if (!value)
                DisableCompress = false;
        }
    }

    public bool DisableCompress
    {
        get => _model.DisableCompress;
        set => SetProperty(_model.DisableCompress, value, v => _model.DisableCompress = v);
    }

    public bool DisableCompressEnabled => !_compressRequiresClassicMode || ClassicMode;
}
