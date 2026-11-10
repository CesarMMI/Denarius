# Pesquisa da Fase 0: Gestão de transações

Este é um plano retroativo, então "pesquisa" aqui significa confirmar as decisões já embutidas no
código e nos testes entregues, e não explorar opções para código ainda não escrito. Cada item
abaixo resolve o que, de outro modo, seria um `NEEDS CLARIFICATION` no Contexto técnico.

## Tecnologia de persistência

- **Decisão**: PostgreSQL via migrations code-first do EF Core, com o `DenariusDbContext`
  compartilhado por todo o projeto — o mesmo banco que [001-category-management](../001-category-management/research.md)
  usa.
- **Justificativa**: Herdado da stack do projeto, e não decidido por feature. `Transaction` não tem
  nenhum requisito que justifique outro banco.
- **Alternativas consideradas**: Nenhuma específica de `Transaction`.

## Filtro e ordenação da lista (História de usuário 3, adicionada em 2026-09-24)

Até 2026-09-24, a lista não tinha filtro, ordenação nem paginação: o `ListTransactionsUseCase`
recebia uma entrada `object?` ignorada e devolvia todas as transações na ordem em que o banco as
produzisse. A História de usuário 3 substituiu isso pelas decisões abaixo.

> **Substituído em 2026-10-11**: as decisões "Onde ficam o filtro e a ordenação", "Busca por descrição", "Filtro por
> mês" e a parte de "Ordenação" que carrega as categorias em memória foram trocadas pelas da seção
> [Alteração de 2026-10-11](#alteração-de-2026-10-11-filtros-e-ordenação-no-banco). Ficam aqui como histórico.

### Onde ficam o filtro e a ordenação

- **Decisão**: O `ListTransactionsUseCase` carrega todas as transações pelo
  `ITransactionRepository.GetAllAsync()`, que não mudou, e aplica todos os filtros e a ordenação
  em memória. O `ITransactionRepository` não é alterado.
- **Justificativa**: Segue o padrão já estabelecido do `ListCategoriesUseCase` (que já carrega
  todas as transações a cada requisição para os seus agregados), mantém toda a lógica nova em
  `Denarius.Application`, onde o `Denarius.Application.Tests` a cobre (Princípio IV da constituição
  — não há projeto de testes de Infrastructure para cobrir lógica de consulta de repositório), e
  não custa mais que a lista anterior sem filtros na escala declarada desta feature.
- **Alternativas consideradas**: Levar os filtros para o repositório como uma consulta do EF Core
  (`Where`/`OrderBy` traduzidos para SQL) — melhor quando os volumes crescerem, mas levaria a
  lógica para uma camada sem testes e ampliaria o `ITransactionRepository` com parâmetros de filtro
  que a camada Domain teria de expressar sem os enums da Application. Adiado até a escala exigir.

### Busca por descrição

- **Decisão**: Busca parcial, sem diferenciar maiúsculas de minúsculas (`OrdinalIgnoreCase`), no
  texto buscado já com trim; texto em branco/só com espaços significa sem filtro; transações sem
  descrição nunca aparecem.
- **Justificativa**: Não diferenciar maiúsculas de minúsculas é o que a folha de filtros do
  front-end já faz localmente, e as descrições das transações são texto livre, em que o usuário
  raramente lembra se usou maiúsculas ou minúsculas.
- **Alternativas consideradas**: Copiar exatamente a busca por nome das categorias — rejeitada
  porque aquela roda como `Contains` em SQL no PostgreSQL e, por isso, diferencia maiúsculas de
  minúsculas; isso não foi alterado aqui, mas fica registrado como uma inconsistência conhecida
  entre as duas listas.

### Filtro por mês (`dateRef`)

- **Decisão**: A mesma semântica do `ListCategoriesUseCase`: qualquer data dentro de um mês
  seleciona o mês do calendário inteiro, do primeiro instante até o último tick, inclusive.
- **Justificativa**: Um único significado de "mês" em toda a API; o front-end já envia
  `YYYY-MM-01` para as categorias e pode reaproveitar isso.
- **Alternativas consideradas**: Um intervalo de datas explícito `from`/`to` — mais flexível, mas
  nenhuma tela atual precisa dele, e ele divergiria das categorias.

### Filtro por tipo

- **Decisão**: Enum `TransactionType` `All` (padrão), `In` (valor > 0), `Out` (valor < 0), com
  binding da query string sem diferenciar maiúsculas de minúsculas.
- **Justificativa**: "In"/"Out" seguem os termos exibidos ao usuário ("Entradas"/"Saídas") e os
  valores `all`/`in`/`out` que o front-end já usa, que fazem binding direto nele. Zero é
  impossível (a entidade o rejeita), então os dois sinais cobrem todas as transações.
- **Alternativas consideradas**: Um `bool?` anulável, como o `withTransaction` das categorias —
  rejeitado porque uma escolha de três estados fica mais clara como enum com um `All` explícito.

### Ordenação

- **Decisão**: Enum `TransactionOrderField` `Date` (padrão), `Description`, `Value`,
  `CategoryName`, com `asc` padrão `false` (data decrescente, mais recentes primeiro). `Value`
  ordena pelo valor com sinal. `CategoryName` carrega as categorias via
  `ICategoryRepository.GetAllAsync(null)` — só quando essa ordenação é pedida — e ordena por uma
  busca de CategoryId → nome.
- **Justificativa**: Mais recentes primeiro é o padrão natural de um histórico de transações e
  corresponde ao que o mock do front-end já mostra. Carregar os nomes das categorias só para essa
  ordenação evita uma consulta extra em todas as outras requisições.
- **Alternativas consideradas**: Usar `asc` padrão `true`, por consistência com as categorias —
  rejeitado porque listaria a transação mais antiga primeiro. Adicionar uma navigation property
  `Category` com um join em SQL, ou adicionar `categoryName` ao `TransactionOutput` — duas mudanças
  maiores (entidade/mapeamento, ou formato da resposta) desnecessárias para atender à ordenação.

### Parâmetros de query inválidos

- **Decisão**: Sem validação própria — um valor desconhecido de `type`/`orderBy` ou um
  `categoryId` malformado falha no model binding, e o `[ApiController]` devolve `400`
  `ValidationProblem` antes de o caso de uso rodar.
- **Justificativa**: O comportamento do framework já dá um erro claro e consistente; as categorias
  contam com o mesmo comportamento para o `orderBy`.
- **Alternativas consideradas**: Voltar silenciosamente para os padrões — rejeitado porque
  esconderia bugs dos clientes.

## Alteração de 2026-10-11: filtros e ordenação no banco

Cada decisão cita o código lido. As verificações que dependem do PostgreSQL real (Docker) viram testes do implement.

### Onde fica a consulta

- **Decisão**: novo método `ITransactionRepository.ListAsync(TransactionListQuery)` no `Domain`, implementado no
  `TransactionRepository` com `AsNoTracking()`. O `ListTransactionsUseCase` só traduz o `ListTransactionsInput` na
  consulta (tira o `Trim()` da busca, converte o `dateRef` em intervalo e o `type`, o `orderBy` e o `asc` nos tipos do
  `Domain`) e devolve os `TransactionOutput`.
- **Por quê**: B1. O `Domain` não vê o `Application` (`TransactionType`, `TransactionOrderField`), então a consulta usa
  tipos próprios do `Domain`. O `GetAllAsync()` fica, porque o `ListCategoriesUseCase` ainda o usa (fora de escopo).
- **Alternativas**: expor `IQueryable` ao `Application` (vaza EF para a regra de negócio); passar parâmetros soltos (seis
  parâmetros, sem nome para a regra).
- **Efeito**: o `ListTransactionsUseCase` deixa de depender do `ICategoryRepository`; a ordenação por nome da categoria
  passa a ser um join no repositório.

### Busca sem acentos

- **Decisão**: o mesmo filtro da busca de categorias (`CategoryRepository.cs:14-15`):
  `EF.Functions.Unaccent(d).ToLower().Contains(EF.Functions.Unaccent(texto).ToLower())`. A extensão `unaccent` já existe
  (migration `AddUnaccentExtension`). Descrição nula não casa (`NULL` em `LIKE`/`strpos` não é verdadeiro).
- **Por quê**: mesma regra das categorias; o texto vai como parâmetro (princípio II). `%`, `_` e `\` já são tratados como
  texto pela tradução do `Contains` (testados para categorias); o `\` ganha teste novo.
- **Alternativas**: `ILIKE` (não ignora acentos); coluna normalizada (dado duplicado, migration de dados).

### Ordem alfabética pt-BR (FR-018)

- **Decisão**: collation ICU declarada em migration (`modelBuilder.HasCollation("pt_br_icu", locale: "pt-BR",
  provider: "icu", deterministic: true)`) e aplicada na consulta com `EF.Functions.Collate(coluna, "pt_br_icu")`. O nome
  da collation é constante, nunca entrada do usuário. O desempate acento/caixa do FR-018 é o comportamento padrão da
  ICU (diferença de acento pesa mais que a de caixa; minúscula antes de maiúscula); o teste do cenário 5 da história 4
  confirma.
- **Por quê**: fixa a regra no banco, igual em qualquer instalação, em vez de depender da collation padrão do cluster.
  Declarada na migration, falha alto (a migration não aplica) se o PostgreSQL não tiver ICU, em vez de ordenar errado.
  Não muda colunas nem dados.
- **Alternativas**: usar a collation embutida `pt-BR-x-icu` (existe só se o initdb a importou, o que depende do
  ambiente); ordenar em memória (volta ao problema de antes); collation padrão do banco (rejeitada na clarificação).

### Transações sem descrição

- **Decisão**: o PostgreSQL põe `NULL` por último na ordem crescente e primeiro na decrescente, o contrário do que o
  FR-018 exige (início no crescente, fim no decrescente). A consulta ordena antes por `descrição != null`
  (crescente: sem descrição primeiro; decrescente: sem descrição por último) e só depois pela descrição com collation.
- **Por quê**: expressão portável e explícita, sem depender do padrão do banco. Teste dedicado nas duas direções.

### Mês

- **Decisão**: intervalo `[início do mês, início do mês seguinte)` em UTC, como o `DatedBetween` dos relatórios
  (`TransactionRepository.cs`), a partir do ano e do mês do `dateRef`. A consulta recebe `DateTime? From` e `DateTime? To`,
  **ambos com `DateTimeKind.Utc`**: o Npgsql recusa `DateTime` não-UTC como parâmetro de `timestamptz`, e o `new
  DateTime(y, m, 1)` de antes (`ListTransactionsUseCase.cs:21`) é `Unspecified`, inofensivo só porque a comparação era
  em memória.
- **Por quê**: remove o `AddTicks(-1)` e o arredondamento para microssegundos do PostgreSQL; usa o índice em `Date`
  (`TransactionConfiguration.cs`, snapshot).
- **Efeito observável**: só muda o resultado de uma transação com fração de segundo no último instante do mês.

### Tipo, categoria e desempates

- **Decisão**: `Type.In` → `Value > 0`; `Type.Out` → `Value < 0`; categoria → `CategoryId ==`. Ordenação por
  `Date`, `Description`, `Value` ou nome da categoria (join com `Categories`), seguida de `Date` e `CreatedAt` com a
  direção do FR-018 (decrescentes, exceto em `date` crescente).
- **Por quê**: iguais ao `ListTransactionsUseCase` de antes; só mudam de lugar.

### Índices (FR-019)

- **Decisão**: nenhuma migration de índice. `Date` e `CategoryId` já são indexados (snapshot do model:
  `HasIndex("CategoryId")`, `HasIndex("Date")`), que são os filtros seletivos do FR-019. Busca e tipo avaliam só o
  conjunto que sobrou dos outros filtros.
- **Por quê**: princípio III, otimizar com medição. Um índice de busca parcial (trigram em `unaccent(lower(...))`)
  exigiria uma função `immutable` própria, e o volume é de milhares de linhas por ano. Se a medição do SC-008 ou da
  busca isolada pedir, vira outra decisão, com o usuário.

### Medição do SC-008

- **Decisão**: a linha de base é medida **antes** de qualquer mudança de código (primeira tarefa do implement), com a
  mesma carga depois. 10.000 transações (200 no mês e 200 na categoria pedidos) geradas por script SQL numa base
  descartável; 3 requisições de aquecimento descartadas e 20 medidas por caso; mediana das 20. Um `EXPLAIN (ANALYZE)` do
  mês e da categoria registra se o plano usa `IX_Transactions_Date` e `IX_Transactions_CategoryId`. Resultado registrado
  na tabela abaixo.
- **Por quê**: o requisito é relativo a "antes", que só existe antes da mudança.
- **Alternativas**: teste automatizado de tempo (instável; fica fora da suíte).

### O que o implement precisa provar (nada abaixo foi executado: o Docker estava desligado)

Cada item é um teste de `Denarius.Infrastructure.Tests` ou uma conferência do SQL gerado; o SQL de ao menos um caso é
capturado para provar que filtro e ordenação foram para o banco.

- O `CREATE COLLATION` gerado por `HasCollation(..., provider: "icu")` é aceito pelo `postgres:18.0`
  (`SELECT collname, collprovider, colllocale FROM pg_collation WHERE collname = 'pt_br_icu'`).
- A sequência "agua, Agua, água, Água, Banana" e a inversa. Um caso extra com espaço, hífen e dígito
  ("Água de coco", "Aguardente", "Água-viva", "Agua 2") documenta a regra efetiva da ICU, que o FR-018 só exemplifica
  com letras.
- `ORDER BY d IS NOT NULL, d COLLATE "pt_br_icu"` nas duas direções, com os nulos no início do crescente e no fim do
  decrescente.
- O join com `Categories` só quando `SortBy == CategoryName`, mantendo `ORDER BY c.Name COLLATE "pt_br_icu"` depois do
  `Select` da transação.
- O `Contains` com `\`, `%` e `_` vai como parâmetro (`strpos` ou equivalente), conferido no SQL.
- `lower(unaccent(...))` com "Açaí", "AÇAÍ" e "ç", na locale padrão do container.
- A lista por mês com `dateRef` `Unspecified`, `Utc` e `Local` não dá `500`; no caso de uso, o teste afirma
  `Kind == Utc` em `From` e `To`.
- Nada registra valores de parâmetros: não há `EnableSensitiveDataLogging` em `src/` (conferido por busca), e o log do EF
  em `Information` mostra o SQL, não os valores. Essa configuração deve continuar assim (FR-021).

### Contrato e dependências

- **Contrato**: `contracts/transactions-api.yaml` passou da 1.2.0 para a 1.3.0, com a descrição da busca, da ordem e do
  mês atualizada. Aditiva: nenhum parâmetro, resposta ou formato muda (B2). A ordem de strings muda para quem dependia
  da cultura do servidor; é o efeito pedido no FR-018.
- **Dependências**: nenhuma nova. `Npgsql.EntityFrameworkCore.PostgreSQL` 10.0.3 já oferece `Unaccent`, `Collate` e
  `HasCollation`.

### Medição de base e de depois

| Caso | Mediana antes (ms) | Mediana depois (ms) | Razão |
| --- | --- | --- | --- |
| Mês (200 de 10.000) | 31,5 | 4,4 | 7,2 |
| Categoria (200 de 10.000) | 32,3 | 4,3 | 7,5 |

Os valores acima são do estado aquecido (mediana das rodadas seguintes à partida da API, ver abaixo). Nas duas primeiras
rodadas depois da partida, o código novo ainda estava aquecendo (JIT) e deu 7,4 e 6,4 ms (mês) e 7,2 e 5,3 ms
(categoria), razões de 4,4 a 6,0 contra os 32 ms do código antigo; a partir da terceira rodada estabilizou em 3,9 a 4,4
ms. Na primeira medição de base (T036), feita com as duas primeiras rodadas do código antigo, deu 32,3 e 32,0 ms (mês) e
34,9 e 31,8 ms (categoria).

**Linha de base (2026-10-11, T036)**: PostgreSQL 18.0 em contêiner descartável (porta 55432), migrations atuais, API em
`Release` (`dotnet run -c Release`) com `ConnectionStrings__DenariusDb` apontando para ele, 10.000 transações (200 em
março de 2026, 200 na categoria "Alvo", sem sobreposição), `curl` sequencial no mesmo computador. Método: 3 requisições
de aquecimento descartadas e 20 medidas por caso; mediana das 20. Duas rodadas, mostradas acima. Referências: a lista
sem filtro (10.000 linhas) levou 51,8 ms, uma lista vazia (código atual, lê tudo) 36,8 ms e o piso de HTTP e conexão
(`GET` de um id inexistente, 404) 2,0 ms. O custo de hoje está em carregar e materializar as 10.000 transações no .NET, e
não no banco: o `Seq Scan` das 10.000 linhas leva 1,0 ms, e as consultas com índice que a lista nova vai gerar levam 0,12
ms (mês, `Bitmap Index Scan` em `IX_Transactions_Date`) e 0,04 ms (categoria, `IX_Transactions_CategoryId`). O script de
carga e o de medição ficaram no diretório temporário da sessão, fora do repositório.

**Medição de depois (2026-10-11, T048)**: mesma base, mesmos dados e mesmo método, com a migration `AddPortugueseCollation`
aplicada e a API nova em `Release`. Para o "antes" aquecido, o código antigo (o commit `9bd7355`, exportado para fora do
repositório) foi servido na porta 5291 contra a mesma base e medido em 6 rodadas: a primeira (145,1 ms no mês e 40,8 ms na
categoria, com a API ainda partindo) foi descartada, e as rodadas 2 a 6 deram 32,8, 35,8, 30,0, 31,5 e 30,4 ms (mês) e
32,9, 32,3, 31,4, 32,7 e 30,2 ms (categoria); a mediana de cada série é a da tabela. O código novo, rodadas 3 a 6:
4,4, 4,4, 4,4 e 3,9 ms (mês) e 4,2, 4,4, 4,4 e 4,3 ms (categoria). O piso de HTTP continuou em 1,9 ms. Referências do
código novo: a lista sem filtro levou 28,7 ms, a busca por "farmacia" 34,6 ms, a ordenação por descrição sem filtro 36,2
ms e a por nome da categoria 33,2 ms; sem o filtro de mês ou de categoria, essas listas ainda leem as 10.000 linhas
(FR-019), então o custo delas continua dominado pela materialização no .NET. O plano de execução do SQL real (com o
`ORDER BY "Date" DESC, "CreatedAt" DESC`) usa `Bitmap Index Scan` em `IX_Transactions_Date` (mês, 0,19 ms) e em
`IX_Transactions_CategoryId` (categoria, 0,14 ms).

## Onde fica a validação

- **Decisão**: A validação por campo (trim/tamanho de `Description`, `Date` diferente do padrão,
  `Value` diferente de zero, `CategoryId` não vazio) fica no construtor/método `Update` de
  `Transaction` (camada Domain), que lança `DomainException` para entradas inválidas — o mesmo
  padrão de `Category`/`Color`. A validação entre entidades (a `Category` referenciada precisa
  existir) fica uma camada acima, em `CreateTransactionUseCase`/`UpdateTransactionUseCase` (camada
  Application), porque a própria `Transaction` não depende de `ICategoryRepository`, nem pode
  depender (Princípio II da constituição — `Domain` não pode depender de comportamento de
  repositório além da sua própria interface).
- **Justificativa**: Torna impossível representar instâncias de `Transaction` com campos
  inválidos, e mantém a verificação de existência entre entidades onde uma chamada de repositório
  é de fato possível.
- **Alternativas consideradas**: Verificar a existência da categoria dentro da própria
  `Transaction` — rejeitado porque exigiria que `Domain` dependesse de `ICategoryRepository`,
  violando o Princípio II.

## Integridade referencial com Category

- **Decisão**: Garantida duas vezes, no mesmo formato da pesquisa da proteção de exclusão de
  Category: uma verificação explícita de existência com `ICategoryRepository.GetByIdAsync` em
  `CreateTransactionUseCase` e `UpdateTransactionUseCase` (lança `NotFoundException`, mapeada para
  `404`), apoiada por uma chave estrangeira no banco (`Transactions.CategoryId → Categories.Id`,
  `ON DELETE RESTRICT`) como rede de segurança.
- **Justificativa**: A verificação na aplicação produz um `404` limpo, voltado ao usuário, quando
  quem chama referencia uma categoria que não existe; a restrição do banco garante a integridade
  referencial mesmo que um caminho de código futuro contorne o caso de uso. Note que a direção
  aqui é o espelho da proteção de Category: Category bloqueia a *exclusão* de uma linha
  referenciada por Transactions; Transaction bloqueia a *criação/atualização* de uma linha que
  referencia uma Category inexistente.
- **Alternativas consideradas**: Confiar só na restrição do banco — rejeitada pelo mesmo motivo de
  Category: a exceção crua de violação de chave estrangeira apareceria como um `500` pouco útil, em
  vez de um `404` limpo e com explicação.

## Mapeamento de erros para status HTTP

- **Decisão**: O mesmo `GlobalExceptionHandler` (`IExceptionHandler`) único, usado por todos os
  controllers, mapeia `NotFoundException → 404`, `DomainException`/`AppException → 400` e qualquer
  outra → `500`, todas renderizadas como `ProblemDetails` da RFC 7807. Não existe, nem é
  necessário, tratamento de exceções específico de Transaction.
- **Justificativa**: Um único mapeamento compartilhado em toda a API pública (Princípio I da
  constituição).
- **Alternativas consideradas**: try/catch em cada controller — rejeitado; desnecessário, já que o
  handler compartilhado atende a esta feature.

## Lacuna de cobertura de testes (apontada e depois fechada)

- **Observação** (na execução original do `/speckit-plan`): `Denarius.Domain.Tests` (13 testes em
  `TransactionTests.cs`) e `Denarius.Application.Tests` (13 testes nos cinco arquivos de teste de
  `UseCases/Transactions/*`) cobriam por completo a entidade e os cinco casos de uso.
  `Denarius.WebAPI.Tests` não tinha nenhum teste que exercitasse o próprio `TransactionsController`
  — o roteamento, o model binding de requisição/resposta e o `201`/cabeçalho `Location` no
  `Create` não eram testados acima da camada de casos de uso.
- **Impacto**: Baixo. O controller não tem lógica de decisão própria; só delega. Mas uma regressão
  de roteamento ou de binding (por exemplo, um campo renomeado) não seria pega por nenhum teste que
  existia na época.
- **Resolução**: Fechada pelo `/speckit-implement` em 2026-09-22 — foi adicionado
  `tests/Denarius.WebAPI.Tests/Transactions/TransactionsControllerTests.cs` (11 testes), no mesmo
  padrão de `HostBuilder`/`TestServer` montado à mão já usado para
  `Cors`/`Middleware`/`Categories` em `Denarius.WebAPI.Tests` (registra o `TransactionsController`
  via `AddApplicationPart`, sem precisar de banco), com fakes escritos à mão para as cinco
  interfaces de casos de uso. É exatamente como
  [001-category-management](../001-category-management/research.md) fechou a sua lacuna
  equivalente.

**Resultado**: Todos os itens do Contexto técnico acima estão resolvidos; não resta nenhum
`NEEDS CLARIFICATION`.
