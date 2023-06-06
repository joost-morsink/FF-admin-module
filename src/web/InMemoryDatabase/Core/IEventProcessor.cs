namespace FfAdmin.InMemoryDatabase;

public interface IEventProcessor
{
    object Start { get; }
    object Process(object model, IContext context, Event e);
    IApplyToTypedDictionary GetTypedDictionaryApplicator();
}
