using System.ComponentModel;
using Share;
using Spectre.Console.Cli;

namespace BuildSite.Cli;

internal sealed class InitSettings : CommandSettings
{
    [CommandArgument(0, "[path]")]
    [Description("Working directory path")]
    public string? DirectoryPath { get; init; }
}

internal sealed class InitCommand(Localizer localizer) : Command<InitSettings>
{
    protected override int Execute(CommandContext context, InitSettings settings, CancellationToken cancellationToken)
    {
        Share.Command.Init(settings.DirectoryPath ?? Directory.GetCurrentDirectory(), localizer);
        return 0;
    }
}
