using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.FeatureManagement;
using FeatureManagementOptions = Shared.FeatureManagement.Options.FeatureManagementOptions;

namespace Shared.FeatureManagement.Builder
{
    public static class FeatureManagementExtensions
    {
        /// <summary>
        /// Adds Shared.FeatureManagement infrastructure.
        /// Registers the native IFeatureManager.
        /// </summary>
        public static IServiceCollection AddSharedFeatureManagement(
            this IServiceCollection services,
            Action<FeatureManagementOptions> configure)
        {
            // 1. Configure Options
            var options = new FeatureManagementOptions();
            configure(options);
            services.AddSingleton(options);

            // 2. Register Native Feature Management
            // We need to resolve IConfiguration temporarily to find the section.
            // In a strict composition root, passing IConfiguration explicitly is better,
            // but here we rely on the service provider building effectively.
            var serviceProvider = services.BuildServiceProvider();
            var configuration = serviceProvider.GetService<IConfiguration>();

            if (configuration == null) return services;

            var section = configuration.GetSection(options.SectionName);

            if (options.FailIfMissing && !section.Exists())
            {
                throw new InvalidOperationException($"FeatureManagement section '{options.SectionName}' is missing.");
            }

            // Register the Feature Management services
            services.AddFeatureManagement(section);

            return services;
        }
    }
}