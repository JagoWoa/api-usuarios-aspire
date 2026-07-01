using Api.Application.Features.Usuarios.Queries.ListarUsuarios;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/usuarios")]
public sealed class UsuariosController : ControllerBase
{
    private readonly ListarUsuariosHandler _listarUsuariosHandler;

    public UsuariosController(ListarUsuariosHandler listarUsuariosHandler)
    {
        _listarUsuariosHandler = listarUsuariosHandler;
    }

    [HttpGet(Name = "ListarUsuarios")]
    public async Task<ActionResult<PaginatedResponse<UsuarioDto>>> Listar(
        [FromQuery] int pagina = 1,
        [FromQuery] int tamanoPagina = 10,
        CancellationToken cancellationToken = default)
    {
        var result = await _listarUsuariosHandler.Handle(
            new ListarUsuariosQuery(pagina, tamanoPagina),
            cancellationToken);

        if (!result.IsSuccess)
            return BadRequest(new ValidationProblemDetails(result.Errors));

        return Ok(result.Data);
    }
}
