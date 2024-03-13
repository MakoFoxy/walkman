using System;
using System.IO;
using MarketRadio.SelectionsLoader.Services.Abstractions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MarketRadio.SelectionsLoader.Services
{
    public class App : IApp
    {
        public string Version => version;
        public string ProductName => productName;
        public string RunId => runId;

        private static readonly string version;
        private static readonly string productName;
        private static readonly string runId;
        
        static App()
        {
            var manifestJson = File.ReadAllText("electron.manifest.json");
            var jObject = JsonConvert.DeserializeObject<JObject>(manifestJson);
            version = jObject["build"]!["buildVersion"]!.Value<string>();
            productName = jObject["build"]!["productName"]!.Value<string>();
            var now = DateTime.Now;
            
            runId = $"{version}_{now:O}_{Guid.NewGuid()}";
        }
    }
}