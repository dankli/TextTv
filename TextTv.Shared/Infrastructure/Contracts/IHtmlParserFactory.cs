namespace TextTv.Shared.Infrastructure.Contracts
{
    public interface IHtmlParserFactory
    {
        HtmlParser CreateParser(string rawHtml);
    }
}
