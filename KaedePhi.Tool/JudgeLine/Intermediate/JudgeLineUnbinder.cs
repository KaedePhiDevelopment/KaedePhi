using KaedePhi.Tool.Common;
using KaedePhi.Tool.JudgeLine;
using KaedePhi.Tool.JudgeLine.Intermediate.Utils;
using KaedePhi.Tool.JudgeLine.Utils;

namespace KaedePhi.Tool.JudgeLine.Intermediate;

/// <summary>
/// 基于 IR 判定线数据执行父子解绑。
/// </summary>
public class JudgeLineUnbinder : LoggableBase, IJudgeLineUnbindService<Ir.JudgeLine>
{
    private const int MaximumFatherChainDepth = 256;

    #region 处理器创建

    private FatherUnbindProcessor CreateProcessor(List<Ir.JudgeLine> allJudgeLines)
    {
        return new FatherUnbindProcessor(
            JudgeLineUnbindCache.GetOrCreateFor(allJudgeLines),
            LogInfo,
            LogWarning,
            LogError,
            LogDebug
        );
    }

    private FatherUnbindPlusProcessor CreatePlusProcessor(
        List<Ir.JudgeLine> allJudgeLines,
        double tolerance,
        double mergeTolerance
    )
    {
        return new FatherUnbindPlusProcessor(
            JudgeLineUnbindCache.GetOrCreateFor(allJudgeLines),
            tolerance,
            mergeTolerance,
            LogInfo,
            LogWarning,
            LogError,
            LogDebug
        );
    }

    #endregion

    #region 统一解绑

    private Ir.JudgeLine UnbindByIndex(
        int targetJudgeLineIndex,
        List<Ir.JudgeLine> allJudgeLines,
        JudgeLineUnbindOptions options,
        IProgress<ToolProgress>? progress = null,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(options);

        if (options.RenderProfile is { } renderProfile)
        {
            using var profileScope = JudgeLineUnbindContext.UseRenderProfile(renderProfile);
            return UnbindCore();
        }

        return UnbindCore();

        Ir.JudgeLine UnbindCore() =>
            options.SamplingMode switch
            {
                JudgeLineUnbindSamplingMode.EqualSpacing => ValidateInput(
                        targetJudgeLineIndex,
                        allJudgeLines,
                        options.Precision,
                        null
                    )
                    .CreateProcessor(allJudgeLines)
                    .FatherUnbind(
                        targetJudgeLineIndex,
                        allJudgeLines,
                        options.Precision,
                        progress,
                        cancellationToken
                    ),
                JudgeLineUnbindSamplingMode.Adaptive => ValidateInput(
                        targetJudgeLineIndex,
                        allJudgeLines,
                        options.Precision,
                        options.Tolerance,
                        options.MergeTolerance
                    )
                    .CreatePlusProcessor(allJudgeLines, options.Tolerance, options.MergeTolerance)
                    .FatherUnbind(
                        targetJudgeLineIndex,
                        allJudgeLines,
                        options.Precision,
                        progress,
                        cancellationToken
                    ),
                _ => throw new ArgumentOutOfRangeException(nameof(options.SamplingMode)),
            };
    }

    /// <inheritdoc/>
    public Ir.JudgeLine Unbind(
        Ir.JudgeLine targetJudgeLine,
        Func<Ir.JudgeLine, Ir.JudgeLine?> fatherResolver,
        JudgeLineUnbindOptions options,
        IProgress<ToolProgress>? progress = null,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(targetJudgeLine);
        ArgumentNullException.ThrowIfNull(fatherResolver);
        ArgumentNullException.ThrowIfNull(options);

        var fatherChain = new List<Ir.JudgeLine>();
        var visited = new HashSet<Ir.JudgeLine>(ReferenceEqualityComparer.Instance);
        Ir.JudgeLine? currentLine = targetJudgeLine;

        while (currentLine is not null)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (!visited.Add(currentLine))
                throw new FormatException("判定线父级关系存在循环。");
            if (fatherChain.Count >= MaximumFatherChainDepth)
                throw new FormatException(
                    $"判定线父级链深度超过安全上限 {MaximumFatherChainDepth}。"
                );

            fatherChain.Add(currentLine);
            currentLine = fatherResolver(currentLine);
        }

        fatherChain.Reverse();
        var allJudgeLines = fatherChain.Select(line => line.Clone()).ToList();
        for (var index = 0; index < allJudgeLines.Count; index++)
            allJudgeLines[index].Father = index - 1;

        return UnbindByIndex(
            allJudgeLines.Count - 1,
            allJudgeLines,
            options,
            progress,
            cancellationToken
        );
    }

    #endregion

    private JudgeLineUnbinder ValidateInput(
        int targetJudgeLineIndex,
        List<Ir.JudgeLine> allJudgeLines,
        double precision,
        double? tolerance,
        double? mergeTolerance = null
    )
    {
        ArgumentNullException.ThrowIfNull(allJudgeLines);
        if (targetJudgeLineIndex < 0 || targetJudgeLineIndex >= allJudgeLines.Count)
            throw new ArgumentOutOfRangeException(nameof(targetJudgeLineIndex));
        NumericParameterValidator.ValidatePrecision(precision);
        if (tolerance is not null)
            NumericParameterValidator.ValidateTolerance(tolerance.Value);
        if (mergeTolerance is not null)
            NumericParameterValidator.ValidateTolerance(mergeTolerance.Value);
        IrChartValidator.ValidateJudgeLineHierarchy(allJudgeLines);
        return this;
    }
}
