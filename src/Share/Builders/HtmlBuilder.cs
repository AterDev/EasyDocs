using Markdig;
using Markdig.Extensions.AutoIdentifiers;
using Share.MarkdownExtension;
using System.Diagnostics;
using System.IO.Compression;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json.Serialization;
using System.Text.Unicode;

namespace Share.Builders;

public class HtmlBuilder : BaseBuilder
{
    /// <summary>
    /// 博客列表
    /// </summary>
    public List<Doc> Blogs { get; set; } = [];
    public List<Sitemap> AdditionalSitemapEntries { get; set; } = [];
    private Catalog? BlogCatalog { get; set; }

    private readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        ReferenceHandler = ReferenceHandler.IgnoreCycles,
        Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
        WriteIndented = true
    };

    public HtmlBuilder(WebInfo webinfo) : base(webinfo)
    {
    }

    public void BuildWebSite()
    {
        if (ExtractWebAssets())
        {
            BuildData();
            if (WebInfo.EnableBlog)
            {
                BuildHtmls("blogs");
            }
            BuildAboutMe();
            BuildIndexHtml();
            if (WebInfo.EnableBlog)
            {
                BuildBlogHtml();
            }
            CopyCustom();
        }
        else
        {
            Command.LogError("缺少基础模板文件!");
        }
    }

    /// <summary>
    /// 解压基础资源
    /// </summary>
    public bool ExtractWebAssets()
    {
#if DEBUG
        return true;
#endif
        var stream = TemplateHelper.GetZipFileStream("web.zip");
        if (stream == null)
        {
            return false;
        }
        using (ZipArchive archive = new(stream, ZipArchiveMode.Read))
        {
            archive.ExtractToDirectory(Output, true);
        }
        return true;
    }

    public void CopyCustom()
    {
        var path = Path.Combine(ContentPath, "custom");
        if (Directory.Exists(path))
        {
            var files = Directory.EnumerateFiles(path, "*", SearchOption.AllDirectories)
                .ToList();
            foreach (var file in files)
            {
                string relativePath = file.Replace(path, Output);
                string? dir = Path.GetDirectoryName(relativePath);
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir!);
                }
                File.Copy(file, relativePath, true);
            }
            Command.LogSuccess("copy custom files!");
        }
    }

    /// <summary>
    ///  html file
    /// </summary>
    private void BuildHtmls(string dirName)
    {
        var dirPath = Path.Combine(ContentPath, dirName);
        Command.LogInfo($"search files in {dirPath}");
        // 配置markdown管道
        MarkdownPipeline pipeline = CreateMarkdownPipeline();

        // 如果是文件存在
        if (File.Exists(dirPath))
        {
            string tplContent = ConvertMarkdownToHtml(dirPath, pipeline);
            string relativePath = dirPath.Replace(dirPath, Path.Combine(Output, dirName)).Replace(".md", ".html");
            string? dir = Path.GetDirectoryName(relativePath);

            File.WriteAllText(relativePath, tplContent, Encoding.UTF8);
            Command.LogSuccess($"generate html:{relativePath}");
            return;
        }

        if (Directory.Exists(dirPath))
        {
            // 读取所有要处理的md文件
            List<string> files = Directory.EnumerateFiles(dirPath, "*.md", SearchOption.AllDirectories)
                .ToList();
            // 复制其他非md文件
            List<string> otherFiles = Directory.EnumerateFiles(dirPath, "*", SearchOption.AllDirectories)
                .Where(f => !f.EndsWith(".md"))
                .ToList();

            foreach (var file in files)
            {
                try
                {
                    string tplContent = ConvertMarkdownToHtml(file, pipeline);

                    string relativePath = file.Replace(dirPath, Path.Combine(Output, dirName)).Replace(".md", ".html");
                    string? dir = Path.GetDirectoryName(relativePath);

                    if (!Directory.Exists(dir))
                    {
                        Directory.CreateDirectory(dir!);
                    }

                    File.WriteAllText(relativePath, tplContent, Encoding.UTF8);
                }
                catch (Exception e)
                {
                    Command.LogError($"parse markdown error: {file}" + e.Message + e.StackTrace);
                }
            }
            Command.LogSuccess($"Generated [{files.Count}] html files!");
            string[] extensions = [".jpg", ".png", ".jpeg", ".gif", ".svg"];
            foreach (var file in otherFiles)
            {
                var extension = Path.GetExtension(file);
                if (!extensions.Contains(extension)) { continue; }

                string relativePath = file.Replace(dirPath, Path.Combine(Output, dirName));
                string? dir = Path.GetDirectoryName(relativePath);

                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir!);
                }

                File.Copy(file, relativePath, true);
            }
            Command.LogSuccess($"copy [{otherFiles.Count}] other files!");
        }
    }

    private static MarkdownPipeline CreateMarkdownPipeline()
    {
        return new MarkdownPipelineBuilder()
            .UseAlertBlocks()
            .UseFigures()
            .UseCitations()
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
    }

    /// <summary>
    /// markdown to html
    /// </summary>
    /// <param name="dirPath"></param>
    /// <param name="pipeline"></param>
    /// <returns></returns>
    private string ConvertMarkdownToHtml(string dirPath, MarkdownPipeline pipeline, string? outputRelativePath = null)
    {
        string markdown = File.ReadAllText(dirPath);
        string html = Markdown.ToHtml(markdown, pipeline);

        var title = GetTitleFromMarkdown(markdown);
        var toc = GetContentTOC(markdown) ?? "";
        var side = GetBlogSide(dirPath);
        var blog = Blogs.FirstOrDefault(b => string.Equals(b.Path, dirPath, StringComparison.OrdinalIgnoreCase));
        var updateTime = blog?.UpdatedTime ?? blog?.CreatedTime ?? new DateTimeOffset(File.GetLastWriteTime(dirPath));
        var authorName = blog?.AuthorName ?? WebInfo.AuthorName;
        string extensionHead = GetExtensionScript(html);
        var relativePath = outputRelativePath ?? GetRelativeHtmlPath(dirPath);
        var canonicalUrl = BuildCanonicalUrl(relativePath);

        var tplContent = TemplateHelper.GetTplContent("blogContent.html");
        tplContent = tplContent.Replace("@{Title}", title)
            .Replace("@{Description}", WebInfo.Description)
            .Replace("@{Keywords}", GetPageKeywords(title))
            .Replace("@{AuthorName}", WebInfo.AuthorName)
            .Replace("@{CanonicalUrl}", canonicalUrl)
            .Replace("@{BaseUrl}", BaseUrl)
            .Replace("@{FaviconPath}", WebInfo.Icon ?? "favicon.ico")
            .Replace("@{Name}", WebInfo.Name)
            .Replace("@{ExtensionHead}", extensionHead)
            .Replace("@{DocAuthor}", System.Net.WebUtility.HtmlEncode(authorName))
            .Replace("@{UpdateTime}", updateTime.ToString("yyyy-MM-dd HH:mm"))
            .Replace("@{NavMenus}", BuildNavigations(ContentPath))
            .Replace("@{toc}", toc)
            .Replace("@{side}", side)
            .Replace("@{content}", html);
        return tplContent;
    }

    private string GetRelativeHtmlPath(string sourcePath)
    {
        var relativePath = Path.GetRelativePath(ContentPath, sourcePath).Replace("\\", "/");
        return relativePath.EndsWith(".md", StringComparison.OrdinalIgnoreCase)
            ? relativePath[..^3] + ".html"
            : relativePath;
    }

    private string GetBlogSide(string sourcePath)
    {
        var blog = Blogs.FirstOrDefault(b => string.Equals(b.Path, sourcePath, StringComparison.OrdinalIgnoreCase));
        if (blog != null && BlogCatalog != null)
        {
            var side = new StringBuilder();
            side.AppendLine("<nav class=\"blog-tree-nav\" aria-label=\"Blog categories\">");
            side.AppendLine("<div class=\"blog-tree-title\">Blogs</div>");
            side.AppendLine("<ul class=\"blog-tree\">");
            GenerateBlogTree(BlogCatalog, blog.Path, side);
            side.AppendLine("</ul>");
            side.AppendLine("</nav>");
            return side.ToString();
        }
        return "";
    }

    private void GenerateBlogTree(Catalog catalog, string currentPath, StringBuilder sb)
    {
        foreach (var doc in catalog.Docs)
        {
            var isCurrent = string.Equals(doc.Path, currentPath, StringComparison.OrdinalIgnoreCase);
            var activeClass = isCurrent ? " blog-tree-current" : string.Empty;
            sb.AppendLine($"<li class=\"blog-tree-document{activeClass}\"><a href=\"{BuildBlogPath(doc.HtmlPath)}\">{System.Net.WebUtility.HtmlEncode(doc.Title)}</a></li>");
        }

        foreach (var child in catalog.Children)
        {
            var containsCurrent = child.GetAllDocs().Any(doc => string.Equals(doc.Path, currentPath, StringComparison.OrdinalIgnoreCase));
            var open = containsCurrent ? " open" : string.Empty;
            sb.AppendLine($"<li class=\"blog-tree-category\"><details{open}><summary>{System.Net.WebUtility.HtmlEncode(child.Name)}</summary><ul>");
            GenerateBlogTree(child, currentPath, sb);
            sb.AppendLine("</ul></details></li>");
        }
    }

    /// <summary>
    /// json 数据文件
    /// </summary>
    public void BuildData()
    {
        if (!Directory.Exists(DataPath))
        {
            Directory.CreateDirectory(DataPath);
        }
        var webInfoContent = JsonSerializer.Serialize(WebInfo, _jsonSerializerOptions);
        File.WriteAllText(Path.Combine(DataPath, Command.WebConfigFileName), webInfoContent, Encoding.UTF8);

        BuildBlogs();
        BuildDocsData();
    }

    private void RemoveBlogArtifacts()
    {
        if (string.Equals(
                Path.GetFullPath(ContentPath),
                Path.GetFullPath(Output),
                StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        var blogOutputPath = Path.Combine(Output, "blogs");
        if (Directory.Exists(blogOutputPath))
        {
            Directory.Delete(blogOutputPath, true);
        }

        foreach (var filePath in new[]
        {
            Path.Combine(Output, "blogs.html"),
            Path.Combine(DataPath, "blogs.json"),
            Path.Combine(Output, "sitemap.xml")
        })
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }
    }

    public void BuildBlogs()
    {
        if (!WebInfo.EnableBlog)
        {
            RemoveBlogArtifacts();
            Blogs = [];
            BlogCatalog = null;
            BuildSitemap([], AdditionalSitemapEntries);
            return;
        }

        // create blogs.json
        var blogPath = Path.Combine(ContentPath, "blogs");
        var rootCatalog = new Catalog { Name = "Root", Path = blogPath };
        TraverseDirectory(blogPath, rootCatalog);
        BlogCatalog = rootCatalog;
        Blogs = rootCatalog.GetAllDocs();
        string json = JsonSerializer.Serialize(rootCatalog, _jsonSerializerOptions);

        string blogDataPath = Path.Combine(DataPath, "blogs.json");
        File.WriteAllText(blogDataPath, json, Encoding.UTF8);
        Command.LogSuccess("update blogs.json!");
        // create sitemap.xml
        var blogs = rootCatalog.GetAllDocs();
        BuildSitemap(blogs, AdditionalSitemapEntries);
    }

    public void BuildDocsData()
    {
        var docInfos = WebInfo.DocInfos;
        var docRootPath = Path.Combine(ContentPath, "docs");

        foreach (var docInfo in docInfos)
        {
            var docPath = Path.Combine(docRootPath, docInfo.Name);
            if (!Directory.Exists(docPath))
            {
                Command.LogWarning($"{docPath} not exist! skip it.");
                continue;
            }
            // 匹配语言
            var languageDirs = Directory.GetDirectories(docPath).Select(d => Path.GetFileName(d));
            var showLanguages = docInfo.Languages;
            var matchLanguages = languageDirs.Where(d => showLanguages.Contains(Path.GetFileName(d))).ToList();
            foreach (var language in matchLanguages)
            {
                var languagePath = Path.Combine(docPath, language);
                // 匹配版本
                var versionDirs = Directory.GetDirectories(languagePath).Select(d => Path.GetFileName(d));
                var showVersions = docInfo.Versions;
                var matchVersions = versionDirs.Where(d => showVersions.Contains(Path.GetFileName(d))).ToList();

                // 以{docInfo.Name}/{language}-{version}.json 生成对应语言版本的内容
                foreach (var version in matchVersions)
                {
                    var versionPath = Path.Combine(languagePath, version);
                    var versionCatalog = new Catalog { Name = $"{docInfo.Name}", Path = versionPath };

                    var sw = Stopwatch.StartNew();
                    TraverseDirectory(versionPath, versionCatalog);
                    sw.Stop();
                    Command.LogInfo($"Traverse {docInfo.Name}-{language}-{version} in {sw.ElapsedMilliseconds} ms");

                    versionCatalog.FirstDocHtmlPath = GetOrderedDocs(versionCatalog).FirstOrDefault()?.HtmlPath;
                    string json = JsonSerializer.Serialize(versionCatalog, _jsonSerializerOptions);
                    string versionDataPath = Path.Combine(DataPath, docInfo.Name);
                    if (!Directory.Exists(versionDataPath))
                    {
                        Directory.CreateDirectory(versionDataPath);
                    }
                    var docFilePath = Path.Combine(versionDataPath, $"{language}-{version}.json");
                    File.WriteAllText(docFilePath, json, Encoding.UTF8);
                    Command.LogSuccess($"update {docInfo.Name}-{language}-{version}.json!");
                }
            }
        }
    }

    /// <summary>
    /// 生成aboutme
    /// </summary>
    public void BuildAboutMe()
    {
        var aboutPath = FindAboutFile(ContentPath);
        if (aboutPath == null)
        {
            return;
        }

        var html = ConvertMarkdownToHtml(aboutPath, CreateMarkdownPipeline(), "about.html");
        var outputPath = Path.Combine(Output, "about.html");
        Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);
        File.WriteAllText(outputPath, html, Encoding.UTF8);
        Command.LogSuccess("update about.html");

    }

    /// <summary>
    /// 构建 index.html
    /// </summary>
    public void BuildIndexHtml()
    {
        var indexPath = Path.Combine(Output, "index.html");
        var indexHtml = TemplateHelper.GetTplContent("index.html");
        Catalog? rootCatalog = null;
        if (WebInfo.EnableBlog)
        {
            var blogData = Path.Combine(DataPath, "blogs.json");
            var blogContent = File.ReadAllText(blogData);
            rootCatalog = JsonSerializer.Deserialize<Catalog>(blogContent);
        }

        if (WebInfo != null)
        {
            var navigations = BuildNavigations(ContentPath);
            var blogHtml = WebInfo.EnableBlog && rootCatalog != null
                ? GenBlogListHtml(rootCatalog, WebInfo)
                : string.Empty;
            // 生成最新的博客列表以及 文档列表(如果有)
            var latestBlogs = WebInfo.EnableBlog
                ? Blogs.OrderByDescending(b => b.PublishTime).Take(4).ToList()
                : [];
            var blogSb = new StringBuilder();
            if (latestBlogs.Count > 0)
            {
                                blogSb.AppendLine("""
                                        <div class="section-title">
                                            New Blogs
                                        </div>
                                        <div class="card-grid">
                                        """);
                foreach (var blog in latestBlogs)
                {
                    var date = blog.UpdatedTime?.ToLocalTime().ToString("yyyy-MM-dd HH:mm");
                    blogSb.AppendLine($"""
                    <div class="blog-card">
                      <a class="title" href="{BuildBlogPath(blog.HtmlPath)}" target="_blank">
                        <p>{blog.Title}</p>
                      </a>
                      <p class="sub-title">👨‍💻 {WebInfo.AuthorName} &nbsp;&nbsp;📆 {date}</p>
                    </div>
                    """);
                }
                blogSb.Append("</div>");
            }

            // 生成文档列表
            var docSb = new StringBuilder();
            if (DocMenus.Count > 0)
            {
                docSb.AppendLine("<div class=\"section-title\">Docs</div>");
                docSb.AppendLine("<div class=\"card-grid\">");

                foreach (var doc in DocMenus)
                {
                    var docInfo = WebInfo.DocInfos.FirstOrDefault(d => d.Name == doc.Key);
                    if (docInfo == null)
                    {
                        continue;
                    }

                    var logoPath = string.IsNullOrWhiteSpace(docInfo.Logo)
                        ? null
                        : Path.Combine(ContentPath, "docs", doc.Key, docInfo.Logo);
                    var image = logoPath != null && File.Exists(logoPath)
                        ? "<img class=\"doc-card-image\" src=\"" + BuildSiteUrl("docs/" + doc.Key + "/" + docInfo.Logo) + "\" />"
                        : string.Empty;
                    docSb.AppendLine("<a href=\"" + BuildSiteUrl("docs/" + doc.Key + ".html") + "\" target=\"_blank\">");
                    docSb.AppendLine("<div class=\"blog-card\">" + image);
                    docSb.AppendLine("<p class=\"title\">" + System.Net.WebUtility.HtmlEncode(docInfo.Name) + " Docs</p>");
                    docSb.AppendLine("<p class=\"sub-title\">" + System.Net.WebUtility.HtmlEncode(docInfo.Description) + "</p>");
                    docSb.AppendLine("</div></a>");
                }
                docSb.Append("</div>");
            }

            var productSb = new StringBuilder();
            if (ProductMenus.Count > 0)
            {
                productSb.AppendLine("<div class=\"section-title\">Products</div>");
                productSb.AppendLine("<div class=\"card-grid\">");
                foreach (var product in ProductMenus)
                {
                    var productInfo = WebInfo.ProductInfos.FirstOrDefault(p => p.Name == product.Key);
                    if (productInfo == null)
                    {
                        continue;
                    }

                    var logoPath = string.IsNullOrWhiteSpace(productInfo.Logo)
                        ? null
                        : Path.Combine(ContentPath, "products", product.Key, productInfo.Logo);
                    var image = logoPath != null && File.Exists(logoPath)
                        ? "<img class=\"doc-card-image\" src=\"" + BuildSiteUrl("products/" + product.Key + "/" + productInfo.Logo) + "\" />"
                        : string.Empty;
                    productSb.AppendLine("<a href=\"" + BuildSiteUrl("products/" + product.Key + ".html") + "\" target=\"_blank\">");
                    productSb.AppendLine("<div class=\"blog-card\">" + image);
                    productSb.AppendLine("<p class=\"title\">" + System.Net.WebUtility.HtmlEncode(productInfo.Name) + "</p>");
                    productSb.AppendLine("<p class=\"sub-title\">" + System.Net.WebUtility.HtmlEncode(productInfo.Description) + "</p>");
                    productSb.AppendLine("</div></a>");
                }
                productSb.Append("</div>");
            }

            var indexTitle = WebInfo.Name;
            indexHtml = indexHtml.Replace("@{Name}", WebInfo.Name)
                .Replace("@{Title}", indexTitle)
                .Replace("@{Description}", WebInfo.Description)
                .Replace("@{Keywords}", GetPageKeywords())
                .Replace("@{AuthorName}", WebInfo.AuthorName)
                .Replace("@{CanonicalUrl}", BuildCanonicalUrl(string.Empty))
                .Replace("@{navigations}", navigations)
                .Replace("@{blogs}", blogSb.ToString())
                .Replace("@{docs}", docSb.ToString())
                .Replace("@{products}", productSb.ToString())
                .Replace("@{FaviconPath}", WebInfo.Icon ?? "favicon.ico")
                .Replace("@{BaseUrl}", BaseUrl);

            File.WriteAllText(indexPath, indexHtml, Encoding.UTF8);
            Command.LogSuccess("update index.html");
        }
    }

    /// <summary>
    /// 构建blogs.html
    /// </summary>
    public void BuildBlogHtml()
    {
        if (!WebInfo.EnableBlog)
        {
            return;
        }

        var indexPath = Path.Combine(Output, "blogs.html");
        var indexHtml = TemplateHelper.GetTplContent("blogs.html");
        var blogData = Path.Combine(DataPath, "blogs.json");
        var blogContent = File.ReadAllText(blogData);
        var rootCatalog = JsonSerializer.Deserialize<Catalog>(blogContent);
        if (rootCatalog != null && WebInfo != null)
        {
            var navigations = BuildNavigations(ContentPath);
            var blogHtml = GenBlogListHtml(rootCatalog, WebInfo);
            var siderBarHtml = GenSiderBar(rootCatalog);

            var blogsTitle = $"{WebInfo.Name} - Blogs";
            indexHtml = indexHtml.Replace("@{Name}", WebInfo.Name)
                .Replace("@{Title}", blogsTitle)
                .Replace("@{Description}", WebInfo.Description)
                .Replace("@{Keywords}", GetPageKeywords("blogs"))
                .Replace("@{AuthorName}", WebInfo.AuthorName)
                .Replace("@{CanonicalUrl}", BuildCanonicalUrl("blogs.html"))
                .Replace("@{navigations}", navigations)
                .Replace("@{BaseUrl}", BaseUrl)
                .Replace("@{FaviconPath}", WebInfo.Icon ?? "favicon.ico")
                .Replace("@{blogList}", blogHtml)
                .Replace("@{siderbar}", siderBarHtml);

            File.WriteAllText(indexPath, indexHtml, Encoding.UTF8);
            Command.LogSuccess("update blogs.html");
        }
    }

    /// <summary>
    /// 创建sitemap.xml
    /// </summary>
    private void BuildSitemap(List<Doc> blogs, IEnumerable<Sitemap>? additionalEntries = null)
    {
        if (!string.IsNullOrWhiteSpace(WebInfo.Domain))
        {
            var sitemaps = new List<Sitemap>();
            var domain = WebInfo.Domain.EndsWith('/') ? WebInfo.Domain[..^1] : WebInfo.Domain;
            foreach (var blog in blogs)
            {
                var sitemap = new Sitemap
                {
                    Loc = domain + BuildBlogPath(blog.HtmlPath),
                    Lastmod = blog.PublishTime.ToString("yyyy-MM-dd")
                };
                sitemaps.Add(sitemap);
            }

            if (additionalEntries != null)
            {
                sitemaps.AddRange(additionalEntries);
            }

            var sitemapPath = Path.Combine(Output, "sitemap.xml");
            if (sitemaps.Count == 0)
            {
                if (File.Exists(sitemapPath))
                {
                    File.Delete(sitemapPath);
                }
                return;
            }

            var sitemapXml = Sitemap.GetSitemaps(sitemaps);
            File.WriteAllText(sitemapPath, sitemapXml, Encoding.UTF8);
            Command.LogSuccess("update sitemap.xml");
        }
    }

    /// <summary>
    /// blog list html
    /// </summary>
    /// <returns></returns>
    private string GenBlogListHtml(Catalog rootCatalog, WebInfo webInfo)
    {
        var sb = new StringBuilder();
        if (rootCatalog == null)
        {
            return string.Empty;
        }

        var blogs = rootCatalog.GetAllDocs().OrderByDescending(b => b.PublishTime).ToList() ?? [];

        foreach (var blog in blogs)
        {
            var html = $"""
                   <div class="card">
                       <div class="card-body">
                           <div class="card-title">
                               <a href = "{BuildBlogPath(blog.HtmlPath)}" target="_blank" class="card-title-link">📑 {blog.Title}</a>
                           </div>
                           <p class="card-meta">
                               👨‍💻 {webInfo?.AuthorName}
                               &nbsp;&nbsp;
                               📆 <span class="publish-time" data-time="{blog.PublishTime:yyyy-MM-ddTHH:mm:sszzz}"></span> 
                           </p>
                       </div>
                   </div>
                   """;
            sb.AppendLine(html);
        }
        return sb.ToString();
    }

    /// <summary>
    /// catalog and date
    /// </summary>
    /// <returns></returns>
    private string GenSiderBar(Catalog data)
    {
        var sb = new StringBuilder();
        var catalogs = data?.Children.ToList() ?? [];
        var allBlogs = data?.GetAllDocs().OrderByDescending(b => b.PublishTime).ToList() ?? [];
        var dates = allBlogs!.Select(b => b.PublishTime)
            .OrderByDescending(b => b)
            .DistinctBy(b => b.ToString("yyyy-MM"))
            .ToList();

        sb.AppendLine("""<div id="catalog-list" class="sidebar-card">""");
        sb.AppendLine("<div class=\"sidebar-title\">分类</div>");
        sb.AppendLine($"""
            <span data-catalog="all" class="filter-item">
                全部 [{allBlogs.Count}]
            </span>
            """);
        foreach (var catalog in catalogs)
        {
            var html = $"""
                <span data-catalog="{catalog.Name}" class="filter-item">
                    {catalog.Name} [{catalog.Docs.Count}]
                </span>
                """;

            sb.AppendLine(html);
        }
        sb.AppendLine("</div>");

        sb.AppendLine("""<div id="date-list" class="sidebar-card">""");
        sb.AppendLine("<div class=\"sidebar-title\">存档</div>");
        sb.AppendLine($"""
            <span data-date="all" class="filter-item">
                全部 [{allBlogs.Count}]
            </span>
            """);
        foreach (var date in dates)
        {
            var count = allBlogs.Count(b => b.PublishTime.Year == date.Year && b.PublishTime.Month == date.Month);
            var html = $"""
                <span data-date="{date:yyyy-MM}" class="filter-item">
                    {date:yyyy-MM} [{count}]
                </span>
                """;
            sb.AppendLine(html);
        }
        sb.AppendLine("</div>");

        return sb.ToString();
    }

    private string BuildBlogPath(string path)
    {
        return path.StartsWith('/')
            ? BaseUrl + "blogs" + path
            : BaseUrl + "blogs/" + path;
    }
}
