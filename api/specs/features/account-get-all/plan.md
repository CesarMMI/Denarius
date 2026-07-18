# account-get-all — Plan

Esta feature já está implementada. Este documento reflete o código real.

## Abordagem técnica

Fluxo Clean Architecture padrão do projeto (ver `specs/ARCHITECTURE.md`):

```
GET /api/accounts
  → AccountEndpoints.MapAccountEndpoints (Denarius.Api/Endpoints/AccountEndpoints.cs)
  → UserId extraído do ClaimsPrincipal (ClaimsPrincipalExtensions.GetUserId())
    → ListAccountsInput(UserId)
  → IListAccountsUseCase → ListAccountsUseCase (Denarius.Application/UseCases/Accounts/ListAccountsUseCase.cs)
      1. IAccountRepository.ListByUserAsync(userId) — busca já filtrada por dono
      2. accounts.Select(AccountOutput.FromEntity)
  → Results.Ok(result) — 200, IEnumerable<AccountOutput>
```

## Decisões locais

- `IAccountRepository.ListByUserAsync(Guid userId)` (`Denarius.Infrastructure/Persistence/Repositories/AccountRepository.cs`)
  filtra por `UserId` diretamente na query (`Where(a => a.UserId == userId)`) e ordena por `Name`
  (`OrderBy(a => a.Name)`) — a ordenação alfabética é uma decisão do repositório, não exposta como
  opção configurável pelo use case ou pelo endpoint.
- `ListAccountsUseCase` não faz nenhuma filtragem adicional: contas ativas e inativas são retornadas
  juntas, sem distinção — mesmo comportamento documentado em `.claude/use-cases/use-cases-account.md`.
- Sem paginação: a query retorna todas as contas do usuário de uma vez. Aceitável dado que o número
  de contas por usuário é tipicamente pequeno (dezenas, não milhares).

## Edge cases e como são tratados

- Usuário sem nenhuma conta → `ListByUserAsync` retorna lista vazia → endpoint responde 200 com `[]`
  (não é um erro).
- Requisição sem token JWT → 401, antes mesmo do use case ser executado (`RequireAuthorization()`
  no grupo de rotas).

## Alternativas descartadas

Nenhuma decisão de projeto alternativa registrada — implementação segue diretamente o padrão de
use case de listagem já estabelecido para os demais aggregates (Category, Transaction).
