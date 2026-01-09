namespace Shared.Health.Models
{
    public class HealthCheckEntryDto
    {
        public required string Status { get; set; }
        public string? Description { get; set; }
        public object? Data { get; set; }
    }
}