using System;

namespace Player.Domain
{
    public class RolePermissions
    {
        public Role Role { get; set; }
        public Guid RoleId { get; set; }

        public Permission Permission { get; set; }
        public Guid PermissionId { get; set; }
    }
}