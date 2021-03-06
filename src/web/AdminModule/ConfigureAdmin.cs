using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;

namespace FfAdmin.AdminModule
{
    [SuppressMessage("ReSharper", "UnusedMethodReturnValue.Global")]
    public static class ConfigureAdmin
    {
        public static IServiceCollection AddAdminModule(this IServiceCollection services, Action<DatabaseOptions>? dbOpts = null)
        {
            services.AddScoped<IAdmin, Admin>();
            services.AddScoped<IOptionRepository, OptionRepository>();
            services.AddScoped<ICharityRepository, CharityRepository>();
            services.AddScoped<IEventRepository, EventRepository>();
            services.AddScoped<IAuditRepository, AuditRepository>();
            services.AddScoped<IDonationRepository, DonationRepository>();
            services.AddScoped<IExportRepository, ExportRepository>();
            services.AddScoped<IDatabaseRepository, DatabaseRepository>();
            services.AddScoped<IDatabase, Database>();
            if (dbOpts != null)
                services.AddOptions<DatabaseOptions>().Configure(dbOpts);
            return services;
        }
    }
}
