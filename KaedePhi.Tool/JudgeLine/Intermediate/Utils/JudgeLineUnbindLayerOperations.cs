using KaedePhi.Tool.Event.Intermediate;
using EventLayer = KaedePhi.Core.Intermediate.Model.Events.EventLayer;

namespace KaedePhi.Tool.JudgeLine.Intermediate.Utils;

/// <summary>
/// 提供解绑前后的判定线事件层整理操作。
/// </summary>
internal static class JudgeLineUnbindLayerOperations
{
    /// <summary>
    /// 清理判定线与父线的全零事件层。
    /// </summary>
    internal static void CleanupRedundantLayers(Ir.JudgeLine judgeLineCopy, Ir.JudgeLine fatherLineCopy)
    {
        judgeLineCopy.EventLayers =
            RemoveUnlessLayer(judgeLineCopy.EventLayers) ?? judgeLineCopy.EventLayers;
        fatherLineCopy.EventLayers =
            RemoveUnlessLayer(fatherLineCopy.EventLayers) ?? fatherLineCopy.EventLayers;
    }

    private static List<EventLayer>? RemoveUnlessLayer(List<EventLayer>? layers)
    {
        if (layers is not { Count: > 1 })
            return layers;
        var layersCopy = layers.Select(layer => layer.Clone()).ToList();
        var intCompressor = new EventCompressor<int>();
        var doubleCompressor = new EventCompressor<double>();
        foreach (var layer in layersCopy)
        {
            layer.AlphaEvents = intCompressor.RemoveUselessEvent(layer.AlphaEvents);
            layer.MoveXEvents = doubleCompressor.RemoveUselessEvent(layer.MoveXEvents);
            layer.MoveYEvents = doubleCompressor.RemoveUselessEvent(layer.MoveYEvents);
            layer.RotateEvents = doubleCompressor.RemoveUselessEvent(layer.RotateEvents);
        }

        return layersCopy;
    }

    /// <summary>
    /// 将解绑后的 X/Y 事件写回判定线并清除父线关系。
    /// </summary>
    internal static void WriteResultToLine(
        Ir.JudgeLine line,
        List<IrEvents.Event<double>> newXEvents,
        List<IrEvents.Event<double>> newYEvents,
        List<IrEvents.Event<double>> fatherRotateEvents,
        Func<
            List<IrEvents.Event<double>>,
            List<IrEvents.Event<double>>,
            List<IrEvents.Event<double>>
        > merge
    )
    {
        for (var i = 1; i < line.EventLayers.Count; i++)
        {
            line.EventLayers[i].MoveXEvents?.Clear();
            line.EventLayers[i].MoveYEvents?.Clear();
        }

        if (line.EventLayers.Count == 0)
            line.EventLayers.Add(new EventLayer());

        line.EventLayers[0].MoveXEvents = newXEvents;
        line.EventLayers[0].MoveYEvents = newYEvents;

        if (line.RotateWithFather)
        {
            var merged = merge(line.EventLayers[0].RotateEvents ?? [], fatherRotateEvents);
            line.EventLayers[0].RotateEvents = merged;
        }

        line.Father = -1;
    }
}
