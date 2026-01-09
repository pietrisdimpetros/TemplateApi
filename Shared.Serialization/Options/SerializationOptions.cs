using System.Text.Json;
using System.Text.Json.Serialization;

namespace Shared.Serialization.Options
{
    /// <summary>
    /// Configuration wrapper for JSON settings.
    /// </summary>
    public sealed class SerializationOptions
    {
        /// <summary>
        /// Gets or sets the JSON Naming Policy. Default is CamelCase.
        /// </summary>
        public JsonNamingPolicy NamingPolicy { get; set; } = JsonNamingPolicy.CamelCase;

        /// <summary>
        /// Gets or sets a value indicating whether to ignore null values during serialization.
        /// Default is true (IgnoreNulls).
        /// </summary>
        public JsonIgnoreCondition IgnoreCondition { get; set; } = JsonIgnoreCondition.WhenWritingNull;

        /// <summary>
        /// Gets or sets a value indicating whether to indent JSON (pretty print).
        /// </summary>
        public bool WriteIndented { get; set; } = false;
    }
}