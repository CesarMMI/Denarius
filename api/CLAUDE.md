# CLAUDE.md — Denarius API

API do Denarius: C# 13 / .NET 9, Clean Architecture, Minimal API. Este arquivo
é um índice — a referência técnica vive nos documentos abaixo, não aqui.
Não repita o conteúdo deles nesta página.

## Documentação

| Documento | Conteúdo |
|---|---|
| [`specs/DEVELOPMENT.md`](specs/DEVELOPMENT.md) | Stack, bibliotecas, build/test, como rodar em dev/produção/bare metal, migrations, configuração (`.env` × `appsettings.json`) |
| [`specs/ARCHITECTURE.md`](specs/ARCHITECTURE.md) | Camadas e dependências entre projetos, onde fica cada artefato, convenções de nomenclatura, mapeamento de exceções → HTTP, endpoints REST, decisões de infraestrutura |
| [`specs/entities/`](specs/entities/) | Entidades de domínio (`User`, `Account`, `Category`, `Transaction`), enums e regras de negócio |
| [`specs/features/<feature>/`](specs/features/) | Spec (`spec.md`), plano técnico (`plan.md`) e tarefas (`tasks.md`) de cada feature/use case existente |

**Hierarquia de propriedade:** `User` → `Account`, `Category`, `Transaction`.
Toda query a repositório filtra por `UserId` — nunca retornar dados de outro
usuário.

## Skills disponíveis

| Skill | Uso |
|---|---|
| `generate-feature-docs` (`.claude/skills/generate-feature-docs/SKILL.md`) | Gera `spec.md`/`plan.md`/`tasks.md` de uma feature nova em `specs/features/{FEATURE_ID}/`. Invocar via `/generate-feature-docs <feature-id> <descrição>` |
| `implement-entity` (`.claude/skills/implement-entity/SKILL.md`) | Cria (ou estende) a classe de uma entidade de domínio — propriedades, construtor com validações, métodos de mutação, exceções de domínio, enum e documentação/testes correspondentes. Deve rodar antes de `implement-repository`. Invocar via `/implement-entity <Aggregate>` |
| `implement-repository` (`.claude/skills/implement-repository/SKILL.md`) | Implementa (ou estende) um Repository completo — interface, EF configuration, implementação, DI, migration e testes — para uma entidade de domínio. Invocar via `/implement-repository <Aggregate>` |
| `implement-usecase` (`.claude/skills/implement-usecase/SKILL.md`) | Implementa (ou estende) um use case completo na camada Application — interface, input, output, implementação, DI e testes de use case. Roda depois de `implement-entity`/`implement-repository`, antes de `implement-endpoint`. Invocar via `/implement-usecase <Aggregate> <Verbo>` |
| `implement-endpoint` (`.claude/skills/implement-endpoint/SKILL.md`) | Expõe (ou altera) um use case já existente como endpoint Minimal API — Request, rota, status codes, `.Produces()`/`.ProducesProblem()`, mapeamento de exceções e testes de endpoint. Roda depois de `implement-usecase`. Invocar via `/implement-endpoint <Aggregate> <Verbo>` |

## Regras para o Agente

### Antes de qualquer tarefa
1. Executar `dotnet build` — se falhar, **parar e reportar o erro** antes de qualquer alteração.
2. Executar `dotnet test` — se falhar, **parar e reportar** quais testes estão quebrando.

### Durante a tarefa
- Trabalhar em **uma tarefa por vez**.
- Nunca declarar "done" sem `dotnet build` e `dotnet test` passando sem erros.
- Ao adicionar um novo use case: usar a skill `implement-usecase` e, para
  expor via API, `implement-endpoint` em seguida (ver tabela "Skills
  disponíveis" acima).
- Ao adicionar uma entidade de domínio: usar as skills `implement-entity` e,
  em seguida, `implement-repository` (ver tabela "Skills disponíveis" acima).

### Documentação faltante

Se durante a implementação você perceber que um contexto não possui
documentação markdown correspondente (entidade sem `*.md` em
`specs/entities/`, feature/use case sem pasta em `specs/features/`, etc.):

1. Use a skill `generate-feature-docs` para features novas, ou crie o
   markdown correspondente **antes** de implementar.
2. Documente o que o contexto deve conter com base no que inferiu do código existente.
3. Sinalize com `<!-- inferido do código — confirmar com o desenvolvedor -->` em cada bloco que não estava explícito.
4. Referencie o novo arquivo na tabela "Documentação" acima, se for um documento novo de topo (não uma feature individual).
5. Só então prossiga com a implementação.

**Nunca implemente algo sem que exista documentação correspondente.**

### Testes
- Testes de **domínio**: xUnit puro, sem mocking, em `Denarius.Domain.Tests/Entities/`
- Testes de **use case**: NSubstitute para repositórios e serviços, em `Denarius.Application.Tests/UseCases/<Aggregate>/`
- Testes de **repositório**: herdar `RepositoryTestBase` (SQLite in-memory), em `Denarius.Infrastructure.Tests/Repositories/`
- Testes de **endpoints**: usar `IClassFixture<WebApplicationFactory<Program>>` com a extensão `WebApplicationFactoryExtensions.CreateTestClient()` (definida em `Denarius.Api.Tests/Shared/CustomWebApplicationFactory.cs`), NSubstitute para use cases injetados, em `Denarius.Api.Tests/Endpoints/`

### Ao terminar
Descrever:
- O que foi feito (arquivos criados/modificados)
- O que ainda está pendente ou requer atenção do desenvolvedor

## O que NÃO fazer

- **Não modificar o schema/chaves** de `appsettings.json`, `appsettings.json.example`, `appsettings.Development.json` ou `api/.env.example` sem avisar explicitamente ao usuário. `appsettings.json` (real) é gitignored e contém segredo — nunca sugerir commitá-lo nem imprimir seu conteúdo real em respostas.
- **Não instalar pacotes NuGet** (`<PackageReference>`) sem confirmar com o usuário.
- **Não alterar contratos públicos** — interfaces de use case (`IXxxUseCase`), records de Input/Output e request models da API — sem aprovação explícita. Quebrar um contrato implica reescrever todos os consumidores e potencialmente quebrar clientes da API.
- **Não usar Controllers** — o projeto usa exclusivamente Minimal APIs com grupos e extensões de `IEndpointRouteBuilder`.
- **Não expor propriedades de entidade de domínio como `public set`** — todas as mutações passam por métodos do domínio (ex: `Update()`, `ApplyDelta()`, `Deactivate()`).
- **Não usar propriedades de navegação (`ICollection<T>`) nem relacionamentos bidirecionais entre entidades** — toda FK é uma propriedade `Guid` simples e unidirecional (só a entidade "filha" referencia o `Id` da "pai").
- **Não chamar `unitOfWork.CommitAsync()` mais de uma vez por use case** — cada use case abre uma única unidade de trabalho.
- **Não retornar entidades de domínio diretamente** nos outputs — sempre mapear para um record `XxxOutput` com `FromEntity()`.
- **Não criar arquivos fora das camadas corretas** — respeitar a estrutura de projetos descrita em [`specs/ARCHITECTURE.md`](specs/ARCHITECTURE.md).
