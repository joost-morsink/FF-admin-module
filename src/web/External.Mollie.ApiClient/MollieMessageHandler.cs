using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

namespace External.Mollie.ApiClient;

public class MollieMessageHandler : DelegatingHandler
{
    private readonly MollieClientOptions _options;

    public MollieMessageHandler(IOptions<MollieClientOptions> options)
    {
        _options = options.Value;
    }
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _options.Token);
        return base.SendAsync(request, cancellationToken);
    }
}
