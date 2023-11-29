using System;

namespace External.GiveWp.ApiClient;

public class GiveWpClientOptions
{
    public Uri BaseUri { get; set; } = new Uri("urn:empty");
    public string ApiKey { get; set; } = string.Empty;
    public string ApiToken { get; set; } = string.Empty;
}