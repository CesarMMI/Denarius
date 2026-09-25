# Data Model: Transaction Management

## Transaction

Represents a single recorded movement of money.

| Field | Type | Rules |
|---|---|---|
| `Id` | `Guid` | Generated on creation (`Guid.NewGuid()`); immutable. |
| `Description` | `string?` | Optional; trimmed; blank/whitespace-only normalizes to `null` rather than being rejected; up to 255 characters after trimming, throws `DomainException` if longer. |
| `Date` | `DateTime` | Required; `default(DateTime)` (unset) throws `DomainException`. |
| `Value` | `decimal` | Required; any nonzero value — positive (money received) or negative (money spent); `0` throws `DomainException`. |
| `CategoryId` | `Guid` | Required; `Guid.Empty` throws `DomainException` at the entity level. Existence of the referenced `Category` is checked one layer up (see below). |
| `CreatedAt` | `DateTime` (UTC) | Set once at creation; immutable. |
| `UpdatedAt` | `DateTime` (UTC) | Set at creation; refreshed by `Update()`. |

Behavior:

- `Transaction(description, date, value, categoryId)` — constructs a new transaction; validates
  and normalizes `description`, validates `date`, `value`, and `categoryId`.
- `Update(description, date, value, categoryId)` — re-validates and replaces all four fields;
  refreshes `UpdatedAt`. A failed validation leaves the existing instance unchanged (the
  exception is thrown before any field is reassigned).
- No soft-delete or status field: a transaction either exists or has been hard-deleted, with no
  guard based on other data (contrast with `Category`, which refuses deletion while referenced).

## Relationship: Transaction → Category

- Each `Transaction.CategoryId` references exactly one `Category`
  ([001-category-management](../001-category-management/data-model.md)); a `Category` has many
  `Transaction`s.
- Enforced at the database level: `FK_Transactions_Categories_CategoryId`,
  `ON DELETE RESTRICT` — mirrors the direction described in
  [001-category-management/data-model.md](../001-category-management/data-model.md), but from
  the referencing side.
- Enforced again at the application level: `CreateTransactionUseCase` and
  `UpdateTransactionUseCase` both call `ICategoryRepository.GetByIdAsync(categoryId)` before
  persisting, so an invalid/nonexistent category surfaces as a clean `404` `NotFoundException`
  instead of a raw constraint-violation error.
- `Transaction` holds no navigation property back to `Category`; only the raw `CategoryId` is
  stored. Nothing else in the domain currently references a `Transaction` — deleting one is
  unconditional.

## Persistence mapping (`Transactions` table)

| Column | Type | Notes |
|---|---|---|
| `Id` | `uuid` | Primary key. |
| `Description` | `character varying(255)` | Nullable. |
| `Date` | `timestamp with time zone` | `NOT NULL`. |
| `Value` | `numeric(18,2)` | `NOT NULL`. |
| `CategoryId` | `uuid` | `NOT NULL`; FK → `Categories.Id`, `ON DELETE RESTRICT`; indexed (`IX_Transactions_CategoryId`). |
| `CreatedAt` | `timestamp with time zone` | `NOT NULL`. |
| `UpdatedAt` | `timestamp with time zone` | `NOT NULL`. |

No state transitions beyond create/update/delete — `Transaction` has no workflow or status
field, and (unlike `Category`) no derived/computed fields are attached to it by any use case.

## List query (`ListTransactionsInput`)

Application-layer input for `ListTransactionsUseCase` (User Story 3); not persisted. Every field
is optional, and filters combine with AND.

| Field | Type | Default | Rules |
|---|---|---|---|
| `Description` | `string?` | `null` | Trimmed; partial, case-insensitive match against `Transaction.Description`. `null`/blank = no filter. Transactions with no description never match. |
| `DateRef` | `DateTime?` | `null` | Any date within the target month; keeps transactions with `Date` from the first moment of that month through its last tick, inclusive. `null` = all months. |
| `Type` | `TransactionType` | `All` | `All` = no filter; `In` = `Value > 0`; `Out` = `Value < 0`. |
| `CategoryId` | `Guid?` | `null` | Keeps only transactions with that `CategoryId`. `null` = all categories. An id that matches no category just produces an empty list. |
| `OrderBy` | `TransactionOrderField` | `Date` | `Date`, `Description`, `Value` (signed), or `CategoryName` (the referenced category's current name). |
| `Ascending` | `bool` | `false` | `false` = descending; the default is most recent first. |

The output is still `IEnumerable<TransactionOutput>`, unchanged in shape — `CategoryName` is only
a sort key and is not added to the response.
