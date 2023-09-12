using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace FfAdmin.ModelCache.ApiClient;

public class AddModelCacheTokenDelegatingHandler : DelegatingHandler
{
    private readonly IModelCacheTokenProvider _tokenProvider;

    public AddModelCacheTokenDelegatingHandler(IModelCacheTokenProvider tokenProvider)
    {
        _tokenProvider = tokenProvider;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        await _tokenProvider.ApplyTokenTo(request);
        return await  base.SendAsync(request, cancellationToken);
    }
}