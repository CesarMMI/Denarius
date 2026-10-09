# Guia rápido: validação dos relatórios financeiros

Como provar que os cinco relatórios funcionam, dos testes unitários ao SQL que o PostgreSQL executa.
As regras dos campos estão em [data-model.md](./data-model.md); os formatos de requisição e resposta
estão em [contracts/reports-api.yaml](./contracts/reports-api.yaml).

## Pré-requisitos

- SDK do .NET 10 e a ferramenta `dotnet-ef` (10.x)
- PostgreSQL acessível com as credenciais de `ConnectionStrings:DenariusDb` em
  `src/Denarius.WebAPI/appsettings.Development.json`, e o `psql` no path
- Um **banco descartável**, para que a validação nunca toque nos dados de desenvolvimento:

  ```
  psql -U postgres -c "CREATE DATABASE denarius_reports_smoke"
  ```

  Todos os comandos abaixo que falam com o banco usam esta connection string:

  ```
  SMOKE="Host=localhost;Port=5432;Database=denarius_reports_smoke;Username=postgres;Password=root"
  ```

## 1. Validação automatizada (sem servidor)

```
dotnet test tests/Denarius.Application.Tests --filter FullyQualifiedName~Reports
dotnet test tests/Denarius.WebAPI.Tests --filter FullyQualifiedName~Reports
dotnet test
```

Esperado: todos os testes passam — as regras dos relatórios (mês vazio, só receita, só despesa,
viradas de mês e de ano, mês atual vs. meses passados e futuros, divisão por zero), o parsing do
`YearMonth` e o binding, os padrões e os `400`s do controller. O último comando roda as três suítes
(Princípio IV da constituição).

## 2. A migration e o seu rollback (Princípio III da constituição)

```
dotnet ef database update --project src/Denarius.Infrastructure --startup-project src/Denarius.WebAPI --connection "$SMOKE"
psql -U postgres -d denarius_reports_smoke -c '\di "IX_Transactions_*"'
```

Esperado: `IX_Transactions_CategoryId` e `IX_Transactions_Date` existem. Depois, faça o rollback
para a migration anterior e reaplique:

```
dotnet ef database update AddTransaction --project src/Denarius.Infrastructure --startup-project src/Denarius.WebAPI --connection "$SMOKE"
psql -U postgres -d denarius_reports_smoke -c '\di "IX_Transactions_*"'
dotnet ef database update --project src/Denarius.Infrastructure --startup-project src/Denarius.WebAPI --connection "$SMOKE"
```

Esperado: depois do rollback, só `IX_Transactions_CategoryId` continua lá e nenhuma tabela ou linha
mudou; depois de reaplicar, os dois índices voltam.

## 3. Popular um histórico de meses conhecido

As datas são escritas como meias-noites UTC, do jeito que a API armazena os dias do calendário (um
`'2026-09-05'` puro seria lido no fuso horário do servidor).

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

Mês a mês: dezembro de 2025 — 7.500,00 de entradas, 2.700,00 de saídas; janeiro de 2026 — 7.500,00
de entradas, 2.800,00 de saídas; fevereiro a julho de 2026 — nada; agosto — 8.000,00 de entradas,
4.000,00 de saídas; setembro — 8.000,00 de entradas, 5.200,00 de saídas em dez categorias; outubro —
8.000,00 de entradas e 2.210,00 de saídas, dos quais só 210,00 com data até 3 de outubro (o salário
e o aluguel foram registrados antes, para os dias 5 e 10).

## 4. Chamar os relatórios

Suba a API no banco descartável:

```
ConnectionStrings__DenariusDb="$SMOKE" dotnet run --project src/Denarius.WebAPI --launch-profile http
```

As expectativas para o mês atual supõem que **hoje é 2026-10-03 em São Paulo**; em outro dia,
recalcule-as com as regras de [data-model.md](./data-model.md).

1. `curl "http://localhost:5276/api/reports/summary?month=2026-09"` — um mês passado: receita 8000,
   despesa 5200, saldo 2800, `savingsRate` 35, projeção igual aos valores reais (5200 e 2800);
   `previousMonth` 8000 / 4000 / 4000 com variações 0, 30 e −30.
2. `curl "http://localhost:5276/api/reports/summary"` — sem mês: `month` é `2026-10`. Receita 8000,
   despesa 2210, saldo 5790, `savingsRate` 72.38; `projectedExpense` 4170 (210 ÷ 3 dias × 31 = 2170,
   mais os 2000 do aluguel lançado para o dia 10) e `projectedBalance` 3830; variações em relação a
   setembro 0, −57.5 e 106.79.
3. `curl "http://localhost:5276/api/reports/summary?month=2026-11"` — um mês futuro: zeros,
   `savingsRate` null, projeção 0; variações em relação a outubro −100.
4. `curl "http://localhost:5276/api/reports/summary?month=2026-03"` — um mês sem dados depois de
   outro também sem dados: zeros, `savingsRate` null e todas as variações null (fevereiro também é
   zero).
5. `curl "http://localhost:5276/api/reports/expensesByCategory?month=2026-09"` — total 5200 e oito
   entradas: Aluguel 2000 (38.46), Mercado 1200 (23.08), Restaurantes 600 (11.54), Lazer 400 (7.69),
   Transporte 300 (5.77), Saúde 250 (4.81), Educação 200 (3.85) e, por fim, Outras 250 (4.81), sem
   id nem cor (Assinaturas + Pets + Presentes). Com `month=2026-08`: Aluguel 2000 (50), Mercado 1500
   (37.5), Lazer 500 (12.5), sem Outras.
6. `curl "http://localhost:5276/api/reports/incomeVsExpense?month=2026-10"` — doze meses, de
   `2025-11` a `2026-10`, do mais antigo para o mais recente: `2025-11` zerado, `2025-12`
   7500/2700/4800, `2026-01` 7500/2800/4700, `2026-02` a `2026-07` zerados, `2026-08`
   8000/4000/4000, `2026-09` 8000/5200/2800, `2026-10` 8000/2210/5790. Com `&months=3`: só de agosto
   a outubro.
7. `curl "http://localhost:5276/api/reports/cumulativeExpenses?month=2026-10"` — `currentMonth` vai
   até o dia 10, o do aluguel já lançado (150 no dia 1, 210 do dia 2 ao 9 e 2210 no dia 10; a receita
   do dia 5 não muda nada); `previousMonth` tem os 30 dias de setembro, de 100 no dia 1 a 5200 nos
   dias 28–30; `daysInCurrentMonth` 31, `daysInPreviousMonth` 30. Com `month=2026-01`: as duas
   séries têm 31 dias, com dezembro de 2025 terminando em 2700 e janeiro de 2026 em 2800. Para
   conferir que uma receita não estende a série, acrescente uma receita depois da última despesa
   (`('Freela', '2026-10-20', 500.00, 'Salário')` no seed): `currentMonth` continua terminando no dia
   10, e no resumo só a receita (e com ela o saldo e o saldo projetado) aumenta em 500.
8. `curl "http://localhost:5276/api/reports/transactions?month=2026-09"` — as doze transações de
   setembro, de "Presente" (dia 28, `out`, 70) a "Streaming" (dia 1º, `out`, 100); "Salário" é
   `in`, 8000, com `categoryName` "Salário".
9. Entrada inválida — cada uma devolve `400` com um corpo `ValidationProblemDetails` e não chama
   nenhum caso de uso: `?month=2026-13`, `?month=2026-9`, `?month=09-2026` em qualquer relatório;
   `incomeVsExpense?months=0`, `?months=25` e `?months=abc`.

## 5. Volume (SC-002)

Adicione 10.000 transações espalhadas por 2025 e 2026 e meça o tempo de cada relatório:

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

Esperado: todos os relatórios dão `200` em bem menos de um segundo (a primeira chamada depois de
subir a API também paga o aquecimento do EF Core).

## 6. Limpeza

Pare a API e apague o banco descartável:

```
psql -U postgres -c "DROP DATABASE denarius_reports_smoke"
```

O banco de desenvolvimento só precisa do novo índice:
`dotnet ef database update --project src/Denarius.Infrastructure --startup-project src/Denarius.WebAPI`.
