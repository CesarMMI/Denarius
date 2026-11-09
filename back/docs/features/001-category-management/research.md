# Pesquisa da Fase 0: Gestão de categorias

Este é um plano retroativo, então "pesquisa" aqui significa confirmar as decisões já embutidas no
código e nos testes entregues, e não explorar opções para código ainda não escrito. Cada item
abaixo resolve o que, de outro modo, seria um `NEEDS CLARIFICATION` no Contexto técnico.

## Tecnologia de persistência

- **Decisão**: PostgreSQL via migrations code-first do EF Core, com o `DenariusDbContext`
  compartilhado por todo o projeto.
- **Justificativa**: Já é o padrão de todo o backend (Restrições de arquitetura da constituição).
  `Category` não tem nenhum requisito — flexibilidade de esquema, consultas em grafo etc. — que
  justifique outro banco.
- **Alternativas consideradas**: Nenhuma específica de `Category`; isso foi herdado da stack do
  projeto, e não decidido por feature.

## Estratégia de agregação do uso (quantidade de transações e saldo por categoria)

- **Decisão**: Calculado na camada Application (`ListCategoriesUseCase`), carregando todas as
  categorias e todas as transações pelos seus repositórios e agregando em memória com LINQ
  (`GroupBy(t => t.CategoryId)`), em vez de levar um `GROUP BY`/join para o repositório ou para o
  banco.
- **Justificativa**: Mantém a lógica de montagem de SQL fora de `Domain`/`Application` (Princípio
  II da constituição) e deixa o caso de uso fácil de testar com coleções simples em memória e
  dublês do NSubstitute, como se vê em `ListCategoriesUseCaseTests`.
- **Alternativas consideradas**: Uma agregação no SQL (um método de repositório que devolve
  contagens/somas já agrupadas) escalaria melhor, mas ampliaria o contrato do repositório e
  levaria lógica para `Infrastructure`. Rejeitada implicitamente em favor da simplicidade na
  escala de dados atual do projeto (veja Escala/escopo no plan.md).
- **Trade-off apontado**: `ITransactionRepository.GetAllAsync()` busca a tabela `Transactions`
  inteira a cada requisição da lista de categorias, sem levar o filtro de período para o banco nem
  quando o `dateRef` restringe o resultado a um mês. Serve hoje; vale revisitar se o volume de
  transações crescer muito.

## Onde fica a validação

- **Decisão**: A validação fica no construtor/método `Update` de `Category` e no construtor do
  value object `Color` (camada Domain), que lançam `DomainException` para entradas inválidas.
- **Justificativa**: Torna impossível representar instâncias inválidas de `Category`/`Color`, como
  em todas as outras entidades/value objects de `Denarius.Domain`. Nenhuma biblioteca externa de
  validação (por exemplo, FluentValidation) é usada no código.
- **Alternativas consideradas**: Atributos de data annotation nos DTOs de entrada
  (`CreateCategoryInput`/`UpdateCategoryInput`) na fronteira da WebAPI — rodariam antes mesmo de
  existir um `Category`/`Color`, mas foram rejeitados implicitamente em favor de uma única fonte de
  verdade da validação, em `Domain`.

## Proteção contra a exclusão de categorias em uso

- **Decisão**: Garantida duas vezes — uma verificação explícita com
  `ITransactionRepository.ExistsByCategoryIdAsync` no `DeleteCategoryUseCase` (lança
  `DomainException`, mapeada para `400`), apoiada por uma chave estrangeira no banco
  (`Transactions.CategoryId → Categories.Id`, `ON DELETE RESTRICT`) como rede de segurança.
- **Justificativa**: A verificação na aplicação produz a explicação para o usuário que a spec exige
  (FR-009); a restrição do banco garante a integridade referencial mesmo que um caminho de código
  futuro contorne o caso de uso.
- **Alternativas consideradas**: Confiar só na restrição do banco — rejeitada porque a exceção crua
  de violação de chave estrangeira apareceria como um `500` pouco útil, em vez de um `400` limpo e
  com explicação.

## Mapeamento de erros para status HTTP

- **Decisão**: Um único `GlobalExceptionHandler` (`IExceptionHandler`) mapeia
  `NotFoundException → 404`, `DomainException`/`AppException → 400` e qualquer outra → `500`, todas
  renderizadas como `ProblemDetails` da RFC 7807.
- **Justificativa**: Um mapeamento compartilhado por todos os controllers, inclusive o
  `CategoriesController`, mantém o formato dos erros consistente em toda a API pública (Princípio I
  da constituição).
- **Alternativas consideradas**: try/catch em cada controller — rejeitado; o handler compartilhado
  existente já atende a esta feature sem mudanças.

## Lacuna de cobertura de testes (apontada e depois fechada)

- **Observação** (na execução original do `/speckit-plan`): `Denarius.Domain.Tests` e
  `Denarius.Application.Tests` cobrem por completo a entidade e os cinco casos de uso;
  `Denarius.WebAPI.Tests` não tinha nenhum teste que exercitasse o próprio `CategoriesController`
  — o roteamento, o binding dos parâmetros de query do `List` e o cabeçalho `Location` do `201` no
  `Create` não eram testados acima da camada de casos de uso.
- **Impacto**: Baixo. O controller não tem lógica de decisão própria; só delega. Mas uma regressão
  de roteamento ou de binding (por exemplo, um parâmetro de query renomeado) não seria pega por
  nenhum teste que existia na época.
- **Resolução**: Fechada pelo `/speckit-implement` em 2026-09-21 — foi adicionado
  `tests/Denarius.WebAPI.Tests/Categories/CategoriesControllerTests.cs` (11 testes), no mesmo
  padrão de `HostBuilder`/`TestServer` montado à mão já usado para `Cors`/`Middleware` em
  `Denarius.WebAPI.Tests` (registra o `CategoriesController` via `AddApplicationPart`, sem precisar
  de banco), com fakes escritos à mão para as cinco interfaces de casos de uso.

**Resultado**: Todos os itens do Contexto técnico acima estão resolvidos; não resta nenhum
`NEEDS CLARIFICATION`.
