using System.Collections;
using System.Globalization;
using System.Resources;
using System.Text.RegularExpressions;
using KaedePhi.Tool.Common;

namespace KaedePhi.Tests.Common;

public partial class ToolResourcesTests
{
    private static readonly ResourceManager Manager = new(
        "KaedePhi.Tool.Common.ToolResources",
        typeof(ChartGetType).Assembly
    );

    [Theory]
    [InlineData("en-US")]
    [InlineData("zh-Hant")]
    public void LocalizedResources_ContainEveryNeutralKeyWithSamePlaceholders(string cultureName)
    {
        var neutral = ReadStrings(CultureInfo.InvariantCulture);
        var localized = ReadStrings(CultureInfo.GetCultureInfo(cultureName));

        localized.Keys.Should().BeEquivalentTo(neutral.Keys);
        foreach (var (key, value) in neutral)
        {
            GetPlaceholders(localized[key])
                .Should()
                .BeEquivalentTo(GetPlaceholders(value), $"资源 {key} 的占位符需要与默认语言一致");
        }
    }

    private static Dictionary<string, string> ReadStrings(CultureInfo culture)
    {
        var resourceSet =
            Manager.GetResourceSet(culture, createIfNotExists: true, tryParents: false)
            ?? throw new InvalidOperationException($"未找到资源集：{culture.Name}");
        return resourceSet
            .Cast<DictionaryEntry>()
            .ToDictionary(entry => (string)entry.Key, entry => (string)entry.Value!);
    }

    private static HashSet<string> GetPlaceholders(string text) =>
        PlaceholderPattern().Matches(text).Select(match => match.Value).ToHashSet();

    [GeneratedRegex(@"\{\d+\}")]
    private static partial Regex PlaceholderPattern();
}
