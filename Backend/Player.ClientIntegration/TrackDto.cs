using System;

namespace Player.ClientIntegration
{
    public class TrackDto
    {
        public Guid Id { get; set; }
        public string Type { get; set; }
        public string Name { get; set; }
        public string Hash { get; set; }
        public double Length { get; set; }

        public DateTime PlayingDateTime { get; set; }
        public string UniqueId => $"{Id}_{PlayingDateTime:HH:mm:ss_dd.MM.yyyy}";
        
        public string UniqueName => $"{Id}__{PathHelper.ToSafeName(Name)}";
    }
}
