using KaedePhi.Core.Primitives.Mathematics;
using KaedePhi.Tool.Converter.StellateRePhiEditExtended.Utils;
using PhiEditEasings = KaedePhi.Core.Formats.PhiEdit.Model.Easings;
using RpeEasing = KaedePhi.Core.Formats.RePhiEdit.Model.Easing;
using RpeEvent = KaedePhi.Core.Formats.RePhiEdit.Model.Events.Event<float>;

namespace KaedePhi.Tests.Converter;

public class StellateRePhiEditEasingTests
{
    public static TheoryData<int> SupportedEasingTypes => new(Enumerable.Range(1, 28));

    [Theory]
    [MemberData(nameof(SupportedEasingTypes))]
    public void Evaluate_UsesCoreEasingDefinitions(int easingType)
    {
        var evt = new RpeEvent { Easing = new RpeEasing(easingType) };

        foreach (var progress in new[] { 0.05d, 0.2d, 0.5d, 0.75d, 0.95d })
        {
            StellateRePhiEditEasing
                .Evaluate(evt, progress, escape: true)
                .Should()
                .BeApproximately(PhiEditEasings.GetFunction(easingType)(progress), 1e-12);
        }
    }

    [Theory]
    [InlineData(-5, 5)]
    [InlineData(0, 1)]
    [InlineData(29, 1)]
    [InlineData(-29, 1)]
    public void Evaluate_NormalizesEasingTypeBeforeLookup(int easingType, int expectedType)
    {
        var evt = new RpeEvent { Easing = new RpeEasing(easingType) };

        StellateRePhiEditEasing
            .Evaluate(evt, 0.4d, escape: true)
            .Should()
            .BeApproximately(PhiEditEasings.GetFunction(expectedType)(0.4d), 1e-12);
    }

    [Fact]
    public void Evaluate_ClampsToEndpointsOutsideTheEscapedRange()
    {
        var evt = new RpeEvent { Easing = new RpeEasing(7) };

        StellateRePhiEditEasing.Evaluate(evt, -0.5d, escape: true).Should().Be(0d);
        StellateRePhiEditEasing.Evaluate(evt, 1.5d, escape: true).Should().Be(1d);
        StellateRePhiEditEasing.Evaluate(evt, 1.5d, escape: false).Should().Be(1d);
    }

    [Fact]
    public void Evaluate_ExtrapolatesBeforeStartWhenNotEscaped()
    {
        var evt = new RpeEvent { Easing = new RpeEasing(5) };

        StellateRePhiEditEasing
            .Evaluate(evt, -0.5d, escape: false)
            .Should()
            .BeApproximately(0.25d, 1e-12);
    }

    [Theory]
    [InlineData(0.1)]
    [InlineData(0.4)]
    [InlineData(0.85)]
    public void Evaluate_BezierEventUsesCoreBezierCurve(double progress)
    {
        float[] points = [0.25f, 0.1f, 0.25f, 1f];
        var evt = new RpeEvent { IsBezier = true, BezierPoints = points };

        StellateRePhiEditEasing
            .Evaluate(evt, progress, escape: true)
            .Should()
            .Be(Bezier.Evaluate(points, progress));
    }

    [Fact]
    public void Evaluate_BezierEventExtendsTheCurveBeforeStartWhenNotEscaped()
    {
        var evt = new RpeEvent
        {
            IsBezier = true,
            BezierPoints = [1f / 3f, 1f / 3f, 2f / 3f, 2f / 3f],
        };

        StellateRePhiEditEasing
            .Evaluate(evt, -0.25d, escape: false)
            .Should()
            .BeApproximately(-0.25d, 1e-5);
        StellateRePhiEditEasing.Evaluate(evt, -0.25d, escape: true).Should().Be(0d);
    }

    [Fact]
    public void Evaluate_DegenerateEasingRangeFallsBackToLinearProgress()
    {
        var evt = new RpeEvent
        {
            Easing = new RpeEasing(5),
            EasingLeft = 0.5f,
            EasingRight = 0.5f,
        };

        StellateRePhiEditEasing.Evaluate(evt, 0.3d, escape: true).Should().Be(0.3d);
    }

    [Fact]
    public void Evaluate_AppliesEasingRangeOfTheEvent()
    {
        var evt = new RpeEvent
        {
            Easing = new RpeEasing(5),
            EasingLeft = 0.5f,
            EasingRight = 1f,
        };

        StellateRePhiEditEasing
            .Evaluate(evt, 0.5d, escape: true)
            .Should()
            .BeApproximately((0.75d * 0.75d - 0.25d) / 0.75d, 1e-12);
    }
}
