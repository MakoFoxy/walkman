using Player.Domain.Base;

namespace Player.Domain
{
    public class ActivityType : Entity
    {
        public string Name { get; set; }
        public string Code { get; set; }


        public const string BusinessCenter = nameof(BusinessCenter);
    }
}