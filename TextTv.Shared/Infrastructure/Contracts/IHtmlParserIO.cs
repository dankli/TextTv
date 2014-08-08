using System.Threading.Tasks;

namespace TextTv.Shared.Infrastructure.Contracts
{
    public interface IHtmlParserIO
    {
        Task<string> TvStyles();
        Task<string> WebStyles();
        Task<string> GetPageTemplate();
    }
}
