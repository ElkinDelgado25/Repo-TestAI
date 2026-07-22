# Clean Architecture: guía visual

## Flujo de una solicitud

```text
HTTP request
    │
    ▼
Controller dentro del archivo del vertical slice
    │  ISender.Send(...)
    ▼
Command o Query + DTO + Validator + Handler + Mapping
    │  ApiDbContext
    ▼
ApiDbContext (EF Core) ──── SQL Server "bd" (Aspire)
```

## Vertical slices de usuarios

```text
Application/Features/Usuarios/
├── Commands/
│   ├── RegistrarUsuario/RegistrarUsuario.cs      POST /api/usuarios
│   ├── ActualizarUsuario/ActualizarUsuario.cs    PUT /api/usuarios/{id}
│   └── EliminarUsuario/EliminarUsuario.cs        DELETE /api/usuarios/{id}
└── Queries/
    ├── ObtenerUsuarios/ObtenerUsuarios.cs        GET /api/usuarios
    └── ObtenerUsuarioPorId/ObtenerUsuarioPorId.cs GET /api/usuarios/{id}
```

## Infraestructura

```text
Api.AppHost
  └── dbserver / bd ── WithReference ──► Api
                                          │
                                          ├── AddSqlServerDbContext<ApiDbContext>("bd")
                                          └── ApiDbContext inyectado en los handlers
```

En Development, `DbInitializer` crea el esquema y usa Bogus para sembrar 25
usuarios solo si la tabla está vacía. `app.MapControllers()` descubre los
controladores declarados dentro de cada feature.

## Pruebas

```text
Api.Tests/
├── Configuracion/
│   ├── Factories/UsuarioFactory.cs       Bogus aislado
│   └── Persistencia/ApiDbContextFactory.cs  EF Core InMemory
└── Features/Usuarios/
    ├── Commands/RegistrarUsuario/         Tests de comandos
└── Queries/ObtenerUsuarios/           Tests de queries y controlador
```

La suite valida casos normales y límite de comandos, paginación, controlador,
consulta por identificador, actualización y eliminación lógica.
