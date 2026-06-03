# Progress Log

## Estado atual do projeto

O backend está funcionalmente completo: todas as 18 features documentadas estão implementadas, expostas via Minimal API e cobertas por testes nas quatro camadas (Domain, Application, Infrastructure, Api — 361 testes, 0 falhas aferidos em 2026-05-28). O projeto encontra-se em estado pronto para integração com um cliente (front-end ou mobile), sem features pendentes ou em andamento.

---

## Última sessão — 2026-05-28

- ✅ `auth-register-user` — Registra um novo usuário com e-mail, senha e nome
- ✅ `auth-login` — Autentica e retorna token JWT
- ✅ `account-create` — Cria conta financeira
- ✅ `account-get-by-id` — Busca conta por ID
- ✅ `account-list` — Lista contas do usuário (incluindo inativas)
- ✅ `account-update` — Atualiza nome e cor da conta
- ✅ `account-deactivate` — Desativa conta (soft delete)
- ✅ `category-create` — Cria categoria Income ou Expense
- ✅ `category-get-by-id` — Busca categoria por ID
- ✅ `category-list` — Lista categorias com filtro opcional por tipo
- ✅ `category-update` — Atualiza nome e cor da categoria
- ✅ `category-delete` — Exclui categoria (transações vinculadas ficam sem categoria)
- ✅ `transaction-create` — Cria transação Income ou Expense e ajusta saldo
- ✅ `transaction-create-transfer` — Cria par de transações Transfer entre contas
- ✅ `transaction-get-by-id` — Busca transação por ID
- ✅ `transaction-list` — Lista transações com filtros por conta, categoria, tipo e período
- ✅ `transaction-update` — Atualiza valor, descrição e categoria
- ✅ `transaction-delete` — Exclui transação e reverte saldo (exclui par em transferências)

Nenhuma feature `in-progress` ou `done` + `verified: false` existe no momento.

---

## Próxima sessão

- **Tarefa prioritária:** Não há features `pending` ou `in-progress` no `feature_list.json`. A próxima tarefa deve ser definida pelo desenvolvedor — candidatos naturais são: relatórios/dashboard (ex: saldo consolidado por período, totais por categoria), exportação de dados, ou integração com cliente front-end/mobile.
- **Contexto necessário:** Ao definir a próxima feature, ler o markdown da entidade/aggregate correspondente em `.claude/entities/` e `.claude/use-cases/` antes de implementar. Se a feature envolver `Transaction`, ler também `.claude/entities/transaction.md` e `.claude/use-cases/use-cases-transaction.md` — é o aggregate mais complexo do domínio.

---

## Observações abertas

- **Mudanças uncommitted acumuladas:** Há três grupos de alterações fora do git: (1) os arquivos de contexto `.claude/*.md` foram reorganizados em `.claude/entities/` e `.claude/use-cases/` mas as deleções e os novos arquivos ainda não foram commitados; (2) `CLAUDE.md` foi movido para a raiz do projeto (estava em `.claude/CLAUDE.md`) — também não commitado; (3) a correção do endpoint `POST /api/transactions/transfer` (era `/transfers`) foi aplicada em `src/Denarius.Api/Endpoints/TransactionEndpoints.cs` e em `tests/Denarius.Api.Tests/Endpoints/TransactionEndpointsTests.cs`, mas igualmente não commitada. Recomenda-se um commit antes de começar a próxima sessão.

- **`openapi.json` modificado:** O arquivo `openapi.json` aparece como modificado no working tree sem ter sido commitado junto com o commit `e4b9031 add OpenAPI spec`. Verificar se a versão atual está sincronizada com a spec gerada em build, ou se a modificação é resíduo de uma execução local.

- **Histórico de reset:** O commit `1b27aba reset project` separa uma iteração anterior do projeto (que usava Controllers e value objects de identificador) da arquitetura atual (Clean Architecture com Minimal APIs). Os commits anteriores ao reset não são relevantes para o estado atual.
