# Quickstart: Validating Transaction Management

This feature is already implemented; this guide validates that it still works, not how to build
it. Field-level rules are in [data-model.md](./data-model.md); the full request/response shapes
are in [contracts/transactions-api.yaml](./contracts/transactions-api.yaml).

## Prerequisites

- .NET 10 SDK
- PostgreSQL reachable at the connection string under `ConnectionStrings:DenariusDb` (already
  set for local dev in `src/Denarius.WebAPI/appsettings.Development.json`)
- Schema up to date:
  ```
  dotnet ef database update --project src/Denarius.Infrastructure --startup-project src/Denarius.WebAPI
  ```
- At least one existing category to attach transactions to — see
  [001-category-management/quickstart.md](../001-category-management/quickstart.md) step 2 if one
  doesn't exist yet.

## Automated validation (primary — no running server needed)

Run just the Transaction-related suites:

```
dotnet test tests/Denarius.Domain.Tests --filter FullyQualifiedName~TransactionTests
dotnet test tests/Denarius.Application.Tests --filter FullyQualifiedName~Transactions
```

Expected: all tests pass. Together they exercise every functional requirement in `spec.md`
(FR-001…FR-012) except the HTTP layer itself, which
`tests/Denarius.WebAPI.Tests/Transactions/TransactionsControllerTests.cs` covers separately (see
`research.md` → Test coverage gap).

Or the full suite, matching Constitution Principle IV:

```
dotnet test
```

## Manual end-to-end smoke test (also exercises the HTTP layer)

1. Start the API: `dotnet run --project src/Denarius.WebAPI` (HTTP profile listens on
   `http://localhost:5276`, per `launchSettings.json`).
2. Create a category to attach the transaction to (skip if one already exists):
   ```
   curl -i -X POST http://localhost:5276/api/categories \
     -H "Content-Type: application/json" \
     -d '{"name":"Mercado","color":"#FF0000"}'
   ```
   Note the returned `id` as `{categoryId}`.
3. Create a transaction:
   ```
   curl -i -X POST http://localhost:5276/api/transactions \
     -H "Content-Type: application/json" \
     -d '{"description":"Compras da semana","date":"2026-09-15T00:00:00Z","value":-150.75,"categoryId":"{categoryId}"}'
   ```
   Expected: `201 Created`, a `Location: /api/transactions/{id}` header, body echoes the
   transaction with the negative value preserved (User Story 1, scenario 1).
4. List transactions: `curl http://localhost:5276/api/transactions` — expect the new transaction
   in the array, alongside every other transaction currently recorded, with no filtering applied
   (User Story 2).
5. Get it by id: `curl http://localhost:5276/api/transactions/{id}` — expect `200` with the same
   data.
6. Update it:
   ```
   curl -i -X PUT http://localhost:5276/api/transactions/{id} \
     -H "Content-Type: application/json" \
     -d '{"description":"","date":"2026-09-16T00:00:00Z","value":200.00,"categoryId":"{categoryId}"}'
   ```
   Expected: `200`, date/value changed, `description` becomes `null` (blank normalizes to no
   description — User Story 1, scenario 2 and 5).
7. Delete it: `curl -i -X DELETE http://localhost:5276/api/transactions/{id}` — expect `204`,
   with no dependent-data refusal (User Story 1, scenario 3; contrast with the Category
   delete-guard).
8. Confirm removal: repeat step 5 — expect `404`.
9. Validation edge case: repeat step 3 with `"value":0` or an omitted `"date"` — expect `400`
   with a `ProblemDetails` body explaining what's wrong (User Story 1, scenario 4).
10. Referential edge case: repeat step 3 with a random `categoryId` (e.g.
    `"00000000-0000-0000-0000-000000000000"`) — expect `404` explaining the category was not
    found.
