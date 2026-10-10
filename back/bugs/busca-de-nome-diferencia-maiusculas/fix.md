# Correção: a busca de categorias por nome diferencia maiúsculas de minúsculas

- **Slug**: busca-de-nome-diferencia-maiusculas · **Corrigido em**: 2026-10-09 · **Avaliação**: ./assessment.md · **Status**: applied

## Mudanças

- `src/Denarius.Infrastructure/Repositories/CategoryRepository.cs`: o filtro compara `lower(unaccent(...))` nos dois lados.
- `DenariusDbContext.cs` declara a extensão `unaccent`. A migration `20261010013826_AddUnaccentExtension` a cria, e o `Down()` a remove com SQL explícito, porque o Npgsql não remove extensões.
- `ListCategoriesUseCase.cs`: o `name` passa por `Trim()`, com o teste `Execute_WithNameFilterSurroundedBySpaces_*`.
- Projeto novo `tests/Denarius.Infrastructure.Tests`, com o `PostgreSqlFixture` e os testes `CategoryRepositoryTests` (maiúsculas, acentos, sem resultado, `%` e `_`). Usa o `Testcontainers.PostgreSql` 4.15.0, sem vulnerabilidades conhecidas (a 4.16.0 tem só um dia), com a imagem `postgres:18.0`.
- O contrato `categories-api.yaml` vai para a 1.1.0. O `back/AGENTS.md` cita o quarto projeto de testes e a exigência do Docker (constituição 3.0.1).

## Verificação

- Antes da correção, 6 casos do repositório e o do trim falhavam. Depois dela, o `dotnet test` passou (51 + 140 + 63 + 10), e o build não teve avisos. O revisor aprovou, e os achados de tamanho foram corrigidos. A migration foi aplicada, revertida e reaplicada numa base descartável, depois apagada. A extensão existia, sumiu e voltou.

## Desvios e pendências

- O trim do `name` entrou no escopo por pedido do usuário depois do portão. O `assessment.md` foi encurtado até 40 linhas a pedido do revisor, sem mudar as decisões.
- Pendências: atualizar a FR-017 da spec 002 do front, que ainda diz que a busca diferencia acentos (depende de aval), e rodar `dotnet ef database update` na base de dev antes de reiniciar a API, porque sem isso a busca responde 500.
