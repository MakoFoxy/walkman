using System;

namespace Player.Domain
{
    public class UserObjects
    {
        public User User { get; set; }
        public Guid UserId { get; set; }
        public ObjectInfo Object { get; set; }
        public Guid ObjectId { get; set; }
    }
}