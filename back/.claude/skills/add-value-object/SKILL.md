---
name: add-value-object
description: Use when introducing a new Value Object in Denarius.Domain (e.g. Money, Email, Percentage) that needs input validation and value-based equality
---

# Add Value Object

## Overview

Reference implementation: `src/Denarius.Domain/ValueObjects/Color.cs` (base class `ValueObject.cs` in the same folder). Use this whenever a domain field is more than a bare primitive — it has validation rules, normalization, or should compare by value instead of reference.

## Steps

1. **Class** in `Domain/ValueObjects/{Name}.cs`, inherits `ValueObject`. Mark `partial` only if using `[GeneratedRegex]` like `Color` does.

2. **Constructor validates and normalizes**, throwing `ArgumentException` with a **pt-BR message** on invalid input (match `Color`'s tone: `"A cor não pode ser vazia."`, `"'{valor}' não é uma cor válida."`). Store only the normalized value.

3. **Implement `GetEqualityComponents()`**:
   ```csharp
   protected override IEnumerable<object> GetEqualityComponents()
   {
       yield return NormalizedValue;
   }
   ```
   Yield every field that should participate in equality, in a stable order.

4. **Override `ToString()`** if the value object commonly needs to round-trip to a string (e.g. for output DTOs or EF conversions).

5. **Wire it into an entity**: the entity's constructor/`Update` method takes the Value Object type directly (not the raw primitive) — validation happens once, at construction of the Value Object, not re-validated by the entity.

6. **EF Core mapping**: in the entity's `IEntityTypeConfiguration`, use `.HasConversion(vo => vo.PrimitiveValue, primitive => new {Name}(primitive))` (see `CategoryConfiguration.cs`'s `Color` mapping) plus `.HasMaxLength(...)` matching the column width.

7. **Application layer boundary**: Input/Output records still use the raw primitive type (`string`, not the Value Object) — the use case constructs the Value Object from the primitive, and the Output record flattens it back (see `CreateCategoryInput.Color: string` and `CategoryOutput.Color: string` around `new Color(input.Color)` / `category.Color.HexCode`).

## Testing

Use the **write-tests** skill to add a `{Name}Tests` in `Denarius.Domain.Tests`: valid input normalizes as expected (use `[Theory]`/`[InlineData]` for the different accepted formats), invalid input throws `ArgumentException` per rule, and equality holds for two instances built from different-but-equivalent raw input.

## Common Mistakes

- Validating the same rule again in the entity or the use case — validation belongs solely in the Value Object constructor.
- Forgetting `GetEqualityComponents()`, which silently falls back to reference equality.
- Exposing the Value Object type directly on Input/Output records instead of its primitive form, coupling the API contract to the domain type.
- Forgetting the `.HasConversion(...)` in the EF configuration, which leaves EF unable to map the type to a column.
