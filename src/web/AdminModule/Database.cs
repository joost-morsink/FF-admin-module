using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Npgsql;

namespace AdminModule
{
    public class DatabaseOptions
    {
        public string Server { get; set; } = "localhost";
        public string User { get; set; } = "ff";
        public string Password { get; set; } = "notsecret";
    }
    public interface IDatabase {
        Task<R> Run<R>(Func<DbConnection, Task<R>> action);
        Task Run(Func<DbConnection, Task> action);
   
    }
    public class Database : IDatabase
    {
        private readonly IOptions<DatabaseOptions> _dbOptions;

        public Database(IOptions<DatabaseOptions> dbOptions)
        {
            _dbOptions = dbOptions;
        }

        public async Task<R> Run<R>(Func<DbConnection, Task<R>> action)
        {
            using var conn = CreateConnection();
            return await action(conn);
        }

        public async Task Run(Func<DbConnection, Task> action)
        {
            using var conn = CreateConnection();
            await action(conn);
        }

        private DbConnection CreateConnection()
        {
            var options = _dbOptions.Value;
            var connectionString = new NpgsqlConnectionStringBuilder()
            {
                ApplicationName = "FfAdminWeb",
                Host = options.Server,
                Username = options.User,
                Password = options.Password
            };
            var connection = new NpgsqlConnection(connectionString.ConnectionString);
            connection.Open();
            return connection;
        }
    }
   
}
