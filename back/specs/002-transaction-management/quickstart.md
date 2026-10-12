# Guia rápido: validação da gestão de transações

Esta feature já está implementada; este guia confere que ela continua funcionando, e não como
construí-la. As regras de cada campo estão em [data-model.md](./data-model.md); os formatos
completos de requisição/resposta estão em [contracts/transactions-api.yaml](./contracts/transactions-api.yaml).

## Pré-requisitos

- SDK do .NET 10
- PostgreSQL acessível pela connection string em `ConnectionStrings:DenariusDb` (já configurada
  para o desenvolvimento local em `src/Denarius.WebAPI/appsettings.Development.json`)
- Esquema do banco atualizado:
  ```
  dotnet ef database update --project src/Denarius.Infrastructure --startup-project src/Denarius.WebAPI
  ```
- Pelo menos uma categoria existente para vincular as transações — veja o passo 2 de
  [001-category-management/quickstart.md](../001-category-management/quickstart.md) se ainda não
  existir nenhuma.

## Validação automatizada (a principal — não precisa de servidor rodando)

Rode só as suítes relacionadas a Transaction:

```
dotnet test tests/Denarius.Domain.Tests --filter FullyQualifiedName~TransactionTests
dotnet test tests/Denarius.Application.Tests --filter FullyQualifiedName~Transactions
```

Esperado: todos os testes passam. Juntas, elas exercitam todos os requisitos funcionais do
`spec.md` (FR-001…FR-021), exceto a própria camada HTTP, que o
`tests/Denarius.WebAPI.Tests/Transactions/TransactionsControllerTests.cs` cobre à parte (veja
`research.md` → Lacuna de cobertura de testes).

Ou a suíte completa, como pede o Princípio IV da constituição:

```
dotnet test
```

## Smoke test manual de ponta a ponta (também exercita a camada HTTP)

1. Suba a API: `dotnet run --project src/Denarius.WebAPI` (o perfil HTTP escuta em
   `http://localhost:5276`, conforme o `launchSettings.json`).
2. Crie uma categoria para vincular a transação (pule se já existir uma):
   ```
   curl -i -X POST http://localhost:5276/api/categories \
     -H "Content-Type: application/json" \
     -d '{"name":"Mercado","color":"#FF0000"}'
   ```
   Anote o `id` devolvido como `{categoryId}`.
3. Crie uma transação:
   ```
   curl -i -X POST http://localhost:5276/api/transactions \
     -H "Content-Type: application/json" \
     -d '{"description":"Compras da semana","date":"2026-09-15T00:00:00Z","value":-150.75,"categoryId":"{categoryId}"}'
   ```
   Esperado: `201 Created`, um cabeçalho `Location: /api/transactions/{id}` e um corpo que devolve
   a transação com o valor negativo preservado (História de usuário 1, cenário 1).
4. Liste as transações: `curl http://localhost:5276/api/transactions` — espere a transação nova no
   array, junto com todas as outras transações registradas, das mais recentes para as mais antigas
   (História de usuário 2). Depois filtre e reordene a lista (História de usuário 3):
   - `curl "http://localhost:5276/api/transactions?description=COMPRAS&dateRef=2026-09-01&type=out&categoryId={categoryId}"`
     — espere a transação nova (busca na descrição sem diferenciar maiúsculas de minúsculas,
     setembro de 2026, valor negativo, a categoria dela).
   - `curl "http://localhost:5276/api/transactions?type=in&categoryId={categoryId}"` — espere que
     ela não apareça (é dinheiro gasto).
   - `curl "http://localhost:5276/api/transactions?dateRef=2026-08-01&categoryId={categoryId}"` —
     espere que ela não apareça (outro mês).
   - `curl "http://localhost:5276/api/transactions?orderBy=value&asc=true"` — espere o array
     ordenado pelo valor com sinal, com a maior despesa primeiro.
   - `curl -i "http://localhost:5276/api/transactions?type=invalid"` — espere `400` com um corpo
     `ProblemDetails` de validação.
5. Atualize-a:
   ```
   curl -i -X PUT http://localhost:5276/api/transactions/{id} \
     -H "Content-Type: application/json" \
     -d '{"description":"","date":"2026-09-16T00:00:00Z","value":200.00,"categoryId":"{categoryId}"}'
   ```
   Esperado: `200`, data/valor alterados e `description` passa a ser `null` (em branco é
   normalizado para sem descrição — História de usuário 1, cenários 2 e 5).
6. Exclua-a: `curl -i -X DELETE http://localhost:5276/api/transactions/{id}` — espere `204`, sem
   recusa por dados dependentes (História de usuário 1, cenário 3; compare com a proteção de
   exclusão de Category).
7. Confirme a remoção: repita a lista sem filtros do passo 4 — a transação não deve mais aparecer.
8. Caso-limite de validação: repita o passo 3 com `"value":0` ou sem o `"date"` — espere `400` com
   um corpo `ProblemDetails` explicando o que está errado (História de usuário 1, cenário 4).
9. Caso-limite referencial: repita o passo 3 com um `categoryId` aleatório (por exemplo,
   `"00000000-0000-0000-0000-000000000000"`) — espere `404` explicando que a categoria não foi
   encontrada.

## Alteração de 2026-10-11: validação dos filtros no banco

Pré-requisito: Docker em execução (os testes do `Denarius.Infrastructure.Tests` usam Testcontainers). Os comandos partem
de `back/`.

1. **Linha de base (antes de alterar código)**: suba uma base descartável com as migrations atuais e gere 10.000
   transações (200 no mês de teste, 200 na categoria de teste) com `generate_series`. Suba a API contra ela e meça 20
   requisições de `GET /api/transactions?dateRef=<mês>` e de `...?categoryId=<id>`, com 3 de aquecimento descartadas antes; registre
   a mediana de cada uma na tabela de [research.md](./research.md#medição-de-base-preencher-no-implement).
2. **Comportamento**: `dotnet test`. Esperado: as quatro suítes passam, cobrindo a busca com e sem acentos, maiúsculas,
   espaços, `%`, `_`, `\` e descrição nula (história 3, cenários 7 a 10), a combinação de filtros, os desempates, a
   posição das transações sem descrição, a ordem "agua, Agua, água, Água, Banana" nas duas direções e os limites do mês
   (história 4), e as rotas e o formato da resposta sem mudança.
3. **Migration**: aplicar, reverter e reaplicar a da collation numa base descartável:
   ```bash
   dotnet ef database update --project src/Denarius.Infrastructure --startup-project src/Denarius.WebAPI
   dotnet ef database update <migration anterior> --project src/Denarius.Infrastructure --startup-project src/Denarius.WebAPI
   dotnet ef database update --project src/Denarius.Infrastructure --startup-project src/Denarius.WebAPI
   ```
4. **Desempenho (SC-008)**: repita a carga do passo 1 depois da implementação e preencha a coluna "depois" do
   research.md. Esperado: a razão antes/depois é pelo menos 5 para o mês e para a categoria.
