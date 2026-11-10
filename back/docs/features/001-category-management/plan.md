# Plano: Gestão de categorias

**Spec**: [spec.md](./spec.md) · **Pesquisa**: [research.md](./research.md) · **Contrato**: [contracts/categories-api.yaml](./contracts/categories-api.yaml)

Plano retroativo: documenta o design já entregue, sem trabalho novo.

## Conferência com as regras

- Camadas (B1): a validação fica em `Category` e `Color` (Domain), a orquestração nos casos de uso (Application), o EF Core em Infrastructure e o `CategoriesController` só chama casos de uso.
- Compatibilidade da API (B2): documenta `api/categories` como está, sem mudança.
- Migrations (B3): a tabela `Categories` nasce em `20260813225006_InitialCreate`, com `Down()` não destrutivo.
- Segurança (II): validação no Domain, consultas por LINQ, erros como `ProblemDetails` pelo `GlobalExceptionHandler` sem detalhe interno.
- Performance (III): a busca por nome roda no banco; a agregação de uso ainda carrega todas as transações em memória (ver Decisões).

## Mudanças

- **Domínio**: `Category`, `Color`, `ICategoryRepository`. Campos e regras em [categoria](../../dominio/categoria.md).
- **Aplicação**: casos de uso Create, Update, Delete e List, com `IO/Categories/`.
- **Infraestrutura**: `CategoryConfiguration`, `CategoryRepository`, FK `Transactions.CategoryId` com `ON DELETE RESTRICT`.
- **API**: `GET`, `POST`, `PUT` e `DELETE` em `api/categories`. Convenções e erros em [api.md](../../api.md).

## Decisões

Detalhes e alternativas em [research.md](./research.md).

- Validação só no Domain, sem biblioteca externa nem data annotations.
- Exclusão protegida duas vezes: `ExistsByCategoryIdAsync` no caso de uso (400 com explicação) e FK `RESTRICT` como rede de segurança.
- Uso por categoria agregado em memória na camada Application (`GroupBy`), por simplicidade na escala single-tenant; o próprio research aponta revisitar se o volume crescer.
- Erros mapeados por um `GlobalExceptionHandler` compartilhado (404, 400, 500).

## Testes previstos

- Domain: `CategoryTests`.
- Application: um conjunto por caso de uso em `UseCases/Categories/`.
- WebAPI.Tests: `CategoriesControllerTests` cobre roteamento, binding da query do `List` e o `Location` do `201`.

## Verificação

1. `dotnet test` com as suítes verdes.
2. Com a API no ar: `POST /api/categories` devolve `201` e `Location`, com `transactionCount` e `balance` zerados; `GET` lista ordenado por nome; `PUT` com cor `#0F0` devolve `#00FF00`; `DELETE` devolve `204`.
3. Nome vazio ou cor inválida devolve `400` com `ProblemDetails`; excluir categoria com transação devolve `400`.
4. `GET /api/categories?dateRef=<mês>` conta só as transações do mês.
