using System.Text;
using KaedePhi.Core.Primitives;
using KaedePhi.Tests.Compatibility;
using Newtonsoft.Json.Linq;
using Pc = KaedePhi.Core.Formats.PhiChain.v6.Model;
using PcSerialization = KaedePhi.Core.Formats.PhiChain.v6.Serialization.ChartSerialization;
using Pf = KaedePhi.Core.Formats.PhiFans.Model;
using PfSerialization = KaedePhi.Core.Formats.PhiFans.Serialization.ChartSerialization;
using Ph = KaedePhi.Core.Formats.Phigros.v3.Model;
using PhSerialization = KaedePhi.Core.Formats.Phigros.v3.Serialization.ChartSerialization;

namespace KaedePhi.Tests.Serialization;

public class JsonCodecCompatibilityTests
{
    [Fact]
    public void PhiFansJsonCodecs_RoundTripBeatArraysAndLeaveStreamsOpen()
    {
        var formatChart = new Pf.Chart
        {
            JudgeLineList =
            [
                new Pf.Line
                {
                    NoteList =
                    [
                        new Pf.Note
                        {
                            Beat = new Beat([2, 1, 4]),
                            HoldEndBeat = new Beat([3, 0, 1]),
                        },
                    ],
                },
            ],
        };

        var formatJson = PfSerialization.ExportToJson(formatChart, false);
        var formatBeat = JObject.Parse(formatJson)["lines"]![0]!["notes"]![0]![
            "beat"
        ]!.ToObject<int[]>();
        Assert.Equal(new[] { 2, 1, 4 }, formatBeat);
        Assert.Equal(
            new Beat([2, 1, 4]),
            PfSerialization.LoadFromJson(formatJson).JudgeLineList[0].NoteList[0].Beat
        );

        using var stream = new MemoryStream();
        PfSerialization.ExportToJsonStream(formatChart, stream, false);
        Assert.True(stream.CanWrite);
        var bytes = stream.ToArray();
        Assert.False(bytes.AsSpan().StartsWith(new byte[] { 0xef, 0xbb, 0xbf }));
        stream.Position = 0;
        Assert.Equal(
            new Beat([3, 0, 1]),
            PfSerialization.LoadFromStream(stream).JudgeLineList[0].NoteList[0].HoldEndBeat
        );
        Assert.True(stream.CanRead);

        using var utf16Stream = new MemoryStream(
            Encoding.Unicode.GetPreamble().Concat(Encoding.Unicode.GetBytes(formatJson)).ToArray()
        );
        Assert.Equal(
            new Beat([2, 1, 4]),
            PfSerialization.LoadFromStream(utf16Stream).JudgeLineList[0].NoteList[0].Beat
        );
        Assert.True(utf16Stream.CanRead);

        var legacyChart = LegacyApi.ConvertTo(new Pf.Chart
        {
            JudgeLineList =
            [
                new Pf.Line
                {
                    NoteList =
                    [
                        new Pf.Note
                        {
                            Beat = new Beat([4, 1, 2]),
                            HoldEndBeat = new Beat([5, 0, 1]),
                        },
                    ],
                },
            ],
        }, LegacyApi.CoreType("KaedePhi.Core.PhiFans.Chart"));
        var legacyJson = (string)LegacyApi.Invoke(legacyChart.GetType(), "ExportToJson", legacyChart, false)!;
        var loaded = (Pf.Chart)LegacyApi.ConvertTo(
            LegacyApi.Invoke(legacyChart.GetType(), "LoadFromJson", null, legacyJson)!,
            typeof(Pf.Chart)
        );
        Assert.Equal(
            new Beat([4, 1, 2]),
            loaded.JudgeLineList[0].NoteList[0].Beat
        );
    }

    [Fact]
    public void PhigrosJsonCodecs_RoundTripNumericNoteTypes()
    {
        var formatChart = new Ph.Chart
        {
            JudgeLineList =
            [
                new Ph.JudgeLine
                {
                    NotesAbove =
                    [
                        new Ph.Note
                        {
                            Type = Ph.NoteType.Hold,
                            HoldTime = 32,
                            Time = 64,
                        },
                    ],
                },
            ],
        };

        var formatJson = PhSerialization.ExportToJson(formatChart, false);
        Assert.Equal(3, JObject.Parse(formatJson)["judgeLineList"]![0]!["notesAbove"]![0]!["type"]);
        Assert.Equal(
            Ph.NoteType.Hold,
            PhSerialization.LoadFromJson(formatJson).JudgeLineList[0].NotesAbove[0].Type
        );

        var legacyChart = LegacyApi.ConvertTo(new Ph.Chart
        {
            JudgeLineList =
            [
                new Ph.JudgeLine
                {
                    NotesAbove =
                    [
                        new Ph.Note
                        {
                            Type = Ph.NoteType.Hold,
                            HoldTime = 32,
                            Time = 64,
                        },
                    ],
                },
            ],
        }, LegacyApi.CoreType("KaedePhi.Core.Phigros.v3.Chart"));

        var legacyJson = (string)LegacyApi.Invoke(legacyChart.GetType(), "ExportToJson", legacyChart, false)!;
        Assert.Equal(3, JObject.Parse(legacyJson)["judgeLineList"]![0]!["notesAbove"]![0]!["type"]);
        var loaded = (Ph.Chart)LegacyApi.ConvertTo(
            LegacyApi.Invoke(legacyChart.GetType(), "LoadFromJson", null, legacyJson)!,
            typeof(Ph.Chart)
        );
        Assert.Equal(
            Ph.NoteType.Hold,
            loaded.JudgeLineList[0].NotesAbove[0].Type
        );
    }

    [Fact]
    public void PhiChainJsonCodecs_RoundTripHoldNotesAndBeatArrays()
    {
        var formatChart = new Pc.Chart
        {
            Lines =
            [
                new Pc.SerializedLine
                {
                    Notes =
                    [
                        new Pc.Note
                        {
                            Type = Pc.NoteType.Hold,
                            Beat = new Beat([1, 0, 1]),
                            HoldBeat = new Beat([2, 0, 1]),
                        },
                    ],
                },
            ],
        };

        var formatJson = PcSerialization.ExportToJson(formatChart, false);
        var formatNote = JObject.Parse(formatJson)["lines"]![0]!["notes"]![0]!;
        Assert.Equal("hold", formatNote["kind"]);
        Assert.Equal(new[] { 2, 0, 1 }, formatNote["hold_beat"]!.ToObject<int[]>());
        var loadedFormatNote = PcSerialization.LoadFromJson(formatJson).Lines[0].Notes[0];
        Assert.Equal(Pc.NoteType.Hold, loadedFormatNote.Type);
        Assert.Equal(new Beat([2, 0, 1]), loadedFormatNote.HoldBeat);

        var legacyChart = LegacyApi.ConvertTo(new Pc.Chart
        {
            Lines =
            [
                new Pc.SerializedLine
                {
                    Notes =
                    [
                        new Pc.Note
                        {
                            Type = Pc.NoteType.Hold,
                            Beat = new Beat([1, 0, 1]),
                            HoldBeat = new Beat([2, 0, 1]),
                        },
                    ],
                },
            ],
        }, LegacyApi.CoreType("KaedePhi.Core.PhiChain.v6.Chart"));

        var legacyJson = (string)LegacyApi.Invoke(legacyChart.GetType(), "ExportToJson", legacyChart, false)!;
        var loaded = (Pc.Chart)LegacyApi.ConvertTo(
            LegacyApi.Invoke(legacyChart.GetType(), "LoadFromJson", null, legacyJson)!,
            typeof(Pc.Chart)
        );
        var loadedLegacyNote = loaded.Lines[0].Notes[0];
        Assert.Equal(Pc.NoteType.Hold, loadedLegacyNote.Type);
        Assert.Equal(new Beat([2, 0, 1]), loadedLegacyNote.HoldBeat);
    }
}
