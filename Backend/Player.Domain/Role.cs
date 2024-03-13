using System.Collections.Generic;
using Player.Domain.Base;

namespace Player.Domain
{
    public class Role : Entity
    {
        public string Name { get; set; }
        public bool IsAdminRole { get; set; }
        public ICollection<RolePermissions> RolePermissions { get; set; } = new List<RolePermissions>();
    }
}
