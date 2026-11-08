# Correção do bug: o "Excluir" aceita clique repetido e nada mostra a exclusão ou a restauração em andamento

- **Slug**: clique-repetido-em-excluir
- **Corrigido em**: 2026-10-07
- **Avaliação**: ./assessment.md
- **Status**: applied

## Resumo

Correção preferida da avaliação, com as escolhas do usuário (P1–P4 = A, P5 = a, P6 = A). Cada página guarda os ids em
exclusão (`deleting`) e um contador de restaurações (`restoring`). A página ignora um segundo pedido para o mesmo id, e
a tabela desabilita o "Excluir" e o "Editar" da linha e troca o ícone do "Excluir" por um spinner, do clique até a
linha sair da lista ou até a recusa. Durante o `POST` do "Desfazer", um `mat-progress-bar` aparece no topo do cartão
da lista. Toda recarga da lista depois de excluir, restaurar ou salvar passa por `reloadWhenIdle` (novo, em
`shared/`): se o `reload()` do resource for ignorado porque a lista já carrega, ele é repetido quando essa carga
termina com sucesso.

## Alterações

| Arquivo | Alteração | Notas |
|------|--------|-------|
| `front/src/app/shared/reload-when-idle/reload-when-idle.ts` | adicionado | `reloadWhenIdle(resource)`: devolve um `reload` que marca uma recarga pendente quando o `reload()` devolve `false`; um `effect` sobre o `status()` a dispara só com `resolved`, limpa a marca antes do `reload()` e a descarta num erro |
| `front/src/app/shared/reload-when-idle/reload-when-idle.spec.ts` | teste adicionado | 5 testes com um `httpResource` de teste |
| `front/src/app/categories/pages/categories-page/categories-page.ts` | modificado | `deleting`, `restoring`, `reloadCategories`; guarda em `delete()`, id retirado na recusa; `restore()` com `finalize`; `save()` recarrega por `reloadCategories` |
| `front/src/app/categories/pages/categories-page/categories-page.html` | modificado | `mat-progress-bar` ("Restaurando a categoria") enquanto `restoring() > 0`; `[deleting]` na tabela |
| `front/src/app/categories/pages/categories-page/categories-page.scss` | modificado | barra `position: sticky; top: 0; z-index: 1` e `margin-bottom` negativo igual a `--mat-progress-bar-track-height` (4px por padrão) |
| `front/src/app/categories/pages/categories-page/categories-page.spec.ts` | testes adicionados | `describe('deleting, while in progress')`, 11 testes |
| `front/src/app/categories/components/categories-table/categories-table.ts` | modificado | input `deleting` |
| `front/src/app/categories/components/categories-table/categories-table.html` | modificado | "Editar" e "Excluir" desabilitados na linha em exclusão (`disabledInteractive` e guarda no clique), spinner `diameter="20"` no "Excluir" |
| `front/src/app/categories/components/categories-table/categories-table.spec.ts` | testes adicionados | 2 testes |
| `front/src/app/transactions/pages/transactions-page/transactions-page.ts` | modificado | o mesmo da página de categorias (`reloadTransactions`) |
| `front/src/app/transactions/pages/transactions-page/transactions-page.html` | modificado | barra "Restaurando a transação"; `[deleting]` na tabela |
| `front/src/app/transactions/pages/transactions-page/transactions-page.scss` | modificado | a mesma regra da barra |
| `front/src/app/transactions/pages/transactions-page/transactions-page.spec.ts` | testes adicionados | `describe('deleting, while in progress')`, 11 testes |
| `front/src/app/transactions/components/transactions-table/transactions-table.ts` | modificado | input `deleting` |
| `front/src/app/transactions/components/transactions-table/transactions-table.html` | modificado | "Editar" e "Excluir" com `[disabled]`, `disabledInteractive` e guarda no clique; spinner no "Excluir" |
| `front/src/app/transactions/components/transactions-table/transactions-table.spec.ts` | testes adicionados | 2 testes |
| `front/specs/002-gestao-de-categorias/spec.md` | modificado | FR-012 com o prazo da P3; notas de desvio removidas (ver abaixo) |
| `front/specs/003-gestao-de-transacoes/spec.md` | modificado | notas de desvio removidas; a FR-020 aponta para este bug (ver abaixo) |

Nenhuma mudança no back, no contrato da API, nos services, nos tipos nem nas rotas. Nenhuma dependência nova: o
`MatProgressBarModule` é do Angular Material e entra só nos chunks sob demanda das duas páginas. O "Recarregar" do
cabeçalho continua chamando o `reload()` direto, como antes (a avaliação limita o passo 4 às recargas depois de
excluir, restaurar e salvar).

## Destaques do diff

```ts
// reload-when-idle.ts
export function reloadWhenIdle(resource: ReloadableResource): () => void {
	let pending = false;
	effect(() => {
		const status = resource.status();
		if (!pending || status === 'loading' || status === 'reloading') return;
		pending = false; // limpa antes, sem laço
		if (status === 'resolved') untracked(() => resource.reload());
	});
	return () => {
		if (!resource.reload()) pending = true;
	};
}

// categories-page.ts (o mesmo em transactions-page.ts)
protected delete({ id, name, color }: Category) {
	if (this.deleting().has(id)) return;
	this.deleting.update((ids) => new Set(ids).add(id));
	this.categoriesService.delete(id).subscribe({
		next: () => { this.reloadCategories(); /* "Desfazer" → this.restore(...) */ },
		error: (error) => { /* tira o id de deleting */ this.showError(...); },
	});
}
```

```html
<!-- categories-table.html (trecho) -->
@let beingDeleted = deleting().has(category.id);
<button matIconButton aria-label="Excluir" ... [disabled]="!category.canDelete || beingDeleted" disabledInteractive
	(click)="category.canDelete && !beingDeleted && delete.emit(category)">
	@if (beingDeleted) { <mat-progress-spinner mode="indeterminate" diameter="20" /> } @else { <mat-icon>delete</mat-icon> }
</button>
```

## Testes adicionados

Escritos antes da correção, com a skill `write-front-tests`: nos specs das páginas, sem harness e sem `whenStable()`
com pedido pendente; os estados em andamento são vistos com `fixture.detectChanges()`, `TestBed.tick()` vem antes de
`expectOne`/`match`, e todos os pedidos são respondidos antes do `verify()`. Quando a resposta de uma carga pode
disparar a recarga pendente, o teste espera uma tarefa (`setTimeout`) em vez do `whenStable()`, que esperaria esse
novo pedido. O spinner é procurado dentro do botão da linha, e não com o helper `spinner()` da página.

Execução "antes": os arquivos de código foram voltados ao `main` (`git checkout`) e a pasta `shared/reload-when-idle/`
foi posta de lado; rodaram os specs de `categories/` e `transactions/` com os testes novos (18 falhas, 153 passando, 171
testes). Em seguida o código foi devolvido. O `reload-when-idle.spec.ts`, rodado sem o `reload-when-idle.ts`, falha no
build: `TS2307: Cannot find module './reload-when-idle'`.

### Reprodução (falham antes, passam depois)

Em `categories-page.spec.ts` e em `transactions-page.spec.ts` (`describe('deleting, while in progress')`), o mesmo
teste nas duas páginas:

| Teste | Antes da correção (nas duas páginas) | Depois |
| --- | --- | --- |
| should not send another DELETE when "Excluir" is clicked again before the response | `expected [ TestRequest, … ] to have a length of 1 but got 2` | passa |
| should not send another DELETE after the deletion is accepted and before the list reloads | `expected [ TestRequest ] to have a length of +0 but got 1` | passa |
| should disable "Excluir" and show a spinner in the row while it is being deleted | `expected false to be true` (botão habilitado) | passa |
| should disable "Editar" while the row is being deleted (P6) | `expected false to be true` (botão habilitado) | passa |
| restoring › should show that the restoration is in progress | `expected null not to be null` (sem barra) | passa |
| when the list is already loading › should reload again when a deletion is accepted during a reload | `Expected one matching request …, found none` (o `reload()` se perde) | passa |
| when the list is already loading › should reload again when a deletion is accepted while the filters load the list | `Expected one matching request …, found none` | passa |

Nas tabelas (`categories-table.spec.ts` e `transactions-table.spec.ts`, `describe('while a … is being deleted')`):

| Teste | Antes da correção | Depois |
| --- | --- | --- |
| should disable its "Excluir" and "Editar", show a spinner in "Excluir" and emit nothing | `NG0303: Can't set value of the 'deleting' input` | passa |

E o `reload-when-idle.spec.ts` (falha no build antes, ver acima; os 5 passam depois): recarrega na hora fora de uma
carga; não perde a recarga pedida durante uma carga (exatamente um `GET` depois da resposta, com o valor novo); vários
`reload` na mesma carga geram um único `GET`; nenhum `GET` sai depois da recarga pendente respondida (sem laço); e,
se a carga em curso falha, a marca é descartada (nenhum `GET`) e um `reload` posterior recarrega na hora.

Os testes "during a reload" também conferem que, entre a primeira lista (que ainda traz a linha excluída) e a nova, o
"Excluir" dessa linha continua desabilitado e um clique não envia `DELETE`; e, no fim, que nenhum outro `GET` sai.

### Guarda (passam antes e depois)

Nas duas páginas:

- should keep "Excluir" of the other rows enabled, without a spinner, while a row is being deleted (e um clique nela
  envia o seu `DELETE`);
- should enable "Excluir" again when the deletion is refused (sem spinner, e um novo clique envia um `DELETE`);
- restoring › should hide the progress once the restoration is accepted;
- restoring › should hide the progress once the restoration is refused (sem recarga).

Nas tabelas, "should leave the other rows as they are" (outras linhas habilitadas e sem spinner, emitindo `edit` e
`delete`; na de categorias, a dica e o bloqueio de `canDelete` iguais) é guarda pelo comportamento, mas falhava antes
da correção pelo mesmo `NG0303`, porque depende do input novo (ver Desvios).

Nenhum teste existente foi alterado, ignorado ou apagado.

## Verificação local

Em `front/`:

- `npx ng test --watch=false` → 31 arquivos, 335 testes, todos passando (eram 304; +31: 5 do `reload-when-idle`, 2 em
  cada tabela e 11 em cada página).
- `npm run lint` → "All files pass linting."
- `npm run build` → sem erro nem aviso de budget (inclusive os de estilo por componente); Initial total 633,08 kB
  (igual à referência).
- `npx prettier --write` nos arquivos alterados e criados: nenhum mudou; nenhum arquivo não tocado mudou.

## Specs ajustadas (P4 = A)

- `front/specs/002-gestao-de-categorias/spec.md`:
  - Casos-limite: o item do segundo clique ficou só com a regra e as referências (FR-012, FR-013); saiu a nota
    "Desvio conhecido: hoje nada indica … não pode mais ser desfeita.".
  - FR-012 (P3): "Durante uma exclusão, um novo clique no "Excluir" da mesma linha NÃO DEVE enviar outro pedido" passou
    a "Do clique até a categoria sair da lista ou até a recusa da exclusão, um novo clique no "Excluir" da mesma linha
    NÃO DEVE enviar outro pedido", com o resto igual; saiu a nota "Desvio conhecido: hoje nada indica a exclusão em
    andamento … (Esclarecimentos, 2026-10-06)."
  - FR-013: saiu "Desvio conhecido: hoje nada indica a restauração em andamento (ver FR-012)."
  - Premissas: "o "Excluir" protegido contra clique repetido, com retorno visual na exclusão e na restauração
    (FR-012, FR-013)" passou a "(FR-012, FR-013, corrigido em `front/bugs/clique-repetido-em-excluir/`)".
- `front/specs/003-gestao-de-transacoes/spec.md`:
  - Casos-limite: o item do "Excluir" de novo na mesma linha ficou só com a regra e a referência (FR-020); saiu a nota
    "Desvio conhecido, que contraria o princípio III … está em andamento.".
  - FR-020: a nota "Desvio conhecido: hoje um clique repetido … definida no `/speckit-bug-assess` dessa correção." foi
    trocada por "; a forma do retorno visual está em `front/bugs/clique-repetido-em-excluir/`.".
  - Premissas: "Também ficam fora o clique repetido em "Excluir" sem retorno visual (FR-020), pelo fluxo de bugs do
    front;" passou a "… (FR-020, corrigido pelo fluxo de bugs do front em `front/bugs/clique-repetido-em-excluir/`);".
    A frase "Bloqueiam a entrega desta feature…" continua como estava.
- Ficaram como estavam: os Esclarecimentos (002 e 003), que são histórico, e as notas de outros desvios (diálogo
  aberto até a resposta, link da quantidade, mês do calendário, busca por nome etc.).

## Desvios da avaliação

- Os testes de guarda "should leave the other rows as they are" das duas tabelas não passam antes da correção: eles
  precisam passar o input `deleting`, que não existia (`NG0303`). Protegem as outras linhas contra a correção, mas não
  puderam ser rodados contra o código antigo. Os guardas das páginas passam antes e depois.
- A barra de restauração ganhou `margin-bottom: calc(-1 * var(--mat-progress-bar-track-height, 4px))` (a altura da
  trilha do `mat-progress-bar`), além do `position: sticky; top: 0` e do `z-index` previstos, para não empurrar a tabela
  enquanto aparece, como pede o passo 3.
- Fora isso, nenhum: a correção seguiu a opção preferida, com os arquivos e os testes previstos.

## Pendências

- O tamanho e o alinhamento do spinner no `matIconButton` e a barra sobre as linhas não foram vistos num navegador;
  conferir no `/speckit-bug-test` ou à mão.
- O "Recarregar" do cabeçalho ainda chama o `reload()` direto e é ignorado se a lista já carrega (comportamento de
  antes, fora do escopo).
- `plan.md`, `research.md`, `tasks.md` e checklists das 002 e 003 podem ainda citar o desvio como pendente; a
  reavaliar no `/speckit-converge` (critério de conclusão da avaliação: FR-012, FR-013 e SC-004 da 002; FR-020,
  FR-021 e SC-004 da 003).
- Próximo passo: `/speckit-bug-test project=front slug=clique-repetido-em-excluir`.
