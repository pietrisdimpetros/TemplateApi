using System.Text.Json.Serialization;

namespace Shared.Serialization.Context
{
    /// <summary>
    /// A centralized JSON Source Generator context for common infrastructure types.
    /// Ensures AOT compatibility for basic primitives and collections.
    /// Consuming apps should chain their own contexts for domain models.
    /// </summary>
    [JsonSourceGenerationOptions(
        PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        WriteIndented = false)]
    [JsonSerializable(typeof(string))]
    [JsonSerializable(typeof(int))]
    [JsonSerializable(typeof(bool))]
    [JsonSerializable(typeof(DateTime))]
    [JsonSerializable(typeof(Guid))]
    [JsonSerializable(typeof(decimal))]
    [JsonSerializable(typeof(object))]
    [JsonSerializable(typeof(Dictionary<string, string>))]
    [JsonSerializable(typeof(Dictionary<string, object>))]
    [JsonSerializable(typeof(List<string>))]
    [JsonSerializable(typeof(List<int>))]
    public partial class AppJsonContext : JsonSerializerContext
    {
    }
}