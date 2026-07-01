using Api.Application.Features.Usuarios.Queries.ListarUsuarios;

namespace Api.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(configuration =>
            configuration.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly)
        );
        services.AddScoped<ListarUsuariosValidator>();

        return services;
    }
}
