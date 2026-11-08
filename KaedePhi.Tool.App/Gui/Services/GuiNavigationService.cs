using KaedePhi.Tool.App.Config;
using KaedePhi.Tool.App.Gui.Models;
using KaedePhi.Tool.App.Gui.ViewModels;
using Serilog;
using static KaedePhi.Tool.Localization.GuiLocalizationString;

namespace KaedePhi.Tool.App.Gui.Services;

internal sealed class GuiNavigationService
{
    private readonly MainViewModel _main;
    private readonly GuiChartService _chart;
    private readonly AppConfigService _config;
    private readonly ILogger _log;

    public ImportViewModel Import { get; } = new();
    public ImportOptionsViewModel ImportOptions { get; } = new();
    public ToolViewModel Tool { get; } = new();
    public ExportViewModel Export { get; } = new();
    public ProcessingViewModel Processing { get; } = new();
    public SettingsViewModel Settings { get; }

    public GuiNavigationService(
        MainViewModel main,
        GuiChartService chart,
        LogService logService,
        AppConfigService config
    )
    {
        _main = main;
        _chart = chart;
        _config = config;
        _log = logService.ForContext<GuiNavigationService>();
        Settings = new SettingsViewModel(config);
        Settings.RequestReturnToTools += ReturnFromSettings;
    }

    public void ShowImport()
    {
        _chart.Clear();
        _main.ClearChartContext();
        Import.UseStream = _config.Config.Convert.StreamImport;
        _main.SetShellState(ShellPageKey.Import, import_button, app_subtitle, 1);
        _main.CurrentPage = Import;
        _log.Information(log_navigate_import);
    }

    public void ShowImportOptions(string formatId, string fileName)
    {
        ImportOptions.DetectedFormat = formatId;
        ImportOptions.FileName = fileName;
        _main.SetShellState(
            ShellPageKey.ImportOptions,
            import_options_title,
            import_options_title,
            1,
            fileName,
            formatId,
            false
        );
        _main.CurrentPage = ImportOptions;
    }

    public void ShowTool()
    {
        Tool.StatusText = string.Empty;
        Tool.HasUnclearContent = _chart.HasUnclearContent;
        _main.SetShellState(ShellPageKey.Tool, tool_select_title, tool_options_title, 2);
        _main.CurrentPage = Tool;
    }

    public void ShowToolForChart(string filePath, string formatId)
    {
        var fileName = Path.GetFileName(filePath);
        Tool.CurrentFileName = fileName;
        Tool.DetectedFormat = formatId;
        _main.SetShellState(
            ShellPageKey.Tool,
            tool_select_title,
            tool_options_title,
            2,
            fileName,
            formatId,
            true
        );
        Tool.RenderOutputDir = Path.Combine(
            Path.GetDirectoryName(filePath) ?? string.Empty,
            "RenderLayer"
        );
        Tool.SelectedTool = null;
        ShowTool();
    }

    public void ShowExport()
    {
        Export.Prepare(Tool.DetectedFormat, _config.Config.Convert);
        _main.SetShellState(ShellPageKey.Export, export_title, export_output_format, 3);
        _main.CurrentPage = Export;
    }

    public void ShowProcessing()
    {
        Processing.Reset();
        _main.SetShellState(ShellPageKey.Processing, processing_title, status_processing, 2);
        _main.CurrentPage = Processing;
    }

    public void ShowSettings()
    {
        Settings.StatusText = string.Empty;
        _main.SetShellState(ShellPageKey.Settings, settings_title, settings_title, 0);
        _main.CurrentPage = Settings;
    }

    public void ReturnFromSettings()
    {
        Tool.ApplyConfigDefaults(_config.Config);
        if (_chart.IsLoaded)
            ShowTool();
        else
            ShowImport();
    }
}
