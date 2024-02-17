using System;

namespace FfAdminWeb.Middleware;

public interface ILastRequest {
    DateTime Timestamp { get; }
    TimeSpan Age { get; }
    void Now();
}