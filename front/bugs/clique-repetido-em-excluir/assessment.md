# Avaliação do bug: o "Excluir" aceita clique repetido e nada mostra a exclusão ou a restauração em andamento

- **Slug**: clique-repetido-em-excluir
- **Projeto**: front
- **Criada em**: 2026-10-07
- **Origem**: texto colado (relato do orquestrador, a partir do `/speckit-converge` das features 002 e 003)
- **Veredito**: válido
- **Severidade**: medium

## Relato (resumido)

> Nas páginas de categorias e de transações, o `delete()` (`categories-page.ts`, `transactions-page.ts`) não guarda
> estado de exclusão em andamento, e o botão "Excluir" só depende de `canDelete` (categorias). Um segundo clique antes
> da resposta manda outro DELETE. Nada indica que a exclusão ou a restauração ("Desfazer") está em andamento. Conferir
> também o "Desfazer": clique repetido e restauração em andamento.

## Sintoma

Um segundo clique no "Excluir" da mesma linha, antes de a linha sair da lista, envia outro `DELETE`. A API o recusa
("Categoria não encontrada." / "Transação não encontrada."), e essa mensagem de erro toma o lugar da que oferece
"Desfazer": a exclusão feita não pode mais ser desfeita. Durante a exclusão e durante a restauração pelo "Desfazer", a
página não mostra nada. O esperado é que o segundo clique não envie nada e que a página mostre a exclusão e a
restauração em andamento.

## Requisitos violados

- `front/specs/002-gestao-de-categorias/spec.md`: FR-012 (428-435) e FR-013 (436-439), Casos-limite (333-337),
  Esclarecimentos (83-91, que pedem a forma da correção a este assess e dizem que ela bloqueia a entrega da 002 e da
  003) e Premissas (526-527). A SC-004 (503-504) depende disso: hoje uma exclusão pode perder o "Desfazer".
- `front/specs/003-gestao-de-transacoes/spec.md`: FR-020 (498-504, "a forma do retorno visual é definida no
  `/speckit-bug-assess` dessa correção"), FR-021 (505-506), Casos-limite (398-403), Esclarecimentos (105-115) e
  Premissas (638-642). A SC-004 (575-576) depende disso, como na 002.
- `AGENTS.md` da raiz, princípio III: "Nada de requisições, consultas ou cálculos repetidos sem necessidade".

### O que as specs exigem, exatamente

| Ponto | 002 (categorias) | 003 (transações) |
| --- | --- | --- |
| Bloquear o segundo pedido | "Durante uma exclusão, um novo clique no 'Excluir' da mesma linha NÃO DEVE enviar outro pedido" (FR-012); "antes de a lista recarregar" (Casos-limite) | "Um novo 'Excluir' na mesma linha antes de a lista recarregar NÃO DEVE enviar outro pedido" (FR-020 e Casos-limite) |
| Mostrar a exclusão em andamento | "a página DEVE mostrar que a exclusão está em andamento" (FR-012) | "a página DEVE mostrar que a exclusão, ou a restauração (FR-021), está em andamento" (FR-020) |
| Mostrar a restauração em andamento | "Durante a restauração, a página DEVE mostrar que ela está em andamento" (FR-013) | idem (FR-020) |
| Por linha ou global | o bloqueio é por linha ("da mesma linha"); o retorno é "a página", sem forma definida | igual |
| Forma | delegada a este assess (Esclarecimentos, exemplo: "desabilitar o 'Excluir' da linha") | delegada a este assess (FR-020) |

Outras linhas continuam livres: excluir outra linha durante uma exclusão é permitido, e a mensagem nova substitui a
anterior (comportamento aceito nos Casos-limite das duas specs: 002, 345-346; 003, 391-393).

**Diferença de prazo (LOW conhecido).** A FR-012 da 002 limita o bloqueio a "durante uma exclusão" (até a resposta
do `DELETE`), enquanto os Casos-limite da própria 002 e a FR-020 e os Casos-limite da 003 dizem "antes de a lista
recarregar". A reprodução mostra que a janela mais danosa é justamente a de depois da resposta: o `DELETE` foi aceito,
a mensagem com "Desfazer" está à vista, a lista ainda recarrega (as linhas continuam visíveis durante a recarga,
Casos-limite 348-350 da 002 e 336 da 003), e um clique nessa hora envia o segundo pedido que apaga o "Desfazer". Um
bloqueio só até a resposta, como diz a FR-012 da 002 ao pé da letra, não corrige o defeito descrito na própria 002.
Proposta: valer nas duas páginas "até a linha sair da lista" (ver P3), e alinhar a redação da FR-012 da 002 no
bug-fix.

A SC-004 também difere na redação ("com uma ação" na 002, "com um único clique" na 003), sem efeito neste bug.

## Reprodução

Reproduzido em 2026-10-07, no `main` (`99c0716`), com um spec temporário (`src/app/zz-repro-temp.spec.ts`, rodado com
`npx ng test --watch=false --include=...` e apagado em seguida; a árvore de trabalho ficou limpa). O spec montava cada
página com `HttpTestingController`, `MatSnackBar` simulado (como os specs das páginas) e clicava no botão "Excluir" da
linha.

| Cenário | Categorias | Transações |
| --- | --- | --- |
| Dois cliques antes da resposta | 2 `DELETE` em voo; botão habilitado; nenhum indicador (`mat-progress-*`) na página | idem |
| Respostas: a 1ª aceita (204), a 2ª recusada (404) | mensagens: "Categoria excluída." com "Desfazer", depois "Categoria não encontrada." com "Fechar" (a segunda substitui a primeira) | "Transação excluída." com "Desfazer", depois "Transação não encontrada." com "Fechar" |
| Um clique depois do 204, com a recarga da lista pendente | 1 `DELETE` novo; botão habilitado; a recusa mostra "Categoria não encontrada." no lugar do "Desfazer" | idem, com "Transação não encontrada." |
| "Desfazer" escolhido, `POST` pendente | nenhum indicador na página e nenhuma mensagem | nenhum indicador |

Passos manuais equivalentes (com a API lenta, por exemplo pela limitação de rede do DevTools):

1. Em `/categories` (numa categoria sem transações) ou em `/transactions`, dar um clique duplo no "Excluir" de uma
   linha.
2. Ver na aba Rede dois `DELETE` para o mesmo id; a mensagem "… excluída." aparece e logo é trocada por "… não
   encontrada.", sem "Desfazer".
3. Repetir com um único clique e escolher "Desfazer": até a lista recarregar, nada na página indica a restauração.

Clique repetido no "Desfazer": **não acontece**. O `MatSnackBarRef.dismissWithAction()` emite o `onAction` uma única
vez, completa o fluxo e fecha a mensagem (`node_modules/@angular/material/fesm2022/snack-bar.mjs:42-50`, Material
21.2.14). O problema do "Desfazer" é só a falta de indicação da restauração em andamento.

### Recarga ignorada com a lista carregando (achado da revisão)

O `ResourceImpl.reload()` não faz nada e devolve `false` quando o estado interno é `loading`, que cobre tanto a
primeira carga ou a troca de filtro quanto uma recarga em curso
(`node_modules/@angular/core/fesm2022/_resource-chunk.mjs:255-260`; o status público só vira `reloading` na linha
370). O `reload()` depois de um 204 pode, então, se perder. Reproduzido no `CategoriesPage` com um segundo spec
temporário (também apagado):

| Cenário | Resultado |
| --- | --- |
| Excluir A, 204, recarga pendente; excluir B, 204 | nenhum `GET` novo para B; a recarga de A, respondida ainda com B, deixa B na lista |
| Trocar o filtro (`GET` pendente) com o `DELETE` de B em voo; 204 | nenhum `GET` novo; a lista do filtro, respondida ainda com B, deixa B na lista |

Hoje o efeito é uma linha desatualizada até o próximo "Recarregar". Com a correção preferida (id mantido em `deleting`
no sucesso), a linha de B ficaria **presa com o spinner e desabilitada** por tempo indeterminado. Isso entra na
correção (passo 4 e P5). O mesmo `reload()` perdido vale para o `save()` (criação, edição e restauração) durante uma
carga.

Validação da opção (a) da P5, no mesmo spec temporário, com um `httpResource` isolado: com o status `loading`,
`reload()` devolveu `false` e marcou uma recarga pendente; quando a carga terminou, um `effect` sobre o `status()`
chamou o `reload()`, saiu exatamente um `GET` novo, e o valor final foi o da resposta nova.

## Caminhos de código suspeitos

- `front/src/app/categories/pages/categories-page/categories-page.ts:74-86`: `delete()` assina o
  `categoriesService.delete(id)` sem guardar estado; cada clique é um pedido novo. Na resposta, `reload()` (77) e a
  mensagem com "Desfazer" (79-82). Na recusa, `showError` (84) abre a mensagem de erro, que substitui a do "Desfazer".
- `categories-page.ts:82` e `88-96`: o "Desfazer" chama `save(create(...))`, que não marca nada como em andamento.
- `front/src/app/categories/pages/categories-page/categories-page.html:31`: `(delete)="delete($event)"`; a tabela não
  recebe nenhum estado de exclusão.
- `front/src/app/categories/components/categories-table/categories-table.html:33-44`: o botão "Excluir" depende só de
  `canDelete` (`[disabled]`, 39) e usa `disabledInteractive` (40), por isso o clique tem a guarda
  `category.canDelete && delete.emit(category)` (41). `categories-table.ts:32`: `delete = output<Category>()`, sem
  input de estado.
- `front/src/app/transactions/pages/transactions-page/transactions-page.ts:101-115`: o mesmo `delete()`, com
  `reload()` (104), "Desfazer" (106-111) e `showError` (113). O "Desfazer" passa por `save()` (117-125).
- `front/src/app/transactions/pages/transactions-page/transactions-page.html:35`: `(delete)="delete($event)"`.
- `front/src/app/transactions/components/transactions-table/transactions-table.html:38`: o botão "Excluir" não tem
  `[disabled]` nenhum; `transactions-table.ts:34`: `delete = output<Transaction>()`.

## Hipótese de causa raiz

Confiança: **alta** (reproduzida nas duas páginas).

As duas páginas tratam a exclusão como uma chamada solta: `delete()` assina o pedido e só reage à resposta, sem
registrar que aquela linha está sendo excluída, e as tabelas não têm como saber disso. O botão continua habilitado do
clique até a linha sair da lista, ou seja, durante o `DELETE` e durante a recarga que vem depois dele. Com o mesmo id,
o segundo pedido é sempre inútil, e a recusa dele abre uma mensagem que, como só uma mensagem aparece por vez, apaga a
que oferecia "Desfazer". A restauração passa pelo mesmo `save()` da criação e da edição, que também não guarda estado,
e a mensagem do "Desfazer" já fechou ao ser escolhida, então nada fica à vista até a recarga.

Uma causa secundária, que a correção não pode ignorar: a recarga depois da exclusão (e do salvamento) chama
`reload()` sem conferir o retorno, e um `reload()` com a lista já carregando é descartado pelo Angular. A lista que
chega pode, então, ainda conter a linha excluída.

**É bug, e não comportamento novo**: as specs 002 e 003 já exigem o bloqueio e o retorno visual e registram o atual
como desvio conhecido, encaminhado ao fluxo de bugs.

## Correção proposta

**Preferida: estado por linha para a exclusão e um indicador na lista para a restauração, em cada página.**

1. **Exclusão, por linha.** Cada página guarda os ids em exclusão num signal,
   `protected readonly deleting = signal<ReadonlySet<string>>(new Set())`:
   - em `delete()`, ignorar o pedido se o id já está no conjunto; senão, acrescentá-lo antes de assinar o `DELETE`;
   - na recusa (`error`), tirar o id do conjunto, porque a lista não recarrega (FR-014 da 002, FR-022 da 003) e a linha
     continua lá, de novo disponível;
   - no sucesso, manter o id: a linha sai da lista na recarga, e os ids são UUIDs que não voltam (a restauração cria
     um registro com id novo). Assim o bloqueio vale "até a linha sair da lista", inclusive durante a recarga; a
     recarga do passo 4 garante que a lista que chega depois do 204 já não traz a linha. O conjunto só cresce com as exclusões aceitas da visita à página, o que é
     desprezível.
   - A guarda na página (e não só no botão) garante o princípio III mesmo que o clique chegue por outro caminho.
2. **Tabelas.** `CategoriesTable` e `TransactionsTable` ganham `readonly deleting = input<ReadonlySet<string>>(new
   Set())`, e a página passa `[deleting]="deleting()"`. No botão "Excluir" da linha em exclusão:
   - desabilitado (`[disabled]="... || deleting().has(category.id)"`), com `disabledInteractive` também na tabela de
     transações, para o foco e a dica continuarem, como já é feito na de categorias; a guarda do clique passa a incluir
     `!deleting().has(id)`;
   - o ícone trocado por um `mat-progress-spinner` pequeno (`diameter="20"`, modo indeterminado), o mesmo componente
     que as tabelas já usam no estado de carregamento (`categories-table.html:53-54`, `transactions-table.html:50`).
     O nome do botão continua "Excluir".
3. **Restauração, global.** Não há linha a marcar (a linha já saiu). Cada página guarda
   `protected readonly restoring = signal(0)`, um contador (duas restaurações podem se sobrepor: desfazer A, excluir B
   e desfazer B antes de A responder), incrementado ao escolher "Desfazer" e decrementado num `finalize` do pedido de
   criação. Enquanto for maior que zero, um `mat-progress-bar` indeterminado aparece no topo do cartão da lista
   (`categories-page.html:30-32`, `transactions-page.html:30-37`, cartão `.table`), com `aria-label` "Restaurando a
   categoria"/"Restaurando a transação". O `save()` continua o mesmo; só o pedido do "Desfazer" é envolvido. O cartão
   `.table` rola (`overflow: auto`, `categories-page.scss:16` e `transactions-page.scss:16`), então a barra fica
   fixa no topo da área que rola, com `position: sticky; top: 0` (e `z-index` acima das linhas), para não rolar junto
   com a lista nem empurrar a tabela.
4. **Recarga que não se perde** (P5, opção (a)). Toda recarga da lista na página (depois de excluir, de restaurar e
   de salvar) passa por uma função que chama `reload()` e, se ele devolver `false` (lista já carregando), marca uma
   recarga pendente; um `effect` sobre o `status()` do resource chama `reload()` de novo quando a carga atual termina.
   Assim a lista exibida sempre reflete a exclusão aceita, e a linha em `deleting` sempre sai (validado no spec
   temporário; ver Reprodução). Como a lógica é a mesma nas duas páginas e não é trivial, ela vai para `shared/`
   (F1), por exemplo `shared/reload-when-idle/reload-when-idle.ts`, com o seu spec: uma função que recebe o resource
   e devolve o `reload` seguro, criada no contexto de injeção da página. A recarga pendente só é disparada quando a
   carga em curso termina com sucesso (status `resolved`). Se ela terminar em erro, a marca é descartada e o caminho
   continua sendo o "Recarregar" do usuário, como hoje. A marca é limpa **antes** de chamar o `reload()`, para não
   haver laço: o `reload()` disparado não marca nada de novo, e nenhum `GET` extra sai depois dele.

O estado `deleting` e o contador `restoring` ficam em cada página: são um signal e três linhas por página, e a
marcação do botão difere nas duas tabelas (dica e `canDelete` só nas categorias). Um utilitário compartilhado para isso
seria uma abstração para duas linhas (princípio V); ver P2. Só a recarga do passo 4 vai para `shared/`.

**Alternativas:**

- (B) Mensagem em vez de indicador: "Excluindo a categoria…" e "Restaurando a categoria…" num snack bar sem duração,
  trocado pela mensagem do resultado. Vantagens: zero layout, anunciada aos leitores de tela. Desvantagens: pisca em
  respostas rápidas; a mensagem de exclusão em andamento tomaria o lugar do "Desfazer" de uma exclusão anterior mais
  cedo do que hoje; e muda as chamadas ao `MatSnackBar` que os specs existentes verificam com
  `toHaveBeenCalledExactlyOnceWith` (`categories-page.spec.ts`, "when undoing fails"), o que obrigaria a alterar testes
  existentes.
- (C) Só o bloqueio até a resposta do `DELETE` (FR-012 da 002 ao pé da letra). **Descartada**: a reprodução mostra que
  o clique depois do 204, com a recarga pendente, ainda apaga o "Desfazer".

**Arquivos que devem mudar (preferida):**

- `front/src/app/categories/pages/categories-page/categories-page.ts`, `.html` e `.spec.ts`;
- `front/src/app/categories/components/categories-table/categories-table.ts`, `.html` e `.spec.ts`;
- `front/src/app/transactions/pages/transactions-page/transactions-page.ts`, `.html` e `.spec.ts`;
- `front/src/app/transactions/components/transactions-table/transactions-table.ts`, `.html` e `.spec.ts`;
- novo: `front/src/app/shared/reload-when-idle/reload-when-idle.ts` e `.spec.ts` (passo 4, se a P5 ficar com (a));
- os `.scss` das páginas, para o `position: sticky` do `mat-progress-bar` (medidas mínimas);
- se a P6 ficar com (A), as tabelas também desabilitam o "Editar" (já listadas acima);
- as specs 002 e 003 (notas de desvio; ver abaixo e P4).

Nenhuma mudança no back, nos services, nos tipos, nas rotas nem no contrato da API; nenhuma dependência nova (o
`MatProgressBarModule` e o `MatProgressSpinnerModule` são do Angular Material, já instalado; o primeiro entra só no
chunk sob demanda das páginas).

**Testes de regressão previstos** (escritos antes da correção). Os marcados **reprodução** falham hoje; os marcados
**guarda** já passam hoje e protegem o comportamento atual contra a correção (não contam como reprodução do bug):

1. `categories-page.spec.ts`, em `deleting`:
   - **reprodução** "should not send another DELETE when Excluir is clicked again before the response": dois cliques em
     `rowButton(1, 'Excluir')`; `httpTesting.match` do `DELETE` tem 1 pedido (hoje 2);
   - **reprodução** "should not send another DELETE after the deletion is accepted and before the list reloads":
     responder 204, fazer `TestBed.tick()` e pegar a recarga com `expectList()` **sem** respondê-la; clicar de novo;
     `expectNone` de `DELETE`; `snackBar.open` chamado uma única vez, com "Desfazer"; só então `flushList`;
   - **reprodução** "should disable Excluir and show a spinner in the row while it is being deleted": com o `DELETE`
     pendente, `fixture.detectChanges()`; o botão da linha está desabilitado e tem um `mat-progress-spinner`;
   - **guarda** (mesmo teste ou à parte): o "Excluir" da outra linha continua habilitado e sem spinner;
   - **guarda** "should enable Excluir again when the deletion is refused": recusar (400) e conferir o botão
     habilitado, sem spinner, e um novo clique enviando um `DELETE`;
   - **reprodução** "should show that the restoration is in progress": depois da exclusão e da recarga,
     `snackBarAction.next()`; com o `POST` pendente, `fixture.detectChanges()` e conferir o `mat-progress-bar` no
     cartão da lista;
   - **guarda**: responder o `POST` (aceito e recusado) e conferir que a barra some;
   - **reprodução** "should reload again when a deletion is accepted while the list is loading" (achado da revisão):
     excluir A, 204, `TestBed.tick()` e pegar a recarga **sem** respondê-la; excluir B, 204; responder a recarga pendente
     ainda com B; `TestBed.tick()` e esperar um novo `GET` (hoje nenhum); responder sem B e conferir que B saiu da lista;
     com a correção sem o passo 4, o botão de B ficaria desabilitado com o spinner, então conferir também que, antes da
     nova lista, o "Excluir" de B continua desabilitado e que nenhum `DELETE` novo sai. Uma variante com a troca de
     filtro (`filters()!.filters.set(...)` com o `DELETE` em voo) cobre o caso da primeira carga;
   - se a P6 ficar com (A), **reprodução** "should disable Editar while the row is being deleted": com o `DELETE`
     pendente e depois do 204 com a recarga pendente, o "Editar" da linha está desabilitado e não abre o diálogo.
2. `transactions-page.spec.ts`, em `deleting`: os mesmos, com `rowButton(0, 'Excluir')`, `feira` e `pagamento`.
3. `categories-table.spec.ts` e `transactions-table.spec.ts`: **reprodução**: com `deleting` contendo o id de uma
   linha, o botão dela fica desabilitado, mostra o spinner e não emite `delete` ao clicar; **guarda**: as outras linhas
   não mudam e, na de categorias, a dica de `canDelete` continua igual.
4. `shared/reload-when-idle/reload-when-idle.spec.ts` (se a P5 ficar com (a)): com um `httpResource` de teste, um
   `reload` durante a carga não perde a recarga (sai exatamente um `GET` depois da resposta); fora da carga, recarrega na
   hora; vários `reload` durante a mesma carga geram um único `GET` novo; depois desse `GET` respondido, nenhum outro
   sai (`expectNone`, sem laço); e, se a carga em curso terminar em erro, a marca é descartada: nenhum `GET` novo sai, e
   um `reload` posterior volta a recarregar na hora.

Cuidado ao escrever os testes (skill `write-front-tests`): nos specs das páginas, nada de harness nem de
`await fixture.whenStable()` com um pedido pendente (o da lista, o `DELETE` ou o `POST`); para ver os estados em
andamento, usar `fixture.detectChanges()` (e `TestBed.tick()` antes de `expectOne`/`match`), responder todos os
pedidos no fim e só então esperar, para o `httpTesting.verify()` passar. O helper `spinner()` dos specs das páginas
procura qualquer `mat-progress-spinner`; os testes de carregamento que o usam não têm exclusão pendente, então não são
afetados, mas os novos devem procurar o spinner dentro da linha.

Verificação final: `npx ng test --watch=false`, `npm run lint` e `npm run build` em `front/`. Critério de conclusão
(Esclarecimentos da 003, 110-115): o bug-fix só conta como concluído quando `front/bugs/clique-repetido-em-excluir/test.md`
registra o resultado `verified`, os testes de reprodução e a suíte completa passam, e o `/speckit-converge` reavalia
contra o código a FR-012, a FR-013 e a SC-004 da 002 e a FR-020, a FR-021 e a SC-004 da 003.

## Riscos e considerações

- **Conjunto que não é limpo no sucesso**: se um dia a API devolvesse o mesmo id (por exemplo, um "desfazer" real que
  restaure o registro), a linha restaurada nasceria bloqueada. Hoje não acontece (a restauração cria um id novo, FR-013
  da 002 e FR-021 da 003). Se mudar, basta tirar o id do conjunto ao restaurar.
- **Recarga que falha depois do 204**: a lista fica vazia com a mensagem de erro de carregamento; o id continua no
  conjunto, e no "Recarregar" a linha já não vem da API. Sem efeito.
- **Acessibilidade**: o botão desabilitado com `disabledInteractive` continua focável e se anuncia indisponível; o
  spinner e a barra não são anunciados como mudança de estado, o que segue o comportamento atual aceito (anúncio dos
  estados fica para a feature de acessibilidade, Premissas das duas specs). O foco ainda se perde quando a linha sai
  (desvio já aceito, 002 Casos-limite 379-380).
- **Layout**: o spinner precisa caber no `matIconButton` (40 px) sem mudar a altura da linha; `diameter="20"`. O
  `mat-progress-bar` fica `position: sticky; top: 0` dentro do cartão `.table`, que rola (`overflow: auto`), para não
  rolar junto com a lista; fica nos budgets de estilo por componente.
- **Recarga ignorada** (ver Reprodução): sem o passo 4, a correção troca uma linha desatualizada por uma linha presa
  com o spinner. Com o passo 4, a exclusão pode custar um `GET` a mais quando a lista já carregava, o que é a consulta
  necessária para mostrar a lista certa (princípio III), e não repetição. O passo 4 muda também a recarga depois de
  salvar, que tinha o mesmo defeito latente; nenhum teste existente depende do `reload()` perdido.
- **"Editar" numa linha em exclusão** (P6): depois do 204, com a recarga pendente, "Editar" ainda abre o diálogo, e
  salvar gera um `PUT` que a API recusa (404); a mensagem de erro apaga o "Desfazer". As specs não exigem bloquear
  isso.
- **Bundle**: `MatProgressBarModule` só nos chunks sob demanda das páginas; efeito desprezível no inicial.
- **Relação com outros bugs**: o diálogo aberto até a resposta (FR-010 da 002, FR-016 da 003) também mexe no `save()`
  e no `openForm()` das mesmas páginas. Os dois bug-fixes tocam os mesmos arquivos; sem dependência entre eles, só
  possível conflito de merge se correrem em paralelo. A forma do "em andamento" escolhida aqui (spinner/barra do
  Material) pode servir de padrão para o "Salvar" em andamento daquele bug.
- **Escopo**: só o front. Nenhuma mudança no back, no contrato da API, em dados ou em segurança.
- **Specs** (notas "desvio conhecido" que saem junto com a correção):
  - 002: a nota de desvio da FR-012 (432-435, "Desvio conhecido: hoje nada indica…" até "(Esclarecimentos,
    2026-10-06)") e a da FR-013 (439, "Desvio conhecido: hoje nada indica a restauração em andamento (ver FR-012)");
    a dos Casos-limite (334-337, de "Desvio conhecido" até "não pode mais ser desfeita"); e a lista das Premissas
    (526-527) passa a dizer que o "Excluir" protegido foi corrigido em `front/bugs/clique-repetido-em-excluir/`. Se a
    P3 for aceita, a FR-012 troca "Durante uma exclusão" por "Do clique até a categoria sair da lista ou até a recusa
    da exclusão", sem mudar o resto.
  - 003: a nota de desvio da FR-020 (500-504, de "Desvio conhecido" até "dessa correção"), substituída por uma
    referência a `front/bugs/clique-repetido-em-excluir/` para a forma do retorno visual; a dos Casos-limite (399-403);
    e a frase das Premissas "Também ficam fora o clique repetido em 'Excluir' sem retorno visual (FR-020)" (638-639)
    passa a registrar o desvio como corrigido, como foi feito para a FR-031. A frase "Bloqueiam a entrega desta
    feature…" (641-642) **continua**: ela é a regra de quais desvios bloqueiam e de quando um bug-fix conta como
    concluído, e não uma nota de desvio.
  - Os Esclarecimentos (002: 83-91; 003: 105-115) são histórico e ficam.

## Decisões do usuário (portão do diagnóstico, 2026-10-07)

A segunda rodada da revisão independente não deixou nada CRITICAL ou HIGH; os dois LOW restantes estão incorporados
(passo 4 e P5, recarga só depois de sucesso; passo 1, referência ao passo 4). O usuário aprovou o diagnóstico e
escolheu:

- **P1**: (A), spinner no "Excluir" da linha durante a exclusão e `mat-progress-bar` no cartão durante a restauração.
- **P2**: (A), o estado `deleting`/`restoring` em cada página; só a recarga do passo 4 vai para `shared/`.
- **P3**: (A), bloqueio do clique até a linha sair da lista ou até a recusa, com a FR-012 da 002 alinhada.
- **P4**: (A), as specs 002 e 003 são ajustadas no próprio `/speckit-bug-fix`, com registro no `fix.md`.
- **P5**: (a), recarregar quando a carga em curso terminar, **só depois de sucesso** (`resolved`); num erro, a marca é
  descartada.
- **P6**: (A), o "Editar" da linha também fica desabilitado enquanto ela estiver em `deleting`.

## Perguntas (respondidas)

- **P1 — Forma do retorno visual**: [NEEDS CLARIFICATION: como mostrar a exclusão e a restauração em andamento?]
  - (A) Exclusão por linha: "Excluir" desabilitado com um spinner no lugar do ícone até a linha sair; restauração:
    `mat-progress-bar` indeterminado no topo do cartão da lista enquanto o `POST` do "Desfazer" está pendente.
    Consequências: o retorno fica onde o usuário clicou; nenhum teste existente muda; um módulo do Material a mais no
    chunk das páginas.
  - (B) Snack bars "Excluindo a categoria…"/"Restaurando a categoria…" (e das transações), sem duração, trocados pelo
    resultado, mais o "Excluir" da linha desabilitado. Consequências: anunciados aos leitores de tela; piscam em
    respostas rápidas; apagam antes o "Desfazer" de uma exclusão anterior; exigem alterar asserções de specs
    existentes.
  - (C) Só a barra no topo do cartão, para exclusão e restauração, com o botão da linha só desabilitado (sem spinner).
    Consequências: um indicador só, mas que não diz qual linha está sendo excluída.
  - **Recomendação**: (A). Usa componentes do Material que as tabelas já usam, mostra o estado no lugar do clique e não
    mexe nas mensagens que as specs fixam.
- **P2 — `shared/` ou em cada página**: [NEEDS CLARIFICATION: extrair o estado de "ids em andamento" para `shared/`?]
  - (A) Em cada página: um `signal<ReadonlySet<string>>` e um contador, cerca de dez linhas por página; o input
    `deleting` em cada tabela.
  - (B) Um utilitário em `shared/` (por exemplo, `shared/pending-ids/`) com `add`/`delete`/`has` sobre um signal, usado
    pelas duas páginas. Consequências: um arquivo e um spec novos para lógica de três linhas.
  - **Recomendação**: (A), pelo princípio V (sem abstração para uma necessidade tão pequena); a F1 só pede `shared/`
    para o que serve a mais de uma feature e vale a pena compartilhar.
- **P3 — Prazo do bloqueio** (LOW conhecido): [NEEDS CLARIFICATION: até a resposta do `DELETE` ou até a linha sair da
  lista?]
  - (A) Até a linha sair da lista (no sucesso) ou até a recusa chegar (na recusa), nas duas páginas, e a FR-012 da 002
    é alinhada à FR-020 da 003 e aos Casos-limite das duas no bug-fix, com a redação "Do clique até a categoria sair
    da lista ou até a recusa da exclusão, um novo clique no 'Excluir' da mesma linha NÃO DEVE enviar outro pedido". As
    Premissas da 003 mantêm a frase "Bloqueiam a entrega…" (641-642). Consequências: cobre a janela da recarga, que a
    reprodução mostrou ser a que apaga o "Desfazer"; depende da P5 para a linha sempre sair.
  - (B) Só até a resposta do `DELETE`, como diz a FR-012 da 002. Consequências: mais curto, mas um clique durante a
    recarga continua enviando um pedido inútil e apagando o "Desfazer"; contraria os Casos-limite da 002 e a FR-020 da
    003.
  - **Recomendação**: (A).
- **P4 — Specs 002 e 003**: [NEEDS CLARIFICATION: ajustar as specs no próprio bug-fix ou numa revisão à parte?]
  - (A) No `/speckit-bug-fix`, com registro no `fix.md`: saem as notas de desvio listadas em "Riscos e considerações";
    a FR-020 da 003 aponta para este bug quanto à forma do retorno; a FR-012 da 002 ganha o prazo da P3; as Premissas
    registram o desvio como corrigido. Os requisitos em si não mudam além do prazo.
  - (B) Deixar as specs e só citá-las no `fix.md`; elas continuam descrevendo um desvio que não existe mais.
  - **Recomendação**: (A), como nos bugs `consulta-repetida-sem-mudanca` e `calendarios-em-ingles`.
- **P5 — Recarga ignorada com a lista carregando** (achado HIGH da revisão): [NEEDS CLARIFICATION: como garantir que
  a linha excluída saia da lista quando o `reload()` é descartado?]
  - (a) Se o `reload()` devolver `false`, recarregar quando a carga atual terminar (passo 4, em `shared/`, aplicado a
    toda recarga da lista nas duas páginas: exclusão, restauração e salvamento). Consequências: a lista exibida sempre
    reflete o que a API aceitou, o mais fiel à FR-013 da 002 e à FR-020 da 003 ("recarregar a lista"); resolve também
    a linha desatualizada de hoje; custa um `GET` a mais só quando a lista já carregava; um arquivo e um spec novos em
    `shared/`. Validada num spec temporário. A recarga pendente só sai quando a carga em curso termina com sucesso
    (`resolved`); num erro, a marca é descartada e vale o "Recarregar" do usuário. A marca é limpa antes do
    `reload()`, sem laço.
  - (b) Tirar o id de `deleting` na primeira lista que chegar depois do 204. Consequências: a linha nunca fica presa,
    mas pode voltar desatualizada e habilitada; um novo clique nela envia um `DELETE` inútil, recusado, que apaga o
    "Desfazer", ou seja, o próprio bug, numa janela menor.
  - (c) Aceitar e documentar como desvio. Consequências: com o id mantido em `deleting`, a linha fica presa com o
    spinner até o próximo "Recarregar"; contraria a FR-013/FR-020.
  - **Recomendação**: (a). É a única que mantém a lista certa e o bloqueio sem buraco; o custo é uma consulta que é
    necessária, e não repetida.
- **P6 — "Editar" numa linha em exclusão** (achado LOW da revisão): [NEEDS CLARIFICATION: desabilitar também o
  "Editar" da linha enquanto ela estiver em `deleting`?]
  - (A) Desabilitar o "Editar" da linha em `deleting` (mesmo input, uma condição no `[disabled]`, com
    `disabledInteractive` e a guarda no clique). Consequências: fecha o mesmo caminho de pedido inútil que apaga o
    "Desfazer" (o `PUT` 404); um teste a mais por página e por tabela; vai além da letra das specs, mas no espírito do
    princípio III e da FR-012/FR-020.
  - (B) Deixar fora do escopo e registrar como observação no `fix.md`. Consequências: mudança menor; o caso continua
    possível, mas exige abrir o diálogo, preencher e salvar dentro da janela da recarga, o que é raro.
  - **Recomendação**: (A). O custo é uma condição no template que já recebe `deleting`, e o efeito (perder o
    "Desfazer") é o mesmo que este bug corrige.
