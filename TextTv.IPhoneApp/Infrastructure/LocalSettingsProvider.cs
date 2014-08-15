using System;
using TextTv.Shared.Infrastructure.Contracts;
using MonoTouch.Foundation;

namespace TextTv.IPhoneApp.Infrastructure
{
	public class LocalSettingsProvider: ILocalSettingsProvider
	{
		private NSUserDefaults defaults;

		public LocalSettingsProvider ()
		{
			this.defaults = NSUserDefaults.StandardUserDefaults;
		}

		#region ILocalSettingsProvider implementation

		public object GetValue (string name)
		{
			string value = this.defaults.StringForKey (name);
			return value;
		}

		public void SetValue (string name, object value)
		{
			string strValue = value.ToString ();
			this.defaults.SetString (name, strValue);
			this.defaults.Synchronize ();
		}

		#endregion
	}
}

