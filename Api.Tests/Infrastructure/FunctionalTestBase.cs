using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Api.Tests.Infrastructure;

[NonParallelizable]
public abstract class FunctionalTestBase
{
    private TestDatabaseApp _databaseApp = null!;
    private ApiWebApplicationFactory _factory = null!;
    private TestDatabase _database = null!;

    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
        try
        {
            _databaseApp = await TestDatabaseApp.StartAsync();
            _factory = new ApiWebApplicationFactory(_databaseApp.ConnectionString);
            _database = new TestDatabase(_factory, _databaseApp.ConnectionString);
            _ = _factory.Services;
        }
        catch
        {
            if (_factory is not null)
                await _factory.DisposeAsync();

            if (_databaseApp is not null)
                await _databaseApp.DisposeAsync();

            throw;
        }
    }

    [SetUp]
    public async Task SetUp()
    {
        WriteTestOutput($"Iniciando {TestContext.CurrentContext.Test.FullName}");
        await _database.RecreateAsync();
        WriteTestOutput("Base de datos recreada y datos fake cargados");
    }

    [TearDown]
    public void TearDown()
    {
        var context = TestContext.CurrentContext;
        WriteTestOutput($"{context.Test.Name}: {context.Result.Outcome.Status}");
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDown()
    {
        await _factory.DisposeAsync();
        await _databaseApp.DisposeAsync();
    }

    protected async Task<TResponse> SendAsync<TResponse>(IRequest<TResponse> request)
    {
        using var scope = _factory.Services.CreateScope();
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();

        return await sender.Send(request);
    }

    protected HttpClient CreateClient()
    {
        return _factory.CreateClient();
    }

    private static void WriteTestOutput(string message)
    {
        TestContext.Out.WriteLine(message);
    }
}
