using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Text;
using HtmlAgilityPack;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TextTv.Tests
{
    [TestClass]
    public class SvtTextReaderTest
    {
        [TestMethod]
        public void ReadPage100()
        {

            var result = CallPage(100);
            Console.WriteLine(result);
        }

        private static string CallPage(int page)
        {
            const string url = @"http://www.svt.se/svttext/web/pages/{0}.html";
            using (HttpClient client = new HttpClient())
            {
                HttpResponseMessage reponse = client.GetAsync(string.Format(url, page)).Result;

                Assert.IsTrue(reponse.IsSuccessStatusCode);

                string content = reponse.Content.ReadAsStringAsync().Result;

                string startTag = "<pre";
                string endTag = "</pre>";
                int startIndex = content.IndexOf("<pre", StringComparison.OrdinalIgnoreCase);
                int endIndex = content.IndexOf("</pre>", StringComparison.OrdinalIgnoreCase);
                int length = endIndex - startIndex;
                var result = content.Substring(startIndex, length + endTag.Length);

                return result;
            }
        }

        [TestMethod]
        public void AddScriptToContent()
        {

    //        <script type="text/javascript">
    //    function SendBlue() {
    //        window.external.notify('blue');
    //    }

    //    function SendGreen() {
    //        window.external.notify('green');
    //    }
    //</script>

            const string startTag = "<script type=\"text/javascript\">";
            const string endTag = "</script>";

            const string functionBody = "function gotToPage(page){window.external.notify(page);}";

            string result = CallPage(100);
            StringBuilder bulder = new StringBuilder().Append(startTag).Append(functionBody).Append(endTag);

            const string hrefTemplate = "<a href='#' onclick='goToPage({0});'>{1}</a>";

            string current = string.Empty;
            
            HtmlDocument document = new HtmlDocument();
            document.LoadHtml(result);

            IEnumerable<HtmlNode> nodes = document.DocumentNode.Descendants("a");

            foreach (var htmlNode in nodes)
            {
                var content = htmlNode.InnerHtml;
                htmlNode.Attributes.Add("onclick", "goToPage("+ content +")");
                htmlNode.Attributes["href"].Value = "#";
            }

            Console.WriteLine(document.DocumentNode.OuterHtml);
            
        }
    }
}
