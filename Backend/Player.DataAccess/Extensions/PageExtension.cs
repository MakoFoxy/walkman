using System.Linq;

namespace Player.DataAccess.Extensions
{
    public static class PageExtension
    {
        public static IQueryable<T> GetPagedQuery<T>(this IQueryable<T> query, int? page, int? itemsPerPage)
        {
            if (page.HasValue && itemsPerPage.HasValue)
            {
                query = query.Skip((page.Value - 1) * itemsPerPage.Value).Take(itemsPerPage.Value);
            }

            return query;
        }
    }
}