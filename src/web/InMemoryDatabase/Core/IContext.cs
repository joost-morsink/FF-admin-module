namespace FfAdmin.InMemoryDatabase;

public interface IContext
{
    T? GetContextOrNull<T>()
        where T : class;

    T GetContext<T>()
        where T : class;
    object? GetContext(Type type);
    IEnumerable<Type> AvailableContexts { get; }
    void EvaluateAll()
    {
        foreach (var context in AvailableContexts)
            GetContext(context);
    }
}
