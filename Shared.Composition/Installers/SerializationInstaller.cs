using Microsoft.Extensions.DependencyInjection;
using Shared.Composition.Options;
using Shared.Health.Internal;
using Shared.Serialization.Builder;

namespace Shared.Composition.Installers
{
    public class SerializationInstaller : IInfrastructureInstaller
    {
        public void Install(IServiceCollection services, SharedInfrastructureOptions rootOptions)
        {
            if (rootOptions.Serialization is null)
                return;
            // 1. Register the Health module's AOT context into the global chain
            rootOptions.Serialization.TypeInfoResolverChain.Add(HealthJsonContext.Default);

            // 2. Register the service normally
            services.AddSharedSerialization(opt =>
            {
                opt.NamingPolicy = rootOptions.Serialization.NamingPolicy;
                opt.IgnoreCondition = rootOptions.Serialization.IgnoreCondition;
                opt.WriteIndented = rootOptions.Serialization.WriteIndented;
                // Note: TypeInfoResolverChain is a list and handled by reference if we wanted to copy it,
                // but AddSharedSerialization handles the registration of the chain elements separately.
            });
        }
    }
}