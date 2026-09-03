using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Markdig;
using Markdig.Extensions.AutoIdentifiers;
using Share.MarkdownExtension;

namespace Share.Builders;
/// <summary>
/// 内容构建
/// </summary>
public class DocsBuilder(WebInfo webInfo) : BaseBuilder(webInfo)
{
    public List<DocInfo> DocInfos { get; set; } = webInfo.DocInfos;
    private string? _gitRoot;
    private string? _repoUrl;
    private string? _branch;
    private static readonly string[] SupportedImageExtensions = [".jpg", ".png", ".jpeg", ".gif", ".svg"];
    private static readonly Regex ImageSourceRegex = new(
        @"(?<prefix><img\b[^>]*?\bsrc\s*=\s*[""'])(?<url>[^""']+)(?<suffix>[""'])",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
    private readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        WriteIndented = true
    };

    /// <summary>
    /// 构建文档
    /// </summary>
    /// <returns></returns>
    public void BuildDocs()
    {
        if (DocInfos == null || DocInfos.Count == 0)
        {
            return;
        }
        InitGitInfo();

        var docRootPath = Path.Combine(ContentPath, "docs");
        var outputDocPath = Path.Combine(Output, "docs");

        var docsCatalog = new Catalog { Name = "Root", Path = docRootPath };
        TraverseDirectory(docRootPath, docsCatalog);

        var tplContent = TemplateHelper.GetTplContent("docs.html");
        tplContent = tplContent.Replace("@{Name}", WebInfo.Name);

        var genFiles = new List<GenFile>();
        // Track first doc for each docInfo to generate static homepage
        var docHomepages = new Dictionary<string, GenFile>();

        foreach (var docInfo in DocInfos)
        {
            var docPath = Path.Combine(docRootPath, docInfo.Name);
            if (!Directory.Exists(docPath))
            {
                Command.LogWarning($"Not found doc: {docPath}");
                continue;
            }

            var languageDirs = Directory.GetDirectories(docPath).Select(d => Path.GetFileName(d));
            var showLanguages = docInfo.Languages;
            var matchLanguages = languageDirs.Where(d => showLanguages.Contains(Path.GetFileName(d))).ToList();

            Command.LogInfo($"match languages: {string.Join(",", matchLanguages)} ");
            var topActions = BuildTopActions(docInfo);

            foreach (var language in matchLanguages)
            {
                var languagePath = Path.Combine(docPath, language);
                // 匹配版本
                var versionDirs = Directory.GetDirectories(languagePath).Select(d => Path.GetFileName(d));
                var showVersions = docInfo.Versions;
                var matchVersions = versionDirs.Where(d => showVersions.Contains(Path.GetFileName(d))).ToList();

                Command.LogInfo($"match versions: {string.Join(",", matchVersions)} ");

                var versionSelect = BuildVersionSelect(matchVersions, languagePath, docsCatalog);

                foreach (var version in matchVersions)
                {
                    var versionPath = Path.Combine(languagePath, version);
                    Command.LogInfo($"Build Docs: {docInfo.Name}/{language}/{versionPath}");
                    // 版本下的目录结构信息
                    var versionCatalog = docsCatalog.FindCatalog(versionPath);
                    if (versionCatalog == null)
                    {
                        Command.LogWarning($"Not found catalog: {versionPath}");
                        continue;
                    }
                    var docTree = BuildTree(versionCatalog);

                    var docs = GetOrderedDocs(versionCatalog);
                    var firstDoc = docs.FirstOrDefault();
                    if (firstDoc != null)
                    {
                        if (DocMenus.ContainsKey(docInfo.Name))
                        {
                            DocMenus.Remove(docInfo.Name);
                        }
                        // Update DocMenus to point to the static homepage instead of firstDoc.HtmlPath
                        DocMenus.Add(docInfo.Name, $"{docInfo.Name}.html");
                    }
                    // md 文件
                    bool isFirstDoc = true;
                    foreach (var doc in docs)
                    {
                        string markdown = File.ReadAllText(doc.Path);

                        var leftNav = versionSelect + docTree;
                        var docContent = BuildDocContent(doc);
                        var title = GetTitleFromMarkdown(markdown);
                        var toc = GetContentTOC(markdown) ?? "";
                        var extensionScript = GetExtensionScript(docContent);
                        var updateTimeStr = (doc.UpdatedTime ?? doc.CreatedTime).ToString("yyyy-MM-dd HH:mm");
                        var editLink = GetEditLink(doc.Path);

                        var canonicalUrl = BuildCanonicalUrl($"docs/{doc.HtmlPath}");
                        var htmlContent = tplContent.Replace("@{BaseUrl}", BaseUrl)
                            .Replace("@{FaviconPath}", WebInfo.Icon ?? "favicon.ico")
                            .Replace("@{ExtensionHead}", extensionScript)
                            .Replace("@{Title}", title)
                            .Replace("@{Description}", WebInfo.Description)
                            .Replace("@{Keywords}", GetPageKeywords(title))
                            .Replace("@{AuthorName}", WebInfo.AuthorName)
                            .Replace("@{CanonicalUrl}", canonicalUrl)
                            .Replace("@{LeftNav}", leftNav)
                            .Replace("@{TOC}", toc)
                            .Replace("@{DocContent}", docContent)
                            .Replace("@{DocId}", ComputeMD5Hash(doc.HtmlPath))
                            .Replace("@{DocName}", docInfo.Name)
                            .Replace("@{Language}", language)
                            .Replace("@{TopActions}", topActions)
                            .Replace("@{Version}", version)
                            .Replace("@{UpdateTime}", updateTimeStr)
                            .Replace("@{DocAuthor}", System.Net.WebUtility.HtmlEncode(doc.AuthorName))
                            .Replace("@{EditLink}", editLink);

                        var outputFilePath = Path.Combine(outputDocPath, doc.HtmlPath);

                        var dirPath = Path.GetDirectoryName(outputFilePath);
                        if (dirPath != null && !Directory.Exists(dirPath))
                        {
                            Directory.CreateDirectory(dirPath);
                        }
                        genFiles.Add(new GenFile
                        {
                            Name = doc.FileName,
                            Path = outputFilePath,
                            Content = htmlContent
                        });

                        // Generate static homepage for the first doc of each docInfo
                        if (isFirstDoc && !docHomepages.ContainsKey(docInfo.Name))
                        {
                            var homepagePath = Path.Combine(outputDocPath, $"{docInfo.Name}.html");

                            var homepageCanonical = BuildCanonicalUrl($"docs/{docInfo.Name}.html");
                            htmlContent = RewriteHomepageImagePaths(htmlContent, doc, homepagePath)
                                .Replace(canonicalUrl, homepageCanonical);
                            docHomepages[docInfo.Name] = new GenFile
                            {
                                Name = $"{docInfo.Name}.html",
                                Path = homepagePath,
                                Content = htmlContent
                            };
                        }
                        if (isFirstDoc)
                        {
                            isFirstDoc = false;
                        }
                    }

                    BuildDocSearchData(docInfo, language, version, docs);
                    var searchPage = BuildDocSearchPage(docInfo, language, version, versionSelect, docTree);
                    genFiles.Add(searchPage);

                    // 其他资源文件
                    List<string> otherFiles = Directory.EnumerateFiles(docPath, "*", SearchOption.AllDirectories)
                        .Where(f => !f.EndsWith(".md"))
                        .ToList();
                    foreach (var file in otherFiles)
                    {
                        var extension = Path.GetExtension(file);
                        if (!SupportedImageExtensions.Contains(extension)) { continue; }

                        string relativePath = file.Replace(ContentPath, Output);
                        string? dir = Path.GetDirectoryName(relativePath);

                        if (!Directory.Exists(dir))
                        {
                            Directory.CreateDirectory(dir!);
                        }

                        File.Copy(file, relativePath, true);
                    }
                    Command.LogSuccess($"Copied [{otherFiles.Count}] other files!");
                }
            }
        }

        var navMenuTmp = BuildNavigations(ContentPath);
        foreach (var genFile in genFiles)
        {
            genFile.Content = genFile.Content?.Replace("@{NavMenus}", navMenuTmp);
            File.WriteAllText(genFile.Path, genFile.Content);
        }
        Command.LogSuccess($"Generated [{genFiles.Count}] doc files!");

        // Generate static homepage files for each doc
        foreach (var homepage in docHomepages.Values)
        {
            homepage.Content = homepage.Content?.Replace("@{NavMenus}", navMenuTmp);
            File.WriteAllText(homepage.Path, homepage.Content);
            Command.LogSuccess($"Generate doc homepage: {homepage.Path}");
        }
    }

    private string RewriteHomepageImagePaths(string htmlContent, Doc doc, string homepagePath)
    {
        var contentRoot = Path.GetFullPath(ContentPath);
        var sourceDirectory = Path.GetDirectoryName(Path.GetFullPath(doc.Path));
        var homepageDirectory = Path.GetDirectoryName(Path.GetFullPath(homepagePath));
        if (sourceDirectory == null || homepageDirectory == null)
        {
            return htmlContent;
        }

        return ImageSourceRegex.Replace(htmlContent, match =>
        {
            var imageUrl = match.Groups["url"].Value;
            var rewrittenUrl = GetHomepageImageUrl(
                imageUrl,
                sourceDirectory,
                contentRoot,
                homepageDirectory);

            return rewrittenUrl == null
                ? match.Value
                : match.Groups["prefix"].Value + rewrittenUrl + match.Groups["suffix"].Value;
        });
    }

    private string? GetHomepageImageUrl(
        string imageUrl,
        string sourceDirectory,
        string contentRoot,
        string homepageDirectory)
    {
        var separatorIndex = imageUrl.IndexOfAny(['?', '#']);
        var imagePath = separatorIndex >= 0 ? imageUrl[..separatorIndex] : imageUrl;
        if (string.IsNullOrWhiteSpace(imagePath))
        {
            return null;
        }

        var decodedImagePath = System.Net.WebUtility.HtmlDecode(imagePath).Trim();
        if (IsNonRelativeUrl(decodedImagePath) || Path.IsPathRooted(decodedImagePath))
        {
            return null;
        }

        try
        {
            decodedImagePath = Uri.UnescapeDataString(decodedImagePath);
        }
        catch (UriFormatException)
        {
            return null;
        }

        var sourceImagePath = Path.GetFullPath(Path.Combine(
            sourceDirectory,
            decodedImagePath.Replace('/', Path.DirectorySeparatorChar)));
        if (!IsPathWithin(sourceImagePath, contentRoot) ||
            !File.Exists(sourceImagePath) ||
            !SupportedImageExtensions.Contains(Path.GetExtension(sourceImagePath)))
        {
            return null;
        }

        var outputImagePath = Path.Combine(
            Path.GetFullPath(Output),
            Path.GetRelativePath(contentRoot, sourceImagePath));
        var relativeImagePath = Path.GetRelativePath(homepageDirectory, outputImagePath)
            .Replace('\\', '/');
        if (!relativeImagePath.StartsWith(".", StringComparison.Ordinal))
        {
            relativeImagePath = "./" + relativeImagePath;
        }

        return relativeImagePath + (separatorIndex >= 0 ? imageUrl[separatorIndex..] : string.Empty);
    }

    private static bool IsNonRelativeUrl(string url)
    {
        return url.StartsWith("/", StringComparison.Ordinal) ||
            Uri.TryCreate(url, UriKind.Absolute, out _);
    }

    private static bool IsPathWithin(string path, string root)
    {
        var relativePath = Path.GetRelativePath(root, path);
        return !Path.IsPathRooted(relativePath) &&
            !relativePath.Equals("..", StringComparison.Ordinal) &&
            !relativePath.StartsWith(".." + Path.DirectorySeparatorChar, StringComparison.Ordinal) &&
            !relativePath.StartsWith(".." + Path.AltDirectorySeparatorChar, StringComparison.Ordinal);
    }

    private void BuildDocSearchData(DocInfo docInfo, string language, string version, List<Doc> docs)
    {
        var docItems = docs.Select(d =>
        {
            var markdown = File.ReadAllText(d.Path);
            var headings = GetContentHeading2(markdown);
            return new
            {
                d.Title,
                d.HtmlPath,
                Headings = headings,
                UpdatedTime = (d.UpdatedTime ?? d.CreatedTime).ToString("yyyy-MM-dd HH:mm")
            };
        }).ToList();

        var docDataPath = Path.Combine(DataPath, docInfo.Name);
        if (!Directory.Exists(docDataPath))
        {
            Directory.CreateDirectory(docDataPath);
        }
        var searchDataPath = Path.Combine(docDataPath, $"{language}-{version}-search.json");
        var json = JsonSerializer.Serialize(docItems, _jsonSerializerOptions);
        File.WriteAllText(searchDataPath, json, Encoding.UTF8);
        Command.LogSuccess($"update {docInfo.Name}-{language}-{version}-search.json!");
    }

    private GenFile BuildDocSearchPage(DocInfo docInfo, string language, string version, string versionSelect, string docTree)
    {
        var tplContent = TemplateHelper.GetTplContent("docsSearch.html");
        var leftNav = versionSelect + docTree;
        var title = $"{docInfo.Name} Search ({language} {version})";
        var canonicalUrl = BuildCanonicalUrl($"docs/{docInfo.Name}/{language}/{version}/search.html");
        var topActions = BuildTopActions(docInfo);

        var htmlContent = tplContent.Replace("@{BaseUrl}", BaseUrl)
            .Replace("@{FaviconPath}", WebInfo.Icon ?? "favicon.ico")
            .Replace("@{Name}", WebInfo.Name)
            .Replace("@{Title}", title)
            .Replace("@{Description}", WebInfo.Description)
            .Replace("@{Keywords}", GetPageKeywords("search"))
            .Replace("@{AuthorName}", WebInfo.AuthorName)
            .Replace("@{CanonicalUrl}", canonicalUrl)
            .Replace("@{DocName}", docInfo.Name)
            .Replace("@{Language}", language)
            .Replace("@{Version}", version)
            .Replace("@{LeftNav}", leftNav)
            .Replace("@{TopActions}", topActions)
            .Replace("@{TOC}", "");

        var outputFilePath = Path.Combine(Output, "docs", docInfo.Name, language, version, "search.html");
        var dirPath = Path.GetDirectoryName(outputFilePath);
        if (dirPath != null && !Directory.Exists(dirPath))
        {
            Directory.CreateDirectory(dirPath);
        }

        return new GenFile
        {
            Name = $"{docInfo.Name}-{language}-{version}-search.html",
            Path = outputFilePath,
            Content = htmlContent
        };
    }


    private void InitGitInfo()
    {
        // 优先使用配置
        _repoUrl = NormalizeRepositoryUrl(WebInfo.RepositoryUrl);
        _branch = WebInfo.Branch;

        // 获取 Git 根目录
        if (ProcessHelper.RunCommand("git", "rev-parse --show-toplevel", out string gitRoot))
        {
            _gitRoot = gitRoot.Trim().Replace('/', Path.DirectorySeparatorChar).Replace('\\', Path.DirectorySeparatorChar);
        }

        // 如果未配置 RepoUrl，尝试自动检测
        if (string.IsNullOrEmpty(_repoUrl))
        {
            if (ProcessHelper.RunCommand("git", "remote get-url origin", out string remoteUrl))
            {
                remoteUrl = remoteUrl.Trim();
                _repoUrl = NormalizeRepositoryUrl(remoteUrl);
            }
        }

        // 如果未配置 Branch，尝试自动检测
        if (string.IsNullOrEmpty(_branch))
        {
            if (ProcessHelper.RunCommand("git", "branch --show-current", out string branch))
            {
                _branch = branch.Trim();
            }
        }
    }

    private string GetEditLink(string filePath)
    {
        if (string.IsNullOrEmpty(_repoUrl) || string.IsNullOrEmpty(_gitRoot))
        {
            return "";
        }

        var fullPath = Path.GetFullPath(filePath);
        if (!fullPath.StartsWith(_gitRoot, StringComparison.OrdinalIgnoreCase))
        {
            return "";
        }

        var relativePath = Path.GetRelativePath(_gitRoot, fullPath).Replace('\\', '/');
        var branch = string.IsNullOrEmpty(_branch) ? "main" : _branch;

        return $"{_repoUrl}/blob/{branch}/{relativePath}";
    }

    public string BuildTopActions(DocInfo docInfo)
    {
        string languages = "";
        if (docInfo.Languages.Length > 0)
        {
            foreach (var lang in docInfo.Languages)
            {
                                languages += $"""
                                        <a href="javascript:void(0);" onclick="doc.selectLanguage('{lang}')" class="dropdown-item">{lang}</a>
                                        """;
            }
        }

        return $"""
                        <div class="dropdown">
                                <div>
                                    <button type="button" class="dropdown-toggle nav-link" title="Language">
                                        🌐
                                    </button>
                                </div>
                                <div class="dropdown-menu" tabindex="-1">
                                        <div id="languageSelect" role="none">
                                        {languages}
                                        </div>
                                </div>
                        </div>
                        """;
    }

    public string BuildDocContent(Doc doc)
    {
        return BuildMarkdownContent(doc);
    }

    /// <summary>
    /// 版本选择控件
    /// </summary>
    /// <param name="docInfo"></param>
    /// <returns></returns>
    public string BuildVersionSelect(List<string> versions, string languagePath, Catalog docsCatalog)
    {
        var sb = new StringBuilder();
        // version select

        sb.AppendLine("""
            <select id="versionSelect" class="version-select">

            """);
        foreach (var version in versions)
        {
            var versionPath = Path.Combine(languagePath, version);
            var versionCatalog = docsCatalog.FindCatalog(versionPath);
            string? url = "";
            if (versionCatalog != null)
            {
                var firstDoc = GetOrderedDocs(versionCatalog).FirstOrDefault();
                url = firstDoc?.HtmlPath;
            }
            sb.AppendLine($"<option data-url='{url}' value='{version}'>{version}</option>");
        }

        sb.AppendLine("</select>");
        return sb.ToString();
    }

    /// <summary>
    /// 树型导航控件
    /// </summary>
    /// <returns></returns>
    public string BuildTree(Catalog rootCatalog, string routePrefix = "docs")
    {
        if (rootCatalog == null)
        {
            throw new ArgumentNullException(nameof(rootCatalog));
        }

        return BuildCatalogTree(rootCatalog, routePrefix);
    }
}

/// <summary>
/// 树形结点
/// </summary>
public class TreeNodeItem
{
    public required string DisplayName { get; set; }
    public required string Href { get; set; }
    public required string Id { get; set; }
}
