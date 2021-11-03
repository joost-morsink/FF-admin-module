using System;
using Microsoft.Extensions.DependencyInjection;

namespace FfAdmin.AdminModule
{
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
            services.AddScoped<IDatabase, Database>();
            if (dbOpts != null)
                services.AddOptions<DatabaseOptions>().Configure(dbOpts);
            return services;
        }
    }
}
