using System.ComponentModel;
using Share;
using Spectre.Console.Cli;

namespace BuildSite.Cli;

internal sealed class BuildSettings : CommandSettings
{
    [CommandArgument(0, "[configPath]")]
    [Description("Path to the webinfo.json configuration file")]
    public string? ConfigPath { get; init; }
}

internal sealed class BuildCommand(Localizer localizer) : Command<BuildSettings>
{
    protected override int Execute(CommandContext context, BuildSettings settings, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(settings.ConfigPath))
        {
            Share.Command.LogError(localizer.Get(Localizer.BuildRequired));
            return 1;
        }

        Share.Command.Build(settings.ConfigPath, localizer);
        return 0;
    }
}
