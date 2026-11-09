using KaedePhi.Core.Primitives;
using KaedePhi.Tool.Common;
using KaedePhi.Tool.Converter.Phigros.v3.Utils;
using KaedePhi.Tool.Converter.RePhiEdit;
using KaedePhi.Tool.Converter.RePhiEdit.Model;
using KaedePhi.Tool.Converter.StellateRePhiEditExtended.Utils;
using IrBlockArea = KaedePhi.Core.Intermediate.Model.BlockArea;
using IrChart = KaedePhi.Core.Intermediate.Model.Chart;
using RpeChart = KaedePhi.Core.Formats.RePhiEdit.Model.Chart;
using RpeJudgeLine = KaedePhi.Core.Formats.RePhiEdit.Model.JudgeLine;

namespace KaedePhi.Tool.Converter.StellateRePhiEditExtended;

/// <summary>
/// StellateRePhiEditExtended 与 IR 谱面之间的格式转换器。
/// </summary>
public sealed class StellateRePhiEditExtendedConverter
    : LoggableBase,
        IChartConverter<RpeChart, Unit?, ConvertOption>,
        ICancellableChartConverter
{
    private CancellationToken _ct;

    /// <summary>
    /// 设置转换过程中使用的取消令牌。
    /// </summary>
    /// <param name="ct">取消令牌</param>
    public void SetCancellationToken(CancellationToken ct) => _ct = ct;

    /// <summary>
    /// 将 StellateRePhiEditExtended 谱面转换为 IR 格式。
    /// </summary>
    /// <param name="source">源谱面</param>
    /// <returns>转换后的 IR 谱面</returns>
    public IrChart ToIr(RpeChart source)
    {
        ArgumentNullException.ThrowIfNull(source);
        _ct.ThrowIfCancellationRequested();

        var sourceLines = source.JudgeLineList;
        var blockAreaLines = sourceLines
            .Select((line, index) => (Line: line, Index: index))
            .Where(item => StellateRePhiEditExtendedTexture.IsBlockAreaTexture(item.Line.Texture))
            .ToList();
        var isBlockAreaLine = new bool[sourceLines.Count];
        foreach (var item in blockAreaLines)
            isBlockAreaLine[item.Index] = true;

        var converted = ConvertRegularLines(source, isBlockAreaLine);
        var blockAreas = ConvertBlockAreaLines(source, blockAreaLines, CreateBeatMapper(converted));
        var retainedLines = RetainJudgeLines(sourceLines, isBlockAreaLine, converted.JudgeLineList);

        return IrChartNormalizer.NormalizeAndValidateNoteEndBeats(
            new IrChart
            {
                BpmList = converted.BpmList,
                Meta = converted.Meta,
                JudgeLineList = retainedLines,
                BlockAreaList = [.. converted.BlockAreaList, .. blockAreas],
            }
        );
    }

    /// <summary>
    /// 使用通用转换器接口将 StellateRePhiEditExtended 谱面转换为 IR 格式。
    /// </summary>
    /// <param name="input">源谱面</param>
    /// <param name="options">未使用的输入选项</param>
    /// <returns>转换后的 IR 谱面</returns>
    public IrChart ToIr(RpeChart input, Unit? options) => ToIr(input);

    /// <summary>
    /// 将 IR 谱面转换为 StellateRePhiEditExtended 格式。
    /// </summary>
    /// <param name="input">IR 谱面</param>
    /// <param name="options">输出转换选项</param>
    /// <returns>转换后的 StellateRePhiEditExtended 谱面</returns>
    public RpeChart FromIr(IrChart input, ConvertOption options)
    {
        ArgumentNullException.ThrowIfNull(input);
        ArgumentNullException.ThrowIfNull(options);
        _ct.ThrowIfCancellationRequested();

        var normalized = IrChartNormalizer.NormalizeAndValidateNoteEndBeats(input);
        var converted = CreateBaseConverter().FromIr(normalized, options);

        for (var index = 0; index < normalized.JudgeLineList.Count; index++)
        {
            _ct.ThrowIfCancellationRequested();
            if (
                StellateRePhiEditExtendedTexture.IsBlockAreaTexture(
                    normalized.JudgeLineList[index].Texture
                )
            )
                LogWarning(
                    string.Format(ToolResources.stellate_warning_marker_texture_reimport, index)
                );
        }

        for (var index = 0; index < normalized.BlockAreaList.Count; index++)
        {
            _ct.ThrowIfCancellationRequested();
            var blockArea =
                normalized.BlockAreaList[index]
                ?? throw StellateExceptions.BlockAreaListContainsNull();
            converted.JudgeLineList.Add(
                StellateRePhiEditExtendedBlockAreaBuilder.ConvertBlockArea(
                    blockArea,
                    index,
                    options.Cutting,
                    LogWarning,
                    _ct
                )
            );
        }

        return converted;
    }

    private RePhiEditConverter CreateBaseConverter()
    {
        var converter = new RePhiEditConverter
        {
            OnInfo = OnInfo,
            OnWarning = OnWarning,
            OnError = OnError,
            OnDebug = OnDebug,
        };
        converter.SetCancellationToken(_ct);
        return converter;
    }

    // 噪域标记判定线以空判定线占位，普通判定线交给 RePhiEdit 转换器处理
    private IrChart ConvertRegularLines(RpeChart source, bool[] isBlockAreaLine)
    {
        var regularSource = new RpeChart
        {
            BpmList = source.BpmList,
            Meta = source.Meta,
            JudgeLineList =
            [
                .. source.JudgeLineList.Select(
                    (line, index) =>
                        isBlockAreaLine[index] ? new RpeJudgeLine { Father = line.Father } : line
                ),
            ],
        };
        return CreateBaseConverter().ToIr(regularSource, null);
    }

    private List<IrBlockArea> ConvertBlockAreaLines(
        RpeChart source,
        List<(RpeJudgeLine Line, int Index)> blockAreaLines,
        PhigrosV3TimeMapper beatMapper
    )
    {
        var geometryConverter = new StellateRePhiEditGeometry(source, beatMapper, _ct);
        foreach (var (line, index) in blockAreaLines)
        {
            _ct.ThrowIfCancellationRequested();
            geometryConverter.PrepareLine(index);
            if (line.Notes is { Count: > 0 })
                LogWarning(string.Format(ToolResources.stellate_warning_notes_ignored, index));
        }

        if (blockAreaLines.Count <= 1)
            return
            [
                .. blockAreaLines.SelectMany(item => geometryConverter.ConvertLine(item.Index)),
            ];

        var convertedBlockAreas = new List<IrBlockArea>[blockAreaLines.Count];
        var sampleParallelism = Math.Max(1, Environment.ProcessorCount / blockAreaLines.Count);
        StellateParallel.Run(() =>
            Parallel.For(
                0,
                blockAreaLines.Count,
                new ParallelOptions
                {
                    CancellationToken = _ct,
                    MaxDegreeOfParallelism = Environment.ProcessorCount,
                },
                index =>
                {
                    convertedBlockAreas[index] = geometryConverter.ConvertLine(
                        blockAreaLines[index].Index,
                        sampleParallelism
                    );
                }
            )
        );

        return [.. convertedBlockAreas.SelectMany(blockAreas => blockAreas)];
    }

    // 移除噪域标记判定线，并把保留判定线的父级重新映射到新索引
    private List<Ir.JudgeLine> RetainJudgeLines(
        List<RpeJudgeLine> sourceLines,
        bool[] isBlockAreaLine,
        List<Ir.JudgeLine> convertedLines
    )
    {
        var oldToNewIndex = new int[sourceLines.Count];
        Array.Fill(oldToNewIndex, -1);
        var retainedLines = new List<Ir.JudgeLine>();
        var retainedSourceIndices = new List<int>();
        for (var index = 0; index < convertedLines.Count; index++)
        {
            _ct.ThrowIfCancellationRequested();
            if (isBlockAreaLine[index])
                continue;
            oldToNewIndex[index] = retainedLines.Count;
            retainedSourceIndices.Add(index);
            retainedLines.Add(convertedLines[index]);
        }

        for (var index = 0; index < retainedLines.Count; index++)
        {
            _ct.ThrowIfCancellationRequested();
            var sourceIndex = retainedSourceIndices[index];
            var (father, skippedBlockAreaParent) = MapFather(
                sourceLines,
                isBlockAreaLine,
                oldToNewIndex,
                sourceIndex
            );
            retainedLines[index].Father = father;
            if (skippedBlockAreaParent)
                LogWarning(
                    string.Format(ToolResources.stellate_warning_parent_converted, sourceIndex)
                );
        }

        return retainedLines;
    }

    private static PhigrosV3TimeMapper CreateBeatMapper(IrChart chart)
    {
        var bpmList =
            chart.BpmList.Count > 0
                ? chart.BpmList
                : [new Ir.BpmItem { Bpm = 120f, StartBeat = new Beat(0) }];
        return new PhigrosV3TimeMapper(bpmList);
    }

    private static (int Father, bool SkippedBlockAreaParent) MapFather(
        List<RpeJudgeLine> sourceLines,
        bool[] isBlockAreaLine,
        int[] oldToNewIndex,
        int sourceIndex
    )
    {
        var father = GetFather(sourceIndex);
        var skippedBlockAreaParent = false;
        var visited = new HashSet<int>();
        while (father >= 0 && isBlockAreaLine[father])
        {
            if (!visited.Add(father))
                throw StellateExceptions.FatherCycle();
            skippedBlockAreaParent = true;
            father = GetFather(father);
        }

        if (father < 0)
            return (-1, skippedBlockAreaParent);
        if (oldToNewIndex[father] < 0)
            throw StellateExceptions.FatherUnmapped(sourceIndex);
        return (oldToNewIndex[father], skippedBlockAreaParent);

        // 父线索引无效时始终报告起始判定线，便于定位问题
        int GetFather(int lineIndex)
        {
            var value = sourceLines[lineIndex].Father;
            return value < -1 || value >= sourceLines.Count
                ? throw StellateExceptions.InvalidFatherIndex(sourceIndex)
                : value;
        }
    }
}
