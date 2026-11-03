using System.Globalization;

namespace KaedePhi.Tests.Common;

public class EnglishTestCultureTests
{
    [Fact]
    public void TestAssembly_UsesEnglishCulture()
    {
        CultureInfo
            .DefaultThreadCurrentCulture.Should()
            .Be(CultureInfo.GetCultureInfo("en-US"));
        CultureInfo
            .DefaultThreadCurrentUICulture.Should()
            .Be(CultureInfo.GetCultureInfo("en-US"));
        CultureInfo.CurrentCulture.Name.Should().Be("en-US");
        CultureInfo.CurrentUICulture.Name.Should().Be("en-US");
    }
}
