using System.Text;
using KaedePhi.Tool.Common;
using KaedePhi.Tool.Converter;

namespace KaedePhi.Tests.Common;

public class ChartGetTypeTests
{
    [Theory]
    [InlineData("{\"formatVersion\":1}", ChartFormatIds.PhigrosV1)]
    [InlineData("{\"formatVersion\":3}", ChartFormatIds.PhigrosV3)]
    public async Task DetectFormat_WithIntegerFormatVersion_ReturnsPhigrosFormat(
        string chartText,
        string expected
    )
    {
        var format = await new ChartFormatCatalog().DetectFormatAsync(chartText, TestContext.Current.CancellationToken);
        format.FormatId.Should().Be(expected);
    }

    [Theory]
    [InlineData("{\"formatVersion\":3.0}")]
    [InlineData("{\"formatVersion\":\"3\"}")]
    [InlineData("{\"formatVersion\":null}")]
    [InlineData("{\"formatVersion\":2}")]
    public async Task DetectFormat_WithInvalidFormatVersion_ThrowsNotSupportedException(string chartText)
    {
        var act = () => new ChartFormatCatalog().DetectFormatAsync(chartText, TestContext.Current.CancellationToken);

        await act.Should().ThrowAsync<NotSupportedException>();
    }

    [Theory]
    [InlineData(
        "{\"META\":{},\"judgeLineList\":[{\"Texture\":\"Pictures\\\\isSubtract0.png\"}]}",
        ChartFormatIds.StellateRePhiEditExtended
    )]
    [InlineData(
        "{\"judgeLineList\":[{\"Texture\":\"isSubtract1.png\"}],\"META\":{}}",
        ChartFormatIds.StellateRePhiEditExtended
    )]
    [InlineData(
        "{\"META\":{},\"judgeLineList\":[{\"Texture\":\"line.png\"}]}",
        ChartFormatIds.RePhiEdit
    )]
    public async Task DetectFormat_WithRePhiEditTextureMarker_ReturnsExpectedFormat(
        string chartText,
        string expected
    )
    {
        var format = await new ChartFormatCatalog().DetectFormatAsync(chartText, TestContext.Current.CancellationToken);
        format.FormatId.Should().Be(expected);
    }

    [Fact]
    public async Task DetectFormat_WithRePhiEditTextureMarker_ReturnsExtendedFormat()
    {
        const string chartText =
            "{\"META\":{},\"judgeLineList\":[{\"Texture\":\"Pictures\\\\isSubtract1.png\"}]}";
        await using var stream = new MemoryStream(Encoding.UTF8.GetBytes(chartText));

        var detected = await new ChartFormatCatalog().DetectFormatAsync(
            stream,
            TestContext.Current.CancellationToken
        );

        detected.FormatId.Should().Be(ChartFormatIds.StellateRePhiEditExtended);
    }

    [Fact]
    public async Task DetectFormat_WithPhiFansSignature_StopsBeforeReadingChartBody()
    {
        var largePayload = new string('x', 100_000);
        var chartText = $"{{\"info\":{{}},\"lines\":[\"{largePayload}\"]}}";
        await using var stream = new ReadTrackingStream(Encoding.UTF8.GetBytes(chartText));

        var format = await new ChartFormatCatalog().DetectFormatAsync(stream, TestContext.Current.CancellationToken);
        format.FormatId.Should().Be(ChartFormatIds.PhiFans);
        stream.FarthestReadPosition.Should().BeInRange(1, stream.Length - 1);
        stream.Position.Should().Be(0);
    }

    [Fact]
    public async Task DetectFormat_WithPhiChainSignature_StopsBeforeReadingChartBody()
    {
        var largePayload = new string('x', 100_000);
        var chartText = $"{{\"format\":6,\"bpm_list\":[\"{largePayload}\"]}}";
        await using var stream = new ReadTrackingStream(Encoding.UTF8.GetBytes(chartText));

        var format = await new ChartFormatCatalog().DetectFormatAsync(stream, TestContext.Current.CancellationToken);
        format.FormatId.Should().Be(ChartFormatIds.PhiChain);
        stream.FarthestReadPosition.Should().BeInRange(1, stream.Length - 1);
        stream.Position.Should().Be(0);
    }

    [Fact]
    public async Task DetectFormat_WithRePhiEditTextureMarker_StopsAfterFindingMarker()
    {
        var largePayload = new string('x', 100_000);
        var chartText =
            "{\"META\":{},\"judgeLineList\":[{\"Texture\":\"Pictures\\\\isSubtract0.png\",\"payload\":\""
            + largePayload
            + "\"}]}";
        await using var stream = new ReadTrackingStream(Encoding.UTF8.GetBytes(chartText));

        var format = await new ChartFormatCatalog().DetectFormatAsync(stream, TestContext.Current.CancellationToken);
        format.FormatId.Should().Be(ChartFormatIds.StellateRePhiEditExtended);
        stream.FarthestReadPosition.Should().BeInRange(1, stream.Length - 1);
        stream.Position.Should().Be(0);
    }

    [Fact]
    public async Task DetectFormat_WithFormatVersion_StopsBeforeReadingChartBody()
    {
        var largePayload = new string('x', 100_000);
        var chartText = $"{{\"formatVersion\":3,\"judgeLineList\":[\"{largePayload}\"]}}";
        await using var stream = new ReadTrackingStream(Encoding.UTF8.GetBytes(chartText));

        var detected = await new ChartFormatCatalog().DetectFormatAsync(
            stream,
            TestContext.Current.CancellationToken
        );

        detected.FormatId.Should().Be(ChartFormatIds.PhigrosV3);
        stream.FarthestReadPosition.Should().BeInRange(1, stream.Length - 1);
        stream.Position.Should().Be(0);
    }
}
