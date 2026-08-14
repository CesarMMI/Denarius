---
name: write-tests
description: Use when writing unit tests for Denarius entities, value objects, or use cases, or when scaffolding a new test project for a layer that doesn't have one yet
---

# Write Tests

## Overview

Denarius uses **xUnit** + **NSubstitute**, plain `Assert.*` (no FluentAssertions). One test project per `src/` project needing coverage, mirroring its folder structure. Reference implementations — read these before writing new tests:
- `tests/Denarius.Domain.Tests/Entities/CategoryTests.cs`, `ValueObjects/ColorTests.cs`
- `tests/Denarius.Application.Tests/UseCases/Categories/Create/CreateCategoryUseCaseTests.cs`

## Project Setup (only if the layer has no test project yet)

```bash
dotnet new xunit -o tests/Denarius.{Layer}.Tests -n Denarius.{Layer}.Tests
rm tests/Denarius.{Layer}.Tests/UnitTest1.cs
dotnet add tests/Denarius.{Layer}.Tests/Denarius.{Layer}.Tests.csproj reference src/Denarius.{Layer}/Denarius.{Layer}.csproj
dotnet add tests/Denarius.{Layer}.Tests/Denarius.{Layer}.Tests.csproj package NSubstitute
dotnet sln Denarius.slnx add tests/Denarius.{Layer}.Tests/Denarius.{Layer}.Tests.csproj --solution-folder tests
```

If the project under test has `internal` use-case implementation classes (it does, by convention — see `scaffold-entity` skill), add `[assembly: InternalsVisibleTo("Denarius.{Layer}.Tests")]` to a `src/Denarius.{Layer}/AssemblyInfo.cs` file, otherwise the test project can't construct them directly.

## Naming and Structure

- Test class: `{TypeUnderTest}Tests.cs`, placed in the same relative folder as the source file (`Entities/CategoryTests.cs` mirrors `Entities/Category.cs`).
- Test method: `MethodUnderTest_Scenario_ExpectedResult` (e.g. `Constructor_EmptyOrWhitespaceName_ThrowsArgumentException`, `Execute_InvalidColor_ThrowsArgumentExceptionAndDoesNotPersist`).
- Use `[Theory]`/`[InlineData]` for validation edge cases (empty/whitespace/null/too-long) instead of repeating near-identical `[Fact]`s.
- Arrange/Act/Assert, no comments marking the sections — the blank lines and code shape make it obvious.

## Testing Entities and Value Objects (`Denarius.Domain.Tests`)

Cover, per entity/value object:
- Valid construction sets all fields correctly.
- Each validation rule has its own `[Theory]` (or `[Fact]` if it's a single case) proving it throws `ArgumentException`.
- Normalization behavior (trimming, casing) if the type does any.
- `Update(...)` methods: valid update changes fields; invalid update throws **and leaves prior state untouched** (test this explicitly — see `CategoryTests.Update_EmptyName_ThrowsArgumentExceptionAndKeepsOriginalState`).
- Value Object equality: same normalized value → `Equal`; different value → `NotEqual`.

**Known gotcha:** `Entity`'s public constructor is called as `base(Guid.NewGuid(), DateTime.UtcNow, DateTime.UtcNow)` — the two `DateTime.UtcNow` calls can differ by a tick, so don't assert `CreatedAt == UpdatedAt` exactly. Assert the difference is within a small tolerance instead (see `CategoryTests.Constructor_ValidNameAndColor_CreatesCategory`).

## Testing Use Cases (`Denarius.Application.Tests`)

Pattern (see `CreateCategoryUseCaseTests`):
```csharp
public class {Verb}{Entity}UseCaseTests
{
    private readonly I{Entity}Repository _repository = Substitute.For<I{Entity}Repository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly I{Verb}{Entity}UseCase _useCase;

    public {Verb}{Entity}UseCaseTests()
    {
        _useCase = new {Verb}{Entity}UseCase(_repository, _unitOfWork);
    }

    // one [Fact] for the happy path: asserts the output shape AND
    // asserts the repository/unitOfWork calls with .Received(1)
    // one [Fact] per validation failure: asserts it throws AND
    // asserts the repository/unitOfWork were NOT called, via .DidNotReceive()
}
```

Always assert both sides of a mutation: the output value **and** that persistence was (or wasn't) triggered. A test that only checks the output can pass even if `SaveChangesAsync()` was never called.

## Running Tests

```bash
dotnet test Denarius.slnx
```

## Common Mistakes

- Asserting `CreatedAt == UpdatedAt` exactly (see the ticks gotcha above).
- Forgetting `InternalsVisibleTo` when the type under test is `internal` — compile error, not a helpful test failure.
- Only asserting the return value of a use case and skipping `Received()`/`DidNotReceive()` checks on the repository/unit of work.
- Adding FluentAssertions or another assertion library — this project sticks to plain xUnit `Assert`.
- One giant `[Fact]` covering multiple validation rules instead of one `[Theory]`/`[Fact]` per rule — makes failures ambiguous.
