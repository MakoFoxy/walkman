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
    //Класс UserManager, реализующий интерфейс IUserManager, предназначен для управления информацией о пользователях в контексте веб-приложения. Вот его основные функции и методы
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly PlayerContext _context;

        private User _currentUser;

        public UserManager(IHttpContextAccessor httpContextAccessor, PlayerContext context)
        {
            //Конструктор принимает IHttpContextAccessor для доступа к HTTP-контексту (чтобы извлекать данные текущего пользователя) и PlayerContext для доступа к базе данных.
            _httpContextAccessor = httpContextAccessor;
            _context = context;
        }

        public async Task<User> GetCurrentUser(CancellationToken cancellationToken = default)
        //Асинхронно возвращает объект текущего пользователя. Если _currentUser уже определен, метод возвращает его сразу, в противном случае — извлекает пользователя из базы данных по идентификатору, полученному из HTTP-контекста.
        {
            if (_currentUser != null)
            {
                return _currentUser;
            }

            var id = GetUserId();
            _currentUser = await GetUserById(id, cancellationToken);
            return _currentUser;
        }

        public async Task<List<Permission>> GetCurrentUserPermissions(CancellationToken cancellationToken = default)
        { //Возвращает список разрешений для текущего пользователя. Если текущий пользователь уже загружен (_currentUser), возвращает его разрешения напрямую, в противном случае сначала извлекает пользователя.
            if (_currentUser != null)
            {
                return _currentUser.Role.RolePermissions.Select(rp => rp.Permission).ToList();
            }

            var user = await GetUserById(GetUserId(), cancellationToken);
            return user.Role.RolePermissions.Select(rp => rp.Permission).ToList();
        }

        public async Task<Organization> GetUserOrganization(CancellationToken cancellationToken = default)
        { //Асинхронно извлекает организацию, связанную с текущим пользователем. Если пользователь еще не загружен, сначала загружает пользователя.
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
        { //Возвращает коллекцию объектов, связанных с текущим пользователем. Если пользователь еще не загружен, сначала загружает пользователя.
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
        { //GetUserBy(Expression<Func<User, bool>> predicate, CancellationToken cancellationToken): Асинхронно извлекает пользователя из базы данных по предоставленному условию (predicate).
            return _context.Users
                .Include(u => u.Role.RolePermissions)
                .ThenInclude(rp => rp.Permission)
                .SingleAsync(predicate, cancellationToken);
        }

        private Guid GetUserId()
        {//GetUserId(): Возвращает идентификатор текущего пользователя, извлекаемого из HTTP-контекста.
            return Guid.Parse(_httpContextAccessor.HttpContext.User.Identity.Name ?? throw new Exception());
        }

        private Task<User> GetUserById(Guid id, CancellationToken cancellationToken = default)
        { //GetUserById(Guid id, CancellationToken cancellationToken = default): Извлекает пользователя по его ID.
            return GetUserBy(u => u.Id == id, cancellationToken);
        }
    }
    //Этот класс представляет собой централизованный способ управления данными пользователя в приложении, включая извлечение информации о пользователе, его разрешениях и связанных с ним объектах и организациях.
}
