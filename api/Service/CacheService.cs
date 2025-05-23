using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

public interface ICacheService
{
    public Task<T> CachedResponse<T>(Func<Task<T>> repositoryMethod);
    public void ClearCacheKeys(string keyMatchString);
    public List<string> GetCacheKeys();
}

public class CacheService(IHttpContextAccessor httpContextAccessor,IMemoryCache memoryCache) : ICacheService
{
    public List<string> cacheKeys = [];
    public async Task<T> CachedResponse<T>(Func<Task<T>> repositoryMethod)
    {
        if(httpContextAccessor.HttpContext != null)
        {
            //if a GET request (this value is only set on get requests)
            if(httpContextAccessor.HttpContext.Items.TryGetValue("RequestURL", out var requestUrl))
            {   
                string? url = requestUrl?.ToString();
                //if cached
                if(memoryCache.TryGetValue(url, out T cachedData))
                {
                    return cachedData;
                }
                //not cached
                else
                {
                    //make the call, update cache keys
                    var data = await repositoryMethod();
                    memoryCache.Set(url, data);
                    cacheKeys.Add(url);
                    return data;
                }
            }
        }
        return default;
    }

    public void ClearCacheKeys(string keyMatchString)
    {
        var matchingKeys = cacheKeys.Where(key => key.Contains(keyMatchString));
        foreach(var key in matchingKeys)
        {
            memoryCache.Remove(key);
            cacheKeys.Remove(key);
        }
    }

    public List<string> GetCacheKeys()
    {   
        return cacheKeys;
    }
}