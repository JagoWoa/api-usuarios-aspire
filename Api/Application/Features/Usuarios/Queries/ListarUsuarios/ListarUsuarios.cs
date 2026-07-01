using Api.Domain.Entities;
using Api.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Api.Application.Features.Usuarios.Queries.ListarUsuarios;

public sealed record ListarUsuariosQuery(int Pagina = 1, int TamanoPagina = 10);

public sealed record UsuarioDto(
    Guid Id,
    string Nombre,
    string Apellido,
    string Email);

public sealed record PaginatedResponse<T>(
    IReadOnlyCollection<T> Items,
    int Pagina,
    int TamanoPagina,
    int TotalRegistros,
    int TotalPaginas);

public sealed record ListarUsuariosResult(
    PaginatedResponse<UsuarioDto>? Data,
    Dictionary<string, string[]> Errors)
{
    public bool IsSuccess => Errors.Count == 0;

    public static ListarUsuariosResult Success(PaginatedResponse<UsuarioDto> data)
    {
        return new ListarUsuariosResult(data, []);
    }

    public static ListarUsuariosResult Failure(Dictionary<string, string[]> errors)
    {
        return new ListarUsuariosResult(null, errors);
    }
}

public sealed class ListarUsuariosValidator
{
    public Dictionary<string, string[]> Validate(ListarUsuariosQuery query)
    {
        ArgumentNullException.ThrowIfNull(query);

        var errors = new Dictionary<string, string[]>();

        if (query.Pagina < 1)
            errors[nameof(query.Pagina)] = ["La pagina debe ser mayor o igual a 1."];

        if (query.Pagina > 100000)
            errors[nameof(query.Pagina)] = ["La pagina no puede ser mayor a 100000."];

        if (query.TamanoPagina < 1)
            errors[nameof(query.TamanoPagina)] = ["El tamano de pagina debe ser mayor o igual a 1."];

        if (query.TamanoPagina > 100)
            errors[nameof(query.TamanoPagina)] = ["El tamano de pagina no puede ser mayor a 100."];

        return errors;
    }
}

public static class UsuarioMappings
{
    public static UsuarioDto ToDto(this Usuario usuario)
    {
        return new UsuarioDto(
            usuario.Id.Value,
            usuario.Nombre,
            usuario.Apellido,
            usuario.Email.Value);
    }
}

public sealed class ListarUsuariosHandler
{
    private readonly AppDbContext _dbContext;
    private readonly ListarUsuariosValidator _validator;

    public ListarUsuariosHandler(
        AppDbContext dbContext,
        ListarUsuariosValidator validator)
    {
        _dbContext = dbContext;
        _validator = validator;
    }

    public async Task<ListarUsuariosResult> Handle(
        ListarUsuariosQuery query,
        CancellationToken cancellationToken = default)
    {
        var errors = _validator.Validate(query);

        if (errors.Count > 0)
            return ListarUsuariosResult.Failure(errors);

        var usuariosQuery = _dbContext
            .Usuarios
            .AsNoTracking()
            .OrderBy(usuario => usuario.Apellido)
            .ThenBy(usuario => usuario.Nombre);

        var totalRegistros = await usuariosQuery.CountAsync(cancellationToken);
        var totalPaginas = totalRegistros == 0
            ? 0
            : (int)Math.Ceiling(totalRegistros / (double)query.TamanoPagina);

        var usuarios = await usuariosQuery
            .Skip((query.Pagina - 1) * query.TamanoPagina)
            .Take(query.TamanoPagina)
            .ToListAsync(cancellationToken);

        var items = usuarios
            .Select(usuario => usuario.ToDto())
            .ToList();

        return ListarUsuariosResult.Success(
            new PaginatedResponse<UsuarioDto>(
                items,
                query.Pagina,
                query.TamanoPagina,
                totalRegistros,
                totalPaginas));
    }
}
