namespace FfAdmin.Calculator.Core;

public partial class EventStream
{
    private class ZeroContext : IContext
    {
        private ImmutableArray<IEventProcessor> _processors;

        public ZeroContext(ImmutableArray<IEventProcessor> processors)
        {
            _processors = processors;
        }

        public T? GetContextOrNull<T>() where T : class
            => _processors.OfType<IEventProcessor<T>>().Select(p => p.Start).FirstOrDefault();

        public T GetContext<T>() where T : class
            => GetContextOrNull<T>() ?? throw new ArgumentException($"EventProcessor for {typeof(T)} not found");

        public object? GetContext(Type type)
            => _processors.Where(p => p.ModelType == type).Select(p => p.Start).FirstOrDefault();

        public IEnumerable<Type> AvailableContexts => _processors.Select(p => p.ModelType);
        public IContext Previous => this;
        public Event Event => NoneEvent.Instance;
    }
}
