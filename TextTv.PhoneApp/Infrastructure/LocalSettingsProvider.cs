using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml.Controls;
using TextTv.AppContext.Infrastructure.Contracts;

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
