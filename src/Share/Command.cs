using Share.Builders;

namespace Share;

public class Command
{
    public static string WebConfigFileName = "webinfo.json";
    public readonly static JsonSerializerOptions JsonSerializerOptions = new()
    {
        WriteIndented = true
    };

    public static void Init(string path)
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
            LogSuccess(Language.Get("initSuccess") + filePath);
        }
        else
        {
            Console.WriteLine("config file already exist!");
        }
        // 创建目录
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
    }

    public static void Build(string configPath)
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
            LogInfo(Language.Get("notExistWebInfo"));
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
        var now = DateTime.Now.ToString("HH:mm:ss");
        Console.WriteLine($"ℹ️ [{now}] {msg}");
    }

    public static void LogWarning(string msg)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"⚠️ {msg}");
        Console.ResetColor();
    }

    public static void LogError(string msg)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"❌ {msg}");
        Console.ResetColor();
    }
    public static void LogSuccess(string msg)
    {
        var now = DateTime.Now.ToString("HH:mm:ss");
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"✅ [{now}] {msg}");
        Console.ResetColor();
    }
}
