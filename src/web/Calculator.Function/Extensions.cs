using FfAdmin.Calculator.Core;
using Microsoft.Extensions.DependencyInjection;

namespace FfAdmin.Calculator.Function;

public static class Extensions
{
    public static IServiceCollection AddModelProcessor<T>(this IServiceCollection services)
        where T : class, IModel<T>
        => services.AddSingleton<IEventProcessor>(sp => T.GetMetaModel().GetProcessor(sp))
            .AddSingleton<IContext<T>>(_ => IContext<T>.Instance)
            .AddSingleton<IMetaModel>(T.GetMetaModel());
    public static IServiceCollection AddModelProcessor<T,K,D>(this IServiceCollection services)
        where T: class, IModel<T,K,D>
    where K: notnull
    where D:class
        => services.AddSingleton<IContext<T,K,D>>(_ => IContext<T,K,D>.Instance)
            .AddSingleton<IContext<T>>(_ => IContext<T>.Instance)
            .AddSingleton<IEventProcessor>(sp => T.GetMetaModel().GetProcessor(sp))
            .AddSingleton<IMetaModel>(T.GetMetaModel());
    public static IServiceCollection AddProcessor<T>(this IServiceCollection services)
        where T : class, IEventProcessor
        => services.AddSingleton<IEventProcessor, T>();
}
