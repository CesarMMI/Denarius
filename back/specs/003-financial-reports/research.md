# Pesquisa da Fase 0: Relatórios financeiros

Cada item resolve uma decisão que o pedido deixou em aberto, ou um conflito entre a redação do pedido
e uma convenção que este código já segue. Onde o pedido dizia "definir e documentar", a decisão está
registrada aqui e em [spec.md](./spec.md) → Premissas.

## Como entradas e saídas são modeladas

- **Decisão**: Seguir exatamente o modelo existente: o sinal de `Transaction.Value` (`decimal`,
  `numeric(18,2)`) é o tipo — positivo é entrada (receita), negativo é saída (despesa), zero é
  impossível (a entidade o rejeita). As categorias não têm tipo. Os relatórios somam as despesas
  como `-SUM(value)` sobre os valores negativos, então toda despesa que a API devolve é um valor
  positivo e `balance = income - expense`. Todo valor monetário é `decimal` de ponta a ponta.
- **Justificativa**: O pedido manda descobrir e seguir o modelo; não há tipo armazenado para ler, e
  o `TransactionType` (`All`/`In`/`Out`) só existe para filtrar a lista de transações pelo sinal.
- **Alternativas consideradas**: Devolver as despesas com sinal (como faz o
  `TransactionOutput.value`) — rejeitado nos relatórios, em que "despesa total" e as barras dos
  gráficos são lidas naturalmente como valores positivos e o pedido define o saldo como receita −
  despesa.

## Datas e o fuso horário America/Sao_Paulo

- **Decisão**: As datas das transações são dias do calendário. O front envia cada dia como a sua
  meia-noite UTC (`front/src/app/shared/date-utils/date-utils.ts`) e a API filtra os meses em UTC,
  então os relatórios agrupam pelas partes da data em UTC de `Transactions.Date` e nunca as
  convertem. O fuso horário decide só *qual dia é hoje*: `TimeProvider.GetUtcNow()` convertido para
  `America/Sao_Paulo` (`TimeZoneInfo.FindSystemTimeZoneById`, id IANA — resolvido pelo ICU no
  Windows e pelo tzdata no Linux). O "hoje" define o mês atual quando `month` é omitido, os dias
  decorridos e a "despesa até hoje" da projeção, e onde termina a série acumulada do mês atual.
- **Justificativa**: Converter as datas armazenadas jogaria todas as transações para o dia anterior
  (00:00 UTC são 21:00 do dia anterior em São Paulo), pondo uma despesa de 1º de setembro em agosto.
  O único instante com que os relatórios lidam é o "agora", e é aí que São Paulo importa: às 22:00
  de 30 de setembro em São Paulo, o relógio UTC do servidor já marca 1º de outubro.
- **Alternativas consideradas**: `DateTime.Now`/`TimeZoneInfo.Local` — depende do fuso do servidor e
  não pode ser testado; uma interface `IClock` própria — o `TimeProvider` da BCL já é essa
  abstração, não precisa de pacote, e o NSubstitute consegue fazer stub do seu `GetUtcNow()`
  virtual.

## O parâmetro `month`

- **Decisão**: Um value type `YearMonth` em `Application/IO/Reports` com um
  `TryParse(string?, IFormatProvider?, out YearMonth)` estático que aceita exatamente `yyyy-MM`
  (`DateOnly.TryParseExact`, cultura invariante). Os controllers recebem
  `[FromQuery] YearMonth? month`; o MVC faz o binding por meio do `TryParse`, e o `[ApiController]`
  transforma uma falha de parse num `400` `ValidationProblem` (`errors.month`) antes de a action
  rodar. Um `month` ausente ou vazio vira `null`, e o caso de uso usa o mês atual em São Paulo. Os
  inputs carregam `YearMonth?`; os outputs escrevem os meses como strings `yyyy-MM`.
- **Justificativa**: É como esta API já valida os parâmetros de query — um `type`/`orderBy`
  desconhecido ou um `categoryId` malformado falham no model binding do mesmo jeito —, então cada
  controller continua sendo uma linha de delegação, e todos os relatórios rejeitam um mês inválido
  da mesma forma. Verificado com um host de teste: `2026-09` faz o binding; `2026-9`, `2026-13`,
  `2026-00`, `09-2026`, `2026-09-01`, `abc` e ` 2026-09` dão `400`; `?month=` vira `null`.
- **Alternativas consideradas**: Um `string? month` interpretado em cada action — cinco cópias do
  mesmo parsing, ou um helper, em controllers "finos"; `DateTime?`/`DateOnly?` — os seus binders
  aceitam muitos formatos (`2026-09-01`, `09/2026`), então o `YYYY-MM` não seria garantido; um value
  object de Domain — o mês é uma entrada de relatório, e não algo que uma entidade armazena, e a
  fronteira de IO mantém os tipos de domínio fora dos Inputs (veja a skill `add-value-object`).

## A quantidade de meses da série de receitas vs. despesas

- **Decisão**: `[FromQuery, Range(1, 24)] int months = 12` na action. Fora do intervalo ou não
  numérico → `400` `ValidationProblem` (`errors.months`) do `[ApiController]`, sem chamar o caso de
  uso.
- **Justificativa**: O mesmo mecanismo e o mesmo formato de resposta do mês inválido; o pedido põe a
  validação de entrada no controller.
- **Alternativas consideradas**: Validar no caso de uso e lançar `AppException` — um formato de
  `400` diferente (`ProblemDetails` com `detail`) para o mesmo tipo de erro de um mês inválido.

## Rotas

- **Decisão**: `ReportsController` sob a rota `api/[controller]` existente, com segmentos em
  camelCase: `GET /api/reports/summary`, `/api/reports/expensesByCategory`,
  `/api/reports/incomeVsExpense`, `/api/reports/cumulativeExpenses`, `/api/reports/transactions`.
  Escritas como templates literais (`[HttpGet("expensesByCategory")]`), e não `[action]`, para que
  renomear um método não possa mudar uma rota pública.
- **Justificativa**: O pedido lista `/reports/summary`, `/reports/expenses-by-category`, ..., mas
  também pede para seguir as convenções existentes e não introduzir novos estilos de nomes. Todas as
  rotas ficam sob `/api`, e a API expõe rotas e valores de enum em camelCase de propósito
  (`CamelCaseRouteTransformer`, commit `4eb9aad`, e o front chamando `orderBy=categoryName`).
  Kebab-case seria a primeira rota num estilo diferente.
- **Alternativas consideradas**: Os caminhos literais em kebab-case — rejeitados pelo motivo acima;
  é uma mudança de três strings no controller (e de uma no service do front) se o dono preferir.

## Agregação no banco

- **Decisão**: Quatro novas consultas no `ITransactionRepository`, implementadas no
  `TransactionRepository` com LINQ do EF Core que o PostgreSQL executa como
  `WHERE "Date" >= @from AND "Date" < @to ... GROUP BY ... SUM(...)`:
  - `SumByMonthAsync(from, to)` → `(Year, Month, Income, Expense)` por mês com transações
    (`GROUP BY` ano e mês; receita e despesa como somas condicionais na mesma passada).
  - `SumExpensesByCategoryAsync(from, to)` → `(Category, Expense)` por categoria com despesas (uma
    subconsulta agrupada com join em `Categories` para o nome e a cor).
  - `SumExpensesByDayAsync(from, to)` → `(Date, Expense)` por dia com despesas.
  - `GetWithCategoryNameAsync(from, to)` → `(Transaction, CategoryName)` para cada transação do
    intervalo (com join em `Categories`; não é um agregado, mas fica limitado ao mês).
  Os intervalos são dias do calendário `DateOnly`, `[from, to)`; o repositório os converte em
  meias-noites UTC, a representação que ele armazena. Os resultados são tuplas nomeadas. As
  consultas somente leitura usam `AsNoTracking()`.
- **Justificativa**: O pedido exige `GROUP BY`/`SUM` no banco em vez de carregar tudo (os casos de
  uso de listagem existentes carregam todas as transações e filtram em memória, o que o pedido
  descarta aqui). Manter as somas no repositório e as regras nos casos de uso deixa os dois
  testáveis: as regras em `Denarius.Application.Tests`, com um repositório substituído, e o SQL na
  execução do quickstart contra o PostgreSQL. Tuplas nomeadas já aparecem nas assinaturas deste
  código (o `(Guid Id, UpdateTransactionInput Input)` do `IUpdateTransactionUseCase`) e evitam, em
  `Domain`, tipos de leitura que só essas consultas usariam; o `DateOnly` mantém o "um dia é
  armazenado como a sua meia-noite UTC" dentro de Infrastructure.
- **Alternativas consideradas**: Uma interface de "repositório de relatórios" em `Application` — a
  constituição permite, mas todas as interfaces de repositório daqui ficam em
  `Domain/Repositories`; SQL cru — o EF Core traduz todas as consultas necessárias, mantendo-as
  tipadas e neutras quanto ao provedor; carregar as transações do mês e somar no caso de uso —
  descartado explicitamente pelo pedido.

## Índices

- **Decisão**: Adicionar `IX_Transactions_Date` (B-tree em `Date`) por meio de
  `TransactionConfiguration.HasIndex(t => t.Date)` e de uma migration, `AddTransactionDateIndex`,
  cujo `Down()` o remove. `IX_Transactions_CategoryId` já existe (migration `AddTransaction`, para a
  chave estrangeira), então nada é adicionado para a categoria.
- **Justificativa**: Todos os relatórios filtram primeiro por um intervalo de datas; o agrupamento
  por categoria e o join com `Categories` então trabalham só com as linhas do mês.
- **Alternativas consideradas**: Um índice composto `(CategoryId, Date)` ou um índice de cobertura
  com `INCLUDE ("Value", "CategoryId")` — mensuráveis só em volumes muito além dos deste app
  single-tenant, e um índice mais largo a cada escrita.

## Percentuais, arredondamento e divisão por zero

- **Decisão**: Todo percentual está na escala 0–100, arredondado para duas casas decimais com
  `MidpointRounding.AwayFromZero`: `savingsRate = balance / totalIncome × 100`, o
  `percentage = amount / total × 100` da categoria e cada variação em relação ao mês anterior
  `(current − previous) / |previous| × 100`. O dinheiro projetado é arredondado para centavos do
  mesmo jeito. `savingsRate` é `null` quando o mês não tem receita; uma variação é `null` quando o
  valor anterior é zero. A taxa de poupança pode ser negativa (despesa acima da receita).
- **Justificativa**: Uma única escala para todos os campos que guardam um percentual, para que
  `percentage: 25.5` e `savingsRate: 25.5` signifiquem a mesma coisa; `null` diz "não se aplica"
  onde 0% seria uma afirmação falsa (sem receita, nada poderia ser poupado, e uma variação a partir
  de zero não tem base). Dividir pelo tamanho do valor anterior mantém o sinal com significado: um
  saldo de −100,00 para 50,00 é +150%, e não −150%.
- **Alternativas consideradas**: Razões (0–1) — corresponde literalmente ao `balance / totalIncome`
  do pedido, mas faz um campo chamado `percentage` guardar 0.255; `0` sem receita — indistinguível
  de "ganhou e não poupou nada".

## Projeção

- **Decisão**: Comparar o mês do relatório com o mês atual em São Paulo. Mês passado: despesa
  projetada = despesa total, saldo projetado = saldo. Mês futuro: os dois zerados. Mês atual:
  `round(expenseToDate / today.Day × daysInMonth, 2)`, em que `expenseToDate` soma as despesas com
  data do dia 1º até hoje (`SumByMonthAsync(firstDay, today + 1)`), e saldo projetado = receita
  total do mês − despesa projetada.
- **Justificativa**: A regra do pedido, lida ao pé da letra: "despesa até hoje" exclui as despesas
  já registradas para dias posteriores do mês, e hoje conta como dia decorrido, então o divisor
  nunca é zero. Só a despesa é projetada; a receita costuma entrar de uma vez e é considerada
  conhecida.
- **Alternativas consideradas**: Usar a despesa total do mês como "despesa até hoje" — exagera o
  ritmo sempre que uma conta futura é registrada antes.

## "Outras" nas despesas por categoria

- **Decisão**: Ordenar pelo valor em ordem decrescente e depois pelo nome da categoria; com mais de
  oito categorias, manter as sete primeiras e somar o resto numa entrada final
  `{ categoryId: null, categoryName: "Outras", color: null }`, para que a lista nunca passe de oito
  entradas. "Outras" vem sempre por último. Os percentuais são calculados para todas as entradas em
  relação à despesa total do mês.
- **Justificativa**: Oito fatias é o que um gráfico de rosca ainda mostra de forma legível; o limite
  do pedido é "mais de 8 categorias". A cauda fica por último porque é a cauda, mesmo que a sua soma
  seja maior que a de algumas categorias mostradas individualmente.
- **Alternativas consideradas**: Oito categorias mais "Outras" (nove entradas) — passa do limite que
  o pedido usa; ordenar "Outras" entre as categorias pela sua soma — dá a impressão de que "Outras"
  é uma categoria.

## Série contínua

- **Decisão**: Receitas vs. despesas monta a lista de meses no caso de uso — de
  `month − (months − 1)` até `month` — e preenche cada um com as somas agrupadas, com zeros onde o
  banco não devolveu linha. A comparação acumulada monta uma entrada por dia, de 1 até o último dia
  de cada série (hoje no mês atual, nenhum num mês futuro, o último dia do mês nos demais casos),
  levando o total acumulado pelos dias sem despesa.
- **Justificativa**: O `GROUP BY` só devolve os meses e dias que têm linhas; as lacunas são uma
  regra do relatório ("série contínua, sem buracos"), então são preenchidas onde as regras vivem e
  têm testes unitários.
- **Alternativas consideradas**: `generate_series` em SQL — leva a regra para um SQL sem testes.

## O tipo da transação nas transações do mês

- **Decisão**: O novo campo `type` reaproveita o `TransactionType`, definido pelo sinal (`In` para
  valor positivo, `Out` para negativo), e `amount` é o valor absoluto. O `TransactionType` ganha
  `[JsonConverter(typeof(JsonStringEnumConverter<TransactionType>))]` e
  `[JsonStringEnumMemberName]` nos seus membros, para ser serializado como `"in"`/`"out"` — os
  mesmos valores que o filtro de query `type` do `GET /api/transactions` aceita.
- **Justificativa**: O vocabulário do próprio domínio para o tipo é `In`/`Out`; um atributo no tipo
  funciona igual no host da API, no host de testes e no documento OpenAPI gerado, sem configuração
  global do serializador. O System.Text.Json vem com o framework, então a `Application` não ganha
  nenhuma dependência nova. O binding da query usa o type converter do enum, que ignora esses
  atributos.
- **Alternativas consideradas**: Um `JsonStringEnumConverter` global no `Program.cs` — os hosts de
  teste e o gerador de OpenAPI leem opções diferentes e cada um precisaria dele; uma propriedade
  `string` — sem tipo.

## Formatos dos outputs

- **Decisão**: Um record de Input e um de Output por relatório em `IO/Reports`, escritos como os
  records existentes (propriedades `{ get; init; }` explícitas e um construtor), com o nome do caso
  de uso (`GetMonthlySummaryInput`, `MonthlySummaryOutput`, ...). Records de item para as listas
  (`CategoryExpenseOutput`, `IncomeVsExpenseOutput`, `AccumulatedExpenseOutput`,
  `MonthlyTransactionOutput`) e `PreviousMonthSummaryOutput` para a comparação do resumo. Os
  formatos são exatamente os do pedido; `previousMonth`, no relatório acumulado, é a série do mês
  anterior, como pedido.
- **Justificativa**: O pedido exige Inputs e Outputs dedicados e nenhuma entidade como resposta.
- **Alternativas consideradas**: Records posicionais — mais concisos, mas fora do estilo dos records
  de IO existentes.

## Documentação

- **Decisão**: [contracts/reports-api.yaml](./contracts/reports-api.yaml) documenta os cinco
  endpoints no mesmo formato OpenAPI 3.0.3 dos contratos das outras features. O documento OpenAPI de
  runtime (`AddOpenApi`/`MapOpenApi`, servido em `/openapi/v1.json` em Development) lista o novo
  controller sem código extra.
- **Justificativa**: São os dois mecanismos de documentação que o projeto já tem; não há README do
  backend.
- **Alternativas consideradas**: Adicionar Swagger UI ou comentários XML — um mecanismo novo que o
  projeto não usa.

## Abordagem de testes

- **Decisão**: Os testes dos casos de uso substituem o `ITransactionRepository` (verificando os
  intervalos `[from, to)` exatos pedidos, inclusive nas viradas de ano) e o `TimeProvider`
  (`GetUtcNow()` com stub, por exemplo `2026-10-01T02:00Z` para provar o mês em São Paulo); o
  `YearMonth` ganha testes próprios; os testes de controller seguem o `TransactionsControllerTests`
  (fakes escritos à mão, `TestServer`, `AddApplicationPart`). As consultas do repositório são
  exercitadas de ponta a ponta pelo quickstart, num banco PostgreSQL descartável populado com
  linhas conhecidas.
- **Justificativa**: Segue os projetos de teste existentes e o Princípio IV da constituição; não há
  projeto de testes de Infrastructure, e criar um (com banco) está fora do escopo.
- **Alternativas consideradas**: O `FakeTimeProvider` de `Microsoft.Extensions.TimeProvider.Testing`
  — um pacote novo para o que um único método virtual com stub já faz; um projeto de testes de
  Infrastructure com Testcontainers — uma dependência e um padrão novos.

**Resultado**: Todos os itens do Contexto técnico estão resolvidos; não resta nenhum
`NEEDS CLARIFICATION`.
