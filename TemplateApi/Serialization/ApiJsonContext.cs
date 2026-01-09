using System.Text.Json.Serialization;
using TemplateApi.Models;

namespace TemplateApi.Serialization
{
    // Register your API-specific models here for AOT support
    [JsonSerializable(typeof(IEnumerable<WeatherForecast>))]
    [JsonSerializable(typeof(WeatherForecast))]
    internal partial class ApiJsonContext : JsonSerializerContext
    {
    }
}