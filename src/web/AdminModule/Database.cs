using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Npgsql;
using Dapper;
using System.Linq;
using FfAdmin.Common;

namespace FfAdmin.AdminModule
{
    [SuppressMessage("ReSharper", "AutoPropertyCanBeMadeGetOnly.Global")]
    public class DatabaseOptions
    {
        public string Server { get; set; } = "localhost";
        public string? Database { get; set; } 
        public string User { get; set; } = "ff";
        public string Password { get; set; } = "notsecret";
    }
    public interface IDatabase
    {
        Task Reset();
        Task<R> Run<R>(Func<NpgsqlConnection, Task<R>> action);
        Task Run(Func<NpgsqlConnection, Task> action);
        IDatabaseOverride? Override { get; }
        Task ApplyOverride(string name);
        Task RemoveOverride();
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
    public class Database : IDatabase, IAsyncDisposable
    {
        private readonly IOptions<DatabaseOptions> _dbOptions;

        public Database(IOptions<DatabaseOptions> dbOptions)
        {
            _dbOptions = dbOptions;
        }

        public async Task<R> Run<R>(Func<NpgsqlConnection, Task<R>> action)
        {
            await using var conn = CreateConnection();
            return await action(conn);
        }

        public async Task Run(Func<NpgsqlConnection, Task> action)
        {
            await using var conn = CreateConnection();
            await action(conn);
        }
        public IDatabaseOverride? Override { get; private set; }
        public async Task ApplyOverride(string name)
        {
            if (Override != null)
                throw new InvalidOperationException();
            Override = await IDatabaseOverride.Create(this, name);
            
        }
        public async Task RemoveOverride()
        {
            var o = Override;
            if (o != null)
            {
                Override = null;
                await o.DisposeAsync();
            }
        }

        private NpgsqlConnection CreateConnection()
        {
            var options = _dbOptions.Value;
            var connectionString = new NpgsqlConnectionStringBuilder
            {
                ApplicationName = "FfAdminWeb",
                Host = options.Server,
                Database = Override?.Name ?? options.Database ?? options.User,
                Username = options.User,
                Password = options.Password,
                LoadTableComposites = true,
                IncludeErrorDetail = true
            };
            var connection = new NpgsqlConnection(connectionString.ConnectionString);
            connection.Open();
            try
            {
                connection.TypeMapper.MapComposite<CoreMessage>("core.message");
                connection.TypeMapper.MapComposite<Audit>("audit.main");
                connection.TypeMapper.MapComposite<AuditFinancial>("audit.financial");
                connection.TypeMapper.MapComposite<AuditTransfers>("audit.transfers");
                connection.TypeMapper.MapComposite<FractionSpec>("core.s_fraction_spec");
            }
            // ReSharper disable once EmptyGeneralCatchClause
            catch { } // Ignore if not found
            return connection;
        }
        public Task Reset()
            => Run(conn =>
            {
                conn.ReloadTypes();
                return Task.CompletedTask;
            });

        public ValueTask DisposeAsync()
            => Override == null ? new() : new(RemoveOverride());
    }
}
