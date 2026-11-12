using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using KaedePhi.Tool.JudgeLine.Utils;

namespace KaedePhi.Tool.JudgeLine.Intermediate.Utils;

/// <summary>
/// 管理判定线解绑结果的谱面级缓存。
/// </summary>
internal static class JudgeLineUnbindCache
{
    private static readonly ConditionalWeakTable<
        List<Ir.JudgeLine>,
        ConcurrentDictionary<JudgeLineUnbindCacheKey, Ir.JudgeLine>
    > CacheTable = new();

    /// <summary>
    /// 获取与判定线列表关联的解绑缓存。
    /// </summary>
    /// <param name="allJudgeLines">当前谱面的全部判定线。</param>
    /// <returns>与判定线列表关联的缓存。</returns>
    internal static ConcurrentDictionary<JudgeLineUnbindCacheKey, Ir.JudgeLine> GetOrCreateFor(
        List<Ir.JudgeLine> allJudgeLines
    ) => CacheTable.GetOrCreateValue(allJudgeLines);

    /// <summary>
    /// 命中缓存时返回克隆结果，避免调用方直接持有缓存实例。
    /// </summary>
    internal static bool TryGetCachedClone(
        JudgeLineUnbindCacheKey cacheKey,
        ConcurrentDictionary<JudgeLineUnbindCacheKey, Ir.JudgeLine> cache,
        string logTag,
        [NotNullWhen(true)] out Ir.JudgeLine? cachedClone,
        Action<string>? logDebug = null
    )
    {
        if (cache.TryGetValue(cacheKey, out var cached))
        {
            logDebug?.Invoke(
                $"{logTag}[{cacheKey.TargetJudgeLineIndex}]: 命中缓存，直接返回已解绑结果"
            );
            cachedClone = cached.Clone();
            return true;
        }

        cachedClone = null;
        return false;
    }

    /// <summary>
    /// 判定线无父线时直接缓存并返回。
    /// </summary>
    internal static bool TryReturnWhenNoFather(
        JudgeLineUnbindCacheKey cacheKey,
        Ir.JudgeLine judgeLineCopy,
        ConcurrentDictionary<JudgeLineUnbindCacheKey, Ir.JudgeLine> cache,
        string logTag,
        Action<string>? logWarning = null
    )
    {
        if (judgeLineCopy.Father > -1)
            return false;
        logWarning?.Invoke($"{logTag}[{cacheKey.TargetJudgeLineIndex}]: 判定线无父线，跳过。");
        cache.TryAdd(cacheKey, judgeLineCopy);
        return true;
    }
}
