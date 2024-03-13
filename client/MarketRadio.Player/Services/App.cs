using System;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
// ReSharper disable InconsistentNaming

namespace MarketRadio.Player.Services
{
    public class App : IApp
    {
        public string Version => version;
        public string ProductName => productName;
        public string RunId => runId;
        public DateTime StartDate => startDate;

        private static readonly string version;
        private static readonly string productName;
        private static readonly string runId;
        private static readonly DateTime startDate;
        
        static App()
        {
            var manifestJson = File.ReadAllText("electron.manifest.json");
            var jObject = JsonConvert.DeserializeObject<JObject>(manifestJson)!;
            version = jObject["build"]!["buildVersion"]!.Value<string>()!;
            productName = jObject["build"]!["productName"]!.Value<string>()!;
            startDate = DateTime.Now;
            
            runId = Guid.NewGuid().ToString();
        }
    }
}