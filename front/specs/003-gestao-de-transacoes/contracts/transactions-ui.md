# Contratos: Gestão de transações

## API consumida

Definida pelo backend, sem mudança por esta feature:

- transações:
  [`back/specs/002-transaction-management/contracts/transactions-api.yaml`](../../../../back/specs/002-transaction-management/contracts/transactions-api.yaml)
  (versão 1.1.0);
- categorias, só a lista:
  [`back/specs/001-category-management/contracts/categories-api.yaml`](../../../../back/specs/001-category-management/contracts/categories-api.yaml)
  (versão 1.0.0).

Toda URL e todo parâmetro ficam nos services (`front/src/app/transactions/services/transactions.service.ts` e
`front/src/app/categories/services/categories.service.ts`); `{apiUrl}` vem de `src/environments/`.

| Método do service                         | Requisição                                                                                                     | Resposta usada         | Quem chama                            |
| ----------------------------------------- | -------------------------------------------------------------------------------------------------------------- | ---------------------- | ------------------------------------- |
| `TransactionsService.list(filters, sort)` | `GET {apiUrl}/transactions[?description=…][&type=in\|out][&categoryId=…][&dateRef=YYYY-MM-01]&orderBy=…&asc=…` | `200` `Transaction[]`  | `httpResource` da página              |
| `TransactionsService.create(input)`       | `POST {apiUrl}/transactions` com `TransactionInput`                                                            | `201` (corpo ignorado) | Página: "Nova transação" e "Desfazer" |
| `TransactionsService.update(id, input)`   | `PUT {apiUrl}/transactions/{id}` com `TransactionInput`                                                        | `200` (corpo ignorado) | Página: "Editar"                      |
| `TransactionsService.delete(id)`          | `DELETE {apiUrl}/transactions/{id}`                                                                            | `204`                  | Página: "Excluir"                     |
| `CategoriesService.list()`                | `GET {apiUrl}/categories` (sem parâmetros: todas, por nome)                                                    | `200` `Category[]`     | `httpResource` da página              |

- Os valores dos filtros e do endereço chegam à API só como parâmetros de consulta codificados, montados com
  `HttpParams` (`transactions.service.ts:18-23`), nunca concatenados ao caminho; o `{id}` de `PUT` e `DELETE` vem só da
  `Transaction` devolvida pela API (a linha da tabela ou a transação em edição).
- Parâmetros vazios não são enviados. A página sempre envia `orderBy` e `asc`, porque sempre tem uma ordenação
  (`date`/`false` ao abrir).
- O texto da busca por descrição vai na query string porque o contrato é do back; o back não registra as URLs
  (`appsettings.json`: `Microsoft.AspNetCore` em `Warning`), e manter isso cabe ao back e à implantação.
- `dateRef` vai como data sem hora (`2026-09-01`); o contrato o declara `format: date-time`, e a API aceita a data sem
  hora. É um desvio conhecido de documentação do back, corrigido pelo fluxo de bugs do back, fora desta feature (ver o
  `plan.md`).
- Depois de `create`, `update`, `delete` e da restauração, a página recarrega a lista com a mesma requisição em vez de
  usar o corpo da resposta, porque os filtros e a ordenação da API decidem se a transação aparece e em que posição
  (`research.md` → Recarga depois de cada mutação).

### Erros

| Resposta                                                                                                                                                                                 | O que o usuário vê                                                                 |
| ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ---------------------------------------------------------------------------------- |
| Falha de `GET /transactions` ou `GET /categories` (qualquer status, inclusive o `400` de um `categoryId` malformado)                                                                     | "Não foi possível carregar as transações." no lugar das linhas                     |
| `POST`/`PUT` recusado com `ProblemDetails.detail` (por exemplo, `404` "Categoria não encontrada.", `400` "O valor da transação não pode ser zero.", `500` "Ocorreu um erro inesperado.") | O `detail`, com "Fechar", por 5 s                                                  |
| `POST`/`PUT` recusado sem `detail` (erro de rede, `400` de model binding)                                                                                                                | "Não foi possível salvar a transação."                                             |
| `DELETE` recusado com `detail` (`404` "Transação não encontrada.")                                                                                                                       | O `detail`, com "Fechar", por 5 s                                                  |
| `DELETE` recusado sem `detail`                                                                                                                                                           | "Não foi possível excluir a transação."                                            |
| "Nova transação" com as categorias em falha (sem requisição)                                                                                                                             | "Não foi possível carregar as categorias. Tente novamente.", com "Fechar", por 5 s |

Nada além do `detail` ou da mensagem padrão é exibido (SC-006). As mensagens do back (`detail`) são sempre em português;
a recusa automática de um pedido malformado (`400` do model binding) vem sem `detail` e cai na mensagem padrão. As
mensagens vão como texto para o `MatSnackBar.open`, e o `SimpleSnackBar` do Material as exibe por interpolação (SC-008).

## UI exposta

### Rotas e endereços (públicos, F2)

| Endereço                                         | Resultado                                                                                                                                                                  |
| ------------------------------------------------ | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `/`                                              | Redireciona para `/transactions` (`app.routes.ts:4-8`)                                                                                                                     |
| `/transactions`                                  | Página de transações, carregada sob demanda (`loadChildren` → `loadComponent`); filtros ocultos, "Mais recentes primeiro"                                                  |
| `/transactions?categoryId=<uuid>`                | Filtros à vista, com a categoria; usado pelo link da quantidade de transações da página de categorias ([002-gestao-de-categorias](../../002-gestao-de-categorias/spec.md)) |
| `/transactions?month=YYYY-MM`                    | Filtros à vista, com o mês; usado pelo "Ver todas" do painel de relatórios ([001-reports-dashboard](../../001-reports-dashboard/contracts/reports-ui.md))                  |
| `/transactions?categoryId=<uuid>&month=YYYY-MM`  | Os dois filtros juntos                                                                                                                                                     |
| `month` fora de `YYYY-MM` ou com mês inexistente | O mês é ignorado; sem outro parâmetro, os filtros ficam ocultos                                                                                                            |
| `categoryId` malformado ou inexistente           | Usado como veio: malformado → erro na tabela; inexistente → lista vazia                                                                                                    |

O item "Transações" (ícone `receipt_long`) é o segundo do menu lateral, entre "Relatórios" e "Categorias"
(`app.ts:23-27`).

### Componentes (inputs → outputs)

Todos, exceto a página, são de apresentação e não fazem HTTP (F1).

| Componente                             | Inputs                                                                                                | Outputs                                                |
| -------------------------------------- | ----------------------------------------------------------------------------------------------------- | ------------------------------------------------------ |
| `app-transactions-page` (rota)         | `ActivatedRoute.snapshot.queryParamMap` (`categoryId`, `month`)                                       | —                                                      |
| `app-transactions-filters`             | `filters: model<TransactionFilters>` (two-way), `categories: Category[]`                              | mudanças em `filters`                                  |
| `app-transactions-table`               | `transactions: Resource<Transaction[] \| undefined>`, `categories: Resource<Category[] \| undefined>` | `edit: Transaction`, `delete: Transaction`             |
| `app-transaction-form` (diálogo)       | `MAT_DIALOG_DATA: TransactionFormData` (`transaction?`, `categories`)                                 | fecha com `TransactionInput`, ou sem valor ao cancelar |
| `app-sort-menu` (shared)               | `options: SortOption[]`, `sort: model<Sort>` (two-way)                                                | mudanças em `sort`                                     |
| `app-month-field` (shared)             | `value: model<Date \| null>` (two-way, via `[formField]`), `clearable`                                | mudanças em `value`                                    |
| `app-page-header` (shared)             | `text` e as ações como conteúdo projetado                                                             | —                                                      |
| `[appMatChipColor]` (shared, diretiva) | a cor `#RRGGBB` da categoria                                                                          | —                                                      |

### Textos e nomes acessíveis

Os textos que o usuário vê estão na spec (FR-001 a FR-032). No diálogo, o "Valor" tem o prefixo "R$" e o marcador
"0,00", o grupo de tipo tem o nome acessível "Tipo" e a descrição tem o contador "N/255". Os nomes acessíveis dos botões
só com ícone são "Recarregar", "Exibir filtros"/"Ocultar filtros", "Ordenar: <opção>", "Editar", "Excluir", "Limpar
descrição", "Limpar tipo", "Limpar categoria" e "Limpar mês"; os do cabeçalho e das linhas também aparecem como dica, e
os "Limpar …" ficam sem dica. Os nomes dos controles do calendário ainda são os padrões do Material, em inglês (desvio
conhecido da FR-030, que bloqueia a entrega); o conjunto completo dos rótulos em português é definido no bug-fix.
