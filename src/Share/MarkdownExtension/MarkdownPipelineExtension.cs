using Markdig;

namespace Share.MarkdownExtension;
public static class MarkdownPipelineExtension
{
    public static MarkdownPipelineBuilder UseBetterCodeBlock(this MarkdownPipelineBuilder pipeline)
    {
        pipeline.Extensions.Add(new CodeBlockExtension());
        return pipeline;
    }
    public static MarkdownPipelineBuilder UseLinkConvert(this MarkdownPipelineBuilder pipeline)
    {
        pipeline.Extensions.Add(new LinkConvertExtension());
        return pipeline;
    }
}
