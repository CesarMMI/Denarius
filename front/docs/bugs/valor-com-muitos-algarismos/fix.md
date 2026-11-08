# Correção do bug: valores com 14 ou mais algarismos inteiros são salvos diferentes do digitado

- **Slug**: valor-com-muitos-algarismos
- **Corrigido em**: 2026-10-07
- **Avaliação**: ./assessment.md
- **Status**: applied

## Resumo

Correção preferida da avaliação, com as decisões do usuário (P1 = A, P2 = A). A regex do valor no `TransactionForm`
passou a limitar a parte inteira a 13 algarismos (`/^\d{1,13}([.,]\d{1,2})?$/`), com um comentário em inglês que
explica o limite. Valores maiores, inclusive os que só passam de 13 algarismos por zeros à esquerda, são recusados com
"Informe um valor válido" e o diálogo aberto. Dentro do limite, a conversão com `parseFloat` é exata e não mudou.

## Alterações

| Arquivo | Alteração | Notas |
|------|--------|-------|
| `front/src/app/transactions/components/transaction-form/transaction-form.ts` | modificado | regex `\d+` → `\d{1,13}` e comentário |
| `front/src/app/transactions/components/transaction-form/transaction-form.spec.ts` | testes adicionados | 6 testes no `describe('validating')` |
| `front/specs/003-gestao-de-transacoes/spec.md` | modificado | notas de desvio do limite do valor removidas (ver abaixo) |

Nenhuma mudança no back, no contrato da API, nos services, nos tipos, no template nem nas dependências. O
`parseFloat` e a aceitação do zero não foram tocados (o zero continua aceito, outro desvio).

## Destaques do diff

```ts
value: new FormControl(..., {
	nonNullable: true,
	// Up to 13 integer digits: with cents, larger values don't fit in a number (double) without rounding.
	validators: [Validators.required, Validators.pattern(/^\d{1,13}([.,]\d{1,2})?$/)],
}),
```

## Testes adicionados

Escritos antes da correção, com a skill `write-front-tests`, no padrão do `it.each(['8.600,00', '-12'])` existente.
Execução "antes" (`npx ng test --watch=false --include='src/app/transactions/components/transaction-form/*.spec.ts'`,
com o `transaction-form.ts` do `main`): 5 falhas e 41 aprovações (46 testes). Depois da correção: 46 aprovações.

### Reprodução (falham antes, passam depois)

`transaction-form.spec.ts`, "should not save a value with more than 13 integer digits: %s":

| Valor digitado | Antes da correção | Depois |
| --- | --- | --- |
| `99999999999999,99` | `close` chamado (salvo alterado) | passa |
| `12345678901234,56` | `close` chamado | passa |
| `10000000000000` | `close` chamado | passa |
| `999999999999999,99` | `close` chamado (salvo alterado) | passa |
| `00000000000001` (P1) | `close` chamado | passa |

O "12345678901234,56", o "10000000000000" e o "00000000000001" garantem que a recusa é pelo tamanho, e não só nos
casos em que o double erra.

### Guarda (passa antes e depois)

- "should save the largest value with 13 integer digits exactly": "9999999999999,99" fecha o diálogo com `value`
  `-9999999999999.99`, e o `JSON.stringify` do valor é `"-9999999999999.99"` (protege o limite superior e a exatidão da
  conversão).

Os testes existentes do valor (`12,345`, `8.600,00`, `-12`, `12.50`, `8600`, a edição de "186,42" e de "8600,00")
continuam passando sem mudança. Nenhum teste existente foi alterado, ignorado ou apagado.

## Verificação local

Em `front/`:

- `npx ng test --watch=false` → 32 arquivos, 363 testes, todos passando (eram 357; +6 do `transaction-form`).
- `npm run lint` → "All files pass linting."
- `npm run build` → sem erro nem aviso de budget; Initial total 633,08 kB (igual à referência).
- `npx prettier --write` na pasta `transaction-form/`: nenhum arquivo mudou; nenhum arquivo não tocado mudou.

## Specs ajustadas (P2 = A)

`front/specs/003-gestao-de-transacoes/spec.md`:

- História 2, cenário 5: "(desvio conhecido: hoje o zero e os valores com mais de 13 algarismos na parte inteira
  passam; ver FR-014)" passou a "(desvio conhecido: hoje o zero passa; ver FR-014)".
- Casos-limite, item do zero e do limite do valor: ficou a regra esperada; saíram "Desvio conhecido: hoje o formulário
  não limita o valor.", o trecho do princípio II sobre os valores com 14 ou mais algarismos, os exemplos
  ("99999999999999,99" salvo como 99.999.999.999.999,98 e "999999999999999,99" como 1.000.000.000.000.000,00) e
  "A partir de cerca de 10^16 … o formulário aceita o valor e a API o recusa…". Ficaram o desvio do zero (que passa e a
  API recusa, sem bloquear) e o desvio do back sobre a faixa e a escala.
- FR-014: "hoje o formulário aceita o zero e valores de qualquer tamanho" passou a citar só o zero, com a nota de que
  ele não bloqueia; o trecho do princípio II sobre os valores com 14 ou mais algarismos deu lugar a "O limite do valor
  foi corrigido em `front/bugs/valor-com-muitos-algarismos/`", junto da frase da data digitada.
- Premissas (regras de negócio): "(FR-014, em que a recusa do zero e o limite do valor são desvios conhecidos)" passou
  a "(FR-014, em que a recusa do zero é um desvio conhecido)".
- Premissas (lista de desvios): "a recusa do zero e o limite do valor (FR-014)" passou a "a recusa do zero (FR-014), o
  limite do valor (FR-014, corrigido em `front/bugs/valor-com-muitos-algarismos/`)".
- Os parágrafos alterados foram requebrados em ~120 colunas.
- Ficaram como estavam: os Esclarecimentos (histórico) e a frase "Bloqueiam a entrega desta feature…" das Premissas,
  que ainda cita os valores com 14 ou mais algarismos entre os bloqueios, como no `data-digitada-como-mes-dia-ano`; o
  bloqueio se encerra pelo critério de conclusão da mesma frase (`test.md` `verified` e `/speckit-converge`).

## Desvios da avaliação

- Na lista de bloqueios das Premissas (frase "Bloqueiam a entrega desta feature…", linhas 633-634 da avaliação), a
  avaliação previa seguir o padrão da data digitada; por instrução do orquestrador, essa frase ficou como histórico,
  sem mudança, que é o mesmo padrão adotado no `data-digitada-como-mes-dia-ano`.
- Os testes de reprodução ganharam o caso "00000000000001", que cobre a decisão P1.
- Fora isso, nenhum: a correção seguiu a opção preferida, nos arquivos e com os testes previstos.

## Pendências

- Conferir num navegador a digitação de "99999999999999,99" e de "9999999999999,99" (no `/speckit-bug-test` ou à mão).
- `plan.md`, `research.md`, `tasks.md` e checklists da 003 podem ainda citar o desvio como pendente; a reavaliar no
  `/speckit-converge` (FR-014, cenário 5 da História 2 e Casos-limite).
- A recusa do zero no formulário e a validação de faixa e escala na API seguem como desvios conhecidos, nos fluxos de
  bugs do front e do back.
- Próximo passo: `/speckit-bug-test project=front slug=valor-com-muitos-algarismos`.
