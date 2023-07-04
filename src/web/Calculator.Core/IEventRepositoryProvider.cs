namespace FfAdmin.Calculator.Core;

public interface IEventRepositoryProvider
{
    Task<(string, Lazy<IEventRepository>)[]> GetRepositories();
    Task<IEventRepository?> GetRepository(string name);
}
