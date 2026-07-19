---
name: implement-usecase
description: >-
  Implementa (ou estende) um use case do Denarius API na camada Application
  — interface em Interfaces/UseCases, Input, Output (se necessário),
  implementação do use case, registro em DependencyInjection e testes de
  use case — seguindo exatamente os padrões já usados em
  Account/Category/Transaction. É o terceiro passo do pipeline de uma
  entidade nova, depois de implement-entity e implement-repository, e vem
  antes de implement-endpoint (que expõe o use case via Minimal API). Use
  quando o usuário pedir para criar uma operação/use case novo (create,
  update, delete, get, list, ou uma ação de domínio específica) na camada de
  aplicação. Não cria nem altera endpoints da API — isso é responsabilidade
  da skill implement-endpoint. Também pode ser chamado diretamente via
  /implement-usecase <Aggregate> <Verbo> [pedido adicional], ex.:
  /implement-usecase Budget Create ou /implement-usecase Account "adicionar
  validação de saldo negativo".
license: MIT
---

# Implementar Use Case

Implementa um use case completo da camada `Denarius.Application` do
Denarius API, replicando exatamente os padrões já usados em `Account`,
`Category` e `Transaction`: não é um tutorial genérico de clean
architecture, é a receita específica deste código-base. Cobre
`Denarius.Application` e `Denarius.Application.Tests`. Assume que a
entidade (`implement-entity`) e o repositório (`implement-repository`) já
existem — esta é a terceira etapa do pipeline, antes de
`implement-endpoint` (que expõe o use case na `Denarius.Api`).

## Entrada

Os argumentos devem conter `{Aggregate}` (nome da entidade em PascalCase
singular, ex.: `Account`, `Budget`) e `{Verbo}` (a operação: `Create`,
`Update`, `GetById`, `List`, `Delete`/`Deactivate`, ou uma ação de domínio
específica como `Transfer`) e, opcionalmente, um pedido específico (regras de
validação da operação, o que o use case deve retornar).

- Se `{Aggregate}` ou `{Verbo}` não vierem nos argumentos e não puderem ser
  inferidos com segurança do pedido do usuário, pergunte antes de prosseguir
  — não invente nenhum dos dois.
- Detecte automaticamente se é criação (não existe
  `I{Verbo}{Aggregate}UseCase.cs`) ou extensão (já existe) — isso muda o
  fluxo, ver "Criar vs. modificar" abaixo.

## Antes de fazer

Nesta ordem:

1. Rode `dotnet build` e `dotnet test`. Se algum falhar, **pare e reporte**
   antes de tocar em qualquer arquivo (regra geral do `CLAUDE.md`).
2. Confirme que `src/Denarius.Domain/Entities/{Aggregate}.cs` e
   `I{Aggregate}Repository.cs` já existem. Esta skill só orquestra a camada
   Application sobre uma entidade e um repositório já prontos — se
   qualquer um dos dois não existir, é sinal de que `implement-entity`
   e/ou `implement-repository` ainda não rodaram para este agregado: pare e
   peça ao usuário para rodar a skill que falta primeiro, em vez de criar
   entidade ou repositório aqui.
3. **Determine o comportamento do use case** (o que valida, o que retorna,
   quais exceções lança) nesta ordem de prioridade — nunca invente uma regra
   que não venha de uma destas fontes:
   a. `specs/features/{feature-id}/spec.md` e `plan.md`, se já existirem —
      fonte mais autoritativa. `{feature-id}` é
      `{aggregate-lowercase}-{verbo-lowercase}` (ex.: `account-create`,
      `transaction-transfer`), mesmo padrão das 18 pastas já existentes em
      `specs/features/`.
   b. Pedido do usuário / argumentos do comando.
   c. Qualquer coisa que ainda faltar ou ficar ambígua depois de a–b:
      pergunte diretamente ao usuário.

   **Gate de documentação faltante**: se `specs/features/{feature-id}/` não
   existir, use a skill `generate-feature-docs` para criá-la (ou crie o
   markdown manualmente, marcando inferências com `<!-- inferido do código —
   confirmar com o desenvolvedor -->`) **antes** de escrever qualquer código
   — regra "Nunca implemente algo sem que exista documentação
   correspondente" do `CLAUDE.md`.
4. Verifique se `I{Verbo}{Aggregate}UseCase.cs` já existe, para saber se
   está em modo criação ou extensão.
5. Leia um use case existente inteiro (interface + input + output +
   implementação + testes de use case) como referência direta de estilo,
   escolhendo o mais parecido com a necessidade do novo use case:
   - CRUD simples, sem orquestração entre agregados → `Account`
     (`CreateAccountUseCase`, `UpdateAccountUseCase`,
     `GetAccountByIdUseCase`, `ListAccountsUseCase`,
     `DeactivateAccountUseCase`).
   - Efeito colateral em outro agregado antes de mutar/remover →
     `DeleteCategoryUseCase` (chama `NullifyTransactionCategoriesAsync`
     antes de deletar).
   - Orquestra múltiplos agregados, valida regra cruzada entre entidades, ou
     precisa de um output composto → `Transaction`
     (`CreateTransactionUseCase`, `CreateTransferUseCase` +
     `CreateTransferOutput`, `ListTransactionsUseCase` para validação de
     filtros/query params).
6. Leia a tabela "Convenções de nomenclatura" em `specs/ARCHITECTURE.md`
   (interface, input, output, output composto, request, endpoint mapper) e a
   tabela "Mapeamento de exceções → HTTP" — não repita essas tabelas na
   resposta, apenas siga-as.

## Criar vs. modificar

- **Criação nova** (interface do use case ainda não existe): siga as 5
  etapas numeradas abaixo por inteiro.
- **Extensão**:
  - Adicionar um use case novo (`{Verbo}` diferente) para o mesmo agregado é
    aditivo — pode prosseguir direto.
  - Alterar a assinatura de `Execute` ou do `Input`/`Output` de um use case
    já existente **é alterar um contrato público** — pare e peça aprovação
    explícita do usuário antes de mexer em qualquer arquivo, listando
    consumidores conhecidos (endpoint e `Request` correspondente na
    `Denarius.Api`, testes de use case, testes de endpoint, e frontend, se
    existir) — regra "Não alterar contratos públicos" do `CLAUDE.md`. Um
    Input/Output alterado exige rodar `implement-endpoint` em seguida para
    ajustar o endpoint afetado.

## 1. Interface do use case

Local: `src/Denarius.Application/Interfaces/UseCases/<Aggregate>/I{Verbo}{Aggregate}UseCase.cs`.

```csharp
namespace Denarius.Application.Interfaces.UseCases.{Aggregate}s;

public interface I{Verbo}{Aggregate}UseCase : IUseCase<{Verbo}{Aggregate}Input, Task<{Aggregate}Output>>;
```

Variantes de retorno, conforme o que o use case faz:
- Ação sem valor de retorno (ex.: `Deactivate`, `Delete`) →
  `IUseCase<{Verbo}{Aggregate}Input, Task>`.
- Listagem → `IUseCase<{Verbo}{Aggregate}Input, Task<IEnumerable<{Aggregate}Output>>>`.
- Output composto (ex.: `CreateTransfer`) → substitua `{Aggregate}Output`
  pelo output composto correspondente.

Regra: `Execute` na interface base `IUseCase<T, U>` (já existe em
`Denarius.Application/Interfaces/IUseCase.cs`) não é declarado `async` — só
a implementação usa `async`/`await`; um verbo por interface, nunca uma
interface genérica cobrindo mais de uma operação.

## 2. Input

Local: `src/Denarius.Application/Inputs/<Aggregate>/{Verbo}{Aggregate}Input.cs`.

```csharp
namespace Denarius.Application.Inputs.{Aggregate}s;

public record {Verbo}{Aggregate}Input(Guid UserId /*, demais parâmetros */);
```

Regra: `UserId` é sempre o primeiro parâmetro (hierarquia de propriedade do
`CLAUDE.md` — toda query a repositório filtra por `UserId`); sem data
annotations, sem lógica — só dados.

## 3. Output (se necessário)

Local: `src/Denarius.Application/Outputs/<Aggregate>/{Aggregate}Output.cs`
(reaproveite se já existir um output para este agregado — a maioria dos
use cases do mesmo agregado compartilha o mesmo `{Aggregate}Output`).

```csharp
using Denarius.Domain.Entities;

namespace Denarius.Application.Outputs.{Aggregate}s;

public record {Aggregate}Output(
    Guid Id,
    // demais propriedades expostas
    DateTime CreatedAt,
    DateTime UpdatedAt)
{
    public static {Aggregate}Output FromEntity({Aggregate} entity) => new(
        entity.Id,
        // demais propriedades
        entity.CreatedAt,
        entity.UpdatedAt);
}
```

Para um use case que precisa agrupar mais de um output (ex.:
`CreateTransferOutput`, que junta duas transações), use um output composto
sem `FromEntity()`:

```csharp
public record {Verbo}{Aggregate}Output({Aggregate}Output Primeiro, {Aggregate}Output Segundo);
```

Regra: nunca retornar a entidade de domínio diretamente do use case — sempre
mapear via `FromEntity()` (regra do `CLAUDE.md`).

## 4. Implementação do use case

Local: `src/Denarius.Application/UseCases/<Aggregate>/{Verbo}{Aggregate}UseCase.cs`.

```csharp
using Denarius.Application.Inputs.{Aggregate}s;
using Denarius.Application.Interfaces.UseCases.{Aggregate}s;
using Denarius.Application.Outputs.{Aggregate}s;
using Denarius.Domain.Exceptions.{Aggregate}s;
using Denarius.Domain.Interfaces.Repositories;

namespace Denarius.Application.UseCases.{Aggregate}s;

public class {Verbo}{Aggregate}UseCase(I{Aggregate}Repository {aggregate}Repository, IUnitOfWork unitOfWork)
    : I{Verbo}{Aggregate}UseCase
{
    public async Task<{Aggregate}Output> Execute({Verbo}{Aggregate}Input input)
    {
        var entity = await {aggregate}Repository.GetByIdAsync(input.{Aggregate}Id, input.UserId);
        if (entity is null)
            throw new {Aggregate}NotFoundException(input.{Aggregate}Id);

        entity.{MetodoDeMutacao}(/* parâmetros */);
        await {aggregate}Repository.UpdateAsync(entity);
        await unitOfWork.CommitAsync();

        return {Aggregate}Output.FromEntity(entity);
    }
}
```

- Injete via construtor primário só os repositórios/`IUnitOfWork`
  realmente usados — use cases só de leitura (`GetById`, `List`) não
  injetam `IUnitOfWork`.
- `GetByIdAsync(id, userId)` retorna `null` quando não encontra (nunca
  lança) — é o use case que decide lançar `{Aggregate}NotFoundException`;
  isso também é o que garante isolamento entre usuários (uma entidade de
  outro usuário nunca é encontrada, então sempre cai no mesmo 404).
- Toda mutação passa por um método nomeado da entidade (`Update`,
  `Deactivate`, `ApplyDelta`, etc.) — nunca um setter.
- Regras de validação que dependem de mais de uma entidade (ex.:
  `!category.AcceptsTransactionType(input.Type)`,
  `sourceAccount.CurrencyCode != destinationAccount.CurrencyCode`) ficam no
  use case, não na entidade — a entidade só valida a si mesma.
- Um use case pode chamar mais de um repositório (orquestração entre
  agregados, ex.: `CreateTransactionUseCase` usa `IAccountRepository` +
  `ICategoryRepository` + `ITransactionRepository`), mas sempre com
  exatamente **um** `await unitOfWork.CommitAsync()`, como penúltima
  instrução, depois de todas as escritas nos repositórios.

Regra: nunca chamar `unitOfWork.CommitAsync()` mais de uma vez por use case
(regra do `CLAUDE.md`); toda busca final é sempre por `userId` — nunca
retornar dado de outro usuário.

## 5. Registro em DI

Arquivo: `src/Denarius.Application/DependencyInjection.cs`. Adicione, no
grupo de registros do agregado (mesma ordem CRUD já usada no arquivo —
Create, Get, List, Update, Delete/Deactivate):

```csharp
services.AddScoped<I{Verbo}{Aggregate}UseCase, {Verbo}{Aggregate}UseCase>();
```

Regra: sempre `AddScoped` — nunca `Singleton` nem `Transient` (mesmo padrão
dos repositórios).

## Testes

### Testes de use case

Local: `tests/Denarius.Application.Tests/UseCases/<Aggregate>/{Verbo}{Aggregate}UseCaseTests.cs`.

```csharp
using Denarius.Application.Inputs.{Aggregate}s;
using Denarius.Application.UseCases.{Aggregate}s;
using Denarius.Domain.Exceptions.{Aggregate}s;
using Denarius.Domain.Interfaces.Repositories;
using NSubstitute;

namespace Denarius.Application.Tests.UseCases.{Aggregate}s;

public class {Verbo}{Aggregate}UseCaseTests
{
    private readonly I{Aggregate}Repository _{aggregate}Repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly {Verbo}{Aggregate}UseCase _sut;

    public {Verbo}{Aggregate}UseCaseTests()
    {
        _{aggregate}Repository = Substitute.For<I{Aggregate}Repository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _sut = new {Verbo}{Aggregate}UseCase(_{aggregate}Repository, _unitOfWork);
    }

    // -------------------------------------------------------------------------
    // Execute — happy path
    // -------------------------------------------------------------------------

    [Fact]
    public async Task Execute_WithValidInput_Returns{Aggregate}()
    {
        // Arrange / Act / Assert
    }

    [Fact]
    public async Task Execute_WithValidInput_CallsRepositoryAndCommit()
    {
        // Act
        // await _{aggregate}Repository.Received(1).UpdateAsync(Arg.Any<{Aggregate}>());
        // await _unitOfWork.Received(1).CommitAsync();
    }

    // -------------------------------------------------------------------------
    // Execute — errors
    // -------------------------------------------------------------------------

    [Fact]
    public async Task Execute_WithNonExistent{Aggregate}_Throws{Aggregate}NotFoundException()
    {
        // Arrange: _{aggregate}Repository.GetByIdAsync(id, userId).Returns(({Aggregate}?)null);
        // Assert.ThrowsAsync<{Aggregate}NotFoundException>(...)
    }
}
```

- `_sut` construído manualmente no construtor (nunca via container de DI).
- Agrupe por assunto sob o banner de 79 traços: caminho feliz, erros (um
  teste por guard clause/exceção que o use case pode lançar).
- Nomenclatura: `Execute_Cenario_ResultadoEsperado` (um arquivo existente,
  `GetCategoryByIdUseCaseTests`, usa `ExecuteAsync_...` — é um desvio
  conhecido do código atual, não copie).
- "Não encontrado" e "pertence a outro usuário" são o mesmo teste: stub
  `GetByIdAsync(id, userId)` retornando `null` para o par `id`/`userId` do
  cenário — não existe checagem de posse separada no use case, ela é
  inteiramente delegada à assinatura do repositório.
- Valide `Received(1)`/`DidNotReceive()` nas chamadas ao repositório e ao
  `CommitAsync()`, tanto no caminho feliz quanto nos erros (uma exceção
  lançada antes da escrita não pode ter chamado `UpdateAsync`/`AddAsync`
  nem `CommitAsync`).

Regra: cada use case novo tem cobertura mínima de caminho feliz e um teste
por guard clause/exceção que ele pode lançar. Cobertura de endpoint (um
teste por status code documentado em `.Produces()`/`.ProducesProblem()`) é
responsabilidade da skill `implement-endpoint`.

## Exceções (se a regra ainda não existir)

Se o use case precisar de uma exceção nova, decida onde ela mora:
- **`Denarius.Domain.Exceptions.{Aggregate}s`** (herda `DomainException`) se
  a invariante pertence à validade intrínseca da entidade ou de uma relação
  entre entidades já modelada no domínio (ex.: `InvalidCategoryException`,
  `InvalidTransferException`, lançadas de dentro de um use case mas
  definidas no domínio).
- **`Denarius.Application.Exceptions.{Aggregate}s`** (herda
  `NotFoundException` ou `AppException`) se é sobre o estado de um recurso
  relativo a este request específico (ex.: `AccountNotFoundException`,
  `InactiveAccountException`, `InvalidDateRangeException`).

Mapear a exceção nova para um status HTTP (`ExceptionMiddleware`,
`.ProducesProblem()`) é responsabilidade da skill `implement-endpoint` — só
sinalize, no relatório final, se este use case introduziu uma exceção que
ainda não existia.

## Consumo (fora do escopo desta skill)

Esta skill não toca em `Denarius.Api` (endpoint Minimal API, `Request`,
testes de endpoint, `ExceptionMiddleware`) nem cria cliente frontend. O
próximo passo do pipeline (`implement-entity` → `implement-repository` →
`implement-usecase` → `implement-endpoint`) é rodar `/implement-endpoint
{Aggregate} {Verbo}` para expor o use case via Minimal API.

## Ao terminar

Reporte:
- Modo usado (criação nova ou extensão) e, se extensão, se houve mudança de
  contrato que exigiu aprovação.
- Arquivos criados/modificados, por camada: interface, input, output (ou
  reaproveitado), implementação, DI, testes de use case,
  `specs/features/{feature-id}/` (se criado por esta execução).
- Se uma exceção nova foi introduzida, e onde ela mora.
- Resultado de `dotnet build` e `dotnet test` após a implementação.
- Lembrete de que o use case ainda não está exposto via API — próximo passo
  é `/implement-endpoint {Aggregate} {Verbo}`.
