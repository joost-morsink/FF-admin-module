using System;
using Microsoft.Extensions.DependencyInjection;
namespace FfAdmin.Test
{
    public abstract class ServiceConsumerTest {
        private class NullScope : IServiceScope
        {
            public static NullScope Instance { get; } = new ();
            public void Dispose()
            {
            }
            public IServiceProvider ServiceProvider => NullServiceProvider.Instance;
            private class NullServiceProvider : IServiceProvider
            {
                public static NullServiceProvider Instance { get; } = new ();
                public object? GetService(Type serviceType)
                    => null;
            }
        }
        protected IServiceScope Scope { get; set; } =  NullScope.Instance;
        public T Get<T>()
            where T : notnull
            => Scope.ServiceProvider.GetRequiredService<T>();
    }
}
