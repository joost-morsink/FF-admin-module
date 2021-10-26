using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using FfAdmin.AdminModule;
using FfAdmin.Common;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;

namespace FfAdminWeb.Controllers
{
    [ApiController]
    [Route("admin/database")]
    public class DatabaseController : Controller
    {
        private readonly IDatabase _database;
        private readonly IHostApplicationLifetime _applicationLifetime;

        public DatabaseController(IDatabase database, IHostApplicationLifetime applicationLifetime)
        {
            _database = database;
            _applicationLifetime = applicationLifetime;
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
            return _database.Execute(@"drop schema audit cascade;
                                       drop schema ff cascade;
                                       drop schema core cascade;");
        }
        private async Task RunIdempotentDatabaseScript()
        {
            using var str = typeof(DatabaseController).Assembly.GetManifestResourceStream("FfAdminWeb.database.sql");
            if (str == null)
                throw new NullReferenceException("Resource stream database.sql could not be found.");
            using var rdr = new StreamReader(str);
            var script = await rdr.ReadToEndAsync();
            await _database.Execute(script);
        }
        private void RestartApplication()
        {
            Task.Factory.StartNew(async () =>
            {
                await Task.Delay(TimeSpan.FromMilliseconds(50));
                _applicationLifetime.StopApplication();
            });
        }
        private Task Reset()
            => _database.Reset();
    }
}
