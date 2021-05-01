using Markdig;

namespace Netyll.Logic.Extensibility
{
    internal class MarkdigMarkdownEngine : ILightweightMarkupEngine
    {
        public string Convert(string source)
        {
            return Markdown.ToHtml(source);
        }
    }
}
