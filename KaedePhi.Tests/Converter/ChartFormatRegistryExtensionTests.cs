using KaedePhi.Tool.Common;
using KaedePhi.Tool.Converter;
using Ir = KaedePhi.Core.Intermediate.Model;

namespace KaedePhi.Tests.Converter;

/// <summary>
/// 验证外部格式注册接口不影响内置格式，并可通过字符串标识调用。
/// </summary>
public sealed class ChartFormatRegistryExtensionTests
{
    [Fact]
    public async Task ExternalFormat_CanRegisterImportAndExportWithoutChartType()
    {
        const string formatId = "tests.simple";
        var descriptor = ChartFormatDescriptor.Create(
            formatId,
            "simple",
            importer: (text, _, _, _) =>
            {
                var chart = new Ir.Chart
                {
                    Meta = new Ir.Meta { Name = text },
                };
                return Task.FromResult(chart);
            },
            exporter: async (chart, path, _, _, _, ct) =>
            {
                await File.WriteAllTextAsync(path, chart.Meta.Name, ct);
            }
        );

        ChartFormatRegistry.Register(descriptor);
        try
        {
            ChartFormatRegistry.Find(formatId).Should().BeSameAs(descriptor);

            var imported = await ChartFormatRegistry.Get(formatId).ImportIrAsync(
                "external-chart",
                ct: TestContext.Current.CancellationToken
            );
            imported.Meta.Name.Should().Be("external-chart");

            var path = Path.Combine(Path.GetTempPath(), $"kaedephi-{Guid.NewGuid():N}.simple");
            try
            {
                await ChartFormatRegistry.Get(formatId).ExportAsync(
                    imported,
                    path,
                    ct: TestContext.Current.CancellationToken
                );
                (await File.ReadAllTextAsync(path, TestContext.Current.CancellationToken))
                    .Should().Be("external-chart");
            }
            finally
            {
                if (File.Exists(path))
                    File.Delete(path);
            }
        }
        finally
        {
            ChartFormatRegistry.Unregister(formatId);
        }
    }

    [Fact]
    public void BuiltInFormatLookupByChartType_RemainsAvailable()
    {
        ChartFormatRegistry.Get(ChartType.RePhiEdit).CanImport.Should().BeTrue();
    }

    [Fact]
    public void ExternalFormat_RejectsDuplicateIdAndCanBeEnumerated()
    {
        const string formatId = "tests.duplicate";
        var descriptor = ChartFormatDescriptor.Create(formatId, "simple", importer: (_, _, _, _) =>
            Task.FromResult(new Ir.Chart()));

        ChartFormatRegistry.Register(descriptor);
        try
        {
            var duplicate = () => ChartFormatRegistry.Register(
                ChartFormatDescriptor.Create(
                    formatId,
                    "simple",
                    importer: (_, _, _, _) => Task.FromResult(new Ir.Chart())
                ));

            duplicate.Should().Throw<InvalidOperationException>();
            ChartFormatRegistry.Formats.Should().Contain(descriptor);
            ChartFormatRegistry.ImportableFormats.Should().Contain(descriptor);
        }
        finally
        {
            ChartFormatRegistry.Unregister(formatId).Should().BeTrue();
        }
    }

    [Fact]
    public async Task StreamOnlyFormat_UsesDefaultOptionsAndRemainsImportable()
    {
        const string formatId = "tests.stream";
        var descriptor = ChartFormatDescriptor.Create(
            formatId,
            "txt",
            streamImporter: async (stream, options, _, ct) =>
            {
                using var reader = new StreamReader(stream, leaveOpen: true);
                return new Ir.Chart
                {
                    Meta = new Ir.Meta { Name = (string)options! + await reader.ReadToEndAsync(ct) },
                };
            },
            importOptionsFactory: () => "prefix:"
        );
        ChartFormatRegistry.Register(descriptor);
        try
        {
            using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes("title"));
            var chart = await ChartFormatRegistry.ImportExternalStreamIrAsync(
                formatId, stream, ct: TestContext.Current.CancellationToken);
            chart.Meta.Name.Should().Be("prefix:title");
            stream.CanRead.Should().BeTrue();
            ChartFormatRegistry.ImportableFormats.Should().Contain(descriptor);
            ChartFormatRegistry.Get("TESTS.STREAM").Should().BeSameAs(descriptor);
        }
        finally
        {
            ChartFormatRegistry.Unregister(formatId);
        }
        ChartFormatRegistry.Find(formatId).Should().BeNull();
    }

    [Fact]
    public void ExternalFormat_CannotReplaceBuiltInFormat()
    {
        var builtIn = ChartFormatRegistry.Get(ChartType.RePhiEdit);
        var descriptor = ChartFormatDescriptor.Create(
            "rephiedit", "txt", importer: (_, _, _, _) => Task.FromResult(new Ir.Chart()));
        var register = () => ChartFormatRegistry.Register(descriptor);
        register.Should().Throw<InvalidOperationException>();
        ChartFormatRegistry.Get(ChartType.RePhiEdit).Should().BeSameAs(builtIn);
        ChartFormatRegistry.Get("RePhiEdit").Should().BeSameAs(builtIn);
    }

    [Fact]
    public async Task CancelledImport_DoesNotInvokeExternalCallback()
    {
        var invoked = false;
        var descriptor = ChartFormatDescriptor.Create(
            "tests.cancelled", "txt", importer: (_, _, _, _) =>
            {
                invoked = true;
                return Task.FromResult(new Ir.Chart());
            });
        var import = () => descriptor.ImportIrAsync("title", ct: new CancellationToken(true));
        await import.Should().ThrowAsync<OperationCanceledException>();
        invoked.Should().BeFalse();
    }
}
