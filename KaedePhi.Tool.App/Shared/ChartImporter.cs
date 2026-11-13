using System.Text;
using KaedePhi.Core.Primitives;
using KaedePhi.Tool.Common;
using KaedePhi.Tool.Converter;
using KaedePhi.Tool.Converter.RePhiEdit;
using KaedePhi.Tool.Converter.StellateRePhiEditExtended;
using KaedePhi.Tool.Converter.StellateRePhiEditExtended.Utils;
using Newtonsoft.Json;
using Chart = KaedePhi.Core.Intermediate.Model.Chart;
using RpeChart = KaedePhi.Core.Formats.RePhiEdit.Model.Chart;
using RpeSerialization = KaedePhi.Core.Formats.RePhiEdit.Serialization.ChartSerialization;

namespace KaedePhi.Tool.App.Shared;

// CLI 与 GUI 共用的谱面格式检测与导入流程
internal static class ChartImporter
{
    // 检测格式后会将进度复位为不确定状态
    public static async Task<ChartFormatDescriptor> DetectFormatAsync(
        string filePath,
        IProgress<double?>? progress,
        CancellationToken ct
    )
    {
        await using var stream = OpenRead(filePath, progress);
        var format = await DetectImportFormatAsync(stream, ct);
        progress?.Report(null);
        return format;
    }

    // preferStream 为 true 时，格式支持流式导入则优先使用；格式不支持文本导入时总是使用流式导入
    public static async Task<Chart> ImportAsync(
        string filePath,
        ChartFormatDescriptor format,
        bool preferStream,
        object? importOptions,
        ChartLogSink? log,
        IProgress<double?>? progress,
        CancellationToken ct
    ) => (await ImportWithFormatAsync(
        filePath,
        format,
        preferStream,
        importOptions,
        log,
        progress,
        ct
    )).Chart;

    public static async Task<(Chart Chart, string FormatId)> ImportWithFormatAsync(
        string filePath,
        ChartFormatDescriptor format,
        bool preferStream,
        object? importOptions,
        ChartLogSink? log,
        IProgress<double?>? progress,
        CancellationToken ct
    )
    {
        await using var stream = OpenRead(filePath, progress);
        // 根字段用于快速识别 RePhiEdit 候选。
        // 扩展格式标记在完整导入时确认，避免格式检测遍历整份谱面。
        if (
            format.FormatId == ChartFormatIds.RePhiEdit
            && ReferenceEquals(format, ChartFormatRegistry.Find(ChartFormatIds.RePhiEdit))
        )
        {
            if (format.CanStreamImport && (preferStream || !format.CanTextImport))
            {
                var source = await RpeSerialization.LoadFromStreamAsync(stream);
                ct.ThrowIfCancellationRequested();
                return ConvertRePhiEdit(source, log, ct);
            }

            using var rePhiReader = new StreamReader(
                stream,
                Encoding.UTF8,
                detectEncodingFromByteOrderMarks: true,
                bufferSize: 4096,
                leaveOpen: true
            );
            var rePhiText = await rePhiReader.ReadToEndAsync(ct);
            progress?.Report(null);
            return ConvertRePhiEdit(await RpeSerialization.LoadFromJsonAsync(rePhiText), log, ct);
        }

        if (format.CanStreamImport && (preferStream || !format.CanTextImport))
            return (await format.ImportStreamIrAsync(stream, importOptions, log, ct), format.FormatId);

        using var reader = new StreamReader(
            stream,
            Encoding.UTF8,
            detectEncodingFromByteOrderMarks: true,
            bufferSize: 4096,
            leaveOpen: true
        );
        var text = await reader.ReadToEndAsync(ct);
        progress?.Report(null);
        return (await format.ImportIrAsync(text, importOptions, log, ct), format.FormatId);
    }

    private static async Task<ChartFormatDescriptor> DetectImportFormatAsync(
        Stream stream,
        CancellationToken ct
    )
    {
        var rePhiRegistration = ChartFormatRegistry.Default.FindRegistration(
            ChartFormatIds.RePhiEdit
        );
        var stellateRegistration = ChartFormatRegistry.Default.FindRegistration(
            ChartFormatIds.StellateRePhiEditExtended
        );
        var builtInDetector = rePhiRegistration?.Detector ?? stellateRegistration?.Detector;
        var hasCustomDetectorAtBuiltInPriority = ChartFormatRegistry
            .Default.Registrations.Where(registration => registration.Priority >= 0)
            .Any(registration => registration.Detector is not null
                && registration.Detector != builtInDetector);

        if (
            !hasCustomDetectorAtBuiltInPriority
            && rePhiRegistration?.Detector is not null
            && stellateRegistration?.Detector == rePhiRegistration.Detector
        )
        {
            var origin = stream.Position;
            if (await HasRePhiEditHeaderAsync(stream, ct))
                return rePhiRegistration.Descriptor;
            stream.Position = origin;
        }

        return await ChartGetType.DetectFormatAsync(stream, ct);
    }

    private static async Task<bool> HasRePhiEditHeaderAsync(Stream stream, CancellationToken ct)
    {
        var origin = stream.Position;
        try
        {
            using var textReader = new StreamReader(
                stream,
                Encoding.UTF8,
                detectEncodingFromByteOrderMarks: true,
                bufferSize: 4096,
                leaveOpen: true
            );
            using var jsonReader = new JsonTextReader(textReader)
            {
                DateParseHandling = DateParseHandling.None,
                CloseInput = false,
            };

            if (
                !await ReadNextAsync(jsonReader, ct)
                || jsonReader.TokenType != JsonToken.StartObject
            )
                return false;

            var hasMetaObject = false;
            while (await ReadNextAsync(jsonReader, ct))
            {
                if (jsonReader.TokenType == JsonToken.EndObject)
                    return false;
                if (jsonReader.TokenType != JsonToken.PropertyName)
                    return false;

                var propertyName = jsonReader.Value as string;
                if (!await ReadNextAsync(jsonReader, ct))
                    return false;

                if (propertyName == "META")
                {
                    hasMetaObject = jsonReader.TokenType == JsonToken.StartObject;
                    await SkipJsonValueAsync(jsonReader, ct);
                    continue;
                }

                if (propertyName == "judgeLineList")
                    return hasMetaObject && jsonReader.TokenType == JsonToken.StartArray;

                if (
                    propertyName is "formatVersion"
                        or "info"
                        or "lines"
                        or "format"
                        or "bpm_list"
                )
                    return false;

                await SkipJsonValueAsync(jsonReader, ct);
            }

            return false;
        }
        catch (JsonException)
        {
            return false;
        }
        finally
        {
            stream.Position = origin;
        }
    }

    private static async Task<bool> ReadNextAsync(JsonTextReader reader, CancellationToken ct)
    {
        while (await reader.ReadAsync(ct))
        {
            if (reader.TokenType != JsonToken.Comment)
                return true;
        }

        return false;
    }

    private static async Task SkipJsonValueAsync(JsonTextReader reader, CancellationToken ct)
    {
        if (reader.TokenType is not (JsonToken.StartObject or JsonToken.StartArray))
            return;

        var depth = 1;
        while (depth > 0 && await reader.ReadAsync(ct))
        {
            switch (reader.TokenType)
            {
                case JsonToken.StartObject or JsonToken.StartArray:
                    depth++;
                    break;
                case JsonToken.EndObject or JsonToken.EndArray:
                    depth--;
                    break;
            }
        }

        if (depth > 0)
            throw new JsonException("JSON 内容未正常结束。");
    }

    private static (Chart Chart, string FormatId) ConvertRePhiEdit(
        RpeChart source,
        ChartLogSink? log,
        CancellationToken ct
    )
    {
        ct.ThrowIfCancellationRequested();
        var isStellate = source.JudgeLineList.Any(line =>
            StellateRePhiEditExtendedTexture.IsBlockAreaTexture(line.Texture)
        );
        var formatId = isStellate
            ? ChartFormatIds.StellateRePhiEditExtended
            : ChartFormatIds.RePhiEdit;
        var sink = log ?? ChartLogSink.None;

        if (isStellate)
        {
            var converter = new StellateRePhiEditExtendedConverter();
            using var subscription = sink.AttachTo(converter);
            converter.SetCancellationToken(ct);
            var chart = converter.ToIr(source);
            ct.ThrowIfCancellationRequested();
            return (chart, formatId);
        }

        var rePhiConverter = new RePhiEditConverter();
        using var rePhiSubscription = sink.AttachTo(rePhiConverter);
        rePhiConverter.SetCancellationToken(ct);
        var rePhiChart = rePhiConverter.ToIr(source, (Unit?)null);
        ct.ThrowIfCancellationRequested();
        return (rePhiChart, formatId);
    }

    private static Stream OpenRead(string filePath, IProgress<double?>? progress)
    {
        var stream = ChartFiles.OpenRead(filePath);
        return progress is null ? stream : new ProgressReadStream(stream, progress);
    }
}
