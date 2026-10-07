using Microsoft.EntityFrameworkCore;
using QMSSystem.Api.Data;

namespace QMSSystem.Api.Services.Workflow;

public static class WorkflowServiceCollectionExtensions
{
    // Registers the QMS module database and the workflow services.
    public static IServiceCollection AddQmsWorkflow(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<QmsDbContext>(options => options.UseSqlServer(connectionString));
        services.AddSingleton(TimeProvider.System);
        services.AddScoped<AuditService>();
        services.AddScoped<WorkflowService>();
        services.AddScoped<WorkflowQueries>();
        return services;
    }
}
