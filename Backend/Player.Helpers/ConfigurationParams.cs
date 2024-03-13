using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace Player.Helpers
{
    public static class ConfigurationParams
    {
        public static void Log(ILogger logger, IEnumerable<KeyValuePair<string, string>> configs, string prefix = "Player")
        {
            var skipConfigs = new[] {"PASSWORD", "TOKEN", "JWT",};
            
            var pairs = configs.Where(pair => !string.IsNullOrEmpty(pair.Value))
                .Where(pair => pair.Key != null && !skipConfigs.Any(sc => pair.Key.ToUpperInvariant().Contains(sc)))
                .Where(pair => pair.Value != null && !skipConfigs.Any(sc => pair.Value.ToUpperInvariant().Contains(sc)));

            if (!string.IsNullOrWhiteSpace(prefix))
            {
                pairs = pairs.Where(pair => pair.Key.ToUpperInvariant().StartsWith(prefix.ToUpperInvariant()));
            }

            var serviceConfig = string.Join(Environment.NewLine, pairs.Select(p => $"{p.Key}: {p.Value}"));
            logger.LogInformation("App running with config with params: \n{ServiceConfig}", serviceConfig);
        }
    }
}