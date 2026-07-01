# Instrucciones de Clean Architecture

Este proyecto usará una sola API como proyecto principal, manteniendo Clean Architecture mediante carpetas, namespaces y reglas claras de dependencia interna.

## Objetivo

Organizar el código para que el dominio y los casos de uso no dependan de detalles externos como base de datos, HTTP, servicios externos, frameworks o controladores.

La API será el único proyecto ejecutable, pero internamente se dividirá en:

- `Domain`: reglas de negocio puras y objetos de valor generados con Vogen.
- `Application`: casos de uso con CQRS, features y vertical slice.
- `Infrastructure`: responsabilidades externas como Data, API, servicios externos, autenticación, persistencia y configuraciones técnicas.

## Estructura recomendada

```text
Api/
  Domain/
    Entities/
    ValueObjects/
    Enums/
    Events/
    Exceptions/

  Application/
    Abstractions/
      Data/
      Messaging/
      Security/
      Time/
    Behaviors/
    Common/
    Features/
      NombreFeature/
        Commands/
          CrearRecurso/
            CrearRecursoCommand.cs
            CrearRecursoHandler.cs
            CrearRecursoValidator.cs
            CrearRecursoResponse.cs
        Queries/
          ObtenerRecurso/
            ObtenerRecursoQuery.cs
            ObtenerRecursoHandler.cs
            ObtenerRecursoResponse.cs

  Infrastructure/
    Data/
      AppDbContext.cs
      Configurations/
      Migrations/
      Repositories/
      UnitOfWork.cs
    Api/
      Endpoints/
      Controllers/
      Middleware/
      Filters/
    ExternalServices/
    Authentication/
    DependencyInjection.cs

  Program.cs
```

## Reglas de dependencia

El flujo de dependencias debe respetar este orden:

```text
Infrastructure -> Application -> Domain
```

Reglas:

- `Domain` no depende de ninguna otra capa.
- `Application` solo depende de `Domain` y de abstracciones propias.
- `Infrastructure` implementa las abstracciones definidas en `Application`.
- `Program.cs` registra servicios y conecta la API con infraestructura.
- Los endpoints o controllers no deben contener lógica de negocio.
- La base de datos, HTTP externo, archivos, colas, correo y autenticación son detalles de infraestructura.

## Domain

La carpeta `Domain` contiene el modelo de negocio puro.

Debe incluir:

- Entidades.
- Objetos de valor generados con Vogen.
- Reglas de negocio.
- Eventos de dominio.
- Excepciones de dominio.
- Enums del negocio.

No debe incluir:

- Entity Framework.
- Atributos de base de datos.
- Controllers o endpoints.
- DTOs de entrada o salida HTTP.
- Servicios externos.
- Configuración de dependencias.
- Objetos de valor implementados manualmente si pueden representarse con Vogen.

Ejemplo:

```csharp
using Vogen;

namespace Api.Domain.ValueObjects;

[ValueObject<string>]
public readonly partial struct Email
{
    private static Validation Validate(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Validation.Invalid("El email es requerido.");

        if (!value.Contains('@'))
            return Validation.Invalid("El email no es valido.");

        return Validation.Ok;
    }
}
```

## Application

La carpeta `Application` contiene los casos de uso del sistema.

Se usará CQRS basado en features y vertical slice:

- Cada feature agrupa sus comandos y queries.
- Cada comando/query tiene su propio handler.
- La validación vive cerca del caso de uso.
- Los DTOs o responses viven dentro de la feature que los necesita.
- La feature no debe depender de controllers, EF Core ni servicios externos concretos.

Ejemplo de feature:

```text
Application/
  Features/
    Productos/
      Commands/
        CrearProducto/
          CrearProductoCommand.cs
          CrearProductoHandler.cs
          CrearProductoValidator.cs
          CrearProductoResponse.cs
      Queries/
        ObtenerProductoPorId/
          ObtenerProductoPorIdQuery.cs
          ObtenerProductoPorIdHandler.cs
          ObtenerProductoPorIdResponse.cs
```

Los handlers deben trabajar contra abstracciones:

```csharp
namespace Api.Application.Abstractions.Data;

public interface IApplicationDbContext
{
    DbSet<Producto> Productos { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
```

## CQRS

CQRS separa operaciones de escritura y lectura.

Comandos:

- Cambian estado.
- Pueden crear, actualizar o eliminar datos.
- Devuelven un resultado mínimo, como un id o response simple.

Queries:

- Solo leen datos.
- No deben modificar estado.
- Pueden devolver DTOs optimizados para lectura.

Convención de nombres:

- `CrearProductoCommand`
- `CrearProductoHandler`
- `ActualizarProductoCommand`
- `EliminarProductoCommand`
- `ObtenerProductoPorIdQuery`
- `ListarProductosQuery`

## Vertical Slice

Cada caso de uso debe ser independiente y fácil de ubicar.

En lugar de separar todo por tipo técnico como `Services`, `Dtos`, `Validators` y `Handlers`, se agrupa por funcionalidad:

```text
Features/
  Clientes/
    Commands/
    Queries/
  Reservas/
    Commands/
    Queries/
  Pagos/
    Commands/
    Queries/
```

Esto ayuda a modificar una funcionalidad sin saltar por todo el proyecto.

## Infrastructure

La carpeta `Infrastructure` contiene los detalles externos.

Debe incluir:

- `Data`: EF Core, DbContext, configuraciones, migraciones, repositorios y Unit of Work.
- `Api`: endpoints, controllers, middleware, filtros y detalles HTTP.
- `ExternalServices`: clientes HTTP, correo, archivos, almacenamiento, pasarelas de pago.
- `Authentication`: JWT, claims, usuarios autenticados, hashing.
- `DependencyInjection.cs`: registro de servicios de infraestructura.

La infraestructura puede implementar interfaces de `Application`.

Ejemplo:

```csharp
namespace Api.Infrastructure.Data;

public sealed class AppDbContext : DbContext, IApplicationDbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Producto> Productos => Set<Producto>();
}
```

## API

La API debe actuar como entrada al sistema.

Responsabilidades:

- Recibir requests HTTP.
- Validar datos de transporte cuando sea necesario.
- Enviar comandos o queries a la capa `Application`.
- Convertir resultados a respuestas HTTP.

No debe:

- Contener reglas de negocio.
- Acceder directamente al DbContext si existe un caso de uso.
- Construir entidades complejas con reglas de negocio dentro del controller/endpoint.
- Llamar directamente servicios externos cuando eso pertenece a un caso de uso.

## Convenciones

Namespaces:

```text
Api.Domain.*
Api.Application.*
Api.Infrastructure.*
```

Registros de dependencias:

```csharp
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
```

Los metodos `AddApplication` y `AddInfrastructure` deben agrupar registros por capa.

## Flujo recomendado

1. Crear o actualizar entidades y objetos de valor en `Domain`.
2. Crear el comando o query dentro de `Application/Features`.
3. Crear validator, handler y response dentro de la misma feature.
4. Definir abstracciones necesarias en `Application/Abstractions`.
5. Implementar detalles externos en `Infrastructure`.
6. Exponer el caso de uso desde `Infrastructure/Api` mediante endpoint o controller.
7. Registrar dependencias en `DependencyInjection.cs`.

## Regla principal

El dominio debe poder entenderse sin saber que existe HTTP, SQL Server, Entity Framework, Aspire o cualquier framework externo.

## Soluciones implementadas

### Usuario

Se creó la entidad `Usuario` dentro de `Api/Domain/Entities` con los campos:

- `Id`
- `Nombre`
- `Apellido`
- `Email`

`Email` se modeló como objeto de valor en `Api/Domain/ValueObjects` usando la libreria Vogen. La entidad `Usuario` solo declara sus propiedades reutilizables y no contiene factories, validaciones manuales ni construccion manual de objetos de valor.

Se agregó el paquete NuGet `Vogen` al proyecto `Api`. Los objetos de valor deben declararse con `[ValueObject<T>]` y deben crearse usando el metodo generado `From`.
