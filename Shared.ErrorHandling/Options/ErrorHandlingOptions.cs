namespace Shared.ErrorHandling.Options
{
    public sealed class ErrorHandlingOptions
    {
        /// <summary>
        /// Gets or sets a value indicating whether to include the exception stack trace 
        /// in the ProblemDetails response. STRICTLY for Development environments.
        /// </summary>
        public required bool IncludeStackTrace { get; set; }
    }
}