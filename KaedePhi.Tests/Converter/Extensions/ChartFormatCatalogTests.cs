using System.Text;
using KaedePhi.Core.Intermediate.Model;
using KaedePhi.Tool.Common;
using KaedePhi.Tool.Converter;

namespace KaedePhi.Tests.Converter.Extensions;

public sealed class ChartFormatCatalogTests
{
    private static ChartFormatDescriptor CreateDescriptor(string id) =>
        ChartFormatDescriptor.Create(
            id, "txt", importer: (text, _, _, _) =>
                Task.FromResult(new Chart { Meta = new Meta { Name = text } })
        );

    [Fact]
    public async Task DefaultFormats_AreRegisteredAndCanBeRemovedAndRestored()
    {
        var catalog = new ChartFormatCatalog();
        foreach (var formatId in new[]
        {
            ChartFormatIds.PhiEdit, ChartFormatIds.RePhiEdit, ChartFormatIds.PhigrosV1,
            ChartFormatIds.PhigrosV3, ChartFormatIds.PhiChain, ChartFormatIds.PhiFans,
            ChartFormatIds.StellateRePhiEditExtended,
        })
        {
            var descriptor = catalog.Get(formatId);
            var registration = catalog.FindRegistration(formatId);
            registration.Should().NotBeNull();
            registration!.Descriptor.Should().BeSameAs(descriptor);
            registration.Detector.Should().NotBeNull();
            registration.Priority.Should().Be(0);
            catalog.Formats.Should().Contain(descriptor);
        }

        const string text = "{\"META\":{},\"judgeLineList\":[]}";
        var saved = catalog.FindRegistration("RePhiEdit")!;
        catalog.Unregister("rephiedit").Should().BeTrue();
        catalog.Find(ChartFormatIds.RePhiEdit).Should().BeNull();
        var detectWithoutFormat = () => catalog.DetectFormatAsync(text, TestContext.Current.CancellationToken);
        await detectWithoutFormat.Should().ThrowAsync<NotSupportedException>();
        ChartFormatRegistry.Get(ChartFormatIds.RePhiEdit).Should().BeSameAs(saved.Descriptor);

        catalog.Register(saved);
        (await catalog.DetectFormatAsync(text, TestContext.Current.CancellationToken))
            .Should().BeSameAs(saved.Descriptor);
        catalog.Get(ChartFormatIds.RePhiEdit).Should().BeSameAs(saved.Descriptor);
    }

    [Fact]
    public async Task RemovedBuiltInFormat_CanBeReplacedByExternalRegistration()
    {
        var catalog = new ChartFormatCatalog();
        var original = catalog.FindRegistration("PhigrosV3")!;
        catalog.Unregister("PhigrosV3").Should().BeTrue();
        var replacement = CreateDescriptor("PhigrosV3");
        catalog.Register(replacement, async (stream, ct) =>
        {
            using var reader = new StreamReader(stream, leaveOpen: true);
            return await reader.ReadLineAsync(ct) == "替代格式";
        });

        var (format, chart) = await catalog.ImportDetectedIrAsync(
            "替代格式\n内容", ct: TestContext.Current.CancellationToken
        );
        format.Should().BeSameAs(replacement);
        chart.Meta.Name.Should().Be("替代格式\n内容");
        catalog.Get(ChartFormatIds.PhigrosV3).Should().BeSameAs(replacement);
        var originalInput = () => catalog.DetectFormatAsync(
            "{\"formatVersion\":3}", TestContext.Current.CancellationToken
        );
        await originalInput.Should().ThrowAsync<NotSupportedException>();

        catalog.Unregister("PhigrosV3").Should().BeTrue();
        catalog.Register(original);
        (await catalog.DetectFormatAsync("{\"formatVersion\":3}", TestContext.Current.CancellationToken))
            .Should().BeSameAs(original.Descriptor);
    }

    [Fact]
    public async Task EmptyCatalog_AndDefaultCatalog_AreIndependent()
    {
        var empty = new ChartFormatCatalog(includeBuiltInFormats: false);
        empty.Formats.Should().BeEmpty();
        var missing = () => empty.DetectFormatAsync(
            "{\"formatVersion\":3}", TestContext.Current.CancellationToken
        );
        await missing.Should().ThrowAsync<NotSupportedException>();

        var registration = ChartFormatRegistry.Default.FindRegistration("PhigrosV3")!;
        empty.Register(registration);
        (await empty.DetectFormatAsync("{\"formatVersion\":3}", TestContext.Current.CancellationToken))
            .Should().BeSameAs(registration.Descriptor);
        empty.Unregister("PhigrosV3").Should().BeTrue();
        ChartFormatRegistry.Find(ChartFormatIds.PhigrosV3).Should().BeSameAs(registration.Descriptor);
    }

    [Fact]
    public async Task SharedDetector_IsExecutedOnlyOnceAcrossRegisteredFormatsAndPriorities()
    {
        var catalog = new ChartFormatCatalog(includeBuiltInFormats: false);
        var first = CreateDescriptor("tests.shared-first");
        var second = CreateDescriptor("tests.shared-second");
        var calls = 0;
        var opens = 0;
        async ValueTask<string?> Detect(Stream stream, CancellationToken ct)
        {
            calls++;
            using var reader = new StreamReader(stream, leaveOpen: true);
            return await reader.ReadLineAsync(ct);
        }
        Func<Stream, CancellationToken, ValueTask<string?>> detector = Detect;
        catalog.Register(new ChartFormatRegistration(first, detector, priority: 1));
        catalog.Register(new ChartFormatRegistration(second, detector));

        Stream Open()
        {
            opens++;
            return new MemoryStream(Encoding.UTF8.GetBytes("TESTS.SHARED-SECOND\n内容"));
        }
        (await catalog.DetectFormatAsync(Open, TestContext.Current.CancellationToken))
            .Should().BeSameAs(second);
        calls.Should().Be(1);
        opens.Should().Be(1);

        var captured = catalog.FindRegistration("tests.shared-second")!;
        catalog.Unregister(second.FormatId).Should().BeTrue();
        var unknown = () => catalog.DetectFormatAsync(Open, TestContext.Current.CancellationToken);
        await unknown.Should().ThrowAsync<NotSupportedException>();
        catalog.Register(captured);
        (await catalog.DetectFormatAsync(Open, TestContext.Current.CancellationToken))
            .Should().BeSameAs(second);
    }

    [Fact]
    public async Task DefaultDetectors_ShareOneStreamAndPreserveFormatIdentity()
    {
        var catalog = new ChartFormatCatalog();
        var opens = 0;
        Stream Open()
        {
            opens++;
            return new MemoryStream(Encoding.UTF8.GetBytes("{\"formatVersion\":3}"));
        }
        var detected = await catalog.DetectFormatAsync(Open, TestContext.Current.CancellationToken);
        detected.Should().BeSameAs(catalog.Get("PhigrosV3"));
        opens.Should().Be(1);
    }

    [Fact]
    public async Task DetectionOnlyFormat_HasTheSameCapabilitiesAsDetectionOnlyBuiltIn()
    {
        var catalog = new ChartFormatCatalog();
        var descriptor = ChartFormatDescriptor.CreateDetectionOnly("tests.detect-only", "chart");
        catalog.Register(descriptor, async (stream, ct) =>
        {
            using var reader = new StreamReader(stream, leaveOpen: true);
            return await reader.ReadLineAsync(ct) == "仅检测";
        });
        var (formatId, builtIn) = ("PhigrosV1", catalog.Get(ChartFormatIds.PhigrosV1));
        catalog.Find(formatId).Should().BeSameAs(builtIn);
        descriptor.CanImport.Should().Be(builtIn.CanImport).And.BeFalse();
        descriptor.CanExport.Should().Be(builtIn.CanExport).And.BeFalse();
        (await catalog.DetectFormatAsync("仅检测\n内容", TestContext.Current.CancellationToken))
            .Should().BeSameAs(descriptor);
        var import = () => catalog.ImportDetectedIrAsync("仅检测\n内容", ct: TestContext.Current.CancellationToken);
        await import.Should().ThrowAsync<NotSupportedException>();
        catalog.Unregister(descriptor.FormatId).Should().BeTrue();
    }
}
