using System.Reflection;

namespace Share;
internal class TemplateHelper
{
    /// <summary>
    /// 读取模板
    /// </summary>
    /// <param name="tplName"></param>
    /// <returns></returns>
    public static string GetTplContent(string tplName)
    {
        return GetResourceContent(tplName + ".tpl");
    }

    public static string GetResourceContent(string resourceName)
    {
        var resourcePath = "Share.template." + resourceName;
        Assembly assembly = Assembly.GetExecutingAssembly();
        using Stream? stream = assembly.GetManifestResourceStream(resourcePath);
        if (stream == null)
        {
            Console.WriteLine("  ❌ can't find tpl file:" + resourcePath);
            return "";
        }
        using StreamReader reader = new(stream);
        return reader.ReadToEnd();
    }


    public static Stream? GetZipFileStream(string fileName)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var stream = assembly.GetManifestResourceStream("Share.template." + fileName);

        if (stream == null)
        {
            Console.WriteLine("  ❌ can't find tpl file:" + fileName);
        }
        return stream;
    }
}
