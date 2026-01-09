namespace Shared.Health.Models
{
    public class HealthResponse
    {
        public required string Status { get; set; }
        public required TimeSpan TotalDuration { get; set; }
        public required Dictionary<string, HealthCheckEntryDto> Results { get; set; } = [];
    }
}