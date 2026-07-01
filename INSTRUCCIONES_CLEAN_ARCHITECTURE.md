# Instrucciones de Clean Architecture

Este proyecto usará una sola API como proyecto principal, manteniendo Clean Architecture mediante carpetas, namespaces y reglas claras de dependencia interna.

## Objetivo

Organizar el código para que el dominio no dependa de detalles externos como base de datos, HTTP, servicios externos, frameworks o controladores.

La API será el único proyecto ejecutable, pero internamente se dividirá en:

- `Domain`: reglas de negocio puras y objetos de valor generados con Vogen.
- `Application`: casos de uso con CQRS, features y vertical slice.
- `Infrastructure`: responsabilidades externas como Data, servicios externos, autenticación, persistencia y configuraciones técnicas.

## Estructura recomendada

```text
Api/
  Domain/
    Common/
      Entity.cs
      UsuarioId.cs
    Entities/
    ValueObjects/
    Enums/
    Events/
    Exceptions/

  Application/
    Behaviors/
    Common/
    Features/
      NombreFeature/
        Commands/
          CrearRecurso/
            CrearRecurso.cs
        Queries/
          ObtenerRecurso/
            ObtenerRecurso.cs

  Controllers/

  Infrastructure/
    Data/
      AppDbContext.cs
      Configurations/
      Migrations/
      Repositories/
      UnitOfWork.cs
    Middleware/
    Filters/
    ExternalServices/
    Authentication/
    DependencyInjection.cs

  Program.cs

Api.Tests/
  Factories/
  Features/
    NombreFeature/
      Commands/
      Queries/
  Infrastructure/

Api.Tests.AppHost/
  AppHost.cs
```

## Reglas de dependencia

El flujo interno del proyecto debe respetar este orden práctico:

```text
Controllers -> Application Features -> Infrastructure Data -> Domain
```

Reglas:

- `Domain` no depende de ninguna otra capa.
- `Application` contiene las features y puede usar `AppDbContext` directamente.
- `Infrastructure` contiene `AppDbContext`, configuraciones, seeds y servicios externos.
- `Controllers` reciben HTTP y delegan en handlers de `Application`.
- `Program.cs` registra servicios y conecta la API con infraestructura.
- Los controllers no deben contener lógica de negocio.
- Los controllers deben enviar commands o queries usando `ISender` de MediatR.
- La base de datos, HTTP externo, archivos, colas, correo y autenticación son detalles de infraestructura.
- No usar comentarios en el código. El código debe ser claro por nombres, estructura y separación de responsabilidades.

## Domain

La carpeta `Domain` contiene el modelo de negocio puro.

Debe incluir:

- Base común para entidades.
- Entidades.
- Objetos de valor generados con Vogen.
- Identificadores tipados generados con Vogen, como `UsuarioId`.
- Reglas de negocio.
- Eventos de dominio.
- Excepciones de dominio.
- Enums del negocio.

No debe incluir:

- Entity Framework.
- Atributos de base de datos.
- Controllers.
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
- Commands y queries deben implementarse con MediatR.
- La validación vive en el mismo archivo del caso de uso.
- Los DTOs, responses y mapeos viven en el mismo archivo de la feature que los necesita.
- Cada caso de uso vertical slice debe quedar en un solo archivo: query/command, DTOs, mapeos, handler y validaciones.
- Las queries de listado deben ser paginadas y validadas.
- La feature no debe depender de controllers ni servicios externos concretos.
- Las features pueden usar `AppDbContext` directamente para consultas y persistencia.

Ejemplo de feature:

```text
Application/
  Features/
    Productos/
      Commands/
        CrearProducto/
          CrearProducto.cs
      Queries/
        ObtenerProductoPorId/
          ObtenerProductoPorId.cs
```

Los handlers pueden trabajar directamente con `AppDbContext`:

```csharp
using Api.Infrastructure.Data;

namespace Api.Application.Features.Productos.Queries.ListarProductos;

public sealed class ListarProductosHandler
{
    private readonly AppDbContext _dbContext;

    public ListarProductosHandler(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }
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

En lugar de separar todo por tipo técnico como `Services`, `Dtos`, `Validators` y `Handlers`, se agrupa por funcionalidad y cada caso de uso queda en un solo archivo:

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
- Middleware, filtros y detalles técnicos HTTP cuando sean responsabilidades transversales.
- `ExternalServices`: clientes HTTP, correo, archivos, almacenamiento, pasarelas de pago.
- `Authentication`: JWT, claims, usuarios autenticados, hashing.
- `DependencyInjection.cs`: registro de servicios de infraestructura.

La infraestructura puede implementar interfaces de `Application`.

Ejemplo:

```csharp
namespace Api.Infrastructure.Data;

public sealed class AppDbContext : DbContext
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
- Usar controllers como entrada HTTP del proyecto.

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
builder.AddInfrastructure();
```

Los metodos `AddApplication` y `AddInfrastructure` deben agrupar registros por capa.

## Flujo recomendado

1. Crear o actualizar entidades y objetos de valor en `Domain`.
2. Crear el comando o query dentro de `Application/Features`.
3. Crear validator, handler y response dentro de la misma feature.
4. Usar `AppDbContext` directamente dentro del handler cuando el caso de uso necesite datos.
5. Implementar detalles externos en `Infrastructure`.
6. Exponer el caso de uso desde un controller.
7. Registrar dependencias en `DependencyInjection.cs`.

## Regla principal

El dominio debe poder entenderse sin saber que existe HTTP, SQL Server, Entity Framework, Aspire o cualquier framework externo.

## Soluciones implementadas

### Usuario

Se creó una base común `Entity<TId>` dentro de `Api/Domain/Common` para declarar campos reutilizables de las entidades.

Se creó `UsuarioId` dentro de `Api/Domain/Common` como objeto de valor Vogen basado en `Guid`.

Se creó la entidad `Usuario` dentro de `Api/Domain/Entities` heredando de `Entity<UsuarioId>` con los campos:

- `Id`, heredado desde `Entity<UsuarioId>`
- `Nombre`
- `Apellido`
- `Email`

`Email` se modeló como objeto de valor en `Api/Domain/ValueObjects` usando la libreria Vogen. La entidad `Usuario` solo declara sus propiedades reutilizables y no contiene factories, validaciones manuales ni construccion manual de objetos de valor.

Se agregó el paquete NuGet `Vogen` al proyecto `Api`. Los objetos de valor deben declararse con `[ValueObject<T>]` y deben crearse usando el metodo generado `From`.

### Endpoint y seeds de usuarios

Se agregó infraestructura de datos con `AppDbContext` en `Api/Infrastructure/Data`.

La API consume la base `bd` definida en Aspire AppHost mediante `Aspire.Microsoft.EntityFrameworkCore.SqlServer`:

```csharp
builder.AddSqlServerDbContext<AppDbContext>("bd");
```

El AppHost mantiene la responsabilidad de crear y exponer SQL Server:

```csharp
var bd = builder
    .AddSqlServer("bdserver")
    .WithLifetime(ContainerLifetime.Persistent)
    .AddDatabase("bd");

builder.AddProject<Projects.Api>("Api")
    .WithExternalHttpEndpoints()
    .WithReference(bd)
    .WaitFor(bd);
```

Se agregó `Bogus` para generar seeds de usuarios al iniciar la API. El seeder crea la base si no existe y solo inserta datos cuando la tabla `Usuarios` está vacía.

Se creó el endpoint mediante controller:

```http
GET /api/usuarios
```

El listado de usuarios es paginado y validado:

```http
GET /api/usuarios?pagina=1&tamanoPagina=10
```

Reglas de validación:

- `pagina` debe ser mayor o igual a `1` y no mayor a `100000`.
- `tamanoPagina` debe estar entre `1` y `100`.

El controller vive en `Api/Controllers/UsuariosController.cs` y delega la lectura a la feature `Application/Features/Usuarios/Queries/ListarUsuarios`.

La feature `ListarUsuarios` sigue vertical slice en un solo archivo: query, DTOs, response paginado, mapeo, validator y handler viven juntos en `ListarUsuarios.cs`.

La feature usa `AppDbContext` directamente. No se utiliza `IApplicationDbContext`.

### Pruebas funcionales

Se creó `Api.Tests` como proyecto NUnit para pruebas funcionales a nivel de features.

Se creó `Api.Tests.AppHost` como AppHost independiente para pruebas. Este AppHost solo levanta la base SQL Server `bd` y no usa lifetime persistente, para que la infraestructura de tests sea aislada del AppHost principal.

Reglas de pruebas:

- Las pruebas de features no deben llamar handlers directamente.
- Las pruebas deben pasar por el pipeline de MediatR usando `ISender`.
- Las pruebas HTTP deben usar `Microsoft.AspNetCore.Mvc.Testing`.
- Las aserciones de pruebas deben usar Shouldly.
- Los datos fake de tests deben crearse con Bogus dentro de factories aisladas.
- El codigo de configuracion de tests debe estar separado de los tests reales.
- Los tests de features deben separarse por `Commands` y `Queries`.
- Las queries deben incluir tests de edge cases de paginacion y validacion.
- La base de datos debe recrearse antes de cada test.
- Respawn puede usarse para reiniciar datos despues de crear el esquema.
- El AppHost principal `Api.AppHost` no debe usarse para pruebas funcionales.

La suite actual valida:

- `ListarUsuarios` desde MediatR devuelve resultado paginado.
- `ListarUsuarios` desde MediatR valida paginacion.
- Edge cases de pagina minima, pagina maxima, tamano minimo, tamano maximo, ultima pagina, pagina sin resultados y tamano maximo permitido.
- `GET /api/usuarios` desde MVC testing devuelve respuesta paginada.
- `GET /api/usuarios` desde MVC testing devuelve `400 BadRequest` cuando la paginacion es invalida.

La configuracion de pruebas vive en `Api.Tests/Infrastructure`. Los datos fake de usuarios viven en `Api.Tests/Factories/UsuarioFactory.cs`.

La salida de ejecucion de pruebas vive en `FunctionalTestBase` usando `TestContext.Out`, para no mezclar logs dentro de los tests reales. Desde VS Code se puede ejecutar la tarea `test: api detallado`, que corre:

```powershell
dotnet test Api.Tests/Api.Tests.csproj --logger "console;verbosity=detailed"
```

Si el panel de resultados de VS Code indica que la prueba no registro salida, eso solo significa que esa vista no capturo texto para el test seleccionado. La validacion real debe revisarse en la terminal o con la tarea detallada.

Para ejecutar pruebas desde el panel de pruebas de VS Code, el workspace debe usar `Api.sln` como solucion por defecto. El archivo `Api.slnx` se conserva, pero C# Dev Kit puede verificar pruebas con mayor estabilidad usando el formato clasico `.sln`.
