using static KaedePhi.Tool.Localization.GuiLocalizationString;

namespace KaedePhi.Tool.App.Gui.Models;

public static class ToolCatalog
{
    // 每次调用都重新读取本地化文本，保证文本随界面语言变化
    public static List<ToolOption> Create() =>
        [
            new()
            {
                Name = tool_unbind_name,
                Description = tool_unbind_desc,
                IconGlyph = FontAwesome.Unbind,
                ToolId = ToolIds.Unbind,
                HasPrecision = true,
                HasTolerance = true,
                HasMergeTolerance = true,
                HasClassicMode = true,
                HasDisableCompress = true,
                DefaultTolerance = 1,
                DefaultMergeTolerance = 0.1,
            },
            new()
            {
                Name = tool_layermerge_name,
                Description = tool_layermerge_desc,
                IconGlyph = FontAwesome.Merge,
                ToolId = ToolIds.LayerMerge,
                HasPrecision = true,
                HasTolerance = true,
                HasClassicMode = true,
                HasDisableCompress = true,
            },
            new()
            {
                Name = tool_cut_name,
                Description = tool_cut_desc,
                IconGlyph = FontAwesome.Cut,
                ToolId = ToolIds.Cut,
                HasPrecision = true,
                HasTolerance = true,
                HasDisableCompress = true,
            },
            new()
            {
                Name = tool_fit_name,
                Description = tool_fit_desc,
                IconGlyph = FontAwesome.Fix,
                ToolId = ToolIds.Fit,
                HasTolerance = true,
                HasFitOptions = true,
                DefaultTolerance = 0.5,
            },
        ];
}
