using FfAdmin.Calculator.Core;
using Microsoft.Extensions.DependencyInjection;

namespace FfAdmin.Calculator.Function;

public static class Extensions
{
    public static IServiceCollection AddModelProcessor<T>(this IServiceCollection services)
        where T : class, IModel<T>
        => services.AddSingleton<IEventProcessor>(T.Processor);

    public static IServiceCollection AddProcessor<T>(this IServiceCollection services)
        where T : class, IEventProcessor
        => services.AddSingleton<IEventProcessor, T>();
}
