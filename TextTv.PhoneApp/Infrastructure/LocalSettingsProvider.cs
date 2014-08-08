using Windows.Storage;
using TextTv.Shared.Infrastructure.Contracts;

namespace TextTv.PhoneApp.Infrastructure
{
    public class LocalSettingsProvider : ILocalSettingsProvider
    {
        public object GetValue(string name)
        {
            return ApplicationData.Current.LocalSettings.Values[name];
        }

        public void SetValue(string name, object value)
        {
            ApplicationData.Current.LocalSettings.Values[name] = value;
        }
    }
}
