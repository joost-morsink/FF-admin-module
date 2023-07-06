using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

namespace FfAdmin.Calculator.Function;

public class ValidationErrorsCalculator : BaseCalculator
{
    public ValidationErrorsCalculator(CalculatorDependencies dependencies) : base(dependencies) { }
    
    [Function("ValidationErrors")]
    public Task<HttpResponseData> GetValidationErrors(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "{branchName}/validation-errors")]
        HttpRequestData request,
        string branchName,
        FunctionContext executionContext,
        int? at)
        => Handle<ValidationErrors>(request, branchName, at);
    
    [Function("ValidationErrorsTheory")]
    public Task<HttpResponseData> PostValidationErrors(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "{branchName}/validation-errors")]
        HttpRequestData request,
        string branchName,
        FunctionContext executionContext,
        int? @base)
        => HandlePost<ValidationErrors>(request, branchName, @base);
}