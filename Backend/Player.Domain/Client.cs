using System;
using Player.Domain.Base;

namespace Player.Domain
{
    public class Client : Entity
    {
        public User User { get; set; }
        public Guid UserId { get; set; }
        public Organization Organization { get; set; }
        public Guid OrganizationId { get; set; }
    }
}
