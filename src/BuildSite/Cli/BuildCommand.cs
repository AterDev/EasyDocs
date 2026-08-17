using System.ComponentModel;
using Share;
using Spectre.Console.Cli;

namespace BuildSite.Cli;

internal sealed class BuildSettings : CommandSettings
{
    [CommandArgument(0, "[configPath]")]
    [Description("Path to webinfo.json / webinfo.json 配置文件路径")]
    public string? ConfigPath { get; init; }
}

internal sealed class BuildCommand : Command<BuildSettings>
{
    protected override int Execute(CommandContext context, BuildSettings settings, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(settings.ConfigPath))
        {
            Share.Command.LogError(Language.Get("buildRequired"));
            return 1;
        }

        Share.Command.Build(settings.ConfigPath);
        return 0;
    }
}
