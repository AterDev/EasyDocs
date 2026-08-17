using System.Globalization;

namespace Share;

public class Language
{
    public static Dictionary<string, string> CN { get; set; } = new()
    {
        {"Command", "命令"},
        {"init", $"初始化配置文件{Command.WebConfigFileName};[path]文件路径。"},
        {"build", "生成静态网站；[configPath] 为配置文件路径"},
        {"doc", "生成文档类静态网站；[configPath]为配置文件(json)"},
        {"buildRequired", "参数[configPath]是必需的。"},
        {"initSuccess", $"初始化配置文件[{Command.WebConfigFileName}] 成功!"},
        {"notExistWebInfo", "未找到配置文件，将使用默认配置。"},
        {"configExists", "配置文件已存在，跳过写入: "},
        {"initDescription", "初始化站点配置和内容目录"},
        {"buildDescription", "根据配置文件生成静态站点"},
        {"pathArgument", "工作目录路径"},
        {"configPathArgument", "webinfo.json 配置文件路径"},
        {"tagline", "自由而生"},
        {"docsLabel", "docs"},
        {"githubLabel", "GitHub"}
    };

    public static Dictionary<string, string> EN { get; set; } = new()
    {
        {"Command", "Commands"},
        {"init", $"init config file {Command.WebConfigFileName}; [path] is path."},
        {"build", "generate static website; [configPath] is config file path"},
        {"doc", "generate doc site; [configPath] is json config file"},
        {"buildRequired", "parameter [configPath] is required!"},
        {"initSuccess", $"Init config file [{Command.WebConfigFileName}] success!"},
        {"notExistWebInfo", "config file not found, will use default config!"},
        {"configExists", "Config file already exists, skip writing: "},
        {"initDescription", "Initialize the site configuration and content directories"},
        {"buildDescription", "Build a static site from the configuration file"},
        {"pathArgument", "Working directory path"},
        {"configPathArgument", "Path to the webinfo.json configuration file"},
        {"tagline", "for freedom"},
        {"docsLabel", "docs"},
        {"githubLabel", "GitHub"}
    };

    public static CultureInfo CurrentCulture => CultureInfo.CurrentUICulture;

    public static bool IsChinese =>
        CurrentCulture.TwoLetterISOLanguageName.Equals("zh", StringComparison.OrdinalIgnoreCase);

    public static string Get(string key)
    {
        return Get(key, IsChinese);
    }

    public static string GetBilingual(string key)
    {
        var primary = Get(key);
        var secondary = Get(key, !IsChinese);
        return $"{primary} / {secondary}";
    }

    private static string Get(string key, bool chinese)
    {
        return chinese ? CN[key] : EN[key];
    }
}
