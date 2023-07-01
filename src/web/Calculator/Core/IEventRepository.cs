namespace FfAdmin.Calculator.Core;

public interface IEventRepository
{
    ValueTask<int> Count();
    ValueTask<Event[]> GetEvents(int start, int? count);
    async ValueTask<Event?> GetEvent(int position)
        => (await GetEvents(position, 1)).FirstOrDefault();
    ValueTask<bool> CanUpdate();

    public static IEventRepository Empty => EmptyImpl.Instance;

    private class EmptyImpl : IEventRepository
    {
        public static EmptyImpl Instance { get; } = new();

        public ValueTask<int> Count()
            => new (0);

        public ValueTask<Event[]> GetEvents(int start, int? count)
            => new(Array.Empty<Event>());

        public ValueTask<bool> CanUpdate()
            => new(false);
    }
}

public static class EventRepositoryExtensions
{
    public static IEventRepository AddEvents(this IEventRepository repo, IEnumerable<Event> events)
        => repo switch
        {
            WithSuffix ws => new WithSuffix(ws._baseRepository, ws._events.AddRange(events)),
            _ => new WithSuffix(repo, events)
        };


    private class WithSuffix : IEventRepository
    {
        internal readonly IEventRepository _baseRepository;
        internal readonly ImmutableList<Event> _events;

        public WithSuffix(IEventRepository baseRepository, IEnumerable<Event> events)
        {
            _baseRepository = baseRepository;
            _events = events.ToImmutableList();
        }

        public async ValueTask<int> Count()
            => await _baseRepository.Count() + _events.Count;

        public async ValueTask<Event[]> GetEvents(int start, int? count)
        {
            var brCount = await _baseRepository.Count();
            if (start >= brCount)
                return (count.HasValue
                        ? _events.Skip(start - brCount).Take(count.Value)
                        : _events.Skip(start - brCount))
                    .ToArray();

            var brEvents = await _baseRepository.GetEvents(start, count);
            if (count.HasValue)
                return brEvents.Length == count.Value
                    ? brEvents
                    : brEvents.Concat(_events.Take(count.Value - brEvents.Length)).ToArray();
            return brEvents.Concat(_events).ToArray();
        }

        public ValueTask<bool> CanUpdate()
            => _baseRepository.CanUpdate();
    }

    public static IEventRepository Prefixed(this IEventRepository repo, int count)
        => repo switch
        {
            Prefix wp => new Prefix(wp._baseRepository, Math.Min(wp._count, count)),
            _ => new Prefix(repo, count)
        };
    private class Prefix : IEventRepository
    {
        internal readonly IEventRepository _baseRepository;
        internal readonly int _count;

        public Prefix(IEventRepository baseRepository, int count)
        {
            _baseRepository = baseRepository;
            _count = count;
        }

        public async ValueTask<int> Count()
            => Math.Min(_count, await _baseRepository.Count());

        public ValueTask<Event[]> GetEvents(int start, int? count)
        {
            count = Math.Min(count ?? int.MaxValue, _count - start);
            return _baseRepository.GetEvents(start, count);
        }

        public ValueTask<bool> CanUpdate()
            => new (false);
    }
}
