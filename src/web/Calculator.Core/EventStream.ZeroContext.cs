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
        
        public ValueTask<object?> GetContext(Type type)
            => new (_processors.Where(p => p.ModelType == type).Select(p => p.Start).FirstOrDefault());

        public IEnumerable<Type> AvailableContexts => _processors.Select(p => p.ModelType);
        public IContext Previous => this;
        public Event Event => NoneEvent.Instance;
    }
}
