using System;
using System.IO;

namespace MarketRadio.Player.Helpers
{
    public static class DefaultLocations
    {
        public static string DatabasePath => Path.Combine(BaseLocation, "db");
        public static string DatabaseFileName => "data_db.db";
        
        public static string TracksPath => Path.Combine(BaseLocation, "tracks");
        public static string LogsPath => Path.Combine(BaseLocation, "logs");
        public static string AppLogsPath => Path.Combine(LogsPath, "app");

        public static string BaseLocation => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "player-client");
    }
}