using KaedePhi.Tool.Common;

namespace KaedePhi.Tool.Converter;

internal static class ChartExportProgressContext
{
    private static readonly AsyncLocal<ProgressScope?> CurrentScope = new();

    public static IDisposable Use(IProgress<ToolProgress>? progress, double start, double end)
    {
        var scope = new ProgressScope(CurrentScope.Value, progress, start, end);
        CurrentScope.Value = scope;
        return scope;
    }

    public static void Report(int completed, int total)
    {
        var scope = CurrentScope.Value;
        if (scope?.Progress is null || total <= 0)
            return;

        var ratio = Math.Clamp((double)completed / total, 0, 1);
        scope.Progress.Report(new ToolProgress(scope.Start + ratio * (scope.End - scope.Start)));
    }

    private sealed class ProgressScope : IDisposable
    {
        private readonly ProgressScope? _previous;

        public ProgressScope(
            ProgressScope? previous,
            IProgress<ToolProgress>? progress,
            double start,
            double end
        )
        {
            _previous = previous;
            Progress = progress;
            Start = start;
            End = end;
        }

        public IProgress<ToolProgress>? Progress { get; }

        public double Start { get; }

        public double End { get; }

        public void Dispose() => CurrentScope.Value = _previous;
    }
}
