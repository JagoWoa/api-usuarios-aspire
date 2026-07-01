using Api.Infrastructure.Data;

namespace Api.Infrastructure;

public static class DependencyInjection
{
    public static IHostApplicationBuilder AddInfrastructure(this IHostApplicationBuilder builder)
    {
        builder.AddSqlServerDbContext<AppDbContext>("bd");

        return builder;
    }
}
