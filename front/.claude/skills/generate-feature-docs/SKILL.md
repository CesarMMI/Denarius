---
name: generate-feature-docs
description: >-
  Gera a documentação de uma feature de frontend no padrão
  spec.md/plan.md/tasks.md dentro de specs/features/{FEATURE_ID}/. Use quando
  o usuário pedir para documentar uma tela, componente ou fluxo novo, criar
  uma spec, planejar uma feature, ou mencionar spec.md, plan.md, tasks.md ou
  specs/features/. Também pode ser chamado diretamente via
  /generate-feature-docs <feature-id> <descrição curta>.
license: MIT
---

# Generate Feature Docs

Gera a documentação de uma feature deste projeto (Angular), separada em três
arquivos dentro de `specs/features/{FEATURE_ID}/`. Cada arquivo tem um papel
exclusivo — não repita conteúdo entre eles.

## Entrada

Os argumentos passados ao skill devem conter `{FEATURE_ID}` e uma descrição
funcional curta, ex.: `/generate-feature-docs account-list "Listar as contas
do usuário com saldo e status"`.

- Se `{FEATURE_ID}` ou a descrição não vierem nos argumentos, pergunte ao
  usuário antes de prosseguir — não invente nenhum dos dois.
- `{FEATURE_ID}` deve ser um slug curto em kebab-case (ex.: `account-list`,
  `transaction-form`).
- `{FEATURE_ID}` não precisa espelhar 1:1 uma operação de backend: uma tela
  pode combinar múltiplos dados/ações, e isso é uma decisão deste projeto,
  não algo a inferir de outro repositório.

## Antes de escrever

Leia, nesta ordem:

1. `specs/ARCHITECTURE.md` deste projeto, se existir — decisões técnicas
   globais do frontend (state management, estrutura de pastas, roteamento).
   **Nunca repita o conteúdo desse arquivo** nos três documentos gerados,
   apenas referencie. Se o arquivo ainda não existir (projeto ainda é
   boilerplate `ng new`/`ng add @angular/material`), prossiga normalmente e
   sinalize ao usuário, ao final, que essas decisões ainda não estão
   documentadas.
2. `.claude/skills/angular-best-practices/SKILL.md` e
   `.claude/skills/angular-developer/SKILL.md` — convenções de código
   Angular a respeitar no `plan.md` (standalone components, signals,
   `OnPush`, Reactive Forms, etc.).
3. Código-fonte real da feature, se ela já estiver implementada (para o
   `plan.md` refletir a implementação de fato, não uma suposição).
4. Outras specs em `specs/features/` que sejam relacionadas, para manter
   consistência de estilo e evitar contradições.

**Não leia nem referencie caminhos fora deste projeto** (ex.: `../api/...`)
para inferir contrato de dados, campos, endpoints ou regras de erro. Se a
feature depende de um contrato externo (o que a API espera/retorna, mensagens
de erro, nome de campos) e essa informação não estiver disponível dentro do
projeto frontend, **pergunte ao usuário** antes de escrever `spec.md` —
não presuma e não tente descobrir sozinho olhando outro repositório/pasta.
Isso evita que a documentação do front fique acoplada à estrutura interna da
API, que pode mudar independentemente.

Se a feature ainda não foi implementada, escreva `plan.md` como uma proposta
técnica a ser validada com o usuário antes do `tasks.md` virar trabalho real.

## Os três arquivos

Crie os arquivos em `specs/features/{FEATURE_ID}/`.

### 1. `spec.md` — O QUÊ e POR QUÊ (nunca COMO)

```markdown
# {FEATURE_ID}

{Outcome em 1-2 frases: o que essa tela/fluxo entrega, do ponto de vista de
quem usa}

> Contrato de dados consumido: {descrever os campos/ação relevantes,
> conforme informado pelo usuário — se não informado, perguntar antes de
> preencher esta seção}

## Entrada
- {campos de formulário, parâmetros de rota, seleções do usuário}

## Happy path
- {o que o usuário vê e faz, passo a passo, sem mencionar componentes}

## Estados e erros
- {loading, vazio, sucesso, erro de validação, erro de servidor — sempre do
  ponto de vista do que aparece na tela}

## Fora de escopo
- {o que essa feature explicitamente NÃO cobre — evita que o agente
  "invente" escopo}
```

Regra: se você sentir vontade de mencionar um componente, serviço, rota,
biblioteca ou padrão de código aqui, pare — isso é `plan.md`, não `spec.md`.

### 2. `plan.md` — decisões técnicas específicas desta feature

```markdown
# {FEATURE_ID} — Plan

## Abordagem técnica
{Componentes/páginas envolvidos, rotas, serviços de API consumidos e o fluxo
de dados entre eles, específico desta feature}

## State management
{Signals/inputs/outputs usados, o que é estado local vs. derivado
(`computed`), e onde vive}

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
      Verificação: {como saber que está pronta — teste específico,
      comportamento observável, ou checagem de acessibilidade}
- [ ] {Tarefa 2}
      Verificação: ...
```

Regra de granularidade: uma tarefa por componente/serviço/arquivo tocado, não
por linha de código. Se uma tarefa não tem um critério de verificação claro,
ela está granular demais ou vaga demais — ajuste.

## Ao terminar

Liste os três arquivos criados e aponte, em uma frase, se `plan.md` é uma
proposta a validar (feature não implementada) ou um retrato do código real
(feature já implementada). Se `specs/ARCHITECTURE.md` ainda não existir,
avise o usuário que decisões de arquitetura do frontend ainda não estão
documentadas.
