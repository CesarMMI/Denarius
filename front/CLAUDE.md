# CLAUDE.md — Denarius Frontend

Frontend do Denarius: Angular 21 (standalone components, signals), Angular
Material, Vitest. Este arquivo é um índice — a referência técnica vive nos
documentos abaixo, não aqui. Não repita o conteúdo deles nesta página.

## Documentação

| Documento | Conteúdo |
|---|---|
| [`specs/ARCHITECTURE.md`](specs/ARCHITECTURE.md) | Decisões técnicas globais do frontend (state management, estrutura de pastas, roteamento). **Ainda não existe** — criar quando a primeira feature trouxer uma decisão que valha para o projeto todo |
| [`specs/features/<feature>/`](specs/features/) | Spec (`spec.md`), plano técnico (`plan.md`) e tarefas (`tasks.md`) de cada tela/fluxo existente. Ainda vazio — projeto é boilerplate `ng new` / `ng add @angular/material`, sem código de feature |

Este projeto **não referencia a estrutura interna da API** (`../api/...`) na
sua documentação — isso acoplaria o front a detalhes que podem mudar
independentemente. Contrato de dados (campos, endpoints, regras de erro) que
a UI precisa conhecer deve ser perguntado ao desenvolvedor quando não estiver
claro, não inferido de outro projeto/pasta.

## Skills disponíveis

| Skill | Uso |
|---|---|
| `generate-feature-docs` (`.claude/skills/generate-feature-docs/SKILL.md`) | Gera `spec.md`/`plan.md`/`tasks.md` de uma tela/fluxo novo em `specs/features/{FEATURE_ID}/`. Invocar via `/generate-feature-docs <feature-id> <descrição>` |
| `angular-developer` (`.claude/skills/angular-developer/SKILL.md`) | Geração de código Angular e orientação arquitetural (signals, forms, DI, routing, SSR, acessibilidade, testes) |
| `angular-best-practices` (`.claude/skills/angular-best-practices/SKILL.md`) | Convenções de TypeScript/Angular/acessibilidade a seguir em qualquer código novo |

## Regras para o Agente

### Antes de qualquer tarefa
1. Executar `ng build` — se falhar, **parar e reportar o erro** antes de qualquer alteração.
2. Executar `ng test` — se falhar, **parar e reportar** quais testes estão quebrando.

### Durante a tarefa
- Trabalhar em **uma tarefa por vez**.
- Nunca declarar "done" sem `ng build` e `ng test` passando sem erros.
- Ao adicionar uma tela/fluxo novo: usar a skill `generate-feature-docs` **antes** de implementar (ver tabela acima).
- Seguir as convenções de `angular-best-practices` e `angular-developer` (standalone components, signals, `OnPush`, Reactive Forms, `inject()`).

### Documentação faltante

Se perceber que uma feature não possui `specs/features/<feature>/` correspondente:

1. Use a skill `generate-feature-docs` antes de implementar.
2. Se o contrato de dados da API consumida por essa tela (campos, endpoint, regras de erro) não estiver claro, **pergunte ao desenvolvedor** — não procure em outro projeto/pasta (ex.: `api/`) para inferir a resposta.
3. Só então prossiga com a implementação.

**Nunca implemente uma tela sem que exista documentação correspondente.**

### Testes
Vitest via `ng test`. Specs ficam ao lado do arquivo testado (`*.spec.ts`), seguindo o padrão de `src/app/app.spec.ts`.

### Ao terminar
Descrever:
- O que foi feito (arquivos criados/modificados)
- O que ainda está pendente ou requer atenção do desenvolvedor

## O que NÃO fazer

- **Não instalar pacotes npm** sem confirmar com o usuário.
- **Não usar NgModules** — o projeto usa exclusivamente standalone components.
- **Não referenciar caminhos de outro projeto (ex.: `../api/...`) na documentação do front** — se um contrato de dados da API for necessário e não estiver claro, perguntar ao desenvolvedor em vez de acoplar a documentação à estrutura interna da API.
- **Não usar `@HostBinding`/`@HostListener`, `ngClass`, `ngStyle` ou `mutate` em signals** — ver `angular-best-practices` para as alternativas.
- **Não criar arquivos fora da estrutura padrão do Angular CLI** (`src/app/...`) sem necessidade.
