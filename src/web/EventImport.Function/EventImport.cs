using System;
using System.Net;
using System.Threading.Tasks;
using External.GiveWp.ApiClient;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

namespace FfAdmin.EventImport.Function;

public class EventImport
{
    private readonly GiveWpClient _giveWp;
    private readonly IEventImportService _eventImportService;

    public EventImport(GiveWpClient giveWp, IEventImportService eventImportService)
    {
        _giveWp = giveWp;
        _eventImportService = eventImportService;
    }

    [Function("ManualImport")]
    public async Task<HttpResponseData> Test(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "import")]
        HttpRequestData request,
        string start,
        FunctionContext executionContext)
    {
        if (!DateOnly.TryParse(start, out var startDate))
            return request.CreateResponse(HttpStatusCode.BadRequest);
        var donations = await _giveWp.GetDonations(startDate);
        await _eventImportService.ProcessGiveWpDonations(donations);
        var response = request.CreateResponse(HttpStatusCode.OK);

        return response;
    }

    [Function("Import")]
    public async Task Import(
        [TimerTrigger("0 0 4 * * *")] TimerInfo timer,
        FunctionContext executionContext)
    {
        var now = DateTimeOffset.UtcNow;
        var startDate = DateOnly.FromDateTime(now.AddDays(-3).Date);

        var donations = await _giveWp.GetDonations(startDate);
        await _eventImportService.ProcessGiveWpDonations(donations);
    }
}
