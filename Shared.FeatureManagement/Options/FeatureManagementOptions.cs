namespace Shared.FeatureManagement.Options
{
    public sealed class FeatureManagementOptions
    {
        /// <summary>
        /// Gets or sets a value indicating whether to throw an exception if the "FeatureManagement" 
        /// section is missing in appsettings.
        /// Default: false.
        /// </summary>
        public bool FailIfMissing { get; set; } = false;

        /// <summary>
        /// Gets or sets the name of the configuration section to bind to.
        /// Default: "FeatureManagement".
        /// </summary>
        public string SectionName { get; set; } = "FeatureManagement";
    }
}