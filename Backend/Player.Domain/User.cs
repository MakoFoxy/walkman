using System;
using System.Collections.Generic;
using Player.Domain.Base;

namespace Player.Domain
{
    public class User : Entity
    {
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Password { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string SecondName { get; set; }
        public string FullName => $"{LastName} {FirstName[0]}.{SecondName?[0]}.";
        public Role Role { get; set; }
        public Guid? RoleId { get; set; }
        public long? TelegramChatId { get; set; }
        public ICollection<UserObjects> Objects { get; set; } = new List<UserObjects>();

        public const string SystemUserEmail = "system@walkman.org";
    }
}
