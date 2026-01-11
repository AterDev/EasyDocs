using Markdig;
using Markdig.Renderers;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;

namespace Share.MarkdownExtension;

public class LinkConvertExtension : IMarkdownExtension
{
    public void Setup(MarkdownPipelineBuilder pipeline)
    {
        pipeline.DocumentProcessed += Pipeline_DocumentProcessed;
    }

    private void Pipeline_DocumentProcessed(MarkdownDocument document)
    {
        foreach (var link in document.Descendants<LinkInline>())
        {
            if (string.IsNullOrEmpty(link.Url)) continue;

            // Skip absolute links (starting with http:// or https://)
            if (link.Url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                link.Url.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            // Handle links with hash fragments (e.g., file.md#section)
            var parts = link.Url.Split('#');
            if (parts[0].EndsWith(".md", StringComparison.OrdinalIgnoreCase))
            {
                parts[0] = parts[0][..^3] + ".html";
                link.Url = string.Join("#", parts);
            }
        }
    }

    public void Setup(MarkdownPipeline pipeline, IMarkdownRenderer renderer)
    {
    }
}
