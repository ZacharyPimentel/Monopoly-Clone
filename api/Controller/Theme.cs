using System.Data;
using Dapper;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("/api/theme")]
public class ThemeController(IThemeRepository themeRepository, ICacheService cacheService) : ControllerBase {

    [HttpGet]
    public async Task<ActionResult<List<Theme>>> GetAll(){
        var themes = await cacheService.CachedResponse(themeRepository.GetAll);
        return Ok(themes);
    }
}