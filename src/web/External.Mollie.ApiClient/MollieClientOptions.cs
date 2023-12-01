using System;

namespace External.Mollie.ApiClient;

public class MollieClientOptions
{
    public Uri BaseUri { get; set; } = new("urn:empty");
    public string Token { get; set; } = "";
}
