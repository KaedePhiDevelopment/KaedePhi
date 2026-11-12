using KaedePhi.Tool.Common;

namespace KaedePhi.Tool.JudgeLine.Utils;

/// <summary>
/// 保存当前解绑调用使用的渲染坐标配置。
/// </summary>
internal static class JudgeLineUnbindContext
{
    private static readonly AsyncLocal<CoordinateProfile?> RenderProfileContext = new();

    /// <summary>
    /// 获取当前解绑调用使用的渲染坐标配置。
    /// </summary>
    internal static CoordinateProfile CurrentRenderProfile =>
        RenderProfileContext.Value ?? CoordinateProfile.DefaultRenderProfile;

    /// <summary>
    /// 在作用域内使用指定的渲染坐标配置。
    /// </summary>
    /// <param name="renderProfile">用于计算的渲染坐标配置。</param>
    /// <returns>用于恢复先前配置的作用域。</returns>
    internal static IDisposable UseRenderProfile(CoordinateProfile renderProfile) =>
        new RenderProfileScope(renderProfile);

    private sealed class RenderProfileScope : IDisposable
    {
        private readonly CoordinateProfile? _previous;

        public RenderProfileScope(CoordinateProfile nextProfile)
        {
            _previous = RenderProfileContext.Value;
            RenderProfileContext.Value = nextProfile;
        }

        public void Dispose() => RenderProfileContext.Value = _previous;
    }
}
