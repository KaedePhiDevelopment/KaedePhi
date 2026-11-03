using KaedePhi.Core.Intermediate.Model;
using KaedePhi.Core.Intermediate.Model.Events;
using KaedePhi.Tool.Common;
using KaedePhi.Tool.Converter;

namespace KaedePhi.Tests.Converter.Extensions;

public sealed class ExternalChartConverter
    : LoggableBase, IChartConverter<ExternalChart, string?, string?>, ICancellableChartConverter
{
    private CancellationToken _ct;

    public void SetCancellationToken(CancellationToken ct) => _ct = ct;

    public Chart ToIr(ExternalChart input, string? options)
    {
        _ct.ThrowIfCancellationRequested();
        LogInfo("导入");
        return new Chart
        {
            Meta = new Meta { Name = options + input.Name },
            JudgeLineList =
            [
                new JudgeLine { EventLayers = [new EventLayer(), new EventLayer()] },
            ],
        };
    }

    public ExternalChart FromIr(Chart input, string? options)
    {
        _ct.ThrowIfCancellationRequested();
        LogInfo("导出");
        return new ExternalChart
        {
            Name = input.Meta.Name + options,
            LayerCount = input.JudgeLineList[0].EventLayers.Count,
        };
    }
}
