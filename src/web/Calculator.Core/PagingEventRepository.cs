using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace FfAdmin.Calculator.Core;

public class PagingEventRepository : IEventRepository
{
    private readonly int _pagePower;
    private int _mask => (1 << _pagePower) - 1;
    private readonly IEventRepository _innerRepo;
    private readonly string _key;
    private readonly IMemoryCache _cache;
    private readonly ValueTask<int> _initialCount;
    private readonly Task _cleanCache;
    private readonly TimeSpan _cacheExpiration;

    public PagingEventRepository(IEventRepository innerRepo, string key, IMemoryCache cache, IOptions<PagingEventRepositoryOptions> options)
    {
        _innerRepo = innerRepo;
        _initialCount = _innerRepo.Count();
        _pagePower = options.Value.PagePower;
        _cacheExpiration = options.Value.CacheExpiration;
        _key = key;
        _cache = cache;
        _cleanCache = CleanCache();
    }

    private async Task CleanCache()
    {
        var page = await Count() >> _pagePower;
        if (_cache.TryGetValue($"{_key}.{page}", out var pag) && pag is Event[] events) 
        {
            if(events.Length != (await Count() & _mask))
                _cache.Remove($"{_key}.{page}");
        }
        else
        {
            while (!_cache.TryGetValue($"{_key}.{page}", out pag) && page > 0)
                page--;
            if(pag is not null)
                _cache.Remove($"{_key}.{page}");
        }
    }
    public ValueTask<int> Count()
        => _initialCount;

    private async Task<Event[]> GetPage(int page)
        => await _cache.GetOrCreateAsync($"{_key}.{page}", async ce =>
            {
                ce.SetAbsoluteExpiration(_cacheExpiration);
                return await _innerRepo.GetEvents(page << _pagePower, 1 << _pagePower);
            }
        ) ?? Array.Empty<Event>();
    
    public async ValueTask<Event[]> GetEvents(int start, int? count)
    {
        await _cleanCache;
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
            index++;
        }

        return result.ToArray();
        
        bool ShouldContinue()
            => index < total && (!count.HasValue || index < start + count.Value);
    }

    public ValueTask<bool> CanUpdate()
        => _innerRepo.CanUpdate();
}

public class PagingEventRepositoryOptions
{
    public TimeSpan CacheExpiration { get; set; } = TimeSpan.FromMinutes(5);
    public int PagePower { get; set; } = 10;
}
