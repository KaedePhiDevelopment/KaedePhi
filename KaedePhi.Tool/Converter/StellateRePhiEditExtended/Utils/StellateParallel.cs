using System.Runtime.ExceptionServices;

namespace KaedePhi.Tool.Converter.StellateRePhiEditExtended.Utils;

internal static class StellateParallel
{
    // 并行任务只有单个失败时直接抛出原始异常，避免调用方收到 AggregateException 包装
    public static void Run(Action parallelWork)
    {
        try
        {
            parallelWork();
        }
        catch (AggregateException exception)
        {
            var failures = exception.Flatten().InnerExceptions;
            if (failures.Count == 1)
                ExceptionDispatchInfo.Capture(failures[0]).Throw();

            throw;
        }
    }
}
