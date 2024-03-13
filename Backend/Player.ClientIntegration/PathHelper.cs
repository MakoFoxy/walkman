using System.Text.RegularExpressions;

namespace Player.ClientIntegration
{
    public static class PathHelper
    {
        private static readonly Regex EscapeRegex = new Regex(":|<|>|\"|\\/|\\\\|\\||\\?|\\*", RegexOptions.Compiled);

        public static string ToSafeName(string filename)
        {
            return EscapeRegex.Replace(filename, "");
        }
    }
}