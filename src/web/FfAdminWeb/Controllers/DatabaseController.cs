using System;
using System.IO;
using System.Threading.Tasks;
using FfAdmin.AdminModule;
using FfAdmin.Common;
using Microsoft.AspNetCore.Mvc;

namespace FfAdminWeb.Controllers
{
    [ApiController]
    [Route("admin/database")]
    public class DatabaseController : Controller
    {
        private readonly IDatabase _database;

        public DatabaseController(IDatabase database)
        {
            _database = database;
        }
        [HttpPost("recreate")]
        public async Task<IActionResult> Recreate()
        {
            try
            {
                await DropDatabase();
                await RunIdempotentDatabaseScript();
                await Reset();
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ValidationMessage[] { new("Exception", ex.Message) });
            }
        }
        [HttpPost("update")]
        public async Task<IActionResult> Update()
        {
            try
            {
                await RunIdempotentDatabaseScript();
                await Reset();
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ValidationMessage[] { new("Exception", ex.Message) });
            }
        }
        private Task DropDatabase()
        {
            return _database.Execute(@"drop schema if exists audit cascade;
                                       drop schema if exists ff cascade;
                                       drop schema if exists core cascade;");
        }
        private async Task RunIdempotentDatabaseScript()
        {
            await using var str = typeof(DatabaseController).Assembly.GetManifestResourceStream("FfAdminWeb.database.sql");
            if (str == null)
                throw new NullReferenceException("Resource stream database.sql could not be found.");
            using var rdr = new StreamReader(str);
            var script = await rdr.ReadToEndAsync();
            await _database.Execute(script);
        }
        private Task Reset()
            => _database.Reset();
    }
}
