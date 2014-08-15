using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;

namespace TextTv.Shared.Infrastructure
{
    public class AppResources
    {
        private Dictionary<string, string> mapper;
        private readonly string languageCode;

        public AppResources(string languageCode)
        {
            this.mapper = new Dictionary<string, string>();
			this.languageCode = languageCode.ToLowerInvariant().Replace('_', '-');
        }

        public string Get(string resource)
        {
            if (this.mapper.Any() == false)
            {
                this.Initialize();
            }

            return this.mapper[resource];
        }

        private void Initialize()
        {
			Assembly assembly = Assembly.Load (new AssemblyName ("TextTv.Shared"));
			var resources = assembly.GetManifestResourceNames ();
			foreach (string resource in resources) {
				string languageFile = this.languageCode + ".json";
				if (resource.EndsWith (languageFile)) {
					using(Stream stream = assembly.GetManifestResourceStream(resource))
					using (StreamReader reader = new StreamReader(stream))
					{
						try{
							var content = reader.ReadToEnd();

							this.mapper = JsonConvert.DeserializeObject<Dictionary<string, string>>(content);
						}
						catch(Exception exception){
							return;
						}
					}

					break;
				}
			}
            
        }
    }
}
