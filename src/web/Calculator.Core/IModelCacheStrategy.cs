namespace FfAdmin.Calculator.Core;

public interface IModelCacheStrategy
{
    bool ShouldCache(int[] positions, int count, int position);
    int[] Optimize(int[] positions, int count);
    
    public static IModelCacheStrategy Default { get; } = new DefaultImpl();
    private class DefaultImpl : IModelCacheStrategy
    {
        public bool ShouldCache(int[] positions, int count, int position)
            => position % 100 == 0 || position == count;

        public int[] Optimize(int[] positions, int count)
            => positions.Length == 0
                ? Enumerable.Range(0, int.MaxValue / 100)
                    .Select(x => x * 100)
                    .TakeWhile(x => x < count)
                    .ToArray()
                : positions;
    }
}
