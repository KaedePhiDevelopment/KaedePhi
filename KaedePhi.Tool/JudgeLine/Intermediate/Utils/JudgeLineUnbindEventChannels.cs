namespace KaedePhi.Tool.JudgeLine.Intermediate.Utils;

/// <summary>
/// 封装解绑计算所需的父线 X/Y/旋转和子线 X/Y 事件通道。
/// </summary>
internal readonly record struct JudgeLineUnbindEventChannels(
    List<IrEvents.Event<double>> Fx,
    List<IrEvents.Event<double>> Fy,
    List<IrEvents.Event<double>> Fr,
    List<IrEvents.Event<double>> Tx,
    List<IrEvents.Event<double>> Ty
);
