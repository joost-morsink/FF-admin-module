using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace FfAdmin.AdminModule
{
    public interface IAdmin
    {
        Task<decimal> CalculateExit(string optionId, decimal extracash, decimal currentInvested, DateTimeOffset timestamp);

    }
    public class Admin : IAdmin
    {
        private readonly IDatabase _database;

        public Admin(IDatabase database)
        {
            _database = database;
        }
        [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Local")]
        [SuppressMessage("ReSharper", "ClassNeverInstantiated.Local")]
        private class CalculateExitRecord
        {
            public decimal? Value { get; init; }
        }
        public async Task<decimal> CalculateExit(string optionId, decimal extracash, decimal currentInvested, DateTimeOffset timestamp)
        {
            var res = await _database.QueryFirst<CalculateExitRecord>("select * from ff.calculate_exit(@opt, @cash, @inv, @time) as value", new
            {
                opt = optionId, cash = extracash, inv = currentInvested, time = timestamp.ToUniversalTime().DateTime
            });
            return res.Value ?? 0.00m;
        }
    }
}
