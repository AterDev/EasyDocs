namespace Models;
/// <summary>
/// 站点信息
/// </summary>
public class WebInfo
{
    /// <summary>
    /// 网站名称
    /// </summary>
    public string Name { get; set; } = BlogConst.BlogName;
    /// <summary>
    /// 网站说明
    /// </summary>
    public string Description { get; set; } = BlogConst.BlogDescription;

    /// <summary>
    /// 作者名称
    /// </summary>
    public string AuthorName { get; set; } = "Ater";
    /// <summary>
    /// 内容目录
    /// </summary>
    public string ContetPath { get; set; } = "./Content";

    /// <summary>
    /// 输出目录
    /// </summary>
    public string OutputPath { get; set; } = "./WebSite";

    /// <summary>
    /// 图标
    /// </summary>
    public string? Icon { get; set; }

    /// <summary>
    /// logo
    /// </summary>
    public string? Logo { get; set; }

    /// <summary>
    /// 子目录，无则保持"/"
    /// </summary>
    public string BaseHref { get; set; } = "/";

    /// <summary>
    /// 部署时的域名,用于生成 sitemap.xml
    /// 例如:https://aterdev.github.io或https://blog.dusi.dev
    /// </summary>
    public string? Domain { get; set; }

    /// <summary>
    /// 仓库地址，用于生成编辑链接
    /// </summary>
    public string? RepositoryUrl { get; set; }

    /// <summary>
    /// 分支名称，默认为 main
    /// </summary>
    public string Branch { get; set; } = "main";

    public List<DocInfo> DocInfos { get; set; } = [];
}
/// <summary>
/// 文档信息
/// </summary>
public class DocInfo
{
    public required string Name { get; set; }

    public string Logo { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// 包含的语言
    /// </summary>
    public string[] Languages { get; set; } = ["en-us"];
    /// <summary>
    /// 包含的版本
    /// </summary>
    public string[] Versions { get; set; } = ["1.0"];
}

public class DocVersion
{
    public required string Version { get; set; }

}
