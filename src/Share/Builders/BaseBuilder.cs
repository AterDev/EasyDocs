using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace Share.Builders;

public partial class BaseBuilder
{
    public WebInfo WebInfo { get; init; }

    public string ContentPath { get; init; }
    public string Output { get; init; }
    public string DataPath { get; init; }

    public string BaseUrl { get; set; }

    public static Dictionary<string, string> DocMenus { get; set; } = [];
    private static readonly Dictionary<string, (DateTimeOffset? Created, DateTimeOffset? Updated)> GitTimeCache = new(StringComparer.OrdinalIgnoreCase);
    private static bool _isGitLoaded = false;
    private static readonly object _gitLoadLock = new();

    public BaseBuilder(WebInfo webInfo)
    {
        WebInfo = webInfo;
        BaseUrl = "/";
        ContentPath = webInfo.ContetPath.EndsWith(Path.DirectorySeparatorChar) ? webInfo.ContetPath[0..^1] : webInfo.ContetPath;
        Output = webInfo.OutputPath;
        DataPath = Path.Combine(Output, BlogConst.DataPath);
    }

    public void EnableBaseUrl()
    {
        BaseUrl = WebInfo?.BaseHref ?? "/";
        if (!BaseUrl.EndsWith('/'))
        {
            BaseUrl += "/";
        }
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
            tocBuilder.AppendLine(" <p class=\"text-lg\">内容大纲</p>");
            tocBuilder.AppendLine(@"<ul class=""toc"">");

            foreach (Match match in matches)
            {
                string headingText = match.Groups[1].Value;
                string headingId = NormalizeGitHub(headingText);

                // 去除表情符号
                headingId = Regex.Replace(headingId, @"[\uD800-\uDBFF][\uDC00-\uDFFF]", "");

                tocBuilder.AppendLine($"""
                    <li>
                      <a href="javascript:void(0);" onclick="window.location.href=window.location.href.split('#')[0]+'#{headingId}'">{headingText}</a>
                    </li>
                    """);
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
        if (content.Contains("class=\"mermaid\""))
        {
            extensionHead += "<script src=\"https://cdn.jsdelivr.net/npm/mermaid@10.9.0/dist/mermaid.min.js\"></script>" + Environment.NewLine;
        }
        if (content.Contains("class=\"math\""))
        {
            extensionHead += """
                <script src="https://polyfill.io/v3/polyfill.min.js?features=es6"></script>
                <script id="MathJax-script" async src="https://cdn.jsdelivr.net/npm/mathjax@3.0.1/es5/tex-mml-chtml.js"></script>
                
                """;
        }
        if (content.Contains("class=\"nomnoml\""))
        {
            extensionHead += """
                <script src="//unpkg.com/graphre/dist/graphre.js"></script>
                <script src="//unpkg.com/nomnoml/dist/nomnoml.js"></script>
                """;
        }
        return extensionHead;
    }

    /// <summary>
    /// 递归构建Catalog
    /// </summary>
    /// <param name="directoryPath"></param>
    /// <param name="parentCatalog"></param>
    protected void TraverseDirectory(string directoryPath, Catalog parentCatalog)
    {
        // 确保 Git 历史已加载
        LoadGitHistory(directoryPath);

        // 排序数据
        var orderFile = Path.Combine(directoryPath, ".order");
        string[] orderData = [];
        if (File.Exists(orderFile))
        {
            orderData = File.ReadAllLines(orderFile).Where(l => !string.IsNullOrWhiteSpace(l))
                .ToArray();
        }

        // 目录及文件
        var dirsPath = Directory.GetDirectories(directoryPath);
        var filesPath = Directory.GetFiles(directoryPath, "*.md");
        var allPath = dirsPath.Concat(filesPath).ToArray();

        if (allPath.Length > 0)
        {
            var orderedPaths = new List<string>();
            if (orderData.Length > 0)
            {
                foreach (var file in orderData)
                {
                    var path = allPath.FirstOrDefault(p => Path.GetFileNameWithoutExtension(p) == file);
                    if (path != null)
                    {
                        orderedPaths.Add(path);
                    }
                }
                var unorderedFiles = allPath.Except(orderedPaths);
                orderedPaths.AddRange(unorderedFiles);
            }
            else
            {
                orderedPaths = allPath.ToList();
            }

            foreach (string itemPath in orderedPaths)
            {
                if (File.Exists(itemPath))
                {
                    var fileInfo = new FileInfo(itemPath);
                    var fileName = Path.GetFileName(itemPath);
                    var gitAddTime = GetCreatedTime(itemPath);
                    var gitUpdateTime = GetUpdatedTime(itemPath);
                    var doc = new Doc
                    {
                        Title = Path.GetFileNameWithoutExtension(itemPath),
                        FileName = fileName,
                        Path = itemPath,
                        PublishTime = gitUpdateTime ?? gitAddTime ?? fileInfo.LastWriteTime,
                        CreatedTime = gitAddTime ?? fileInfo.CreationTime,
                        UpdatedTime = gitUpdateTime ?? fileInfo.LastWriteTime,
                        Catalog = parentCatalog
                    };

                    doc.HtmlPath = Path.Combine(GetFullPath(parentCatalog), doc.FileName.Replace(".md", ".html"));

                    doc.HtmlPath = doc.HtmlPath.Replace('\\', '/');
                    parentCatalog.Docs.Add(doc);
                }
                else if (Directory.Exists(itemPath))
                {
                    var existMd = Directory.GetFiles(itemPath, "*.md").Length > 0;
                    var existDir = Directory.GetDirectories(itemPath).Length > 0;
                    var name = Path.GetFileName(itemPath);
                    if (existMd || existDir && name != "_images")
                    {
                        var catalog = new Catalog
                        {
                            Name = name,
                            Parent = parentCatalog,
                            Path = itemPath
                        };
                        parentCatalog.Children.Add(catalog);
                        TraverseDirectory(itemPath, catalog);
                    }
                }
            }
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
            foreach (var menu in DocMenus)
            {
                docLinkHtml += $@"<a href=""/docs/{menu.Value}"" class=""block px-4 py-2 text"">{menu.Key}</a>" + Environment.NewLine;
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

    private static void LoadGitHistory(string directoryPath)
    {
        if (_isGitLoaded) return;
        lock (_gitLoadLock)
        {
            if (_isGitLoaded) return;

            try
            {
                // 获取 Git 根目录
                if (!ProcessHelper.RunCommand("git", "rev-parse --show-toplevel", out string gitRoot))
                {
                    _isGitLoaded = true; // Git 不可用，跳过
                    return;
                }
                gitRoot = gitRoot.Trim().Replace('/', Path.DirectorySeparatorChar).Replace('\\', Path.DirectorySeparatorChar);

                var process = new Process();
                process.StartInfo.FileName = "git";
                // 获取所有提交日志，格式：COMMIT_DATE:ISO8601
                // 紧接着是文件状态和路径
                // 增加 -c core.quotepath=false 防止中文路径被转义
                process.StartInfo.Arguments = "-c core.quotepath=false log --name-status --date=iso-strict --format=\"COMMIT_DATE:%ad\"";
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                process.StartInfo.WorkingDirectory = gitRoot; // 在 Git 根目录运行
                process.StartInfo.StandardOutputEncoding = Encoding.UTF8;

                process.Start();

                string? line;
                DateTimeOffset currentCommitDate = DateTimeOffset.MinValue;

                while ((line = process.StandardOutput.ReadLine()) != null)
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;

                    if (line.StartsWith("COMMIT_DATE:"))
                    {
                        if (DateTimeOffset.TryParse(line.Substring(12), out var date))
                        {
                            currentCommitDate = date;
                        }
                        continue;
                    }

                    // 解析状态和路径
                    // 格式: M    path/to/file
                    // 格式: A    path/to/file
                    // 格式: R100 old/path new/path
                    var parts = line.Split('\t');
                    if (parts.Length >= 2)
                    {
                        var status = parts[0][0];
                        var filePath = parts.Last(); // 对于重命名，取新路径

                        // 转换为本地路径
                        var fullPath = Path.GetFullPath(Path.Combine(gitRoot, filePath.Replace('/', Path.DirectorySeparatorChar)));

                        if (!GitTimeCache.TryGetValue(fullPath, out var info))
                        {
                            // 第一次遇到文件（从新到旧），这是最后修改时间
                            info.Updated = currentCommitDate;
                            GitTimeCache[fullPath] = info;
                        }

                        // 如果是添加操作，更新创建时间（从新到旧扫描，越旧的 A 越接近真实创建时间）
                        if (status == 'A')
                        {
                            var currentInfo = GitTimeCache[fullPath];
                            // 只有当 Created 为空时才设置，或者我们总是取最新的 'A'？
                            // git log --diff-filter=A 返回的是包含 A 的 commit。
                            // 如果文件被删除重加，最新的 A 是最近一次添加。
                            // 我们希望 CreatedTime 是最近一次添加的时间。
                            // 因为我们是从新到旧扫描，遇到的第一个 A 就是最近的 A。
                            if (!currentInfo.Created.HasValue)
                            {
                                currentInfo.Created = currentCommitDate;
                                GitTimeCache[fullPath] = currentInfo;
                            }
                        }
                    }
                }
                process.WaitForExit();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Warning] LoadGitHistory failed: {ex.Message}");
            }
            finally
            {
                _isGitLoaded = true;
            }
        }
    }

    private static DateTimeOffset? GetCreatedTime(string path)
    {
        var fullPath = Path.GetFullPath(path);
        if (GitTimeCache.TryGetValue(fullPath, out var info) && info.Created.HasValue)
        {
            return info.Created;
        }

        if (_isGitLoaded) return null;

        if (ProcessHelper.RunCommand("git", @$"log --diff-filter=A --format=%aI -- ""{path}""", out string output))
        {
            output = output.Split("\n").First();
            return ConvertToDateTimeOffset(output);
        }
        return null;
    }

    private static DateTimeOffset? GetUpdatedTime(string path)
    {
        var fullPath = Path.GetFullPath(path);
        if (GitTimeCache.TryGetValue(fullPath, out var info) && info.Updated.HasValue)
        {
            return info.Updated;
        }

        if (_isGitLoaded) return null;

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


    public static string ComputeMD5Hash(string input)
    {
        using (MD5 md5 = MD5.Create())
        {
            byte[] inputBytes = Encoding.UTF8.GetBytes(input);
            byte[] hashBytes = md5.ComputeHash(inputBytes);
            StringBuilder sb = new();
            for (int i = 0; i < hashBytes.Length; i++)
            {
                sb.Append(hashBytes[i].ToString("x2"));
            }
            return sb.ToString();
        }
    }

    [GeneratedRegex(@"^# (.*)$", RegexOptions.Multiline)]
    private static partial Regex TitleRegex();

    private static string NormalizeGitHub(string text)
    {
        if (string.IsNullOrEmpty(text))
            return text;

        var builder = new StringBuilder(text.Length);
        bool previousIsDash = false;
        for (int i = 0; i < text.Length; i++)
        {
            char c = text[i];
            if (c == ' ' || c == '\t' || c == '\n' || c == '\r')
            {
                if (!previousIsDash)
                {
                    builder.Append('-');
                    previousIsDash = true;
                }
            }
            else if (IsValidAnchorChar(c))
            {
                builder.Append(char.ToLowerInvariant(c));
                previousIsDash = false;
            }
        }
        // Remove trailing dash
        if (builder.Length > 0 && builder[builder.Length - 1] == '-')
            builder.Length--;
        return builder.ToString();
    }

    private static bool IsValidAnchorChar(char c)
    {
        return char.IsLetterOrDigit(c) || c == '-' || c == '_';
    }
}
