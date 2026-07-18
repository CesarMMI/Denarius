# account-update — Plan

Esta feature já está implementada. Este documento reflete o código real.

## Abordagem técnica

Fluxo Clean Architecture padrão do projeto (ver `specs/ARCHITECTURE.md`):

```
PUT /api/accounts/{id}
  → AccountEndpoints.MapAccountEndpoints (Denarius.Api/Endpoints/AccountEndpoints.cs)
  → { id } da rota + UpdateAccountRequest(Name, Color) + UserId extraído do ClaimsPrincipal
    (ClaimsPrincipalExtensions.GetUserId())
    → UpdateAccountInput(UserId, AccountId, Name, Color)
  → IUpdateAccountUseCase → UpdateAccountUseCase (Denarius.Application/UseCases/Accounts/UpdateAccountUseCase.cs)
      1. IAccountRepository.GetByIdAsync(accountId, userId) — busca já filtrada por dono
      2. account is null → lança AccountNotFoundException(accountId)
      3. account.Update(name, color) — valida e muta a entidade (Denarius.Domain/Entities/Account.cs)
      4. IAccountRepository.UpdateAsync(account)
      5. IUnitOfWork.CommitAsync() — única chamada de commit do use case
  → AccountOutput.FromEntity(account)
  → Results.Ok(result) — 200
```

## Decisões locais

- `IAccountRepository.GetByIdAsync(Guid id, Guid userId)` já filtra a query por dono
  (`FirstOrDefaultAsync(a => a.Id == id && a.UserId == userId)`), mesmo padrão usado em
  `account-get-by-id`. "Conta não existe" e "conta pertence a outro usuário" chegam ao use case
  como o mesmo `null` — ambos os casos viram `AccountNotFoundException` (mesmo princípio de
  não-enumeração usado em `Login`, ver `specs/ARCHITECTURE.md`).
- A validação de `Name`/`Color` vazios ou nulos vive inteiramente em `Account.Update()`
  (`InvalidNameException`, `InvalidColorException`) — o use case não duplica essa validação,
  apenas propaga o que a entidade lançar.
- `Account.Update(name, color)` não recebe `currencyCode` como parâmetro — a assinatura do método
  já impede a alteração da moeda por construção, não por uma checagem em runtime.
- `UpdateAccountInput`/`UpdateAccountRequest` só carregam `Name` e `Color` — não existe caminho
  para o chamador enviar `CurrencyCode`, `Balance` ou `IsActive` nesta feature.
- Contas inativas (`IsActive = false`) não são filtradas pela query — o endpoint permite atualizar
  `Name`/`Color` de uma conta desativada normalmente.
- `AccountNotFoundException` herda `NotFoundException` → mapeada para HTTP 404 pelo
  `ExceptionMiddleware`; `InvalidNameException`/`InvalidColorException` herdam `DomainException` →
  mapeadas para HTTP 400.

## Edge cases e como são tratados

- `AccountId` que nunca existiu → `AccountNotFoundException` → 404.
- `AccountId` existente mas de outro usuário → mesmo resultado do caso acima, `AccountNotFoundException`
  → 404 (repositório já filtra por `UserId` na query, não há checagem de posse separada no use case).
- `Name` vazio, só espaços ou nulo → `InvalidNameException` → 400.
- `Color` vazia, só espaços ou nula → `InvalidColorException` → 400.
- Requisição sem token JWT → 401, antes mesmo do use case ser executado (`RequireAuthorization()`
  no grupo de rotas).
- Conta inativa recebendo update → sucesso normal, `IsActive` permanece `false` (não é alterado por
  este fluxo).

## Alternativas descartadas

Nenhuma decisão de projeto alternativa registrada — implementação segue diretamente o padrão de
use case de escrita por id já estabelecido para os demais aggregates (Category, Transaction).
