using KaedePhi.Core.Intermediate.Model.Controls;
using KaedePhi.Tool.Common;

namespace KaedePhi.Tests.Common;

public class ControlDefaultCheckerTests
{
    [Fact]
    public void DefaultControls_HaveDefaultEffectAndMatchExactly()
    {
        AssertDefaultControls(XControl.Default);
        AssertDefaultControls(AlphaControl.Default);
        AssertDefaultControls(SizeControl.Default);
        AssertDefaultControls(SkewControl.Default);
        AssertDefaultControls(YControl.Default);
    }

    [Fact]
    public void DefaultValues_IgnoreControlPositionsOrderAndEasing()
    {
        AssertDefaultValues(XControl.Default);
        AssertDefaultValues(AlphaControl.Default);
        AssertDefaultValues(SizeControl.Default);
        AssertDefaultValues(SkewControl.Default);
        AssertDefaultValues(YControl.Default);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(3)]
    public void DefaultValues_IgnoreControlCount(int count)
    {
        var controls = Enumerable.Range(0, count).Select(i => new XControl { X = i }).ToList();

        ControlDefaultChecker.HasDefaultEffect(controls).Should().BeTrue();
        ControlDefaultChecker.IsDefaultControls(controls).Should().BeFalse();
    }

    [Fact]
    public void MissingControls_HaveDefaultEffectButDoNotMatchExactly()
    {
        ControlDefaultChecker.HasDefaultEffect<XControl>(null).Should().BeTrue();
        ControlDefaultChecker.IsDefaultControls<XControl>(null).Should().BeFalse();
    }

    [Theory]
    [InlineData(1.1f)]
    [InlineData(float.NaN)]
    [InlineData(float.PositiveInfinity)]
    [InlineData(float.NegativeInfinity)]
    public void NonDefaultValues_DoNotHaveDefaultEffect(float value)
    {
        ControlDefaultChecker
            .HasDefaultEffect(new List<XControl> { new(), new() { Pos = value } })
            .Should()
            .BeFalse();
        ControlDefaultChecker
            .HasDefaultEffect(new List<AlphaControl> { new(), new() { Alpha = value } })
            .Should()
            .BeFalse();
        ControlDefaultChecker
            .HasDefaultEffect(new List<SizeControl> { new(), new() { Size = value } })
            .Should()
            .BeFalse();
        ControlDefaultChecker
            .HasDefaultEffect(new List<SkewControl> { new() { Skew = 0f }, new() { Skew = value } })
            .Should()
            .BeFalse();
        ControlDefaultChecker
            .HasDefaultEffect(new List<YControl> { new(), new() { Y = value } })
            .Should()
            .BeFalse();
    }

    [Theory]
    [InlineData(float.NaN)]
    [InlineData(float.PositiveInfinity)]
    [InlineData(float.NegativeInfinity)]
    public void NonFiniteControlPosition_DoesNotHaveDefaultEffect(float position)
    {
        var controls = XControl.Default;
        controls[0].X = position;

        ControlDefaultChecker.HasDefaultEffect(controls).Should().BeFalse();
    }

    [Fact]
    public void DefaultValueComparison_UsesExistingFloatTolerance()
    {
        var controls = XControl.Default;
        controls[1].Pos += Constants.FloatEpsilon / 2f;
        ControlDefaultChecker.HasDefaultEffect(controls).Should().BeTrue();

        controls[1].Pos = 1f + Constants.FloatEpsilon * 2f;
        ControlDefaultChecker.HasDefaultEffect(controls).Should().BeFalse();
    }

    private static void AssertDefaultControls<T>(List<T> controls)
        where T : ControlBase
    {
        ControlDefaultChecker.HasDefaultEffect(controls).Should().BeTrue();
        ControlDefaultChecker.IsDefaultControls(controls).Should().BeTrue();
    }

    private static void AssertDefaultValues<T>(List<T> controls)
        where T : ControlBase
    {
        controls.Reverse();
        for (var i = 0; i < controls.Count; i++)
        {
            controls[i].X = 10f + i * 100f;
            controls[i].Easing = new KaedePhi.Core.Intermediate.Model.Easing(i + 2);
        }

        ControlDefaultChecker.HasDefaultEffect(controls).Should().BeTrue();
        ControlDefaultChecker.IsDefaultControls(controls).Should().BeFalse();
    }
}
