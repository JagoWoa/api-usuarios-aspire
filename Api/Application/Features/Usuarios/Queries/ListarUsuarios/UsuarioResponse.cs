namespace Api.Application.Features.Usuarios.Queries.ListarUsuarios;

public sealed record UsuarioResponse(
    Guid Id,
    string Nombre,
    string Apellido,
    string Email);
