using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TextTv.AppContext.Infrastructure;
using TextTv.AppContext.Infrastructure.Contracts;
using TextTv.PhoneApp.Infrastructure.IO;

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
