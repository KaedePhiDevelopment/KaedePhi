using KaedePhi.Core.Primitives;
using KaedePhi.Tool.Common;
using KaedePhi.Tool.JudgeLine;
using KaedePhi.Tool.JudgeLine.Intermediate;
using IrEvents = KaedePhi.Core.Intermediate.Model.Events;
using JudgeLine = KaedePhi.Core.Intermediate.Model.JudgeLine;

namespace KaedePhi.Tests.JudgeLines;

public sealed class JudgeLineUnbinderContractTests
{
    [Fact]
    public void AdaptiveUnbind_IsAvailableThroughInterface()
    {
        IJudgeLineUnbindService<JudgeLine> unbinder = new JudgeLineUnbinder();
        var lines = new List<JudgeLine> { new() };

        var result = unbinder.Unbind(
            lines[0],
            _ => null,
            new JudgeLineUnbindOptions
            {
                SamplingMode = JudgeLineUnbindSamplingMode.Adaptive,
                Precision = 16d,
                Tolerance = 0.1d,
                MergeTolerance = 0.2d,
            },
            cancellationToken: TestContext.Current.CancellationToken
        );

        result.Father.Should().Be(-1);
    }

    [Fact]
    public void AdaptiveUnbindWithRenderProfile_IsAvailableThroughInterface()
    {
        IJudgeLineUnbindService<JudgeLine> unbinder = new JudgeLineUnbinder();
        var lines = new List<JudgeLine> { new() };

        var result = unbinder.Unbind(
            lines[0],
            _ => null,
            new JudgeLineUnbindOptions
            {
                SamplingMode = JudgeLineUnbindSamplingMode.Adaptive,
                Precision = 16d,
                Tolerance = 100d,
                MergeTolerance = 0.2d,
                RenderProfile = CoordinateProfile.DefaultRenderProfile,
            },
            cancellationToken: TestContext.Current.CancellationToken
        );

        result.Father.Should().Be(-1);
    }

    [Fact]
    public void UnbindWithOptions_UsesTheConfiguredSamplingStrategy()
    {
        IJudgeLineUnbindService<JudgeLine> unbinder = new JudgeLineUnbinder();
        var equalSpacingLines = CreateLines();

        var equalSpacingResult = unbinder.Unbind(
            equalSpacingLines[1],
            line => line.Father >= 0 ? equalSpacingLines[line.Father] : null,
            new JudgeLineUnbindOptions
            {
                SamplingMode = JudgeLineUnbindSamplingMode.EqualSpacing,
                Precision = 4d,
            },
            cancellationToken: TestContext.Current.CancellationToken
        );
        var adaptiveLines = CreateLines();
        var adaptiveResult = unbinder.Unbind(
            adaptiveLines[1],
            line => line.Father >= 0 ? adaptiveLines[line.Father] : null,
            new JudgeLineUnbindOptions
            {
                SamplingMode = JudgeLineUnbindSamplingMode.Adaptive,
                Precision = 4d,
                Tolerance = 0.1d,
                MergeTolerance = 0.1d,
            },
            cancellationToken: TestContext.Current.CancellationToken
        );
        var adaptiveWithProfileLines = CreateLines();
        var adaptiveWithProfileResult = unbinder.Unbind(
            adaptiveWithProfileLines[1],
            line => line.Father >= 0 ? adaptiveWithProfileLines[line.Father] : null,
            new JudgeLineUnbindOptions
            {
                SamplingMode = JudgeLineUnbindSamplingMode.Adaptive,
                Precision = 4d,
                Tolerance = 0.1d,
                MergeTolerance = 0.1d,
                RenderProfile = CoordinateProfile.DefaultRenderProfile,
            },
            cancellationToken: TestContext.Current.CancellationToken
        );

        equalSpacingResult.EventLayers[0].MoveXEvents.Should().HaveCount(4);
        adaptiveResult.EventLayers[0].MoveXEvents.Should().HaveCount(1);
        adaptiveWithProfileResult.EventLayers[0].MoveXEvents.Should().HaveCount(1);
    }

    private static List<JudgeLine> CreateLines() =>
        [
            new JudgeLine
            {
                EventLayers =
                [
                    new IrEvents.EventLayer
                    {
                        MoveXEvents =
                        [
                            new IrEvents.Event<double>
                            {
                                StartBeat = new Beat(0d),
                                EndBeat = new Beat(1d),
                                StartValue = 0d,
                                EndValue = 100d,
                            },
                        ],
                    },
                ],
            },
            new JudgeLine { Father = 0, EventLayers = [new IrEvents.EventLayer()] },
        ];
}
