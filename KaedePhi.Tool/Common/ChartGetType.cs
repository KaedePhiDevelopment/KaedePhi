using System.Globalization;
using System.Text;
using JetBrains.Annotations;
using KaedePhi.Tool.Compatibility;
using KaedePhi.Tool.Converter;
using KaedePhi.Tool.Converter.StellateRePhiEditExtended.Utils;
using Newtonsoft.Json;

namespace KaedePhi.Tool.Common;

/// <summary>
/// 表示谱面获取类型数据。
/// </summary>
public static class ChartGetType
{
    private const string UnsupportedChartMessage =
        "无法推断谱面类型，可能是因为谱面文本格式不正确或者不受支持的谱面类型。"
        + "请确保输入的谱面文本格式正确，并且是受支持的谱面类型之一。";

    /// <summary>
    /// 检测谱面文本在默认注册表中已注册的格式。
    /// </summary>
    /// <param name="chartText">待检测的谱面文本。</param>
    /// <param name="ct">取消令牌。</param>
    /// <returns>检测到的格式描述。</returns>
    public static Task<ChartFormatDescriptor> DetectFormatAsync(
        string chartText, CancellationToken ct = default
    ) => DetectFormatAsync(ChartFormatRegistry.Default, chartText, ct);

    internal static Task<ChartFormatDescriptor> DetectFormatAsync(
        ChartFormatCatalog catalog, string chartText, CancellationToken ct
    )
    {
        ArgumentNullException.ThrowIfNull(chartText);
        ct.ThrowIfCancellationRequested();
        var bytes = Encoding.UTF8.GetBytes(chartText);
        return DetectFormatAsync(catalog, () => new MemoryStream(bytes, writable: false), ct);
    }

    /// <summary>
    /// 从可定位输入流的当前位置检测默认注册表中的格式，并在结束时恢复位置。
    /// </summary>
    /// <param name="stream">可读取、可定位的谱面输入流。</param>
    /// <param name="ct">取消令牌。</param>
    /// <returns>检测到的格式描述。</returns>
    public static Task<ChartFormatDescriptor> DetectFormatAsync(
        Stream stream, CancellationToken ct = default
    ) => DetectFormatAsync(ChartFormatRegistry.Default, stream, ct);

    internal static async Task<ChartFormatDescriptor> DetectFormatAsync(
        ChartFormatCatalog catalog, Stream stream, CancellationToken ct
    )
    {
        ArgumentNullException.ThrowIfNull(stream);
        if (!stream.CanRead || !stream.CanSeek)
            throw new ArgumentException("检测需要可读取、可定位的流；其他输入可使用流工厂重载。", nameof(stream));
        ct.ThrowIfCancellationRequested();
        var origin = stream.Position;
        try
        {
            return await DetectFormatAsync(
                catalog,
                () =>
                {
                    stream.Position = origin;
                    return new ChartProbeStream(stream, origin);
                },
                ct
            );
        }
        finally
        {
            stream.Position = origin;
        }
    }

    /// <summary>
    /// 使用流工厂检测默认注册表中的格式，支持不可定位的输入。
    /// </summary>
    /// <param name="openRead">每次调用均返回从谱面起点开始的独立可读流；检测完成后关闭该流。</param>
    /// <param name="ct">取消令牌。</param>
    /// <returns>检测到的格式描述。</returns>
    public static Task<ChartFormatDescriptor> DetectFormatAsync(
        Func<Stream> openRead, CancellationToken ct = default
    ) => DetectFormatAsync(ChartFormatRegistry.Default, openRead, ct);

    internal static async Task<ChartFormatDescriptor> DetectFormatAsync(
        ChartFormatCatalog catalog, Func<Stream> openRead, CancellationToken ct
    )
    {
        ArgumentNullException.ThrowIfNull(openRead);
        ct.ThrowIfCancellationRequested();
        var registrations = catalog.Registrations.Where(registration => registration.Detector is not null).ToArray();
        var priorities = registrations.Select(registration => registration.Priority)
            .Distinct()
            .OrderByDescending(priority => priority);
        var detectedIds = new Dictionary<Func<Stream, CancellationToken, ValueTask<string?>>, string?>();

        foreach (var priority in priorities)
        {
            var matches = new List<ChartFormatDescriptor>();
            foreach (var registration in registrations.Where(item => item.Priority == priority))
            {
                ct.ThrowIfCancellationRequested();
                var detector = registration.Detector!;
                if (!detectedIds.TryGetValue(detector, out var detectedId))
                {
                    await using var stream = OpenDetectionStream(openRead);
                    detectedId = await detector(stream, ct);
                    detectedIds.Add(detector, detectedId);
                }
                if (string.Equals(detectedId, registration.Descriptor.FormatId, StringComparison.OrdinalIgnoreCase))
                    matches.Add(registration.Descriptor);
                ct.ThrowIfCancellationRequested();
            }

            ct.ThrowIfCancellationRequested();
            if (matches.Count > 1)
                throw new InvalidOperationException(
                    $"谱面同时匹配多个同优先级格式：{string.Join("、", matches.Select(match => match.FormatId))}。"
                );
            if (matches.Count == 1)
                return matches[0];
        }

        throw new NotSupportedException(UnsupportedChartMessage);
    }

    internal static Stream OpenDetectionStream(Func<Stream> openRead)
    {
        var stream = openRead() ?? throw new InvalidOperationException("流工厂返回了空流。");
        if (stream.CanRead)
            return stream;
        stream.Dispose();
        throw new InvalidOperationException("流工厂返回的流不可读取。");
    }

    internal static async ValueTask<string?> DetectBuiltInFormatAsync(Stream stream, CancellationToken ct)
    {
        using var textReader = new StreamReader(stream, Encoding.UTF8, true, 4096, leaveOpen: true);
        using var jsonReader = CreateJsonReader(textReader);
        try
        {
            return await ReadBuiltInFormatAsync(jsonReader, ct);
        }
        catch (UnrecognizedChartFormatException)
        {
            return null;
        }
        catch (JsonException)
        {
            return null;
        }
    }

    /// <summary>
    /// 使用谱面文本推算内置谱面类型；外部格式请使用 <see cref="DetectFormatAsync(string, CancellationToken)"/>。
    /// </summary>
    /// <param name="chartText">待检测的谱面文本。</param>
    /// <returns>内置谱面类型。</returns>
    /// <exception cref="NotSupportedException">输入了不支持的谱面类别</exception>
    [PublicAPI]
    [Pure]
    [Obsolete("将在下一个大版本移除；请改用 DetectFormatAsync，并使用返回的格式描述。")]
    public static ChartType GetType(string chartText)
    {
        ArgumentNullException.ThrowIfNull(chartText);

        using var textReader = new StringReader(chartText);
        return (ChartType)LegacyChartTypeMapping.ToValue(DetectBuiltInFormat(textReader));
    }

    /// <summary>
    /// 从文本读取器中推算内置谱面类型，不缓存完整谱面结构。
    /// </summary>
    /// <param name="reader">待检测的文本读取器。</param>
    /// <returns>检测到的谱面类型。</returns>
    /// <exception cref="NotSupportedException">输入了不支持的谱面类别。</exception>
    [PublicAPI]
    [Obsolete("将在下一个大版本移除；请改用 DetectFormatAsync，并使用返回的格式描述。")]
    public static ChartType GetType(TextReader reader) =>
        (ChartType)LegacyChartTypeMapping.ToValue(DetectBuiltInFormat(reader));

    internal static string DetectBuiltInFormat(TextReader reader)
    {
        ArgumentNullException.ThrowIfNull(reader);
        using var jsonReader = CreateJsonReader(reader);
        try
        {
            return ReadBuiltInFormat(jsonReader);
        }
        catch (UnrecognizedChartFormatException e)
        {
            throw new NotSupportedException(e.Message);
        }
        catch (JsonException e)
        {
            throw new NotSupportedException(e.Message, e);
        }
    }

    /// <summary>
    /// 从输入流中推算内置谱面类型，匹配到足够的类型标记后停止读取，不缓存完整谱面文本。
    /// </summary>
    /// <param name="stream">待检测的谱面输入流。</param>
    /// <returns>检测到的谱面类型。</returns>
    /// <exception cref="NotSupportedException">输入了不支持的谱面类别。</exception>
    [PublicAPI]
    [Obsolete("将在下一个大版本移除；请改用 DetectFormatAsync，并使用返回的格式描述。")]
    public static ChartType GetType(Stream stream)
    {
        ArgumentNullException.ThrowIfNull(stream);
        using var textReader = new StreamReader(
            stream,
            Encoding.UTF8,
            detectEncodingFromByteOrderMarks: true,
            bufferSize: 4096,
            leaveOpen: true
        );
        return (ChartType)LegacyChartTypeMapping.ToValue(DetectBuiltInFormat(textReader));
    }

    /// <summary>
    /// 异步从输入流中推算内置谱面类型，匹配到足够的类型标记后停止读取，不缓存完整谱面文本。
    /// </summary>
    /// <param name="stream">待检测的谱面输入流。</param>
    /// <param name="ct">用于响应取消请求的令牌。</param>
    /// <returns>检测到的谱面类型。</returns>
    /// <exception cref="NotSupportedException">输入了不支持的谱面类别。</exception>
    [PublicAPI]
    [Obsolete("将在下一个大版本移除；请改用 DetectFormatAsync，并使用返回的格式描述。")]
    public static async Task<ChartType> GetTypeAsync(Stream stream, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(stream);
        using var textReader = new StreamReader(
            stream,
            Encoding.UTF8,
            detectEncodingFromByteOrderMarks: true,
            bufferSize: 4096,
            leaveOpen: true
        );
        using var jsonReader = CreateJsonReader(textReader);
        try
        {
            return (ChartType)LegacyChartTypeMapping.ToValue(await ReadBuiltInFormatAsync(jsonReader, ct));
        }
        catch (UnrecognizedChartFormatException e)
        {
            throw new NotSupportedException(e.Message);
        }
        catch (JsonException e)
        {
            throw new NotSupportedException(e.Message, e);
        }
    }

    private static JsonTextReader CreateJsonReader(TextReader textReader) =>
        new(textReader) { DateParseHandling = DateParseHandling.None, CloseInput = false };

    private static string ReadBuiltInFormat(JsonTextReader reader)
    {
        if (!ReadNext(reader))
            throw new UnrecognizedChartFormatException(UnsupportedChartMessage);

        if (reader.TokenType == JsonToken.Integer)
            return ChartFormatIds.PhiEdit;
        if (reader.TokenType != JsonToken.StartObject)
            throw new UnrecognizedChartFormatException(UnsupportedChartMessage);

        var hasInfoObject = false;
        var hasLinesArray = false;
        var hasPhiChainFormat = false;
        var hasBpmList = false;
        var hasFormatVersion = false;
        var formatVersionValid = false;
        var formatVersion = 0;
        var hasRePhiEditMeta = false;
        var hasStellateTexture = false;
        var hasScannedJudgeLineList = false;

        while (ReadNext(reader))
        {
            if (reader.TokenType == JsonToken.EndObject)
                break;
            if (reader.TokenType != JsonToken.PropertyName)
                throw new JsonException("JSON 根对象包含非法属性令牌。");

            var propertyName = Convert.ToString(reader.Value, CultureInfo.InvariantCulture);
            if (!ReadNext(reader))
                throw new JsonException("JSON 属性缺少值。");

            switch (propertyName)
            {
                case "META" when reader.TokenType == JsonToken.StartObject:
                    hasRePhiEditMeta = true;
                    break;
                case "formatVersion":
                    hasFormatVersion = true;
                    formatVersionValid = TryReadFormatVersion(reader, out formatVersion);
                    break;
                case "info":
                    hasInfoObject = reader.TokenType == JsonToken.StartObject;
                    break;
                case "lines":
                    hasLinesArray = reader.TokenType == JsonToken.StartArray;
                    break;
                case "format":
                    hasPhiChainFormat = IsIntegerValue(reader, 6UL);
                    break;
                case "bpm_list":
                    hasBpmList = reader.TokenType == JsonToken.StartArray;
                    break;
            }

            if (!hasRePhiEditMeta)
            {
                if (hasFormatVersion && formatVersionValid)
                    return GetFormatIdFromVersion(formatVersion);
                if (hasInfoObject && hasLinesArray)
                    return ChartFormatIds.PhiFans;
                if (hasPhiChainFormat && hasBpmList)
                    return ChartFormatIds.PhiChain;
            }

            hasStellateTexture |= SkipValue(
                reader,
                string.Equals(propertyName, "judgeLineList", StringComparison.Ordinal),
                hasRePhiEditMeta
            );
            hasScannedJudgeLineList |= string.Equals(
                propertyName,
                "judgeLineList",
                StringComparison.Ordinal
            );

            if (hasRePhiEditMeta)
            {
                if (hasStellateTexture)
                    return ChartFormatIds.StellateRePhiEditExtended;
                if (hasScannedJudgeLineList)
                    return ChartFormatIds.RePhiEdit;
            }
        }

        if (hasRePhiEditMeta)
            return hasStellateTexture ? ChartFormatIds.StellateRePhiEditExtended : ChartFormatIds.RePhiEdit;

        if (hasFormatVersion)
        {
            return !formatVersionValid
                ? throw new UnrecognizedChartFormatException(UnsupportedChartMessage)
                : GetFormatIdFromVersion(formatVersion);
        }

        if (hasInfoObject && hasLinesArray)
            return ChartFormatIds.PhiFans;
        if (hasPhiChainFormat && hasBpmList)
            return ChartFormatIds.PhiChain;

        throw new UnrecognizedChartFormatException(UnsupportedChartMessage);
    }

    private static async Task<string> ReadBuiltInFormatAsync(JsonTextReader reader, CancellationToken ct)
    {
        if (!await ReadNextAsync(reader, ct))
            throw new UnrecognizedChartFormatException(UnsupportedChartMessage);

        if (reader.TokenType == JsonToken.Integer)
            return ChartFormatIds.PhiEdit;
        if (reader.TokenType != JsonToken.StartObject)
            throw new UnrecognizedChartFormatException(UnsupportedChartMessage);

        var hasInfoObject = false;
        var hasLinesArray = false;
        var hasPhiChainFormat = false;
        var hasBpmList = false;
        var hasFormatVersion = false;
        var formatVersionValid = false;
        var formatVersion = 0;
        var hasRePhiEditMeta = false;
        var hasStellateTexture = false;
        var hasScannedJudgeLineList = false;

        while (await ReadNextAsync(reader, ct))
        {
            if (reader.TokenType == JsonToken.EndObject)
                break;
            if (reader.TokenType != JsonToken.PropertyName)
                throw new JsonException("JSON 根对象包含非法属性令牌。");

            var propertyName = Convert.ToString(reader.Value, CultureInfo.InvariantCulture);
            if (!await ReadNextAsync(reader, ct))
                throw new JsonException("JSON 属性缺少值。");

            switch (propertyName)
            {
                case "META" when reader.TokenType == JsonToken.StartObject:
                    hasRePhiEditMeta = true;
                    break;
                case "formatVersion":
                    hasFormatVersion = true;
                    formatVersionValid = TryReadFormatVersion(reader, out formatVersion);
                    break;
                case "info":
                    hasInfoObject = reader.TokenType == JsonToken.StartObject;
                    break;
                case "lines":
                    hasLinesArray = reader.TokenType == JsonToken.StartArray;
                    break;
                case "format":
                    hasPhiChainFormat = IsIntegerValue(reader, 6UL);
                    break;
                case "bpm_list":
                    hasBpmList = reader.TokenType == JsonToken.StartArray;
                    break;
            }

            if (!hasRePhiEditMeta)
            {
                if (hasFormatVersion && formatVersionValid)
                    return GetFormatIdFromVersion(formatVersion);
                if (hasInfoObject && hasLinesArray)
                    return ChartFormatIds.PhiFans;
                if (hasPhiChainFormat && hasBpmList)
                    return ChartFormatIds.PhiChain;
            }

            hasStellateTexture |= await SkipValueAsync(
                reader,
                string.Equals(propertyName, "judgeLineList", StringComparison.Ordinal),
                hasRePhiEditMeta,
                ct
            );
            hasScannedJudgeLineList |= string.Equals(
                propertyName,
                "judgeLineList",
                StringComparison.Ordinal
            );

            if (hasRePhiEditMeta)
            {
                if (hasStellateTexture)
                    return ChartFormatIds.StellateRePhiEditExtended;
                if (hasScannedJudgeLineList)
                    return ChartFormatIds.RePhiEdit;
            }
        }

        if (hasRePhiEditMeta)
            return hasStellateTexture ? ChartFormatIds.StellateRePhiEditExtended : ChartFormatIds.RePhiEdit;

        if (hasFormatVersion)
        {
            if (!formatVersionValid)
                throw new UnrecognizedChartFormatException(UnsupportedChartMessage);
            return GetFormatIdFromVersion(formatVersion);
        }

        if (hasInfoObject && hasLinesArray)
            return ChartFormatIds.PhiFans;
        if (hasPhiChainFormat && hasBpmList)
            return ChartFormatIds.PhiChain;

        throw new UnrecognizedChartFormatException(UnsupportedChartMessage);
    }

    private static bool ReadNext(JsonTextReader reader)
    {
        while (reader.Read())
        {
            if (reader.TokenType != JsonToken.Comment)
                return true;
        }

        return false;
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

    private static bool SkipValue(
        JsonTextReader reader,
        bool detectStellateTexture = false,
        bool stopOnStellateTexture = false
    )
    {
        if (reader.TokenType is not (JsonToken.StartObject or JsonToken.StartArray))
            return false;

        var depth = 1;
        var hasStellateTexture = false;
        while (depth > 0)
        {
            if (!reader.Read())
                throw new JsonException("JSON 值未正常结束。");

            if (
                detectStellateTexture
                && reader.TokenType == JsonToken.PropertyName
                && string.Equals(reader.Value as string, "Texture", StringComparison.Ordinal)
            )
            {
                if (!reader.Read())
                    throw new JsonException("JSON 属性缺少值。");
                if (
                    reader.TokenType == JsonToken.String
                    && StellateRePhiEditExtendedTexture.IsBlockAreaTexture(reader.Value as string)
                )
                {
                    hasStellateTexture = true;
                    if (stopOnStellateTexture)
                        return true;
                }
                if (reader.TokenType is JsonToken.StartObject or JsonToken.StartArray)
                    depth++;
                continue;
            }

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

        return hasStellateTexture;
    }

    private static async Task<bool> SkipValueAsync(
        JsonTextReader reader,
        bool detectStellateTexture,
        bool stopOnStellateTexture,
        CancellationToken ct
    )
    {
        if (reader.TokenType is not (JsonToken.StartObject or JsonToken.StartArray))
            return false;

        var depth = 1;
        var hasStellateTexture = false;
        while (depth > 0)
        {
            if (!await reader.ReadAsync(ct))
                throw new JsonException("JSON 值未正常结束。");

            if (
                detectStellateTexture
                && reader.TokenType == JsonToken.PropertyName
                && string.Equals(reader.Value as string, "Texture", StringComparison.Ordinal)
            )
            {
                if (!await reader.ReadAsync(ct))
                    throw new JsonException("JSON 属性缺少值。");
                if (
                    reader.TokenType == JsonToken.String
                    && StellateRePhiEditExtendedTexture.IsBlockAreaTexture(reader.Value as string)
                )
                {
                    hasStellateTexture = true;
                    if (stopOnStellateTexture)
                        return true;
                }
                if (reader.TokenType is JsonToken.StartObject or JsonToken.StartArray)
                    depth++;
                continue;
            }

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

        return hasStellateTexture;
    }

    private static bool TryReadFormatVersion(JsonTextReader reader, out int formatVersion)
    {
        formatVersion = 0;
        if (reader.TokenType != JsonToken.Integer)
            return false;

        var text = Convert.ToString(reader.Value, CultureInfo.InvariantCulture);
        return long.TryParse(
                text,
                NumberStyles.Integer,
                CultureInfo.InvariantCulture,
                out var parsed
            )
            && parsed is >= int.MinValue and <= int.MaxValue
            && (formatVersion = (int)parsed) == parsed;
    }

    private static bool IsIntegerValue(JsonTextReader reader, ulong expected)
    {
        if (reader.TokenType != JsonToken.Integer)
            return false;

        return ulong.TryParse(
                Convert.ToString(reader.Value, CultureInfo.InvariantCulture),
                NumberStyles.Integer,
                CultureInfo.InvariantCulture,
                out var value
            )
            && value == expected;
    }

    private static string GetFormatIdFromVersion(int formatVersion) =>
        formatVersion switch
        {
            1 => ChartFormatIds.PhigrosV1,
            3 => ChartFormatIds.PhigrosV3,
            _ => throw new UnrecognizedChartFormatException(UnsupportedChartMessage),
        };
}
