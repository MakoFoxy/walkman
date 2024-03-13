using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Player.DataAccess;
using Player.Domain;
using Player.Services.Abstractions;

namespace Player.Services
{
    public class UserManager : IUserManager
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly PlayerContext _context;

        private User _currentUser;

        public UserManager(IHttpContextAccessor httpContextAccessor, PlayerContext context)
        {
            _httpContextAccessor = httpContextAccessor;
            _context = context;
        }

        public async Task<User> GetCurrentUser(CancellationToken cancellationToken = default)
        {
            if (_currentUser != null)
            {
                return _currentUser;
            }
            
            var id = GetUserId();
            _currentUser =  await GetUserById(id, cancellationToken);
            return _currentUser;
        }

        public async Task<List<Permission>> GetCurrentUserPermissions(CancellationToken cancellationToken = default)
        {
            if (_currentUser != null)
            {
                return _currentUser.Role.RolePermissions.Select(rp => rp.Permission).ToList();
            }
            
            var user = await GetUserById(GetUserId(), cancellationToken);
            return user.Role.RolePermissions.Select(rp => rp.Permission).ToList();
        }

        public async Task<Organization> GetUserOrganization(CancellationToken cancellationToken = default)
        {
            if (_currentUser == null)
            {
                await GetCurrentUser(cancellationToken);
            }
            
            if (_currentUser != null)
            {
                return await _context.Clients
                    .Where(c => c.UserId == _currentUser.Id)
                    .Select(c => c.Organization)
                    .SingleOrDefaultAsync(cancellationToken);
            }

            return null;
        }
        
        public async Task<ICollection<ObjectInfo>> GetUserObjects(CancellationToken cancellationToken = default)
        {
            if (_currentUser == null)
            {
                await GetCurrentUser(cancellationToken);
            }

            if (_currentUser != null)
            {
                await _context.Entry(_currentUser)
                    .Collection(u => u.Objects)
                    .Query()
                    .Include(uo => uo.Object)
                    .LoadAsync(cancellationToken);
                return _currentUser.Objects.Select(uo => uo.Object).ToList();
            }

            return new List<ObjectInfo>();
        }

        private Task<User> GetUserBy(Expression<Func<User, bool>> predicate, CancellationToken cancellationToken)
        {
            return _context.Users
                .Include(u => u.Role.RolePermissions)
                .ThenInclude(rp => rp.Permission)
                .SingleAsync(predicate, cancellationToken);
        }

        private Guid GetUserId()
        {
            return Guid.Parse(_httpContextAccessor.HttpContext.User.Identity.Name ?? throw new Exception());
        }
        
        private Task<User> GetUserById(Guid id, CancellationToken cancellationToken = default)
        {
            return GetUserBy(u => u.Id == id, cancellationToken);
        }
    }
}
