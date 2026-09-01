using System.Diagnostics;
using System.Text;
using System.Text.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Models;
using Share;

namespace Share.Tests;

[TestClass]
public class ProductGenerationTests
{
    private string _root = string.Empty;
    private string _content = string.Empty;
    private string _output = string.Empty;

    [TestInitialize]
    public void Initialize()
    {
        _root = Path.Combine(Path.GetTempPath(), "EasyDocsTests", Guid.NewGuid().ToString("N"));
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
    public void Build_GeneratesProductPagesDataNavigationAndRawAssets()
    {
        var configPath = CreateFixture();

        Command.Build(configPath);

        var productPage = Path.Combine(_output, "products", "MyProduct", "en-us", "Getting Started.html");
        var productSearch = Path.Combine(_output, "products", "MyProduct", "en-us", "search.html");
        var productLanding = Path.Combine(_output, "products", "MyProduct.html");
        var productData = Path.Combine(_output, "data", "products", "MyProduct", "en-us.json");
        var searchData = Path.Combine(_output, "data", "products", "MyProduct", "en-us-search.json");
        var rawPolicy = Path.Combine(_output, "products", "MyProduct", "privacy-policy.html");

        Assert.IsTrue(File.Exists(productPage));
        Assert.IsTrue(File.Exists(productSearch));
        Assert.IsTrue(File.Exists(productLanding));
        Assert.IsTrue(File.Exists(productData));
        Assert.IsTrue(File.Exists(searchData));
        Assert.IsTrue(File.Exists(rawPolicy));

        var page = File.ReadAllText(productPage);
        StringAssert.Contains(page, "Welcome to MyProduct");
        StringAssert.Contains(page, "products.js");
        StringAssert.Contains(page, "data-productName=\"MyProduct\"");
        StringAssert.Contains(page, "/test-site/docs/MyDocs.html");
        StringAssert.Contains(page, "/test-site/products/MyProduct.html");
        var releaseNotesIndex = page.IndexOf("href=\"/test-site/products/MyProduct/en-us/Release Notes.html", StringComparison.Ordinal);
        var guidesIndex = page.IndexOf("href=\"/test-site/products/MyProduct/en-us/Guides/Installation.html", StringComparison.Ordinal);
        var gettingStartedIndex = page.IndexOf("href=\"/test-site/products/MyProduct/en-us/Getting Started.html", StringComparison.Ordinal);
        Assert.IsTrue(releaseNotesIndex >= 0 && guidesIndex >= 0 && gettingStartedIndex >= 0 &&
            releaseNotesIndex < guidesIndex && guidesIndex < gettingStartedIndex,
            $"Release index={releaseNotesIndex}, Guides index={guidesIndex}, Getting Started index={gettingStartedIndex}");

        var index = File.ReadAllText(Path.Combine(_output, "index.html"));
        StringAssert.Contains(index, ">Docs<");
        StringAssert.Contains(index, ">Products<");
        var productsButtonStart = index.IndexOf(">Products", StringComparison.Ordinal);
        var productsButtonEnd = index.IndexOf("</button>", productsButtonStart, StringComparison.Ordinal);
        Assert.IsTrue(productsButtonStart >= 0 && productsButtonEnd > productsButtonStart);
        StringAssert.Contains(index[productsButtonStart..productsButtonEnd], "dropdown-icon");
        StringAssert.Contains(index, "/test-site/products/MyProduct.html");
        StringAssert.Contains(index, "/test-site/products/MyProduct/logo.svg");

        var generatedPolicy = File.ReadAllText(rawPolicy);
        StringAssert.Contains(generatedPolicy, "href=\"/products/MyProduct/en-us/Getting%20Started.html\"");
        StringAssert.Contains(File.ReadAllText(productLanding), "products/MyProduct/en-us/Release Notes.html");

        var sitemap = File.ReadAllText(Path.Combine(_output, "sitemap.xml"));
        StringAssert.Contains(sitemap, "https://example.test/test-site/products/MyProduct.html");
        StringAssert.Contains(sitemap, "https://example.test/test-site/products/MyProduct/en-us/Getting Started.html");

        var generatedWebInfo = File.ReadAllText(Path.Combine(_output, "data", "webinfo.json"));
        StringAssert.Contains(generatedWebInfo, "\"ProductInfos\"");
        StringAssert.Contains(generatedWebInfo, "\"DefaultLanguage\": \"en-us\"");
    }

    [TestMethod]
    public void Build_ResolvesUppercaseAboutToLowercaseOutputAndUrl()
    {
        var configPath = CreateFixture(useUppercaseAbout: true);

        Command.Build(configPath);

        Assert.IsTrue(File.Exists(Path.Combine(_output, "about.html")));
        Assert.IsFalse(Directory.GetFiles(_output, "*", SearchOption.TopDirectoryOnly)
            .Any(path => Path.GetFileName(path) == "About.html"));
        var index = File.ReadAllText(Path.Combine(_output, "index.html"));
        StringAssert.Contains(index, "/test-site/about.html");
    }

    [TestMethod]
    public void Build_UsesRootBaseHrefForDocsAndProducts()
    {
        var configPath = CreateFixture(baseHref: "/");

        Command.Build(configPath);

        var index = File.ReadAllText(Path.Combine(_output, "index.html"));
        StringAssert.Contains(index, "href=\"/products/MyProduct.html\"");
        Assert.IsFalse(index.Contains("/test-site/", StringComparison.Ordinal));

        var productPage = File.ReadAllText(Path.Combine(_output, "products", "MyProduct", "en-us", "Getting Started.html"));
        StringAssert.Contains(productPage, "href=\"/css/app.css\"");
        StringAssert.Contains(productPage, "href=\"/docs/MyDocs.html\"");
    }

    [TestMethod]
    public void Build_DisablesBlogPipelineAndBlogNavigation()
    {
        var configPath = CreateFixture(enableBlog: false);
        Directory.CreateDirectory(Path.Combine(_output, "blogs"));
        Write(Path.Combine(_output, "blogs", "Old.html"), "old blog");
        Write(Path.Combine(_output, "data", "blogs.json"), "old blog data");
        Write(Path.Combine(_output, "blogs.html"), "old blog list");
        Write(Path.Combine(_output, "sitemap.xml"), "old sitemap");

        Command.Build(configPath);

        Assert.IsFalse(File.Exists(Path.Combine(_output, "blogs.html")));
        Assert.IsFalse(File.Exists(Path.Combine(_output, "blogs", "Welcome.html")));
        Assert.IsFalse(File.Exists(Path.Combine(_output, "data", "blogs.json")));

        var index = File.ReadAllText(Path.Combine(_output, "index.html"));
        Assert.IsFalse(index.Contains("New Blogs", StringComparison.Ordinal));
        Assert.IsFalse(index.Contains("blogs.html", StringComparison.OrdinalIgnoreCase));

        var docsPage = File.ReadAllText(Path.Combine(_output, "docs", "MyDocs", "en-us", "1.0", "README.html"));
        Assert.IsFalse(docsPage.Contains("blogs.html", StringComparison.OrdinalIgnoreCase));

        var sitemap = File.ReadAllText(Path.Combine(_output, "sitemap.xml"));
        Assert.IsFalse(sitemap.Contains("/blogs/", StringComparison.OrdinalIgnoreCase));
        StringAssert.Contains(sitemap, "/products/MyProduct.html");
    }

    [TestMethod]
    public void Build_UsesGitAuthorInBlogAndDocDetails()
    {
        var configPath = CreateFixture();
        RunGit(_root, "init");
        RunGit(_root, "config", "user.name", "Commit Author");
        RunGit(_root, "config", "user.email", "author@example.test");
        RunGit(_root, "add", ".");
        RunGit(_root, "commit", "-m", "Add test content");

        Command.Build(configPath);

        var blogPage = File.ReadAllText(Path.Combine(_output, "blogs", "Welcome.html"));
        StringAssert.Contains(blogPage, "class=\"site-nav\"");
        StringAssert.Contains(blogPage, "href=\"/test-site/docs/MyDocs.html\"");
        StringAssert.Contains(blogPage, "👨‍💻 Commit Author");
        StringAssert.Contains(blogPage, "📆");

        var docsPage = File.ReadAllText(Path.Combine(_output, "docs", "MyDocs", "en-us", "1.0", "README.html"));
        StringAssert.Contains(docsPage, "👨‍💻 Commit Author");
        StringAssert.Contains(docsPage, "📆");

        var blogData = File.ReadAllText(Path.Combine(_output, "data", "blogs.json"));
        StringAssert.Contains(blogData, "\"AuthorName\": \"Commit Author\"");
    }

    [TestMethod]
    public void Build_RejectsBothAboutFilenameVariants()
    {
        if (OperatingSystem.IsWindows())
        {
            Assert.Inconclusive("Windows cannot represent both about.md and About.md as separate files.");
        }

        var configPath = CreateFixture();
        Write(Path.Combine(_content, "About.md"), "# Duplicate About");

        Assert.ThrowsException<InvalidOperationException>(() => Command.Build(configPath));
    }

    private string CreateFixture(bool useUppercaseAbout = false, string baseHref = "/test-site/", bool enableBlog = true)
    {
        Directory.CreateDirectory(Path.Combine(_content, "blogs"));
        Directory.CreateDirectory(Path.Combine(_content, "docs", "MyDocs", "en-us", "1.0"));
        Directory.CreateDirectory(Path.Combine(_content, "products", "MyProduct", "en-us"));
        Directory.CreateDirectory(Path.Combine(_content, "products", "MyProduct", "zh-cn"));

        Write(Path.Combine(_content, "blogs", "Welcome.md"), "# Welcome\n\n## Blog\n\nHello.");
        Write(Path.Combine(_content, useUppercaseAbout ? "About.md" : "about.md"), "# About EasyDocs");
        Write(Path.Combine(_content, "docs", "MyDocs", "en-us", "1.0", "README.md"), "# Docs\n\n## Documentation\n\nDocs.");

        Write(Path.Combine(_content, "products", "MyProduct", "en-us", ".order"), "Release Notes\nGuides\nGetting Started\n");
        Write(Path.Combine(_content, "products", "MyProduct", "zh-cn", ".order"), "Getting Started\n");
        Write(Path.Combine(_content, "products", "MyProduct", "en-us", "Getting Started.md"),
            "# Welcome to MyProduct\n\n## Introduction\n\nEnglish content.");
        Write(Path.Combine(_content, "products", "MyProduct", "en-us", "Release Notes.md"),
            "# Release Notes\n\n## Changes\n\nRelease content.");
        Write(Path.Combine(_content, "products", "MyProduct", "en-us", "Guides", "Installation.md"),
            "# Installation\n\n## Setup\n\nInstallation content.");
        Write(Path.Combine(_content, "products", "MyProduct", "zh-cn", "Getting Started.md"),
            "# 欢迎使用 MyProduct\n\n## 介绍\n\n中文内容。");
        Write(Path.Combine(_content, "products", "MyProduct", "logo.svg"), "<svg xmlns=\"http://www.w3.org/2000/svg\"></svg>");
        Write(Path.Combine(_content, "products", "MyProduct", "privacy-policy.html"),
            "<!doctype html><title>Privacy Policy</title><a href=\"/products/MyProduct/en-us/Getting%20Started.html\">Product</a>");

        var webInfo = new WebInfo
        {
            Name = "EasyDocs Test",
            Description = "Test site",
            AuthorName = "Test",
            EnableBlog = enableBlog,
            ContetPath = _content,
            OutputPath = _output,
            BaseHref = baseHref,
            Domain = "https://example.test",
            DocInfos =
            [
                new()
                {
                    Name = "MyDocs",
                    Languages = ["en-us"],
                    Versions = ["1.0"]
                }
            ],
            ProductInfos =
            [
                new()
                {
                    Name = "MyProduct",
                    Logo = "logo.svg",
                    Description = "Test product",
                    Languages = ["en-us", "zh-cn"],
                    DefaultLanguage = "en-us"
                }
            ]
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

    private static void RunGit(string workingDirectory, params string[] arguments)
    {
        using var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "git",
                WorkingDirectory = workingDirectory,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            }
        };

        foreach (var argument in arguments)
        {
            process.StartInfo.ArgumentList.Add(argument);
        }

        Assert.IsTrue(process.Start());
        process.WaitForExit();
        var error = process.StandardError.ReadToEnd();
        Assert.AreEqual(0, process.ExitCode, $"git {string.Join(' ', arguments)} failed: {error}");
    }
}
