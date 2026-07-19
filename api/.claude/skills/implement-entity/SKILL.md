---
name: implement-entity
description: >-
  Cria (ou estende) uma entidade de domínio completa no Denarius API — classe
  em Denarius.Domain/Entities, enum(s) associado(s), exceções de domínio,
  documentação em specs/entities/ e testes de domínio com xUnit — seguindo
  exatamente os padrões já usados em Account/Category/Transaction/User. É o
  primeiro passo do pipeline de uma entidade nova: deve rodar ANTES de
  implement-repository. Use quando o usuário pedir para criar uma entidade de
  domínio nova, adicionar uma propriedade ou regra de negócio a uma entidade
  existente, ou modelar um conceito de domínio. Também pode ser chamado
  diretamente via /implement-entity <Aggregate> [pedido adicional], ex.:
  /implement-entity Budget ou /implement-entity Account "adicionar campo
  Icon".
license: MIT
---

# Implementar Entidade de Domínio

Cria a classe de uma entidade de domínio do Denarius API, replicando
exatamente os padrões já usados em `Account`, `Category`, `Transaction` e
`User` — não é um tutorial genérico de DDD, é a receita específica deste
código-base. Cobre só `Denarius.Domain` e `Denarius.Domain.Tests`
(propriedades, construtor, métodos de mutação, exceções, enum, documentação e
testes). Persistência (EF configuration, repositório, migration) é
responsabilidade da skill `implement-repository`, que roda **depois** desta.

## Entrada

Os argumentos devem conter `{Aggregate}` (nome da entidade em PascalCase
singular, ex.: `Account`, `Budget`) e, opcionalmente, um pedido específico
(regras de negócio da entidade nova, ou "adicionar campo X" no modo extensão).

- Se `{Aggregate}` não vier nos argumentos, pergunte ao usuário antes de
  prosseguir — não invente o nome da entidade.
- Detecte automaticamente se é criação (não existe
  `src/Denarius.Domain/Entities/{Aggregate}.cs`) ou extensão (já existe) —
  isso muda o fluxo, ver "Criar vs. modificar" abaixo.

## Antes de fazer

Nesta ordem:

1. Rode `dotnet build` e `dotnet test`. Se algum falhar, **pare e reporte**
   antes de tocar em qualquer arquivo (regra geral do `CLAUDE.md`).
2. **Determine o formato da entidade** (propriedades e tipos, validações do
   construtor, métodos de mutação, relacionamentos, se precisa de enum) nesta
   ordem de prioridade — nunca invente uma regra de negócio que não venha de
   uma destas fontes:
   a. `specs/entities/{aggregate}.md`, se já existir (mesmo que parcial) — é
      a fonte mais autoritativa, pode vir de uma execução anterior desta
      skill.
   b. `specs/features/*/spec.md` e `specs/features/*/plan.md` — procure pelo
      nome do agregado; se uma feature já modelou essa entidade, use como
      fonte para as partes que ela cobre.
   c. Os argumentos/descrição passados no comando.
   d. Qualquer coisa que ainda faltar ou ficar ambígua depois de a–c:
      pergunte diretamente ao usuário (campos + tipos, regras de validação,
      métodos de mutação, entidades relacionadas, precisa de enum?).

   Se (a) e (b) divergirem sobre o formato de um campo, exponha o conflito ao
   usuário em vez de escolher uma das duas silenciosamente.
3. Confirme se `src/Denarius.Domain/Entities/{Aggregate}.cs` já existe, para
   saber se está em modo criação ou extensão.
4. Leia uma entidade análoga existente por inteiro (entidade + exceções +
   testes) como referência direta de estilo, escolhendo a mais parecida com o
   formato do novo agregado:
   - Caso simples, sem enum, sem validação cruzada entre campos → `Account`
     (`Entities/Account.cs`, `Exceptions/Accounts/`,
     `Entities/AccountTests.cs`).
   - Tem enum + comportamento dependente do tipo → `Category` (padrão de
     método-predicado como `AcceptsTransactionType()`).
   - Tem validação condicional entre campos (campo obrigatório/proibido
     dependendo de um tipo) → `Transaction`.

   Leia também o `specs/entities/{exemplo}.md` correspondente como molde de
   documentação.
5. Leia as tabelas "Onde ficam as peças", "Convenções de nomenclatura" e
   "Mapeamento de exceções → HTTP" em `specs/ARCHITECTURE.md` para confirmar
   caminhos, nomes e o motivo de toda exceção de domínio herdar
   `DomainException` — não repita essas tabelas na resposta, apenas siga-as.

## Criar vs. modificar

- **Criação nova** (arquivo da entidade ainda não existe): siga as 5 etapas
  numeradas abaixo por inteiro.
- **Extensão de entidade existente**:
  - Aditiva (método novo; propriedade nova opcional/com valor default que não
    muda a assinatura do construtor nem renomeia/remove nada existente) —
    pode prosseguir direto, só nas etapas afetadas.
  - Alterar a assinatura do construtor, renomear/remover uma propriedade ou
    método público, ou renomear um valor de enum **é alterar um contrato
    público** — pare e peça aprovação explícita do usuário antes de mexer em
    qualquer arquivo, listando consumidores conhecidos do formato atual (EF
    configuration/repositório em `Denarius.Infrastructure`, se
    `implement-repository` já rodou para este agregado; qualquer uso em
    Application/Api) — regra "Não alterar contratos públicos" do `CLAUDE.md`.
  - Em qualquer um dos dois casos: se a mudança afetar o que é persistido
    (propriedade nova/removida/renomeada, enum alterado), sinalize
    explicitamente em "Ao terminar" que `implement-repository` precisa rodar
    (de novo) depois, para atualizar a EF configuration e gerar uma migration
    nova — isso está fora do escopo desta skill.

## 1. Documentação

Local: `specs/entities/{aggregate-lowercase}.md`.

Mesma estrutura de seções de `specs/entities/account.md`, na mesma ordem:

```markdown
# {Aggregate}

{Parágrafo curto: o que a entidade representa e por que existe.}

## Propriedades

| Propriedade | Descrição |
|---|---|
| `Id` | Identificador único d[a/o] {aggregate} |
| ... | ... |

## Tipos (`{EnumName}`)

<!-- só incluir esta seção se a entidade tiver um enum associado -->

| Valor | Descrição |
|---|---|
| ... | ... |

## Regras de negócio

### Criação

- {uma regra por linha, referenciando o campo em backticks}

### {Nome da operação de mutação}

- Disponível via `MetodoDeMutacao(parametros)`
- {regras específicas dessa operação}

## Relacionamentos

- Pertence a um `{EntidadePai}`
- Possui zero ou mais `{EntidadeFilha}`
```

- Marque qualquer conteúdo não confirmado por uma fonte do passo 2 de "Antes
  de fazer" com `<!-- inferido do código — confirmar com o desenvolvedor -->`
  (mesma string literal exigida pelo `CLAUDE.md`, para manter consistência
  grepável em todo `specs/`, mesmo quando a inferência vier de uma descrição
  do usuário e não de código já existente).
- Se a entidade tiver uma FK para uma entidade existente, releia o arquivo
  `specs/entities/{pai}.md` imediatamente antes de editar (evite conteúdo em
  cache) e adicione ao `## Relacionamentos` dele o bullet reverso ("Possui
  zero ou mais `{Aggregate}`"), igual ao padrão cruzado já usado em
  `account.md`/`user.md`.

Regra: nenhuma linha de código é escrita antes de esta documentação existir e
refletir as regras acordadas ("Nunca implemente algo sem que exista
documentação correspondente", `CLAUDE.md`).

## 2. Enum (se necessário)

Local: `src/Denarius.Domain/Enums/{EnumName}.cs`. Só criar se o doc do passo 1
definir um.

```csharp
namespace Denarius.Domain.Enums;

public enum {EnumName}
{
    Valor1,
    Valor2,
}
```

Regra: namespace plano `Denarius.Domain.Enums`, um enum por arquivo, membros
em PascalCase, sem valores numéricos explícitos atribuídos. Nenhuma
conversão/serialização é declarada aqui — `HasConversion<int>()` é
responsabilidade da Infrastructure, fora do escopo desta skill.

## 3. Exceções de domínio

Local: `src/Denarius.Domain/Exceptions/{Aggregate}s/` (subpasta plural) para
regras específicas deste agregado. Só crie a subpasta se pelo menos uma
violação for realmente específica dele — reutilize uma exceção compartilhada
já existente na raiz de `Exceptions/` (`InvalidNameException`,
`InvalidColorException`) quando a regra já é coberta por uma delas; não
duplique.

```csharp
using Denarius.Domain.Exceptions;

namespace Denarius.Domain.Exceptions.{Aggregate}s;

public class Invalid{Campo}Exception : DomainException
{
    public Invalid{Campo}Exception() : base("{mensagem descritiva}.") { }
}
```

Toda exceção nova herda `DomainException`. Estilo de construtor conforme o
caso: sem parâmetro + mensagem fixa (regra única), mensagem obrigatória sem
default (mesmo tipo cobre violações distintas com textos diferentes), ou
mensagem com valor default (exceção pensada para ser compartilhada entre
agregados). Nomenclatura: `Invalid<Campo>Exception` ou `<Motivo>Exception`.

Regra: toda violação de regra de negócio lança uma subclasse de
`DomainException`, nunca `InvalidOperationException`/`ArgumentException` cru
— só `DomainException` (e subclasses) mapeia para HTTP 400 no
`ExceptionMiddleware` (`specs/ARCHITECTURE.md`). (`Transaction.MarkAsIncoming()`
e `LinkTransferPeer()` hoje lançam `InvalidOperationException` — é um desvio
conhecido do código existente, não um padrão a copiar.)

## 4. Classe da entidade

Local: `src/Denarius.Domain/Entities/{Aggregate}.cs`.

```csharp
using Denarius.Domain.Exceptions;
using Denarius.Domain.Exceptions.{Aggregate}s;

namespace Denarius.Domain.Entities;

public class {Aggregate}
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    // demais propriedades, todas { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    public {Aggregate}(Guid userId /*, demais parâmetros */)
    {
        // guard clauses primeiro, uma exceção específica por regra violada
        if (/* regra inválida */)
            throw new Invalid{Campo}Exception();

        Id = Guid.NewGuid();
        UserId = userId;
        // demais atribuições
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Update(/* campos mutáveis */)
    {
        // revalida os campos recebidos, atribui, e:
        UpdatedAt = DateTime.UtcNow;
    }
}
```

Regra: toda propriedade é `{ get; private set; }` — nenhuma com setter
público; único construtor público, validando tudo antes de atribuir;
`Id = Guid.NewGuid()` gerado no cliente (não pelo banco); `CreatedAt`/
`UpdatedAt = DateTime.UtcNow` no construtor; FK sempre como `Guid` simples,
nunca propriedade de navegação ou `ICollection<T>` (regra "Não usar
propriedades de navegação" do `CLAUDE.md`); toda mutação passa por método
nomeado que revalida seus próprios parâmetros e atualiza `UpdatedAt` (nunca um
setter genérico); métodos que representam uma transição de estado idempotente
(ex.: `Deactivate()`) não lançam exceção se chamados quando já não há efeito
— apenas retornam; zero dependência de `Application`/`Infrastructure`/`Api`.

## 5. Testes de domínio

Local: `tests/Denarius.Domain.Tests/Entities/{Aggregate}Tests.cs`.

```csharp
using Denarius.Domain.Entities;
using Denarius.Domain.Exceptions;
using Denarius.Domain.Exceptions.{Aggregate}s;

namespace Denarius.Domain.Tests.Entities;

public class {Aggregate}Tests
{
    private static {Aggregate} Valid{Aggregate}(Guid? userId = null) =>
        new(userId ?? Guid.NewGuid() /*, demais argumentos válidos */);

    // -------------------------------------------------------------------------
    // Creation — happy path
    // -------------------------------------------------------------------------

    [Fact]
    public void Create_WithValidFields_SetsPropertiesCorrectly()
    {
        // Arrange / Act / Assert
    }

    // -------------------------------------------------------------------------
    // Creation — errors
    // -------------------------------------------------------------------------

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_WithInvalid{Campo}_Throws(string? valor)
    {
        Assert.Throws<Invalid{Campo}Exception>(() =>
            new {Aggregate}(Guid.NewGuid() /*, valor no lugar do campo testado */));
    }

    // -------------------------------------------------------------------------
    // {NomeDoMetodoDeMutacao}
    // -------------------------------------------------------------------------

    [Fact]
    public void {Metodo}_{Cenario}_{ResultadoEsperado}()
    {
        // Arrange / Act / Assert
    }
}
```

- Fábrica estática privada de instância válida no topo da classe.
- Agrupe os testes por assunto sob o banner de 79 traços: criação caminho
  feliz, criação erros (um `[Theory]` por campo validado), um bloco por
  método de mutação — incluindo idempotência e casos de borda (use
  `Record.Exception` para provar que um caso idempotente *não* lança).
- Nomenclatura: `Metodo_Cenario_ResultadoEsperado`.
- Só `Assert.*`/`Assert.Throws<T>`/`Record.Exception` — sem mocking, sem
  FluentAssertions (o projeto de testes de domínio não referencia nenhuma
  dessas bibliotecas).

Regra: todo guard clause do construtor e toda regra de cada método de mutação
documentados no passo 1 têm pelo menos um teste correspondente — a suíte é o
critério de "pronto", não apenas a compilação.

## Consumo (fora do escopo desta skill)

Esta skill não toca em `Denarius.Infrastructure` (EF configuration,
repositório, migration, DI, testes de repositório) nem em
`Denarius.Application`/`Denarius.Api` (use cases, inputs/outputs, endpoints).
O próximo passo natural do pipeline é rodar `/implement-repository
{Aggregate}` para a camada de persistência; depois disso, o use case segue o
checklist já existente no `CLAUDE.md`.

## Ao terminar

Reporte:
- Modo usado (criação nova ou extensão) e, se extensão, se houve mudança de
  contrato que exigiu aprovação.
- Arquivos criados/modificados: documentação (própria e, se aplicável, do doc
  pai atualizado), enum, exceções, classe da entidade, testes.
- Se o formato persistido mudou (propriedade nova/removida/renomeada, enum
  alterado): aviso explícito para rodar `/implement-repository {Aggregate}`
  em seguida.
- Resultado de `dotnet build` e `dotnet test` após a implementação.
- O que ficou pendente (ex.: persistência via `implement-repository`, use
  case, endpoint — nenhum coberto por esta skill).
