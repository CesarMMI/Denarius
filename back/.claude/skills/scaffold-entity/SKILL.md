---
name: scaffold-entity
description: Use when adding a new domain entity to Denarius (e.g. Account, Transaction, Wallet, Budget) that needs CRUD support across the Domain, Application, Infrastructure and WebAPI projects
---

# Scaffold Entity

## Overview

Denarius is a .NET 10 Clean Architecture solution (`Denarius.Domain` → `Denarius.Application` → `Denarius.Infrastructure` → `Denarius.WebAPI`) with a strict one-class-per-use-case pattern. Every entity repeats the same file skeleton across all four projects. `Category` (see files below) is the reference implementation — copy its shape, don't invent a new one.

Reference files to read before starting:
- `src/Denarius.Domain/Entities/Category.cs`, `Entity.cs`
- `src/Denarius.Application/IO/Categories/*`, `UseCases/Categories/Create/*`
- `src/Denarius.Infrastructure/Persistence/Configurations/CategoryConfiguration.cs`, `Repositories/CategoryRepository.cs`
- `src/Denarius.WebAPI/Controllers/CategoriesController.cs`

## Before Starting

Confirm with the user (don't guess):
1. Entity name (singular, PascalCase) and its fields (name + C# type + validation rules).
2. Which use cases are needed now — `Create` is the only one implemented anywhere in the codebase today. `Update` / `Delete` / `GetById` / `List` are not yet established patterns.
3. Whether any field needs a Value Object (money, email, percentage, hex color, ...) — see the `add-value-object` skill.
4. If a use case is `GetById`/`Update`/`Delete` and the entity might not be found: **there is no NotFound/domain-exception convention yet** (the only wired error path is `ArgumentException` → 400 in the controller). Ask the user how they want "not found" handled before inventing an exception type or status code.

## Layer-by-Layer

### 1. Domain (`Denarius.Domain`)

`Entities/{Entity}.cs`:
- Inherits `Entity` (`Id`, `CreatedAt`, `UpdatedAt` already provided).
- Public constructor: `: base(Guid.NewGuid(), DateTime.UtcNow, DateTime.UtcNow)`, validates every field via private static `Validate*` helpers that throw `ArgumentException` with **pt-BR messages** (match the tone of `Category.ValidateName`).
- Private constructor `(Guid id, DateTime createdAt, DateTime updatedAt, ...allFieldsInDeclarationOrder)` with **no validation** — this exists purely so EF Core can materialize rows via constructor binding. Parameter names must match the property names (case-insensitive) and include every persisted property.
- `public void Update(...)` re-validates and calls `MarkAsUpdated()` at the end.

`Repositories/I{Entity}Repository.cs`:
```csharp
public interface I{Entity}Repository : IRepository<{Entity}>
{
}
```
Add custom query signatures here only if the use case needs more than the base CRUD in `IRepository<T>`.

### 2. Application (`Denarius.Application`)

`IO/{Entity}s/{Entity}Output.cs` — record, constructor takes the entity and flattens any Value Object to its primitive (e.g. `Color` → `.HexCode`).

`IO/{Entity}s/{Verb}{Entity}Input.cs` — record per use case that needs input (e.g. `Create{Entity}Input`, `Update{Entity}Input`).

`UseCases/{Entity}s/{Verb}/I{Verb}{Entity}UseCase.cs`:
```csharp
public interface I{Verb}{Entity}UseCase : IUseCase<{Verb}{Entity}Input, Task<{Entity}Output>>
{
}
```

`UseCases/{Entity}s/{Verb}/{Verb}{Entity}UseCase.cs` — class is `internal`, interface is `public`; primary-constructor DI of `I{Entity}Repository` and `IUnitOfWork`; always ends a mutation with `await unitOfWork.SaveChangesAsync()`.

Register **every** new use case in `Denarius.Application/DependencyInjection.cs`:
```csharp
services.AddScoped<I{Verb}{Entity}UseCase, {Verb}{Entity}UseCase>();
```

### 3. Infrastructure (`Denarius.Infrastructure`)

`Persistence/Configurations/{Entity}Configuration.cs` — `IEntityTypeConfiguration<{Entity}>`, `ToTable("{Entity}s")`, `HasKey`, `.Property(...).HasMaxLength(...).IsRequired()` per field, `.HasConversion(...)` for any Value Object.

`Repositories/{Entity}Repository.cs`:
```csharp
public class {Entity}Repository(DenariusDbContext context) : Repository<{Entity}>(context), I{Entity}Repository;
```

Add `public DbSet<{Entity}> {Entity}s => Set<{Entity}>();` to `DenariusDbContext`.

Register in `Denarius.Infrastructure/DependencyInjection.cs`:
```csharp
services.AddScoped<I{Entity}Repository, {Entity}Repository>();
```

Generate and apply the migration (Infrastructure holds migrations, WebAPI is the startup project):
```bash
dotnet ef migrations add Add{Entity} --project src/Denarius.Infrastructure --startup-project src/Denarius.WebAPI
dotnet ef database update --project src/Denarius.Infrastructure --startup-project src/Denarius.WebAPI
```

### 4. WebAPI (`Denarius.WebAPI`)

`Controllers/{Entity}sController.cs` — `[ApiController] [Route("api/[controller]")]`, primary constructor injects one use case interface per action, each action wraps the use case call in `try { ... } catch (ArgumentException ex) { return BadRequest(ex.Message); }`. Follow `CategoriesController.Create` exactly for the `Created($"/api/{entity}s/{output.Id}", output)` shape.

## Checklist

| Layer | File | Notes |
|---|---|---|
| Domain | `Entities/{Entity}.cs` | public ctor + private EF ctor + `Update()` |
| Domain | `Repositories/I{Entity}Repository.cs` | extends `IRepository<T>` |
| Application | `IO/{Entity}s/{Entity}Output.cs` | flattens value objects |
| Application | `IO/{Entity}s/{Verb}{Entity}Input.cs` | one per use case with input |
| Application | `UseCases/{Entity}s/{Verb}/I{Verb}{Entity}UseCase.cs` + impl | `internal` impl class |
| Application | `DependencyInjection.cs` | `AddScoped` per use case |
| Infrastructure | `Persistence/Configurations/{Entity}Configuration.cs` | maxlength/required/conversions |
| Infrastructure | `Repositories/{Entity}Repository.cs` | one-liner |
| Infrastructure | `Persistence/DenariusDbContext.cs` | add `DbSet` |
| Infrastructure | `DependencyInjection.cs` | `AddScoped` repository |
| Infrastructure | migration | `dotnet ef migrations add` + `database update` |
| WebAPI | `Controllers/{Entity}sController.cs` | one action per use case |
| Tests | `tests/Denarius.Domain.Tests/...`, `tests/Denarius.Application.Tests/...` | see Testing section below |

## Testing

Once the entity and its use cases exist, use the **write-tests** skill to cover them: validation rules and `Update()` behavior on the entity/value objects (`Denarius.Domain.Tests`), and the happy path + validation failures for each use case with a mocked repository/unit of work (`Denarius.Application.Tests`). Do this per use case as you add it, not as a single pass at the end.

## Common Mistakes

- Forgetting to register the new use case/repository in **both** `DependencyInjection.cs` files (Application and Infrastructure) — DI resolution fails at startup, not compile time.
- Skipping the private EF-materialization constructor, or getting its parameter order/names out of sync with the properties.
- Writing validation exception messages in English — this codebase's domain messages are pt-BR (see `Category.ValidateName`).
- Inventing a "not found" exception/status code convention on the spot instead of asking the user (see "Before Starting").
- Forgetting to actually run `dotnet ef migrations add` after changing the entity/configuration.
