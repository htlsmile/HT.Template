using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;

namespace HT.Template.BackEnd
{
    public enum EnableDb { LocalDB, PostgreSQL, SQLite }

    public static class Config
    {
        private static IConfigurationRoot GetConfigurationRoot(string filename = "appsettings.json") =>
            new ConfigurationBuilder().AddJsonFile(filename, false, true).Build();

        public static IConfiguration Configuration { get; } = GetConfigurationRoot();

        public static JsonOptions Configure(this JsonOptions options)
        {
            options.JsonSerializerOptions.PropertyNamingPolicy = null;
            return options;
        }

        public static EnableDb EnableDb { get; } = Enum.TryParse<EnableDb>(Configuration[nameof(EnableDb)], out var r) ? r : EnableDb.SQLite;

        public static string GetConnectionString() => Configuration.GetConnectionString($"{Enum.GetName(typeof(EnableDb), EnableDb)}");

        public static DbContextOptions<AppDbContext> DbContextOptions { get; } =
            new DbContextOptionsBuilder<AppDbContext>()
            .Configure()
            .Options as DbContextOptions<AppDbContext>;

        public static string AssemblyName { get; } = typeof(Config).Assembly.GetName().Name;

        public static DbContextOptionsBuilder Configure(this DbContextOptionsBuilder builder) => EnableDb switch
        {
            EnableDb.LocalDB => builder.UseLazyLoadingProxies().UseSqlServer(GetConnectionString(), sql => sql.MigrationsAssembly(AssemblyName)),
            EnableDb.PostgreSQL => builder.UseLazyLoadingProxies().UseNpgsql(GetConnectionString(), sql => sql.MigrationsAssembly(AssemblyName)),
            EnableDb.SQLite => builder.UseLazyLoadingProxies().UseSqlite(GetConnectionString(), sql => sql.MigrationsAssembly(AssemblyName)),
            _ => builder.UseLazyLoadingProxies().UseSqlite(GetConnectionString(), sql => sql.MigrationsAssembly(AssemblyName)),
        };

        public static ModelBuilder Configure(this ModelBuilder builder)
        {
            builder.AddModels().AddComment();
            switch (EnableDb)
            {
                case EnableDb.LocalDB:
                    break;
                case EnableDb.PostgreSQL:
                    builder.HasPostgresExtension("uuid-ossp");//启用Guid主键类型扩展
                    break;
                case EnableDb.SQLite:
                    break;
                default:
                    break;
            }
            return builder;
        }
    }

}