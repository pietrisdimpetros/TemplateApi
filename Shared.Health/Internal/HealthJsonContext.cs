using Shared.Health.Models;
using System.Text.Json.Serialization;

namespace Shared.Health.Internal
{
    [JsonSerializable(typeof(HealthResponse))]
    public partial class HealthJsonContext : JsonSerializerContext
    {
    }
}