using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace FfAdmin.EventStore.ApiClient;

public class AddEventStoreTokenDelegatingHandler : DelegatingHandler
{
    private readonly IEventStoreTokenProvider _tokenProvider;

    public AddEventStoreTokenDelegatingHandler(IEventStoreTokenProvider tokenProvider)
    {
        _tokenProvider = tokenProvider;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        await _tokenProvider.ApplyTokenTo(request);
        return await base.SendAsync(request, cancellationToken);
    }
}