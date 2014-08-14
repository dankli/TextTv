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
            this.languageCode = languageCode.ToLowerInvariant();
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
            using(var stream = Assembly.Load(new AssemblyName("TextTv.Shared"))
                .GetManifestResourceStream(@"TextTv.Shared.Localization." + this.languageCode + ".json"))
            using (StreamReader reader = new StreamReader(stream))
            {
                var content = reader.ReadToEnd();
                this.mapper = JsonConvert.DeserializeObject<Dictionary<string, string>>(content);
            }
        }
    }
}
