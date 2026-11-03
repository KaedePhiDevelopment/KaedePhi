using KaedePhi.Tool.Common;
using KaedePhi.Tool.Converter;

namespace KaedePhi.Tests.Converter.Extensions;

[Collection("共享格式注册表")]
public sealed class SharedCatalogMutationTests
{
    [Fact]
    public async Task DefaultRegistry_CanRemoveAndRestoreBuiltInRegistration()
    {
        const string id = "PhigrosV1";
        const string input = "{\"formatVersion\":1}";
        var saved = ChartFormatRegistry.Default.FindRegistration(id)!;
        try
        {
            ChartFormatRegistry.UnregisterFormat(id).Should().BeTrue();
            ChartFormatRegistry.Find(id).Should().BeNull();
            var detect = () => ChartGetType.DetectFormatAsync(input, TestContext.Current.CancellationToken);
            await detect.Should().ThrowAsync<NotSupportedException>();

            ChartFormatRegistry.RegisterFormat(saved);
            ChartFormatRegistry.Get(ChartFormatIds.PhigrosV1).Should().BeSameAs(saved.Descriptor);
            (await ChartGetType.DetectFormatAsync(input, TestContext.Current.CancellationToken))
                .Should().BeSameAs(saved.Descriptor);
        }
        finally
        {
            if (ChartFormatRegistry.Default.FindRegistration(id) is null)
                ChartFormatRegistry.RegisterFormat(saved);
        }
    }
}
