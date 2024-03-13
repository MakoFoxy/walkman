using System;
using System.Linq;
using Player.Domain;

namespace Player.DataAccess.Extensions
{
    public static class TrackExtension
    {
        public static IQueryable<Advert> GetValidTracksOn(this PlayerContext context, DateTime date)
        {
            return context.Adverts
                .Where(a => a.AdLifetimes.Any(al => al.DateBegin <= date && al.DateEnd >= date && !al.InArchive) && a.IsValid);
        }
    }
}