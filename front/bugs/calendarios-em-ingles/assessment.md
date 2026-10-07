# Avaliação do bug: os calendários mostram e anunciam textos em inglês

- **Slug**: calendarios-em-ingles
- **Projeto**: front
- **Criada em**: 2026-10-07
- **Origem**: texto colado (relato do orquestrador, a partir do `/speckit-converge` das features 002 e 003)
- **Veredito**: válido
- **Severidade**: medium

## Relato (resumido)

> Os calendários (o datepicker do campo "Mês" compartilhado, `shared/month-field`, usado em categorias, transações e
> relatórios, e o datepicker do campo "Data" do formulário de transação) mostram textos em inglês: os nomes acessíveis
> e os rótulos dos botões, como "Previous month" e "Choose month and year". A regra de Idioma exige textos exibidos ao
> usuário em português. Evidência preliminar do converge: nenhum `MatDatepickerIntl` em português é fornecido.
> Conferir também se os nomes de meses e dias e o formato vêm em pt-BR. Fora deste bug: a data digitada lida como
> mês/dia/ano, que será avaliada à parte.

## Sintoma

Os botões do calendário (abrir, navegar, trocar de visão e fechar) se anunciam aos leitores de tela em inglês, e o
botão de fechar, visível quando recebe o foco pelo teclado, mostra "Close calendar". O esperado são rótulos em
português em todos os calendários da aplicação. Os nomes dos meses e dos dias, os nomes acessíveis das células e o
formato das datas já estão em pt-BR.

## Requisitos violados

- `front/specs/002-gestao-de-categorias/spec.md`: FR-020 (linhas 476-478), Casos-limite (340-342) e Esclarecimentos
  (42-46 e 90-91, onde a correção bloqueia a entrega da 002 e da 003).
- `front/specs/003-gestao-de-transacoes/spec.md`: FR-030 (540-544), Casos-limite (383-385) e Esclarecimentos (57-60,
  110-121). O FR-030 diz que "o conjunto completo desses rótulos é definido no bug-fix desta correção"; esta avaliação
  propõe o conjunto (ver P2).
- `front/specs/001-reports-dashboard/spec.md`: não tem requisito equivalente. A FR-009 e a SC-004 tratam só da
  formatação pt-BR de valores; o seletor de mês da página é o mesmo `MonthField`, então vale a regra de Idioma do
  `AGENTS.md` da raiz e a frase "vale para todos os calendários da aplicação" das specs 002 e 003.
- `AGENTS.md` da raiz, seção Idioma: "textos exibidos ao usuário em português".

## Reprodução

Reproduzido em 2026-10-07, no `main` (`42266bd`), com um spec temporário (`src/app/zz-repro-temp.spec.ts`, rodado com
`npx ng test --watch=false --include=...` e apagado em seguida; a árvore de trabalho ficou limpa). O spec criava o
`MonthField` (num host) e o `TransactionForm` com `LOCALE_ID` `pt-BR`, abria o calendário pelo `MatDatepicker.open()`,
passava pelas visões com o botão do período e coletava `aria-label`, texto e descrição de cada botão.

| Onde | Texto encontrado |
| --- | --- |
| Botão que abre o calendário (`mat-datepicker-toggle`), nos dois campos | `aria-label="Open calendar"` |
| Visão de meses (abertura do "Mês"): botão do ano | `aria-label="Choose date"`, texto "2026" |
| Visão de meses: setas | "Previous year", "Next year" |
| Visão de dias (abertura do "Data"; no "Mês", pelo botão do ano): botão do período | `aria-label="Choose month and year"`, texto "SET. DE 2026" |
| Visão de dias: setas | "Previous month", "Next month" |
| Visão de anos (pelo botão do período da visão de dias): botão do período | `aria-label="Choose date"`, texto "2016 – 2039", descrição oculta "2016 to 2039" |
| Visão de anos: setas | "Previous 24 years", "Next 24 years" |
| Botão de fechar (oculto, aparece com o foco do teclado), em todas as visões | texto "Close calendar" |

Já em pt-BR, sem mudança necessária:

| Item | Resultado |
| --- | --- |
| Valor do campo "Mês" | "09/2026" (MM/aaaa) |
| Valor do campo "Data" | "07/10/2026" (dd/mm/aaaa) |
| Meses na visão de meses | "JAN.", "FEV.", …; nome acessível "janeiro de 2026", … |
| Dias na visão de dias | "1", "2", …; nome acessível "1 de setembro de 2026", … |
| Cabeçalho dos dias da semana | "D S T Q Q S S", com os nomes completos ("domingo", "segunda-feira", …) para leitores de tela |

Passos manuais equivalentes, com um leitor de tela ou o inspetor de acessibilidade do navegador:

1. Em `/categories`, `/transactions` (com os filtros à vista) ou `/reports`, focar o botão do calendário do campo
   "Mês": ele se anuncia como "Open calendar".
2. Abrir o calendário: as setas se anunciam como "Previous year"/"Next year" e o botão do ano como "Choose date".
   Navegar com Tab até o fim do calendário: aparece o botão "Close calendar".
3. Em `/transactions`, abrir "Nova transação" e o calendário do campo "Data": "Choose month and year",
   "Previous month" e "Next month"; no botão do período, a visão de anos traz "Previous 24 years", "Next 24 years" e
   a descrição "2016 to 2039".

## Caminhos de código suspeitos

- `node_modules/@angular/material/fesm2022/datepicker.mjs:36-57` (Material 21.2.14): o `MatDatepickerIntl` traz os
  rótulos em inglês (`openCalendarLabel`, `closeCalendarLabel`, `prevMonthLabel`, `nextMonthLabel`, `prevYearLabel`,
  `nextYearLabel`, `prevMultiYearLabel`, `nextMultiYearLabel`, `switchToMonthViewLabel`, `switchToMultiYearViewLabel`,
  `calendarLabel`, `startDateLabel`, `endDateLabel`, `comparisonDateLabel`, `formatYearRange`, `formatYearRangeLabel`).
  Eles são lidos no cabeçalho do calendário (2087-2103), no botão de fechar (2577) e no toggle (template na 3827).
- `datepicker.mjs:4995` e `5009`: o `MatDatepickerModule` declara `providers: [MatDatepickerIntl]`. Isso é decisivo
  para a correção (ver abaixo).
- `front/src/app/app.config.ts:13`: só `LOCALE_ID` `pt-BR`; nenhum `MatDatepickerIntl`. Em todo o `src/` não há
  nenhuma referência a `MatDatepickerIntl`.
- `front/src/app/shared/month-field/month-field.ts:4,14,15-20`: importa o `MatDatepickerModule` e fornece o
  `provideNativeDateAdapter` com o formato MM/aaaa. Usado em `categories-filters.ts:5,12`,
  `transactions-filters.ts:8,15` e `reports-page.ts:7`.
- `front/src/app/transactions/components/transaction-form/transaction-form.ts:5-6,29,35`: importa o
  `MatDatepickerModule` e fornece `provideNativeDateAdapter()`; o calendário está em `transaction-form.html:19-21`.

O que já está certo, e por quê:

- `front/src/main.ts:1,7`: `registerLocaleData(localePt)`.
- `front/src/app/app.config.ts:13`: `LOCALE_ID` `pt-BR`.
- `node_modules/@angular/material/fesm2022/_date-formats-chunk.mjs:4-7`: o `MAT_DATE_LOCALE` usa por padrão
  `inject(LOCALE_ID)`, então o `NativeDateAdapter` dos dois `provideNativeDateAdapter` formata nomes de meses, dias e
  datas com `Intl` em `pt-BR`. Não falta `MAT_DATE_LOCALE` explícito.

## Hipótese de causa raiz

Confiança: **alta** (reproduzida, e a correção foi provada no mesmo spec temporário).

O Angular Material traduz os textos do calendário por um serviço, o `MatDatepickerIntl`, cujo padrão é em inglês, e o
projeto nunca forneceu uma versão em português. O `LOCALE_ID` e o `MAT_DATE_LOCALE` não afetam esse serviço: eles só
mudam o que o `DateAdapter` formata (meses, dias e datas), que por isso já sai em pt-BR.

Há um detalhe que muda a correção esperada. O `MatDatepickerModule` declara o próprio `MatDatepickerIntl` como
provider (`datepicker.mjs:4995`). Como o `MonthField` e o `TransactionForm` são standalone e importam o módulo, esse
provider entra no injetor de ambiente do componente criado (a página da rota, que importa o `MonthField` por meio dos
filtros, e o `TransactionForm`, criado pelo `MatDialog`) e esconde qualquer provider da raiz. No spec temporário:

| Onde o `MatDatepickerIntl` em português foi fornecido | Resultado no `TransactionForm` |
| --- | --- |
| Em lugar nenhum | "Open calendar", "Previous 24 years" |
| Na raiz (equivalente ao `app.config.ts`) | **continua** "Open calendar", "Previous 24 years" |
| Nos `providers` do componente | "Abrir calendário" e o rótulo traduzido |
| Na raiz, com um host que importa só `MatDatepicker`, `MatDatepickerInput` e `MatDatepickerToggle` (sem o módulo) | "Abrir calendário" |

Ou seja, um provider só no `app.config.ts`, como sugeria a evidência preliminar, **não corrige nada** enquanto os
componentes importarem o `MatDatepickerModule`.

**É bug, e não comportamento novo**: as specs 002 e 003 já exigem os rótulos em português e registram o atual como
desvio conhecido, encaminhado ao fluxo de bugs.

## Correção proposta

**Preferida: uma subclasse em português em `shared/` e um provider nos dois componentes que têm calendário.**

1. Criar `front/src/app/shared/datepicker-intl/pt-br-datepicker-intl.ts`: uma classe `@Injectable()` que estende
   `MatDatepickerIntl` e sobrescreve todos os rótulos e o `formatYearRangeLabel`, para que nenhum texto em inglês
   reste. Isso inclui o `calendarLabel`, que o Material 21 não lê em lugar nenhum; o `startDateLabel` e o
   `endDateLabel`, só usados em seletores de intervalo, que o app não tem; e o `comparisonDateLabel`, que hoje já é
   renderizado num `span` oculto `.mat-calendar-body-hidden-label` (`datepicker.mjs` ~162 e 465), mas que nenhum
   `aria-describedby` aponta em modo de data única, então não chega ao leitor de tela. Comentários e identificadores em inglês, textos em
   português. O conjunto proposto está na P2.
2. Em `month-field.ts`, acrescentar `{ provide: MatDatepickerIntl, useClass: PtBrDatepickerIntl }` aos `providers`, ao
   lado do `provideNativeDateAdapter` que já está lá.
3. Em `transaction-form.ts`, o mesmo, ao lado do `provideNativeDateAdapter()`.

O provider no componente fica no injetor do elemento, que vence o do módulo (provado no spec temporário). Segue o
padrão que o projeto já usa para o calendário: o `DateAdapter` também é fornecido em cada componente, e não no
`app.config.ts`. A classe fica em `shared/` porque serve a duas features (F1). Os specs dos componentes passam a
cobrir os rótulos sem precisar de nenhum provider de teste, então um componente de calendário novo que esqueça o
provider não passa despercebido se seguir o mesmo padrão de testes.

**Alternativas:**

- (B) Provider no `app.config.ts` e troca do `MatDatepickerModule` pelas peças standalone (`MatDatepicker`,
  `MatDatepickerInput`, `MatDatepickerToggle`) nos `imports` do `MonthField` e do `TransactionForm`. Vantagem: um
  lugar só, como os outros padrões do Material no `app.config.ts` (`MAT_FORM_FIELD_DEFAULT_OPTIONS`,
  `MAT_TOOLTIP_DEFAULT_OPTIONS`), e vale para calendários futuros. Desvantagens: um `MatDatepickerModule` importado de
  novo em qualquer componente volta a esconder o provider em silêncio; e os specs dos componentes não enxergam o
  `app.config.ts`, então precisam repetir o provider no `TestBed` (o teste verifica a troca dos imports, mas não que o
  provider está no `app.config.ts`, o que pediria um teste à parte da configuração).
- (C) Só o provider no `app.config.ts`. **Descartada**: provado que não muda nada (ver a tabela da causa raiz).

**Arquivos que devem mudar (preferida):**

- novo: `front/src/app/shared/datepicker-intl/pt-br-datepicker-intl.ts` e `pt-br-datepicker-intl.spec.ts`;
- `front/src/app/shared/month-field/month-field.ts` e `month-field.spec.ts`;
- `front/src/app/transactions/components/transaction-form/transaction-form.ts` e `transaction-form.spec.ts`.

Nenhuma mudança no back, nos services, nos tipos, nas rotas nem no contrato da API.

**Testes de regressão previstos** (escritos antes da correção; falham hoje com os rótulos em inglês):

1. `month-field.spec.ts`:
   - "should name the calendar toggle in Portuguese": o botão do `mat-datepicker-toggle` tem `aria-label`
     "Abrir calendário";
   - "should name the calendar controls in Portuguese in every view": abrir pelo `datepicker().open()`; na visão de
     meses, verificar o botão do período ("Escolher data"), as setas ("Ano anterior", "Próximo ano") e o botão de
     fechar ("Fechar calendário"); clicar no botão do período e verificar a visão de dias ("Escolher mês e ano", "Mês
     anterior", "Próximo mês"); clicar de novo e verificar a visão de anos ("24 anos anteriores", "Próximos 24 anos" e
     a descrição "de 2016 a 2039", com a data do host fixa). Fechar o calendário no fim do teste.
2. `transaction-form.spec.ts`: "should name the date calendar controls in Portuguese": o toggle ("Abrir calendário")
   e, com o calendário aberto, a visão de dias ("Escolher mês e ano", "Mês anterior", "Próximo mês", "Fechar
   calendário"). As outras visões já ficam cobertas pelo item 1 e pelo 3.
3. `pt-br-datepicker-intl.spec.ts`: a classe isolada, com todos os rótulos (inclusive os que não chegam ao leitor de tela
   hoje: `calendarLabel`, `startDateLabel`, `endDateLabel` e `comparisonDateLabel`) e o `formatYearRangeLabel`/`formatYearRange`, para que um rótulo novo do Material, numa atualização, apareça
   como lacuna ao revisar o teste.

Cuidado ao escrever os testes (skill `write-front-tests`): o calendário abre num overlay fora do `fixture`, então as
consultas são no `document` (`.mat-calendar-previous-button`, `.mat-calendar-next-button`,
`.mat-calendar-period-button`, `.mat-datepicker-close-button`), e cada teste fecha o calendário no fim. O spec
temporário abriu e fechou o calendário com `open()`/`close()` e `fixture.whenStable()` sem travar (não há
`httpResource` nesses componentes). Os specs existentes que já usam o `MatDatepicker` (`categories-page.spec.ts:277`,
`reports-page.spec.ts:194`, `transactions-page.spec.ts:284`, `month-field.spec.ts`) não dependem dos rótulos e ficam como estão.

Verificação final: `npx ng test --watch=false`, `npm run lint` e `npm run build` em `front/`.

## Riscos e considerações

- **Provider esquecido num calendário futuro** (preferida): quem criar outro componente com calendário precisa do
  provider. Mitigação: os testes dos rótulos no padrão dos itens 1 e 2. A alternativa (B) troca esse risco por outro,
  o do `MatDatepickerModule` reimportado.
- **Atualização do Material**: um rótulo novo no `MatDatepickerIntl` sairia em inglês. O teste da classe (item 3) e a
  leitura do changelog na atualização mitigam.
- **"Mês anterior" e "Próximo mês" repetidos nos relatórios**: a página de relatórios já tem botões com esses nomes
  (`reports-page.html:5,8`). As setas do calendário teriam os mesmos nomes, mas só existem dentro do calendário aberto,
  que é um diálogo com foco preso; não há ambiguidade para o leitor de tela.
- **Bundle**: uma classe pequena num chunk sob demanda (as páginas são lazy); efeito desprezível no inicial.
- **Relação com a data digitada**: o bug da data digitada lida como mês/dia/ano (FR-014 da 003), fora desta avaliação,
  também passa pelo `provideNativeDateAdapter()` de `transaction-form.ts:35`, a mesma linha de `providers` que esta
  correção altera. Os dois bug-fixes mexem no mesmo trecho; não há dependência entre eles, só possível conflito de
  merge se correrem em paralelo.
- **Escopo**: só o front. Nenhuma mudança no back, no contrato da API, em dados ou em segurança; nenhuma dependência
  nova.
- **Specs**: depois da correção, as notas "Desvio conhecido" sobre os calendários em inglês deixam de valer: na 002, a
  FR-020 (476-478) e os Casos-limite (340-342); na 003, a FR-030 (540-544, inclusive a frase de que o conjunto dos
  rótulos é definido no bug-fix) e os Casos-limite (383-385). Os Esclarecimentos são histórico e ficam. As listas de
  desvios nas Premissas (002: 524-528; 003: 632-637) e a de bloqueios (003: 110-112, 643-646) citam o desvio como
  pendente e podem dizer que ele foi corrigido em `front/bugs/calendarios-em-ingles/`, como foi feito para a consulta
  repetida. Ver P3.

## Decisões do usuário (portão do diagnóstico, 2026-10-07)

A revisão independente não achou nada CRITICAL, HIGH ou MEDIUM. O usuário aprovou o diagnóstico e escolheu (A) nas
três perguntas abaixo:

- **P1**: (A), o provider da subclasse nos `providers` do `MonthField` e do `TransactionForm`.
- **P2**: (A), o conjunto completo de rótulos, com o `formatYearRangeLabel` "de 2016 a 2039".
- **P3**: (A), as specs 002 e 003 são ajustadas no próprio `/speckit-bug-fix`, com registro no `fix.md`.

## Perguntas (respondidas)

- **P1 — Onde fornecer os rótulos**: [NEEDS CLARIFICATION: nos componentes ou no `app.config.ts` com troca dos
  imports?]
  - (A) Provider da subclasse nos `providers` do `MonthField` e do `TransactionForm`, ao lado do
    `provideNativeDateAdapter`. Consequências: dois pontos; os specs dos componentes cobrem a correção diretamente; um
    calendário novo precisa lembrar do provider.
  - (B) Provider no `app.config.ts` e troca do `MatDatepickerModule` pelas peças standalone nos dois componentes.
    Consequências: um ponto para o provider, mas dois imports alterados; reimportar o módulo desfaz a correção em
    silêncio; os specs precisam repetir o provider.
  - **Recomendação**: (A). Segue o padrão já usado para o `DateAdapter`, é a menor mudança e não depende de uma regra
    implícita sobre qual import usar.
- **P2 — Conjunto dos rótulos** (o FR-030 da 003 diz que ele é definido no bug-fix): [NEEDS CLARIFICATION: aprovar o
  conjunto abaixo ou ajustar?]
  - (A) Proposto: `calendarLabel` "Calendário"; `openCalendarLabel` "Abrir calendário"; `closeCalendarLabel` "Fechar
    calendário"; `prevMonthLabel` "Mês anterior"; `nextMonthLabel` "Próximo mês"; `prevYearLabel` "Ano anterior";
    `nextYearLabel` "Próximo ano"; `prevMultiYearLabel` "24 anos anteriores"; `nextMultiYearLabel` "Próximos 24 anos";
    `switchToMonthViewLabel` "Escolher data"; `switchToMultiYearViewLabel` "Escolher mês e ano"; `startDateLabel`
    "Data inicial"; `endDateLabel` "Data final"; `comparisonDateLabel` "Período de comparação";
    `formatYearRangeLabel` "de 2016 a 2039"; `formatYearRange` "2016 – 2039" (como está, sem texto).
  - (B) Traduzir só os rótulos que aparecem hoje (sem os de intervalo e o `calendarLabel`). Menos linhas, mas deixa
    textos em inglês para um seletor de período futuro.
  - **Recomendação**: (A), completo e literal, para não restar nenhum texto em inglês no serviço.
- **P3 — Specs 002 e 003**: [NEEDS CLARIFICATION: ajustar as specs no próprio bug-fix ou numa revisão à parte?]
  - (A) No `/speckit-bug-fix`, com registro no `fix.md`: saem as notas "Desvio conhecido" da FR-020 e dos
    Casos-limite da 002, e da FR-030 e dos Casos-limite da 003; a frase do FR-030 sobre o conjunto passa a apontar
    para `front/bugs/calendarios-em-ingles/`; as listas das Premissas e de bloqueios registram o desvio como
    corrigido. Os requisitos em si não mudam: a frase "e vale para todos os calendários da aplicação" continua na FR-020 da 002 e na FR-030 da 003 quando a nota de desvio sair.
  - (B) Deixar as specs como estão e só citá-las no `fix.md`; elas continuam descrevendo um desvio que não existe mais.
  - **Recomendação**: (A), como foi feito no bug `consulta-repetida-sem-mudanca` e como as próprias specs pedem.
