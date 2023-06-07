namespace FfAdmin.InMemoryDatabase;

public interface IHistoricContext
{
    IContext Current => GetByAge(0);
    IContext Previous => GetByAge(1);
    IContext GetByAge(int age);
}
