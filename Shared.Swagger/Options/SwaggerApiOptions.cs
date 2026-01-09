namespace Shared.Swagger.Options
{
    public sealed class OpenApiOptions
    {
        /// <summary>
        /// Gets or sets the title of the API document (e.g., "My API").
        /// </summary>
        public required string DocumentTitle { get; set; }

        /// <summary>
        /// Gets or sets the version of the API (e.g., "v1").
        /// </summary>
        public required string DocumentVersion { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to enable JWT Bearer Auth support.
        /// </summary>
        public bool EnableAuth { get; set; } = true;
    }
}