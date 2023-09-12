using System;

namespace FfAdmin.ModelCache.ApiClient;

public class ModelCacheApiClientOptions
{
    public Uri BaseUri { get; set; } = new("urn:empty");
    public string ApplicationId { get; set; } = "";
}
