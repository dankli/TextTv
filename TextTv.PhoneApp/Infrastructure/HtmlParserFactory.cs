using TextTv.PhoneApp.Infrastructure.IO;
using TextTv.Shared.Infrastructure;
using TextTv.Shared.Infrastructure.Contracts;

namespace TextTv.PhoneApp.Infrastructure
{
    public class HtmlParserFactory : IHtmlParserFactory
    {
        public HtmlParser CreateParser(string rawHtml)
        {
            return new HtmlParser(new HtmlParserIO(), rawHtml);
        }
    }
}
