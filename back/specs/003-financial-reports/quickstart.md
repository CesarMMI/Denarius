# Quickstart: Validating the Financial Reports

How to prove the five reports work, from the unit tests to the SQL that PostgreSQL runs. Field rules
are in [data-model.md](./data-model.md); request and response shapes are in
[contracts/reports-api.yaml](./contracts/reports-api.yaml).

## Prerequisites

- .NET 10 SDK and the `dotnet-ef` tool (10.x)
- PostgreSQL reachable with the credentials of `ConnectionStrings:DenariusDb` in
  `src/Denarius.WebAPI/appsettings.Development.json`, and `psql` on the path
- A **scratch database**, so the validation never touches the development data:

  ```
  psql -U postgres -c "CREATE DATABASE denarius_reports_smoke"
  ```

  Every command below that talks to the database uses this connection string:

  ```
  SMOKE="Host=localhost;Port=5432;Database=denarius_reports_smoke;Username=postgres;Password=root"
  ```

## 1. Automated validation (no server needed)

```
dotnet test tests/Denarius.Application.Tests --filter FullyQualifiedName~Reports
dotnet test tests/Denarius.WebAPI.Tests --filter FullyQualifiedName~Reports
dotnet test
```

Expected: every test passes — the report rules (empty month, only income, only expense, month and
year turns, current vs. past and future months, division by zero), `YearMonth` parsing, and the
controller's binding, defaults and `400`s. The last command runs the three suites (Constitution
Principle IV).

## 2. Migration and its rollback (Constitution Principle III)

```
dotnet ef database update --project src/Denarius.Infrastructure --startup-project src/Denarius.WebAPI --connection "$SMOKE"
psql -U postgres -d denarius_reports_smoke -c '\di "IX_Transactions_*"'
```

Expected: `IX_Transactions_CategoryId` and `IX_Transactions_Date` exist. Then roll back to the
previous migration and re-apply:

```
dotnet ef database update AddTransaction --project src/Denarius.Infrastructure --startup-project src/Denarius.WebAPI --connection "$SMOKE"
psql -U postgres -d denarius_reports_smoke -c '\di "IX_Transactions_*"'
dotnet ef database update --project src/Denarius.Infrastructure --startup-project src/Denarius.WebAPI --connection "$SMOKE"
```

Expected: after the rollback only `IX_Transactions_CategoryId` remains and no table or row changed;
after the re-apply both indexes are back.

## 3. Seed a known month history

Dates are written as UTC midnights, the way the API stores calendar days (a bare `'2026-09-05'`
would be read in the server's time zone).

```sql
INSERT INTO "Categories" ("Id", "Name", "Color", "CreatedAt", "UpdatedAt")
SELECT gen_random_uuid(), name, color, now(), now()
FROM (VALUES ('Salário', '#1E88E5'), ('Aluguel', '#8E24AA'), ('Mercado', '#43A047'),
             ('Restaurantes', '#F4511E'), ('Lazer', '#FDD835'), ('Transporte', '#00ACC1'),
             ('Saúde', '#E53935'), ('Educação', '#3949AB'), ('Assinaturas', '#6D4C41'),
             ('Pets', '#7CB342'), ('Presentes', '#D81B60')) AS c(name, color);

INSERT INTO "Transactions" ("Id", "Description", "Date", "Value", "CategoryId", "CreatedAt", "UpdatedAt")
SELECT gen_random_uuid(), description, (day || 'T00:00:00Z')::timestamptz, value,
       (SELECT "Id" FROM "Categories" WHERE "Name" = category), now(), now()
FROM (VALUES
  ('Salário', '2025-12-05',  7500.00, 'Salário'), ('Aluguel', '2025-12-10', -1900.00, 'Aluguel'),
  ('Natal',   '2025-12-20',  -800.00, 'Presentes'),
  ('Salário', '2026-01-05',  7500.00, 'Salário'), ('Aluguel', '2026-01-10', -1900.00, 'Aluguel'),
  ('Feira',   '2026-01-15',  -900.00, 'Mercado'),
  ('Feira',   '2026-08-02', -1000.00, 'Mercado'), ('Salário', '2026-08-05', 8000.00, 'Salário'),
  ('Aluguel', '2026-08-10', -2000.00, 'Aluguel'), ('Feira',   '2026-08-20',  -500.00, 'Mercado'),
  ('Show',    '2026-08-25',  -500.00, 'Lazer'),
  ('Streaming', '2026-09-01', -100.00, 'Assinaturas'), ('Feira', '2026-09-03', -700.00, 'Mercado'),
  ('Salário', '2026-09-05',  8000.00, 'Salário'), ('Ônibus',  '2026-09-08',  -300.00, 'Transporte'),
  ('Aluguel', '2026-09-10', -2000.00, 'Aluguel'), ('Jantar',  '2026-09-12',  -600.00, 'Restaurantes'),
  ('Cinema',  '2026-09-15',  -400.00, 'Lazer'),   ('Farmácia', '2026-09-18', -250.00, 'Saúde'),
  ('Feira',   '2026-09-20',  -500.00, 'Mercado'), ('Curso',   '2026-09-22',  -200.00, 'Educação'),
  ('Ração',   '2026-09-25',   -80.00, 'Pets'),    ('Presente', '2026-09-28',  -70.00, 'Presentes'),
  ('Feira',   '2026-10-01',  -150.00, 'Mercado'), ('Ônibus',  '2026-10-02',   -60.00, 'Transporte'),
  ('Salário', '2026-10-05',  8000.00, 'Salário'), ('Aluguel', '2026-10-10', -2000.00, 'Aluguel')
) AS t(description, day, value, category);
```

Month by month: December 2025 — 7,500.00 in, 2,700.00 out; January 2026 — 7,500.00 in, 2,800.00
out; February to July 2026 — nothing; August — 8,000.00 in, 4,000.00 out; September — 8,000.00 in,
5,200.00 out in ten categories; October — 8,000.00 in and 2,210.00 out, of which only 210.00 dated
up to October 3 (the salary and the rent are recorded ahead, for the 5th and the 10th).

## 4. Call the reports

Start the API on the scratch database:

```
ConnectionStrings__DenariusDb="$SMOKE" dotnet run --project src/Denarius.WebAPI --launch-profile http
```

The expectations for the current month assume **today is 2026-10-03 in São Paulo**; on another day,
recompute them with the rules in [data-model.md](./data-model.md).

1. `curl "http://localhost:5276/api/reports/summary?month=2026-09"` — a past month: income 8000,
   expense 5200, balance 2800, `savingsRate` 35, projection equal to the actual values (5200 and
   2800); `previousMonth` 8000 / 4000 / 4000 with changes 0, 30 and −30.
2. `curl "http://localhost:5276/api/reports/summary"` — no month: `month` is `2026-10`. Income 8000,
   expense 2210, balance 5790, `savingsRate` 72.38; `projectedExpense` 2170 (210 ÷ 3 days × 31) and
   `projectedBalance` 5830; changes against September 0, −57.5 and 106.79.
3. `curl "http://localhost:5276/api/reports/summary?month=2026-11"` — a future month: zeros,
   `savingsRate` null, projection 0; changes against October −100.
4. `curl "http://localhost:5276/api/reports/summary?month=2026-03"` — a month without data after
   another one: zeros, `savingsRate` null and every change null (February is zero too).
5. `curl "http://localhost:5276/api/reports/expensesByCategory?month=2026-09"` — total 5200 and eight
   entries: Aluguel 2000 (38.46), Mercado 1200 (23.08), Restaurantes 600 (11.54), Lazer 400 (7.69),
   Transporte 300 (5.77), Saúde 250 (4.81), Educação 200 (3.85), then Outras 250 (4.81) with no id or
   color (Assinaturas + Pets + Presentes). With `month=2026-08`: Aluguel 2000 (50), Mercado 1500
   (37.5), Lazer 500 (12.5), no Outras.
6. `curl "http://localhost:5276/api/reports/incomeVsExpense?month=2026-10"` — twelve months, from
   `2025-11` to `2026-10`, oldest first: `2025-11` zeros, `2025-12` 7500/2700/4800, `2026-01`
   7500/2800/4700, `2026-02` to `2026-07` zeros, `2026-08` 8000/4000/4000, `2026-09` 8000/5200/2800,
   `2026-10` 8000/2210/5790. With `&months=3`: only August to October.
7. `curl "http://localhost:5276/api/reports/cumulativeExpenses?month=2026-10"` — `currentMonth` stops
   at day 3 (150, 210, 210); `previousMonth` has the 30 days of September, from 100 on day 1 to 5200
   on days 28–30; `daysInCurrentMonth` 31, `daysInPreviousMonth` 30. With `month=2026-01`: both
   series have 31 days, December 2025 ending at 2700 and January 2026 at 2800.
8. `curl "http://localhost:5276/api/reports/transactions?month=2026-09"` — the twelve September
   transactions, from "Presente" (28th, `out`, 70) to "Streaming" (1st, `out`, 100); "Salário" is
   `in`, 8000, with `categoryName` "Salário".
9. Invalid input — each returns `400` with a `ValidationProblemDetails` body and calls no use case:
   `?month=2026-13`, `?month=2026-9`, `?month=09-2026` on any report;
   `incomeVsExpense?months=0`, `?months=25` and `?months=abc`.

## 5. Volume (SC-002)

Add 10,000 transactions spread over 2025 and 2026, then time each report:

```sql
INSERT INTO "Transactions" ("Id", "Description", "Date", "Value", "CategoryId", "CreatedAt", "UpdatedAt")
SELECT gen_random_uuid(), 'Carga ' || n,
       '2025-01-01T00:00:00Z'::timestamptz + (n % 730) * interval '1 day',
       CASE WHEN n % 10 = 0 THEN 3000.00 ELSE -((n % 500) + 1)::numeric END,
       (SELECT "Id" FROM "Categories" ORDER BY "Name" OFFSET (n % 11) LIMIT 1), now(), now()
FROM generate_series(1, 10000) AS n;
```

```
for path in "summary?month=2026-09" "expensesByCategory?month=2026-09" "incomeVsExpense?month=2026-12&months=24" "cumulativeExpenses?month=2026-09" "transactions?month=2026-09"; do
  curl -s -o /dev/null -w "$path %{http_code} %{time_total}s\n" "http://localhost:5276/api/reports/$path"
done
```

Expected: every report `200` in well under one second (the first call after start-up also pays
for warming up EF Core).

## 6. Clean up

Stop the API and drop the scratch database:

```
psql -U postgres -c "DROP DATABASE denarius_reports_smoke"
```

The development database only needs the new index:
`dotnet ef database update --project src/Denarius.Infrastructure --startup-project src/Denarius.WebAPI`.
