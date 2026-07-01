using Api.Domain.ValueObjects;

namespace Api.Domain.Entities;

public sealed class Usuario
{
    public Guid Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Apellido { get; set; } = string.Empty;
    public Email Email { get; set; }
}
