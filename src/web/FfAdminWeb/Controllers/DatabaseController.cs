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
        private readonly IDatabaseRepository _database;

        public DatabaseController(IDatabaseRepository database)
        {
            _database = database;
        }
        [HttpPost("recreate")]
        public async Task<IActionResult> Recreate()
        {
            try
            {
                await _database.DropStructure();
                await _database.UpdateStructure();
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ValidationMessage[]
                {
                    new("Exception", ex.Message)
                });
            }
        }
        [HttpPost("update")]
        public async Task<IActionResult> Update()
        {
            try
            {
                await _database.UpdateStructure();
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ValidationMessage[]
                {
                    new("Exception", ex.Message)
                });
            }
        }
    }
}
