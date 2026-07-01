extern alias ApiProject;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace Api.Tests.Infrastructure;

public sealed class ApiWebApplicationFactory : WebApplicationFactory<ApiProject::Program>
{
    private readonly string _connectionString;
    private readonly string? _previousConnectionString;

    public ApiWebApplicationFactory(string connectionString)
    {
        _connectionString = connectionString;
        _previousConnectionString = Environment.GetEnvironmentVariable("ConnectionStrings__bd");
        Environment.SetEnvironmentVariable("ConnectionStrings__bd", _connectionString);
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        var contentRoot = Path.GetFullPath(
            Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "Api")
        );

        builder.UseEnvironment("Testing");
        builder.UseContentRoot(contentRoot);
        builder.ConfigureAppConfiguration(configuration =>
        {
            configuration.AddInMemoryCollection(
                new Dictionary<string, string?> { ["ConnectionStrings:bd"] = _connectionString }
            );
        });
    }

    protected override void Dispose(bool disposing)
    {
        Environment.SetEnvironmentVariable("ConnectionStrings__bd", _previousConnectionString);
        base.Dispose(disposing);
    }
}
