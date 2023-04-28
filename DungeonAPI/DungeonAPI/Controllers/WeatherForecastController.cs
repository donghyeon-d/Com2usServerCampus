using DungeonAPI.Services;
using DungeonAPI.Models;
using Microsoft.AspNetCore.Mvc;

namespace DungeonAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class WeatherForecastController : ControllerBase
{
    private static readonly string[] Summaries = new[]
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

    readonly ILogger<WeatherForecastController> _logger;
    readonly IMasterDataDb _masterDataDb;

    public WeatherForecastController(ILogger<WeatherForecastController> logger, IMasterDataDb masterDataDb)
    {
        _logger = logger;
        _masterDataDb = masterDataDb;
    }

    [HttpPost]
    public IEnumerable<WeatherForecast> Get()
    {
        return Enumerable.Range(1, 5).Select(index => new WeatherForecast
        {
            Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            TemperatureC = Random.Shared.Next(-20, 55),
            Summary = Summaries[Random.Shared.Next(Summaries.Length)]
        })
        .ToArray();
    }

    [HttpGet]
    public async Task<Tuple<ErrorCode, MasterData>> Post()
    {
        return await _masterDataDb.Get();
    }
}

