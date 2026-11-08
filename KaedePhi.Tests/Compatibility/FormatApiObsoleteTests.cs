using System.Reflection;
using KaedePhi.Tool.Common;
using KaedePhi.Tool.Converter;

namespace KaedePhi.Tests.Compatibility;

public sealed class FormatApiObsoleteTests
{
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
}
