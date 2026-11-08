namespace KaedePhi.Tool.App.Shared;

public static class ChartFiles
{
    private const int BufferSize = 65536;

    // 以顺序异步读取方式打开谱面文件，允许其他进程同时读取
    public static FileStream OpenRead(string path) =>
        new(path, FileMode.Open, FileAccess.Read, FileShare.Read, BufferSize, useAsync: true);

    public static bool IsSamePath(string? first, string? second) =>
        !string.IsNullOrWhiteSpace(first)
        && !string.IsNullOrWhiteSpace(second)
        && string.Equals(
            Path.GetFullPath(first),
            Path.GetFullPath(second),
            StringComparison.OrdinalIgnoreCase
        );
}
