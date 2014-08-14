using TextTv.Shared.Infrastructure.Contracts;

namespace TextTv.Shared.Infrastructure
{
    public class HtmlParserFactory : IHtmlParserFactory
    {
        public HtmlParser CreateParser(string rawHtml)
        {
            return new HtmlParser(new HtmlParserIO(), rawHtml);
        }
    }
}
