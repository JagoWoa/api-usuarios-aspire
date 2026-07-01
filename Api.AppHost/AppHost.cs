var builder = DistributedApplication.CreateBuilder(args);

var bd = builder
    .AddSqlServer("bdserver")
    .WithLifetime(ContainerLifetime.Persistent)
    .AddDatabase("bd");

builder.AddProject<Projects.Api>("Api")
.WithExternalHttpEndpoints().WithReference(bd).WaitFor(bd);

builder.Build().Run();
