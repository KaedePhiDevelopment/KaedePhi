using KaedePhi.Tool.Common;

namespace KaedePhi.Tool.JudgeLine;

/// <summary>
/// 提供统一的判定线解绑入口及坐标计算功能。
/// </summary>
/// <typeparam name="TJudgeLine">判定线类型。</typeparam>
public interface IJudgeLineUnbindService<TJudgeLine> : ILoggable
{
    /// <summary>
    /// 根据统一选项将目标判定线与父判定线解绑。
    /// </summary>
    /// <param name="targetJudgeLineIndex">目标判定线在列表中的索引。</param>
    /// <param name="allTJudgeLines">当前谱面的全部判定线。</param>
    /// <param name="options">采样策略、采样精度和渲染坐标系选项。</param>
    /// <param name="progress">进度回调。</param>
    /// <param name="cancellationToken">取消解绑操作的令牌。</param>
    /// <returns>解绑后的判定线。</returns>
    TJudgeLine Unbind(
        int targetJudgeLineIndex,
        List<TJudgeLine> allTJudgeLines,
        JudgeLineUnbindOptions options,
        IProgress<ToolProgress>? progress = null,
        CancellationToken cancellationToken = default
    );
}
