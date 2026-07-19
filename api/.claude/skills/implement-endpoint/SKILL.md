---
name: implement-endpoint
description: >-
  Expõe (ou altera) um use case do Denarius API como endpoint Minimal API na
  camada Denarius.Api — Request (se necessário), mapeamento de rota em
  Map{Aggregate}Endpoints, status codes, .Produces()/.ProducesProblem(),
  mapeamento de exceções novas em ExceptionMiddleware e testes de endpoint
  — seguindo exatamente os padrões já usados em
  Account/Category/Transaction. É o quarto e último passo do pipeline de
  uma entidade nova, depois de implement-entity, implement-repository e
  implement-usecase. Assume que o use case já existe na camada Application
  — não cria nem altera use cases, inputs, outputs ou regras de negócio,
  isso é responsabilidade da skill implement-usecase. Use quando o usuário
  pedir para criar/adicionar/expor um endpoint da API, ligar um use case já
  pronto a uma rota HTTP, ou ajustar status codes/rotas/Request de um
  endpoint existente. Também pode ser chamado diretamente via
  /implement-endpoint <Aggregate> <Verbo> [pedido adicional], ex.:
  /implement-endpoint Budget Create ou /implement-endpoint Account
  "retornar 422 quando a conta estiver inativa".
license: MIT
---

# Implementar Endpoint

Expõe um use case já existente do Denarius API como endpoint HTTP na camada
`Denarius.Api`, replicando exatamente os padrões já usados em `Account`,
`Category` e `Transaction`: não é um tutorial genérico de Minimal API, é a
receita específica deste código-base. Cobre `Denarius.Api/Endpoints`,
`Denarius.Api/Requests`, `Denarius.Api/Middleware/ExceptionMiddleware.cs` e
`Denarius.Api.Tests`. Assume que o use case (`implement-usecase`) já
existe — esta é a quarta e última etapa do pipeline.

## Entrada

Os argumentos devem conter `{Aggregate}` (nome da entidade em PascalCase
singular, ex.: `Account`, `Budget`) e `{Verbo}` (a operação já implementada
como use case: `Create`, `Update`, `GetById`, `List`,
`Delete`/`Deactivate`, ou uma ação de domínio específica como `Transfer`)
e, opcionalmente, um pedido específico (rota/verbo HTTP diferente do
padrão, status codes esperados, campos do `Request`).

- Se `{Aggregate}` ou `{Verbo}` não vierem nos argumentos e não puderem ser
  inferidos com segurança do pedido do usuário, pergunte antes de
  prosseguir — não invente nenhum dos dois.
- Detecte automaticamente se é criação (a rota para `{Verbo}{Aggregate}`
  ainda não existe em `Map{Aggregate}Endpoints`) ou extensão/ajuste (já
  existe) — isso muda o fluxo, ver "Criar vs. modificar" abaixo.

## Antes de fazer

Nesta ordem:

1. Rode `dotnet build` e `dotnet test`. Se algum falhar, **pare e reporte**
   antes de tocar em qualquer arquivo (regra geral do `CLAUDE.md`).
2. Confirme que
   `src/Denarius.Application/Interfaces/UseCases/<Aggregate>/I{Verbo}{Aggregate}UseCase.cs`
   já existe. Esta skill só liga a camada `Denarius.Api` a um use case já
   pronto — se ele não existir, é sinal de que `implement-usecase` ainda
   não rodou para este verbo/agregado: pare e peça ao usuário para rodar
   `/implement-usecase {Aggregate} {Verbo}` primeiro, em vez de criar o use
   case aqui.
3. Leia a implementação do use case
   (`src/Denarius.Application/UseCases/<Aggregate>/{Verbo}{Aggregate}UseCase.cs`)
   inteira para saber exatamente: o `Input` que ele espera (quais campos
   além de `UserId`), o `Output` que devolve, e **todas** as exceções que
   pode lançar (direta ou indiretamente) — isso determina o `Request`, o
   corpo do handler e todos os `.ProducesProblem()` do endpoint. Nunca
   adivinhe uma exceção que o use case não lança.
4. **Determine a rota e o comportamento HTTP** nesta ordem de prioridade —
   nunca invente o que não vier de uma destas fontes:
   a. `specs/features/{feature-id}/spec.md` e `plan.md`, se já existirem —
      fonte mais autoritativa. `{feature-id}` é
      `{aggregate-lowercase}-{verbo-lowercase}` (ex.: `account-create`,
      `transaction-transfer`).
   b. Padrão já estabelecido em `specs/ARCHITECTURE.md` ("Endpoints REST")
      para o mesmo verbo em outros agregados (`Create` → `POST`, `Update`
      → `PUT`, `GetById`/`List` → `GET`, `Delete`/`Deactivate` → `DELETE`).
   c. Pedido do usuário / argumentos do comando.
   d. Qualquer coisa que ainda faltar ou ficar ambígua depois de a–c:
      pergunte diretamente ao usuário.

   **Gate de documentação faltante**: se `specs/features/{feature-id}/` não
   existir, use a skill `generate-feature-docs` para criá-la (ou crie o
   markdown manualmente, marcando inferências com `<!-- inferido do código
   — confirmar com o desenvolvedor -->`) **antes** de escrever qualquer
   código — regra "Nunca implemente algo sem que exista documentação
   correspondente" do `CLAUDE.md`.
5. Leia um endpoint existente inteiro (`Map{Aggregate}Endpoints` + Request,
   se houver, + testes de endpoint) como referência direta de estilo,
   escolhendo o mais parecido com o verbo HTTP e o formato de retorno do
   novo endpoint — `AccountEndpoints.cs` cobre os quatro verbos CRUD +
   deactivate; `TransactionEndpoints.cs` cobre um endpoint com output
   composto (`POST /transactions/transfer`) e filtros de query string
   (`ListTransactionsUseCase`).
6. Leia a tabela "Convenções de nomenclatura" (linha "Request (API)",
   "Endpoint mapper") e a tabela "Mapeamento de exceções → HTTP" em
   `specs/ARCHITECTURE.md` — não repita essas tabelas na resposta, apenas
   siga-as.

## Criar vs. modificar

- **Rota nova** (verbo ainda não mapeado para este agregado): siga as
  etapas numeradas abaixo por inteiro.
- **Ajuste**:
  - Adicionar `.ProducesProblem()` que faltava, corrigir `.WithSummary()`,
    ou mapear uma exceção nova no `ExceptionMiddleware` é aditivo — pode
    prosseguir direto.
  - Alterar o método HTTP, o path, o formato do `Request` de um endpoint já
    existente, ou o status code de sucesso **é alterar um contrato público
    da API** — pare e peça aprovação explícita do usuário antes de mexer
    em qualquer arquivo, listando consumidores conhecidos (testes de
    endpoint, frontend, e qualquer client externo, se existir) — regra
    "Não alterar contratos públicos" do `CLAUDE.md`.

## 1. Request (se o método precisar de corpo)

Local: `src/Denarius.Api/Requests/<Aggregate>/{Verbo}{Aggregate}Request.cs`.
Só campos que vêm do corpo — nunca `UserId`, que sempre vem do
`ClaimsPrincipal`, nem o `Id` do recurso, que vem da rota.

```csharp
namespace Denarius.Api.Requests.{Aggregate}s;

public record {Verbo}{Aggregate}Request(/* campos do corpo, na mesma ordem do Input (menos UserId/Id) */);
```

`GET`/`DELETE` sem corpo não precisam de `Request`; `GET` com filtros usa
parâmetros de query direto na assinatura do handler (ver
`ListTransactionsUseCase`/`TransactionEndpoints.cs` como referência), não um
`Request`.

## 2. Endpoint Minimal API

Local: `src/Denarius.Api/Endpoints/<Aggregate>Endpoints.cs`, dentro de
`Map{Aggregate}Endpoints` (extensão de `IEndpointRouteBuilder` — crie o
arquivo/grupo se ainda não existir para este agregado; grupo tem
`.RequireAuthorization()`).

```csharp
group.MapPut("/{id:guid}", async (Guid id, {Verbo}{Aggregate}Request request, ClaimsPrincipal user, I{Verbo}{Aggregate}UseCase useCase) =>
{
    var result = await useCase.Execute(new {Verbo}{Aggregate}Input(user.GetUserId(), id, request.Campo));
    return Results.Ok(result);
})
.WithName("{Verbo}{Aggregate}")
.WithSummary("{Descrição curta da operação}")
.Produces<{Aggregate}Output>()
.ProducesProblem(400)
.ProducesProblem(401)
.ProducesProblem(404);
```

- `ClaimsPrincipal user` + `user.GetUserId()` (extensão já existente em
  `Denarius.Api/Extensions/ClaimsPrincipalExtensions.cs`) é sempre a fonte
  do `UserId` — nunca aceito no `Request` nem na query string.
- Status code de sucesso por verbo: `Results.Created($"/api/.../{result.Id}", result)`
  para criação; `Results.Ok(result)` para get/update/list; `Results.NoContent()`
  para delete/deactivate (sem corpo de retorno).
- `.ProducesProblem(404)` só se o use case (lido no passo 3 de "Antes de
  fazer") pode lançar uma `NotFoundException`; `.ProducesProblem(422)` só
  se pode lançar algo como `InactiveAccountException`; `.ProducesProblem(401)`
  em todos (grupo tem `.RequireAuthorization()`) — nunca declare um
  `.ProducesProblem()` para uma exceção que o use case não lança.

Regra: nunca Controllers — só grupos/extensões de `IEndpointRouteBuilder`
(regra do `CLAUDE.md`); nunca expor `UserId` como parâmetro vindo do
cliente.

## 3. Mapeamento de exceção → HTTP (se necessário)

Toda `NotFoundException` nova já mapeia para HTTP 404 automaticamente
(`ExceptionMiddleware` faz `switch` por tipo base), assim como
`DomainException`/`AppException` para 400. Se a exceção que o use case
lança precisar de um status diferente de 400, adicione um `case`
específico para ela em `src/Denarius.Api/Middleware/ExceptionMiddleware.cs`,
**antes** dos `case` de `DomainException`/`AppException` — siga a tabela
"Mapeamento de exceções → HTTP" de `specs/ARCHITECTURE.md`, não a repita
aqui.

## 4. Atualizar a lista de rotas em ARCHITECTURE.md

Adicione a rota nova (ou ajuste a existente) na seção "Endpoints REST" de
`specs/ARCHITECTURE.md`, mantendo o agrupamento por agregado e o
alinhamento das colunas já usado no arquivo.

## Testes de endpoint

Local: `tests/Denarius.Api.Tests/Endpoints/<Aggregate>EndpointsTests.cs`,
`IClassFixture<WebApplicationFactory<Program>>` +
`WebApplicationFactoryExtensions.CreateTestClient()` (definida em
`tests/Denarius.Api.Tests/Shared/CustomWebApplicationFactory.cs`).

```csharp
public class {Aggregate}EndpointsTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly I{Verbo}{Aggregate}UseCase _{verbo}{Aggregate} = Substitute.For<I{Verbo}{Aggregate}UseCase>();
    private readonly HttpClient _client;

    public {Aggregate}EndpointsTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateTestClient(services =>
        {
            services.AddScoped<I{Verbo}{Aggregate}UseCase>(_ => _{verbo}{Aggregate});
        });
    }

    [Fact]
    public async Task {Verbo}{Aggregate}_CallsUseCaseWithCurrentUserId()
    {
        // Arrange: _{verbo}{Aggregate}.Execute(Arg.Any<...Input>()).Returns(...)
        // Act: await _client.PutAsJsonAsync/GetAsync/PostAsJsonAsync(...)
        // Assert status code + await _{verbo}{Aggregate}.Received(1).Execute(Arg.Is<...Input>(i => i.UserId == TestAuthHandler.UserId));
    }
}
```

Regra: pelo menos um teste de endpoint por status code documentado nos
`.Produces()`/`.ProducesProblem()` — incluindo o `401` sem `Authorization`
header, se ainda não houver um teste genérico cobrindo isso para o grupo.

## Consumo (fora do escopo desta skill)

Esta é a última etapa do pipeline (`implement-entity` →
`implement-repository` → `implement-usecase` → `implement-endpoint`). Esta
skill não cria cliente frontend nem altera o use case, `Input`/`Output`
subjacentes — se o comportamento do endpoint estiver errado por causa da
lógica do use case (não da camada HTTP), o ajuste é em `implement-usecase`.

## Ao terminar

Reporte:
- Modo usado (rota nova ou ajuste) e, se ajuste, se houve mudança de
  contrato que exigiu aprovação.
- Arquivos criados/modificados: request (se houver), endpoint,
  `ExceptionMiddleware` (se alterado), `specs/ARCHITECTURE.md` (rota
  adicionada/ajustada), testes de endpoint.
- Resultado de `dotnet build` e `dotnet test` após a implementação.
- O que ficou pendente (ex.: frontend consumindo o novo endpoint, se não
  foi pedido).
