using Microsoft.Extensions.Caching.Memory;

namespace FfAdmin.Calculator.Core;

public class PagingEventRepository : IEventRepository
{
    private readonly int _pagePower;
    private int _mask => (1 << _pagePower) - 1;
    private readonly IEventRepository _innerRepo;
    private readonly string _key;
    private readonly IMemoryCache _cache;

    public PagingEventRepository(IEventRepository innerRepo, int pagePower, string key, IMemoryCache cache)
    {
        _innerRepo = innerRepo;
        _pagePower = pagePower;
        _key = key;
        _cache = cache;
    }

    public ValueTask<int> Count()
        => _innerRepo.Count();

    private async Task<Event[]> GetPage(int page)
        => await _cache.GetOrCreateAsync($"{_key}.{page}", async ce =>
            {
                ce.SetAbsoluteExpiration(TimeSpan.FromMinutes(5));
                return await _innerRepo.GetEvents(page << _pagePower, 1 << _pagePower);
            }
        ) ?? Array.Empty<Event>();
    
    public async ValueTask<Event[]> GetEvents(int start, int? count)
    {
        var total = await Count();
        var result = new List<Event>();
        int currentPage = start >> _pagePower;
        var page = await GetPage(currentPage);
        var index = start;
        
        while (ShouldContinue())
        {
            if(index >> _pagePower != currentPage)
            {
                currentPage = index >> _pagePower;
                page = await GetPage(currentPage);
            }

            result.Add(page[index & _mask]);
        }

        return result.ToArray();
        
        bool ShouldContinue()
            => index < total && (!count.HasValue || index < start + count.Value);
    }

    public ValueTask<bool> CanUpdate()
        => _innerRepo.CanUpdate();
}
