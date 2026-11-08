namespace KaedePhi.Tool.App.Cli.Infrastructure;

public static class SharedOptions
{
    public static Option<string?> CreateInputRpeOption() =>
        new("--input", "-i")
        {
            Description = CliLocalizationString.cli_opt_input_rpe_desc,
            Arity = ArgumentArity.ZeroOrOne,
        };

    public static Option<string?> CreateInputPhieditOption() =>
        new("--input", "-i")
        {
            Description = CliLocalizationString.cli_opt_input_phiedit_desc,
            Arity = ArgumentArity.ZeroOrOne,
        };

    public static Option<string?> CreateOutputAutoOption() =>
        new("--output", "-o")
        {
            Description = CliLocalizationString.cli_opt_output_auto_desc,
            Arity = ArgumentArity.ZeroOrOne,
        };

    public static Option<string?> CreateOutputPathOption() =>
        new("--output", "-o")
        {
            Description = CliLocalizationString.cli_opt_output_path_desc,
            Arity = ArgumentArity.ZeroOrOne,
        };

    public static Option<string?> CreateWorkspaceRpeOption() =>
        new("--workspace", "-w")
        {
            Description = CliLocalizationString.cli_opt_workspace_rpe_desc,
            Arity = ArgumentArity.ZeroOrOne,
        };

    public static Option<string?> CreateWorkspaceDefaultOption() =>
        new("--workspace", "-w")
        {
            Description = CliLocalizationString.cli_opt_workspace_default_desc,
            Arity = ArgumentArity.ZeroOrOne,
        };

    public static Option<double> PrecisionOption { get; } =
        new("--precision", "-p")
        {
            Description = CliLocalizationString.cli_opt_precision_desc,
            Arity = ArgumentArity.ExactlyOne,
        };

    public static Option<double> ToleranceOption { get; } =
        new("--tolerance", "-t")
        {
            Description = CliLocalizationString.cli_opt_tolerance_desc,
            Arity = ArgumentArity.ExactlyOne,
        };

    public static Option<bool> ClassicOption { get; } =
        new("--classic") { Description = CliLocalizationString.cli_opt_classic_mode_desc };

    public static Option<bool> NoCompressOption { get; } =
        new("--no-compress") { Description = CliLocalizationString.cli_opt_compress_desc };

    public static Option<bool> DryRunOption { get; } =
        new("--dry-run") { Description = CliLocalizationString.cli_opt_dry_run_desc };

    public static Option<bool> StreamOutputOption { get; } =
        new("--stream") { Description = CliLocalizationString.cli_opt_stream_output_desc };

    public static Option<bool> FormatOutputOption { get; } =
        new("--format") { Description = CliLocalizationString.cli_opt_format_desc };

    public static T? GetIfSpecified<T>(ParseResult result, Option<T> option)
        where T : struct
    {
        return result.GetResult(option) is not null ? result.GetValue(option) : null;
    }

    // 用户在命令行显式指定了该选项时取其值，否则使用回退值（通常来自配置文件）
    public static T ValueOr<T>(this ParseResult result, Option<T> option, T fallback)
        where T : struct => GetIfSpecified(result, option) ?? fallback;

    public static Option<T> CreateValueOption<T>(string name, string description) =>
        new(name) { Description = description, Arity = ArgumentArity.ExactlyOne };

    public static Option<bool> CreateFlagOption(string name, string description) =>
        new(name) { Description = description };
}
