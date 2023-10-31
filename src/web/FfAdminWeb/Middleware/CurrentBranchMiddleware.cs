using System.Linq;
using System.Threading.Tasks;
using FfAdmin.AdminModule;
using Microsoft.AspNetCore.Http;

namespace FfAdminWeb.Middleware;

public class CurrentBranchMiddleware 
{
    private readonly RequestDelegate _next;

    public CurrentBranchMiddleware(RequestDelegate next)
    {
        _next = next;
    }
    public Task Invoke(HttpContext context, IMutableContext<Branch> branchContext)
    {
        branchContext.Value = context.Request.Headers.TryGetValue("Branch", out var branch)
            ? branch.FirstOrDefault() ?? "Tmp"
            : "Tmp";
        return _next.Invoke(context);
    }
}
