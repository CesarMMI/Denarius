# Contratos: Gestão de categorias

## API consumida

Definida pelo back em
[`back/specs/001-category-management/contracts/categories-api.yaml`](../../../../back/specs/001-category-management/contracts/categories-api.yaml)
(não copiada nem alterada aqui). Toda requisição passa pelo `CategoriesService`
(`front/src/app/categories/services/categories.service.ts`); `{apiUrl}` vem de `src/environments/`.

| Método do service     | Requisição                                                                                                                                                                                                  | Resposta usada                                     |
| --------------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | -------------------------------------------------- |
| `list(filters, sort)` | `GET {apiUrl}/categories[?name=…][&withTransaction=true\|false][&dateRef=YYYY-MM-DD][&orderBy=name\|transactionCount\|balance&asc=true\|false]` — devolve `{ url, params }` para o `httpResource` da página | `200` `Category[]`                                 |
| `create(input)`       | `POST {apiUrl}/categories` com `{ name, color }`                                                                                                                                                            | `201` (corpo ignorado; a página recarrega a lista) |
| `update(id, input)`   | `PUT {apiUrl}/categories/{id}` com `{ name, color }`                                                                                                                                                        | `200` (corpo ignorado; a página recarrega a lista) |
| `delete(id)`          | `DELETE {apiUrl}/categories/{id}`                                                                                                                                                                           | `204`                                              |

Erros: `400` e `404` chegam como `ProblemDetails`, e a página mostra o `detail`; o `500` do back também traz um
`detail` genérico. Só sem `ProblemDetails` (por exemplo, sem conexão) a página usa os seus textos.

Desvios conhecidos do back, com destino fora desta feature (ver a tabela de desvios do [plan](../plan.md#desvios-conhecidos-trabalho-de-outros-fluxos-não-desta-feature)):

- **`dateRef`**: o contrato declara `format: date-time`; o front envia `YYYY-MM-DD` (o dia 1 do mês), que o back
  aceita e testa (`back/tests/Denarius.WebAPI.Tests/Categories/CategoriesControllerTests.cs:211`). Só de documentação
  do contrato; corrigido pelo fluxo de bugs no back (D3 da pesquisa), sem mudança no front.
- **`name`**: o contrato deixa a diferenciação de maiúsculas "a cargo do banco"; a spec (FR-017) pede que não
  diferencie maiúsculas e continue diferenciando acentos. Desvio do back, com destino num ciclo do back.

## UI exposta

- **Rota** `/categories`, pública, carregada sob demanda (`loadChildren` em `app.routes.ts` → `loadComponent` da
  `CategoriesPage`), com o item "Categorias" (ícone `sell`) em terceiro no menu, depois de "Relatórios" e "Transações".
- **Endereço gerado** pelo link da quantidade: `/transactions?categoryId=<id>`. Esperado pela FR-004 (desvio
  conhecido): `/transactions?categoryId=<id>&month=YYYY-MM` com um mês escolhido. Quem recebe esse endereço é a
  [003-gestao-de-transacoes](../../003-gestao-de-transacoes/spec.md).
- **Módulos que outras features importam**: `services/categories.service.ts`, `types/category.ts` e
  `testing/category-fixture.ts` são importados por `transactions/`, com exceção à F1 registrada no plan da 003, até um
  ciclo de refatoração movê-los para `shared/` (D5 da pesquisa).

### Cabeçalho da página (`app-page-header`, título "Categorias")

| Controle                                     | Nome acessível / dica                        | Ação                                                            |
| -------------------------------------------- | -------------------------------------------- | --------------------------------------------------------------- |
| Botão de ícone `refresh`                     | "Recarregar" / "Recarregar"                  | `categories.reload()`                                           |
| Botão de ícone `filter_alt`/`filter_alt_off` | "Exibir filtros"/"Ocultar filtros" (as duas) | Alterna `filtersVisible`                                        |
| `app-sort-menu`                              | "Ordenar: <opção>" (as duas)                 | Seis opções `menuitemradio`, a em uso com `aria-checked="true"` |
| Botão "Nova categoria" (`add`)               | texto visível                                | Abre o diálogo sem dados                                        |

### Componentes (inputs → outputs), todos de apresentação

| Componente                                   | Inputs                                          | Outputs                                     | Observações                                                                                                                                                                                    |
| -------------------------------------------- | ----------------------------------------------- | ------------------------------------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `app-categories-filters`                     | `filters` (`model.required<CategoryFilters>`)   | `filtersChange` (two-way)                   | "Nome" com debounce de 300 ms ou na saída do campo; "Limpar nome", "Limpar transações no período" e, pelo `app-month-field clearable`, "Limpar mês" só com o filtro preenchido                 |
| `app-categories-table`                       | `categories: Resource<Category[] \| undefined>`; `deleting: ReadonlySet<string>` | `edit: Category`, `delete: Category`        | Colunas `name`, `transactionCount`, `balance`, `actions`; o `delete` só é emitido com `canDelete` e não é emitido para a linha em exclusão, que mostra um spinner e desabilita "Editar" e "Excluir" |
| `app-category-form` (diálogo do `MatDialog`) | `MAT_DIALOG_DATA: Category \| undefined`        | `afterClosed(): CategoryInput \| undefined` | Título "Nova categoria"/"Editar categoria"; "Cancelar" (`mat-dialog-close`) e "Salvar" (submit)                                                                                                |

### Componentes compartilhados usados (`front/src/app/shared/`)

| Componente          | Contrato usado aqui                                                    |
| ------------------- | ---------------------------------------------------------------------- |
| `app-page-header`   | `text` + ações projetadas                                              |
| `app-sort-menu`     | `options: SortOption[]`, `sort` (`model.required<Sort>`)               |
| `app-month-field`   | `value` (`model<Date \| null>`, ligado por `[formField]`), `clearable` |
| `[appMatChipColor]` | cor `#RRGGBB` → tons 90/30 em `light-dark(…)`                          |

### Mensagens (`MatSnackBar`)

| Situação                           | Texto                                                      | Ação       | Duração |
| ---------------------------------- | ---------------------------------------------------------- | ---------- | ------- |
| Criação aceita                     | "Categoria criada."                                        | —          | 3 s     |
| Edição aceita                      | "Categoria salva."                                         | —          | 3 s     |
| Exclusão aceita                    | "Categoria excluída."                                      | "Desfazer" | 5 s     |
| Desfazer aceito                    | "Categoria restaurada."                                    | —          | 3 s     |
| Falha ao criar, editar ou desfazer | `detail` da API ou "Não foi possível salvar a categoria."  | "Fechar"   | 5 s     |
| Falha ao excluir                   | `detail` da API ou "Não foi possível excluir a categoria." | "Fechar"   | 5 s     |

As mensagens são exibidas como texto: `MatSnackBar.open` recebe uma string, que o `SimpleSnackBar` do Material mostra
por interpolação (garantia do Material, sem teste próprio). Um `detail` vazio (`""`) apareceria como mensagem vazia,
porque o texto da página só entra quando o `detail` não existe; o back de hoje não produz esse caso.
