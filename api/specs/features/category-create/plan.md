# category-create — Plan

Esta feature já está implementada. Este documento reflete o código real.

## Abordagem técnica

Fluxo Clean Architecture padrão do projeto (ver `specs/ARCHITECTURE.md`):

```
POST /api/categories
  → CategoryEndpoints.MapCategoryEndpoints (Denarius.Api/Endpoints/CategoryEndpoints.cs)
  → CreateCategoryRequest → CreateCategoryInput (UserId extraído do ClaimsPrincipal via
    ClaimsPrincipalExtensions.GetUserId())
  → ICreateCategoryUseCase → CreateCategoryUseCase (Denarius.Application/UseCases/Categories/CreateCategoryUseCase.cs)
      1. new Category(userId, name, color, type) — entidade valida Name e Color no
         construtor, lança InvalidNameException / InvalidColorException se inválidos
      2. ICategoryRepository.AddAsync(category)
      3. IUnitOfWork.CommitAsync() — única chamada de commit do use case
  → CategoryOutput.FromEntity(category)
  → Results.Created($"/api/categories/{result.Id}", result) — 201
```

## Decisões locais

- Toda validação de entrada (`Name`, `Color`) vive inteiramente no construtor da entidade
  `Category` — o use case não duplica essas regras, apenas deixa a exceção de domínio propagar.
- `Type` não tem validação própria no construtor: é um enum (`CategoryType`, `Income`/`Expense`),
  então qualquer valor fora do range é impossível de representar em C#. Não existe um caso real
  de "`Type` não informado" no código — na deserialização do request, a ausência do campo faz
  o binder usar o valor padrão do enum (`Income = 0`), não lança uma exceção de validação.
- Resposta HTTP usa `Results.Created` com `Location: /api/categories/{id}`, consistente com o
  endpoint `GET /api/categories/{id}` (`GetCategoryById`), que já existe.

## Edge cases e como são tratados

- `Name` ou `Color` só com espaços em branco (`"   "`) → mesma exceção do campo vazio
  (`string.IsNullOrWhiteSpace`), tanto no construtor de `Category` quanto refletido nos testes
  de `CreateCategoryUseCaseTests`.
- `Type` ausente no JSON do request → não gera erro; a categoria é criada como `Income` (valor
  padrão do enum). Diverge do texto original em `.claude/use-cases/use-cases-category.md`, que
  lista "`Type` não informado" como erro — o código não implementa essa validação.

## Alternativas descartadas

Nenhuma decisão de projeto alternativa registrada — implementação segue diretamente o padrão de
use case já estabelecido para os demais aggregates (User, Account, Transaction), o mesmo usado
em `specs/features/account-create/plan.md`.
