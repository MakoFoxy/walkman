using System;
using System.Linq;
using MarketRadio.Player.DataAccess.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Object = MarketRadio.Player.DataAccess.Domain.Object;

namespace MarketRadio.Player.DataAccess
{
    public class PlayerContext : DbContext //Класс наследуется от DbContext, предоставляя функционал для работы с базой данных через Entity Framework.
    {
        public PlayerContext(DbContextOptions options) : base(options)
        {//В конструкторе класс принимает DbContextOptions, которые конфигурируют контекст, например, указывают строку подключения к базе данных.
        }

        public DbSet<Object> Objects { get; set; } //Для каждой сущности, которую необходимо управлять через контекст, определено свойство типа DbSet<T>. Это позволяет выполнять операции создания, чтения, обновления и удаления (CRUD) с этими сущностями.
        public DbSet<Playlist> Playlists { get; set; }
        public DbSet<Track> Tracks { get; set; }
        public DbSet<ObjectInfo> ObjectInfos { get; set; }
        public DbSet<PendingRequest> PendingRequest { get; set; }
        public DbSet<PlaybackResult> PlaybackResults { get; set; }
        public DbSet<BanList> BanLists { get; set; }
        public DbSet<UserSetting> UserSettings { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder) //Метод OnModelCreating используется для настройки модели данных, которая будет отображена на схему базы данных.
        {//modelBuilder.Entity<ObjectInfo>()...: Конфигурация для сущности ObjectInfo включает в себя:
            modelBuilder.Entity<ObjectInfo>().OwnsOne(oi => oi.City); // указывает, что ObjectInfo содержит один объект City как вложенный объект.
            modelBuilder.Entity<ObjectInfo>().OwnsOne(oi => oi.Settings); //указывает, что ObjectInfo содержит настройки как вложенный объект.
            modelBuilder.Entity<ObjectInfo>().Ignore(oi => oi.FreeDays); //игнорирование свойства FreeDays при отображении модели на схему базы данных.

            var dayOfWeekArrayValueComparer = new ValueComparer<DayOfWeek[]>(
                //Конфигурируется специальный сравниватель для свойства FreeDays, чтобы обеспечить корректное сравнение массивов дней недели при определении изменений в свойствах сущности.
                (dw1, dw2) => dw1!.SequenceEqual(dw2!),
                dw => dw.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                dw => dw.ToHashSet().ToArray());

            modelBuilder.Entity<ObjectInfo>().Property(p => p.FreeDays)
            //Преобразование свойства FreeDays с использованием .HasConversion(), чтобы хранить массив DayOfWeek как строку в базе данных и обратно преобразовывать его в массив при чтении. Это необходимо, поскольку в реляционных базах данных нет прямой поддержки массивов.
                .HasConversion(v => string.Join(',', v),
                    v =>
                        string.IsNullOrEmpty(v)
                            ? Array.Empty<DayOfWeek>()
                            : v.Split(',', StringSplitOptions.None).Select(fd => Enum.Parse<DayOfWeek>(fd, true)).ToArray())
                .Metadata.SetValueComparer(dayOfWeekArrayValueComparer);

            base.OnModelCreating(modelBuilder);
            //Класс PlayerContext играет ключевую роль в архитектуре приложения, обеспечивая взаимодействие между объектной моделью приложения и базой данных.
            //Использование DbContext и DbSet<T> упрощает выполнение операций с данными, а настройки в OnModelCreating позволяют детально сконфигурировать отображение объектной модели на схему базы данных, включая специфичные преобразования и поведение для отдельных свойств.
        }
    }
}