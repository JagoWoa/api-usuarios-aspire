using Api.Application.Abstractions.Data;
using Microsoft.EntityFrameworkCore;

namespace Api.Application.Features.Usuarios.Queries.ListarUsuarios;

public sealed class ListarUsuariosHandler
{
    private readonly IApplicationDbContext _dbContext;

    public ListarUsuariosHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyCollection<UsuarioResponse>> Handle(
        ListarUsuariosQuery query,
        CancellationToken cancellationToken = default)
    {
        var usuarios = await _dbContext
            .Usuarios
            .AsNoTracking()
            .OrderBy(usuario => usuario.Apellido)
            .ThenBy(usuario => usuario.Nombre)
            .ToListAsync(cancellationToken);

        return usuarios
            .Select(usuario => new UsuarioResponse(
                usuario.Id.Value,
                usuario.Nombre,
                usuario.Apellido,
                usuario.Email.Value))
            .ToList();
    }
}
