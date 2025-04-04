using System;

namespace FfAdmin.ExchangeRate;

public class OpenExchangeRatesOrgOptions
{
    public string AppId { get; set; } = string.Empty;
    public Uri BaseUri { get; set; } = new("https://openexchangerates.org/api/");
}