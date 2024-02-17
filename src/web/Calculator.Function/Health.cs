using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

namespace FfAdmin.Calculator.Function;

public class Health
{
    [Function("Health")]
    public HttpResponseData GetHealth(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "health")]
        HttpRequestData request,
        FunctionContext executionContext)
    {
        var response = request.CreateResponse(HttpStatusCode.OK);
        return response;
    }
}
