﻿using System.Text;
using System.Text.RegularExpressions;

namespace Share.Builders;
public partial class BaseBuilder
{
    public WebInfo WebInfo { get; init; }
    public string BaseUrl { get; set; }

    public BaseBuilder(WebInfo webInfo)
    {
        WebInfo = webInfo;
        BaseUrl = "/";
    }

    /// <summary>
    /// 内容页TOC
    /// </summary>
    /// <param name="markdown"></param>
    /// <returns></returns>
    protected string? GetContentTOC(string markdown)
    {
        markdown = Regex.Replace(markdown, @"```.*?```", "", RegexOptions.Singleline);
        markdown = Regex.Replace(markdown, @"`.*?`", "", RegexOptions.Singleline);

        string heading2Pattern = @"^##\s+(.+)$";
        MatchCollection matches = Regex.Matches(markdown, heading2Pattern, RegexOptions.Multiline);

        if (matches.Count > 0)
        {
            var tocBuilder = new StringBuilder();
            tocBuilder.AppendLine("<div class=\"toc-block sticky top-2\">");
            tocBuilder.AppendLine(" <p class=\"text-lg\">导航</p>");
            tocBuilder.AppendLine(@"<ul class=""toc"">");

            foreach (Match match in matches)
            {
                string headingText = match.Groups[1].Value;
                string headingId = headingText.ToLower().Replace(" ", "-");
                tocBuilder.AppendLine($"  <li><a href='#{headingId}'>{headingText}</a></li>");
            }
            tocBuilder.AppendLine("</ul>");
            tocBuilder.AppendLine("</div>");
            return tocBuilder.ToString();
        }
        return null;
    }

    /// <summary>
    /// 获取标题
    /// </summary>
    /// <param name="content"></param>
    /// <returns></returns>
    protected static string GetTitleFromMarkdown(string content)
    {
        // 使用正则表达式匹配标题
        var regex = TitleRegex();
        var match = regex.Match(content);
        return match.Success ? match.Groups[1].Value.Trim() : "";
    }

    private static string GetFullPath(Catalog catalog)
    {
        var path = catalog.Name;
        if (catalog.Parent != null)
        {
            path = Path.Combine(GetFullPath(catalog.Parent), path);
        }
        return path.Replace("Root", "");
    }
    protected string GetExtensionScript(string content)
    {
        string extensionHead = "";
        if (content.Contains("<div class=\"mermaid\">"))
        {
            extensionHead += "<script src=\"https://cdn.jsdelivr.net/npm/mermaid@10.9.0/dist/mermaid.min.js\"></script>" + Environment.NewLine;
        }
        if (content.Contains("<div class=\"math\">"))
        {
            extensionHead += """
                <script src="https://polyfill.io/v3/polyfill.min.js?features=es6"></script>
                <script id="MathJax-script" async src="https://cdn.jsdelivr.net/npm/mathjax@3.0.1/es5/tex-mml-chtml.js"></script>
                
                """;
        }
        if (content.Contains("<div class=\"nomnoml\">"))
        {
            extensionHead += """
                <script src="//unpkg.com/graphre/dist/graphre.js"></script>
                <script src="//unpkg.com/nomnoml/dist/nomnoml.js"></script>
                """;
        }
        return extensionHead;
    }

    protected string AddHtmlTags(string content, string title = "", string toc = "")
    {
        string extensionHead = GetExtensionScript(content);

        var tplContent = TemplateHelper.GetTplContent("docContent.html");
        tplContent = tplContent.Replace("@{Title}", title)
            .Replace("@{BaseUrl}", BaseUrl)
            .Replace("@{ExtensionHead}", extensionHead)
            .Replace("@{toc}", toc)
            .Replace("@{content}", content);
        return tplContent;
    }

    /// <summary>
    /// 递归构建Catalog
    /// </summary>
    /// <param name="directoryPath"></param>
    /// <param name="parentCatalog"></param>
    protected void TraverseDirectory(string directoryPath, Catalog parentCatalog)
    {
        foreach (string subDirectoryPath in Directory.GetDirectories(directoryPath))
        {
            var existMd = Directory.GetFiles(subDirectoryPath, "*.md").Length > 0;
            var catalog = new Catalog
            {
                Name = Path.GetFileName(subDirectoryPath),
                Parent = parentCatalog,
                Path = subDirectoryPath
            };
            parentCatalog.Children.Add(catalog);
            TraverseDirectory(subDirectoryPath, catalog);
        }

        foreach (string filePath in Directory.GetFiles(directoryPath, "*.md"))
        {
            var fileInfo = new FileInfo(filePath);
            var fileName = Path.GetFileName(filePath);
            var gitAddTime = GetCreatedTime(filePath);
            var gitUpdateTime = GetUpdatedTime(filePath);
            var doc = new Doc
            {
                Title = Path.GetFileNameWithoutExtension(filePath),
                FileName = fileName,
                Path = filePath,
                PublishTime = gitUpdateTime ?? gitAddTime ?? fileInfo.LastWriteTime,
                CreatedTime = gitAddTime ?? fileInfo.CreationTime,
                UpdatedTime = gitUpdateTime ?? fileInfo.LastWriteTime,
                Catalog = parentCatalog
            };

            doc.HtmlPath = Path.Combine(GetFullPath(parentCatalog), doc.FileName.Replace(".md", ".html"));
            parentCatalog.Docs.Add(doc);
        }
    }
    /// <summary>
    /// 菜单导航
    /// </summary>
    /// <returns></returns>
    protected string BuildNavigations(string contentPath)
    {
        var hasDocs = WebInfo.DocInfos.Count > 0;
        var hasBlog = Directory.Exists(Path.Combine(contentPath, "blogs"));
        var hasAbout = File.Exists(Path.Combine(contentPath, "about.md"));
        var navigations = new StringBuilder();
        if (hasBlog)
        {
            navigations.AppendLine(@"<a href=""/blogs.html"" class=""block py-2 text text-lg"">Blogs</a>");
        }
        if (hasDocs)
        {
            var docLinkHtml = "";
            foreach (var docName in WebInfo.DocInfos.Select(d => d.Name))
            {
                docLinkHtml += $@"<a href=""/docs/{docName}.html"" class=""block px-4 py-2 text"">{docName}</a>" + Environment.NewLine;
            }
            var docsMenuHtml = $$"""
                <div class="relative dropdown">
                  <div>
                    <button type="button" class="flex items-center gap-x-1 text text-lg">
                      Docs
                      <svg class="-mr-1 h-5 w-5 text-gray-400" viewBox="0 0 20 20" fill="currentColor" aria-hidden="true"
                        data-slot="icon">
                        <path fill-rule="evenodd"
                          d="M5.22 8.22a.75.75 0 0 1 1.06 0L10 11.94l3.72-3.72a.75.75 0 1 1 1.06 1.06l-4.25 4.25a.75.75 0 0 1-1.06 0L5.22 9.28a.75.75 0 0 1 0-1.06Z"
                          clip-rule="evenodd" />
                      </svg>
                    </button>
                  </div>
                  <div class="absolute z-10 mt-2 w-56 origin-top-right rounded-md bg-card dropdown-content hidden" tabindex="-1">
                    <div class="py-1" role="none">
                      {{docLinkHtml}}
                    </div>
                  </div>
                </div>
                """;
            navigations.AppendLine(docsMenuHtml);
        }
        if (hasAbout)
        {
            navigations.AppendLine("<a href=\"/about.html\" target=\"_blank\" class=\"block py-2 text text-lg \">About</a>");
        }
        return navigations.ToString();
    }

    private static DateTimeOffset? GetCreatedTime(string path)
    {
        if (ProcessHelper.RunCommand("git", @$"log --diff-filter=A --format=%aI -- ""{path}""", out string output))
        {
            output = output.Split("\n").First();
            return ConvertToDateTimeOffset(output);
        }
        return null;
    }

    private static DateTimeOffset? GetUpdatedTime(string path)
    {
        return ProcessHelper.RunCommand("git", @$"log -n 1 --format=%aI -- ""{path}""", out string output)
            ? ConvertToDateTimeOffset(output)
            : null;
    }

    private static DateTimeOffset? ConvertToDateTimeOffset(string output)
    {
        var dateString = output.Trim();
        string format = "yyyy-MM-ddTHH:mm:sszzz"; // 定义日期时间格式
        return DateTimeOffset.TryParseExact(dateString, format, null, System.Globalization.DateTimeStyles.None, out var result)
            ? result
            : null;
    }

    [GeneratedRegex(@"^# (.*)$", RegexOptions.Multiline)]
    private static partial Regex TitleRegex();
}
