using Controller.Database.Entities;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Controller.Database
{
    public class DatabaseContext : DbContext
    {
        public DbSet<QueryResult> Queries { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder builder)
        {
            var connectionString = new SqliteConnectionStringBuilder()
            {
                Mode = SqliteOpenMode.ReadWriteCreate,
                DataSource = "database.db"

            }.ToString();
            builder.UseSqlite(connectionString);
        }
    }
}
