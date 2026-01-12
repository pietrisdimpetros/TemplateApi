namespace Shared.Idempotency.Options
{
    public sealed class IdempotencyOptions
    {
        /// <summary>
        /// The header name to check for the idempotency key.
        /// Default: "Idempotency-Key".
        /// </summary>
        public string HeaderName { get; set; } = "Idempotency-Key";

        /// <summary>
        /// How long to keep the cached response in Redis.
        /// Default: 24 hours.
        /// </summary>
        public int ExpirationMinutes { get; set; } = 24 * 60;

        /// <summary>
        /// Whether to enforce the presence of the header on decorated endpoints.
        /// If true, returns 400 Bad Request if missing.
        /// </summary>
        public bool EnforceHeader { get; set; } = true;
    }
}