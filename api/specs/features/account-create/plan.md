# account-create — Plan

Esta feature já está implementada. Este documento reflete o código real.

## Abordagem técnica

Fluxo Clean Architecture padrão do projeto (ver `specs/ARCHITECTURE.md`):

```
POST /api/accounts
  → AccountEndpoints.MapAccountEndpoints (Denarius.Api/Endpoints/AccountEndpoints.cs)
  → CreateAccountRequest → CreateAccountInput (UserId extraído do ClaimsPrincipal via
    ClaimsPrincipalExtensions.GetUserId())
  → ICreateAccountUseCase → CreateAccountUseCase (Denarius.Application/UseCases/Accounts/CreateAccountUseCase.cs)
      1. new Account(userId, name, currencyCode, color) — entidade valida Name,
         CurrencyCode e Color no construtor, lança InvalidNameException /
         InvalidCurrencyCodeException / InvalidColorException se inválidos
      2. IAccountRepository.AddAsync(account)
      3. IUnitOfWork.CommitAsync() — única chamada de commit do use case
  → AccountOutput.FromEntity(account)
  → Results.Created($"/api/accounts/{result.Id}", result) — 201
```

## Decisões locais

- Toda validação de entrada (`Name`, `CurrencyCode`, `Color`) vive inteiramente no construtor
  da entidade `Account` — o use case não duplica essas regras, apenas deixa a exceção de
  domínio propagar.
- `CurrencyCode` é validado com uma checagem estrutural simples (3 letras maiúsculas,
  `IsValidCurrencyCode` em `Denarius.Domain/Entities/Account.cs`), não contra uma lista real
  de códigos ISO 4217 — qualquer combinação de 3 letras maiúsculas passa (ex: `"XXX"`).
- `Balance` e `IsActive` não fazem parte do input — são sempre `0` e `true`, atribuídos
  dentro do construtor da entidade, não configuráveis pelo chamador.
- Resposta HTTP usa `Results.Created` com `Location: /api/accounts/{id}`, consistente com
  o endpoint `GET /api/accounts/{id}` (`GetAccountById`), que já existe.

## Edge cases e como são tratados

- `CurrencyCode` minúsculo (`"brl"`) ou com case misto (`"Brl"`) → `InvalidCurrencyCodeException`
  — a checagem exige `char.IsUpper` em todos os caracteres, não normaliza para maiúsculo.
- `CurrencyCode` com tamanho diferente de 3 (`"BR"`, `"BRLL"`) → `InvalidCurrencyCodeException`.
- `Name` ou `Color` só com espaços em branco (`"   "`) → mesma exceção do campo vazio
  (`string.IsNullOrWhiteSpace`).

## Alternativas descartadas

Nenhuma decisão de projeto alternativa registrada — implementação segue diretamente o
padrão de use case já estabelecido para os demais aggregates (User, Category, Transaction).
