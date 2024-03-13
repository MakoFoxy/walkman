using System;
using System.IO;

namespace MarketRadio.SelectionsLoader.Helpers
{
    public static class DefaultLocations
    {
        public static string DatabasePath => Path.Combine(BaseLocation, "db");
        
        public static string TracksPath => Path.Combine(BaseLocation, "tracks");
        public static string LogsPath => Path.Combine(BaseLocation, "logs");

        public static string BaseLocation => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "selections-loader");
    }
}