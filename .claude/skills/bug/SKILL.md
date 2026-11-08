---
name: bug
description: Use when the application behaves differently from what the spec or the user expects. Runs the Denarius bug flow: diagnosis, user gate, test-first fix and verification, recorded in a single docs/bugs/<slug>.md.
---

# Fluxo de bug

Leia o `AGENTS.md` da raiz e o do projeto. O relatório é um arquivo só, `<projeto>/docs/bugs/<slug>.md`, no projeto da causa raiz, com no máximo ~30 linhas e em português (pt-BR). Cite `arquivo:linha`, nome de teste e hash de commit em vez de copiar código ou saída.

Se o diagnóstico mostrar que não é bug, e sim comportamento novo, pare e siga a skill `feature`. Se a correção exigir os dois projetos, cada um ganha o seu relatório, com o back primeiro.

## 1. Diagnóstico (sem alterar código)

Crie o arquivo a partir de `<projeto>/.claude/templates/bug.md`, com `<slug>` em kebab-case e sem acentos. Preencha **Sintoma** (esperado vs observado e como reproduzir), **Causa raiz** (`arquivo:linha` e por quê) e **Correção proposta**.

## 2. Portão

Apresente o diagnóstico e a correção proposta ao usuário e **espere a aprovação**. O diagnóstico não tem revisor próprio: quem o revisa é o usuário.

## 3. Correção

1. Escreva primeiro o teste que reproduz o bug e rode-o: ele precisa falhar.
2. Faça a correção mínima até o teste passar.
3. Preencha **Correção aplicada** com o nome do teste e o commit (se o usuário pediu o commit).

Nenhum teste existente é apagado, ignorado ou enfraquecido.

## 4. Verificação e revisão

1. Rode a reprodução original e a suíte completa do projeto; no front, também o lint e o build. Preencha **Verificação**.
2. Chame o agente `reviewer` para o fim do bug-fix (prompt com projeto, arquivo do bug, momento e decisões do usuário). Corrija os CRITICAL e HIGH e repita, no máximo 2 rodadas; depois o usuário decide.
3. Entregue um resumo ao usuário.
