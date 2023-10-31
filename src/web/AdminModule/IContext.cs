namespace FfAdmin.AdminModule;

public interface IContext<T>
{
    T Value { get; }
}
