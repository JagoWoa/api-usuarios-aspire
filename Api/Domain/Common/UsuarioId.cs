using Vogen;

namespace Api.Domain.Common;

[ValueObject<Guid>]
public readonly partial struct UsuarioId
{
    private static Validation Validate(Guid value)
    {
        return value == Guid.Empty
            ? Validation.Invalid("El id del usuario es requerido.")
            : Validation.Ok;
    }
}
