using System.Text.RegularExpressions;

namespace MarketRadio.Player.Helpers
{
    public static partial class PathHelper
    {
        [GeneratedRegex(":|<|>|\"|\\/|\\\\|\\||\\?|\\*")]
        private static partial Regex EscapeRegex();

        public static string ToSafeName(string filename)
        {
            return EscapeRegex().Replace(filename, "");
        }
    }
}