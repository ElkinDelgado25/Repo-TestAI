# Instrucciones de arquitectura

## Objetivo

Este repositorio implementa una **Clean Architecture modular dentro de un único
proyecto ASP.NET Core API** (`Api`). No se crearán proyectos separados para
Domain, Application o Infrastructure: la separación se mantiene mediante
carpetas, dependencias unidireccionales y límites explícitos de responsabilidad.

La dirección permitida de las dependencias es:

```text
Infrastructure (Api, Data, integraciones externas) -> Application -> Domain
```

`Domain` no depende de ninguna otra capa. Aunque todas las carpetas compilen en
el mismo ensamblado, el código debe respetar esta dirección.

## Estructura

```text
Api/
├── Domain/
│   ├── Entities/
│   ├── ValueObjects/
│   ├── Events/
│   ├── Exceptions/
│   └── Common/
├── Application/
│   ├── Abstractions/
│   │   ├── Cqrs/
│   │   ├── Persistence/
│   │   └── Services/
│   ├── Behaviors/
│   └── Features/
│       └── {Feature}/
│           ├── Commands/
│           ├── Queries/
│           └── Contracts/
├── Infrastructure/
│   ├── Api/
│   │   ├── Endpoints/
│   │   ├── Contracts/
│   │   └── Mapping/
│   ├── Data/
│   │   ├── Configurations/
│   │   ├── Repositories/
│   │   └── Migrations/
│   ├── Integrations/
│   └── DependencyInjection.cs
└── Program.cs
```

Crear carpetas solamente cuando tengan contenido. Adaptar el tipo de endpoint
(Minimal API o Controllers) al estilo ya adoptado por el proyecto; no mezclar
ambos estilos sin una razón concreta.

## Domain

- Contiene el modelo de negocio: entidades, objetos de valor, reglas,
  excepciones y eventos de dominio.
- Las entidades protegen sus invariantes mediante constructores, métodos de
  comportamiento y encapsulación. Evitar entidades anémicas con lógica de
  negocio en handlers o endpoints.
- Los objetos de valor se representan exclusivamente con **Vogen**. Declarar
  un tipo `partial` con `[ValueObject<T>]`; Vogen genera su creación,
  igualdad y comportamiento de value object. No crear implementaciones
  manuales de value objects, ni bases propias para ellos.
- La clase base `Entity<TId>` contiene únicamente el identificador genérico
  reutilizable. Los campos específicos de negocio pertenecen a cada entidad.
- No incluir dependencias de ASP.NET Core, Entity Framework Core, MediatR,
  serialización, HTTP, base de datos ni servicios externos.
- No exponer DTOs, modelos de request/response ni tipos de persistencia.

## Application

- Orquesta casos de uso y depende únicamente de `Domain` y de abstracciones
  propias.
- Aplicar CQRS y vertical slice: cada caso de uso vive dentro de
  `Application/Features/{Feature}` y mantiene juntos su request, handler,
  validador, resultado y mapeos específicos.
- Un comando expresa un cambio de estado. Una consulta solo lee datos y no
  genera efectos secundarios observables.
- Mantener comandos y consultas pequeños y orientados a intención de negocio,
  no a tablas o endpoints CRUD genéricos.
- Las interfaces para persistencia, reloj, identidad, mensajería o servicios
  externos viven en `Application/Abstractions`. Sus implementaciones viven en
  `Infrastructure`.
- Los handlers no conocen `HttpContext`, controladores, códigos HTTP,
  `DbContext` ni implementaciones concretas de infraestructura.
- Las validaciones de entrada de cada caso de uso deben residir junto al slice.
  Las invariantes de negocio irrenunciables permanecen en `Domain`.

Ejemplo de slice:

```text
Application/Features/Products/Commands/CreateProduct/
├── CreateProductCommand.cs
├── CreateProductHandler.cs
├── CreateProductValidator.cs
└── CreateProductResult.cs
```

## Infrastructure

- Implementa las responsabilidades externas y es la capa de adaptación.
- `Infrastructure/Api` recibe solicitudes HTTP, las transforma en comandos o
  consultas de Application y convierte sus resultados a contratos HTTP.
- `Infrastructure/Data` contiene el acceso a datos: `DbContext`, mapeos de
  persistencia, configuraciones, repositorios, migraciones y transacciones.
- `Infrastructure/Integrations` contiene clientes de servicios externos,
  mensajería, archivos, correo u otros adaptadores.
- La API no contiene reglas de negocio ni acceso directo a la base de datos.
  Debe delegar en el caso de uso correspondiente.
- No filtrar entidades de dominio ni excepciones internas como respuesta HTTP.
  Usar contratos de API y un manejo centralizado de errores.
- Registrar las implementaciones de infraestructura en extensiones de
  inyección de dependencias; `Program.cs` debe actuar principalmente como
  composition root.

## Convenciones

- Usar `async` para operaciones de I/O y propagar `CancellationToken` desde el
  endpoint hasta la infraestructura.
- Preferir tipos explícitos de request/response por caso de uso. No reutilizar
  entidades de dominio como modelos HTTP ni como modelos de EF Core.
- Mantener los nombres consistentes con el lenguaje ubicuo del dominio. Para el
  modelo de negocio actual se emplea español (`Usuario`, `Nombre`, `Apellido`)
  y los nombres técnicos pueden permanecer en inglés cuando sea la convención
  de ASP.NET Core o de una dependencia.
- Una feature nueva debe incluir, como mínimo, su caso de uso en Application y
  su adaptador HTTP en Infrastructure/Api. Añadir el adaptador de datos o la
  abstracción necesaria sin violar la dirección de dependencias.
- Al modificar una feature, localizar sus cambios dentro de su vertical slice;
  evitar carpetas globales de `Commands`, `Handlers` o `Dtos` compartidas por
  toda la aplicación.

### Objetos de valor con Vogen

- Mantener las declaraciones de Vogen en `Domain/ValueObjects`.
- Para crear un valor utilizar el método generado `From`, por ejemplo:
  `Email.From("usuario@ejemplo.com")`.
- Cuando un valor requiera una regla de dominio, declararla en el tipo parcial
  mediante el mecanismo de validación provisto por Vogen; no sustituirlo por
  constructores o implementaciones manuales de igualdad.
- Las entidades reciben y exponen tipos de valor del dominio, nunca los tipos
  primitivos que representan.

## Criterio de revisión

Antes de dar por terminado un cambio, comprobar:

1. La regla de negocio está en `Domain` y no en el endpoint o repositorio.
2. El caso de uso está autocontenido en su feature de `Application`.
3. Las dependencias externas se alcanzan mediante una abstracción de
   Application e implementación en Infrastructure.
4. Las entidades de dominio no cruzan el límite HTTP o de persistencia.
5. La solución compila y las pruebas relevantes pasan.

## Soluciones implementadas

### Usuario (2026-07-20)

- Se creó la entidad `Usuario` en `Api/Domain/Entities/Usuario.cs`.
- Hereda de `Entity<UsuarioId>`, que aporta únicamente el identificador común
  reutilizable.
- Sus campos de dominio son `UsuarioId`, `Nombre`, `Apellido` y `Email`; los
  cuatro se declaran con Vogen en `Api/Domain/ValueObjects`.
