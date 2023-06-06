namespace FfAdmin.InMemoryDatabase;

public interface IContext
{
    T? GetContext<T>()
        where T : class;
    object? GetContext(Type type);
    IEnumerable<Type> AvailableContexts { get; }
}
