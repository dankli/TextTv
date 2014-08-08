using Windows.ApplicationModel.Resources;
using TextTv.Shared.Infrastructure.Contracts;

namespace TextTv.PhoneApp.Infrastructure
{
    public class AppResources : IAppResources
    {
        private static readonly ResourceLoader loader = new ResourceLoader();

        public string Get(string resource)
        {
            return loader.GetString(resource);
        }
    }
}
