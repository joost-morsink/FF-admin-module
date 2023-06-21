using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Data.SqlClient;

namespace FfAdmin.EventStore.Function;

public class EventStore
{
    private readonly IEventStore _eventStore;

     public EventStore(IEventStore eventStore)
     {
         _eventStore = eventStore;
     }
    private class EventDto
    {
        public string Branch { get; set; } = "";
        public int Sequence  { get; set; }
        public string Content { get; set; } = "";
    }

    [Function("GetAllBranches")]
    public async Task<HttpResponseData> GetAllBranches(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "branches")] HttpRequestData req,
        FunctionContext executionContext)
    {
        var response = req.CreateResponse(HttpStatusCode.OK);
        response.Headers.Add("Content-Type","text/plain; charset=utf-8");
        try
        {
            await Task.Yield();
            var branches = await _eventStore.GetBranchNames();
            await response.WriteAsJsonAsync(branches);
            return response;
        }
        catch (Exception e)
        {
            response.WriteString(e.ToString());
            return response;
        }
    }
    [Function("EventStore")]
    public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "test")] HttpRequestData req,
        FunctionContext executionContext)
    {
        var logger = executionContext.GetLogger("EventStore");
        var response = req.CreateResponse(HttpStatusCode.OK);
        response.Headers.Add("Content-Type", "text/plain; charset=utf-8");
        try
        {
            using var sqlConnection =
                new SqlConnection(
                    "Server=tcp:g4g.database.windows.net,1433;Initial Catalog=EventStore;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;Authentication=\"Active Directory Default\"");
            await sqlConnection.OpenAsync();
            var result = await
                sqlConnection.QueryAsync<EventDto>(
                    "select * from [ConsolidatedEvents] where [Branch] = 'Main' order by [Sequence]");


            response.WriteString(string.Join(Environment.NewLine,
                result.Select(e => $"{e.Branch};{e.Sequence};{e.Content}")));

            return response;
        }
        catch (Exception e)
        {
            response.WriteString(e.ToString());
            return response;
        }
    }
}


public class Health
{
    public static string Message { get; set; } = "App is up!";

    [Function("Health")]
    public HttpResponseData Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "health")] HttpRequestData req,
        FunctionContext executionContext)
    {
        var response = req.CreateResponse(HttpStatusCode.OK);
        response.Headers.Add("Content-Type", "text/plain; charset=utf-8");
        response.WriteString(Message);
        
        return response;
    }

}
