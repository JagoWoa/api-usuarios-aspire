using Api.Infrastructure.Data;
using Api.Tests.Factories;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Respawn;

namespace Api.Tests.Infrastructure;

public sealed class TestDatabase
{
    private readonly ApiWebApplicationFactory _factory;
    private readonly string _connectionString;

    public TestDatabase(ApiWebApplicationFactory factory, string connectionString)
    {
        _factory = factory;
        _connectionString = connectionString;
    }

    public async Task RecreateAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        await dbContext.Database.EnsureDeletedAsync();
        await dbContext.Database.EnsureCreatedAsync();

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        var respawner = await Respawner.CreateAsync(
            connection,
            new RespawnerOptions { DbAdapter = DbAdapter.SqlServer, WithReseed = true });

        await respawner.ResetAsync(connection);

        dbContext.Usuarios.AddRange(UsuarioFactory.CreateMany());
        await dbContext.SaveChangesAsync();
    }
}
