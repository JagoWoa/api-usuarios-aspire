using Api.Domain.Common;
using Api.Domain.ValueObjects;

namespace Api.Domain.Entities;

public sealed class Usuario : Entity<UsuarioId>
{
    public string Nombre { get; set; } = string.Empty;
    public string Apellido { get; set; } = string.Empty;
    public Email Email { get; set; }
}
