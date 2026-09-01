using System.Globalization;
using System.Text;
using Share.Builders;
using Spectre.Console;

namespace Share;

public class Command
{
    public static string WebConfigFileName = "webinfo.json";
    public static string PreviewFileName = "preview.cs";
    public readonly static JsonSerializerOptions JsonSerializerOptions = new()
    {
        WriteIndented = true
    };

    public static void Init(string path, Localizer? localizer = null)
    {
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }

        var filePath = Path.Combine(path, WebConfigFileName);
        var webInfo = new WebInfo
        {
            DocInfos = [new() { Name = "example" }]
        };

        if (!File.Exists(filePath))
        {
            var json = JsonSerializer.Serialize(webInfo, JsonSerializerOptions);
            File.WriteAllText(filePath, json);
            LogSuccess((localizer?.Get(Localizer.InitSuccess) ?? "Initialized webinfo.json successfully: ") + filePath);
        }
        else
        {
            LogWarning((localizer?.Get(Localizer.ConfigExists) ?? "Configuration file already exists, skipping: ") + filePath);
        }

        string[] dirs = ["blogs", "docs/example/zh-cn/1.0", "docs/example/en-us/1.0"];
        foreach (var dir in dirs)
        {
            var dirPath = Path.Combine(path, "Content", dir);
            if (!Directory.Exists(dirPath))
            {
                Directory.CreateDirectory(dirPath);
            }
        }

        var aboutMeFile = Path.Combine(path, "Content", "about.md");
        if (!File.Exists(aboutMeFile))
        {
            File.WriteAllText(aboutMeFile, "# About Me");
        }

        var previewFile = Path.Combine(path, PreviewFileName);
        if (!File.Exists(previewFile))
        {
            File.WriteAllText(previewFile, TemplateHelper.GetResourceContent(PreviewFileName), new UTF8Encoding(false));
            LogSuccess((localizer?.Get(Localizer.PreviewSuccess) ?? "Initialized preview.cs successfully: ") + previewFile);
        }
        else
        {
            LogWarning((localizer?.Get(Localizer.PreviewExists) ?? "Preview script already exists, skipping: ") + previewFile);
        }

        LogInfo(localizer?.Get(Localizer.InitNextSteps) ??
            "Update webinfo.json before running build. To preview the generated site, run: dotnet run preview.cs -- .\\WebSite. See the official documentation: https://dusi.dev/docs/EasyDocs.html");
    }

    public static void Build(string configPath, Localizer? localizer = null)
    {
        var webInfoPath = Path.Combine(configPath);
        var webInfo = new WebInfo();
        if (File.Exists(webInfoPath))
        {
            var json = File.ReadAllText(webInfoPath);
            webInfo = JsonSerializer.Deserialize<WebInfo>(json);
        }
        else
        {
            LogInfo(localizer?.Get(Localizer.NotExistWebInfo) ?? "Configuration file not found; using default configuration.");
        }

        BaseBuilder.ResetMenus();

        var productBuilder = new ProductBuilder(webInfo!);
        productBuilder.EnableBaseUrl();
        productBuilder.DiscoverProducts();

        var docBuilder = new DocsBuilder(webInfo!);
        docBuilder.EnableBaseUrl();
        docBuilder.BuildDocs();

        productBuilder.BuildProducts();

        var builder = new HtmlBuilder(webInfo!)
        {
            AdditionalSitemapEntries = productBuilder.SitemapEntries
        };
        builder.EnableBaseUrl();
        builder.BuildWebSite();
    }

    public static void LogInfo(string msg)
    {
        var now = DateTime.Now.ToString("HH:mm:ss", CultureInfo.InvariantCulture);
        AnsiConsole.MarkupLine($"[grey]ℹ[/] [dim]{now}[/] {Markup.Escape(msg)}");
    }

    public static void LogWarning(string msg)
    {
        AnsiConsole.MarkupLine($"[yellow]⚠[/] {Markup.Escape(msg)}");
    }

    public static void LogError(string msg)
    {
        AnsiConsole.MarkupLine($"[red]✖[/] {Markup.Escape(msg)}");
    }

    public static void LogSuccess(string msg)
    {
        var now = DateTime.Now.ToString("HH:mm:ss", CultureInfo.InvariantCulture);
        AnsiConsole.MarkupLine($"[green]✔[/] [dim]{now}[/] {Markup.Escape(msg)}");
    }
}
