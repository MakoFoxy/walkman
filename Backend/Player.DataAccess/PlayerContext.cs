using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using HibernatingRhinos.Profiler.Appender.EntityFramework;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Player.Domain;
using Microsoft.EntityFrameworkCore.Storage;

namespace Player.DataAccess
{
    // Объявление класса PlayerContext, который наследует от DbContext.
    public class PlayerContext : DbContext
    {
        // Поле для хранения текущей транзакции базы данных.
        private IDbContextTransaction _currentTransaction;

        // Конструктор, принимающий параметры для конфигурации контекста.
        public PlayerContext(DbContextOptions options) : base(options)
        {
        }
        // Пустой конструктор.
        public PlayerContext()
        {
        }

        // DbSet для каждой сущности в базе данных, представляющий коллекцию записей.

        //PlayerContext является производным от DbContext классом, который представляет сессию с базой данных, позволяя запрашивать и сохранять данные. Он содержит наборы данных для каждой сущности, с которыми вы работаете (например, Adverts, MusicTracks, Objects и т.д.).
        public DbSet<Advert> Adverts { get; set; }
        public DbSet<MusicTrack> MusicTracks { get; set; }
        public DbSet<ObjectInfo> Objects { get; set; }
        public DbSet<ActivityType> ActivityTypes { get; set; }
        public DbSet<AdHistory> AdHistories { get; set; }
        public DbSet<AdLifetime> AdLifetimes { get; set; }
        public DbSet<AdTime> AdTimes { get; set; }
        public DbSet<AdvertPlaylist> AdvertPlaylists { get; set; }
        public DbSet<Image> Images { get; set; }
        public DbSet<Interval> Intervals { get; set; }
        public DbSet<MusicHistory> MusicHistories { get; set; }
        public DbSet<MusicTrackPlaylist> MusicTrackPlaylists { get; set; }
        public DbSet<Playlist> Playlists { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<ServiceCompany> ServiceCompanies { get; set; }
        public DbSet<TrackType> TrackTypes { get; set; }
        public DbSet<Manager> Managers { get; set; }
        public DbSet<Selection> Selections { get; set; }
        public DbSet<MusicTrackSelection> MusicTrackSelections { get; set; }
        public DbSet<ObjectSelection> ObjectSelections { get; set; }
        public DbSet<MusicTrackGenre> MusicTrackGenres { get; set; }
        public DbSet<Genre> Genres { get; set; }
        public DbSet<CheapPlaylistTemplate> CheapPlaylistTemplates { get; set; }
        public DbSet<Client> Clients { get; set; }
        public DbSet<AdvertType> AdvertTypes { get; set; }
        public DbSet<City> Cities { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Permission> Permissions { get; set; }
        public DbSet<Organization> Organizations { get; set; }
        public DbSet<PlayerTask> Tasks { get; set; }
        public DbSet<BannedMusicInObject> BannedMusicInObject { get; set; }

        // Метод для выполнения хранимой процедуры удаления плейлиста в базе данных.
        public Task DeletePlaylist(Guid playlistId)
        {
            return Database.ExecuteSqlInterpolatedAsync($"select delete_playlist({playlistId})");
        }
        // Метод OnModelCreating используется для настройки моделей при создании контекста.
        //Метод OnModelCreating используется для настройки модели, которая отражает базу данных. Этот метод использует рефлексию для автоматической настройки всех сущностей и их конфигураций, имплементирующих IEntityTypeConfiguration<>, что сокращает количество шаблонного кода и упрощает масштабирование проекта.
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Получение метода Entity<> для дальнейшего использования при конфигурации сущностей.
            var entityMethod = modelBuilder.GetType().GetMethods()
                .Where(m => m.Name == "Entity")
                .Where(em => em.IsGenericMethod && !em.GetParameters().Any())
                .Single();

            var contextAssembly = typeof(PlayerContext).Assembly;
            // Получение всех типов в сборке, где определен PlayerContext, для автоматической настройки с помощью IEntityTypeConfiguration.
            foreach (var type in contextAssembly.GetTypes())
            {
                if (type.GetConstructor(Type.EmptyTypes) == null &&
                    type.GetConstructor(new[] { typeof(DbContext) }) == null)
                {
                    continue;
                }
                // Поиск интерфейсов IEntityTypeConfiguration и их применение.
                foreach (var @interface in type.GetInterfaces())
                {
                    if (!@interface.IsGenericType)
                    {
                        continue;
                    }

                    if (@interface.GetGenericTypeDefinition() == typeof(IEntityTypeConfiguration<>))
                    {
                        var target = entityMethod.MakeGenericMethod(@interface.GenericTypeArguments[0]);
                        var entityTypeBuilder = target.Invoke(modelBuilder, new object[0]);

                        object obj;

                        if (type.GetConstructor(new[] { typeof(DbContext) }) != null)
                        {
                            obj = Activator.CreateInstance(type, this);
                        }
                        else
                        {
                            obj = Activator.CreateInstance(type);
                        }

                        var configureMethod = type.GetMethod("Configure");
                        Debug.Assert(configureMethod != null, nameof(configureMethod) + " != null");
                        configureMethod.Invoke(obj, new[] { entityTypeBuilder });
                    }
                }
            }
            base.OnModelCreating(modelBuilder);
#if DEBUG
            // Инициализация профайлера Entity Framework в режиме отладки.
            EntityFrameworkProfiler.Initialize();
#endif
        }

        // Методы для получения названия таблицы и столбца из модели.
        public string GetTableName<T>()
        {
            return GetTableName(typeof(T));
        }

        public string GetTableName(Type type)
        {
            return Model.FindEntityType(type).GetTableName();
        }

        public string GetColumnName<T>(Expression<Func<T, object>> expression)
        {
            var propertyInfo = GetPropertyFromExpression(expression);
            var entityType = Model.FindEntityType(typeof(T));
            var property = entityType.GetProperties().Where(p => p.PropertyInfo?.MetadataToken == propertyInfo.MetadataToken).First();
            return property.GetColumnName(StoreObjectIdentifier.Table(entityType.GetTableName(), entityType.GetSchema()));
        }
        // Внутренний метод для извлечения PropertyInfo из выражения.
        private PropertyInfo GetPropertyFromExpression<T>(Expression<Func<T, object>> expression)
        {
            MemberExpression exp;

            //this line is necessary, because sometimes the expression comes in as Convert(originalexpression)
            if (expression.Body is UnaryExpression)
            {
                var unExp = (UnaryExpression)expression.Body;
                if (unExp.Operand is MemberExpression)
                {
                    exp = (MemberExpression)unExp.Operand;
                }
                else
                    throw new ArgumentException();
            }
            else if (expression.Body is MemberExpression)
            {
                exp = (MemberExpression)expression.Body;
            }
            else
            {
                throw new ArgumentException();
            }

            return (PropertyInfo)exp.Member;
        }

        #region Transaction Handling

        // Регион с методами для управления транзакциями.
        //В разделе "Transaction Handling" представлены методы для управления транзакциями базы данных. Это включает в себя начало транзакции (BeginTransactionAsync), её подтверждение (CommitTransactionAsync) и откат (RollbackTransactionAsync). Управление транзакциями необходимо для обеспечения целостности данных при выполнении операций, которые должны быть выполнены полностью или вообще не выполняться.

        public async Task BeginTransactionAsync(CancellationToken token = default)
        {
            if (_currentTransaction != null)
            {
                return;
            }

            if (!Database.IsInMemory())
            {
                _currentTransaction = await Database.BeginTransactionAsync(IsolationLevel.ReadCommitted, token);
            }
        }

        public async Task CommitTransactionAsync(CancellationToken token = default)
        {
            try
            {
                if (_currentTransaction != null)
                {
                    await _currentTransaction.CommitAsync(token);
                }
            }
            catch
            {
                await RollbackTransactionAsync(token);
                throw;
            }
            finally
            {
                if (_currentTransaction != null)
                {
                    _currentTransaction.Dispose();
                    _currentTransaction = null;
                }
            }
        }

        public async Task RollbackTransactionAsync(CancellationToken token = default)
        {
            try
            {
                if (_currentTransaction != null)
                {
                    await _currentTransaction.RollbackAsync(token);
                }
            }
            finally
            {
                if (_currentTransaction != null)
                {
                    _currentTransaction.Dispose();
                    _currentTransaction = null;
                }
            }
        }

        #endregion

        // Методы для корректного освобождения ресурсов.
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _currentTransaction?.Rollback();
                _currentTransaction?.Dispose();
            }
        }

        public sealed override void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }


    // Класс расширений для IQueryable, позволяющий преобразовать LINQ-запрос в SQL-запрос.
    public static class QueryableExtensions
    {
        // Метод ToSql преобразует IQueryable в строку SQL.
        //Это класс расширений для IQueryable, который содержит метод ToSql, преобразующий LINQ-запросы в строку SQL. Это может быть полезно для отладки или логирования. Метод использует несколько внутренних и приватных членов Entity Framework для получения итогового SQL запроса, что является продвинутой техникой и может быть полезной для разработчиков, желающих видеть, какой SQL код генерируется из их LINQ запросов.
        public static string ToSql<TEntity>(this IQueryable<TEntity> query) where TEntity : class
        {
            var enumerator = query.Provider.Execute<IEnumerable<TEntity>>(query.Expression).GetEnumerator();
            var relationalCommandCache = enumerator.Private("_relationalCommandCache");
            var selectExpression = relationalCommandCache.Private<SelectExpression>("_selectExpression");
            var factory = relationalCommandCache.Private<IQuerySqlGeneratorFactory>("_querySqlGeneratorFactory");

            // Генерация SQL команды.
            var sqlGenerator = factory.Create();
            var command = sqlGenerator.GetCommand(selectExpression);
            // Возвращение SQL строки.
            string sql = command.CommandText;
            return sql;
        }
        // Вспомогательные методы для доступа к приватным полям.
        private static object Private(this object obj, string privateField) => obj?.GetType()
            .GetField(privateField, BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(obj);

        private static T Private<T>(this object obj, string privateField) => (T)obj?.GetType()
            .GetField(privateField, BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(obj);
    }
}
//Общие моменты:

//     Класс PlayerContext предоставляет основу для взаимодействия с базой данных и определяет, как данные моделируются и сохраняются.
//     DbContextTransactionPipelineBehavior интегрирует управление транзакциями с CQRS паттерном, используя MediatR.
//     QueryableExtensions предоставляет дополнительную функциональность для отладки и разработки, позволяя просматривать сгенерированный SQL из LINQ запросов.

// Эти элементы вместе образуют мощную базу для разработки приложений, использующих Entity Framework Core, CQRS и MediatR, обеспечивая чистоту кода, его расширяемость и легкость поддержки.


// OnModelCreating:

//     Метод OnModelCreating используется в классе DbContext для настройки модели данных через Fluent API. Это включает в себя настройку маппингов сущностей на таблицы базы данных, определение ключей, индексов, отношений между сущностями и других аспектов схемы данных.
//     OnModelCreating вызывается автоматически один раз при инициализации модели Entity Framework Core. Это происходит при первом обращении к контексту в вашем приложении.
//     Изменения, сделанные в OnModelCreating, не применяются автоматически к базе данных. Они определяют, как ваше приложение "видит" схему данных в коде.

// Миграции:

//     Миграции — это механизм, используемый в Entity Framework Core для управления изменениями схемы базы данных. Миграции позволяют вам версионировать схему базы данных и применять изменения к ней, не теряя существующих данных.
//     Когда вы вносите изменения в модель данных (например, добавляете новую сущность или изменяете существующие сущности) и выполняете миграцию, EF Core генерирует код, который изменяет структуру базы данных так, чтобы она соответствовала вашей новой модели.
//     Миграции можно применять и откатывать, что позволяет вам управлять состоянием схемы базы данных в зависимости от версии вашего приложения.

// Таким образом, OnModelCreating определяет, как должна выглядеть модель данных в коде и как она маппится на схему базы данных, но не изменяет саму базу данных. Миграции же используются для изменения схемы базы данных — они "материализуют" изменения, заданные в вашем коде, в самой базе данных.

// Следовательно, OnModelCreating и миграции взаимодополняют друг друга: OnModelCreating задаёт структуру модели, а миграции позволяют изменять структуру базы данных в соответствии с этой моделью.


// Да, ваше понимание абсолютно верное:

//     Маппинг (Настройка в OnModelCreating): Этот процесс включает в себя определение того, как сущности в вашем коде соответствуют таблицам в базе данных, включая как данные должны быть переданы между вашим приложением и базой данных. Это включает в себя настройку отношений между таблицами, определение первичных и внешних ключей, индексов, типов данных столбцов и других аспектов структуры данных. Маппинг определяет "карту" для Entity Framework, используемую для взаимодействия с базой данных, но сам по себе не изменяет структуру базы данных.

//     Миграция: Миграции используются для применения изменений, определенных в процессе маппинга, к самой базе данных. Когда вы вносите изменения в сущности или их конфигурации и затем генерируете и применяете миграцию, Entity Framework создает или обновляет структуру базы данных (таблицы, столбцы, ограничения и так далее) так, чтобы она соответствовала вашей модели данных. Миграции фиксируют изменения в виде версий, которые можно применять, откатывать и управлять ими, что позволяет эффективно поддерживать структуру базы данных в актуальном состоянии.

// Таким образом, маппинг создает связь между вашими сущностями в коде и таблицами в базе данных, а миграции используются для обновления или изменения структуры самой базы данных в соответствии с этими связями и конфигурациями.

// Таким образом, маппинг отвечает за связь и передачу данных между вашим приложением и базой данных, а миграции управляют структурой базы данных и поддерживают её актуальность в соответствии с вашими классами и конфигурациями сущностей.