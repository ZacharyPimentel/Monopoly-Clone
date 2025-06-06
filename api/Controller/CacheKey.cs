using Microsoft.AspNetCore.Mvc;
using System.Data;

[ApiController]
[Route("monopoly/cachekey")]
public class CacheKeyController(ICacheService cacheService)
{
    [HttpGet]    
    public ActionResult<List<string>> GetAll()
    {
        return cacheService.GetCacheKeys();
    }
} 