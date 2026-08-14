---
name: add-use-case
description: Use when adding a single new operation (Update, Delete, GetById, List, or a custom action) to a Denarius entity whose Domain and Infrastructure layers already exist
---

# Add Use Case

## Overview

Narrower slice of `scaffold-entity` for when the entity already has its Domain/Infrastructure plumbing (repository, EF configuration, DbSet) and you're only adding one more Application-layer operation plus its WebAPI endpoint. Reference implementation: `Create` for `Category` — read `src/Denarius.Application/UseCases/Categories/Create/*`, `src/Denarius.Application/IO/Categories/*`, and `src/Denarius.WebAPI/Controllers/CategoriesController.cs` first.

## Before Starting

- If the operation needs a "not found" outcome (`GetById`, `Update`, `Delete` on a missing id): there is no established exception/status-code convention in this codebase yet (only `ArgumentException` → 400 is wired). Ask the user how they want it handled instead of guessing.
- If the operation mutates the entity, check whether `Entity.cs` already exposes a method for it (e.g. `Update(...)`). If not, add that domain method first — use cases should not mutate entity state directly, they call entity methods.

## Steps

1. **Input/Output records** in `Application/IO/{Entity}s/`:
   - Only add an `{Verb}{Entity}Input.cs` if the operation takes parameters beyond an id (an id-only input can just be a `Guid` parameter on the use case, no record needed — check existing precedent before adding one preemptively).
   - Reuse the existing `{Entity}Output.cs` for the return shape; don't create a new output record unless the operation returns a genuinely different shape (e.g. `List` returns `IEnumerable<{Entity}Output>`, still the same record).

2. **Use case interface + implementation** in `Application/UseCases/{Entity}s/{Verb}/`:
   ```csharp
   public interface I{Verb}{Entity}UseCase : IUseCase<TInput, TOutput>
   {
   }

   internal class {Verb}{Entity}UseCase(I{Entity}Repository repository, IUnitOfWork unitOfWork) : I{Verb}{Entity}UseCase
   {
       public async Task<...> Execute(TInput input)
       {
           // fetch via repository.GetByIdAsync when the op targets one entity
           // mutate via the entity's own method, never by setting properties directly
           // await unitOfWork.SaveChangesAsync() for any mutation (Create/Update/Delete) — skip for pure reads (GetById/List)
       }
   }
   ```
   Interface is `public`, implementation class is `internal` — matches every existing use case.

3. **Register in `Denarius.Application/DependencyInjection.cs`**:
   ```csharp
   services.AddScoped<I{Verb}{Entity}UseCase, {Verb}{Entity}UseCase>();
   ```

4. **Controller action** in `Controllers/{Entity}sController.cs`:
   - Add the new use case interface to the primary constructor parameter list.
   - Add an action following the existing HTTP-verb convention (`[HttpGet("{id}")]`, `[HttpPut("{id}")]`, `[HttpDelete("{id}")]`, `[HttpGet]` for list).
   - Wrap in `try { ... } catch (ArgumentException ex) { return BadRequest(ex.Message); }` like `Create` does, plus whatever "not found" handling was agreed on in "Before Starting".

## Testing

After wiring the use case, use the **write-tests** skill to add a `{Verb}{Entity}UseCaseTests` covering: the happy path (output shape + `Received(1)` on the repository call and `SaveChangesAsync`), and one case per validation/not-found failure (`DidNotReceive()` on persistence calls). If the entity gained a new method (e.g. `Update`) to support this use case, cover it in `Denarius.Domain.Tests` too.

## Common Mistakes

- Forgetting the `AddScoped` registration — the app builds fine and fails at runtime DI resolution.
- Mutating entity properties directly from the use case instead of calling an `Update`-style method on the entity (breaks the validation-lives-in-the-entity convention).
- Skipping `SaveChangesAsync()` on a mutating use case, or calling it unnecessarily on a read-only one.
- Inventing a new Output/Input record shape when the existing `{Entity}Output` already fits.
