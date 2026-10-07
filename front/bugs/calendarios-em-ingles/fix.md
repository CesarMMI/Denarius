# Correção do bug: os calendários mostram e anunciam textos em inglês

- **Slug**: calendarios-em-ingles
- **Corrigido em**: 2026-10-07
- **Avaliação**: ./assessment.md
- **Status**: applied

## Resumo

Opção (A) da P1, escolhida pelo usuário: uma subclasse do `MatDatepickerIntl` em português em `shared/`
(`PtBrDatepickerIntl`), fornecida nos `providers` do `MonthField` e do `TransactionForm`, ao lado do
`provideNativeDateAdapter` que já estava lá. O provider do componente vence o `MatDatepickerIntl` que o
`MatDatepickerModule` declara, e por isso a correção não está no `app.config.ts`. Pela P2 (A), a classe traz o conjunto
completo de rótulos. Pela P3 (A), saíram das specs 002 e 003 as notas "Desvio conhecido" deste bug.

## Alterações

| Arquivo | Alteração | Notas |
|------|--------|-------|
| `front/src/app/shared/datepicker-intl/pt-br-datepicker-intl.ts` | adicionado | `@Injectable()` que estende o `MatDatepickerIntl`, com os 14 rótulos e o `formatYearRangeLabel` em português; o `formatYearRange` ("2016 – 2039") é herdado |
| `front/src/app/shared/datepicker-intl/pt-br-datepicker-intl.spec.ts` | teste adicionado | 16 casos: cada rótulo (`it.each`), o `formatYearRangeLabel` e o `formatYearRange` |
| `front/src/app/shared/month-field/month-field.ts` | modificado | `{ provide: MatDatepickerIntl, useClass: PtBrDatepickerIntl }` nos `providers` |
| `front/src/app/shared/month-field/month-field.spec.ts` | testes adicionados | 2 testes de regressão (`describe('calendar labels')`) |
| `front/src/app/transactions/components/transaction-form/transaction-form.ts` | modificado | o mesmo provider, ao lado do `provideNativeDateAdapter()` (que não mudou) |
| `front/src/app/transactions/components/transaction-form/transaction-form.spec.ts` | teste adicionado | 1 teste de regressão |
| `front/specs/002-gestao-de-categorias/spec.md` | modificado | notas "Desvio conhecido" deste bug removidas (ver abaixo) |
| `front/specs/003-gestao-de-transacoes/spec.md` | modificado | notas "Desvio conhecido" deste bug removidas; o conjunto de rótulos passa a apontar para este bug |

Nenhuma mudança no back, no contrato da API, nos services, nos tipos, nas rotas, no `app.config.ts` nem no formato do
`provideNativeDateAdapter`. Nenhuma dependência nova.

## Destaques do diff

```ts
// pt-br-datepicker-intl.ts (trecho)
@Injectable()
export class PtBrDatepickerIntl extends MatDatepickerIntl {
	override openCalendarLabel = 'Abrir calendário';
	// ... os demais rótulos da P2
	override formatYearRangeLabel(start: string, end: string): string {
		return `de ${start} a ${end}`;
	}
}

// month-field.ts e transaction-form.ts
{ provide: MatDatepickerIntl, useClass: PtBrDatepickerIntl },
```

## Testes adicionados

Escritos antes da correção. Como o build dos testes compila todos os specs, o `pt-br-datepicker-intl.spec.ts` foi
posto de lado (movido para fora do projeto) na execução que mostrou a falha dos outros dois arquivos, e devolvido em
seguida. Seguindo a técnica da avaliação, as consultas ao calendário são no `document` (`.mat-datepicker-content …`), o
calendário abre e fecha pelo `MatDatepicker.open()`/`close()` com `fixture.whenStable()`, e o `MonthField` fixa o mês
do host em setembro de 2026 para a faixa de anos.

| Teste | Antes da correção | Depois |
| --- | --- | --- |
| `pt-br-datepicker-intl.spec.ts` (16 casos) | falha no build: `TS2307: Cannot find module './pt-br-datepicker-intl'` | passa |
| `month-field.spec.ts` › calendar labels › should name the calendar toggle in Portuguese | falha: `expected 'Open calendar' to be 'Abrir calendário'` | passa |
| `month-field.spec.ts` › calendar labels › should name the calendar controls in Portuguese in every view | falha: `expected 'Choose date' to be 'Escolher data'` | passa |
| `transaction-form.spec.ts` › should name the date calendar controls in Portuguese | falha: `expected 'Open calendar' to be 'Abrir calendário'` | passa |

O teste das visões do `MonthField` cobre a visão de meses ("Escolher data", "Ano anterior", "Próximo ano", "Fechar
calendário"), a de dias ("Escolher mês e ano", "Mês anterior", "Próximo mês") e a de anos ("Escolher data", a
descrição "de 2016 a 2039", "24 anos anteriores", "Próximos 24 anos"). O do `TransactionForm` cobre o toggle e a visão
de dias. Nenhum teste existente foi alterado.

## Verificação local

Em `front/`:

- `npx ng test --watch=false` → 30 arquivos, 304 testes, todos passando (eram 285; +19).
- `npm run lint` → "All files pass linting."
- `npm run build` → sem erro nem aviso de budget; Initial total 633,08 kB (igual à referência).
- `npx prettier --write` nos arquivos alterados: só reformatou o `month-field.spec.ts`, que foi tocado; nenhum
  arquivo não tocado mudou.

## Specs ajustadas (P3 = A)

Só saíram as notas de desvio deste bug e a referência a ele; o texto e o sentido de requisitos, critérios e cenários
não mudaram, e a frase "e vale para todos os calendários da aplicação" continua na FR-020 da 002 e na FR-030 da 003.

- `front/specs/002-gestao-de-categorias/spec.md`:
  - Esclarecimento do calendário e das cores: "o atual é um desvio conhecido, a corrigir pelo fluxo de bugs do front,
    fora desta spec" passou a "O desvio foi corrigido pelo fluxo de bugs do front (`front/bugs/calendarios-em-ingles/`),
    fora desta spec".
  - Casos-limite: o item do calendário ficou só com a regra e a referência à FR-020; saiu a nota com "Open calendar",
    "Choose date", "Previous year", "Next year" e "Close calendar".
  - FR-020: saiu "Desvio conhecido: hoje eles estão em inglês (ver Casos-limite); a correção segue o fluxo de bugs do
    front, fora desta spec,"; a frase ficou "… (por exemplo, "Abrir calendário"), e vale para todos os calendários da
    aplicação."
  - Premissas: "o calendário em português (FR-020)" passou a "(FR-020, corrigido em
    `front/bugs/calendarios-em-ingles/`)".
- `front/specs/003-gestao-de-transacoes/spec.md`:
  - Esclarecimento dos calendários: "O atual é um desvio conhecido, corrigido pelo fluxo de bugs do front, fora desta
    spec" passou a "O desvio foi corrigido pelo fluxo de bugs do front (`front/bugs/calendarios-em-ingles/`), fora
    desta spec".
  - Casos-limite: o item dos botões de navegação ficou só com a regra e a referência à FR-030; saiu a nota com os
    rótulos em inglês.
  - FR-030: "o conjunto completo desses rótulos é definido no bug-fix desta correção. Desvio conhecido: … e vale para
    todos os calendários da aplicação. O desvio contraria a regra de idioma do `AGENTS.md` e bloqueia a entrega desta
    feature." passou a "…, e vale para todos os calendários da aplicação; o conjunto completo desses rótulos está em
    `front/bugs/calendarios-em-ingles/`." A frase do bloqueio saiu com a nota; o bloqueio continua registrado nos
    Esclarecimentos e nas Premissas.
  - Premissas: "os calendários em português (FR-030)" passou a "(FR-030, corrigidos em
    `front/bugs/calendarios-em-ingles/`)".
- Ficaram como estavam, como no bug `consulta-repetida-sem-mudanca`: as perguntas dos Esclarecimentos (que citam o
  estado "hoje em inglês" na pergunta), as listas de bloqueios (003, Esclarecimentos e Premissas; 002, esclarecimento
  dos bloqueios) e o esclarecimento da 003 que diz que o conjunto sai "do bug-fix da FR-030", que é histórico e
  continua certo. Nenhum cenário de aceitação citava este bug.
- Notas de outros desvios (data digitada, dia de outro mês no "Mês" etc.) não foram tocadas.

## Desvios da avaliação

Nenhum. A correção seguiu a opção preferida, com os arquivos e os testes previstos.

## Pendências

- Um componente novo com calendário precisa do mesmo provider (risco registrado na avaliação); os testes de rótulos
  dos dois componentes servem de modelo.
- Numa atualização do Angular Material, conferir se o `MatDatepickerIntl` ganhou rótulos novos (o teste da classe
  lista os atuais).
- `plan.md`, `research.md`, `tasks.md` e checklists das 002 e 003 podem ainda citar o desvio como pendente; a
  reavaliar no `/speckit-converge`.
- Próximo passo: `/speckit-bug-test project=front slug=calendarios-em-ingles`.
