using BuildingBlocks.Exceptions.Handler;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;

namespace Ordering.Api;

public static class DependencyInjection
{
    public static IServiceCollection AddApiServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddCarter();

        services.AddExceptionHandler<CustomExceptionHandler>();
        services.AddHealthChecks()
            .AddSqlServer(configuration.GetConnectionString("Database")!);

        return services;
    }

    public static WebApplication UseApiServices(this WebApplication app)
    {
        app.MapCarter();

        app.UseExceptionHandler(options =>
        {
            options.Run(async context =>
            {
                var exceptionHandler = context.RequestServices.GetRequiredService<IExceptionHandler>();
                var exception = context.Features.Get<IExceptionHandlerFeature>()?.Error;

                if (exception != null)
                {
                    await exceptionHandler.TryHandleAsync(context, exception, context.RequestAborted);
                }
            });
        });

        app.UseHealthChecks("/health",
            new HealthCheckOptions
            {
                ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
            }
        );

        return app;
    }
}
