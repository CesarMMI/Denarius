---
name: feature
description: Use when a change alters application behavior, the API contract, data or security, or is a refactoring (anything beyond a trivial change or a bug). Runs the Denarius feature flow: spec, plan, tasks, gates, implementation and review.
---

# Fluxo de feature

Este fluxo vale para o `back/` ou o `front/`, um projeto por vez. Os `AGENTS.md` (raiz e do projeto) são a constituição: leia os dois antes de começar. Artefatos novos são escritos em português (pt-BR) e ficam enxutos: cite `arquivo:linha`, nome de teste e hash de commit em vez de copiar código, diffs ou saída de comandos, e apague as seções do template que não se aplicam.

## 1. Preparar

1. **Projeto**: onde a feature acontece. Trabalho nos dois projetos faz o ciclo completo no back e depois no front, com uma spec em cada. O front só consome o que `back/docs/api.md` e o OpenAPI já oferecem: se faltar algo, a spec diz isso e o usuário aprova antes de o back mudar.
2. **Feature existente**: leia os nomes e as specs em `<projeto>/docs/features/`. Se uma cobre claramente o assunto, trabalhe nela: edite o `spec.md`, o `plan.md`, o `research.md` e o `contracts/` dela, com requisitos, critérios e tarefas novos numerados em sequência, e **não crie pasta**. Só crie uma feature nova se o usuário pedir explicitamente ou se você avaliar que o assunto merece uma; nesse caso, confirme com o usuário antes de criar a pasta.
3. **Número e nome** (só para uma feature nova): o próximo `NNN` é o maior número em `<projeto>/docs/features/` mais 1, com o nome em kebab-case e sem acentos (`005-metas-de-orcamento`). Crie a branch `feat/<nome>`.
4. **Tamanho**, confirmado com o usuário:
   - **Pequena**: um só projeto, sem mudar contrato, dados nem segurança, e cabe numa tela, num endpoint ou numa regra.
   - **Grande**: o resto (contrato, dados, segurança, refatoração ampla ou mais de um projeto). Na dúvida, grande, ou pergunte.

## 2. Spec

Numa feature nova, copie `<projeto>/.claude/templates/spec.md` para `<projeto>/docs/features/NNN-nome/spec.md` e preencha. A spec diz o quê e por quê, sem implementação, com requisitos testáveis (`FR-001`) e critérios de sucesso mensuráveis (`SC-001`), incluindo metas de desempenho que importam ao usuário.

Cada ambiguidade que muda escopo, comportamento, contrato, dados ou segurança vira `[NEEDS CLARIFICATION: …]` e vai ao usuário (no máximo 5 perguntas por vez, cada uma com as opções, as consequências e uma recomendação). Não decida sozinho. Apague o marcador ao resolver.

## 3. Plan e tarefas

Numa feature nova, copie `<projeto>/.claude/templates/plan.md` para `plan.md` na mesma pasta. Antes, leia os documentos vivos do projeto (`docs/dominio/` e `docs/api.md` no back, `docs/design.md` no front). O plan faz a conferência com as regras do projeto, registra as decisões e inclui as tarefas (`T001`):

- Os testes vêm antes da implementação que eles cobrem.
- A última tarefa atualiza os documentos vivos com o que mudou de permanente (obrigatória).
- Contrato novo ou alterado vai em `contracts/` na pasta da feature.
- `research.md` só se a pesquisa for longa demais para as decisões do plan.

## 4. Portões

Antes de cada portão, faça a revisão: se a feature toca contrato, dados ou segurança, chame o agente `reviewer` (prompt com projeto, pasta, momento e decisões do usuário); nos demais casos, releia os artefatos contra os `AGENTS.md`. Corrija os achados CRITICAL e HIGH. Depois de 2 rodadas com achados desses níveis, o usuário decide.

Em cada portão, apresente um resumo do que foi produzido, os achados da revisão (resolvidos e pendentes), as decisões tomadas e os riscos, e **espere a aprovação**.

| Tamanho | Portões |
| --- | --- |
| Pequena | 1: spec e plan aprovados juntos |
| Grande | 2: primeiro a spec; depois o plan com as tarefas |

## 5. Implementar

Só depois da aprovação. TDD: teste que falha, o mínimo de código para passar, refatoração com os testes verdes. Marque cada tarefa `[X]` ao concluí-la. Nenhum teste existente é apagado, ignorado ou enfraquecido para a suíte passar.

Dúvida que surgir (conflito entre regras, teste que quebra fora do escopo, necessidade de mexer no outro projeto) volta ao usuário antes de seguir, com opções e uma recomendação.

## 6. Revisão final e entrega

1. Rode a suíte do projeto (`dotnet test` no back; no front, `ng test`, lint e build), com os comandos do `AGENTS.md` dele.
2. Chame o agente `reviewer` para o fim do implement. Corrija os CRITICAL e HIGH e repita, com o mesmo limite de 2 rodadas.
3. Confirme que os documentos vivos foram atualizados (ou que nada permanente mudou).
4. Entregue um resumo ao usuário. Não faça commit sem ele pedir.
