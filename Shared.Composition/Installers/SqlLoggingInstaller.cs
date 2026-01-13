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
                connectionStringFactory: sp => sp.GetRequiredService<SharedInfrastructureOptions>().Database?.ConnectionString,
                    configureOptions: opt =>
                    {
                        opt.SchemaName = rootOptions.SqlLogging.SchemaName;
                        opt.TableName = rootOptions.SqlLogging.TableName;
                        opt.BatchSize = rootOptions.SqlLogging.BatchSize;
                    }
            );
        }
    }
}