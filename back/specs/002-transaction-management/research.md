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
