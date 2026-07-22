# Instrucciones de Clean Architecture

## Objetivo

Este repositorio usa un único proyecto ASP.NET Core API (`Api`) organizado por
carpetas y responsabilidades. La separación lógica mantiene estas dependencias:

```text
Infrastructure (HTTP, Data e integraciones) -> Application -> Domain
```

Aunque todas las carpetas compilan en el mismo ensamblado, `Domain` no depende
de ASP.NET Core, EF Core, MediatR ni servicios externos.

## Estructura adoptada

```text
Api/
├── Domain/
│   ├── Common/
│   ├── Entities/
│   └── ValueObjects/
├── Application/
│   ├── Behaviors/
│   └── Features/Usuarios/
│       ├── Commands/RegistrarUsuario/RegistrarUsuario.cs
│       └── Queries/ObtenerUsuarios/ObtenerUsuarios.cs
├── Infrastructure/
│   ├── Data/
│   │   ├── Configurations/
│   │   └── Seed/
│   └── DependencyInjection.cs
└── Program.cs
```

Crear carpetas solo cuando contengan código. No mezclar Minimal APIs y
controladores sin una razón concreta: este proyecto utiliza controladores.

## Domain

- Las entidades, reglas, excepciones y eventos viven en `Domain`.
- `Entity<TId>` solo aporta el identificador reutilizable y la igualdad por tipo
  e identificador; no contiene objetos de valor manuales.
- Los objetos de valor se definen exclusivamente con **Vogen** mediante
  `[ValueObject<T>]`. No crear una base `ValueObject` ni implementar igualdad o
  constructores manuales para esos tipos.
- Los valores persistidos incluyen `Conversions.EfCoreValueConverter` y se
  convierten en la configuración de EF Core.
- Domain no conoce DTOs, HTTP, DbContext ni detalles de SQL Server.

## Application: CQRS y vertical slice

- Cada caso de uso vive en `Application/Features/{Entidad}/{Commands|Queries}`.
- Cada feature ocupa **un único archivo**. Allí se declaran el command o query,
  DTOs, request HTTP, mapeos, handler, validador y controlador correspondiente.
- MediatR envía commands y queries a sus handlers. FluentValidation se ejecuta
  antes del handler mediante `ValidationBehavior`.
- En este proyecto único, los handlers inyectan directamente `ApiDbContext`.
  Esta decisión simplifica las features, pero las acopla a EF Core y a
  Infrastructure; no usarla si se requiere aislamiento estricto entre capas.
- Un command cambia estado; una query solo lee datos.
- Los controladores solo traducen HTTP a mensajes MediatR y convierten el
  resultado a una respuesta HTTP; no implementan lógica de negocio ni acceso a
  datos.

## Infrastructure y Aspire

- `ApiDbContext` es el contexto que inyectan las features. EF Core configura las
  conversiones de Vogen en `Infrastructure/Data/Configurations`.
- El AppHost de Aspire crea el SQL Server persistente `dbserver`, la base `bd` y
  entrega la conexión a la API con `WithReference`.
- `Program.cs` registra el contexto con la integración nativa de Aspire:

  ```csharp
  builder.AddSqlServerDbContext<ApiDbContext>("bd");
  ```

- MediatR, FluentValidation, controladores y OpenAPI se registran en el
  composition root (`Program.cs` y extensiones de DI).

## Soluciones implementadas

### Usuario

- La entidad `Usuario` tiene `UsuarioId Id`, `Nombre`, `Apellido` y `Email`.
- `UsuarioId`, `Nombre`, `Apellido` y `Email` son objetos de valor de Vogen.
- `Email` normaliza el texto y valida su formato básico.
- `UsuarioConfiguration` persiste los tipos fuertes de Vogen como tipos nativos
  de SQL Server y exige unicidad para el correo.

### Features HTTP

- `POST /api/usuarios`: registra un usuario y devuelve `201 Created`.
- `GET /api/usuarios`: lista usuarios paginados con `pagina` y `tamanoPagina`
  y devuelve `200 OK`.
- `GET /api/usuarios/{id}`: obtiene un usuario por identificador; devuelve
  `200 OK`, `404 Not Found` si no existe o `400 Bad Request` si el identificador
  no es válido.
- La entrada inválida devuelve `400 Bad Request` y un correo repetido devuelve
  `409 Conflict`.
- Ambos controladores se descubren mediante `app.MapControllers()`.

### Semillas de desarrollo

- `DbInitializer.InitializeDatabaseAsync()` se invoca exclusivamente cuando el
  entorno es `Development`.
- Crea el esquema con `EnsureCreatedAsync()` y, si no hay usuarios, usa Bogus en
  español para insertar 25 registros reproducibles.
- Cuando se incorporen migraciones, sustituir `EnsureCreatedAsync()` por
  `MigrateAsync()`.

## Criterio de revisión

Antes de terminar un cambio comprobar:

1. La regla de negocio está en Domain, no en controlador o DbContext.
2. El caso de uso está autocontenido en un archivo de su feature.
3. Las features usan directamente `ApiDbContext`, según la decisión del
   proyecto de mantener una única API simplificada.
4. Las entidades de dominio no se exponen como contratos HTTP.
5. La solución compila y las pruebas relevantes pasan.

## Pruebas automatizadas

- `Api.Tests` es el único proyecto de pruebas; `Api` continúa siendo el único
  proyecto de producción.
- Los tests reales viven en `Api.Tests/Features/Usuarios/Commands` y
  `Api.Tests/Features/Usuarios/Queries`; no se mezclan comandos y consultas.
  Validan handlers, validación de paginación y controladores con Shouldly.
- La infraestructura de prueba se mantiene separada en
  `Api.Tests/Configuracion`: `ApiDbContextFactory` crea una base EF Core InMemory
  aislada por test y `UsuarioFactory` encapsula los datos ficticios de Bogus.
- Los tests no instancian `Faker` ni configuran EF Core directamente.
- La suite cubre casos normales y límite: registro persistido, correo inválido,
  normalización de correo, nombre vacío, correo duplicado, lista paginada,
  colección vacía, página fuera de rango, parámetros inválidos y respuestas
  `200 OK` y `400 Bad Request` del controlador; además cubre búsqueda de
  usuario por identificador existente, inexistente e inválido.
