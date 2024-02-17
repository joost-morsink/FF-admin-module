using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace FfAdminWeb.Middleware;

public class LastRequestMiddleware
{
    private readonly RequestDelegate _next;
    
    public LastRequestMiddleware(RequestDelegate next)
    {
        _next = next;
    }
    public Task Invoke(HttpContext context, ILastRequest lastRequest)
    {
        lastRequest.Now();
        return _next.Invoke(context);
    }
}