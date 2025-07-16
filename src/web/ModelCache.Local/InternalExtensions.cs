namespace FfAdmin.ModelCache.Local;

internal static class InternalExtensions
{
    public static async IAsyncEnumerable<E> ToAsyncEnumerable<E>(this Task<E[]> task)
    {
        foreach (var x in await task.ConfigureAwait(false))
            yield return x;
    }
    public static async IAsyncEnumerable<T> ToAsyncEnumerable<T>(this Task<T> task)
    {
        yield return await task.ConfigureAwait(false);
    }
}
