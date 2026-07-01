using Api.Domain.Common;
using Api.Domain.Entities;
using Api.Domain.ValueObjects;
using Bogus;

namespace Api.Tests.Factories;

public static class UsuarioFactory
{
    public static IReadOnlyCollection<Usuario> CreateMany(int count = 25)
    {
        var faker = new Faker<Usuario>("es")
            .UseSeed(12345)
            .RuleFor(usuario => usuario.Id, faker => UsuarioId.From(faker.Random.Guid()))
            .RuleFor(usuario => usuario.Nombre, faker => faker.Name.FirstName())
            .RuleFor(usuario => usuario.Apellido, faker => faker.Name.LastName())
            .RuleFor(
                usuario => usuario.Email,
                faker => Email.From($"usuario{faker.UniqueIndex}@tests.local"));

        return faker.Generate(count);
    }
}
