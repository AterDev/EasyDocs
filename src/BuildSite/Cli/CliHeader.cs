using System.Reflection;
using Share;
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
        var logo = new FigletText("EasyDocs")
        {
            Color = Color.Purple,
            Justification = Justify.Center
        };

        AnsiConsole.Write(new Panel(logo)
        {
            Border = BoxBorder.Rounded,
            Padding = new Padding(1, 0)
        });

        AnsiConsole.MarkupLine(
            $"[yellow]—→ {Markup.Escape(Language.Get("tagline"))} 🗽 ←—[/] [grey]v{Markup.Escape(Version)}[/]");
        AnsiConsole.MarkupLine(
            $"[blue][[{Markup.Escape(Language.Get("docsLabel"))}]][/] : [underline blue]{Markup.Escape(DocsUrl)}[/]");
        AnsiConsole.MarkupLine(
            $"[blue][[{Markup.Escape(Language.Get("githubLabel"))}]][/] : [underline blue]{Markup.Escape(GitHubUrl)}[/]");
        AnsiConsole.WriteLine();
    }
}
