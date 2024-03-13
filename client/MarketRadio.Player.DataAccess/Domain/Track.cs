using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using MarketRadio.Player.DataAccess.Domain.Base;
using MarketRadio.Player.Helpers;

namespace MarketRadio.Player.DataAccess.Domain
{
    public class Track : Entity
    {
        public required string Type { get; set; }
        public required string Name { get; set; }        
        public required string Hash { get; set; }
        public double Length { get; set; }

        [NotMapped]
        public string UniqueName => $"{Id}__{PathHelper.ToSafeName(Name)}";

        [NotMapped]
        public string FilePath => Path.Combine(DefaultLocations.TracksPath, UniqueName);
        
        public const string Advert = nameof(Advert);
        public const string Music = nameof(Music);
        public const string Silent = nameof(Silent);
    }
}