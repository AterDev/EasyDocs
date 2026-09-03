using System.Text;
using System.Text.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Models;
using Share;

namespace Share.Tests;

[TestClass]
public class DocsGenerationTests
{
    private string _root = string.Empty;
    private string _content = string.Empty;
    private string _output = string.Empty;

    [TestInitialize]
    public void Initialize()
    {
        _root = Path.Combine(Path.GetTempPath(), "EasyDocsDocsTests", Guid.NewGuid().ToString("N"));
        _content = Path.Combine(_root, "Content");
        _output = Path.Combine(_root, "WebSite");
        Directory.CreateDirectory(_content);
    }

    [TestCleanup]
    public void Cleanup()
    {
        if (Directory.Exists(_root))
        {
            foreach (var file in Directory.EnumerateFiles(_root, "*", SearchOption.AllDirectories))
            {
                File.SetAttributes(file, FileAttributes.Normal);
            }

            foreach (var directory in Directory.EnumerateDirectories(_root, "*", SearchOption.AllDirectories))
            {
                new DirectoryInfo(directory).Attributes = FileAttributes.Normal;
            }

            Directory.Delete(_root, true);
        }
    }

    [TestMethod]
    public void Build_RewritesHomepageImagesRelativeToSourceMarkdown()
    {
        var configPath = CreateFixture();

        Command.Build(configPath);

        var homepage = File.ReadAllText(Path.Combine(_output, "docs", "MyDocs.html"));
        StringAssert.Contains(homepage, "src=\"./MyDocs/en-us/1.0/assets/root.svg?version=1#diagram\"");
        StringAssert.Contains(homepage, "src=\"./MyDocs/en-us/shared.svg\"");
        StringAssert.Contains(homepage, "src='./MyDocs/en-us/1.0/assets/root.svg'");
        StringAssert.Contains(homepage, "src=\"https://example.com/remote.svg\"");

        var document = File.ReadAllText(Path.Combine(
            _output,
            "docs",
            "MyDocs",
            "en-us",
            "1.0",
            "Guides",
            "Guide.html"));
        StringAssert.Contains(document, "src=\"../assets/root.svg?version=1#diagram\"");
        StringAssert.Contains(document, "src=\"../../shared.svg\"");
        StringAssert.Contains(document, "title=\"Edit\"");
        Assert.IsFalse(document.Contains("Edit on GitHub", StringComparison.Ordinal));

        Assert.IsTrue(File.Exists(Path.Combine(_output, "docs", "MyDocs", "en-us", "1.0", "assets", "root.svg")));
        Assert.IsTrue(File.Exists(Path.Combine(_output, "docs", "MyDocs", "en-us", "shared.svg")));
    }

    [TestMethod]
    public void Build_RendersBlockquoteWithPackagedDarkThemeStyles()
    {
        var configPath = CreateFixture();

        Command.Build(configPath);

        var document = File.ReadAllText(Path.Combine(
            _output,
            "docs",
            "MyDocs",
            "en-us",
            "1.0",
            "Guides",
            "Guide.html"));
        StringAssert.Contains(document, "<blockquote>");
        StringAssert.Contains(document, "<p>A quoted paragraph.</p>");
        StringAssert.Contains(document, "<p>Another quoted paragraph.</p>");

        var markdownCss = File.ReadAllText(Path.Combine(_output, "css", "markdown.css"));
        StringAssert.Contains(markdownCss, ".markdown-content blockquote");
        StringAssert.Contains(markdownCss, ".markdown-content blockquote > p");
        StringAssert.Contains(markdownCss, "background-color: var(--color-surface);");
        StringAssert.Contains(markdownCss, "border-left: 4px solid var(--color-border);");

        var appCss = File.ReadAllText(Path.Combine(_output, "css", "app.css"));
        StringAssert.Contains(appCss, "--color-bg: #131313;");
    }

    private string CreateFixture()
    {
        var versionPath = Path.Combine(_content, "docs", "MyDocs", "en-us", "1.0");
        var guidePath = Path.Combine(versionPath, "Guides");
        Directory.CreateDirectory(guidePath);

        Write(Path.Combine(versionPath, ".order"), "Guides\n");
        Write(Path.Combine(guidePath, "Guide.md"), """
            # Guide

            > A quoted paragraph.
            >
            > Another quoted paragraph.

            ![Root asset](../assets/root.svg?version=1#diagram)
            ![Parent asset](../../shared.svg)
            <img src='../assets/root.svg' alt='Raw HTML asset'>
            ![Remote asset](https://example.com/remote.svg)
            """);
        Write(Path.Combine(versionPath, "assets", "root.svg"), "<svg xmlns=\"http://www.w3.org/2000/svg\"></svg>");
        Write(Path.Combine(_content, "docs", "MyDocs", "en-us", "shared.svg"), "<svg xmlns=\"http://www.w3.org/2000/svg\"></svg>");

        var webInfo = new WebInfo
        {
            Name = "EasyDocs Test",
            Description = "Test site",
            AuthorName = "Test",
            EnableBlog = false,
            ContetPath = _content,
            OutputPath = _output,
            BaseHref = "/test-site/",
            DocInfos =
            [
                new()
                {
                    Name = "MyDocs",
                    Languages = ["en-us"],
                    Versions = ["1.0"]
                }
            ],
            ProductInfos = []
        };

        var configPath = Path.Combine(_root, "webinfo.json");
        Write(configPath, JsonSerializer.Serialize(webInfo, new JsonSerializerOptions { WriteIndented = true }));
        return configPath;
    }

    private static void Write(string path, string content)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, content, new UTF8Encoding(false));
    }
}
