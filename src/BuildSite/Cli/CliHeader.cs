using System.Reflection;
using Spectre.Console;

namespace BuildSite.Cli;

internal static class CliHeader
{
    public const string DocsUrl = "https://dusi.dev/docs/EasyDocs.html";
    public const string GitHubUrl = "https://github.com/AterDev/EasyDocs";

    public static string Version =>
        Assembly.GetEntryAssembly()?.GetName().Version?.ToString(3) ?? "unknown";

    public static void Write()
    {
        // Keep the logo and its wording fixed. The command descriptions are localized,
        // but the branding should remain consistent across system languages.
        const string logo = """
            ███████┐ ███████┐ ██████┐   ██████┐  ██████┐
            ██┌────┘ └──███┌┘ ██┌──██┐ ██┌───██┐ ██┌───┘
            █████┐     ███┌┘  ██│  ██│ ██│   ██│ ██│
            ██┌──┘    ███┌┘   ██│  ██│ ██│   ██│ ██│
            ███████┐ ███████┐ ██████┌┘ └██████┌┘ ██████┐
            └──────┘ └──────┘ └─────┘   └─────┘  └─────┘
            """;
        var sign = $"🗽 for freedom.                       {Version}";

        AnsiConsole.Write(
            new Panel(
                new Rows(
                    new Markup($"[bold purple]{Markup.Escape(logo.Trim())}[/]"),
                    new Markup($"[yellow]{Markup.Escape(sign)}[/]")))
                .Border(BoxBorder.Rounded)
                .BorderColor(Color.Grey));

        AnsiConsole.MarkupLine($"[blue][[docs]]  : [link]{Markup.Escape(DocsUrl)}[/][/]");
        AnsiConsole.MarkupLine($"[blue][[GitHub]]: [link]{Markup.Escape(GitHubUrl)}[/][/]");
        AnsiConsole.WriteLine();
    }
}
