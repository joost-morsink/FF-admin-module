namespace FfAdmin.AdminModule;

public class Context<T> : IContext<T>, IMutableContext<T>
{
    public T Value { get; set; } = default!;
}