using Vogen;

namespace Api.Domain.ValueObjects;

[ValueObject<string>]
public readonly partial struct Email
{
    private static Validation Validate(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Validation.Invalid("El email es requerido.");

        if (!value.Contains('@') || !value.Contains('.'))
            return Validation.Invalid("El email no es valido.");

        return Validation.Ok;
    }
}
