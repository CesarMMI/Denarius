# Denarius

Aplicação de finanças pessoais. O repositório tem dois projetos independentes:

- `back/`: API REST em .NET 10 (Clean Architecture, EF Core, PostgreSQL). Regras em [`back/AGENTS.md`](back/AGENTS.md).
- `front/`: SPA em Angular 21 (Angular Material, Vitest). Regras em [`front/AGENTS.md`](front/AGENTS.md).

Este arquivo reúne as regras que valem para o repositório inteiro e o fluxo de trabalho obrigatório. Junto com `back/AGENTS.md` e `front/AGENTS.md`, ele forma a constituição do projeto. Os arquivos dos projetos só acrescentam regras; nunca afrouxam as daqui.

## Estrutura

| Caminho | Conteúdo |
| --- | --- |
| `compose.yaml` | Ambiente de desenvolvimento completo: inclui o `compose.yaml` de cada projeto e liga o `web` à `api`. É só orquestração; os projetos continuam independentes e cada um também sobe sozinho |
| `<projeto>/Dockerfile`, `<projeto>/compose.yaml` | Ambiente de desenvolvimento do projeto em Docker. Só desenvolvimento: imagens de produção ainda não existem e serão uma feature própria |
| `.claude/agents/reviewer.md` | O revisor independente (ver "Revisão") |
| `.claude/skills/feature`, `.claude/skills/bug` | Os fluxos de trabalho, abaixo |
| `<projeto>/.claude/skills/` | Skills específicas de cada projeto |
| `<projeto>/.claude/templates/` | Templates de `spec.md`, `plan.md` e `bug.md` do projeto |
| `<projeto>/docs/features/NNN-nome/` | `spec.md` e `plan.md` de cada feature (um assunto, como categorias, transações ou relatórios), numeradas por projeto (mais `contracts/` e `research.md` quando preciso). Mudanças posteriores no mesmo assunto entram na mesma pasta |
| `<projeto>/docs/bugs/<slug>.md` | Relatório de bug, no projeto da causa raiz |
| `back/docs/dominio/<entidade>.md`, `back/docs/api.md`, `front/docs/design.md` | Documentos vivos: o estado atual do domínio, da API e da identidade visual. Descrevem o que vale hoje, e não o histórico de cada feature |

## Princípios gerais

### I. Desenvolvimento guiado por testes (TDD)

- Todo comportamento novo ou alterado começa por um teste que falha. Depois vem o mínimo de código para ele passar e, com os testes verdes, a refatoração.
- Correção de bug começa por um teste que reproduz o bug e falha antes da correção.
- No `plan.md`, as tarefas de teste vêm antes das tarefas de implementação que elas cobrem.
- Nenhum teste existente é apagado, ignorado ou enfraquecido para a suíte passar.
- Uma mudança só está pronta quando a suíte completa do projeto afetado passa (os comandos estão no `AGENTS.md` de cada projeto).

_Por quê:_ os testes são a única proteção automática contra regressões, e escrevê-los primeiro garante que eles verificam o comportamento pedido, e não o código que já existe.

### II. Segurança

- Nenhum segredo real no repositório. Os únicos valores versionados são a connection string do banco local de desenvolvimento (`back/src/Denarius.WebAPI/appsettings.Development.json`) e a mesma senha local (`root`, como padrão de `${DB_PASSWORD:-root}`) no `back/compose.yaml`; fora isso, use user-secrets ou variáveis de ambiente.
- A API valida toda entrada externa. A validação do front serve à experiência do usuário e não substitui a do back.
- Acesso a dados só por consultas parametrizadas (LINQ/EF Core), nunca por SQL montado por concatenação.
- Erros não expõem detalhes internos (stack trace, SQL, caminhos); um erro inesperado chega ao cliente como uma mensagem genérica.
- As proteções dos frameworks continuam ligadas: sanitização do Angular, CORS restrito às origens configuradas e HTTPS fora de desenvolvimento. Desligar alguma delas exige justificativa no `plan.md` e aprovação do usuário.
- Dados financeiros não vão para logs.
- Dependência nova só com o motivo registrado no `plan.md`, mantida ativamente e sem vulnerabilidade conhecida (`npm audit`, `dotnet list package --vulnerable`).

_Por quê:_ o Denarius guarda dados financeiros pessoais; vazar ou corromper esses dados é o pior defeito possível.

### III. Performance

- Metas de desempenho que importam ao usuário entram na spec como critérios de sucesso mensuráveis, e o plan diz como serão cumpridas.
- O trabalho é proporcional ao que é exibido: filtros, agregações e ordenação acontecem na fonte dos dados, e não depois de carregar tudo.
- Nada de requisições, consultas ou cálculos repetidos sem necessidade.
- Otimização sem medição não justifica complexidade: primeiro meça, depois otimize.
- As regras concretas de cada stack estão no `AGENTS.md` de cada projeto.

_Por quê:_ a lentidão se acumula feature a feature; metas explícitas e medidas evitam que ela passe despercebida.

### IV. Contrato entre front e back

- A API REST do back é o único ponto de integração entre os projetos. Ela é descrita pelo OpenAPI que a própria API gera e por `back/docs/api.md` (convenções, erros e regras que o OpenAPI não diz); o contrato de cada feature fica em `back/docs/features/NNN-nome/contracts/`.
- Mudanças no contrato são aditivas por padrão. Uma mudança incompatível só sai com estratégia de versionamento e caminho de migração documentados.
- O front consome só o que o contrato já oferece. Se uma feature do front precisar de mudança na API, a spec diz isso, o usuário aprova explicitamente e o back entrega a mudança, com o fluxo de features completo, antes de o front depender dela.

_Por quê:_ um front que supõe um contrato diferente quebra em execução, e não no build.

### V. Escopo e simplicidade

- O trabalho de um projeto fica nele: uma tarefa do front não altera o back, nem o contrário, sem aprovação explícita do usuário.
- Faça a menor mudança que atende à spec. Siga os padrões que o projeto já usa antes de criar novos, e não crie abstrações para necessidades hipotéticas.

_Por quê:_ mudanças pequenas e previsíveis são mais fáceis de revisar, testar e desfazer.

## Fluxo de trabalho

Toda mudança implementada por um agente segue o fluxo do seu tamanho. Na dúvida sobre o tamanho, pergunte ao usuário.

| Tamanho | Quando | Fluxo |
| --- | --- | --- |
| Trivial | Só testes, estilo, textos, documentação ou configuração, sem mudar o comportamento da aplicação nem o contrato da API | TDD direto (princípio I), sem skill; entrega com suíte, lint e build passando e um resumo |
| Bug | O comportamento difere do que a spec ou o usuário espera | Skill `bug`: diagnóstico em `docs/bugs/<slug>.md`, **portão**, teste que falha, correção, verificação |
| Feature pequena | Comportamento novo ou alterado num só projeto, sem mudar contrato, dados nem segurança, que cabe numa tela, num endpoint ou numa regra | Skill `feature`: `spec.md` e `plan.md`, **um portão** (os dois aprovados juntos), implementação, revisão final |
| Feature grande | O resto: contrato, dados, segurança, refatoração ampla ou mais de um projeto | Skill `feature`: `spec.md`, **portão**, `plan.md` com as tarefas, **portão**, implementação, revisão final |

- Em cada portão, apresente um resumo do que foi produzido, os achados da revisão (resolvidos e pendentes), as decisões tomadas e os riscos, e espere a aprovação.
- Trabalho que envolve os dois projetos faz o ciclo completo no back primeiro e depois no front, com uma spec em cada projeto.
- **Nunca crie uma feature nova se já existe uma que claramente cobre o assunto.** Antes de começar, procure em `<projeto>/docs/features/`. Uma mudança num assunto já coberto (acrescentar um campo a um relatório, mudar a apresentação de uma lista que já tem feature) entra na `spec.md`, no `plan.md`, no `research.md` e no `contracts/` da feature existente, com requisitos, critérios e tarefas novos numerados em sequência; o fluxo e os portões do tamanho da mudança valem do mesmo jeito. Só se abre uma feature nova quando o usuário pedir explicitamente ou quando o agente avaliar que o assunto merece uma: nesse caso, o agente confirma com o usuário antes de criar a pasta.
- Mudança permanente no domínio, na API ou na identidade visual atualiza o documento vivo correspondente: toda feature tem uma tarefa para isso no `plan.md`.
- A entrega termina com a suíte de testes, o lint e o build do projeto afetado passando, e com um resumo para o usuário.

### Dúvidas

Qualquer dúvida (na spec, no plan, na implementação, num conflito entre regras, num teste que quebra fora do escopo ou na necessidade de mexer no outro projeto) vai para o usuário antes de seguir, com as opções, as consequências de cada uma e uma recomendação. Nunca suponha nem decida sozinho algo que mude escopo, comportamento, contrato da API, dados ou segurança.

### Artefatos enxutos

- Artefatos registram decisões e evidências, não reproduzem o que já está no repositório: cite `arquivo:linha`, o nome do teste e o hash do commit em vez de copiar código, diffs ou saída de testes e comandos.
- O relatório de um bug tem no máximo ~30 linhas.
- Omita as seções do template que não se aplicam, em vez de preenchê-las com "N/A".

### Revisão

- As etapas rodam no agente principal, que conversa com o usuário e aplica os portões. Sub-agentes só para a revisão abaixo ou quando o usuário pedir.
- Um revisor novo e sem o contexto do autor (o agente `reviewer`, em `.claude/agents/`) confere o resultado contra os `AGENTS.md` e os artefatos anteriores. Ele não edita: aponta achados com severidade, local e recomendação.
- O autor corrige os achados CRITICAL e HIGH e a revisão roda de novo; depois de 2 rodadas com achados CRITICAL ou HIGH, o usuário decide.
- O prompt do revisor informa o projeto, a pasta da feature ou o arquivo do bug, o momento e as decisões que o usuário já tomou.
- Sem sub-agentes, faça a revisão como um passo separado, relendo os artefatos contra a tabela abaixo.

| Momento | O revisor confere |
| --- | --- |
| Antes do portão (só feature que toca contrato, dados ou segurança) | A spec cobre o pedido sem escopo a mais; requisitos testáveis e sem detalhe de implementação; critérios de sucesso mensuráveis; segurança e performance consideradas. No plan: conferência fiel aos `AGENTS.md`; decisões justificadas; contratos coerentes com o outro projeto; testes previstos |
| Fim do implement ou do bug-fix (sempre) | Testes escritos antes do código; suíte, lint e build passando; segurança, performance e fronteiras do projeto; aderência ao plan ou ao diagnóstico; documentos vivos atualizados |

Nas demais features, a revisão antes do portão é uma releitura do próprio autor contra esta tabela. O diagnóstico do bug não tem revisor próprio: o usuário o revisa no portão.

## Idioma

- Artefatos novos (specs, plans e relatórios de bug), os documentos vivos e os `AGENTS.md` são escritos em português (pt-BR).
- As specs já existentes, em inglês, ficam como estão. Não traduza nem reescreva esses arquivos de passagem.
- Mantenha os identificadores dos templates: `FR-001`, `SC-001`, `T001`, `[P]`, `[X]`, `[NEEDS CLARIFICATION: …]`.
- Nomes de pasta (`NNN-nome` das features, `<slug>` dos bugs) ficam em kebab-case e sem acentos, por exemplo `002-metas-de-orcamento`.
- No código, siga as convenções existentes: identificadores e comentários em inglês, textos exibidos ao usuário em português.

## Governança

- Esta constituição (este arquivo, `back/AGENTS.md` e `front/AGENTS.md`) prevalece sobre convenções informais e precedentes. Numa mudança que cruza os projetos, o `AGENTS.md` de cada um governa a sua parte, e nenhum dispensa o outro.
- Mudanças nestas regras são feitas com a aprovação do usuário e registram o motivo na mensagem do commit; o histórico do git é o registro de versões.
- Toda revisão confere a aderência a estas regras. Uma violação bloqueia a entrega até ser corrigida ou até o usuário aprovar uma exceção registrada no `plan.md`.
- Specs e bugs anteriores a 2026-10-04 citam as constituições 1.0.0 de cada projeto (no histórico do git, em `<projeto>/.specify/memory/constitution.md`): I (API) → IV, B2 e F2; II (fronteiras) → V, B1 e F1; III (migrations) → B3 e F3; IV (testes) → I. Os que citam `specs/`, `bugs/`, `tasks.md` ou as skills `speckit-*` descrevem o fluxo antigo: as pastas hoje estão em `docs/features/` e `docs/bugs/`. Nos relatórios de bug do front, os números das features já seguem a numeração nova (001 categorias, 002 transações, 003 relatórios), mas os números de linha citados valem para as versões anteriores à conversão de 2026-11-09 de `spec.md` e `plan.md` e para `tasks.md`, `quickstart.md`, `data-model.md` e `checklists/`, hoje removidos; essas versões estão no histórico do git (por exemplo, em `8b26163`).
