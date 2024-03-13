using System;
using System.Collections.Generic;
using Player.DTOs;

namespace Player.BusinessLogic.Features.Managers.Models
{
    public class ManagerModel
    {
        public Guid Id { get; set; }
        public string LastName { get; set; }
        public string FirstName { get; set; }
        public string SecondName { get; set; }
        public SimpleDto Role { get; set; }
        public string PhoneNumber { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        public IEnumerable<SimpleDto> Objects { get; set; } = new List<SimpleDto>();
    }
}