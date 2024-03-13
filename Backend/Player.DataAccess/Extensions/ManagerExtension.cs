using System.Linq;
using Player.Domain;

namespace Player.DataAccess.Extensions
{
    public static class ManagerExtension
    {
        public static IQueryable<Manager> GetValidManagers(this PlayerContext context)
        {
            return context.Managers
                .Where(a => a.User.Email != User.SystemUserEmail);
        }
    }
}