using System;
using MarketRadio.SelectionsLoader.Domain;
using Microsoft.EntityFrameworkCore;

namespace MarketRadio.SelectionsLoader.DataAccess
{
    public class DatabaseContext : DbContext
    {
        public DatabaseContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<Track> Tracks { get; set; }
        public DbSet<Genre> Genres { get; set; }
        public DbSet<Selection> Selections { get; set; }
        public DbSet<TrackInSelection> TrackInSelections { get; set; }
        public DbSet<Settings> Settings { get; set; }
        public DbSet<Task> Tasks { get; set; }
        
        public string GetTableName<T>()
        {
            return GetTableName(typeof(T));
        }
        
        public string GetTableName(Type type)
        {
            return Model.FindEntityType(type).GetTableName();
        }
    }
}