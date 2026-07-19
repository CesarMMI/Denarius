---
name: implement-repository
description: >-
  Implementa (ou estende) um Repository de uma entidade de domínio em todas as
  camadas do Denarius API — interface em Denarius.Domain, DbSet e
  ApplyConfiguration no DenariusDbContext, IEntityTypeConfiguration em
  Denarius.Infrastructure, implementação do repositório, registro em
  DependencyInjection, migration do EF Core e testes com RepositoryTestBase
  (SQLite in-memory) — seguindo exatamente os padrões já usados em
  Account/Category/Transaction/User. Use quando o usuário pedir para criar um
  repositório novo, adicionar persistência para uma entidade de domínio, ou
  adicionar/alterar um método de um repositório existente. Também pode ser
  chamado diretamente via /implement-repository <Aggregate> [pedido
  adicional], ex.: /implement-repository Budget ou /implement-repository
  Account "adicionar ExistsByNameAsync".
license: MIT
---

# Implement Repository

Implementa um Repository completo do Denarius API, replicando exatamente os
padrões já usados em `Account`, `Category`, `Transaction` e `User` — não é um
tutorial genérico de repository pattern, é a receita específica deste
código-base.

## Entrada

Os argumentos devem conter `{Aggregate}` (nome da entidade em PascalCase
singular, ex.: `Account`, `Budget`) e, opcionalmente, um pedido específico
(um método novo, uma alteração pontual).

- Se `{Aggregate}` não vier nos argumentos, pergunte ao usuário antes de
  prosseguir — não invente o nome da entidade.
- Detecte automaticamente se é criação (não existe
  `I{Aggregate}Repository.cs`) ou extensão (já existe) — isso muda o fluxo,
  ver "Criar vs. modificar" abaixo.

## Antes de fazer

Nesta ordem:

1. Rode `dotnet build` e `dotnet test`. Se algum falhar, **pare e reporte**
   antes de tocar em qualquer arquivo (regra geral do `CLAUDE.md`).
2. Confirme que `src/Denarius.Domain/Entities/{Aggregate}.cs` e
   `specs/entities/{aggregate}.md` existem. Esta skill só implementa a
   camada de persistência — se qualquer um dos dois não existir, é sinal de
   que a skill `implement-entity` ainda não rodou para este agregado: pare e
   peça ao usuário para rodar `/implement-entity {Aggregate}` primeiro, em
   vez de criar a entidade ou a documentação aqui.
3. Verifique se `I{Aggregate}Repository.cs` e `{Aggregate}Repository.cs` já
   existem, para saber se está em modo criação ou extensão.
4. Leia um repositório existente inteiro (interface + configuration + impl +
   testes) como referência direta de estilo, escolhendo o mais parecido com
   a necessidade do novo agregado: `Account` (caso simples), `Transaction`
   (filtros opcionais + bulk `AddRangeAsync`/`UpdateRangeAsync`), `Category`
   (`ExecuteUpdateAsync` em lote) ou `User` (unicidade / lookup sem userId).
5. Leia a tabela "Convenções de nomenclatura" e "Onde ficam as peças" em
   `specs/ARCHITECTURE.md` para confirmar caminhos e nomes — não repita
   essas tabelas na resposta, apenas siga-as.

## Criar vs. modificar

- **Criação nova** (interface ainda não existe): siga as 7 etapas abaixo
  inteiras.
- **Extensão de repositório existente**:
  - Adicionar um método novo à interface é uma mudança aditiva (não quebra
    consumidores existentes) — pode prosseguir implementando nas camadas
    afetadas (interface, implementação, testes, e configuração/migration
    somente se envolver coluna ou relação nova).
  - Alterar a assinatura de um método existente, remover um método, ou
    mudar seu contrato de retorno **é alterar um contrato público** — pare e
    peça aprovação explícita do usuário antes de mexer em qualquer arquivo,
    listando quais use cases/testes hoje dependem da assinatura atual (regra
    "Não alterar contratos públicos" do `CLAUDE.md`).

## 1. Interface de domínio

Local: `src/Denarius.Domain/Interfaces/Repositories/I{Aggregate}Repository.cs`.

```csharp
public interface I{Aggregate}Repository
{
    Task<{Aggregate}?> GetByIdAsync(Guid id, Guid userId);
    Task<IEnumerable<{Aggregate}>> ListByUserAsync(Guid userId /*, filtros opcionais */);
    Task AddAsync({Aggregate} entity);
    Task UpdateAsync({Aggregate} entity);
    Task DeleteAsync({Aggregate} entity);
    // + métodos específicos do agregado, só os que um use case real vai chamar
}
```

Regra: declare somente métodos que casos de uso reais vão consumir agora —
nada especulativo — e toda assinatura que busca/filtra dados de um usuário
recebe `userId` como parâmetro (não existe repositório genérico/base neste
projeto).

## 2. DbSet e registro no DbContext

Arquivo: `src/Denarius.Infrastructure/Persistence/DenariusDbContext.cs`.

- Adicione `public DbSet<{Aggregate}> {Aggregate}s => Set<{Aggregate}>();`
  junto aos DbSets existentes.
- Registre `modelBuilder.ApplyConfiguration(new {Aggregate}Configuration());`
  dentro de `OnModelCreating`.

Regra: o DbContext nunca expõe métodos de repositório — só DbSets e
`ApplyConfiguration`.

## 3. Configuração EF

Local: `src/Denarius.Infrastructure/Persistence/Configurations/{Aggregate}Configuration.cs`,
implementando `IEntityTypeConfiguration<{Aggregate}>`.

```csharp
public class {Aggregate}Configuration : IEntityTypeConfiguration<{Aggregate}>
{
    public void Configure(EntityTypeBuilder<{Aggregate}> builder)
    {
        builder.ToTable("{Aggregate}s");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedNever();
        builder.Property(e => e.UserId).IsRequired();
        // demais propriedades: IsRequired()/HasMaxLength() em strings,
        // HasConversion<int>() em enums, HasColumnType("TEXT") em decimal
        builder.HasOne<OutraEntidade>().WithMany()
               .HasForeignKey(e => e.OutraEntidadeId)
               .OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(e => e.UserId);
        // builder.Ignore(e => e.PropriedadeComputada);
    }
}
```

Regra: FKs são sempre unidirecionais — `HasOne<T>().WithMany()` sem lambda no
`WithMany` (regra "Não usar propriedades de navegação" do `CLAUDE.md`); nunca
adicione `ICollection<T>` na entidade principal para satisfazer o EF.

## 4. Implementação do repositório

Local: `src/Denarius.Infrastructure/Persistence/Repositories/{Aggregate}Repository.cs`.

```csharp
public class {Aggregate}Repository(DenariusDbContext context) : I{Aggregate}Repository
{
    public async Task<{Aggregate}?> GetByIdAsync(Guid id, Guid userId) =>
        await context.{Aggregate}s.FirstOrDefaultAsync(e => e.Id == id && e.UserId == userId);

    public async Task<IEnumerable<{Aggregate}>> ListByUserAsync(Guid userId) =>
        await context.{Aggregate}s.Where(e => e.UserId == userId).OrderBy(e => /* campo natural */).ToListAsync();

    public async Task AddAsync({Aggregate} entity) => await context.{Aggregate}s.AddAsync(entity);

    public Task UpdateAsync({Aggregate} entity) { context.{Aggregate}s.Update(entity); return Task.CompletedTask; }

    public Task DeleteAsync({Aggregate} entity) { context.{Aggregate}s.Remove(entity); return Task.CompletedTask; }
}
```

Regra: nunca chame `SaveChangesAsync`/`unitOfWork.CommitAsync()` dentro do
repositório — quem decide quando commitar é o use case, via `IUnitOfWork`.
Única exceção: operações em lote com `ExecuteUpdateAsync`/`ExecuteDeleteAsync`
(ex.: `CategoryRepository.NullifyTransactionCategoriesAsync`), que executam
imediatamente e legitimamente não passam pelo `IUnitOfWork`.

## 5. Registro em DI

Arquivo: `src/Denarius.Infrastructure/DependencyInjection.cs`. Adicione, no
grupo de registros de repositório logo após `AddDbContext`:

```csharp
services.AddScoped<I{Aggregate}Repository, {Aggregate}Repository>();
```

Regra: sempre `AddScoped` (mesmo lifetime do `DbContext`) — nunca
`Singleton` nem `Transient`.

## 6. Migration

```
dotnet ef migrations add <NomeDaMigration> \
  --project src/Denarius.Infrastructure \
  --startup-project src/Denarius.Api
```

Regra: nunca rode `dotnet ef database update` manualmente e nunca edite uma
migration já commitada — migrations pendentes são aplicadas automaticamente
no startup (`RUN_MIGRATIONS_ON_STARTUP`, ver `specs/DEVELOPMENT.md`); para
corrigir algo, gere uma migration nova.

## 7. Testes

Local: `tests/Denarius.Infrastructure.Tests/Repositories/{Aggregate}RepositoryTests.cs`,
herdando `RepositoryTestBase` (SQLite in-memory).

```csharp
public class {Aggregate}RepositoryTests : RepositoryTestBase
{
    private readonly {Aggregate}Repository _sut;

    public {Aggregate}RepositoryTests()
    {
        _sut = new {Aggregate}Repository(Context);
    }

    private static {Aggregate} Valid{Aggregate}(Guid userId /*, overrides */) =>
        new(userId, /* args válidos */);

    // -------------------------------------------------------------------------
    // NomeDoMetodo
    // -------------------------------------------------------------------------

    [Fact]
    public async Task NomeDoMetodo_Cenario_Resultado()
    {
        // Arrange / Act / Assert
    }
}
```

- SUT é sempre construído manualmente no construtor do teste (`new
  {Aggregate}Repository(Context)`), nunca via container de DI.
- Se o agregado tiver FK, crie um helper privado de seed que persiste a
  entidade relacionada primeiro (padrão `SeedAccountAsync` em
  `TransactionRepositoryTests`).
- Agrupe os testes por método sob o banner de 79 traços acima.
- Depois de uma escrita via `_sut`, chame `await Context.SaveChangesAsync()`
  explicitamente (o repositório não commita sozinho) e depois valide via
  `CreateFreshContext()` (prova persistência real, não cache do change
  tracker) ou diretamente na entidade retornada.
- Cobertura mínima por método: caminho feliz; não encontrado/lista vazia;
  isolamento entre usuários (usuário B nunca vê dado do usuário A); garantias
  de ordenação, se houver `OrderBy`.

Regra: todo método que filtra por `userId` precisa de um teste que prove que
dados de outro usuário nunca vazam — essa é a regra mais crítica do
`CLAUDE.md` ("Toda query a repositório filtra por UserId").

## Consumo (fora do escopo desta skill)

Um use case injeta `I{Aggregate}Repository` + `IUnitOfWork`, chama o(s)
método(s) do repositório (que só ficam no change tracker do
`DenariusDbContext`) e finaliza com exatamente um
`await unitOfWork.CommitAsync()`. Esta skill não cria o use case — se o
usuário também pedir isso, rode `/implement-usecase {Aggregate}` em
seguida.

## Ao terminar

Reporte:
- Modo usado (criação nova ou extensão) e, se extensão, se houve mudança de
  contrato que exigiu aprovação.
- Arquivos criados/modificados, por camada (interface, DbContext,
  configuration, implementação, DI, migration, testes, spec de entidade se
  criada).
- Nome da migration gerada.
- Resultado de `dotnet build` e `dotnet test` após a implementação.
- O que ficou pendente (ex.: wiring do use case, se não foi pedido).
