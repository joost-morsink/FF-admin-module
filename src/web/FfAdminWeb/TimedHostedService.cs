using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FfAdmin.Common;
using FfAdminWeb.Middleware;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FfAdminWeb;

public class TimedHostedService : IHostedService, IDisposable
{
    private int executionCount = 0;
    private readonly ILogger<TimedHostedService> _logger;
    private Timer? _timer = null;
    private readonly IServiceProvider _provider;

    public TimedHostedService(IServiceProvider provider, ILogger<TimedHostedService> logger)
    {
        _logger = logger;
        _provider = provider;
    }

    public Task StartAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Timed Hosted Service running.");

        _timer = new Timer(DoWork, null, TimeSpan.Zero,
            TimeSpan.FromMinutes(1));

        return Task.CompletedTask;
    }

    private async void DoWork(object? state)
    {
        using var scope = _provider.CreateScope();
        var lastRequest = scope.ServiceProvider.GetRequiredService<ILastRequest>();
        if (lastRequest.Age > TimeSpan.FromMinutes(10))
            return;
        
        var count = Interlocked.Increment(ref executionCount);

        _logger.LogInformation(
            "Timed Hosted Service is working. Count: {Count}", count);
        
        var checks = scope.ServiceProvider.GetServices<ICheckOnline>();
        await Task.WhenAll(checks.Select(check => check.IsOnline()));
    }

    public Task StopAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Timed Hosted Service is stopping.");

        _timer?.Change(Timeout.Infinite, 0);

        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _timer?.Dispose();
    }
}
