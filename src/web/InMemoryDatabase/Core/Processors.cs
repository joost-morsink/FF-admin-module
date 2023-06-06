namespace FfAdmin.InMemoryDatabase;

public class Processors
{
    public ImmutableList<IEventProcessor> Items { get; }
    public ImmutableList<IApplyToTypedDictionary> Applicators { get; }

    public static Processors Create(params IEventProcessor[] processors)
        => new(processors.ToImmutableList());
    public static Processors Create(IEnumerable<IEventProcessor> processors)
        => new(processors.ToImmutableList());

    private Processors(ImmutableList<IEventProcessor> items)
    {
        Items = items;
        Applicators = items.Select(p => p.GetTypedDictionaryApplicator()).ToImmutableList();
    }
}
