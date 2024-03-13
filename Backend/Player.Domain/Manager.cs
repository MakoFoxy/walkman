using System;
using Player.Domain.Base;

namespace Player.Domain
{
    public class Manager : Entity
    {
        public User User { get; set; }
        public Guid UserId { get; set; }
    }
}