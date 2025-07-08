namespace FfAdmin.Calculator.Core;

public partial class EventStream
{
    private class ZeroContext : IContext
    {
        private readonly MetaModels _metaModels;

        public ZeroContext(MetaModels metaModels)
        {
            _metaModels = metaModels;
        }

        public ValueTask<object?> GetContext(Type type)
            => new(_metaModels.Get(type)?.Empty);

        public ValueTask<object?> GetContext(object header, object key)
            => new(_metaModels.Get(header.GetType())?.EmptyDetail);

        public IEnumerable<Type> AvailableContexts => _metaModels.AvailableTypes;
        public ValueTask<IContext> Previous => new(this);
        public Event Event => NoneEvent.Instance;
    }
}
