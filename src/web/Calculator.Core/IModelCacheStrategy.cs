namespace FfAdmin.Calculator.Core;

public interface IModelCacheStrategy
{
    bool ShouldCache(int[] positions, int count, int position);
    int[] Optimize(int[] positions, int count);
    
    public static IModelCacheStrategy Default { get; } = new DefaultImpl();
    private class DefaultImpl : IModelCacheStrategy
    {
        public bool ShouldCache(int[] positions, int count, int position)
            => positions.Contains(position) || position == count;

        public int[] Optimize(int[] positions, int count)
            => FilterPositions(positions, count).ToArray();

        public IEnumerable<int> FilterPositions(IEnumerable<int> positions, int count)
        {
            const float FACTOR = 1.5f;
            return Inner().Reverse().Where(x => x > 0).Distinct();
            IEnumerable<int> Inner()
            {
                var gap = 50;

                var last = count;
                foreach (var pos in positions.Prepend(0).Reverse())
                {
                    if (pos > last)
                        yield return pos;
                    if (last - pos >= gap)
                    {
                        yield return last;
                        while (last - pos > Convert.ToInt32(gap * FACTOR))
                        {
                            yield return last - gap;
                            last = last - gap;
                            gap = Convert.ToInt32(gap * FACTOR);
                        }
                        last = pos;
                        gap = Convert.ToInt32(gap * FACTOR);
                    }
                }

                yield return last;
            }
        }
    }
}
