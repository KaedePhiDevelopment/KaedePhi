using KaedePhi.Tool.App.Cli.Infrastructure;

namespace KaedePhi.Tool.App.Cli.Commands;

// 渲染工具已被彻底移除，保留隐藏命令仅用于提示使用旧命令的用户
[CliCommand("render-event", Aliases = ["render"])]
public static partial class RenderCommand
{
    [CliHandler]
    private static int Handle(ParseResult _)
    {
        ConsoleWriter.Error(CliLocalizationString.render_removed_notice);
        return 1;
    }
}
