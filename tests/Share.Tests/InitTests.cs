using System.Text;
using System.Text.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Models;
using Share;

namespace Share.Tests;

[TestClass]
public class InitTests
{
    private string _root = string.Empty;

    [TestInitialize]
    public void Initialize()
    {
        _root = Path.Combine(Path.GetTempPath(), "EasyDocsInitTests", Guid.NewGuid().ToString("N"));
    }

    [TestCleanup]
    public void Cleanup()
    {
        if (Directory.Exists(_root))
        {
            Directory.Delete(_root, true);
        }
    }

    [TestMethod]
    public void Init_CreatesPreviewScriptAndPreservesExistingScript()
    {
        Command.Init(_root);

        var previewPath = Path.Combine(_root, "preview.cs");
        Assert.IsTrue(File.Exists(previewPath));
        var webInfo = JsonSerializer.Deserialize<WebInfo>(File.ReadAllText(Path.Combine(_root, "webinfo.json")));
        Assert.IsNotNull(webInfo);
        Assert.IsTrue(webInfo!.EnableBlog);

        var preview = File.ReadAllText(previewPath);
        StringAssert.Contains(preview, "#:sdk Microsoft.NET.Sdk.Web");
        StringAssert.Contains(preview, "app.MapFallbackToFile(\"index.html\")");

        File.WriteAllText(previewPath, "custom preview", new UTF8Encoding(false));
        Command.Init(_root);

        Assert.AreEqual("custom preview", File.ReadAllText(previewPath));
    }
}
