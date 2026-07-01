using Api.Application.Features.Usuarios.Queries.ListarUsuarios;

namespace Api.Infrastructure.Api.Endpoints;

public static class UsuariosEndpoints
{
    public static IEndpointRouteBuilder MapUsuariosEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app
            .MapGroup("/api/usuarios")
            .WithTags("Usuarios");

        group
            .MapGet("/", async (ListarUsuariosHandler handler, CancellationToken cancellationToken) =>
            {
                var usuarios = await handler.Handle(new ListarUsuariosQuery(), cancellationToken);

                return Results.Ok(usuarios);
            })
            .WithName("ListarUsuarios")
            .WithSummary("Lista los usuarios registrados.");

        return app;
    }
}
