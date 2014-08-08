using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;
using TextTv.AppContext.Infrastructure.Contracts;

namespace TextTv.AppContext.Infrastructure
{
    public class HtmlParser
    {
        private readonly IHtmlParserIO _htmlParserIo;
        private readonly string _rawHtml;

        public HtmlParser(IHtmlParserIO htmlParserIo, string rawHtml)
        {
            _htmlParserIo = htmlParserIo;
            _rawHtml = rawHtml;
        }

        public async Task<string> ParseForTvAsync()
        {
            string style = await _htmlParserIo.TvStyles().ConfigureAwait(false);
            return await this.ParseAsync(style);
        }

        public async Task<string> ParseForWebAsync()
        {
            string style = await _htmlParserIo.WebStyles().ConfigureAwait(false);
            return await this.ParseAsync(style);
        }

        private async Task<string> ParseAsync(string styles)
        {
            string template = await _htmlParserIo.GetPageTemplate().ConfigureAwait(false);
            template = template.Replace("%%styles%%", styles);
            template = template.Replace("%%content%%", _rawHtml);
            
            const string startTag = "<script type=\"text/javascript\">";
            const string endTag = "</script>";
            const string functionBody = "function goToPage(page){window.external.notify(page);}";

            StringBuilder builder = new StringBuilder().Append(startTag).Append(functionBody).Append(endTag);
            template = template.Replace("%%scripts%%", builder.ToString());

            HtmlDocument document = new HtmlDocument();
            document.LoadHtml(template);

            var linkNodes = document.DocumentNode.Descendants("a").ToList();
            if (linkNodes.Any())
            {
                foreach (var htmlNode in linkNodes)
                {
                    if (htmlNode.Attributes.Contains("href"))
                    {
                        htmlNode.Attributes.Add("onclick", "goToPage('" + htmlNode.InnerHtml + "')");
                        htmlNode.Attributes["href"].Value = "#";
                    }
                }
            }

            List<HtmlNode> spanNodes = document.DocumentNode.Descendants("span").ToList();

            foreach (HtmlNode spanNode in spanNodes)
            {
                if (spanNode.Attributes.Contains("style"))
                {
                    var style = spanNode.Attributes["style"].Value;
                    if (style.Contains("background"))
                    {
                        style = style.Replace("../../", "http://www.svt.se/svttext/");
                        spanNode.Attributes["style"].Value = style;
                    }
                }
            }


            template = document.DocumentNode.OuterHtml;

            return template;
        }
    }
}
