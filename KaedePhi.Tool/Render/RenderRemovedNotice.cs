namespace KaedePhi.Tool.Render;

internal static class RenderRemovedNotice
{
    // 同时用于过时标记与运行期异常，保证调用方在编译期和运行期看到相同的说明
    public const string Message =
        "渲染工具已被彻底移除，不再提供任何功能；相关接口仅为二进制兼容而保留，调用时始终抛出未实现异常。";

    public static NotImplementedException CreateException() => new(Message);
}
