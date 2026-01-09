using System.Runtime.InteropServices;
namespace Shared.Logging.Enrichment
{
    /// <summary>
    /// Defines a contract for providing static enrichment properties to logging scopes.
    /// </summary>
    public interface ILogEnricher
    {
        /// <summary>
        /// Retrieves the dictionary of static properties (e.g., MachineName, Environment) 
        /// to be applied to log scopes.
        /// </summary>
        IReadOnlyDictionary<string, object> GetEnrichmentProperties();
    }

    /// <summary>
    /// Native implementation of log enrichment using System.Environment.
    /// </summary>
    internal sealed class LogEnricher : ILogEnricher
    {
        private readonly Dictionary<string, object> _cachedProperties;

        public LogEnricher()
        {
            _cachedProperties = new Dictionary<string, object>
            {
                ["MachineName"] = Environment.MachineName,
                ["Environment"] = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production",
                ["OSArchitecture"] = RuntimeInformation.OSArchitecture.ToString(),
                ["OSDescription"] = RuntimeInformation.OSDescription
            };
        }

        public IReadOnlyDictionary<string, object> GetEnrichmentProperties()
        {
            return _cachedProperties;
        }
    }
}