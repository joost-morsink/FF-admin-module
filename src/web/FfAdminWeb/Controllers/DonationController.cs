using System.Linq;
using System.Threading.Tasks;
using FfAdmin.AdminModule;
using Microsoft.AspNetCore.Mvc;

namespace FfAdminWeb.Controllers
{
    [ApiController]
    [Route("admin/donations")]
    public class DonationController
    {
        private readonly IDonationRepository _donationRepository;

        public DonationController(IDonationRepository donationRepository)
        {
            _donationRepository = donationRepository;
        }
        [HttpGet("bycurrency")]
        public async Task<ActionResult<IDonationRepository.DonationAggregation[]>> GetByCurrency()
        {
            var recs = await _donationRepository.GetAggregations();
            return recs.Select(r=> r.Round(2)).ToArray();
        }
    }
}
