using Markdig;
using Markdig.Extensions.AutoIdentifiers;
using Share.MarkdownExtension;
using System.Text;

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

                    var docs = versionCatalog.GetAllDocs();
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
                        var githubLink = GetGithubLink(doc.Path);

                        var htmlContent = tplContent.Replace("@{BaseUrl}", BaseUrl)
                            .Replace("@{FaviconPath}", WebInfo.Icon ?? "favicon.ico")
                            .Replace("@{ExtensionHead}", extensionScript)
                            .Replace("@{Title}", title)
                            .Replace("@{LeftNav}", leftNav)
                            .Replace("@{TOC}", toc)
                            .Replace("@{DocContent}", docContent)
                            .Replace("@{DocId}", ComputeMD5Hash(doc.HtmlPath))
                            .Replace("@{DocName}", docInfo.Name)
                            .Replace("@{Language}", language)
                            .Replace("@{TopActions}", topActions)
                            .Replace("@{Version}", version)
                            .Replace("@{UpdateTime}", updateTimeStr)
                            .Replace("@{GithubLink}", githubLink);

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

                    // 其他资源文件
                    List<string> otherFiles = Directory.EnumerateFiles(versionPath, "*", SearchOption.AllDirectories)
                        .Where(f => !f.EndsWith(".md"))
                        .ToList();
                    string[] extensions = [".jpg", ".png", ".jpeg", ".gif", ".svg"];
                    foreach (var file in otherFiles)
                    {
                        var extension = Path.GetExtension(file);
                        if (!extensions.Contains(extension)) { continue; }

                        string relativePath = file.Replace(ContentPath, Output);
                        string? dir = Path.GetDirectoryName(relativePath);

                        if (!Directory.Exists(dir))
                        {
                            Directory.CreateDirectory(dir!);
                        }

                        File.Copy(file, relativePath, true);
                    }
                    Command.LogSuccess("Copied other files!");
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

    private void InitGitInfo()
    {
        // 优先使用配置
        _repoUrl = WebInfo.RepositoryUrl;
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
                // 处理 SSH 格式 git@github.com:User/Repo.git -> https://github.com/User/Repo
                if (remoteUrl.StartsWith("git@"))
                {
                    remoteUrl = remoteUrl.Replace(":", "/").Replace("git@", "https://");
                }
                if (remoteUrl.EndsWith(".git"))
                {
                    remoteUrl = remoteUrl[..^4];
                }
                _repoUrl = remoteUrl;
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

    private string GetGithubLink(string filePath)
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
                    <a href="javascript:void(0);" onclick="doc.selectLanguage('{lang}')" class="block px-3 py-1 text">{lang}</a>
                    """;
            }
        }

        return $"""
            <div class="relative dropdown">
                <div class="relative inline-block cursor-pointer">
                  <button type="button" class="flex items-center gap-x-1 text text-lg">
                    🌐
                  </button>
                </div>
                <div class="absolute right-0 mt-1 w-24 rounded-md bg-card dropdown-content hidden z-10 text-center">
                    <div id="languageSelect" class="py-1" role="none">
                    {languages}
                    </div>
                </div>
            </div>
            """;
    }

    public string BuildDocContent(Doc doc)
    {
        var pipeline = new MarkdownPipelineBuilder()
            .UseAlertBlocks()
            .UseFigures()
            .UseCitations()
            .UseFigures()
            .UseEmphasisExtras()
            .UseMathematics()
            .UseMediaLinks()
            .UseListExtras()
            .UseTaskLists()
            .UseDiagrams()
            .UseAutoLinks()
            .UseAutoIdentifiers(AutoIdentifierOptions.GitHub)
            .UsePipeTables()
            .UseBetterCodeBlock()
            .Build();

        string markdown = File.ReadAllText(doc.Path);
        string html = Markdig.Markdown.ToHtml(markdown, pipeline);
        //string relativePath = dirPath.Replace(dirPath, Path.Combine(Output, dirName)).Replace(".md", ".html");
        return html;
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
            <select id="versionSelect" class="border border-gray-300 dark:border-neutral-700 rounded-md p-2 my-2 w-full bg-white dark:bg-neutral-800 text-neutral-900 dark:text-neutral-100 focus:outline-none focus:ring-2 focus:ring-blue-500">

            """);
        foreach (var version in versions)
        {
            var versionPath = Path.Combine(languagePath, version);
            var versionCatalog = docsCatalog.FindCatalog(versionPath);
            string? url = "";
            if (versionCatalog != null)
            {
                var firstDoc = versionCatalog.GetAllDocs().FirstOrDefault();
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
    public string BuildTree(Catalog rootCatalog)
    {
        if (rootCatalog == null)
        {
            throw new ArgumentNullException(nameof(rootCatalog));
        }

        var sb = new StringBuilder();
        sb.AppendLine(@"<div class=""tree"">");
        sb.AppendLine(@"<ul class=""root-list"">");
        GenerateCatalogHtml(rootCatalog, sb);
        sb.AppendLine("</ul>");
        sb.AppendLine("</div>");
        return sb.ToString();
    }

    private void GenerateCatalogHtml(Catalog catalog, StringBuilder sb)
    {
        var orderFile = Path.Combine(catalog.Path, ".order");
        string[] orderData = [];
        if (File.Exists(orderFile))
        {
            orderData = File.ReadLines(orderFile).Where(l => !string.IsNullOrWhiteSpace(l))
                .ToArray();
        }
        var nodeItems = new List<TreeNodeItem>();

        if (catalog.Docs != null && catalog.Docs.Count > 0)
        {
            foreach (var doc in catalog.Docs)
            {
                var nodeItem = new TreeNodeItem
                {
                    DisplayName = doc.FileName.Replace(".md", ""),
                    Href = doc.HtmlPath,
                    Id = ComputeMD5Hash(doc.HtmlPath)
                };
                nodeItems.Add(nodeItem);
            }
        }

        if (catalog.Children != null && catalog.Children.Count > 0)
        {
            foreach (var child in catalog.Children)
            {
                var nodeItem = new TreeNodeItem
                {
                    DisplayName = child.Name,
                    Href = string.Empty,
                    Id = ComputeMD5Hash(child.Path)
                };
                nodeItems.Add(nodeItem);
            }
        }

        foreach (var item in nodeItems)
        {
            if (string.IsNullOrEmpty(item.Href))
            {
                sb.AppendLine(@$"<li><span class=""caret"">{item.DisplayName}</span>");
                sb.AppendLine(@"<ul class=""nested"">");

                var child = catalog.Children?.FirstOrDefault(c => c.Name == item.DisplayName);
                if (child != null)
                    GenerateCatalogHtml(child, sb);
                sb.AppendLine("</ul>");

            }
            else
            {
                sb.AppendLine($"""
                    <li id="{item.Id}" class="space">
                        <a class="text" href="/docs/{item.Href}">{item.DisplayName}</a>
                    </li>
                    """);
            }
        }
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