using KaedePhi.Core.Intermediate.Model;
using KaedePhi.Tool.Common;
using KaedePhi.Tool.Converter;
using KaedePhi.Tool.Converter.RePhiEdit.Model;
using KaedePhi.Tool.Render.Intermediate;
using KaedePhi.Core.Primitives;
using IrChart = KaedePhi.Core.Intermediate.Model.Chart;
using IrEvents = KaedePhi.Core.Intermediate.Model.Events;
using IrControls = KaedePhi.Core.Intermediate.Model.Controls;

namespace KaedePhi.Tests.Compatibility;

/// <summary>
/// 验证废弃 KPC 兼容层的行为与迁移后的 IR 实现保持一致。
/// </summary>
public class KpcCompatibilityShimTests
{
    private const double Tolerance = 0.5;
    private static readonly Type Mapper = LegacyApi.ToolType("KaedePhi.Tool.Compatibility.KpcCompatibilityMapper");

    [Fact]
    public void KpcChartNormalizer_RoundTrip_PreservesChartData()
    {
        var chart = CreateChart();
        var legacyChart = ToLegacyChart(chart);

        var result = LegacyApi.Invoke(
            LegacyApi.ToolType("KaedePhi.Tool.Common.KpcChartNormalizer"),
            "NormalizeAndValidateNoteEndBeats", null, legacyChart
        )!;
        result.Should().NotBeSameAs(legacyChart);
        var normalized = (IrChart)LegacyApi.Invoke(Mapper, "ToIntermediate", null, result)!;

        normalized.Should().NotBeSameAs(chart);
        normalized.Meta.Name.Should().Be("compat-chart");
        normalized.Meta.Offset.Should().Be(-25);
        normalized.BpmList.Should().HaveCount(2);
        normalized.BpmList[1].Bpm.Should().Be(180f);
        ((double)normalized.BpmList[1].StartBeat).Should().Be(4d);

        normalized.JudgeLineList.Should().HaveCount(1);
        var line = normalized.JudgeLineList[0];
        line.Name.Should().Be("line-0");
        line.Father.Should().Be(-1);
        line.AttachUi.Should().Be(AttachUi.Pause);
        line.BpmFactor.Should().Be(2f);
        line.Anchor.Should().Equal(0.25f, 0.75f);
        line.EventLayers.Should().HaveCount(1);
        line.Notes.Should().HaveCount(2);
        line.Extended.TextEvents.Should().NotBeNull();
        line.PositionControls.Should().HaveCount(1);

        // 非 Hold 音符的结束拍被规范为起始拍
        line.Notes[0].Type.Should().Be(NoteType.Tap);
        line.Notes[0].EndBeat.Should().Be(line.Notes[0].StartBeat);
        // Hold 音符的结束拍保持不变
        line.Notes[1].Type.Should().Be(NoteType.Hold);
        ((double)line.Notes[1].EndBeat).Should().Be(6d);

        var speed = line.EventLayers[0].SpeedEvents![0];
        speed.StartValue.Should().Be(1.5f);
        speed.EndValue.Should().Be(2.5f);
        ((int)speed.Easing).Should().Be(5);
        speed.Font.Should().BeNull();

        var text = line.Extended.TextEvents![0];
        text.StartValue.Should().Be("start");
        text.EndValue.Should().Be("end");
        text.Font.Should().Be("font.ttf");

        var color = line.Extended.ColorEvents![0];
        color.StartValue.Should().Equal(1, 2, 3);
        color.EndValue.Should().Equal(4, 5, 6);

        line.PositionControls[0].Pos.Should().Be(0.5f);
        line.PositionControls[0].X.Should().Be(1.25f);
        ((int)line.PositionControls[0].Easing).Should().Be(7);
    }

    [Fact]
    public void KpcChartValidator_MappedHierarchy_DetectsCycle()
    {
        var lines = new List<JudgeLine>
        {
            new() { Father = 1 },
            new() { Father = 0 },
        };
        var legacyLines = LegacyApi.ConvertTo(
            lines, typeof(List<>).MakeGenericType(LegacyApi.CoreType("KaedePhi.Core.KaedePhi.JudgeLine"))
        );

        var act = () => LegacyApi.Invoke(
            LegacyApi.ToolType("KaedePhi.Tool.Common.KpcChartValidator"),
            "ValidateJudgeLineHierarchy", null, legacyLines
        );

        act.Should().Throw<FormatException>();
    }

    [Fact]
    public void KpcRenderValidator_MappedChart_ValidatesLikeIrImplementation()
    {
        var chart = CreateChart();
        var options = (IrRenderOptions)Activator.CreateInstance(
            LegacyApi.ToolType("KaedePhi.Tool.Render.KaedePhi.KpcRenderOptions")
        )!;

        var legacyAct = () => LegacyApi.Invoke(
            LegacyApi.ToolType("KaedePhi.Tool.Common.KpcRenderValidator"),
            "Validate", null, ToLegacyChart(chart), options, null, null
        );

        var irAct = () => IrRenderValidator.Validate(chart, options);

        legacyAct.Should().NotThrow();
        irAct.Should().NotThrow();
    }

    [Fact]
    public void EventFit_LegacyOverload_MatchesIrImplementation()
    {
        var irEvents = new List<IrEvents.Event<double>>
        {
            CreateEvent(0, 1, 0, 100),
            CreateEvent(1, 2, 100, 200),
            CreateEvent(2, 3, 200, 300),
        };
        var legacyEvents = LegacyApi.InvokeGeneric(Mapper, "ToKpc", [typeof(double)], null, irEvents)!;

        var legacyType = LegacyApi.ToolType("KaedePhi.Tool.Event.KaedePhi.EventFit`1")
            .MakeGenericType(typeof(double));
        var result = LegacyApi.Invoke(
            legacyType, "FitEvents", Activator.CreateInstance(legacyType), legacyEvents, Tolerance
        )!;
        var legacyResult = (List<IrEvents.Event<double>>)LegacyApi.InvokeGeneric(
            Mapper, "ToIntermediate", [typeof(double)], null, result
        )!;
        var irResult = new KaedePhi.Tool.Event.Intermediate.EventFit<double>().FitEvents(
            irEvents,
            Tolerance
        );

        legacyResult.Should().HaveCount(irResult.Count);
        for (var i = 0; i < legacyResult.Count; i++)
        {
            ((double)legacyResult[i].StartBeat).Should().Be((double)irResult[i].StartBeat);
            ((double)legacyResult[i].EndBeat).Should().Be((double)irResult[i].EndBeat);
            legacyResult[i].StartValue.Should().Be(irResult[i].StartValue);
            legacyResult[i].EndValue.Should().Be(irResult[i].EndValue);
            ((int)legacyResult[i].Easing).Should().Be((int)irResult[i].Easing);
        }
    }

    [Fact]
    public async Task ChartFormatDescriptor_LegacyImportExport_RoundTrips()
    {
        var chart = CreateChart();
        var descriptor = ChartFormatRegistry.Get(ChartFormatIds.RePhiEdit);
        var path = Path.Combine(Path.GetTempPath(), $"kpc-compat-{Guid.NewGuid():N}.json");

        try
        {
            await LegacyApi.AwaitResult(LegacyApi.Invoke(
                typeof(ChartFormatDescriptor), "ExportAsync", descriptor,
                ToLegacyChart(chart), path, null, new ConvertOption(), null, TestContext.Current.CancellationToken
            ));

            var text = await File.ReadAllTextAsync(path, TestContext.Current.CancellationToken);
            var result = (await LegacyApi.AwaitResult(LegacyApi.Invoke(
                typeof(ChartFormatDescriptor), "ImportAsync", descriptor,
                text, null, null, TestContext.Current.CancellationToken
            )))!;
            var imported = (IrChart)LegacyApi.Invoke(Mapper, "ToIntermediate", null, result)!;

            imported.Meta.Name.Should().Be(chart.Meta.Name);
            imported.BpmList.Should().HaveCount(chart.BpmList.Count);
            imported.JudgeLineList.Should().HaveCount(chart.JudgeLineList.Count);
            imported.JudgeLineList[0].Notes.Should().HaveCount(chart.JudgeLineList[0].Notes.Count);
        }
        finally
        {
            if (File.Exists(path))
                File.Delete(path);
        }
    }

    [Fact]
    public async Task ChartFormatDescriptor_LegacyOptions_AreAccepted()
    {
        var chart = CreateChart();
        var descriptor = ChartFormatRegistry.Get(ChartFormatIds.PhiEdit);

        var export = () =>
            LegacyApi.AwaitResult(LegacyApi.Invoke(
                typeof(ChartFormatDescriptor), "ExportAsync", descriptor,
                ToLegacyChart(chart),
                "unused.pec",
                new ChartWriteSettings { DryRun = true },
                Activator.CreateInstance(LegacyApi.ToolType("KaedePhi.Tool.Converter.PhiEdit.Model.KpcToPhiEditConvertOptions")),
                null,
                TestContext.Current.CancellationToken
            ));

        await export.Should().NotThrowAsync();
    }

    private static object ToLegacyChart(IrChart chart) =>
        LegacyApi.Invoke(Mapper, "ToKpc", null, chart)!;

    private static IrChart CreateChart()
    {
        var line = new JudgeLine
        {
            Name = "line-0",
            Father = -1,
            AttachUi = AttachUi.Pause,
            BpmFactor = 2f,
            Anchor = [0.25f, 0.75f],
            Extended = new IrEvents.ExtendLayer
            {
                TextEvents =
                [
                    new IrEvents.Event<string>
                    {
                        StartBeat = new Beat(0d),
                        EndBeat = new Beat(1d),
                        StartValue = "start",
                        EndValue = "end",
                        Font = "font.ttf",
                    },
                ],
                ColorEvents =
                [
                    new IrEvents.Event<byte[]>
                    {
                        StartBeat = new Beat(0d),
                        EndBeat = new Beat(1d),
                        StartValue = [1, 2, 3],
                        EndValue = [4, 5, 6],
                    },
                ],
            },
            EventLayers =
            [
                new IrEvents.EventLayer
                {
                    SpeedEvents =
                    [
                        new IrEvents.Event<float>
                        {
                            StartBeat = new Beat(0d),
                            EndBeat = new Beat(2d),
                            StartValue = 1.5f,
                            EndValue = 2.5f,
                            Easing = new Easing(5),
                        },
                    ],
                },
            ],
            Notes =
            [
                new Note
                {
                    Type = NoteType.Tap,
                    StartBeat = new Beat(1d),
                    EndBeat = new Beat(2d),
                    PositionX = 0.5,
                    Tint = [10, 20, 30],
                },
                new Note
                {
                    Type = NoteType.Hold,
                    StartBeat = new Beat(3d),
                    EndBeat = new Beat(6d),
                    PositionX = -0.5,
                },
            ],
            PositionControls =
            [
                new IrControls.XControl
                {
                    Pos = 0.5f,
                    X = 1.25f,
                    Easing = new Easing(7),
                },
            ],
        };

        return new IrChart
        {
            Meta = new Meta
            {
                Name = "compat-chart",
                Author = "compat-author",
                Offset = -25,
            },
            BpmList =
            [
                new BpmItem { Bpm = 120f, StartBeat = new Beat(0d) },
                new BpmItem { Bpm = 180f, StartBeat = new Beat(4d) },
            ],
            JudgeLineList = [line],
        };
    }

    private static IrEvents.Event<double> CreateEvent(
        double startBeat,
        double endBeat,
        double startValue,
        double endValue
    ) =>
        new()
        {
            StartBeat = new Beat(startBeat),
            EndBeat = new Beat(endBeat),
            StartValue = startValue,
            EndValue = endValue,
        };

}
