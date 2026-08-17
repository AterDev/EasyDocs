using System.ComponentModel;
using Share;
using Spectre.Console.Cli;

namespace BuildSite.Cli;

internal sealed class InitSettings : CommandSettings
{
    [CommandArgument(0, "[path]")]
    [Description("Working directory path / 工作目录路径")]
    public string? DirectoryPath { get; init; }
}

internal sealed class InitCommand : Command<InitSettings>
{
    protected override int Execute(CommandContext context, InitSettings settings, CancellationToken cancellationToken)
    {
        Share.Command.Init(settings.DirectoryPath ?? Directory.GetCurrentDirectory());
        return 0;
    }
}
