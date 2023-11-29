using System;
using System.Text.Json.Serialization;

namespace External.GiveWp.ApiClient;

public class GiveWpDonations
{
    [JsonPropertyName("donations")] public GiveWpDonation[] Donations { get; set; } = Array.Empty<GiveWpDonation>();
}