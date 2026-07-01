using Api.Application;
using Api.Infrastructure;
using Api.Infrastructure.Api.Endpoints;
using Api.Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddApplication();
builder.AddInfrastructure();

var app = builder.Build();

await app.Services.SeedDatabaseAsync();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapUsuariosEndpoints();
app.MapControllers();

app.Run();
