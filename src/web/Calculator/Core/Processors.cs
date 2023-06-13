namespace FfAdmin.Calculator.Core;

public class Processors
{
    public ImmutableList<IEventProcessor> Items { get; }
    
    public static Processors Create(params IEventProcessor[] processors)
        => new(processors.ToImmutableList());
    public static Processors Create(IEnumerable<IEventProcessor> processors)
        => new(processors.ToImmutableList());

    private Processors(ImmutableList<IEventProcessor> items)
    {
        Items = items;
    }
}
