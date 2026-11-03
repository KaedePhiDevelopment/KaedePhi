using System.Globalization;
using Xunit;

[assembly: AssemblyFixture(typeof(KaedePhi.Tests.Common.EnglishTestCultureFixture))]

namespace KaedePhi.Tests.Common;

public sealed class EnglishTestCultureFixture
{
    public EnglishTestCultureFixture()
    {
        var culture = CultureInfo.GetCultureInfo("en-US");
        CultureInfo.DefaultThreadCurrentCulture = culture;
        CultureInfo.DefaultThreadCurrentUICulture = culture;
        CultureInfo.CurrentCulture = culture;
        CultureInfo.CurrentUICulture = culture;
    }
}
