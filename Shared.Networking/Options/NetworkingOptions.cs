namespace Shared.Networking.Options
{
    public sealed class NetworkingOptions
    {
        /// <summary>
        /// Gets or sets the User-Agent header value sent with requests.
        /// Default: "Shared-Networking-Client".
        /// </summary>
        public string UserAgent { get; set; } = "Shared-Networking-Client";

        /// <summary>
        /// Gets or sets the global timeout for requests in seconds.
        /// Default: 30 seconds.
        /// </summary>
        public int TimeoutSeconds { get; set; } = 30;

        /// <summary>
        /// Gets or sets the maximum number of retry attempts for transient failures.
        /// Default: 3.
        /// </summary>
        public int MaxRetries { get; set; } = 3;

        /// <summary>
        /// Gets or sets a value indicating whether to bypass SSL certificate validation.
        /// STRICTLY for Development/Testing environments.
        /// </summary>
        public bool IgnoreSslErrors { get; set; } = false;
    }
}