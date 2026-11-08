# Correção do bug: a data digitada no formulário de transação é lida como mês/dia/ano

- **Slug**: data-digitada-como-mes-dia-ano
- **Corrigido em**: 2026-10-07
- **Avaliação**: ./assessment.md
- **Status**: applied

## Resumo

Correção preferida da avaliação, com as decisões do usuário (P1 = A, P2 = A, P3 = A). Uma subclasse própria do
`NativeDateAdapter`, `PtBrDateAdapter`, ao lado do `transaction-form`, sobrescreve só o `parse`: lê o texto digitado
apenas como dia/mês/ano, com o ano em quatro algarismos e com ou sem zeros à esquerda, como data local, e devolve uma
data inválida para qualquer outro formato, para as datas inexistentes e para os anos de 0000 a 0099. O
`TransactionForm` fornece a subclasse no lugar do adapter nativo; a exibição, o calendário e os formatos continuam os
do `provideNativeDateAdapter()`.

## Alterações

| Arquivo | Alteração | Notas |
|------|--------|-------|
| `front/src/app/transactions/components/transaction-form/pt-br-date-adapter.ts` | adicionado | `PtBrDateAdapter extends NativeDateAdapter`, só com o `parse` |
| `front/src/app/transactions/components/transaction-form/pt-br-date-adapter.spec.ts` | teste adicionado | 16 testes da classe isolada |
| `front/src/app/transactions/components/transaction-form/transaction-form.ts` | modificado | `{ provide: DateAdapter, useClass: PtBrDateAdapter }` depois do `provideNativeDateAdapter()`; o `PtBrDatepickerIntl` continua |
| `front/src/app/transactions/components/transaction-form/transaction-form.spec.ts` | testes adicionados | `describe('typing the date')`, 6 testes, com `LOCALE_ID` `pt-BR` |
| `front/specs/003-gestao-de-transacoes/spec.md` | modificado | notas de desvio da data digitada removidas (ver abaixo) |

Nenhuma mudança no back, no contrato da API, nos services, nos tipos, nas rotas nem no `month-field` (que é `readonly`).
Nenhuma dependência nova. O `parseFloat` do valor não foi tocado (outro bug).

## Destaques do diff

```ts
// pt-br-date-adapter.ts
@Injectable()
export class PtBrDateAdapter extends NativeDateAdapter {
	override parse(value: unknown, parseFormat?: unknown): Date | null {
		if (typeof value !== 'string') return super.parse(value, parseFormat);
		const text = value.trim();
		if (!text) return null;

		const match = text.match(/^(\d{1,2})\/(\d{1,2})\/(\d{4})$/);
		if (!match) return this.invalid();
		const [day, month, year] = match.slice(1).map(Number);
		const date = new Date(year, month - 1, day);
		const overflowed = date.getFullYear() !== year || date.getMonth() !== month - 1 || date.getDate() !== day;
		return overflowed ? this.invalid() : date;
	}
}

// transaction-form.ts
providers: [
	provideNativeDateAdapter(),
	{ provide: DateAdapter, useClass: PtBrDateAdapter },
	{ provide: MatDatepickerIntl, useClass: PtBrDatepickerIntl },
],
```

A conferência de transbordamento recusa os dias e meses inexistentes (31/02, 02/30, 13/13, 00/10) e também os anos de
0000 a 0099, que o `new Date` leva para 1900-1999 (`05/10/0026` vira 1926): é a decisão P2, coberta pelo teste
"should reject 05/10/0026". Um valor que não é texto (número ou `null`) segue o `parse` herdado.

## Testes adicionados

Escritos antes da correção, com a skill `write-front-tests`. Execução "antes": como o `pt-br-date-adapter.spec.ts` não
compila sem a classe, ele rodou contra um esboço temporário `class PtBrDateAdapter extends NativeDateAdapter {}` (sem
sobrescrever nada, ou seja, o `parse` de hoje); o `transaction-form.ts` ainda era o do `main`. Resultado: 17 falhas e
23 aprovações nos dois specs (40 testes). Em seguida o esboço foi trocado pela classe real e o formulário passou a
fornecê-la: 40 aprovações.

### Reprodução (falham antes, passam depois)

`pt-br-date-adapter.spec.ts`:

| Teste | Antes da correção | Depois |
| --- | --- | --- |
| should parse a typed date as day/month/year | `expected 2026-05-10T03:00:00.000Z to deeply equal 2026-10-05T03:00:00.000Z` | passa |
| should parse a day above 12 | `expected Invalid Date to deeply equal 2026-09-24T03:00:00.000Z` | passa |
| should ignore surrounding spaces | `expected 2026-05-10T03:00:00.000Z to deeply equal 2026-10-05T03:00:00.000Z` | passa |
| should reject 02/30/2026, 05/10/26, 05/10/0026, 5/10, 05-10-2026, 2026-09-24, Oct 5 2026 (7 casos) | `expected true to be false` (data válida) | passa |
| should parse an empty text as no date | `expected Invalid Date to be null` (o "   " vira data inválida) | passa |

`transaction-form.spec.ts` (`describe('typing the date')`):

| Teste | Antes da correção | Depois |
| --- | --- | --- |
| should save a typed date as day/month/year: 05/10/2026 | salvou `2026-05-10T00:00:00.000Z` | passa |
| should save a typed date as day/month/year: 5/9/2026 | salvou `2026-05-09T00:00:00.000Z` | passa |
| should show the typed date unchanged after leaving the field | `expected '09/05/2026' to be '05/09/2026'` | passa |
| should not save the date 02/30/2026 | `close` chamado (salvou `2026-03-02`) | passa |
| should not save the date 2026-09-24 | `close` chamado (salvou `2026-09-23`) | passa |
| should not save the date 05/10/26 | `close` chamado (salvou `2026-05-10`) | passa |

### Guarda (passam antes e depois)

`pt-br-date-adapter.spec.ts`:

- should reject 31/02/2026, 13/13/2026, 00/10/2026 e 05/10/2026x (4 casos): já recusadas hoje, por acaso, como diz a
  avaliação;
- should keep formatting as dd/mm/yyyy: `format(new Date(2026, 9, 5), dateInput)` → "05/10/2026" com
  `MAT_DATE_LOCALE` `pt-BR` (protege a exibição herdada).

Nenhum teste existente foi alterado, ignorado ou apagado. O `LOCALE_ID` `pt-BR` entra só no `beforeEach` do
`describe('typing the date')`, antes do `render()`.

## Verificação local

Em `front/`:

- `npx ng test --watch=false` → 32 arquivos, 357 testes, todos passando (eram 335; +22: 16 do `pt-br-date-adapter` e 6 do `transaction-form`).
- `npm run lint` → "All files pass linting."
- `npm run build` → sem erro nem aviso de budget; Initial total 633,08 kB (igual à referência; a classe entra no chunk sob demanda de transações).
- `npx prettier --write` na pasta `transaction-form/`: nenhum arquivo mudou; nenhum arquivo não tocado mudou.

## Specs ajustadas (P3 = A)

`front/specs/003-gestao-de-transacoes/spec.md`:

- História 2, cenário 9: saiu "desvio conhecido: hoje "5/9/2026" é salva como 9 de maio, e "2026-09-24", como
  23/09/2026 no fuso do Brasil;"; ficou "(ver FR-014)".
- Casos-limite, item da data digitada: ficou só a regra e a referência (FR-014); saiu "Desvio conhecido, que fere a
  integridade dos dados (princípio II) e bloqueia a entrega desta feature: hoje o campo não segue … o diálogo fecha
  sem mostrá-la.".
- FR-014: saiu "e não lê a data digitada como dia/mês/ano" do desvio conhecido, e o trecho do princípio II passou a
  falar só dos valores com 14 ou mais algarismos ("Os valores com 14 ou mais algarismos inteiros, gravados hoje
  diferentes do digitado, ferem a integridade dos dados … e bloqueiam a entrega desta feature; o zero, que a API
  recusa, não bloqueia."); acrescentada a frase "A leitura da data digitada como dia/mês/ano foi corrigida em
  `front/bugs/data-digitada-como-mes-dia-ano/`.". Os desvios do zero e dos valores com 14 ou mais algarismos
  continuam.
- Premissas (regras de negócio): "(FR-014, em que a recusa do zero, o limite do valor e o formato da data são desvios
  conhecidos)" passou a "(FR-014, em que a recusa do zero e o limite do valor são desvios conhecidos)".
- Premissas (lista de desvios): "a data digitada, a recusa do zero e o limite do valor (FR-014)" passou a "a data
  digitada (FR-014, corrigida em `front/bugs/data-digitada-como-mes-dia-ano/`), a recusa do zero e o limite do valor
  (FR-014)".
- Ficaram como estavam: os Esclarecimentos (histórico) e a frase "Bloqueiam a entrega desta feature…" das Premissas,
  que ainda cita a data digitada entre os bloqueios, como foi feito no `clique-repetido-em-excluir`; o bloqueio se
  encerra pelo critério de conclusão da mesma frase (`test.md` `verified` e `/speckit-converge`).

## Desvios da avaliação

- Na lista de bloqueios das Premissas (frase "Bloqueiam a entrega desta feature…"), a avaliação previa registrar a data
  digitada como corrigida; por instrução do orquestrador, essa frase ficou como histórico, sem mudança. A data
  digitada aparece como corrigida na lista de desvios das Premissas, logo antes dela.
- Fora isso, nenhum: a correção seguiu a opção preferida, nos arquivos e com os testes previstos. A forma dos
  providers foi a primeira sugerida (`provideNativeDateAdapter()` seguido de `{ provide: DateAdapter, useClass:
  PtBrDateAdapter }`).

## Pendências

- Conferir num navegador a digitação, o `blur` e o Enter (no `/speckit-bug-test` ou à mão).
- `plan.md`, `research.md`, `tasks.md` e checklists da 003 podem ainda citar o desvio como pendente; a reavaliar no
  `/speckit-converge` (FR-014, cenário 9 da História 2 e Casos-limite).
- Os valores com 14 ou mais algarismos (`parseFloat`) e a recusa do zero seguem para a avaliação própria.
- Próximo passo: `/speckit-bug-test project=front slug=data-digitada-como-mes-dia-ano`.
