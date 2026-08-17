using System.Text.Json.Serialization;

namespace Models;


/// <summary>
/// 分类
/// </summary>
public class Catalog
{
    /// <summary>
    /// 名称
    /// </summary>
    public required string Name { get; set; }

    public required string Path { get; set; }

    public ICollection<Catalog> Children { get; set; } = [];

    public ICollection<Doc> Docs { get; set; } = [];

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? FirstDocHtmlPath { get; set; }

    public Catalog? Parent { get; set; }

    /// <summary>
    /// 获取Catalog所有文档
    /// </summary>
    /// <returns></returns>
    public List<Doc> GetAllDocs()
    {
        var docs = new List<Doc>();

        docs.AddRange(Docs);
        foreach (var catalog in Children)
        {
            docs.AddRange(catalog.GetAllDocs());
        }
        return docs;
    }

    /// <summary>
    /// 查找子Catalog
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public Catalog? FindCatalog(string path)
    {
        if (Path == path)
        {
            return this;
        }
        foreach (var catalog in Children)
        {
            var result = catalog.FindCatalog(path);
            if (result != null)
            {
                return result;
            }
        }
        return null;
    }

}
