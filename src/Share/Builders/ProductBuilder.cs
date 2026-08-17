using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Unicode;

namespace Share.Builders;

/// <summary>
/// 构建无版本的产品内容。
/// </summary>
public class ProductBuilder(WebInfo webInfo) : BaseBuilder(webInfo)
{
    private readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        ReferenceHandler = ReferenceHandler.IgnoreCycles,
        Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
        WriteIndented = true
    };

    private Catalog? _productsCatalog;
    private string? _gitRoot;
    private string? _repoUrl;
    private string? _branch;

    public List<ProductInfo> ProductInfos { get; } = webInfo.ProductInfos;
    public List<Sitemap> SitemapEntries { get; } = [];

    /// <summary>
    /// 在生成 Docs 之前发现有效产品，使所有页面都能生成完整导航。
    /// </summary>
    public void DiscoverProducts()
    {
        ProductMenus.Clear();
        var productRoot = Path.Combine(ContentPath, "products");
        foreach (var productInfo in ProductInfos)
        {
            var productPath = Path.Combine(productRoot, productInfo.Name);
            var defaultLanguagePath = Path.Combine(productPath, productInfo.DefaultLanguage);
            if (!Directory.Exists(productPath))
            {
                Command.LogWarning($"Not found product: {productPath}");
                continue;
            }

            if (!productInfo.Languages.Contains(productInfo.DefaultLanguage, StringComparer.OrdinalIgnoreCase) ||
                !Directory.Exists(defaultLanguagePath) ||
                !Directory.EnumerateFiles(defaultLanguagePath, "*.md", SearchOption.AllDirectories).Any())
            {
                Command.LogWarning($"Product [{productInfo.Name}] has no valid default language [{productInfo.DefaultLanguage}].");
                continue;
            }

            ProductMenus[productInfo.Name] = productInfo.Name + ".html";
        }
    }

    public void BuildProducts()
    {
        DiscoverProducts();
        SitemapEntries.Clear();

        if (ProductInfos.Count == 0)
        {
            return;
        }

        var productRoot = Path.Combine(ContentPath, "products");
        if (!Directory.Exists(productRoot))
        {
            return;
        }

        InitGitInfo();
        _productsCatalog = new Catalog { Name = "Root", Path = productRoot };
        TraverseDirectory(productRoot, _productsCatalog);

        foreach (var productInfo in ProductInfos)
        {
            if (!ProductMenus.ContainsKey(productInfo.Name))
            {
                continue;
            }

            var productPath = Path.Combine(productRoot, productInfo.Name);
            var productCatalog = _productsCatalog.FindCatalog(productPath);
            if (productCatalog == null)
            {
                continue;
            }

            var languages = GetMatchingLanguages(productInfo, productPath);
            if (languages.Count == 0)
            {
                Command.LogWarning($"No configured languages found for product [{productInfo.Name}].");
                continue;
            }

            var outputProductPath = Path.Combine(Output, "products", productInfo.Name);
            CopyStaticFiles(productPath, outputProductPath);

            var languageCatalogs = new Dictionary<string, Catalog>(StringComparer.OrdinalIgnoreCase);
            foreach (var language in languages)
            {
                var languagePath = Path.Combine(productPath, language);
                var languageCatalog = _productsCatalog.FindCatalog(languagePath);
                if (languageCatalog == null || GetOrderedDocs(languageCatalog).Count == 0)
                {
                    Command.LogWarning($"Product [{productInfo.Name}] language [{language}] has no Markdown documents.");
                    continue;
                }

                languageCatalogs[language] = languageCatalog;
                BuildLanguageData(productInfo, language, languagePath);
                BuildProductPages(productInfo, language, languageCatalog, languages);
            }

            if (languageCatalogs.TryGetValue(productInfo.DefaultLanguage, out var defaultCatalog))
            {
                var firstDoc = GetOrderedDocs(defaultCatalog).FirstOrDefault();
                if (firstDoc != null)
                {
                    BuildProductLandingPage(productInfo, productInfo.DefaultLanguage, languages, defaultCatalog, firstDoc);
                    SitemapEntries.Add(new Sitemap
                    {
                        Loc = BuildCanonicalUrl("products/" + productInfo.Name + ".html"),
                        Lastmod = firstDoc.PublishTime.ToString("yyyy-MM-dd")
                    });
                }
            }
        }

        Command.LogSuccess("Generated product content!");
    }

    private List<string> GetMatchingLanguages(ProductInfo productInfo, string productPath)
    {
        var directories = Directory.GetDirectories(productPath)
            .Select(Path.GetFileName)
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .ToDictionary(name => name!, StringComparer.OrdinalIgnoreCase);

        return productInfo.Languages
            .Select(language => directories.TryGetValue(language, out var actualName) ? actualName : null)
            .Where(language => language != null)
            .Select(language => language!)
            .ToList();
    }

    private void BuildProductPages(ProductInfo productInfo, string language, Catalog languageCatalog, List<string> languages)
    {
        var tree = BuildCatalogTree(languageCatalog, "products");
        var docs = GetOrderedDocs(languageCatalog);

        foreach (var doc in docs)
        {
            var markdown = File.ReadAllText(doc.Path);
            var html = BuildProductPage(productInfo, language, languages, doc, tree, markdown);
            var outputPath = Path.Combine(Output, "products", doc.HtmlPath);
            var directory = Path.GetDirectoryName(outputPath);
            if (!string.IsNullOrWhiteSpace(directory))
            {
                Directory.CreateDirectory(directory);
            }

            File.WriteAllText(outputPath, html, Encoding.UTF8);
            SitemapEntries.Add(new Sitemap
            {
                Loc = BuildCanonicalUrl("products/" + doc.HtmlPath),
                Lastmod = doc.PublishTime.ToString("yyyy-MM-dd")
            });
        }

        var searchPath = Path.Combine(Output, "products", productInfo.Name, language, "search.html");
        var searchHtml = BuildProductSearchPage(productInfo, language, languages, tree);
        Directory.CreateDirectory(Path.GetDirectoryName(searchPath)!);
        File.WriteAllText(searchPath, searchHtml, Encoding.UTF8);
    }

    private string BuildProductPage(ProductInfo productInfo, string language, List<string> languages, Doc doc, string tree, string markdown, string? canonicalPath = null)
    {
        var docContent = BuildMarkdownContent(doc);
        var title = GetTitleFromMarkdown(markdown);
        var canonicalUrl = BuildCanonicalUrl(canonicalPath ?? "products/" + doc.HtmlPath);
        var template = TemplateHelper.GetTplContent("product.html");

        return template
            .Replace("@{BaseUrl}", BaseUrl)
            .Replace("@{FaviconPath}", WebInfo.Icon ?? "favicon.ico")
            .Replace("@{Name}", WebInfo.Name)
            .Replace("@{ExtensionHead}", GetExtensionScript(docContent))
            .Replace("@{Title}", title)
            .Replace("@{Description}", productInfo.Description)
            .Replace("@{Keywords}", GetPageKeywords(title))
            .Replace("@{AuthorName}", WebInfo.AuthorName)
            .Replace("@{CanonicalUrl}", canonicalUrl)
            .Replace("@{LeftNav}", tree)
            .Replace("@{TOC}", GetContentTOC(markdown) ?? string.Empty)
            .Replace("@{DocContent}", docContent)
            .Replace("@{DocId}", ComputeMD5Hash(doc.HtmlPath))
            .Replace("@{ProductName}", productInfo.Name)
            .Replace("@{Language}", language)
            .Replace("@{TopActions}", BuildProductTopActions(languages))
            .Replace("@{UpdateTime}", (doc.UpdatedTime ?? doc.CreatedTime).ToString("yyyy-MM-dd HH:mm"))
            .Replace("@{GithubLink}", GetGithubLink(doc.Path))
            .Replace("@{NavMenus}", BuildNavigations(ContentPath));
    }

    private string BuildProductSearchPage(ProductInfo productInfo, string language, List<string> languages, string tree)
    {
        var template = TemplateHelper.GetTplContent("productSearch.html");
        var title = productInfo.Name + " Search (" + language + ")";
        var canonicalUrl = BuildCanonicalUrl("products/" + productInfo.Name + "/" + language + "/search.html");

        return template
            .Replace("@{BaseUrl}", BaseUrl)
            .Replace("@{FaviconPath}", WebInfo.Icon ?? "favicon.ico")
            .Replace("@{Name}", WebInfo.Name)
            .Replace("@{Title}", title)
            .Replace("@{Description}", productInfo.Description)
            .Replace("@{Keywords}", GetPageKeywords("search"))
            .Replace("@{AuthorName}", WebInfo.AuthorName)
            .Replace("@{CanonicalUrl}", canonicalUrl)
            .Replace("@{ProductName}", productInfo.Name)
            .Replace("@{Language}", language)
            .Replace("@{LeftNav}", tree)
            .Replace("@{TopActions}", BuildProductTopActions(languages))
            .Replace("@{TOC}", string.Empty)
            .Replace("@{NavMenus}", BuildNavigations(ContentPath));
    }

    private void BuildLanguageData(ProductInfo productInfo, string language, string languagePath)
    {
        var languageCatalog = new Catalog { Name = productInfo.Name, Path = languagePath };
        TraverseDirectory(languagePath, languageCatalog);
        var docs = GetOrderedDocs(languageCatalog);
        languageCatalog.FirstDocHtmlPath = docs.FirstOrDefault()?.HtmlPath;

        var productDataPath = Path.Combine(DataPath, "products", productInfo.Name);
        Directory.CreateDirectory(productDataPath);
        var dataPath = Path.Combine(productDataPath, language + ".json");
        File.WriteAllText(dataPath, JsonSerializer.Serialize(languageCatalog, _jsonSerializerOptions), Encoding.UTF8);

        var searchItems = docs.Select(doc =>
        {
            var markdown = File.ReadAllText(doc.Path);
            return new
            {
                doc.Title,
                doc.HtmlPath,
                Headings = GetContentHeading2(markdown),
                UpdatedTime = (doc.UpdatedTime ?? doc.CreatedTime).ToString("yyyy-MM-dd HH:mm")
            };
        });
        var searchPath = Path.Combine(productDataPath, language + "-search.json");
        File.WriteAllText(searchPath, JsonSerializer.Serialize(searchItems, _jsonSerializerOptions), Encoding.UTF8);
    }

    private void BuildProductLandingPage(ProductInfo productInfo, string language, List<string> languages, Catalog languageCatalog, Doc firstDoc)
    {
        var markdown = File.ReadAllText(firstDoc.Path);
        var tree = BuildCatalogTree(languageCatalog, "products");
        var html = BuildProductPage(
            productInfo,
            language,
            languages,
            firstDoc,
            tree,
            markdown,
            "products/" + productInfo.Name + ".html");

        var outputPath = Path.Combine(Output, "products", productInfo.Name + ".html");
        Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);
        File.WriteAllText(outputPath, html, Encoding.UTF8);
    }

    private string BuildProductTopActions(List<string> languages)
    {
        var links = new StringBuilder();
        foreach (var language in languages)
        {
            links.AppendLine("<a href=\"javascript:void(0);\" onclick=\"products.selectLanguage('" +
                System.Net.WebUtility.HtmlEncode(language) + "')\" class=\"dropdown-item\">" +
                System.Net.WebUtility.HtmlEncode(language) + "</a>");
        }

        return """
            <div class="dropdown">
                <div>
                    <button type="button" class="dropdown-toggle nav-link" title="Language">🌐</button>
                </div>
                <div class="dropdown-menu" tabindex="-1">
                    <div id="languageSelect" role="none">
            """ + links + """
                    </div>
                </div>
            </div>
            """;
    }

    private void InitGitInfo()
    {
        _repoUrl = WebInfo.RepositoryUrl;
        _branch = WebInfo.Branch;

        if (ProcessHelper.RunCommand("git", "rev-parse --show-toplevel", out var gitRoot))
        {
            _gitRoot = gitRoot.Trim().Replace('/', Path.DirectorySeparatorChar).Replace('\\', Path.DirectorySeparatorChar);
        }

        if (string.IsNullOrWhiteSpace(_repoUrl) && ProcessHelper.RunCommand("git", "remote get-url origin", out var remoteUrl))
        {
            remoteUrl = remoteUrl.Trim();
            if (remoteUrl.StartsWith("git@", StringComparison.OrdinalIgnoreCase))
            {
                remoteUrl = remoteUrl.Replace(":", "/").Replace("git@", "https://");
            }
            if (remoteUrl.EndsWith(".git", StringComparison.OrdinalIgnoreCase))
            {
                remoteUrl = remoteUrl[..^4];
            }
            _repoUrl = remoteUrl;
        }

        if (string.IsNullOrWhiteSpace(_branch) && ProcessHelper.RunCommand("git", "branch --show-current", out var branch))
        {
            _branch = branch.Trim();
        }
    }

    private string GetGithubLink(string filePath)
    {
        if (string.IsNullOrWhiteSpace(_repoUrl) || string.IsNullOrWhiteSpace(_gitRoot))
        {
            return string.Empty;
        }

        var fullPath = Path.GetFullPath(filePath);
        if (!fullPath.StartsWith(_gitRoot, StringComparison.OrdinalIgnoreCase))
        {
            return string.Empty;
        }

        var relativePath = Path.GetRelativePath(_gitRoot, fullPath).Replace('\\', '/');
        return _repoUrl + "/blob/" + (string.IsNullOrWhiteSpace(_branch) ? "main" : _branch) + "/" + relativePath;
    }
}
