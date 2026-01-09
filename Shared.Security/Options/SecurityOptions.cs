namespace Shared.Security.Options
{
    public sealed class SecurityOptions
    {
        /// <summary>
        /// Gets or sets the Authority (Issuer) for the JWT token (e.g., "https://identity.example.com").
        /// </summary>
        public required string Authority { get; set; }

        /// <summary>
        /// Gets or sets the Audience for the JWT token (e.g., "payment-service").
        /// </summary>
        public required string Audience { get; set; }

        /// <summary>
        /// Gets or sets the list of allowed origins for CORS.
        /// Example: ["https://portal.example.com", "http://localhost:4200"]
        /// </summary>
        public required string[] AllowedOrigins { get; set; }
    }
}