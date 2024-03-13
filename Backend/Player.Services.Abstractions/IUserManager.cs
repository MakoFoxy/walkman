using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Player.Domain;

namespace Player.Services.Abstractions
{
    public interface IUserManager
    {
        Task<User> GetCurrentUser(CancellationToken cancellationToken = default);
        Task<List<Permission>> GetCurrentUserPermissions(CancellationToken cancellationToken = default);
        Task<Organization> GetUserOrganization(CancellationToken cancellationToken = default);
        Task<ICollection<ObjectInfo>> GetUserObjects(CancellationToken cancellationToken = default);
    }
}
