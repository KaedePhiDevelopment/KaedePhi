using System.Globalization;
using KaedePhi.Tool.Common;

namespace KaedePhi.Tool.Compatibility;

internal static class LegacyChartTypeMapping
{
    internal static bool IsDefined(int value) => value is >= 0 and <= 6;

    internal static string ToFormatId(int value) =>
        value switch
        {
            0 => ChartFormatIds.PhiEdit,
            1 => ChartFormatIds.RePhiEdit,
            2 => ChartFormatIds.PhigrosV1,
            3 => ChartFormatIds.PhigrosV3,
            4 => ChartFormatIds.PhiChain,
            5 => ChartFormatIds.PhiFans,
            6 => ChartFormatIds.StellateRePhiEditExtended,
            _ => value.ToString(CultureInfo.InvariantCulture),
        };

    internal static int ToValue(string formatId) =>
        formatId switch
        {
            ChartFormatIds.PhiEdit => 0,
            ChartFormatIds.RePhiEdit => 1,
            ChartFormatIds.PhigrosV1 => 2,
            ChartFormatIds.PhigrosV3 => 3,
            ChartFormatIds.PhiChain => 4,
            ChartFormatIds.PhiFans => 5,
            ChartFormatIds.StellateRePhiEditExtended => 6,
            _ => -1,
        };
}
