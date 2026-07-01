using Api.Domain.Common;
using Api.Domain.Entities;
using Api.Domain.ValueObjects;
using Bogus;
using Microsoft.EntityFrameworkCore;

namespace Api.Infrastructure.Data;

public static class DatabaseSeeder
{
    public static async Task SeedDatabaseAsync(this IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        await dbContext.Database.EnsureCreatedAsync();

        if (await dbContext.Usuarios.AnyAsync())
            return;

        var faker = new Faker<Usuario>("es")
            .RuleFor(usuario => usuario.Id, faker => UsuarioId.From(faker.Random.Guid()))
            .RuleFor(usuario => usuario.Nombre, faker => faker.Name.FirstName())
            .RuleFor(usuario => usuario.Apellido, faker => faker.Name.LastName())
            .RuleFor(
                usuario => usuario.Email,
                (faker, usuario) => Email.From(faker.Internet.Email(usuario.Nombre, usuario.Apellido).ToLowerInvariant()));

        var usuarios = faker.Generate(25);

        dbContext.Usuarios.AddRange(usuarios);
        await dbContext.SaveChangesAsync();
    }
}
