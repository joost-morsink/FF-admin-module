using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using FfAdmin.AdminModule;
using FfAdmin.EventStore;
using FfAdminWeb.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace FfAdminWeb.Utils
{
    public static class Ext
    {
        internal static JsonSerializerOptions Without(this JsonSerializerOptions options, JsonConverter converter)
        {
            var res = new JsonSerializerOptions(options);
            res.Converters.Remove(converter);
            return res;
        }
        public static async Task<string> ReadFormFile(this IFormFile formFile)
        {
            await using var stream = formFile.OpenReadStream();
            using var reader = new StreamReader(stream);
            var content = await reader.ReadToEndAsync();
            return content;
        }
        public static IServiceCollection AddFfAdmin(this IServiceCollection services, Action<DatabaseOptions> options)
        {
            services.AddSingleton<IEventStore, EventStore>();
            services.AddScoped<IEventingSystem, EventingSystem>();
            services.AddAdminModule(options);

            return services;
        }
    }
}
