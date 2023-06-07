namespace FfAdmin.InMemoryDatabase;

public interface IEventProcessor
{
    object Start { get; }
    object Process(object model, IHistoricContext historicContext, Event e);
    IApplyToTypedDictionary GetTypedDictionaryApplicator();
}
