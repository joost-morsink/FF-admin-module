using System;
using System.Net.Http;
using System.Threading.Tasks;
using Azure.Core;

namespace FfAdmin.ModelCache.ApiClient;

public interface IModelCacheTokenProvider
{
    ValueTask<AccessToken> GetTokenAsync();

    async ValueTask ApplyTokenTo(HttpRequestMessage message)
    {
        var token = await GetTokenAsync();
        if(!string.IsNullOrWhiteSpace(token.Token) && token.ExpiresOn>DateTimeOffset.Now)
            message.Headers.Authorization = new("Bearer", token.Token);
    }
}