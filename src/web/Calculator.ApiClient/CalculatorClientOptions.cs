using System;

namespace Calculator.ApiClient;

public class CalculatorClientOptions
{
    public Uri BaseUri { get; set; } = new("urn:empty");
    public TimeSpan Timeout { get; set; } = TimeSpan.FromMinutes(5);
}
