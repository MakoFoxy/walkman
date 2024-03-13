using System.IO;
using System.Linq;

namespace MarketRadio.Player.Helpers
{
    public static  class DirectoryInfoExtension
    {
        public static long GetDirectorySize(this DirectoryInfo d)
        {
            return Directory
                .EnumerateFiles(d.FullName, "*.*", SearchOption.AllDirectories)
                .Sum(f => new FileInfo(f).Length);
        }
    }
}