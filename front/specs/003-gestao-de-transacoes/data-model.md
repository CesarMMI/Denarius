# Modelo de dados: Gestão de transações

O front não tem banco (F3). Este documento descreve os tipos que a página usa, o estado que ela guarda e como os
valores do formulário, dos filtros e do endereço viram requisições para a API. As regras de negócio de cada campo são
do backend, em [002-transaction-management/data-model.md](../../../back/specs/002-transaction-management/data-model.md)
e [001-category-management/data-model.md](../../../back/specs/001-category-management/data-model.md); aqui fica só o
que o front acrescenta ou converte. Os caminhos são relativos a `front/src/app/`.

## Tipos

### `Transaction` (`transactions/types/transaction.ts:1-9`)

Espelha `TransactionOutput` de `transactions-api.yaml` (F2), sem campos supostos.

| Campo                    | Tipo             | Observações                                                                          |
| ------------------------ | ---------------- | ------------------------------------------------------------------------------------ |
| `id`                     | `string`         | UUID da API.                                                                         |
| `description`            | `string \| null` | `null` quando a transação não tem descrição.                                         |
| `categoryId`             | `string`         | UUID de uma categoria.                                                               |
| `value`                  | `number`         | Com sinal: positivo é entrada, negativo é saída; nunca zero na API.                  |
| `date`                   | `string`         | Meia-noite UTC do dia (`2026-09-24T00:00:00Z`); o front só usa a parte `YYYY-MM-DD`. |
| `createdAt`, `updatedAt` | `string`         | Recebidos e não usados pela página.                                                  |

### `TransactionInput` (`transactions/types/transaction.ts:11`)

`Pick<Transaction, 'description' | 'categoryId' | 'value' | 'date'>`: o corpo de `POST` e `PUT`, igual a
`CreateTransactionInput` e `UpdateTransactionInput`. É também o resultado com que o diálogo fecha.

### `TransactionFilters` (`transactions/types/transaction-filters.ts:1-8`)

| Campo          | Tipo                  | Vazio                     | Vira na query                                   |
| -------------- | --------------------- | ------------------------- | ----------------------------------------------- |
| `description?` | `string`              | `''`                      | `description=<texto>`                           |
| `type?`        | `'in' \| 'out' \| ''` | `''` ("Todos")            | `type=in` ou `type=out`                         |
| `categoryId?`  | `string`              | `''` ("Todas")            | `categoryId=<id>`                               |
| `month?`       | `Date \| null`        | `null` ("Todos os meses") | `dateRef=YYYY-MM-01` (primeiro dia, data local) |

Os quatro campos são opcionais (`transaction-filters.ts:3-7`): um filtro vazio ou ausente é omitido da query
(`transactions/services/transactions.service.ts:19-22`). A página sempre preenche os quatro, com os valores vazios da
tabela quando não há filtro.

### `Category` (`categories/types/category.ts:1-10`, importado de outra feature)

Espelha `CategoryOutput` de `categories-api.yaml`. A página usa só `id`, `name` e `color`: o nome e a cor da etiqueta,
as opções do filtro "Categoria" e do formulário. A importação desse tipo de dentro de `categories/` é coberta pela
exceção à F1 aprovada em 2026-10-05, até um ciclo de refatoração movê-lo para `shared/` (ver `plan.md`).

### `TransactionFormData` (`transactions/components/transaction-form/transaction-form.ts:15-18`)

O `MAT_DIALOG_DATA` do diálogo: `transaction` (ausente na criação) e `categories`, uma cópia da lista de categorias
feita quando o diálogo abre (`transactions/pages/transactions-page/transactions-page.ts:90`).

### `SortOption` (`shared/sort-menu/sort-menu.ts:9-11`)

`Sort` do Material (`active`, `direction`) com um `label`. As oito opções da página estão em
`transactions-page.ts:62-71`; `active` é o valor de `orderBy` da API (`date`, `description`, `categoryName`, `value`) e
`direction` vira `asc=true|false`.

## Estado da página (`TransactionsPage`)

| Signal / recurso | Tipo                          | Início                                                                                                    | Muda quando                                                                |
| ---------------- | ----------------------------- | --------------------------------------------------------------------------------------------------------- | -------------------------------------------------------------------------- |
| `filters`        | `signal<TransactionFilters>`  | `{ description: '', type: '', categoryId: <do endereço ou ''>, month: <do endereço ou null> }` (`:53-58`) | O usuário muda ou limpa um filtro                                          |
| `filtersVisible` | `signal<boolean>`             | `true` se o endereço trouxe categoria ou mês válido (`:59`)                                               | "Exibir filtros"/"Ocultar filtros"; ocultar não mexe em `filters`          |
| `sort`           | `signal<Sort>`                | `{ active: 'date', direction: 'desc' }` (`:61`)                                                           | O usuário escolhe uma opção no menu                                        |
| `transactions`   | `httpResource<Transaction[]>` | Primeira consulta ao abrir                                                                                | `filters` ou `sort` mudam (nova requisição); `reload()` (mesma requisição) |
| `categories`     | `httpResource<Category[]>`    | Primeira consulta ao abrir                                                                                | Só `reload()` ("Recarregar")                                               |
| `categoryList`   | `computed<Category[]>`        | `[]` até as categorias chegarem, e em erro (`:78`)                                                        | —                                                                          |

## Estado derivado da tabela (`TransactionsTable`)

| Derivado        | Regra                                                                                                                               | Onde                            |
| --------------- | ----------------------------------------------------------------------------------------------------------------------------------- | ------------------------------- |
| `rows`          | As transações, só quando os dois recursos têm valor; senão `[]`                                                                     | `transactions-table.ts:39-42`   |
| `sameDate`      | As linhas seguidas por outra com o mesmo `date.slice(0, 10)`                                                                        | `transactions-table.ts:44-47`   |
| `categoryById`  | `Map` de `id` para `Category`                                                                                                       | `transactions-table.ts:49-52`   |
| Linha sem dados | Spinner se algum recurso carrega; "Não foi possível carregar as transações." se algum falhou; senão "Nenhuma transação encontrada." | `transactions-table.html:47-57` |

### Estados do recurso e o que a tabela mostra

| `transactions` / `categories`                                                         | Linhas                              | Linha sem dados                            |
| ------------------------------------------------------------------------------------- | ----------------------------------- | ------------------------------------------ |
| `loading` em qualquer um (primeira carga, novo filtro ou ordenação)                   | nenhuma                             | spinner                                    |
| `reloading` com valor nos dois ("Recarregar", depois de salvar, excluir ou restaurar) | as atuais, até a nova versão chegar | —                                          |
| `reloading` com lista vazia ou depois de erro                                         | nenhuma                             | spinner                                    |
| `error` em qualquer um, sem carregamento em andamento                                 | nenhuma                             | "Não foi possível carregar as transações." |
| `resolved` nos dois, com transações                                                   | as transações                       | —                                          |
| `resolved` nos dois, sem transações                                                   | nenhuma                             | "Nenhuma transação encontrada."            |

## Conversões

### Formulário → `TransactionInput` (`transaction-form.ts:61-71`)

| Campo do formulário                                     | Validação no front (hoje)                           | Vai para a API como                                                                                   |
| ------------------------------------------------------- | --------------------------------------------------- | ----------------------------------------------------------------------------------------------------- |
| `type` (`'in'`/`'out'`)                                 | — (começa em `'out'`)                               | O sinal de `value`                                                                                    |
| `value` (texto, com o prefixo "R$" e o marcador "0,00") | Obrigatório; `/^\d+([.,]\d{1,2})?$/`                | `parseFloat` com vírgula trocada por ponto (`:64`), um número de ponto flutuante; negativo se `'out'` |
| `date` (`Date`)                                         | Obrigatório; data válida para o `NativeDateAdapter` | `YYYY-MM-DDT00:00:00.000Z` do dia local (`DateUtils.toApiDate`)                                       |
| `categoryId`                                            | Obrigatório                                         | Como está                                                                                             |
| `description`                                           | `maxlength` 255 no input, com o contador "N/255"    | Com trim; `null` se ficar vazia                                                                       |

Desvios conhecidos nessa validação e conversão (FR-014, fluxo de bugs do front): o padrão aceita zero e qualquer
quantidade de algarismos; o `parseFloat` altera os valores com 14 ou mais algarismos inteiros ("99999999999999.99" vai
no JSON como 99999999999999.98); e a data digitada é lida pelo `Date.parse` (mês/dia/ano, ou ISO em UTC), e não como
dia/mês/ano. A data digitada e os valores com 14 ou mais algarismos gravam um dado diferente do digitado: são violações
conhecidas do princípio II e bloqueiam a entrega desta feature; o zero, que a API recusa com `400`, não bloqueia. Do
lado do back, a API só recusa o zero, arredonda sem aviso mais de duas casas e responde `500` a partir de 10^16 (desvio
conhecido do back, que não bloqueia).

### `Transaction` → formulário de edição (`transaction-form.ts:45-58`)

| Campo         | Valor inicial                                                                             |
| ------------- | ----------------------------------------------------------------------------------------- |
| `type`        | `'in'` se `value > 0`, senão `'out'`                                                      |
| `value`       | `Math.abs(value).toFixed(2)` com vírgula (`186,42`, `8600,00`)                            |
| `date`        | `DateUtils.fromApiDate(date)`: o dia como data local                                      |
| `categoryId`  | `transaction.categoryId` (na criação, a primeira categoria da lista, ou `''` sem nenhuma) |
| `description` | `transaction.description ?? ''`                                                           |

### Exclusão → "Desfazer" (`transactions-page.ts:101-111`)

A transação excluída é recriada com `POST` e `{ description, categoryId, value, date }` copiados da linha, com a data
exatamente como veio da API. A restaurada ganha `id`, `createdAt` e `updatedAt` novos.

### Endereço → estado inicial (`transactions-page.ts:49-59`)

| Parâmetro    | Leitura                                                                             | Inválido                                                                                  |
| ------------ | ----------------------------------------------------------------------------------- | ----------------------------------------------------------------------------------------- |
| `categoryId` | Como veio, sem validação                                                            | Vai para a API: malformado → `400` → erro na tabela; inexistente → `200 []` → lista vazia |
| `month`      | `DateUtils.fromMonthKey`: `^(\d{4})-(0[1-9]\|1[0-2])$` → primeiro dia do mês, local | Ignorado (`null`), como se não houvesse mês                                               |

Lidos só uma vez, do `snapshot`; os filtros escolhidos depois não são gravados no endereço.

## Transições

Não há entidade com ciclo de vida no front. As únicas transições são as dos recursos (tabela acima) e as do diálogo:

```text
fechado ──"Nova transação"/"Editar"──▶ aberto ──"Cancelar"/Esc/clique fora──▶ fechado (nada é salvo)
                                         └──"Salvar" com campos válidos──▶ fechado com TransactionInput ──▶ POST/PUT
                                                                             ├─ sucesso: snackbar 3 s + reload()
                                                                             └─ recusa: snackbar 5 s com "Fechar"
```

O diálogo fechar antes da resposta da API é o desvio conhecido da FR-016; o esperado (aberto até a API aceitar, sem
um segundo "Salvar", e aberto com o que foi digitado na recusa) é corrigido pelo fluxo de bugs do front, e o
comportamento durante a espera fica para o `/speckit-bug-assess` dessa correção.
