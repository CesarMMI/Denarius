# Avaliação do bug: a data digitada no formulário de transação é lida como mês/dia/ano

- **Slug**: data-digitada-como-mes-dia-ano
- **Projeto**: front
- **Criada em**: 2026-10-07
- **Origem**: texto colado (relato do orquestrador, a partir do `/speckit-converge` da feature 003)
- **Veredito**: válido
- **Severidade**: high

## Relato (resumido)

> No formulário de transação (`transaction-form.ts`), uma data digitada no campo "Data" é interpretada como
> mês/dia/ano: "05/10/2026" vira 10 de maio, em vez de 5 de outubro. Evidência preliminar do converge: o
> `provideNativeDateAdapter` usa `Date.parse`, que não respeita o `MAT_DATE_LOCALE` pt-BR. Conferir também as datas
> inválidas ("31/02", "13/13/2026"), o ano com dois algarismos, o que acontece ao sair do campo, se o campo "Mês"
> compartilhado (`shared/month-field`) aceita digitação e sofre o mesmo problema, e se a exibição já está em pt-BR.
> Fora deste bug: os valores com 14 ou mais algarismos (`parseFloat`), avaliados à parte.

## Sintoma

O campo "Data" exibe dd/mm/aaaa, mas lê o texto digitado com as regras do `Date.parse` do navegador (formato
americano mês/dia/ano e vários outros formatos soltos). A transação é salva, sem aviso, num dia diferente do digitado,
e datas válidas em dia/mês/ano com o dia acima de 12 são recusadas. O esperado (FR-014 da 003) é ler só dia/mês/ano,
com o ano em quatro algarismos e com ou sem zeros à esquerda, e recusar com "Informe uma data válida" qualquer outro
formato e as datas inexistentes.

## Requisitos violados

- `front/specs/003-gestao-de-transacoes/spec.md`: FR-014 (linhas 456-465), cenário 9 da História 2 (218-221),
  Casos-limite (367-374) e Esclarecimentos (26-30 e 96-100; o segundo classifica o desvio como violação do princípio II
  que bloqueia a entrega da 003; ver também 110-112 e 635-638).
- `AGENTS.md` da raiz, princípio II (Segurança): o dado financeiro gravado difere do digitado, sem aviso.

## Reprodução

Reproduzido em 2026-10-07, no `main` (`2323400`), com um spec temporário (`src/app/zz-repro-temp.spec.ts`, rodado com
`npx ng test --watch=false --include=...` e apagado em seguida; a árvore de trabalho ficou limpa). O spec criava o
`TransactionForm` com `LOCALE_ID` `pt-BR`, preenchia o valor "12,50", digitava o texto no campo "Data" (evento
`input`), registrava o texto do campo antes e depois do `blur` e então enviava o formulário. Fuso da máquina:
America/Sao_Paulo (UTC-3).

| Digitado | Campo depois de sair dele | Salvo (`date` enviado) | Esperado (FR-014) |
| --- | --- | --- | --- |
| `05/10/2026` | `10/05/2026` | `2026-05-10` | 5 de outubro (`2026-10-05`) |
| `5/9/2026` | `09/05/2026` | `2026-05-09` | 5 de setembro (`2026-09-05`) |
| ` 05/10/2026 ` (espaços nas pontas) | `10/05/2026` | `2026-05-10` | 5 de outubro |
| `24/09/2026` | `24/09/2026` (sem mudança) | recusada: "Informe uma data válida" | 24 de setembro |
| `02/30/2026` | `02/03/2026` | `2026-03-02` (o 30/02 "transborda") | recusada |
| `05/10/26` (ano com 2 algarismos) | `10/05/2026` | `2026-05-10` | recusada |
| `05/10/0026` (ano com zeros à esquerda) | `10/05/2026` | `2026-05-10` | recusada (decisão P2) |
| `5/10` (sem ano) | `10/05/2001` | `2001-05-10` | recusada |
| `05-10-2026` | `10/05/2026` | `2026-05-10` | recusada |
| `Oct 5 2026` | `05/10/2026` | `2026-10-05` | recusada |
| `2026-09-24` | `23/09/2026` | `2026-09-23` (meia-noite UTC = 23/09 21h no Brasil) | recusada |
| `31/02/2026` | `31/02/2026` | recusada: "Informe uma data válida" | recusada (já certo) |
| `13/13/2026` | `13/13/2026` | recusada: "Informe uma data válida" | recusada (já certo) |
| `00/10/2026` | `00/10/2026` | recusada: "Informe uma data válida" | recusada (já certo) |
| `05/10/2026x` | `05/10/2026x` | recusada: "Informe uma data válida" | recusada (já certo) |

A linha de "05/10/0026" e as duas últimas foram acrescentadas depois da revisão: o resultado vem do `Date.parse` do
V8 (Node, mesmo fuso), que é exatamente o que o `NativeDateAdapter.parse` chama, sem passar pelo spec temporário.

Observações:

- **Ao sair do campo**: o texto só é reescrito no `blur` (e só quando a data é válida), então o usuário vê a data
  reinterpretada apenas ao sair do campo. Salvando com Enter logo depois de digitar, o valor já foi lido no evento
  `input` e é salvo sem que o campo mostre a troca.
- **Datas inválidas**: "31/02/2026", "13/13/2026", "00/10/2026" e "05/10/2026x" já são recusadas, mas por acaso
  (o `Date.parse` não consegue ler esses textos: o mês 31, 13 ou 00 não existe na leitura mês/dia/ano, e o "x" no fim
  invalida o texto). "02/30/2026" mostra que a leitura atual não recusa os dias inexistentes: ela os transborda para
  o mês seguinte.
- **Exibição**: já em pt-BR. O valor inicial aparece como `07/10/2026` (dd/mm/aaaa), formatado pelo `Intl` com o
  `MAT_DATE_LOCALE`, que por padrão vem do `LOCALE_ID` `pt-BR` do `app.config.ts`. Não há mudança de exibição a fazer.
- **Campo "Mês" (`shared/month-field`)**: não é afetado. O `input` tem `readonly` (`month-field.html:5`) e abre o
  calendário no clique, então o usuário não digita; o `MatDatepickerInput` só chama o `parse` no evento `input`
  (`datepicker.mjs:3356-3359`). A exibição "MM/aaaa" já está em pt-BR (avaliação `calendarios-em-ingles`).

Passos manuais equivalentes:

1. Em `/transactions`, abrir "Nova transação" e preencher o valor.
2. Apagar a data e digitar "05/10/2026"; sair do campo: ele passa a mostrar "10/05/2026".
3. Salvar: a transação aparece em 10/05/2026. Digitando "24/09/2026", o formulário mostra "Informe uma data válida".

## Caminhos de código suspeitos

- `node_modules/@angular/material/fesm2022/core.mjs:124-129` (Material 21.2.14): `NativeDateAdapter.parse` ignora o
  formato e o locale e faz `new Date(Date.parse(value))`. O `Date.parse` do V8 lê "nn/nn/aaaa" como mês/dia/ano,
  transborda dias inexistentes, aceita ano com dois algarismos, ano ausente e formatos em inglês, e lê "aaaa-mm-dd"
  como meia-noite UTC.
- `node_modules/@angular/material/fesm2022/core.mjs:286-290`: o `MAT_NATIVE_DATE_FORMATS` tem `parse.dateInput: null`;
  não há formato de leitura a configurar no adapter nativo.
- `node_modules/@angular/material/fesm2022/datepicker.mjs:3356-3365` (`_onInput`): chama o `parse` a cada digitação e
  passa a data ao formulário; `3249-3254` (`_parseValidator`): só marca `matDatepickerParse` quando o resultado é uma
  data inválida, então qualquer data que o `Date.parse` aceite passa; `3381-3388` (`_onBlur`/`_formatValue`): reescreve
  o texto só ao sair do campo.
- `front/src/app/transactions/components/transaction-form/transaction-form.ts:5,36`: fornece
  `provideNativeDateAdapter()` sem nenhuma personalização da leitura.
- `front/src/app/transactions/components/transaction-form/transaction-form.html:19-23`: o único campo de data
  digitável da aplicação (`[matDatepicker]` só aparece aqui e no `month-field`, que é `readonly`).
- `front/src/app/shared/date-utils/date-utils.ts:6-8`: `toApiDate` usa o dia local, por isso "2026-09-24", lido como
  UTC, sai como 23/09. Está correto para datas locais; o problema é o `parse` produzir uma data UTC.

## Hipótese de causa raiz

Confiança: **alta** (reproduzida, e a correção proposta foi provada no mesmo spec temporário).

O `NativeDateAdapter` do Angular Material formata com `Intl` no locale configurado, mas lê o texto com `Date.parse`,
que não conhece locale nem formato: a própria documentação do Material recomenda um adapter próprio ou de biblioteca
quando a leitura importa. O projeto usa o adapter nativo sem personalizar o `parse`, então a exibição (dd/mm/aaaa) e a
leitura (regras do `Date.parse`, na prática mês/dia/ano) divergem. O relato preliminar está certo quanto à causa; um
`MAT_DATE_LOCALE` explícito não mudaria nada, porque o `parse` não o consulta.

**É bug, e não comportamento novo**: a FR-014 da 003 já exige a leitura dia/mês/ano e registra o atual como desvio
conhecido, encaminhado ao fluxo de bugs.

## Correção proposta

**Preferida: uma subclasse do `NativeDateAdapter` ao lado do `transaction-form`, que só sobrescreve o `parse`,
fornecida no `TransactionForm`.**

1. Criar `front/src/app/transactions/components/transaction-form/pt-br-date-adapter.ts`: uma classe `@Injectable()` que estende o
   `NativeDateAdapter` e sobrescreve `parse(value, parseFormat)`:
   - número: mantém o comportamento herdado (`super.parse`);
   - texto vazio (depois de `trim`): `null` (o campo fica sem data e o `Validators.required` mostra "Informe uma data
     válida", como hoje);
   - texto que casa com `^(\d{1,2})/(\d{1,2})/(\d{4})$` depois de `trim`: cria a data local do dia (meia-noite local,
     como o `DateUtils.fromApiDate`, com `new Date(y, m - 1, d)`) e confere que dia, mês e ano não transbordaram; se
     transbordaram (31/02, 13/13, 00/10 e os anos 0000-0099, que o `new Date` leva para 1900-1999), devolve
     `this.invalid()` (decisão P2);
   - qualquer outro texto: `this.invalid()`, que o `_parseValidator` do Material transforma em erro
     (`matDatepickerParse`) e o template mostra como "Informe uma data válida".

   A exibição, a navegação do calendário e o resto ficam herdados. Comentários e identificadores em inglês.
2. Em `transaction-form.ts`, trocar o `DateAdapter` do `provideNativeDateAdapter()` pela subclasse, mantendo o
   `MAT_DATE_FORMATS` nativo. Por exemplo: `provideNativeDateAdapter()` seguido de
   `{ provide: DateAdapter, useClass: PtBrDateAdapter }` (o último provider vence), ou os dois providers explícitos
   (`DateAdapter` e `MAT_DATE_FORMATS` com `MAT_NATIVE_DATE_FORMATS`). A forma exata fica para o bug-fix.
3. O `month-field` não muda: não aceita digitação.

Segue o padrão já usado no projeto: o adapter e o `MatDatepickerIntl` são fornecidos no próprio componente (ver
`front/bugs/calendarios-em-ingles/`). A classe fica em `transactions/`, ao lado do `transaction-form`, e não em
`shared/`, porque hoje só o formulário de transação aceita data digitada: pela F1, só vai para `shared/` o que serve a
mais de uma feature, e o princípio V desaconselha antecipar esse uso (decisão do usuário). Se outra feature precisar,
a classe é movida para `shared/` nessa hora. No spec temporário, essa subclasse produziu exatamente o esperado:

| Digitado | Campo depois de sair dele | Salvo |
| --- | --- | --- |
| `05/10/2026` | `05/10/2026` | `2026-10-05` |
| `5/9/2026` | `05/09/2026` | `2026-09-05` |
| ` 05/10/2026 ` | `05/10/2026` | `2026-10-05` |
| `24/09/2026` | `24/09/2026` | `2026-09-24` |
| `31/02/2026`, `02/30/2026`, `13/13/2026`, `2026-09-24`, `05/10/26`, `5/10`, `05-10-2026`, `Oct 5 2026` | sem mudança | recusada: "Informe uma data válida" |

Os casos "05/10/0026", "00/10/2026" e "05/10/2026x", acrescentados depois da revisão, não passaram pelo spec
temporário com a subclasse. Pela regra acima eles são recusados (o regex não casa o "x", e o dia 00 e o ano 0026
transbordam), e os testes do item 1 confirmam isso no bug-fix.

**Alternativas:**

- (B) Adapter de biblioteca: `@angular/material-date-fns-adapter` com `date-fns` (ou `@angular/material-luxon-adapter`
  com `luxon`), com `parse.dateInput: 'dd/MM/yyyy'` e `display` equivalente. Vantagens: leitura estrita por formato,
  mantida por terceiros, e útil se outras features passarem a ter campos de data com formatos diferentes. Desvantagens:
  duas dependências novas (o adapter e a biblioteca) para substituir umas 15 linhas, o que o princípio II exige
  justificar no plan (motivo, manutenção ativa, `npm audit` limpo) e o princípio V desaconselha ("não crie abstrações
  para necessidades hipotéticas"); troca o adapter também no `month-field` para manter um padrão só, ou deixa dois
  adapters diferentes na aplicação; muda o tipo das datas (date-fns usa `Date`, mas o luxon usa `DateTime`, que
  quebraria o `DateUtils`); e o `date-fns` aceita "5/9/2026" em `dd/MM/yyyy` só no modo não estrito, o que pede
  conferir o comportamento com dois formatos de leitura. Não recomendada agora.
- (C) Máscara de digitação (por exemplo, `__/__/____`) no input. Desvantagem: dependência nova ou código próprio de
  máscara, e não resolve sozinha a leitura, que continuaria no `Date.parse`. Descartada.
- (D) Fornecer `MAT_DATE_LOCALE` explícito `pt-BR`. **Descartada**: o `parse` nativo não usa o locale (ver a causa
  raiz).

**Arquivos que devem mudar (preferida):**

- novo: `front/src/app/transactions/components/transaction-form/pt-br-date-adapter.ts` e `pt-br-date-adapter.spec.ts`;
- `front/src/app/transactions/components/transaction-form/transaction-form.ts` e `transaction-form.spec.ts`.

Nenhuma mudança no back, nos services, nos tipos, nas rotas, no contrato da API ou no `month-field`; nenhuma dependência
nova.

**Testes de regressão previstos** (escritos antes da correção; os marcados com (falha) falham hoje):

1. `pt-br-date-adapter.spec.ts` (a classe isolada, com `TestBed` ou `new` num contexto de injeção):
   - (falha) "should parse a typed date as day/month/year": "05/10/2026" → 5 de outubro de 2026, e "5/9/2026" → 5 de
     setembro de 2026, ambas à meia-noite local;
   - (falha) "should parse a day above 12": "24/09/2026" → 24 de setembro de 2026;
   - "should ignore surrounding spaces": " 05/10/2026 " → 5 de outubro de 2026 (falha hoje: vira 10 de maio);
   - (falha, em parte) `it.each` "should reject %s": "31/02/2026", "02/30/2026", "13/13/2026", "00/10/2026",
     "05/10/26", "05/10/0026", "5/10", "05-10-2026", "2026-09-24", "Oct 5 2026", "05/10/2026x" → data inválida
     (`isValid` falso); hoje "31/02/2026", "13/13/2026", "00/10/2026" e "05/10/2026x" já dão inválida, os demais não
     (o "05/10/0026", por exemplo, vira 10/05/2026);
   - "should parse an empty text as no date": "" e "   " → `null`;
   - "should keep formatting as dd/mm/yyyy" com `MAT_DATE_LOCALE` `pt-BR`: `format(new Date(2026, 9, 5), dateInput)`
     → "05/10/2026" (já passa; protege a herança).
2. `transaction-form.spec.ts` (fluxo do componente, no padrão do `type()`/`save()` existentes, com `LOCALE_ID` `pt-BR`
   no `TestBed` desses testes, porque a suíte hoje não o fornece e a exibição sairia em en-US):
   - (falha) "should save a typed date as day/month/year": digitar "05/10/2026" e salvar → `date`
     `2026-10-05T00:00:00.000Z`; e "5/9/2026" → `2026-09-05T00:00:00.000Z`;
   - (falha) "should show the typed date unchanged after leaving the field": digitar "5/9/2026", disparar `blur` → o
     campo mostra "05/09/2026";
   - (falha) `it.each` "should not save the date %s": "02/30/2026", "2026-09-24", "05/10/26" → `close` não chamado e
     `errors()` igual a `['Informe uma data válida']`.

Cuidado ao escrever os testes (skill `write-front-tests`): o `_onInput` do Material lê `event.target.value`, então o
helper `type()` atual (atribui o valor e dispara `input`) serve; o `blur` é um `new Event('blur')` no input. O
resultado de "2026-09-24" depende do fuso da máquina (no Brasil vira 23/09), mas a asserção é a recusa, que não
depende do fuso. Não há `httpResource` no formulário, então o `whenStable()` não trava.

Verificação final: `npx ng test --watch=false`, `npm run lint` e `npm run build` em `front/`.

## Riscos e considerações

- **Comportamento que deixa de existir**: hoje "Oct 5 2026", "2026-09-24" e outros formatos soltos são aceitos; depois
  da correção, são recusados, como pede a FR-014. Só afeta a digitação; o calendário e o valor pré-preenchido na edição
  (que vem do `DateUtils.fromApiDate`, um `Date`, sem passar pelo `parse`) não mudam.
- **Anos de 0000 a 0099**: o regex aceita quatro algarismos, mas `new Date(26, …)` vira 1926, e a conferência de
  transbordamento recusa "05/10/0026". Pela decisão P2, essa recusa é o comportamento desejado; o bug-fix a registra no
  `fix.md` e um teste a cobre.
- **Atualização do Material**: a subclasse depende da assinatura de `parse(value, parseFormat)` e do `invalid()` do
  `NativeDateAdapter`, estáveis há várias versões. O teste da classe (item 1) mitiga.
- **Mesma linha do `calendarios-em-ingles`**: a correção mexe na mesma linha de `providers` (`transaction-form.ts:36`)
  que o bug já corrigido dos rótulos. Ele está commitado, então não há conflito; só é preciso manter o
  `PtBrDatepickerIntl` ao lado.
- **Fora deste bug**: os valores com 14 ou mais algarismos (`parseFloat`, `transaction-form.ts:65`) e a recusa do zero,
  também da FR-014, ficam para a avaliação própria.
- **Bundle**: uma classe pequena no chunk sob demanda de transações; efeito desprezível.
- **Escopo**: só o front. Nenhuma mudança no back, no contrato da API, em dados ou em segurança além de impedir a
  gravação de uma data diferente da digitada (o que cumpre o princípio II).
- **Specs (desvios conhecidos)**: depois da correção, deixam de valer na 003:
  - o cenário 9 da História 2 (218-221): sai o parêntese "desvio conhecido: hoje '5/9/2026' é salva como 9 de maio…";
  - os Casos-limite (367-374): sai a parte "Desvio conhecido, que fere a integridade dos dados… diálogo fecha sem
    mostrá-la", ficando a regra esperada;
  - a FR-014 (461-465): sai "e não lê a data digitada como dia/mês/ano" e a menção à data digitada no trecho sobre o
    princípio II, mas **continuam** os desvios do zero e dos valores com 14 ou mais algarismos, que são de outro bug;
  - as Premissas (597-598): "o formato da data" sai da lista de desvios conhecidos;
  - a lista de desvios das Premissas (~622-634: a data digitada é citada na 624, e a menção "corrigidos em" dos
    calendários está na 627) e a de bloqueios (635-638) registram a data digitada como corrigida em
    `front/bugs/data-digitada-como-mes-dia-ano/`, como foi feito para os calendários e a consulta repetida.
  - Os Esclarecimentos (26-30, 96-100 e 110-112) são histórico e ficam. Ver P3.

## Decisões do usuário (portão do diagnóstico, 2026-10-07)

A revisão independente não achou nada CRITICAL nem HIGH; os achados MEDIUM e LOW (o caso "05/10/0026" na reprodução e
nos testes, a lista das recusas que já passam hoje e as linhas das Premissas da 003) foram corrigidos nesta versão. O
usuário aprovou o diagnóstico e decidiu:

- **P1**: (A), a subclasse própria do `NativeDateAdapter`, sem dependência nova.
- **Local da classe**: em `transactions/`, ao lado do `transaction-form`
  (`front/src/app/transactions/components/transaction-form/pt-br-date-adapter.ts` e `.spec.ts`), e não em `shared/`,
  porque pela F1 só vai para `shared/` o que serve a mais de uma feature (princípio V).
- **P2**: (A), recusar os anos de 0000 a 0099.
- **P3**: (A), as notas de desvio da 003 saem no próprio `/speckit-bug-fix`, com registro no `fix.md`.

## Perguntas (respondidas)

- **P1 — Onde ler a data**: [NEEDS CLARIFICATION: subclasse própria do `NativeDateAdapter` ou adapter de biblioteca?]
  - (A) Subclasse `PtBrDateAdapter` (proposta original em `shared/date-adapter/`; por decisão do usuário, fica em
    `transactions/components/transaction-form/`), que só sobrescreve o `parse`, fornecida no
    `TransactionForm`. Consequências: cerca de 15 linhas próprias e testadas; nenhuma dependência nova; o
    `month-field` fica como está.
  - (B) `@angular/material-date-fns-adapter` + `date-fns` com formato `dd/MM/yyyy`. Consequências: duas dependências
    novas, com justificativa pelo princípio II; leitura por formato mantida por terceiros; convém trocar também o
    `month-field` para um adapter só.
  - **Recomendação**: (A). É a menor mudança, segue o padrão do `PtBrDatepickerIntl` e não traz dependência para uma
    necessidade que hoje se resume a um formato.
- **P2 — Anos com zeros à esquerda ("05/10/0026")**: [NEEDS CLARIFICATION: como tratar anos de 0000 a 0099?]
  - (A) Recusar: o resultado natural de `new Date(y, m, d)` com a conferência de transbordamento. Consequência: um ano
    digitado errado por falta de algarismos ("0026") é recusado, o que protege contra erro de digitação.
  - (B) Aceitar literalmente com `setFullYear` (ano 26). Consequência: segue a letra da FR-014 ("ano em quatro
    algarismos"), mas grava uma transação no ano 26, quase sempre um erro de digitação; a API, que não limita a data,
    aceitaria.
  - **Recomendação**: (A). O caso não tem uso real em finanças pessoais, e recusar é mais seguro para a integridade
    dos dados; o bug-fix registra a decisão no `fix.md` e um teste a cobre.
- **P3 — Specs 003**: [NEEDS CLARIFICATION: ajustar a spec no próprio bug-fix ou numa revisão à parte?]
  - (A) No `/speckit-bug-fix`, com registro no `fix.md`, retirando as notas de desvio listadas em "Riscos e
    considerações" e mantendo os desvios do zero e dos valores grandes. Consequência: a spec deixa de descrever um
    desvio que não existe mais.
  - (B) Deixar a spec como está e só citá-la no `fix.md`.
  - **Recomendação**: (A), como foi feito nos bugs `calendarios-em-ingles` e `consulta-repetida-sem-mudanca`.
