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

        public static string ToSql(this IEnumerable<IExportRepository.ExportRow> rows)
        {
            var date = SqlString(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            return $@"Insert into wp_donation_performance (
                                     Donation_Id,
                                     Donor_Id,
                                     Option_Id,
                                     Charity_Id,
                                     Currency,
                                     Exchanged_Amount,
                                     Has_Entered,
                                     Worth_Amount,
                                     Allocated_Amount,
                                     Transferred_Amount,
                                     Create_DateTime) 
            Values {String.Join("\r\n,", rows.Select(Line))};";

            string Line(IExportRepository.ExportRow row)
            {
                return $@"({row.Donation_id},{row.Donor_id},{row.Option_id},{row.Charity_id},{SqlString(row.Currency)},{SqlNumeric(row.Exchanged_amount)},{row.Has_entered},{SqlNumeric(row.Worth)},{SqlNumeric(row.Allocated)},{SqlNumeric(row.Transferred)},{date})";
            }
            string SqlString(string s) => $"'{s.Replace("'", "''")}'";
            string SqlNumeric(decimal d) => decimal.Round(d,4,MidpointRounding.ToZero).ToString(CultureInfo.InvariantCulture);
        }
        
    }
}
