using System.Threading.Tasks;
using External.GiveWp.ApiClient;

namespace FfAdmin.EventImport.Function;

public interface IEventImportService
{
    Task ProcessGiveWpDonations(GiveWpDonation[] donations);
}
