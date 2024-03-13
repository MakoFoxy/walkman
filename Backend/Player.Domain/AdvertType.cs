using Player.Domain.Base;

namespace Player.Domain
{
    public class AdvertType : Entity
    {
        public const string Own = nameof(Own);
        public const string Commercial = nameof(Commercial);
        public const string State = nameof(State);

        public string Name { get; set; }
        public string Code { get; set; }
    }
}