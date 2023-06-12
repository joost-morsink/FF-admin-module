namespace FfAdmin.InMemoryDatabase;

public interface IHistoryCache
{
    object GetAtPosition(int position);
}
