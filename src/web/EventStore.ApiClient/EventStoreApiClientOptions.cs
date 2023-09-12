using System;

namespace FfAdmin.EventStore.ApiClient;

public class EventStoreApiClientOptions
{
    public Uri BaseUri { get; set; } = new("urn:empty");
    public string ApplicationId { get; set; } = "";
}
