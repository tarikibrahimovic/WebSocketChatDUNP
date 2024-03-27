using Microsoft.AspNetCore.Mvc;

namespace WebSocketChatDUNP.Controllers
{
    [ApiController]
    [Route("ws")]
    public class WeatherForecastController : ControllerBase
    {

        //[HttpGet]
        //public IActionResult Get(HttpClient)
        //{
        //    return Enumerable.Range(1, 5).Select(index => new WeatherForecast
        //    {
        //        Date = DateTime.Now.AddDays(index),
        //        TemperatureC = Random.Shared.Next(-20, 55),
        //        Summary = Summaries[Random.Shared.Next(Summaries.Length)]
        //    })
        //    .ToArray();
        //}
    }
}