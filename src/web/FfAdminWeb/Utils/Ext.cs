using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using FfAdmin.AdminModule;
using FfAdmin.EventStore;
using FfAdminWeb.Middleware;
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
        public static IServiceCollection AddFfAdmin(this IServiceCollection services)
        {
            services.AddAdminModule();

            return services;
        }

        public static IServiceCollection AddMiddlewares(this IServiceCollection services)
        {
            return services.AddSingleton<ILastRequest, LastRequest>()
                .AddSingleton<LastRequestMiddleware>()
                .AddSingleton<CurrentBranchMiddleware>();
        }
    }
}
