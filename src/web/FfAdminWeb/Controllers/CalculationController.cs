using System;
using System.Threading.Tasks;
using FfAdmin.AdminModule;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using FfAdmin.Common;

namespace FfAdminWeb.Controllers
{
    [ApiController]
    [Route("admin/calculation")]
    public class CalculationController : Controller
    {
        private readonly IAdmin _admin;
        private readonly IOptionRepository _optionRepository;

        public CalculationController(IAdmin admin, IOptionRepository optionRepository)
        {
            _admin = admin;
            _optionRepository = optionRepository;
        }
        [HttpGet("exit")]
        public async Task<ActionResult<decimal>> CalculateExit(string option, decimal? extra_cash, decimal? invested, DateTimeOffset? timestamp)
        {
            if ((await _optionRepository.GetOptions()).All(o => o.Id != option))
                return BadRequest(new ValidationMessage[]
                {
                    new("Option", "Option does not exist")
                });

            return await _admin.CalculateExit(option, extra_cash ?? 0m, invested ?? 0m, timestamp ?? DateTimeOffset.UtcNow);
        }
    }
}
