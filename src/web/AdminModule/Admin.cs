using System;
using System.Threading.Tasks;
using Dapper;

namespace FfAdmin.AdminModule
{
    public interface IAdmin
    {
        Task<decimal> CalculateExit(int optionId, decimal currentInvested, DateTimeOffset timestamp);
       
    }
    public class Admin : IAdmin
    {
        private readonly IDatabase _database;

        public Admin(IDatabase database)
        {
            _database = database;
        }
        private class CalculateExitRecord {
            public decimal? Value { get; set; }
        }
        public async Task<decimal> CalculateExit(int optionId, decimal currentInvested, DateTimeOffset timestamp)
        {
            var res = await _database.QueryFirst<CalculateExitRecord>("select * from ff.calculate_exit(@opt, @inv, @time) as value", new
            {
                opt = optionId,
                inv = currentInvested,
                time = timestamp.ToUniversalTime().DateTime
            });
            return res.Value ?? 0.00m;
        }
    }
}
