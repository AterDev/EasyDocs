namespace Models;
/// <summary>
/// 文档
/// </summary>
public class Doc
{
    /// <summary>
    /// 标题
    /// </summary>
    public required string Title { get; set; }

    /// <summary>
    /// html路径
    /// </summary>
    public string HtmlPath { get; set; } = string.Empty;

    /// <summary>
    /// 路径
    /// </summary>
    public required string Path { get; set; }

    /// <summary>
    /// 对应html名称
    /// </summary>
    public required string FileName { get; set; }

    /// <summary>
    /// 发布时间
    /// </summary>
    public DateTimeOffset PublishTime { get; set; } = DateTimeOffset.Now;
    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTimeOffset CreatedTime { get; set; } = DateTimeOffset.Now;
    /// <summary>
    /// 更新时间
    /// </summary>
    public DateTimeOffset? UpdatedTime { get; set; }

    /// <summary>
    /// 最近一次提交作者
    /// </summary>
    public string AuthorName { get; set; } = string.Empty;

    public Catalog? Catalog { get; set; }
}
