using System.Reflection;
using System.Runtime.CompilerServices;
using KaedePhi.Tool.Common;
using KaedePhi.Tool.Converter;

namespace KaedePhi.Tests.Compatibility;

public sealed class FormatApiObsoleteTests
{
    [Fact]
    public void LegacyFormatApis_AreMarkedForRemovalWithMigrationGuidance()
    {
        var legacyType = LegacyApi.ToolType("KaedePhi.Tool.Common.ChartType");
        var cases = new (MemberInfo Member, string Replacement)[]
        {
            (legacyType, "FormatId"),
            (typeof(ChartFormatDescriptor).GetConstructor(Type.EmptyTypes)!, "Create"),
            (typeof(ChartFormatDescriptor).GetProperty("Type")!, "FormatId"),
            (typeof(ChartFormatDescriptor).GetProperty("BuiltInType")!, "FormatId"),
            (typeof(ChartGetType).GetMethod("GetType", [typeof(string)])!, "DetectFormatAsync"),
            (typeof(ChartGetType).GetMethod("GetType", [typeof(TextReader)])!, "DetectFormatAsync"),
            (typeof(ChartGetType).GetMethod("GetType", [typeof(Stream)])!, "DetectFormatAsync"),
            (
                typeof(ChartGetType).GetMethod(
                    "GetTypeAsync",
                    [typeof(Stream), typeof(CancellationToken)]
                )!,
                "DetectFormatAsync"
            ),
            (
                typeof(ChartFormatCatalog).GetMethod(
                    nameof(ChartFormatCatalog.Find),
                    [legacyType]
                )!,
                "Find(string)"
            ),
            (
                typeof(ChartFormatCatalog).GetMethod(nameof(ChartFormatCatalog.Get), [legacyType])!,
                "Get(string)"
            ),
            (
                typeof(ChartFormatRegistry).GetMethod(
                    nameof(ChartFormatRegistry.Find),
                    [legacyType]
                )!,
                "Find(string)"
            ),
            (
                typeof(ChartFormatRegistry).GetMethod(
                    nameof(ChartFormatRegistry.Get),
                    [legacyType]
                )!,
                "Get(string)"
            ),
            (typeof(ChartFormatRegistry).GetMethod("ImportIrAsync")!, "ImportDetectedIrAsync"),
            (typeof(ChartFormatRegistry).GetMethod("ImportAsync")!, "ImportDetectedIrAsync"),
            (typeof(ChartFormatRegistry).GetMethod("Unregister")!, "UnregisterFormat"),
        };

        foreach (var (member, replacement) in cases)
        {
            var attribute = member.GetCustomAttribute<ObsoleteAttribute>();
            attribute.Should().NotBeNull($"{member.Name} 应提示迁移");
            attribute!.IsError.Should().BeFalse();
            attribute.Message.Should().Contain("下一个大版本移除").And.Contain(replacement);
        }

        typeof(ChartFormatDescriptor)
            .GetProperty("Type")!
            .GetCustomAttribute<RequiredMemberAttribute>()
            .Should()
            .NotBeNull();
    }

    [Fact]
    public void NewFormatApis_AreNotMarkedObsolete()
    {
        typeof(ChartFormatDescriptor)
            .GetProperty(nameof(ChartFormatDescriptor.FormatId))!
            .GetCustomAttribute<ObsoleteAttribute>()
            .Should()
            .BeNull();
        typeof(ChartGetType)
            .GetMethod(
                nameof(ChartGetType.DetectFormatAsync),
                [typeof(string), typeof(CancellationToken)]
            )!
            .GetCustomAttribute<ObsoleteAttribute>()
            .Should()
            .BeNull();
        typeof(ChartFormatRegistry)
            .GetMethod(nameof(ChartFormatRegistry.UnregisterFormat))!
            .GetCustomAttribute<ObsoleteAttribute>()
            .Should()
            .BeNull();
        typeof(ChartFormatCatalog)
            .GetMethod(nameof(ChartFormatCatalog.Find), [typeof(string)])!
            .GetCustomAttribute<ObsoleteAttribute>()
            .Should()
            .BeNull();
    }

    [Fact]
    public void LegacyEnumAliasesAndConstructor_PreserveExistingValues()
    {
        var legacyType = LegacyApi.ToolType("KaedePhi.Tool.Common.ChartType");
        var typeProperty = typeof(ChartFormatDescriptor).GetProperty("Type")!;
        var builtInProperty = typeof(ChartFormatDescriptor).GetProperty("BuiltInType")!;
        var catalog = new ChartFormatCatalog();
        foreach (var value in Enum.GetValues(legacyType))
        {
            var formatId = value.ToString()!;
            var format = catalog.Get(formatId);
            typeProperty.GetValue(format).Should().Be(value);
            builtInProperty.GetValue(format).Should().Be(value);
            LegacyApi
                .Invoke(typeof(ChartFormatCatalog), "Find", catalog, value)
                .Should()
                .BeSameAs(format);
            LegacyApi
                .Invoke(typeof(ChartFormatRegistry), "Get", null, value)
                .Should()
                .BeSameAs(format);

            var legacy = (ChartFormatDescriptor)
                Activator.CreateInstance(typeof(ChartFormatDescriptor))!;
            typeProperty.SetValue(legacy, value);
            typeof(ChartFormatDescriptor)
                .GetProperty(nameof(ChartFormatDescriptor.FileExtension))!
                .SetValue(legacy, "json");
            legacy.FormatId.Should().Be(formatId);
        }
        var external = ChartFormatDescriptor.CreateDetectionOnly("tests.external", "test");
        Convert.ToInt32(typeProperty.GetValue(external)).Should().Be(-1);
        builtInProperty.GetValue(external).Should().BeNull();
        var undefined = Enum.ToObject(legacyType, -1);
        LegacyApi.Invoke(typeof(ChartFormatCatalog), "Find", catalog, undefined).Should().BeNull();
    }

    [Fact]
    public void LegacyUnregister_StillProtectsDefaultBuiltInFormats()
    {
        var descriptor = ChartFormatRegistry.Get(ChartFormatIds.RePhiEdit);
        LegacyApi
            .Invoke(typeof(ChartFormatRegistry), "Unregister", null, ChartFormatIds.RePhiEdit)
            .Should()
            .Be(false);
        ChartFormatRegistry.Get(ChartFormatIds.RePhiEdit).Should().BeSameAs(descriptor);
    }

    [Fact]
    public async Task LegacyDetectionAndImport_ContinueToReturnEnumValues()
    {
        const string text = "{\"META\":{},\"judgeLineList\":[]}";
        using var reader = new StringReader(text);
        LegacyApi
            .Invoke(typeof(ChartGetType), "GetType", null, reader)!
            .ToString()
            .Should()
            .Be(ChartFormatIds.RePhiEdit);
        using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(text));
        var detected = await LegacyApi.AwaitResult(
            LegacyApi.Invoke(
                typeof(ChartGetType),
                "GetTypeAsync",
                null,
                stream,
                TestContext.Current.CancellationToken
            )
        );
        detected!.ToString().Should().Be(ChartFormatIds.RePhiEdit);
        var imported = (ITuple)
            (
                await LegacyApi.AwaitResult(
                    LegacyApi.Invoke(
                        typeof(ChartFormatRegistry),
                        "ImportIrAsync",
                        null,
                        text,
                        null,
                        null,
                        TestContext.Current.CancellationToken
                    )
                )
            )!;
        imported[0]!.ToString().Should().Be(ChartFormatIds.RePhiEdit);
        imported[1].Should().BeOfType<Core.Intermediate.Model.Chart>();
    }
}
