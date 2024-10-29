using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

namespace FfAdmin.Calculator.Function;

public class MonthlyDonationsCalculator : BaseCalculator
{
    public MonthlyDonationsCalculator(CalculatorDependencies dependencies) : base(dependencies) { }
    
    [Function("MonthlyDonations")]
    public Task<HttpResponseData> GetMonthlyDonations(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "{branchName}/monthly-donations/{option}")]
        HttpRequestData request,
        string branchName,
        string option,
        FunctionContext executionContext,
        int? at)
        => Handle<MonthlyDonations>(request, branchName, at, data => data.Donations);
    
    [Function("MonthlyDonationsTheory")]
    public Task<HttpResponseData> PostMonthlyDonations(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "{branchName}/monthly-donations/{option}")]
        HttpRequestData request,
        string branchName,
        string option,
        FunctionContext executionContext,
        int? @base)
        => HandlePost<MonthlyDonations>(request, branchName, @base, data => data.Donations);
}
