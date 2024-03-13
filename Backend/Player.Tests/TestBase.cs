using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Player.DataAccess;

namespace Player.Tests
{
    public class TestBase
    {
        protected PlayerContext PlayerContext = new PlayerContext();

        public TestBase()
        {
            InitDb();
        }
        
        protected void InitDb()
        {
            var connection = new SqliteConnection("DataSource=:memory:;Foreign Keys=False");
            connection.Open();

            var options = new DbContextOptionsBuilder<PlayerContext>()
                .UseSqlite(connection)
                .Options;

            PlayerContext = new PlayerContext(options);
            PlayerContext.Database.EnsureCreated();
        }
    }
}