using System.ComponentModel.DataAnnotations;

namespace ASP.NET.WebAPI.Models
{
    public class WeatherForecast
    {
        [Required]
        public DateOnly Date { get; set; }
        [Required]
        public int TemperatureC { get; set; }
        public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
        public string? Summary { get; set; }
    }
}
