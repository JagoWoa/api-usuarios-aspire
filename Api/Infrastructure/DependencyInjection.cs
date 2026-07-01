using Api.Application.Abstractions.Data;
using Api.Infrastructure.Data;

namespace Api.Infrastructure;

public static class DependencyInjection
{
    public static IHostApplicationBuilder AddInfrastructure(this IHostApplicationBuilder builder)
    {
        builder.AddSqlServerDbContext<AppDbContext>("bd");

        builder.Services.AddScoped<IApplicationDbContext>(serviceProvider =>
            serviceProvider.GetRequiredService<AppDbContext>());

        return builder;
    }
}
