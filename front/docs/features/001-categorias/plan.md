# Plano: Gestão de categorias

**Spec**: [spec.md](./spec.md) · **Testes de caracterização**: `da60700` · **Contrato consumido**: [`categories-api.yaml`](../../../../back/docs/features/001-categorias/contracts/categories-api.yaml); a UI está em [`contracts/categories-ui.md`](./contracts/categories-ui.md)

Plano retroativo: documenta a arquitetura já entregue em `front/src/app/categories/` (e nas peças de `shared/` que ela usa). O trabalho que coube a esta feature foram testes de caracterização e o ajuste de dados de teste; nenhum código de produção mudou.

## Conferência com as regras

- Organização (F1): `categories/` com `pages/`, `components/`, `services/`, `types/` e `testing/`; URLs só nos services; a página guarda o estado e orquestra diálogo e mensagens; tabela, filtros e formulário não fazem HTTP. **Exceção aprovada pelo usuário**: o `CategoryForm` injeta o `ColorsService`, cujo `httpResource` (`providedIn: 'root'`) busca a paleta estática `public/data/default-colors.json` na primeira abertura do diálogo (`category-form.ts`, `colors.service.ts`); a paleta não é a API, e o service faz o papel de cache (SC-009); a alternativa, a página buscar a paleta e passá-la ao diálogo, a carregaria antes da hora. Também vale a exceção de `transactions/` (importa `categories.service.ts`, `types/category.ts` e `testing/category-fixture.ts`), registrada no [plan da 003](../002-transacoes/plan.md), até um ciclo de refatoração mover isso para `shared/`.
- Contrato (F2 e IV): só os endpoints de categorias de `back/docs/api.md` e do contrato; `types/category.ts` espelha `CategoryOutput` (oito campos) e `CategoryInput`. O contrato ainda documenta `dateRef` como `format: date-time`, e o front envia só `YYYY-MM-DD` (`categories.service.ts`); a API aceita a data sem hora, então não há efeito no front.
- Rotas (F2): `/categories`, pública e não renomeada; o link da quantidade usa `/transactions?categoryId=<id>`, que a 003 recebe.
- Banco (F3): nenhuma migration.
- Material (F4): componentes e tokens do Material em toda a página. Código próprio mínimo: a paleta de botões com `input type="color"` (o Material não tem seletor de cor) e a diretiva `shared/mat-chip-color` (contraste de 7:1, SC-005). Só desktop (diálogo de 400 px).
- Segurança (II): nomes e mensagens da API só por interpolação e `MatSnackBar`, sem `innerHTML` nem `bypassSecurityTrust*`; a validação do diálogo é só de experiência, e a API valida de novo; nenhum `console.*`, e todo `subscribe` trata o erro (o `httpResource` guarda o erro no estado, então o `HttpErrorResponse`, com o `?name=` buscado na URL, não chega ao `ErrorHandler` global); o `environment.ts` de produção usa HTTPS.
- Performance (III): rota lazy (`app.routes.ts`, `categories.routes.ts`); filtros, ordenação e agregação na API; debounce de 300 ms em "Nome"; `shallowEqual` nos signals `filters` e `sort` e `reloadWhenIdle` evitam consulta repetida (FR-023, SC-006); signals `deleting` e `restoring` evitam o clique repetido (FR-012, FR-013); paleta buscada uma vez (SC-009). Budgets do `angular.json` respeitados no último build medido (chunk lazy `categories-page` com cerca de 15 kB; bundle inicial com 633 kB, abaixo do aviso de 700 kB).
- Dependência nova: nenhuma.

## Mudanças

- **Telas e componentes** (`src/app/categories/`): `pages/categories-page` (estado: signals de filtros, visibilidade e ordenação, `httpResource` da lista, diálogo e mensagens), `components/categories-filters` (Signal Forms, debounce de 300 ms), `categories-table` (rótulo, link da quantidade, saldo, Editar/Excluir, estados) e `category-form` (Reactive Forms, diálogo). Reaproveita de `shared/`: `page-header`, `sort-menu`, `month-field`, `mat-chip-color`, `date-utils`, `datepicker-intl` (calendário em português), `shallow-equal` e `reload-when-idle`.
- **Serviços e tipos**: `categories.service.ts` (lista com os parâmetros, criar, atualizar, excluir), `colors.service.ts` (paleta), `types/category.ts`, `types/category-filters.ts` (só do front) e `testing/category-fixture.ts`.
- **`docs/design.md`**: nada muda; o rótulo segue o padrão de chip já documentado.

## Decisões

Detalhes e alternativas em [`research.md`](./research.md). Em resumo: filtros, ordenação, quantidade e saldo calculados pela API (princípio III); estado da lista num `httpResource` da página, com a comparação por conteúdo nos filtros e na ordenação; a paleta é um JSON estático, e não um endpoint novo; a exclusão é imediata, com "Desfazer" que recria a categoria (a API não restaura), em vez de confirmação.

## Testes previstos

Um `*.spec.ts` ao lado de cada arquivo de código (só `categories.routes.ts`, `types/` e `testing/` ficam sem, por não terem lógica). Os testes de caracterização desta feature cobrem comportamento já aceito e passam desde a primeira execução; cada um foi visto falhar uma vez, quebrando de propósito o comportamento coberto.

- `category-form.spec.ts`: `maxlength="100"` e contador "0/100", "N/100", "100/100" (FR-008); cor fora da paleta sem cor marcada e na "Cor personalizada" (Casos-limite); grupo "Cor" com `role="group"` e "Cor personalizada" (FR-020).
- `categories-page.spec.ts`: exclusão sem corpo de erro (rede) mostra "Não foi possível excluir a categoria." com "Fechar" e sem recarregar (FR-014); "Desfazer" recusado, com `detail` da API e com erro de rede (FR-013, FR-014); ocultar filtros não refaz a consulta nem os limpa (FR-015); recarga mantém as linhas, sem indicador (FR-006); título e dicas do cabeçalho (FR-001, FR-020). Os dados de teste usam respostas reais da API: nome só com espaços recusado com `400` e o `detail` do domínio, e a recusa de excluir com a mensagem do `DeleteCategoryUseCase`.
- `categories-table.spec.ts`: `<b>teste</b>` literal no rótulo (FR-021); dica de "Editar" (FR-020). O texto do `MatSnackBar` é exibido como texto pelo próprio Material, sem teste próprio.
- `shared/sort-menu/sort-menu.spec.ts`: a dica do botão é o `label()` "Ordenar: …" (FR-019, FR-020).
- `app.spec.ts`: em `/categories`, o item "Categorias" tem `aria-current="page"` (FR-001).
- Regressão dos desvios corrigidos: [`consulta-repetida-sem-mudanca`](../../bugs/consulta-repetida-sem-mudanca/) (FR-023), [`calendarios-em-ingles`](../../bugs/calendarios-em-ingles/) (FR-020) e [`clique-repetido-em-excluir`](../../bugs/clique-repetido-em-excluir/) (FR-012, FR-013).
- Sem teste, por decisão: "Limpar mês" no painel (coberto em `month-field.spec.ts`) e a "Cor personalizada" sobrescrita por uma paleta atrasada.

## Riscos

- Desvios ainda abertos, cada um num fluxo de bugs próprio, sem bloquear: link da quantidade sem o mês (FR-004, `categories-table.html`); diálogo que fecha antes da resposta (FR-010, vale também para transações); dia escolhido no `month-field` que não muda o filtro (FR-016, campo compartilhado).
- Trabalho futuro, em ciclos próprios: remover `ColorsService.reload()`, sem uso em produção (`colors.service.ts:15`); mover `Category`, a fixture e a requisição da lista para `shared/` (exceção da F1 acima); uma feature de acessibilidade (pontos da spec). As correções no `shared/month-field` e essa refatoração mexem em peças desta página e precisam rodar os specs de `categories/`.

## Verificação

De dentro de `front/`: `npx ng test --watch=false` (suíte completa), `npm run lint` e `npm run build`, sem estouro de budget e com o chunk `categories-page` fora do bundle inicial. Manual, com a API no ar (`npm start`, `http://localhost:4200/categories`):

1. A lista vem em ordem A–Z, com o nome no rótulo colorido, a quantidade como link e o saldo em BRL (negativo em vermelho); no tema escuro, os tons do rótulo se invertem.
2. "Nova categoria": "0/100" e a primeira cor marcada; "Salvar" sem nome mostra "Informe um nome"; com nome, "Categoria criada.". "Editar" com "Cor personalizada": "Categoria salva.".
3. "Excluir" numa categoria sem transações: "Categoria excluída." e, ao desfazer, "Categoria restaurada."; com transações, o botão está desabilitado, com a dica.
4. Filtros: "Nome" (recarrega após a pausa), "Com transações", um mês (quantidades e saldos do mês); ocultar e exibir mantém os valores; ordenar por "Menor saldo primeiro" atualiza o botão e a marca.
5. Quantidade de uma categoria abre `/transactions?categoryId=<id>`. Com a API parada, "Recarregar" mostra "Não foi possível carregar as categorias."; um nome de 100 caracteres cabe no rótulo numa linha, com o cartão rolando na horizontal.
