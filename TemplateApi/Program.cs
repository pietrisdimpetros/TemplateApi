using Shared.Composition.Builder;
using Shared.Composition.Options;
using Shared.Health.Tags;
using TemplateApi.Business.Builder;
using TemplateApi.Business.Constants;
using TemplateApi.Business.Data;
using TemplateApi.Business.Health.Checks;
using TemplateApi.Business.Workers.Audit;
using TemplateApi.Configuration;

namespace TemplateApi
{
    public partial class Program
    {
        private static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // ============================================================================
            // 1. PREPARE CONFIGURATION
            // ============================================================================
            // We build the options object MANUALLY first.
            // This ensures Installers see the final values immediately.
            #region Configuration
            var infraOptions = new SharedInfrastructureOptions();
            // A. Bind from appsettings.json
            builder.Configuration.GetSection("Infrastructure").Bind(infraOptions);
            // B. Apply Code-Based Logic (Dev/Prod defaults)
            var configurator = new ConfigureSharedInfrastructureOptions(builder.Environment, builder.Configuration);
            configurator.Configure(infraOptions);
            #endregion

            // ============================================================================
            // 2. REGISTER INFRASTRUCTURE
            // ============================================================================
            builder.Services.AddInfrastructure(
                infraOptions,
                healthBuilder =>
                {
                    healthBuilder.AddCheck<AuditLogHealthCheck>("audit_log_storage", tags: [HealthCheckTags.Ready]);
                    healthBuilder.AddCheck<GraphFunctionalityCheck>("graph_functional_test", tags: [HealthCheckTags.Ready]);
                    healthBuilder.AddCheck<DemoCheck>("demo_test", tags: [HealthCheckTags.Demo, HealthCheckTags.Ready]);

                    if (!string.IsNullOrEmpty(infraOptions.Database?.ConnectionString))
                    {
                        healthBuilder.AddSqlServer(
                           connectionString: infraOptions.Database.ConnectionString,
                           name: "sql_server",
                           tags: [HealthCheckTags.Ready]
                       );
                    }
                });

            // ============================================================================
            // 3. REGISTER BUSINESS LOGIC
            // ============================================================================
            builder.Services.AddBusinessLogic();

            // ============================================================================
            // 4. DATABASE MODULES
            // ============================================================================
            #region Module DbContexts
            builder.Services.AddModuleDbContext<CatalogDbContext>(ModuleSchemas.Catalog);
            #endregion

            // ============================================================================
            // 5. CUSTOM WORKERS
            // ============================================================================
            #region Custom Workers
            builder.Services.AddHostedService<DataCleanupWorker>();
            #endregion

            // ============================================================================
            // 6. POST-CONFIGURATION 
            // Post-configure specifically for the SQL Logger constants
            // =============================================================================
            builder.Services.PostConfigure<SharedInfrastructureOptions>(options =>
            {
                if (options.SqlLogging != null)
                {
                    options.SqlLogging.SchemaName = AuditConstants.Schema;
                    options.SqlLogging.TableName = AuditConstants.Table;
                }
            });

            // ============================================================================
            // 7. OTHER SERVICES (Controllers, etc.)
            // ============================================================================
            builder.Services.AddControllers();

            // ============================================================================
            // 8. BUILD THE APP
            // ============================================================================
            var app = builder.Build();

            // ============================================================================
            // 9.MIDDLEWARE PIPELINE
            // ============================================================================
            app.UseSharedInfrastructure();

            // ====================================================================
            // 10. MAP ENDPOINTS
            // ============================================================================
            app.MapControllers();

            // ============================================================================
            // 11. RUN THE APP
            // ============================================================================
            app.Run();
        }
    }
}