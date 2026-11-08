# Quickstart: Cor da categoria nas transações do mês

Pré-requisito: Docker em execução (os testes do repositório usam Testcontainers). De dentro de `back/`:

1. `dotnet test`: as quatro suítes passam, incluindo os testes novos de `color` no use case, no repositório (teste novo em `TransactionRepositoryTests`) e no controller.
2. API no ar (`docker compose up`): `GET http://localhost:5276/api/reports/transactions?month=2026-11` devolve, em cada item, `color` igual à cor da categoria em `GET /api/categories` (SC-001), e os campos antigos inalterados (SC-002).
3. Mude a cor de uma categoria e repita a chamada: os itens dela trazem a cor nova (FR-003).
4. SC-003: com o log do EF no nível de informação, a chamada mostra uma só consulta; compare o tempo com o de `main` na mesma massa de dados (até 110%).
