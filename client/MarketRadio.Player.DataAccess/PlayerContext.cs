using System;
using System.Linq;
using MarketRadio.Player.DataAccess.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Object = MarketRadio.Player.DataAccess.Domain.Object;

namespace MarketRadio.Player.DataAccess
{
    public class PlayerContext : DbContext
    {
        public PlayerContext(DbContextOptions options) : base(options)
        {
        }
        
        public DbSet<Object> Objects { get; set; }
        public DbSet<Playlist> Playlists { get; set; }
        public DbSet<Track> Tracks { get; set; }
        public DbSet<ObjectInfo> ObjectInfos { get; set; }
        public DbSet<PendingRequest> PendingRequest { get; set; }
        public DbSet<PlaybackResult> PlaybackResults { get; set; }
        public DbSet<BanList> BanLists { get; set; }
        public DbSet<UserSetting> UserSettings { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ObjectInfo>().OwnsOne(oi => oi.City);
            modelBuilder.Entity<ObjectInfo>().OwnsOne(oi => oi.Settings);
            modelBuilder.Entity<ObjectInfo>().Ignore(oi => oi.FreeDays);

            var dayOfWeekArrayValueComparer = new ValueComparer<DayOfWeek[]>(
                (dw1, dw2) => dw1!.SequenceEqual(dw2!),
                dw => dw.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                dw => dw.ToHashSet().ToArray());

            modelBuilder.Entity<ObjectInfo>().Property(p => p.FreeDays)
                .HasConversion(v => string.Join(',', v),
                    v =>
                        string.IsNullOrEmpty(v)
                            ? Array.Empty<DayOfWeek>()
                            : v.Split(',', StringSplitOptions.None).Select(fd => Enum.Parse<DayOfWeek>(fd, true)).ToArray())
                .Metadata.SetValueComparer(dayOfWeekArrayValueComparer);

            base.OnModelCreating(modelBuilder);
        }
    }
}