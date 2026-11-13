using KaedePhi.Tool.Common;

namespace KaedePhi.Tool.JudgeLine;

/// <summary>
/// 提供统一的判定线解绑入口及坐标计算功能。
/// </summary>
/// <typeparam name="TJudgeLine">判定线类型。</typeparam>
public interface IJudgeLineUnbindService<TJudgeLine> : ILoggable
{
    /// <summary>
    /// 沿父判定线解析器提供的关系逐级处理，并将目标判定线与父判定线解绑。
    /// </summary>
    /// <param name="targetJudgeLine">需要解绑的目标判定线。</param>
    /// <param name="fatherResolver">获取指定判定线直接父线的函数；根判定线应返回空值。</param>
    /// <param name="options">采样策略、采样精度和渲染坐标系选项。</param>
    /// <param name="progress">解绑进度回调。</param>
    /// <param name="cancellationToken">取消解绑操作的令牌。</param>
    /// <returns>解绑后的目标判定线。</returns>
    /// <exception cref="FormatException">父级关系存在循环或父级链超过安全深度时引发。</exception>
    TJudgeLine Unbind(
        TJudgeLine targetJudgeLine,
        Func<TJudgeLine, TJudgeLine?> fatherResolver,
        JudgeLineUnbindOptions options,
        IProgress<ToolProgress>? progress = null,
        CancellationToken cancellationToken = default
    );
}
