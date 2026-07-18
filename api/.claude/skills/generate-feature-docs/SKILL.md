---
name: generate-feature-docs
description: >-
  Gera a documentação de uma feature no padrão spec.md/plan.md/tasks.md dentro
  de specs/features/{FEATURE_ID}/. Use quando o usuário pedir para documentar
  uma feature nova, criar uma spec, planejar uma feature, ou mencionar
  spec.md, plan.md, tasks.md ou specs/features/. Também pode ser chamado
  diretamente via /generate-feature-docs <feature-id> <descrição curta>.
license: MIT
---

# Generate Feature Docs

Gera a documentação de uma feature deste projeto, separada em três arquivos
dentro de `specs/features/{FEATURE_ID}/`. Cada arquivo tem um papel
exclusivo — não repita conteúdo entre eles.

## Entrada

Os argumentos passados ao skill devem conter `{FEATURE_ID}` e uma descrição
funcional curta, ex.: `/generate-feature-docs account-export "Exportar
transações de uma conta em CSV"`.

- Se `{FEATURE_ID}` ou a descrição não vierem nos argumentos, pergunte ao
  usuário antes de prosseguir — não invente nenhum dos dois.
- `{FEATURE_ID}` deve ser um slug curto em kebab-case (ex.: `account-export`,
  `recurring-transactions`).

## Antes de escrever

Leia, nesta ordem:

1. `specs/entities/*.md` — entidades de domínio envolvidas na feature.
2. `specs/ARCHITECTURE.md` — decisões técnicas globais do projeto. **Nunca
   repita o conteúdo desse arquivo** nos três documentos gerados, apenas
   referencie.
3. Código-fonte real da feature, se ela já estiver implementada (para o
   `plan.md` refletir a implementação de fato, não uma suposição).
4. Outras specs em `specs/features/` que sejam relacionadas, para manter
   consistência de estilo e evitar contradições.

Se a feature ainda não foi implementada, escreva `plan.md` como uma proposta
técnica a ser validada com o usuário antes do `tasks.md` virar trabalho real.

## Os três arquivos

Crie os arquivos em `specs/features/{FEATURE_ID}/`.

### 1. `spec.md` — O QUÊ e POR QUÊ (nunca COMO)

```markdown
# {FEATURE_ID}

{Outcome em 1-2 frases: o que essa feature entrega, do ponto de vista de quem usa}

> Entidade(s) de referência: `specs/entities/<entidade>.md`

## Entrada
- ...

## Happy path
- ...

## Erros
- ...

## Fora de escopo
- {o que essa feature explicitamente NÃO cobre — evita que o agente "invente" escopo}
```

Regra: se você sentir vontade de mencionar uma classe, camada, biblioteca ou
padrão de código aqui, pare — isso é `plan.md`, não `spec.md`.

### 2. `plan.md` — decisões técnicas específicas desta feature

```markdown
# {FEATURE_ID} — Plan

## Abordagem técnica
{Camadas/componentes envolvidos e o fluxo entre eles, específico desta feature}

## Decisões locais
{Decisões que valem só para esta feature — não promova para ARCHITECTURE.md
a menos que se repita em outras features}

## Edge cases e como são tratados
{Casos que não são óbvios a partir do spec.md}

## Alternativas descartadas
{Se relevante: o que foi considerado e por que não foi escolhido}
```

Regra: se a decisão já está em `ARCHITECTURE.md`, apenas referencie — não
copie. Se uma decisão aqui está se repetindo em 3+ features, é sinal de que
ela deveria subir para `ARCHITECTURE.md`.

### 3. `tasks.md` — decomposição em tarefas verificáveis

```markdown
# {FEATURE_ID} — Tasks

- [ ] {Tarefa 1}
      Verificação: {como saber que está pronta — teste específico, comportamento observável}
- [ ] {Tarefa 2}
      Verificação: ...
```

Regra de granularidade: uma tarefa por camada/arquivo tocado, não por linha de
código. Se uma tarefa não tem um critério de verificação claro, ela está
granular demais ou vaga demais — ajuste.

## Ao terminar

Liste os três arquivos criados e aponte, em uma frase, se `plan.md` é uma
proposta a validar (feature não implementada) ou um retrato do código real
(feature já implementada).
