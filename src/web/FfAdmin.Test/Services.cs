using System;
using FfAdminWeb.Utils;
using Microsoft.Extensions.DependencyInjection;

namespace FfAdmin.Test
{
    public class Services
    {
        public static Services Instance { get; } = new ();
        private Services()
        {
            var services = new ServiceCollection();
            services.AddFfAdmin(opts => {});
            services.AddScoped<ITemporaryDatabase, TempDatabase>();
            Provider = services.BuildServiceProvider();
        }
        public ServiceProvider Provider { get; }
        public static IServiceScope CreateScope()
            => Instance.Provider.CreateScope();
    }
}
