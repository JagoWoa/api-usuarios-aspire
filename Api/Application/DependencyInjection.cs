using Api.Application.Features.Usuarios.Queries.ListarUsuarios;

namespace Api.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<ListarUsuariosHandler>();

        return services;
    }
}
