using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

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
    }
}
