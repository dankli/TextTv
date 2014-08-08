using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using TextTv.AppContext.Infrastructure.Contracts;

namespace TextTv.PhoneApp.Infrastructure.IO
{
    public class HtmlParserIO : IHtmlParserIO
    {
        private string tvStyles;
        private string webStyles;
        private string template;

        public async Task<string> TvStyles()
        {
            if (string.IsNullOrWhiteSpace(this.tvStyles))
            {
                this.tvStyles = await this.GetStyles("phoneTv.css", "localstyleTv.css");
            }

            return this.tvStyles;
        }

        public async Task<string> WebStyles()
        {
             if (string.IsNullOrWhiteSpace(this.webStyles))
            {
                this.webStyles = await this.GetStyles("phoneWeb.css", "localstyleWeb.css");
            }

            return this.webStyles;
        }

        public async Task<string> GetPageTemplate()
        {
            if (string.IsNullOrWhiteSpace(this.template))
            {
                this.template = await this.GetPageTemplateResource().ConfigureAwait(false); ;
            }

            return this.template;
        }

        private async Task<string> GetPageTemplateResource()
        {
            Assembly assembly = Assembly.Load(new AssemblyName("TextTv.AppContext"));

            using (Stream stream = assembly.GetManifestResourceStream(@"TextTv.AppContext.HtmlResources.pageTemplate.html"))
            using (StreamReader reader = new StreamReader(stream))
            {
                return await reader.ReadToEndAsync();
            }
        }

        private async Task<string> GetStyles(string coreCss, string localStyleCss)
        {
            const string startTag = "<style media='screen' type='text/css'>";
            const string endTag = "</style>";

            string phoneCssContent = await this.GetCssContentFrom(coreCss).ConfigureAwait(false);
            string localStyleCssContent = await this.GetCssContentFrom(localStyleCss).ConfigureAwait(false);

            return
                new StringBuilder().Append(startTag)
                    .Append(phoneCssContent)
                    .Append(localStyleCssContent)
                    .Append(endTag)
                    .ToString();
        }

        private async Task<string> GetCssContentFrom(string css)
        {
            Assembly assembly = Assembly.Load(new AssemblyName("TextTv.AppContext"));

            using (Stream stream = assembly.GetManifestResourceStream(@"TextTv.AppContext.HtmlResources.css." + css))
            using(StreamReader reader = new StreamReader(stream))
            {
                return await reader.ReadToEndAsync();
            }
        }
    }
}
