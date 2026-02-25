using System.Net;
using FfAdmin.Calculator;
using FfAdmin.Calculator.Core;
using FfAdmin.Calculator.Function;
using Microsoft.AspNetCore.Localization;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

namespace FfAdmin.Calculator.Function;

public class OptionWorthsCalculator : BaseCalculator
{
    public OptionWorthsCalculator(CalculatorDependencies dependencies) : base(dependencies) { }

    private OptionWorth GetData(OptionWorths2.Header header)
        => new (header.Id, header.Timestamp, header.Invested, header.Cash, FractionSet.Empty, header.UnenteredDonations);

    [Function("OptionWorths")]
    public Task<HttpResponseData> GetOptionWorths(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "{branchName}/option-worths")]
        HttpRequestData request,
        string branchName,
        FunctionContext executionContext,
        int? at)
        => Handle<OptionWorths2>(request, branchName, at, data => data.Worths.Values.Select(GetData));
    
    [Function("OptionWorthsTheory")]
    public Task<HttpResponseData> PostOptionWorths(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "{branchName}/option-worths")]
        HttpRequestData request,
        string branchName,
        FunctionContext executionContext,
        int? @base)
        => HandlePost<OptionWorths2>(request, branchName, @base, data => data.Worths.Values.Select(GetData));

    [Function("OptionWorthHistory")]
    public Task<HttpResponseData> GetOptionWorthHistory(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "{branchName}/option-worth-history/{id}")]
        HttpRequestData request,
        string branchName,
        FunctionContext executionContext,
        string id,
        int? at)
        => Handle<OptionWorthHistory>(request, branchName, at, data => data.Options[id]);
    
    [Function("OptionWorthHistoryChart")]
    public Task<HttpResponseData> GetOptionWorthHistoryChart(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "{branchName}/option-worth-history-chart/{id}")]
        HttpRequestData request,
        string branchName,
        FunctionContext executionContext,
        string id,
        int? at)
        => GetChart<OptionWorthHistory>(request, branchName, at,
            data => new OptionWorthHistoryCharting()
                .GenerateChartSvg(data.Options[id], "Option Worth History"));

}

public class OptionWorths2Calculator(CalculatorDependencies dependencies) : BaseCalculator(dependencies)
{
    
    [Function("OptionWorths2")]
    public Task<HttpResponseData> GetOptionWorths2(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "{branchName}/option-worths2")]
        HttpRequestData request,
        string branchName,
        FunctionContext executionContext,
        int? at)
        => Handle<OptionWorths2>(request, branchName, at, data => data.Worths);
}
