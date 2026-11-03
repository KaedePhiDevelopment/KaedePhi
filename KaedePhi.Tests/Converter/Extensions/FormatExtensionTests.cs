using System.Text;
using KaedePhi.Core.Intermediate.Model;
using KaedePhi.Tool.Common;
using KaedePhi.Tool.Converter;
using KaedePhi.Tool.Layer;
using KaedePhi.Tool.Layer.Intermediate;
using EventLayer = KaedePhi.Core.Intermediate.Model.Events.EventLayer;

namespace KaedePhi.Tests.Converter.Extensions;

[Collection("共享格式注册表")]
public sealed class FormatExtensionTests
{
    private static ChartFormatDescriptor CreateDescriptor(string id) =>
        ChartFormatDescriptor.Create(
            id, "txt", importer: (text, _, _, _) =>
                Task.FromResult(new Chart { Meta = new Meta { Name = text } })
        );

    private static async ValueTask<bool> DetectExternal(Stream stream, CancellationToken ct)
    {
        using var reader = new StreamReader(stream, leaveOpen: true);
        return await reader.ReadLineAsync(ct) == "KAED_TEST";
    }

    [Fact]
    public async Task ExternalSerializerAndConverter_WorkWithDetectionAndIrToolPipeline()
    {
        const string id = "tests.typed-extension";
        var serializer = new ExternalChartSerializer();
        var descriptor = ChartFormatDescriptor.Create<ExternalChart, string?, string?>(
            id,
            "txt",
            serializer,
            () => new ExternalChartConverter(),
            () => "前缀:",
            () => ":后缀"
        );
        var logs = new List<string>();
        var sink = new ChartLogSink { Info = logs.Add };
        ChartFormatRegistry.Register(descriptor, DetectExternal);
        var path = Path.Combine(Path.GetTempPath(), $"kaedephi-{Guid.NewGuid():N}.txt");
        try
        {
            var (format, chart) = await ChartFormatRegistry.ImportDetectedIrAsync(
                "KAED_TEST\n标题", log: sink, ct: TestContext.Current.CancellationToken
            );
            format.Should().BeSameAs(descriptor);
            format.FormatId.Should().Be(id);
            chart.Meta.Name.Should().Be("前缀:标题");

            ILayerProcessor<EventLayer> processor = new LayerProcessor();
            var source = ChartPipeline.FromIr(chart, TestContext.Current.CancellationToken);
            var processed = source.Transform(ir =>
            {
                var line = ir.JudgeLineList[0];
                line.EventLayers = [processor.LayerMerge(line.EventLayers, 8)];
                return ir;
            });
            source.ToIr().JudgeLineList[0].EventLayers.Should().HaveCount(2);
            chart.JudgeLineList[0].EventLayers.Should().HaveCount(2);
            await format.ExportAsync(
                processed.ToIr(), path, new ChartWriteSettings { Indented = true }, log: sink,
                ct: TestContext.Current.CancellationToken
            );
            (await File.ReadAllTextAsync(path, TestContext.Current.CancellationToken))
                .Should().Be("KAED_TEST\n前缀:标题:后缀|1");
            serializer.LastIndented.Should().BeTrue();
            logs.Should().Equal("导入", "导出");
        }
        finally
        {
            ChartFormatRegistry.UnregisterFormat(id);
            if (File.Exists(path))
                File.Delete(path);
        }
    }

    [Fact]
    public async Task Detection_RestoresOffsetAndIsolatesDetectors()
    {
        const string id = "tests.position";
        var descriptor = CreateDescriptor(id);
        ChartFormatRegistry.Register(descriptor, async (stream, ct) =>
        {
            stream.Position.Should().Be(0);
            var result = await DetectExternal(stream, ct);
            stream.Dispose();
            return result;
        });
        try
        {
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes("prefixKAED_TEST\n内容"));
            stream.Position = 6;
            var format = await ChartGetType.DetectFormatAsync(stream, TestContext.Current.CancellationToken);
            format.Should().BeSameAs(descriptor);
            stream.Position.Should().Be(6);
            stream.CanRead.Should().BeTrue();

            var (importedFormat, chart) = await ChartFormatRegistry.ImportDetectedIrAsync(
                stream, ct: TestContext.Current.CancellationToken
            );
            importedFormat.Should().BeSameAs(descriptor);
            chart.Meta.Name.Should().Be("KAED_TEST\n内容");
            stream.CanRead.Should().BeTrue();
        }
        finally
        {
            ChartFormatRegistry.UnregisterFormat(id);
        }
    }

    [Fact]
    public async Task NonSeekableFactory_ReopensAndDisposesEveryProbeAndImportStream()
    {
        const string id = "tests.nonseekable";
        var descriptor = ChartFormatDescriptor.Create<ExternalChart, string?, string?>(
            id, "txt", new ExternalChartSerializer(), () => new ExternalChartConverter()
        );
        var opened = 0;
        var disposed = 0;
        Stream Open()
        {
            opened++;
            return new NonSeekableReadStream(
                new MemoryStream(Encoding.UTF8.GetBytes("KAED_TEST\n内容")),
                () => disposed++
            );
        }

        ChartFormatRegistry.Register(descriptor, DetectExternal);
        try
        {
            using var nonSeekable = Open();
            var invalid = () => ChartGetType.DetectFormatAsync(nonSeekable);
            await invalid.Should().ThrowAsync<ArgumentException>();

            var (format, chart) = await ChartFormatRegistry.ImportDetectedIrAsync(
                Open, ct: TestContext.Current.CancellationToken
            );
            format.Should().BeSameAs(descriptor);
            chart.Meta.Name.Should().Be("内容");
            opened.Should().BeGreaterThan(2);
            disposed.Should().Be(opened - 1);
        }
        finally
        {
            ChartFormatRegistry.UnregisterFormat(id);
        }
    }

    [Fact]
    public async Task Detection_ResolvesPriorityAndReportsTies()
    {
        const string highId = "tests.priority-high";
        const string equalId = "tests.priority-equal";
        const string lowId = "tests.priority-low";
        const string json = "{\"formatVersion\":3}";
        static ValueTask<bool> Match(Stream _, CancellationToken __) => ValueTask.FromResult(true);
        ChartFormatRegistry.Register(CreateDescriptor(lowId), Match, -1);
        try
        {
            (await ChartGetType.DetectFormatAsync(json, TestContext.Current.CancellationToken))
                .FormatId.Should().Be(ChartFormatIds.PhigrosV3);
            ChartFormatRegistry.Register(CreateDescriptor(equalId), Match);
            var ambiguous = () => ChartGetType.DetectFormatAsync(json, TestContext.Current.CancellationToken);
            var exception = await ambiguous.Should().ThrowAsync<InvalidOperationException>();
            exception.Which.Message.Should().Contain(equalId).And.Contain("PhigrosV3");

            ChartFormatRegistry.Register(CreateDescriptor(highId), Match, 1);
            (await ChartGetType.DetectFormatAsync(json, TestContext.Current.CancellationToken))
                .FormatId.Should().Be(highId);
            ChartFormatRegistry.UnregisterFormat(highId).Should().BeTrue();
            ChartFormatRegistry.UnregisterFormat(equalId).Should().BeTrue();
            (await ChartGetType.DetectFormatAsync("未知格式", TestContext.Current.CancellationToken))
                .FormatId.Should().Be(lowId);
        }
        finally
        {
            ChartFormatRegistry.UnregisterFormat(highId);
            ChartFormatRegistry.UnregisterFormat(equalId);
            ChartFormatRegistry.UnregisterFormat(lowId);
        }
    }

    [Fact]
    public async Task Detection_PropagatesDetectorErrorsAndRestoresStream()
    {
        const string id = "tests.detector-error";
        ChartFormatRegistry.Register(CreateDescriptor(id), (stream, _) =>
        {
            stream.ReadByte();
            throw new InvalidDataException("检测失败");
        }, 1);
        try
        {
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes("xKAED_TEST"));
            stream.Position = 1;
            var detect = () => ChartGetType.DetectFormatAsync(stream, TestContext.Current.CancellationToken);
            await detect.Should().ThrowAsync<InvalidDataException>();
            stream.Position.Should().Be(1);
            stream.CanRead.Should().BeTrue();
        }
        finally
        {
            ChartFormatRegistry.UnregisterFormat(id);
        }
    }

    [Fact]
    public async Task Detection_ObservesCancellationAndUnregistersDetector()
    {
        const string id = "tests.detector-cancel";
        var invoked = false;
        ChartFormatRegistry.Register(CreateDescriptor(id), (_, _) =>
        {
            invoked = true;
            return ValueTask.FromResult(true);
        }, 1);
        try
        {
            var detect = () => ChartGetType.DetectFormatAsync("未知格式", new CancellationToken(true));
            await detect.Should().ThrowAsync<OperationCanceledException>();
            invoked.Should().BeFalse();
        }
        finally
        {
            ChartFormatRegistry.UnregisterFormat(id);
        }
        var unknown = () => ChartGetType.DetectFormatAsync("未知格式", TestContext.Current.CancellationToken);
        await unknown.Should().ThrowAsync<NotSupportedException>();
    }

    [Fact]
    public async Task Detection_CancellationDuringProbeRestoresPositionAndSkipsOtherDetectors()
    {
        const string id = "tests.detector-cancel-during";
        using var cts = new CancellationTokenSource();
        ChartFormatRegistry.Register(CreateDescriptor(id), (stream, ct) =>
        {
            ct.Should().Be(cts.Token);
            stream.ReadByte();
            cts.Cancel();
            return ValueTask.FromResult(false);
        }, 1);
        try
        {
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes("prefix{\"formatVersion\":3}"));
            stream.Position = 6;
            var detect = () => ChartGetType.DetectFormatAsync(stream, cts.Token);
            await detect.Should().ThrowAsync<OperationCanceledException>();
            stream.Position.Should().Be(6);
            stream.CanRead.Should().BeTrue();
        }
        finally
        {
            ChartFormatRegistry.UnregisterFormat(id);
        }
    }

    [Fact]
    public async Task Detection_UsesRegistrationSnapshotWhenDetectorIsUnregistered()
    {
        const string id = "tests.detector-snapshot";
        var descriptor = CreateDescriptor(id);
        ChartFormatRegistry.Register(descriptor, (_, _) =>
        {
            ChartFormatRegistry.UnregisterFormat(id);
            return ValueTask.FromResult(true);
        }, 1);
        try
        {
            var result = await ChartGetType.DetectFormatAsync("未知格式", TestContext.Current.CancellationToken);
            result.Should().BeSameAs(descriptor);
            ChartFormatRegistry.Find(id).Should().BeNull();
            var detectAgain = () => ChartGetType.DetectFormatAsync("未知格式", TestContext.Current.CancellationToken);
            await detectAgain.Should().ThrowAsync<NotSupportedException>();
        }
        finally
        {
            ChartFormatRegistry.UnregisterFormat(id);
        }
    }

    [Fact]
    public async Task TypedSerializer_RejectsWrongOptionsAndKeepsExistingOutputOnFailure()
    {
        var serializer = new ExternalChartSerializer { FailWrite = true };
        var descriptor = ChartFormatDescriptor.Create<ExternalChart, string?, string?>(
            "tests.typed-failure", "txt", serializer, () => new ExternalChartConverter()
        );
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes("KAED_TEST\n标题"));
        var invalidOptions = () => descriptor.ImportStreamIrAsync(
            stream, 42, ct: TestContext.Current.CancellationToken
        );
        await invalidOptions.Should().ThrowAsync<ArgumentException>();

        var chart = new ExternalChartConverter().ToIr(new ExternalChart { Name = "标题" }, null);
        var path = Path.Combine(Path.GetTempPath(), $"kaedephi-{Guid.NewGuid():N}.txt");
        try
        {
            await File.WriteAllTextAsync(path, "原文件", TestContext.Current.CancellationToken);
            var export = () => descriptor.ExportAsync(chart, path, ct: TestContext.Current.CancellationToken);
            await export.Should().ThrowAsync<InvalidDataException>();
            (await File.ReadAllTextAsync(path, TestContext.Current.CancellationToken)).Should().Be("原文件");
            Directory.GetFiles(Path.GetDirectoryName(path)!, Path.GetFileName(path) + ".*.tmp")
                .Should().BeEmpty();
        }
        finally
        {
            if (File.Exists(path))
                File.Delete(path);
        }
    }

    [Fact]
    public async Task Detection_PropagatesStreamReadFailureInsteadOfTryingAnotherFormat()
    {
        const string id = "tests.read-error";
        var invoked = false;
        ChartFormatRegistry.Register(CreateDescriptor(id), (_, _) =>
        {
            invoked = true;
            return ValueTask.FromResult(true);
        });
        try
        {
            var detect = () => ChartGetType.DetectFormatAsync(
                () => new FailingReadStream(), TestContext.Current.CancellationToken
            );
            var exception = await detect.Should().ThrowAsync<NotSupportedException>();
            exception.Which.Message.Should().Be("读取失败");
            invoked.Should().BeFalse();
        }
        finally
        {
            ChartFormatRegistry.UnregisterFormat(id);
        }
    }

    [Fact]
    public async Task BinaryFormat_CanBeDetectedAndImportedFromStream()
    {
        const string id = "tests.binary";
        byte[] bytes = [0, 255, 7, 42];
        var descriptor = ChartFormatDescriptor.Create(
            id,
            "bin",
            streamImporter: async (stream, _, _, ct) =>
            {
                var input = new byte[4];
                await stream.ReadExactlyAsync(input, ct);
                return new Chart { Meta = new Meta { Name = Convert.ToHexString(input) } };
            }
        );
        ChartFormatRegistry.Register(descriptor, async (stream, ct) =>
        {
            var header = new byte[2];
            return await stream.ReadAtLeastAsync(header, 2, false, ct) == 2
                && header[0] == 0 && header[1] == 255;
        });
        try
        {
            using var stream = new MemoryStream(bytes);
            var (format, chart) = await ChartFormatRegistry.ImportDetectedIrAsync(
                stream, ct: TestContext.Current.CancellationToken
            );
            format.Should().BeSameAs(descriptor);
            chart.Meta.Name.Should().Be("00FF072A");
        }
        finally
        {
            ChartFormatRegistry.UnregisterFormat(id);
        }
    }

    [Fact]
    public async Task Detection_ReportsConflictsBetweenExternalFormats()
    {
        const string first = "tests.conflict-first";
        const string second = "tests.conflict-second";
        ChartFormatRegistry.Register(CreateDescriptor(first), DetectExternal);
        ChartFormatRegistry.Register(CreateDescriptor(second), DetectExternal);
        try
        {
            var detect = () => ChartGetType.DetectFormatAsync("KAED_TEST\n标题", TestContext.Current.CancellationToken);
            var exception = await detect.Should().ThrowAsync<InvalidOperationException>();
            exception.Which.Message.Should().Contain(first).And.Contain(second);
        }
        finally
        {
            ChartFormatRegistry.UnregisterFormat(first);
            ChartFormatRegistry.UnregisterFormat(second);
        }
    }

    [Fact]
    public void Pipeline_UsesExternalConvertersAndIsolatesProcessedCharts()
    {
        var converter = new ExternalChartConverter();
        var pipeline = ChartPipeline.From<ExternalChart, string?, string?>(
            new ExternalChart { Name = "标题" }, converter, "源:", TestContext.Current.CancellationToken
        );
        Chart? captured = null;
        var processed = pipeline.Transform(chart =>
        {
            captured = chart;
            chart.Meta.Name += ":处理";
            return chart;
        });
        captured!.Meta.Name = "外部修改";
        pipeline.ToIr().Meta.Name.Should().Be("源:标题");
        processed.ToIr().Meta.Name.Should().Be("源:标题:处理");
        processed.To<ExternalChart, string?, string?>(new ExternalChartConverter(), ":目标")
            .Name.Should().Be("源:标题:处理:目标");
    }

    [Fact]
    public async Task TypedSerializer_DryRunDoesNotReplaceExistingOutput()
    {
        var serializer = new ExternalChartSerializer();
        var descriptor = ChartFormatDescriptor.Create<ExternalChart, string?, string?>(
            "tests.typed-dry-run", "txt", serializer, () => new ExternalChartConverter()
        );
        var chart = new ExternalChartConverter().ToIr(new ExternalChart { Name = "标题" }, null);
        var path = Path.Combine(Path.GetTempPath(), $"kaedephi-{Guid.NewGuid():N}.txt");
        try
        {
            await File.WriteAllTextAsync(path, "原文件", TestContext.Current.CancellationToken);
            await descriptor.ExportAsync(
                chart, path, new ChartWriteSettings { DryRun = true, Indented = true },
                ct: TestContext.Current.CancellationToken
            );
            (await File.ReadAllTextAsync(path, TestContext.Current.CancellationToken)).Should().Be("原文件");
            serializer.LastIndented.Should().BeTrue();
        }
        finally
        {
            if (File.Exists(path))
                File.Delete(path);
        }
    }

    [Fact]
    public async Task TypedSerializer_DryRunRejectsInvalidOutputPath()
    {
        var serializer = new ExternalChartSerializer();
        var descriptor = ChartFormatDescriptor.Create<ExternalChart, string?, string?>(
            "tests.typed-invalid-path", "txt", serializer, () => new ExternalChartConverter()
        );
        var chart = new ExternalChartConverter().ToIr(new ExternalChart { Name = "标题" }, null);
        var export = () => descriptor.ExportAsync(
            chart, "", new ChartWriteSettings { DryRun = true }, ct: TestContext.Current.CancellationToken
        );
        await export.Should().ThrowAsync<ArgumentException>();
        serializer.LastIndented.Should().BeNull();
    }

    [Theory]
    [InlineData("{\"formatVersion\":1}", ChartFormatIds.PhigrosV1)]
    [InlineData("{\"formatVersion\":3}", ChartFormatIds.PhigrosV3)]
    [InlineData("{\"META\":{},\"judgeLineList\":[]}", ChartFormatIds.RePhiEdit)]
    [InlineData("{\"info\":{},\"lines\":[]}", ChartFormatIds.PhiFans)]
    [InlineData("{\"format\":6,\"bpm_list\":[]}", ChartFormatIds.PhiChain)]
    [InlineData("123\n", ChartFormatIds.PhiEdit)]
    public async Task Detection_PreservesBuiltInIdentification(string text, string formatId)
    {
        (await ChartGetType.DetectFormatAsync(text, TestContext.Current.CancellationToken))
            .Should().BeSameAs(ChartFormatRegistry.Get(formatId));
    }
}
