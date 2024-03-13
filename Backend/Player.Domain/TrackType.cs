using Player.Domain.Base;

namespace Player.Domain
{
    public class TrackType : Entity
    {
        public const string Advert = nameof(Advert);
        public const string Music = nameof(Music);
        public const string Silent = nameof(Silent);

        public string Name { get; set; }
        public string Code { get; set; }
    }
}