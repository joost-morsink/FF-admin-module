namespace FfAdmin.InMemoryDatabase;

public interface IApplyToTypedDictionary
{
    TypedDictionary Start(TypedDictionary context);

    TypedDictionary Process(TypedDictionary result, IHistoricContext historicContext, Event e);
}
