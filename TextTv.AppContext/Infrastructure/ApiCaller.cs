using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using TextTv.AppContext.Model;

namespace TextTv.AppContext.Infrastructure
{
    public class ApiCaller
    {
        public async Task<ResponseResult> GetSvtTextForWeb(int page)
        {
            const string url = @"http://www.svt.se/svttext/web/pages/{0}.html";

            using (HttpClient client = new HttpClient())
            {
                return await GetResponseResult(client, page, url);
            }
        }

        public async Task<ResponseResult> GetSvtTextForTv(int page)
        {
            const string url = @"http://www.svt.se/svttext/tv/pages/{0}.html";

            using (HttpClient client = new HttpClient())
            {
                return await GetResponseResult(client, page, url);
            }
        }

        private static async Task<ResponseResult> GetResponseResult(HttpClient client, int page, string urlFormat)
        {
            SetClientHeaders(client);

            HttpResponseMessage response = await client.GetAsync(string.Format(urlFormat, page));
            response.EnsureSuccessStatusCode();

            ResponseResult responseResult = ReadResponseHeaders(response);

            string content = await ReadContent(response);

            string startTag = "<pre";
            string endTag = "</pre>";
            int startIndex = content.IndexOf(startTag, StringComparison.OrdinalIgnoreCase);
            int endIndex = content.LastIndexOf(endTag, StringComparison.OrdinalIgnoreCase);
            int length = endIndex - startIndex;
            string result = content.Substring(startIndex, length + endTag.Length);
            responseResult.Markup = result;
            return responseResult;
        }

        private static ResponseResult ReadResponseHeaders(HttpResponseMessage response)
        {
            ResponseResult responseResult = new ResponseResult();

            DateTimeOffset? lastModified = response.Headers.Date;
            if (lastModified.HasValue)
            {
                responseResult.Date = lastModified.Value;
            }

            string eTag = response.Headers.ETag.Tag;
            if (string.IsNullOrWhiteSpace(eTag) == false)
            {
                responseResult.ETag = eTag;
            }

            if (response.Content.Headers.LastModified.HasValue)
            {
                responseResult.LastModified = response.Content.Headers.LastModified.Value;
            }
            return responseResult;
        }

        private static void SetClientHeaders(HttpClient client)
        {
            client.DefaultRequestHeaders.CacheControl = new CacheControlHeaderValue
            {
                NoCache = true
            };
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/html"));
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/xhtml+xml"));
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/xml"));
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("image/webp"));
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*"));

            client.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));
            client.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("deflate"));

            client.DefaultRequestHeaders.Pragma.Add(new NameValueHeaderValue("no-cache"));
        }

        private static async Task<string> ReadContent(HttpResponseMessage response)
        {
            string content;
            if (response.Content.Headers.ContentEncoding.Contains("gzip"))
            {
                Stream stream = await response.Content.ReadAsStreamAsync();
                using (GZipStream gzipStream = new GZipStream(stream, CompressionMode.Decompress))
                using (StreamReader reader = new StreamReader(gzipStream))
                {
                    content = reader.ReadToEnd();
                }
            }
            else if (response.Content.Headers.ContentEncoding.Contains("deflate"))
            {
                Stream stream = await response.Content.ReadAsStreamAsync();
                using (DeflateStream gzipStream = new DeflateStream(stream, CompressionMode.Decompress))
                using (StreamReader reader = new StreamReader(gzipStream))
                {
                    content = reader.ReadToEnd();
                }
            }
            else
            {
                content = await response.Content.ReadAsStringAsync();
            }

            return content;
        }
    }
}
