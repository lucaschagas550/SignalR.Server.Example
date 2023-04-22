using Microsoft.AspNetCore.Mvc;
using SignalR.Example.Model;
using SignalR.Example.WebSockets;
using StackExchange.Redis;
using System.Diagnostics;
using System.Text.Json;

namespace SignalR.Example.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

        private readonly ILogger<WeatherForecastController> _logger;
        private readonly WebSocket _connection;
        private readonly IDatabase _cache;

        public WeatherForecastController(ILogger<WeatherForecastController> logger, WebSocket connection, IDatabase cache)
        {
            _logger = logger;
            _connection = connection;
            _cache = cache;
        }


        [HttpGet("GetKeyValue")]
        public async Task<IActionResult> GetKeyValue(string key)
        {
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var value = await _cache.SetMembersAsync(key);

            var content = value.FirstOrDefault();

            if (content.IsNullOrEmpty)
                return Ok();

            return Ok(JsonSerializer.Deserialize<Person>(content, options));
        }

        [HttpPost]
        public async Task<IActionResult> SetKeyValue(Person person)
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
            };

            var value = await _cache.SetAddAsync(person.Name, JsonSerializer.Serialize(person, options));
            _cache.KeyExpire(person.Name, TimeSpan.FromSeconds(40));


            return Ok();
        }

        [HttpPost("Publish")]
        public async Task<IActionResult> Publish(Person person)
        {
            Console.WriteLine($"Date: {DateTime.Now.ToLocalTime().ToString("dd/MM/yyyy hh:mm:ss.fff")}");
            Debug.WriteLine($"Date: {DateTime.Now.ToLocalTime().ToString("dd/MM/yyyy hh:mm:ss.fff")} \n");

            await _connection.PublishMessageRedis(person);

            return Ok();
        }

        [HttpGet(Name = "GetWeatherForecast")]
        public async Task<IEnumerable<WeatherForecast>> Get()
        {
            await _connection.SendMessage("Lucas", "Test123");

            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToArray();
        }
    }
}