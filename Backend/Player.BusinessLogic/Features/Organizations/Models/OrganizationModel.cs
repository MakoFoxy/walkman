using System;
using System.Collections.Generic;
using Player.DTOs;

namespace Player.BusinessLogic.Features.Organizations.Models
{
    public class OrganizationModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Bin { get; set; }
        public string Address { get; set; }
        public string Bank { get; set; }
        public string Iik { get; set; }
        public string Phone { get; set; }
        public IEnumerable<ClientModel> Clients { get; set; } = new List<ClientModel>();
    
        public class ClientModel
        {
            public Guid Id { get; set; }
            public string FirstName { get; set; }
            public string SecondName { get; set; }
            public string LastName { get; set; }
            public string Email { get; set; }
            public string PhoneNumber { get; set; }
            public SimpleDto Role { get; set; }
            public IEnumerable<SimpleDto> Objects { get; set; } = new List<SimpleDto>();
            public string Password { get; set; }
        }
    }
}