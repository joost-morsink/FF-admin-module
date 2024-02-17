using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using External.GiveWp.ApiClient;
using FfAdmin.Common;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

namespace FfAdmin.EventImport.Function;

public class EventImport
{
    private readonly GiveWpClient _giveWp;
    private readonly IEventImportService _eventImportService;
    private readonly IEnumerable<ICheckOnline> _onlineChecks;

    public EventImport(GiveWpClient giveWp, IEventImportService eventImportService, IEnumerable<ICheckOnline> onlineChecks)
    {
        _giveWp = giveWp;
        _eventImportService = eventImportService;
        _onlineChecks = onlineChecks;
    }
    private async Task<bool> IsOnline()
    {
        var onlines = await Task.WhenAll(_onlineChecks.Select(check => check.IsOnline()));
        return onlines.All(x => x);
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
        if (!await IsOnline())
            return request.CreateResponse(HttpStatusCode.ServiceUnavailable);
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
        if (!await IsOnline())
            throw new UnavailableDependencyException();

        var now = DateTimeOffset.UtcNow;
        var startDate = DateOnly.FromDateTime(now.AddDays(-3).Date);

        var donations = await _giveWp.GetDonations(startDate);
        await _eventImportService.ProcessGiveWpDonations(donations);
    }

    [Function("MonthlyImport")]
    public async Task MonthlyImport(
        [TimerTrigger("0 2 4 28 * *")] TimerInfo timer,
        FunctionContext executionContext)
    {
        if (!await IsOnline())
            throw new UnavailableDependencyException();

        var now = DateTimeOffset.UtcNow;
        var startDate = DateOnly.FromDateTime(now.AddMonths(-1).Date);
        
        var donations = await _giveWp.GetDonations(startDate);
        await _eventImportService.ProcessGiveWpDonations(donations);
    }
}

public class UnavailableDependencyException : Exception
{
}
