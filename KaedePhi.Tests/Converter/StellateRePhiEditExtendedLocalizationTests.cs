using System.Globalization;
using KaedePhi.Core.Formats.RePhiEdit.Model;
using KaedePhi.Core.Formats.RePhiEdit.Model.Events;
using KaedePhi.Core.Primitives;
using KaedePhi.Tool.Converter.RePhiEdit.Model;
using KaedePhi.Tool.Converter.StellateRePhiEditExtended;
using IrBlockArea = KaedePhi.Core.Intermediate.Model.BlockArea;
using IrBpmItem = KaedePhi.Core.Intermediate.Model.BpmItem;
using IrChart = KaedePhi.Core.Intermediate.Model.Chart;
using IrEasing = KaedePhi.Core.Intermediate.Model.Easing;
using IrEvent = KaedePhi.Core.Intermediate.Model.Events.Event<double>;
using IrJudgeLine = KaedePhi.Core.Intermediate.Model.JudgeLine;
using RpeChart = KaedePhi.Core.Formats.RePhiEdit.Model.Chart;
using RpeEvent = KaedePhi.Core.Formats.RePhiEdit.Model.Events.Event<float>;
using RpeJudgeLine = KaedePhi.Core.Formats.RePhiEdit.Model.JudgeLine;

namespace KaedePhi.Tests.Converter;

public class StellateRePhiEditExtendedLocalizationTests
{
    [Theory]
    [InlineData(
        "en-US",
        "The display phase order of IR block area 0 is invalid and cannot be converted to StellateRePhiEditExtended."
    )]
    [InlineData("zh-Hant", "IR 噪域 0 的顯示階段順序無效，無法轉換為 StellateRePhiEditExtended。")]
    [InlineData("", "IR 噪域 0 的显示阶段顺序无效，无法转换为 StellateRePhiEditExtended。")]
    public void ExportLocalizesErrorsByUiCulture(string cultureName, string expected)
    {
        var chart = CreateChart(area => area.AppearBeat = new Beat(5));

        var exception = WithUiCulture(
            cultureName,
            () => Assert.Throws<FormatException>(() => Export(chart))
        );

        Assert.Equal(expected, exception.Message);
    }

    [Fact]
    public void ExportLocalizesTrackNamesInsideErrors()
    {
        var chart = CreateChart(area =>
        {
            area.RotateEvents = [CreateIrEvent(4, 2)];
            area.MoveXEvents = [CreateIrEvent(0, 1, bezierPoints: [0.1f, 0.2f])];
        });

        var bezier = Assert.Throws<FormatException>(() => Export(chart));
        Assert.Equal(
            "The X-axis movement Bezier control points of IR block area 0 are invalid.",
            bezier.Message
        );

        chart.BlockAreaList[0].MoveXEvents = null;
        var order = Assert.Throws<FormatException>(() => Export(chart));
        Assert.Equal("A rotation event of IR block area 0 ends before it starts.", order.Message);
    }

    [Fact]
    public void ExportRejectsOversizedBlockAreasWithLocalizedMessage()
    {
        var chart = CreateChart(area =>
        {
            area.TopRightX = double.MaxValue;
            area.BottomLeftX = -double.MaxValue;
        });

        var exception = Assert.Throws<FormatException>(() => Export(chart));

        Assert.Equal(
            "The IR block area size is outside the range RePhiEdit can encode.",
            exception.Message
        );
    }

    [Fact]
    public void ExportLocalizesValueNamesInsideErrors()
    {
        var chart = CreateChart(area => area.TopRightX = 1e39d);

        var exception = Assert.Throws<FormatException>(() => Export(chart));

        Assert.Equal(
            "The width of IR block area 0 is outside the range RePhiEdit can encode.",
            exception.Message
        );
    }

    [Fact]
    public void ExportRejectsNonFiniteCoordinatesWithLocalizedMessage()
    {
        var chart = CreateChart(area => area.TopRightY = double.NaN);

        var exception = Assert.Throws<FormatException>(() => Export(chart));

        Assert.Equal("IR block area coordinates must be finite numbers.", exception.Message);
    }

    [Fact]
    public void ExportWarnsLocalizedWhenAnchorEventsAreBaked()
    {
        var chart = CreateChart(area => area.RotateAnchorXEvents = [CreateIrEvent(0, 1)]);
        var warnings = new List<string>();

        Export(chart, warnings, precision: 8);

        Assert.Contains(
            "The anchor transform of IR block area 0 was sampled at 8 segments per beat and baked into position events; the anchor events themselves are not preserved.",
            warnings
        );
    }

    [Fact]
    public void ExportWarnsLocalizedWhenJudgeLineUsesMarkerTexture()
    {
        var chart = CreateChart(_ => { });
        chart.JudgeLineList[0].Texture = "isSubtract0.png";
        var warnings = new List<string>();

        Export(chart, warnings);

        Assert.Contains(
            "IR judge line 0 uses a block area marker texture and will be converted to a block area when the exported chart is imported again.",
            warnings
        );
    }

    [Fact]
    public void ImportWarnsLocalizedWhenMarkerLineHasNotesOrChildren()
    {
        var marker = CreateMarkerLine();
        marker.Notes = [new Note { StartBeat = new Beat(1), EndBeat = new Beat(1) }];
        var child = new RpeJudgeLine { Father = 0 };
        var source = new RpeChart
        {
            BpmList = [new() { Bpm = 120f, StartBeat = new Beat(0) }],
            JudgeLineList = [marker, child],
        };
        var warnings = new List<string>();

        new StellateRePhiEditExtendedConverter { OnWarning = warnings.Add }.ToIr(source);

        Assert.Contains(
            "Notes on StellateRePhiEditExtended judge line 0 are ignored because the line is converted to a block area.",
            warnings
        );
        Assert.Contains(
            "The parent of StellateRePhiEditExtended judge line 1 was converted to a block area, so the parent-child transform cannot be preserved.",
            warnings
        );
    }

    [Fact]
    public void ImportRejectsInvalidParentIndexWithLocalizedMessage()
    {
        var source = new RpeChart
        {
            BpmList = [new() { Bpm = 120f, StartBeat = new Beat(0) }],
            JudgeLineList = [new RpeJudgeLine { Father = 7 }],
        };

        var exception = Assert.Throws<FormatException>(() =>
            new StellateRePhiEditExtendedConverter().ToIr(source)
        );

        Assert.Equal("RePhiEdit judge line 0 has an invalid parent index.", exception.Message);
    }

    [Fact]
    public void ImportRejectsCyclicMarkerParentsWithLocalizedMessage()
    {
        var first = CreateMarkerLine();
        var second = CreateMarkerLine();
        first.Father = 1;
        second.Father = 0;
        var source = new RpeChart
        {
            BpmList = [new() { Bpm = 120f, StartBeat = new Beat(0) }],
            JudgeLineList = [first, second, new RpeJudgeLine { Father = 0 }],
        };

        var exception = Assert.Throws<FormatException>(() =>
            new StellateRePhiEditExtendedConverter().ToIr(source)
        );

        Assert.Equal(
            "The RePhiEdit judge line parent hierarchy contains a cycle.",
            exception.Message
        );
    }

    private static void Export(
        IrChart chart,
        List<string>? warnings = null,
        int precision = ConvertOption.DefaultPrecision
    )
    {
        var option = new ConvertOption();
        option.Cutting.UnsupportedEasingPrecision = precision;
        var converter = new StellateRePhiEditExtendedConverter();
        if (warnings is not null)
            converter.OnWarning = warnings.Add;
        converter.FromIr(chart, option);
    }

    private static T WithUiCulture<T>(string cultureName, Func<T> action)
    {
        var original = CultureInfo.CurrentUICulture;
        try
        {
            CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo(cultureName);
            return action();
        }
        finally
        {
            CultureInfo.CurrentUICulture = original;
        }
    }

    private static IrChart CreateChart(Action<IrBlockArea> configure)
    {
        var area = new IrBlockArea
        {
            TopRightX = 0.5d,
            TopRightY = 0.5d,
            BottomLeftX = -0.5d,
            BottomLeftY = -0.5d,
            AppearBeat = new Beat(1),
            EnableBeat = new Beat(2),
            DisableBeat = new Beat(3),
            DisappearBeat = new Beat(4),
        };
        configure(area);
        return new IrChart
        {
            BpmList = [new IrBpmItem { Bpm = 120f, StartBeat = new Beat(0) }],
            JudgeLineList = [new IrJudgeLine()],
            BlockAreaList = [area],
        };
    }

    private static IrEvent CreateIrEvent(int startBeat, int endBeat, float[]? bezierPoints = null) =>
        new()
        {
            StartBeat = new Beat(startBeat),
            EndBeat = new Beat(endBeat),
            StartValue = 0d,
            EndValue = 1d,
            Easing = new IrEasing(1),
            IsBezier = bezierPoints is not null,
            BezierPoints = bezierPoints ?? [],
        };

    private static RpeJudgeLine CreateMarkerLine() =>
        new()
        {
            Texture = "isSubtract0.png",
            EventLayers =
            [
                new EventLayer
                {
                    SpeedEvents =
                    [
                        CreateMarkerEvent(0, 1f),
                        CreateMarkerEvent(1, 2f),
                        CreateMarkerEvent(2, 3f),
                        CreateMarkerEvent(3, 4f),
                    ],
                },
            ],
        };

    private static RpeEvent CreateMarkerEvent(int beat, float phase) =>
        new()
        {
            StartBeat = new Beat(beat),
            EndBeat = new Beat(beat),
            StartValue = phase,
            EndValue = phase,
        };
}
