using System;
using System.Threading.Tasks;
using Azure.Core;
using Azure.Identity;
using Microsoft.Extensions.Options;

namespace FfAdmin.EventStore.ApiClient;

public class EventStoreTokenProvider : IEventStoreTokenProvider
{
    private readonly EventStoreApiClientOptions _options;

    public EventStoreTokenProvider(IOptions<EventStoreApiClientOptions> options)
    {
        _options = options.Value;
    }

    public ValueTask<AccessToken> GetTokenAsync()
    {
        if (!string.IsNullOrWhiteSpace(_options.ApplicationId))
            return new DefaultAzureCredential().GetTokenAsync(
                new TokenRequestContext(new[] {_options.ApplicationId}));
        return new ValueTask<AccessToken>(new AccessToken("", DateTimeOffset.MinValue));
    }
}