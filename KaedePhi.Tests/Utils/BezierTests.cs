using KaedePhi.Core.Primitives.Mathematics;

namespace KaedePhi.Tests.Utils;

public class BezierTests
{
    private static readonly float[] LinearPoints = [1f / 3f, 1f / 3f, 2f / 3f, 2f / 3f];

    [Theory]
    [InlineData(0.1)]
    [InlineData(0.37)]
    [InlineData(0.5)]
    [InlineData(0.92)]
    public void Evaluate_InsideUnitRange_MatchesDo(double progress)
    {
        float[] points = [0.25f, 0.1f, 0.25f, 1f];

        var expected = Bezier.Do(points, (float)progress, 0d, 1d);

        Bezier.Evaluate(points, progress).Should().BeApproximately(expected, 1e-6);
    }

    [Theory]
    [InlineData(-0.5)]
    [InlineData(-0.1)]
    [InlineData(1.25)]
    [InlineData(1.5)]
    public void Evaluate_OutsideUnitRange_ExtendsTheCurveInsteadOfTruncating(double progress)
    {
        Bezier.Evaluate(LinearPoints, progress).Should().BeApproximately(progress, 1e-5);
    }

    [Fact]
    public void Evaluate_AtEndpoints_ReturnsEndpointValues()
    {
        float[] points = [0.42f, 0f, 0.58f, 1f];

        Bezier.Evaluate(points, 0d).Should().BeApproximately(0d, 1e-6);
        Bezier.Evaluate(points, 1d).Should().BeApproximately(1d, 1e-6);
    }

    [Fact]
    public void Do_OutsideUnitRange_StillTruncatesToEndpointValues()
    {
        Bezier.Do(LinearPoints, -0.5f, 10d, 20d).Should().Be(10d);
        Bezier.Do(LinearPoints, 1.5f, 10d, 20d).Should().Be(20d);
    }

    [Theory]
    [InlineData(null)]
    [InlineData(new float[] { 0f, 1f })]
    public void Evaluate_WithoutFourControlValues_Throws(float[]? points)
    {
        var act = () => Bezier.Evaluate(points!, 0.5d);

        act.Should().Throw<ArgumentException>();
    }
}
