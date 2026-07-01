using Api.Application.Features.Usuarios.Queries.ListarUsuarios;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/usuarios")]
public sealed class UsuariosController : ControllerBase
{
    private readonly ISender _sender;

    public UsuariosController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet(Name = "ListarUsuarios")]
    public async Task<ActionResult<PaginatedResponse<UsuarioDto>>> Listar(
        [FromQuery] int pagina = 1,
        [FromQuery] int tamanoPagina = 10,
        CancellationToken cancellationToken = default
    )
    {
        var result = await _sender.Send(
            new ListarUsuariosQuery(pagina, tamanoPagina),
            cancellationToken
        );

        if (!result.IsSuccess)
            return BadRequest(new ValidationProblemDetails(result.Errors));

        return Ok(result.Data);
    }
}
