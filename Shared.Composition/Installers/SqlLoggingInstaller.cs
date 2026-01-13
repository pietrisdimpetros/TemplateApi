using Microsoft.Extensions.DependencyInjection;
using Shared.Composition.Options;
using Shared.Logging.Sql.Builder;

namespace Shared.Composition.Installers
{
    public class SqlLoggingInstaller : IInfrastructureInstaller
    {
        public void Install(IServiceCollection services, SharedInfrastructureOptions rootOptions)
        {
            if (rootOptions.SqlLogging is null)
                return;

            services.AddSharedSqlLogging(
                // THE BRIDGE: 
                // Connects the Logging module to the Data module dynamically at runtime.
                connectionStringFactory: sp =>
                {
                    // You can resolve any registered service here.
                    // We strongly assume Shared.Data has been registered and its options are available.
                    var infra = sp.GetRequiredService<SharedInfrastructureOptions>();

                    // Return the Database connection string
                    return infra.Database?.ConnectionString;
                },
                configureOptions: opt =>
                {
                    opt.SchemaName = rootOptions.SqlLogging.SchemaName;
                    opt.TableName = rootOptions.SqlLogging.TableName;
                    opt.BatchSize = rootOptions.SqlLogging.BatchSize;
                    opt.FlushIntervalSeconds = rootOptions.SqlLogging.FlushIntervalSeconds;
                }
            );
        }
    }
}