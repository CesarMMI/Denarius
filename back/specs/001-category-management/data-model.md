# Data Model: Category Management

## Category

Represents a user-defined grouping for transactions.

| Field | Type | Rules |
|---|---|---|
| `Id` | `Guid` | Generated on creation (`Guid.NewGuid()`); immutable. |
| `Name` | `string` | Required; trimmed; 1-100 characters after trimming; throws `DomainException` otherwise. |
| `Color` | `Color` (value object) | Required; must normalize to a valid hex code (see below). |
| `CreatedAt` | `DateTime` (UTC) | Set once at creation; immutable. |
| `UpdatedAt` | `DateTime` (UTC) | Set at creation; refreshed by `Update()`. |

Behavior:

- `Category(name, color)` — constructs a new category; validates and trims `name`, validates
  `color`.
- `Update(name, color)` — re-validates and replaces `Name`/`Color`; refreshes `UpdatedAt`.
- No soft-delete or status field: a category either exists or has been hard-deleted (subject to
  the transaction guard below).

## Color (value object)

Not Category-specific, but Category is its only current consumer.

| Field | Type | Rules |
|---|---|---|
| `HexCode` | `string` | Normalized to `#RRGGBB` uppercase. Accepts input with or without a leading `#`, and 3-digit shorthand (`F00` → `#FF0000`). Anything that doesn't match `^#[0-9A-F]{6}$` after normalization throws `DomainException`. |

Equality is by value (`HexCode`).

## Relationship: Category ↔ Transaction

- One `Category` has many `Transaction`s; each `Transaction.CategoryId` references exactly one
  `Category`.
- Enforced at the database level: `FK_Transactions_Categories_CategoryId`,
  `ON DELETE RESTRICT` — the database itself refuses to delete a `Category` row referenced by
  any `Transaction` row.
- Enforced again at the application level: `DeleteCategoryUseCase` checks
  `ITransactionRepository.ExistsByCategoryIdAsync` first, so the restriction surfaces as a clean
  `400` `DomainException` instead of a raw constraint-violation error.
- `Category` holds no back-reference/collection to its transactions (no navigation property) —
  the count/balance shown per category is computed on read, not stored (see below).

## Derived (not persisted): per-category usage

Computed by `ListCategoriesUseCase` for each `Category`, not stored on the entity:

| Field | Type | Definition |
|---|---|---|
| `TransactionCount` | `int` | Count of `Transaction`s with matching `CategoryId` (within the requested calendar month, if `dateRef` is given). |
| `Balance` | `decimal` | Sum of those transactions' `Value`. |

## Persistence mapping (`Categories` table)

| Column | Type | Notes |
|---|---|---|
| `Id` | `uuid` | Primary key. |
| `Name` | `character varying(100)` | `NOT NULL`. |
| `Color` | `character varying(7)` | `NOT NULL`; stores `Color.HexCode` via an EF Core value converter. |
| `CreatedAt` | `timestamp with time zone` | `NOT NULL`. |
| `UpdatedAt` | `timestamp with time zone` | `NOT NULL`. |

No state transitions beyond create/update/delete — `Category` has no workflow or status field.
