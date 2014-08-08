using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TextTv.AppContext.Infrastructure.Contracts
{
    public interface IHtmlParserFactory
    {
        HtmlParser CreateParser(string rawHtml);
    }
}
