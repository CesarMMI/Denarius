# Avaliação do bug: valores com 14 ou mais algarismos inteiros são salvos diferentes do digitado

- **Slug**: valor-com-muitos-algarismos
- **Projeto**: front
- **Criada em**: 2026-10-07
- **Origem**: texto colado (relato do orquestrador, a partir do `/speckit-converge` da feature 003)
- **Veredito**: válido
- **Severidade**: high

## Relato (resumido)

> No formulário de transação (`transaction-form.ts`), o valor digitado é convertido com `parseFloat`. Com 14 ou mais
> algarismos, a precisão de `number` (double) altera o valor salvo. Exemplo: "12345678901234,56" vira outro número.
> Isso fere o princípio II, que trata da corrupção de dados financeiros. Conferir o formato aceito hoje, o limite de
> casas decimais, a serialização para a API, o maior valor representável sem perda e a exibição (pipe de moeda).
> Fora deste bug: a aceitação do valor zero, outro desvio, não bloqueante.

## Sintoma

O campo "Valor" aceita qualquer quantidade de algarismos na parte inteira. A partir de 14 algarismos inteiros com
centavos, o número enviado à API pode diferir do digitado, sem aviso: "99999999999999,99" é salvo como
99.999.999.999.999,98. O esperado (FR-014 da 003) é recusar, com "Informe um valor válido" e o diálogo aberto, os
valores com mais de 13 algarismos na parte inteira (acima de R$ 9.999.999.999.999,99).

## Requisitos violados

- `front/specs/003-gestao-de-transacoes/spec.md`: FR-014 (linhas 451-460), cenário 5 da História 2 (204-208),
  Casos-limite (350-360) e Esclarecimentos (31-34, 96-100 e 110-112; o segundo e o terceiro classificam o desvio como
  violação do princípio II que bloqueia a entrega da 003).
- `AGENTS.md` da raiz, princípio II (Segurança): o dado financeiro gravado difere do digitado, sem aviso.

## Reprodução

Reproduzido em 2026-10-07, no `main` (`2358c0c`), com um spec temporário
(`src/app/transactions/components/transaction-form/zz-repro-temp.spec.ts`, rodado com
`npx ng test --watch=false --include=...` e apagado em seguida; a árvore de trabalho ficou limpa). O spec criava o
`TransactionForm` para uma nova transação (tipo "Saída"), digitava o valor (evento `input`), enviava o formulário e
comparava o `value` passado ao `close` do diálogo, serializado com `JSON.stringify` (como o `HttpClient` faz), com o
texto digitado.

| Digitado | Enviado à API (`value` no JSON) | Resultado |
| --- | --- | --- |
| `9999999999999,99` (13 inteiros) | `-9999999999999.99` | igual |
| `12345678901234,56` (14 inteiros, exemplo do relato) | `-12345678901234.56` | igual (por acaso, ver abaixo) |
| `99999999999999,99` (14 inteiros) | `-99999999999999.98` | **diferente** |
| `999999999999999,99` (15 inteiros) | `-1000000000000000` | **diferente** |
| `9999999999999999,99` (16 inteiros) | `-10000000000000000` | **diferente**: o valor digitado cabe no `numeric(18,2)` do back, mas o enviado (10^16, com 17 inteiros) passa do limite, e a API responde com o erro inesperado |

Complemento, no Node (mesmo V8, mesma expressão `parseFloat(value.replace(',', '.'))` seguida de `JSON.stringify`),
com 200.000 valores aleatórios com centavos para cada tamanho da parte inteira:

| Algarismos inteiros | Valores alterados |
| --- | --- |
| 13 | 0% |
| 14 | 11,7% |
| 15 | 83,2% |
| 16 | 98,3% |

O exemplo do relato ("12345678901234,56") não reproduz: com 14 algarismos, a maior parte dos valores sobrevive, e o
erro aparece só em parte deles, como "99999999999999,99". O defeito é o mesmo: a partir de 14 algarismos inteiros com
centavos, o resultado deixa de ser garantido.

Passos manuais equivalentes:

1. Em `/transactions`, abrir "Nova transação".
2. Digitar "99999999999999,99" no valor e salvar: o formulário aceita.
3. A transação aparece na lista como R$ -99.999.999.999.999,98.

## Respostas às verificações pedidas

- **Formato aceito hoje** (`transaction-form.ts:54`): `Validators.pattern(/^\d+([.,]\d{1,2})?$/)`. Só algarismos, com
  vírgula ou ponto decimal e até duas casas; sem sinal e sem separador de milhar ("8.600,00" é recusado, como pede a
  spec). A parte inteira não tem limite de tamanho: esse é o defeito.
- **Casas decimais**: no máximo duas, já garantidas pela regex ("12,345" é recusado; teste existente em
  `transaction-form.spec.ts:97`).
- **Conversão e envio** (`transaction-form.ts:70-75`): `parseFloat(value.replace(',', '.'))`, com o sinal aplicado pelo
  tipo, vai como `number` em `TransactionInput.value` (`types/transaction.ts:5,11`), e o `HttpClient` o serializa com
  `JSON.stringify` (`transactions.service.ts:28,32`), que escreve a menor representação decimal do double.
- **Contrato** (`back/specs/002-transaction-management/contracts/transactions-api.yaml:213-215 e 232-234`): `value` é
  `type: number, format: decimal`, um número JSON. O contrato não prevê o valor como string; mudar isso seria uma
  mudança de contrato, no back, fora deste bug. O back lê o número JSON como `decimal` (lê o texto, sem passar por
  double) e grava em `numeric(18,2)` (`TransactionConfiguration.cs:24`): até 16 algarismos inteiros. Acima disso a
  gravação falha com o erro inesperado; o domínio só recusa o zero (`Transaction.cs:60-63`). A falta de validação de
  faixa e escala na API é desvio conhecido do back, registrado na 003, que não bloqueia esta feature.
- **Maior valor sem perda no front**: um double guarda exatamente qualquer número decimal de até 15 algarismos
  significativos, e o `JSON.stringify` devolve o mesmo texto. Com duas casas decimais, isso dá 13 algarismos inteiros:
  até 9.999.999.999.999,99, exatamente o limite da FR-014. Inteiros sem centavos seriam exatos até 2^53 − 1
  (9.007.199.254.740.991), mas o limite precisa valer com centavos. O limite de 13 também fica abaixo do limite do back
  (16 inteiros), então tudo o que o front aceitar o back consegue gravar.
- **Exibição**: a lista usa `{{ transaction.value | currency: 'BRL' }}` (`transactions-table.html:27`) e os relatórios,
  `Intl.NumberFormat` (`reports/chart-formats.ts:4`). Os dois recebem o `number` que veio do JSON da API, então têm a
  mesma limitação, mas só para valores de mais de 15 algarismos significativos. Com o limite de 13 inteiros no
  formulário, o valor de cada transação salva pelo front é exibido sem perda (os totais dos relatórios são somas e
  podem passar desse tamanho; ver Riscos). Valores maiores só existiriam se gravados antes da
  correção ou por outro cliente da API (ver Riscos). A edição pré-preenche o valor com `toFixed(2)`
  (`transaction-form.ts:52`), que também é exato até 13 inteiros.

## Caminhos de código suspeitos

- `front/src/app/transactions/components/transaction-form/transaction-form.ts:54`: a regex do valor não limita o
  tamanho da parte inteira (`\d+`).
- `front/src/app/transactions/components/transaction-form/transaction-form.ts:70`: `parseFloat` converte para double;
  com mais de 15 algarismos significativos o resultado é arredondado para o double mais próximo. Não é um erro em si:
  dentro do limite da FR-014 a conversão é exata.
- `front/src/app/transactions/types/transaction.ts:5,11` e `services/transactions.service.ts:28,32`: o valor viaja como
  `number`, conforme o contrato.

## Hipótese de causa raiz

Confiança: **alta** (reproduzida no componente e explicada pela precisão do double).

O formulário converte o texto em `number` (double, 53 bits de mantissa), que só garante 15 algarismos decimais
significativos. Como a validação não limita a parte inteira, valores com 14 ou mais algarismos inteiros e centavos
(16 ou mais significativos) chegam ao `parseFloat` e podem ser arredondados para outro número, que vai para a API e é
gravado sem aviso. A FR-014 já fixa o limite de 13 algarismos inteiros justamente porque é o maior tamanho que o double
guarda sem perda com centavos; o código só não o aplica.

**É bug, e não comportamento novo**: a FR-014 já exige a recusa e registra o atual como desvio conhecido, encaminhado
ao fluxo de bugs.

## Correção proposta

**Preferida: limitar a parte inteira a 13 algarismos na regex do valor, no front, sem mudança no back nem no
contrato.**

1. Em `transaction-form.ts:54`, trocar `/^\d+([.,]\d{1,2})?$/` por `/^\d{1,13}([.,]\d{1,2})?$/`. A mensagem
   continua "Informe um valor válido" (o `<mat-error>` existente, `transaction-form.html:14`).
2. A conversão com `parseFloat` fica: dentro do limite ela é exata (0% de alterações em 200.000 valores de 13
   algarismos, e a regra dos 15 significativos). Um comentário curto, em inglês, junto da regex explica por que o
   limite é 13 (valores maiores com centavos não cabem num `number` sem perda).
3. O zero não muda aqui (outro desvio, fora deste bug): a regex nova continua aceitando "0" e "0,00".
4. O input do valor não tem `maxlength` e não precisa de um: a regex basta, e um `maxlength` cortaria o texto em
   silêncio, em vez de mostrar "Informe um valor válido".

**Fora do escopo do front**: a correção "completa" de enviar o valor como string ou decimal exato, sem passar por
double, exigiria mudar o contrato (`value` é `number`) e o back, com o ciclo completo do back antes, pelo princípio IV.
Não é necessária: o limite de 13 algarismos da FR-014 já garante que o double guarda o valor sem perda, e o back aceita
até 16 inteiros. A validação de faixa e escala na API continua como desvio conhecido do back, no fluxo de bugs dele.

**Alternativas:**

- (B) Validar pelo número convertido (por exemplo, um validador que recusa `Math.abs(n) >= 1e13`). Desvantagem: mais
  código que a regex e o mesmo resultado; e um validador sobre o número convertido precisaria ainda conferir o texto
  para não depender do próprio arredondamento. Não recomendada.
- (C) Aceitar zeros à esquerda além dos 13 algarismos (`/^0*\d{1,13}([.,]\d{1,2})?$/`), para que, por exemplo,
  "00000000000001" (14 caracteres, valor 1) passe. Desvantagem: regra menos óbvia para um caso sem uso real. Ver P1.
- (D) Mandar o valor como string ou usar uma biblioteca decimal. Exige mudança de contrato e de back (acima) ou
  dependência nova sem necessidade. Descartada.

**Arquivos que devem mudar:**

- `front/src/app/transactions/components/transaction-form/transaction-form.ts` (a regex e o comentário);
- `front/src/app/transactions/components/transaction-form/transaction-form.spec.ts` (testes);
- `front/specs/003-gestao-de-transacoes/spec.md` (notas de desvio; ver P2).

Nenhuma mudança no back, nos services, nos tipos, no contrato da API, no template ou nas dependências.

**Testes de regressão previstos** (em `transaction-form.spec.ts`, no `describe('validating')`, no padrão do
`it.each(['8.600,00', '-12'])` existente; escritos antes da correção; os marcados com (falha) falham hoje):

1. (falha) `it.each` "should not save the value %s" estendido, ou um novo `it.each` "should not save a value with more
   than 13 integer digits: %s", com "99999999999999,99", "12345678901234,56", "10000000000000" e
   "999999999999999,99" → `close` não chamado e `errors()` igual a `['Informe um valor válido']`. Hoje os quatro são
   salvos (o primeiro e o último já alterados). O "12345678901234,56" e o "10000000000000" garantem que a recusa é
   pelo tamanho, e não só nos casos em que o double erra.
2. "should save the largest value with 13 integer digits exactly": digitar "9999999999999,99" e salvar → `close`
   chamado com `value` `-9999999999999.99`, e `JSON.stringify` do valor igual a `"-9999999999999.99"` (já passa;
   protege o limite superior e a exatidão da conversão). Opcionalmente o mesmo com "1234567890123,45".
3. Os testes existentes do valor (`12,345`, `8.600,00`, `-12`, `12.50`, `8600`, a edição de "186,42" e de "8600,00")
   continuam passando sem mudança.

Verificação final: `npx ng test --watch=false`, `npm run lint` e `npm run build` em `front/`.

## Riscos e considerações

- **Transações já gravadas com 14 ou mais algarismos inteiros** (só possíveis antes da correção ou por outro cliente da
  API): a exibição delas pode mostrar um valor arredondado, e o diálogo de edição as pré-preenche com um valor que a
  nova regra recusa, então o usuário precisa corrigir o valor para salvar. É o comportamento correto (o formulário não
  regrava um valor que não consegue representar), e é improvável em finanças pessoais.
- **Zeros à esquerda**: com a regex preferida, "00000000000001" é recusado, embora valha 1. Ver P1.
- **Limite mais estrito que o back**: o front recusa de 14 a 16 algarismos inteiros, que o back aceitaria; é o limite
  da FR-014 e o único que o `number` garante. A validação de faixa e escala na API fica para o fluxo de bugs do back.
- **Zero**: continua aceito pelo formulário (outro desvio, não bloqueante, fora deste bug).
- **Totais dos relatórios**: as somas e agregações exibidas nos relatórios podem passar de 15 algarismos
  significativos mesmo com cada transação dentro do limite, e então ser exibidas com arredondamento. É um caso fora
  deste bug (o formulário não grava nada diferente do digitado) e improvável em finanças pessoais.
- **Escopo**: só o front. Nenhuma mudança no back, no contrato da API, em dados ou em dependências.
- **Specs (desvios conhecidos)**: depois da correção, deixam de valer na 003 (ver P2):
  - o cenário 5 da História 2 (207-208): o parêntese passa a citar só o zero ("hoje o zero passa; ver FR-014");
  - os Casos-limite (350-360): sai "Desvio conhecido: hoje o formulário não limita o valor." e a descrição dos valores
    com 14 ou mais algarismos salvos diferentes do digitado e de 10^16 aceito pelo formulário; ficam a regra esperada,
    o desvio do zero (que passa e a API recusa) e o desvio do back sobre faixa e escala;
  - a FR-014 (456-460): "hoje o formulário aceita o zero e valores de qualquer tamanho" passa a citar só o zero, e o
    trecho sobre os valores com 14 ou mais algarismos e o princípio II dá lugar à menção "o limite do valor foi
    corrigido em `front/bugs/valor-com-muitos-algarismos/`";
  - as Premissas (592-593): "em que a recusa do zero e o limite do valor são desvios conhecidos" passa a citar só o
    zero; a lista de desvios (620) registra o limite do valor como corrigido em `front/bugs/valor-com-muitos-algarismos/`,
    e a de bloqueios (633-634) segue o mesmo padrão adotado para a data digitada;
  - os Esclarecimentos (31-34, 96-100 e 110-112) são histórico e ficam.

## Decisões do usuário (portão do diagnóstico, 2026-10-07)

A revisão independente não achou nada CRITICAL, HIGH nem MEDIUM; os achados LOW (a citação de `types/transaction.ts`,
a linha de 16 algarismos da reprodução, o alcance da exibição sem perda, os totais dos relatórios e o `maxlength`)
foram corrigidos nesta versão. O usuário aprovou o diagnóstico e decidiu:

- **P1**: (A), recusar os valores que passem de 13 algarismos inteiros, inclusive só por zeros à esquerda
  (`/^\d{1,13}([.,]\d{1,2})?$/`).
- **P2**: (A), as notas de desvio da 003 saem no próprio `/speckit-bug-fix`, com registro no `fix.md`, mantendo as do
  zero e da validação de faixa e escala no back.

## Perguntas (respondidas)

- **P1 — Zeros à esquerda**: [NEEDS CLARIFICATION: um valor com mais de 13 algarismos inteiros só por zeros à esquerda,
  como "00000000000001", deve ser aceito?]
  - (A) Recusar: a regex conta os algarismos digitados (`^\d{1,13}…`). Consequência: regra simples e literal ("até 13
    algarismos na parte inteira"); o caso não tem uso real.
  - (B) Aceitar: ignorar os zeros à esquerda na contagem (`^0*\d{1,13}…`). Consequência: regex um pouco menos óbvia e
    mais um teste; nenhum ganho prático.
  - **Recomendação**: (A).
- **P2 — Specs 003**: [NEEDS CLARIFICATION: ajustar a spec no próprio bug-fix ou numa revisão à parte?]
  - (A) No `/speckit-bug-fix`, com registro no `fix.md`, retirando as notas listadas em "Riscos e considerações" e
    mantendo o desvio do zero e o do back. Consequência: a spec deixa de descrever um desvio que não existe mais.
  - (B) Deixar a spec como está e só citá-la no `fix.md`.
  - **Recomendação**: (A), como nos bugs `calendarios-em-ingles`, `consulta-repetida-sem-mudanca` e
    `data-digitada-como-mes-dia-ano`.
