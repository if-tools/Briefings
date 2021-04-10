using System;
using IFToolsBriefings.Data.Models;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace IFToolsBriefings.Data
{
    public class DatabaseContext : DbContext
    {
        public DbSet<Briefing> Briefings { get; set; }
        
        public DbSet<FileAttachment> Attachments { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var dbPath = Environment.GetEnvironmentVariable("BRIEFINGS_DATABASE_PATH");
            if (dbPath == null) return;
            
            var connectionStringBuilder = new SqliteConnectionStringBuilder
            {
                DataSource = dbPath
            };
            var connectionString = connectionStringBuilder.ToString();
            var connection = new SqliteConnection(connectionString);

            optionsBuilder.UseSqlite(connection);
        }
    }

}