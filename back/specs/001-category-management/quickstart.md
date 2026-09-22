# Quickstart: Validating Category Management

This feature is already implemented; this guide validates that it still works, not how to build
it. Field-level rules are in [data-model.md](./data-model.md); the full request/response shapes
are in [contracts/categories-api.yaml](./contracts/categories-api.yaml).

## Prerequisites

- .NET 10 SDK
- PostgreSQL reachable at the connection string under `ConnectionStrings:DenariusDb` (already
  set for local dev in `src/Denarius.WebAPI/appsettings.Development.json`)
- Schema up to date:
  ```
  dotnet ef database update --project src/Denarius.Infrastructure --startup-project src/Denarius.WebAPI
  ```

## Automated validation (primary — no running server needed)

Run just the Category-related suites:

```
dotnet test tests/Denarius.Domain.Tests --filter FullyQualifiedName~CategoryTests
dotnet test tests/Denarius.Application.Tests --filter FullyQualifiedName~Categories
```

Expected: all tests pass. Together they exercise every functional requirement in `spec.md`
(FR-001…FR-015) except the HTTP layer itself (see `research.md` → Test coverage gap).

Or the full suite, matching Constitution Principle IV:

```
dotnet test
```

## Manual end-to-end smoke test (also exercises the HTTP layer)

1. Start the API: `dotnet run --project src/Denarius.WebAPI` (HTTP profile listens on
   `http://localhost:5276`, per `launchSettings.json`).
2. Create a category:
   ```
   curl -i -X POST http://localhost:5276/api/categories \
     -H "Content-Type: application/json" \
     -d '{"name":"Lazer","color":"#FF0000"}'
   ```
   Expected: `201 Created`, a `Location: /api/categories/{id}` header, body echoes the category
   with `transactionCount: 0` and `balance: 0`.
3. List categories: `curl http://localhost:5276/api/categories` — expect the new category in the
   array, sorted by name ascending by default (User Story 1, User Story 3).
4. Get it by id: `curl http://localhost:5276/api/categories/{id}` — expect `200` with the same
   data.
5. Update it:
   ```
   curl -i -X PUT http://localhost:5276/api/categories/{id} \
     -H "Content-Type: application/json" \
     -d '{"name":"Diversao","color":"#0F0"}'
   ```
   Expected: `200`, name changed, color normalized to `#00FF00`.
6. Delete it: `curl -i -X DELETE http://localhost:5276/api/categories/{id}` — expect `204`.
7. Confirm removal: repeat step 4 — expect `404`.
8. Validation edge case: repeat step 2 with `"name":""` or `"color":"not-a-color"` — expect
   `400` with a `ProblemDetails` body explaining what's wrong (User Story 1, scenario 5).
9. Delete-guard edge case: create a category, record a transaction against it via
   `POST /api/categories` → `POST /api/transactions` (Transactions API — its own feature, out of
   scope here), then attempt delete — expect `400`; the category should still be present in
   step 3's list.
10. Usage stats: repeat step 9 once more with a known transaction value, then `GET
    /api/categories?dateRef=<that month>` — expect `transactionCount`/`balance` to reflect only
    that month's transactions (User Story 2).
