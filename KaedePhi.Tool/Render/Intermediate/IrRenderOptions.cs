using SkiaSharp;

namespace KaedePhi.Tool.Render.Intermediate;

/// <summary>
/// 已移除的 IR 渲染配置：渲染工具已被彻底移除，创建实例及访问任何属性均抛出未实现异常。
/// </summary>
[Obsolete(RenderRemovedNotice.Message, true)]
public class IrRenderOptions
{
    /// <summary>
    /// 已移除，调用始终抛出未实现异常。
    /// </summary>
    /// <exception cref="NotImplementedException">渲染工具已被彻底移除，始终抛出。</exception>
    public IrRenderOptions() => throw RenderRemovedNotice.CreateException();

    #region 布局与采样

    /// <summary>每拍对应的像素高度（已移除）</summary>
    public float PixelsPerBeat
    {
        get => throw RenderRemovedNotice.CreateException();
        set => throw RenderRemovedNotice.CreateException();
    }

    /// <summary>每个通道的宽度（已移除）</summary>
    public int ChannelWidth
    {
        get => throw RenderRemovedNotice.CreateException();
        set => throw RenderRemovedNotice.CreateException();
    }

    /// <summary>每个事件的采样点数（已移除）</summary>
    public int SamplesPerEvent
    {
        get => throw RenderRemovedNotice.CreateException();
        set => throw RenderRemovedNotice.CreateException();
    }

    /// <summary>每拍的细分格线数（已移除）</summary>
    public int BeatSubdivisions
    {
        get => throw RenderRemovedNotice.CreateException();
        set => throw RenderRemovedNotice.CreateException();
    }

    /// <summary>曲线描边宽度（已移除）</summary>
    public float StrokeWidth
    {
        get => throw RenderRemovedNotice.CreateException();
        set => throw RenderRemovedNotice.CreateException();
    }

    /// <summary>左侧留白宽度（已移除）</summary>
    public int LeftMargin
    {
        get => throw RenderRemovedNotice.CreateException();
        set => throw RenderRemovedNotice.CreateException();
    }

    /// <summary>顶部留白高度（已移除）</summary>
    public int HeaderHeight
    {
        get => throw RenderRemovedNotice.CreateException();
        set => throw RenderRemovedNotice.CreateException();
    }

    /// <summary>底部留白（已移除）</summary>
    public int BottomPadding
    {
        get => throw RenderRemovedNotice.CreateException();
        set => throw RenderRemovedNotice.CreateException();
    }

    /// <summary>通道之间的间距（已移除）</summary>
    public int ChannelPadding
    {
        get => throw RenderRemovedNotice.CreateException();
        set => throw RenderRemovedNotice.CreateException();
    }

    #endregion

    #region 值域与分段

    /// <summary>采样范围填充比例（已移除）</summary>
    public double RangePaddingRatio
    {
        get => throw RenderRemovedNotice.CreateException();
        set => throw RenderRemovedNotice.CreateException();
    }

    /// <summary>每个事件用于值域探测的采样点数（已移除）</summary>
    public int RangeSamplesPerEvent
    {
        get => throw RenderRemovedNotice.CreateException();
        set => throw RenderRemovedNotice.CreateException();
    }

    /// <summary>段分组容差（已移除）</summary>
    public double SegmentGroupTolerance
    {
        get => throw RenderRemovedNotice.CreateException();
        set => throw RenderRemovedNotice.CreateException();
    }

    /// <summary>最小值域半宽（已移除）</summary>
    public double MinValueRangeHalf
    {
        get => throw RenderRemovedNotice.CreateException();
        set => throw RenderRemovedNotice.CreateException();
    }

    /// <summary>最小值域半宽比例（已移除）</summary>
    public double MinValueRangeHalfRatio
    {
        get => throw RenderRemovedNotice.CreateException();
        set => throw RenderRemovedNotice.CreateException();
    }

    #endregion

    #region 颜色

    /// <summary>背景颜色（已移除）</summary>
    public SKColor BackgroundColor
    {
        get => throw RenderRemovedNotice.CreateException();
        set => throw RenderRemovedNotice.CreateException();
    }

    /// <summary>整拍格线颜色（已移除）</summary>
    public SKColor BeatGridColor
    {
        get => throw RenderRemovedNotice.CreateException();
        set => throw RenderRemovedNotice.CreateException();
    }

    /// <summary>半拍格线颜色（已移除）</summary>
    public SKColor SubBeatGridColor
    {
        get => throw RenderRemovedNotice.CreateException();
        set => throw RenderRemovedNotice.CreateException();
    }

    /// <summary>通道背景颜色（已移除）</summary>
    public SKColor ChannelBackgroundColor
    {
        get => throw RenderRemovedNotice.CreateException();
        set => throw RenderRemovedNotice.CreateException();
    }

    /// <summary>通道中心参考线颜色（已移除）</summary>
    public SKColor CenterLineColor
    {
        get => throw RenderRemovedNotice.CreateException();
        set => throw RenderRemovedNotice.CreateException();
    }

    /// <summary>事件背景块颜色（已移除）</summary>
    public SKColor EventBlockColor
    {
        get => throw RenderRemovedNotice.CreateException();
        set => throw RenderRemovedNotice.CreateException();
    }

    /// <summary>事件间连接线颜色（已移除）</summary>
    public SKColor ConnectionColor
    {
        get => throw RenderRemovedNotice.CreateException();
        set => throw RenderRemovedNotice.CreateException();
    }

    /// <summary>标注文字颜色（已移除）</summary>
    public SKColor TextColor
    {
        get => throw RenderRemovedNotice.CreateException();
        set => throw RenderRemovedNotice.CreateException();
    }

    /// <summary>MoveX 通道颜色（已移除）</summary>
    public SKColor MoveXColor
    {
        get => throw RenderRemovedNotice.CreateException();
        set => throw RenderRemovedNotice.CreateException();
    }

    /// <summary>MoveY 通道颜色（已移除）</summary>
    public SKColor MoveYColor
    {
        get => throw RenderRemovedNotice.CreateException();
        set => throw RenderRemovedNotice.CreateException();
    }

    /// <summary>Rotate 通道颜色（已移除）</summary>
    public SKColor RotateColor
    {
        get => throw RenderRemovedNotice.CreateException();
        set => throw RenderRemovedNotice.CreateException();
    }

    /// <summary>Alpha 通道颜色（已移除）</summary>
    public SKColor AlphaColor
    {
        get => throw RenderRemovedNotice.CreateException();
        set => throw RenderRemovedNotice.CreateException();
    }

    /// <summary>Speed 通道颜色（已移除）</summary>
    public SKColor SpeedColor
    {
        get => throw RenderRemovedNotice.CreateException();
        set => throw RenderRemovedNotice.CreateException();
    }

    #endregion
}
