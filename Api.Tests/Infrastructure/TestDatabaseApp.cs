using Aspire.Hosting;
using Aspire.Hosting.Testing;

namespace Api.Tests.Infrastructure;

public sealed class TestDatabaseApp : IAsyncDisposable
{
    private static readonly TimeSpan DefaultTimeout = TimeSpan.FromMinutes(3);
    private readonly DistributedApplication _app;

    private TestDatabaseApp(DistributedApplication app, string connectionString)
    {
        _app = app;
        ConnectionString = connectionString;
    }

    public string ConnectionString { get; }

    public static async Task<TestDatabaseApp> StartAsync()
    {
        using var cancellationTokenSource = new CancellationTokenSource(DefaultTimeout);
        var cancellationToken = cancellationTokenSource.Token;

        var builder =
            await DistributedApplicationTestingBuilder.CreateAsync<Projects.Api_Tests_AppHost>(
                ["--environment=Testing"],
                cancellationToken
            );

        var app = await builder
            .BuildAsync(cancellationToken)
            .WaitAsync(DefaultTimeout, cancellationToken);

        await app.StartAsync(cancellationToken).WaitAsync(DefaultTimeout, cancellationToken);

        await app
            .ResourceNotifications.WaitForResourceHealthyAsync("bd", cancellationToken)
            .WaitAsync(DefaultTimeout, cancellationToken);

        var connectionString = await app.GetConnectionStringAsync("bd", cancellationToken);

        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException(
                "No se pudo obtener la cadena de conexion de la base de datos de pruebas."
            );

        return new TestDatabaseApp(app, connectionString);
    }

    public async ValueTask DisposeAsync()
    {
        await _app.DisposeAsync();
    }
}
