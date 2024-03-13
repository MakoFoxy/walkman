using System;

namespace Player.BusinessLogic.Features.Clients.Models
{
    public class ClientModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Bin { get; set; }
        public string LegalAddress { get; set; }
        public string Bank { get; set; }
        public string Iik { get; set; }
        public string FirstPerson { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
    }
}