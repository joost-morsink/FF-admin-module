using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

namespace External.GiveWp.ApiClient;

internal class GiveWpMessageHandler : DelegatingHandler
{
    private readonly GiveWpClientOptions _options;

    public GiveWpMessageHandler(IOptions<GiveWpClientOptions> options)
    {
        _options = options.Value;
    }
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        UriBuilder builder = new UriBuilder(request.RequestUri!);
        builder.Query = (builder.Query.Length==0 ? "?" : builder.Query+"&") + $"key={_options.ApiKey}&token={_options.ApiToken}";
        request.RequestUri = builder.Uri;
        return base.SendAsync(request, cancellationToken);
    }
}