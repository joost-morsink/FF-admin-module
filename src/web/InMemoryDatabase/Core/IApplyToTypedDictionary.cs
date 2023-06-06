namespace FfAdmin.InMemoryDatabase;

public interface IApplyToTypedDictionary
{
    TypedDictionary Start(TypedDictionary context);

    TypedDictionary Process(TypedDictionary result, Func<TypedDictionary> context, IContext previousContext,
        Event e);
}