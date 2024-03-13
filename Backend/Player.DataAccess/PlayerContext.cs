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
    public class PlayerContext : DbContext
    {
        private IDbContextTransaction _currentTransaction;

        public PlayerContext(DbContextOptions options) : base(options)
        {
        }

        public PlayerContext()
        {
        }

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

        public Task DeletePlaylist(Guid playlistId)
        {
            return Database.ExecuteSqlInterpolatedAsync($"select delete_playlist({playlistId})");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var entityMethod = modelBuilder.GetType().GetMethods()
                .Where(m => m.Name == "Entity")
                .Where(em => em.IsGenericMethod && !em.GetParameters().Any())
                .Single();
            
            var contextAssembly = typeof(PlayerContext).Assembly;
            
            foreach (var type in contextAssembly.GetTypes())
            {
                if (type.GetConstructor(Type.EmptyTypes) == null &&
                    type.GetConstructor(new []{typeof(DbContext)}) == null)
                {
                    continue;
                }

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
                        
                        if (type.GetConstructor(new []{typeof(DbContext)}) != null)
                        {
                            obj = Activator.CreateInstance(type, this);
                        }
                        else
                        {
                            obj = Activator.CreateInstance(type);
                        }

                        var configureMethod = type.GetMethod("Configure");
                        Debug.Assert(configureMethod != null, nameof(configureMethod) + " != null");
                        configureMethod.Invoke(obj, new []{entityTypeBuilder});
                    }
                }
            }
            base.OnModelCreating(modelBuilder);
#if DEBUG
            EntityFrameworkProfiler.Initialize();
#endif
        }
        
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

        private PropertyInfo GetPropertyFromExpression<T>(Expression<Func<T,object>> expression)
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

    public static class QueryableExtensions
    {
        public static string ToSql<TEntity>(this IQueryable<TEntity> query) where TEntity : class
        {
            var enumerator = query.Provider.Execute<IEnumerable<TEntity>>(query.Expression).GetEnumerator();
            var relationalCommandCache = enumerator.Private("_relationalCommandCache");
            var selectExpression = relationalCommandCache.Private<SelectExpression>("_selectExpression");
            var factory = relationalCommandCache.Private<IQuerySqlGeneratorFactory>("_querySqlGeneratorFactory");

            var sqlGenerator = factory.Create();
            var command = sqlGenerator.GetCommand(selectExpression);

            string sql = command.CommandText;
            return sql;
        }

        private static object Private(this object obj, string privateField) => obj?.GetType()
            .GetField(privateField, BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(obj);

        private static T Private<T>(this object obj, string privateField) => (T) obj?.GetType()
            .GetField(privateField, BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(obj);
    }
}
