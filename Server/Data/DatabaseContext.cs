using System;
using IFToolsBriefings.Shared.Data.Models;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;

namespace IFToolsBriefings.Server.Data
{
    public class DatabaseContext : DbContext
    {
        public DbSet<Briefing> Briefings { get; set; }
        public DbSet<FileAttachment> Attachments { get; set; }
        public DbSet<AttachmentsTotalSize> AttachmentsTotalSize { get; set; }

        private static readonly string LocalConnectionString =
            $"mongodb://{Environment.GetEnvironmentVariable("MONGO_DB_USERNAME")}:{Environment.GetEnvironmentVariable("MONGO_DB_PASSWORD")}@{Environment.GetEnvironmentVariable("MONGO_DB_URL")}:{Environment.GetEnvironmentVariable("MONGO_DB_PORT")}";

        private static readonly string ConnectionString =
            Environment.GetEnvironmentVariable("MONGO_DB_CONN_STRING")?.Replace("\"", "") ?? LocalConnectionString;
        
        private static readonly IMongoClient Client = new MongoClient(ConnectionString);
        private static readonly IMongoDatabase _database = Client.GetDatabase(Environment.GetEnvironmentVariable("MONGO_DB_DATABASE"));
        
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseMongoDB(_database.Client, _database.DatabaseNamespace.DatabaseName);
        }
    }

}