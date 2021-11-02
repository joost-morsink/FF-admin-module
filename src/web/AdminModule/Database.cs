using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Npgsql;
using Dapper;
using System.Linq;

namespace FfAdmin.AdminModule
{
    public class DatabaseOptions
    {
        public string Server { get; set; } = "localhost";
        public string User { get; set; } = "ff";
        public string Password { get; set; } = "notsecret";
    }
    public interface IDatabase
    {
        Task Reset();
        Task<R> Run<R>(Func<NpgsqlConnection, Task<R>> action);
        Task Run(Func<NpgsqlConnection, Task> action);

    }
    public static class DatabaseExt
    {
        public static Task<R[]> Query<R>(this IDatabase db, string query, object? param = null)
            => db.Run(async c => (await c.QueryAsync<R>(query, param)).ToArray());
        public static Task<R> QueryFirst<R>(this IDatabase db, string query, object? param = null)
            => db.Run(c => c.QueryFirstAsync<R>(query, param));
        public static Task Execute(this IDatabase db, string query, object? param = null)
            => db.Run(c => c.ExecuteAsync(query, param));
    }
    public class Database : IDatabase
    {
        private readonly IOptions<DatabaseOptions> _dbOptions;

        public Database(IOptions<DatabaseOptions> dbOptions)
        {
            _dbOptions = dbOptions;
        }

        public async Task<R> Run<R>(Func<NpgsqlConnection, Task<R>> action)
        {
            using var conn = CreateConnection();
            return await action(conn);
        }

        public async Task Run(Func<NpgsqlConnection, Task> action)
        {
            using var conn = CreateConnection();
            await action(conn);
        }

        private NpgsqlConnection CreateConnection()
        {
            var options = _dbOptions.Value;
            var connectionString = new NpgsqlConnectionStringBuilder()
            {
                ApplicationName = "FfAdminWeb",
                Host = options.Server,
                Username = options.User,
                Password = options.Password,
                LoadTableComposites = true
            };
            var connection = new NpgsqlConnection(connectionString.ConnectionString);
            connection.Open();
            try
            {
                connection.TypeMapper.MapComposite<CoreMessage>("core.message");
                connection.TypeMapper.MapComposite<Audit>("audit.main");
                connection.TypeMapper.MapComposite<AuditFinancial>("audit.financial");
                connection.TypeMapper.MapComposite<AuditTransfers>("audit.transfers");
            }
            catch { } // Ignore if not found
            return connection;
        }
        public Task Reset()
            => Run(conn =>
            {
                conn.ReloadTypes();
                return Task.CompletedTask;
            });
        
    }

}
