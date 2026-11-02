using System.Collections.Concurrent;
using JudgeLine = KaedePhi.Core.Intermediate.Model.JudgeLine;

namespace KaedePhi.Tool.JudgeLines.Intermediate.Utils;

/// <summary>
/// IR 判定线父子解绑处理器基类。
/// 封装缓存检查、父线递归解绑准备、通道合并及日志记录等共享逻辑，
/// 具体采样策略由子类（等间隔 / 自适应）实现。
/// </summary>
public abstract class FatherUnbindProcessorBase
{
    /// <summary>
    /// 表示缓存。
    /// </summary>
    protected readonly ConcurrentDictionary<FatherUnbindHelpers.UnbindCacheKey, JudgeLine> Cache;
    /// <summary>
    /// 表示日志信息。
    /// </summary>
    protected readonly Action<string>? LogInfo;
    /// <summary>
    /// 表示日志警告。
    /// </summary>
    protected readonly Action<string>? LogWarning;
    /// <summary>
    /// 表示日志错误。
    /// </summary>
    protected readonly Action<string>? LogError;
    /// <summary>
    /// 表示日志调试。
    /// </summary>
    protected readonly Action<string>? LogDebug;

    /// <summary>
    /// 初始化父子线解绑处理器基类的新实例。
    /// </summary>
    /// <param name="cache">共享缓存</param>
    /// <param name="logInfo">信息日志回调</param>
    /// <param name="logWarning">警告日志回调</param>
    /// <param name="logError">错误日志回调</param>
    /// <param name="logDebug">调试日志回调</param>
    protected FatherUnbindProcessorBase(
        ConcurrentDictionary<FatherUnbindHelpers.UnbindCacheKey, JudgeLine> cache,
        Action<string>? logInfo = null,
        Action<string>? logWarning = null,
        Action<string>? logError = null,
        Action<string>? logDebug = null
    )
    {
        Cache = cache;
        LogInfo = logInfo;
        LogWarning = logWarning;
        LogError = logError;
        LogDebug = logDebug;
    }

    /// <summary>
    /// 表示准备结果数据。
    /// </summary>
    /// <param name="JudgeLine">当前判定线数据</param>
    /// <param name="FatherLine">父判定线数据</param>
    /// <param name="ShouldReturn">是否应提前返回</param>
    protected readonly record struct PrepareResult(
        JudgeLine JudgeLine,
        JudgeLine? FatherLine,
        bool ShouldReturn
    );

    /// <summary>
    /// 准备解绑上下文所需的数据。
    /// </summary>
    /// <param name="targetJudgeLineIndex">目标判定线索引</param>
    /// <param name="allJudgeLines">全部判定线列表</param>
    /// <param name="logTag">日志标识</param>
    /// <param name="startAction">开始处理时使用的说明文字</param>
    /// <param name="cacheKeyFactory">生成缓存键的函数</param>
    /// <param name="recursiveUnbind">递归解绑判定线的函数</param>
    /// <returns>操作得到的结果。</returns>
    protected PrepareResult PrepareUnbindContext(
        int targetJudgeLineIndex,
        List<JudgeLine> allJudgeLines,
        string logTag,
        string startAction,
        Func<int, FatherUnbindHelpers.UnbindCacheKey> cacheKeyFactory,
        Func<int, List<JudgeLine>, JudgeLine> recursiveUnbind
    )
    {
        if (
            FatherUnbindHelpers.TryGetCachedClone(
                cacheKeyFactory(targetJudgeLineIndex),
                Cache,
                logTag,
                out var cached,
                LogDebug
            )
        )
        {
            return new PrepareResult(cached, null, true);
        }

        var judgeLineCopy = allJudgeLines[targetJudgeLineIndex].Clone();
        var allJudgeLinesCopy = allJudgeLines.Select(jl => jl.Clone()).ToList();

        if (
            FatherUnbindHelpers.TryReturnWhenNoFather(
                cacheKeyFactory(targetJudgeLineIndex),
                judgeLineCopy,
                Cache,
                logTag,
                LogWarning
            )
        )
        {
            return new PrepareResult(judgeLineCopy, null, true);
        }

        LogInfo?.Invoke(
            $"{logTag}[{targetJudgeLineIndex}]: {startAction}，父线索引={judgeLineCopy.Father}"
        );

        // allJudgeLinesCopy 已经是克隆列表，直接使用其中的元素，无需再次克隆
        var fatherLineCopy = allJudgeLinesCopy[judgeLineCopy.Father];
        if (fatherLineCopy.Father >= 0)
        {
            LogDebug?.Invoke(
                $"{logTag}[{targetJudgeLineIndex}]: 父线 {judgeLineCopy.Father} 仍有父线，递归解绑"
            );
            fatherLineCopy = recursiveUnbind(judgeLineCopy.Father, allJudgeLinesCopy);
        }

        FatherUnbindHelpers.CleanupRedundantLayers(judgeLineCopy, fatherLineCopy);

        return new PrepareResult(judgeLineCopy, fatherLineCopy, false);
    }
}
