# Plano: Gestão de transações

**Spec**: [spec.md](./spec.md) · **Entregue em**: `e67d201` (página), `ef291ee` (testes de caracterização) · **Pesquisa**: [research.md](./research.md) · **Contrato da tela**: [contracts/transactions-ui.md](./contracts/transactions-ui.md)

## Conferência com as regras

- Organização (F1): feature `src/app/transactions/` (`pages/`, `components/`, `services/`, `types/`, `testing/`), com rota lazy. **Exceção aprovada pelo usuário (2026-10-05)**: `transactions/` importa o `CategoriesService`, o tipo `Category` e o `buildCategory` (testes) de `categories/` (`transactions-page.ts`, `transaction-form.ts`, `transactions-filters.ts`, `transactions-table.ts`), até uma refatoração mover essas peças para `shared/`; refatorar agora mexeria em três features, contra o princípio V.
- Contrato (F2 e IV): consome `transactions-api.yaml` e a lista de `categories-api.yaml` como estão ([`back/docs/api.md`](../../../../back/docs/api.md)); `Transaction`, `TransactionInput` e `Category` espelham os schemas. O contrato declara `dateRef` como `date-time`, embora a API aceite a data sem hora que o front envia: é documentação do back, sem efeito aqui.
- Rotas (F2): `/transactions`, a raiz (`''` redireciona) e os parâmetros `categoryId` e `month`, lidos uma vez do `snapshot`; nenhum caminho renomeado.
- Banco (F3): nenhuma migration.
- Material (F4): `mat-table`, chips (cores por tokens `--mat-chip-*` com `light-dark()`, pela diretiva `MatChipColor`), diálogo, datepicker, button-toggle, select, menu, snackbar, tooltip e spinner; os quatro `.scss` da feature têm de 5 a 18 linhas.
- Segurança (II): textos interpolados nos templates, sem `innerHTML` nem `bypassSecurityTrust*` (SC-008, teste C13); as mensagens de erro são `ProblemDetails.detail` ou a padrão, via `MatSnackBar.open` (`transactions-page.ts`); parâmetros por `HttpParams`; nenhum `console.*`, e toda requisição das mutações trata o erro, então nada chega ao `ErrorHandler` global. O texto da busca vai na query string porque o contrato é do back, que não registra URLs. A integridade do dado digitado (data, valores com mais de 13 algarismos inteiros) foi corrigida em [data-digitada-como-mes-dia-ano](../../bugs/data-digitada-como-mes-dia-ano/) e [valor-com-muitos-algarismos](../../bugs/valor-com-muitos-algarismos/); a faixa e a escala do valor, no back, em [valor-sem-faixa-e-escala](../../../../back/docs/bugs/valor-sem-faixa-e-escala/).
- Performance (III): filtros e ordenação na API (`transactions.service.ts`); categorias só ao abrir e em "Recarregar"; busca com `debounce` de 300 ms; rota lazy; o `httpResource` aborta a carga anterior. A consulta repetida e o clique repetido em "Excluir" foram corrigidos ([consulta-repetida-sem-mudanca](../../bugs/consulta-repetida-sem-mudanca/), [clique-repetido-em-excluir](../../bugs/clique-repetido-em-excluir/)). Budgets do `angular.json` respeitados (referência: Initial total de 633,08 kB, chunk `transactions-page` de 41,61 kB). A lista sem paginação fica aceita até uma medição mostrar lentidão.
- Idioma: rótulos do calendário em português, via `PtBrDatepickerIntl` provido no `MonthField` e no `TransactionForm`, sem mexer no bundle inicial ([calendarios-em-ingles](../../bugs/calendarios-em-ingles/)).
- Dependência nova: nenhuma.

## Mudanças

- **Telas e componentes**: `TransactionsPage` guarda o estado (signals `filters`, `filtersVisible`, `sort`, `deleting`, `restoring` e dois `httpResource`s), faz as mutações, mostra os snackbars e oferece "Desfazer" recriando a transação. `TransactionsFilters` (Signal Forms), `TransactionsTable` (agrupa por dia e pinta a etiqueta com `MatChipColor`) e `TransactionForm` (diálogo com Reactive Forms e `PtBrDateAdapter`) são de apresentação. Do `shared/`: `page-header`, `sort-menu`, `month-field`, `mat-chip-color`, `date-utils`, `shallow-equal`, `reload-when-idle` e `datepicker-intl`.
- **Serviços e tipos**: `TransactionsService.list` monta `{ url, params }` a partir dos filtros e da ordenação; `create`, `update` e `delete` devolvem `Observable`s. Tipos em `transactions/types/`.
- **`docs/design.md`**: sem mudança permanente.

## Decisões

Detalhadas em [research.md](./research.md); em resumo:

- **Dados e estado**: a página é dona do estado, e filtragem e ordenação vão para a API (FR-031), com recarga da lista a cada mutação, porque a presença e a posição da transação dependem dos filtros e da ordenação.
- **Consulta repetida**: `shallowEqual` no `equal` dos signals `filters` e `sort` da página, e não no menu de ordenação nem no `MonthField` compartilhados.
- **Exclusão**: o guarda `deleting` por id e o `reloadWhenIdle`, que repete a recarga quando a lista já está carregando, evitam o segundo pedido e a perda do "Desfazer"; "Desfazer" recria a transação, porque a API não desfaz exclusões.
- **Datas**: o dia do calendário, sem fuso (`DateUtils`); a data digitada é lida pelo `PtBrDateAdapter`.
- **Estados**: as linhas esperam as duas listas, porque cada uma depende do nome e da cor da categoria.

## Testes previstos

Um `*.spec.ts` ao lado de cada arquivo (service, página, tabela, filtros e formulário, mais os de `shared/` e `app.routes.spec.ts` para a raiz). Os testes de caracterização do comportamento já existente e aceito (`research.md` → Decisões do usuário), conferidos quebrando de propósito o comportamento coberto: `categoryId` malformado e inexistente (FR-028), raiz → `/transactions` (FR-002), recarga sem indicador (FR-008), estrutura de rolagem (FR-011; a rolagem real é conferida à mão), campo "Data" vazio, formulário sem categoria, valor com ponto, milhar e sinal, contador "N/255" (FR-014), exclusão e restauração recusadas (FR-021, FR-022), `categoryId` com `month` juntos e marcação como texto (SC-008, FR-032). Cada bug corrigido tem o seu teste de reprodução. Ainda sem teste, por serem desvios abertos: FR-016, FR-017, FR-019, o zero em FR-014 e o dia escolhido no "Mês" em FR-024.

## Riscos

- Os desvios abertos (FR-014 zero, FR-016, FR-017, FR-019, FR-024) são defeitos com destino no fluxo de bugs, não exceções. A correção da FR-024 mexe no `shared/month-field`, usado também por categorias e relatórios, e deve considerar as três páginas.
- O teste de estilos do cabeçalho e da rolagem roda em jsdom, que não mede layout.

## Verificação

`npx ng test --watch=false`, `npm run lint` e `npm run build` sem erro nem estouro de budget. À mão, com a API no ar e duas categorias, numa janela de 1366×768: lista com etiquetas e valores; só a lista rolando com os filtros à vista; criar, editar, excluir e desfazer; filtros, ordenação e endereços (`?month=2026-09`, `?categoryId=<id>`, `?month=setembro`); com a API parada, "Recarregar" mostra o erro. Na aba Rede, abrir a página faz uma consulta de transações e uma de categorias, cada mudança de filtro ou de ordenação, uma de transações, e repetir a mesma escolha, nenhuma (SC-007).
