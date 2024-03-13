using System.Collections.Generic;
using Player.Domain.Base;

namespace Player.Domain
{
    public class Organization : Entity
    {
        public string Name { get; set; }
        public string Bin { get; set; }
        public string Address { get; set; }
        public string Bank { get; set; }
        public string Iik { get; set; }
        public string Phone { get; set; }
        public ICollection<Client> Clients { get; set; } = new List<Client>();
        public ICollection<Advert> Adverts { get; set; } = new List<Advert>();
    }
}