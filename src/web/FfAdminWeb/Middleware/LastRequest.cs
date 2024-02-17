using System;

namespace FfAdminWeb.Middleware;

public class LastRequest : ILastRequest {
    public DateTime Timestamp { get; private set; }
    public TimeSpan Age => DateTime.UtcNow - Timestamp;
    public void Now() => Timestamp = DateTime.UtcNow;
}
