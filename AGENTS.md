# Denarius

Aplicação de finanças pessoais. O repositório tem dois projetos independentes:

- `back/`: API REST em .NET 10 (Clean Architecture, EF Core, PostgreSQL). Regras em [`back/AGENTS.md`](back/AGENTS.md).
- `front/`: SPA em Angular 21 (Angular Material, Vitest). Regras em [`front/AGENTS.md`](front/AGENTS.md).

Este arquivo reúne as regras que valem para o repositório inteiro e o fluxo de trabalho obrigatório. Junto com `back/AGENTS.md` e `front/AGENTS.md`, ele forma a constituição do projeto: é o que o Spec Kit usa no _Constitution Check_ do plan, no `/speckit-analyze` e no `/speckit-converge`. Os arquivos dos projetos só acrescentam regras; nunca afrouxam as daqui.

## Estrutura

| Caminho | Conteúdo |
| --- | --- |
| `.specify/` | Spec Kit compartilhado: scripts, templates, a extensão `bug` e o ponteiro local da feature ativa (`feature.json`) |
| `.claude/skills/speckit-*` | Skills do Spec Kit, usadas pelos dois projetos |
| `<projeto>/specs/NNN-nome/` | Artefatos de cada feature, numerados por projeto |
| `<projeto>/bugs/<slug>/` | Relatórios de bug (`assessment.md`, `fix.md`, `test.md`), no projeto da causa raiz |
| `<projeto>/.claude/skills/` | Skills específicas de cada projeto |

## Princípios gerais

### I. Desenvolvimento guiado por testes (TDD)

- Todo comportamento novo ou alterado começa por um teste que falha. Depois vem o mínimo de código para ele passar e, com os testes verdes, a refatoração.
- Correção de bug começa por um teste que reproduz o bug e falha antes da correção.
- No `tasks.md`, as tarefas de teste vêm antes das tarefas de implementação que elas cobrem.
- Nenhum teste existente é apagado, ignorado ou enfraquecido para a suíte passar.
- Uma mudança só está pronta quando a suíte completa do projeto afetado passa (os comandos estão no `AGENTS.md` de cada projeto).

_Por quê:_ os testes são a única proteção automática contra regressões, e escrevê-los primeiro garante que eles verificam o comportamento pedido, e não o código que já existe.

### II. Segurança

- Nenhum segredo real no repositório. O único valor versionado é a connection string do banco local de desenvolvimento (`back/src/Denarius.WebAPI/appsettings.Development.json`); fora isso, use user-secrets ou variáveis de ambiente.
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

- A API REST do back, documentada em `back/specs/*/contracts/`, é o único ponto de integração entre os projetos.
- Mudanças no contrato são aditivas por padrão. Uma mudança incompatível só sai com estratégia de versionamento e caminho de migração documentados.
- O front consome só o que o contrato já oferece. Se uma feature do front precisar de mudança na API, a spec diz isso, o usuário aprova explicitamente e o back entrega a mudança, com o ciclo completo do Spec Kit, antes de o front depender dela.

_Por quê:_ um front que supõe um contrato diferente quebra em execução, e não no build.

### V. Escopo e simplicidade

- O trabalho de um projeto fica nele: uma tarefa do front não altera o back, nem o contrário, sem aprovação explícita do usuário.
- Faça a menor mudança que atende à spec. Siga os padrões que o projeto já usa antes de criar novos, e não crie abstrações para necessidades hipotéticas.

_Por quê:_ mudanças pequenas e previsíveis são mais fáceis de revisar, testar e desfazer.

## Fluxo de trabalho

Toda mudança implementada por um agente (feature, ajuste de comportamento, refatoração ou correção de bug) segue a cerimônia do Spec Kit, incluindo as etapas opcionais. Se a mudança parecer pequena demais para isso, pergunte ao usuário antes de pular qualquer etapa; nunca pule por conta própria.

### Features e mudanças de comportamento

| Etapa | Resultado |
| --- | --- |
| 1. `/speckit-specify` | `<projeto>/specs/NNN-nome/spec.md` e `checklists/requirements.md` |
| 2. `/speckit-clarify` | Ambiguidades resolvidas com o usuário e registradas na spec |
| **Portão** | **O usuário aprova a spec** |
| 3. `/speckit-plan` | `plan.md`, `research.md`, `data-model.md`, `contracts/` e `quickstart.md` |
| 4. `/speckit-checklist` | Checklists de qualidade dos requisitos: no mínimo segurança e performance, e API (back) ou UX (front) |
| **Portão** | **O usuário aprova o plan** |
| 5. `/speckit-tasks` | `tasks.md`, com os testes antes da implementação |
| 6. `/speckit-analyze` | Relatório de consistência entre spec, plan e tasks |
| **Portão** | **O usuário aprova a análise e libera a implementação** |
| 7. `/speckit-implement` | Código e testes; tasks marcadas `[X]` |
| 8. `/speckit-converge` | Trabalho restante anexado ao `tasks.md`; repita 7 e 8 até convergir |

- Em cada portão, apresente um resumo do que foi produzido, os achados da revisão (resolvidos e pendentes), as decisões tomadas e os riscos, e espere a aprovação.
- No portão da análise, apresente uma correção proposta para cada achado do `/speckit-analyze`. Com a aprovação do usuário, os achados CRITICAL e HIGH são corrigidos e o `/speckit-analyze` roda de novo antes do implement.
- Trabalho que envolve os dois projetos faz o ciclo completo no back primeiro e depois no front, com uma spec em cada projeto.
- Mudanças nestas regras passam por `/speckit-constitution`.
- A entrega termina com a suíte de testes, o lint e o build do projeto afetado passando, e com um resumo para o usuário.

### Bugs

1. `/speckit-bug-assess`: diagnóstico em `<projeto>/bugs/<slug>/assessment.md`, sem alterar código.
2. **Portão: o usuário aprova o diagnóstico e a correção proposta.**
3. `/speckit-bug-fix`: primeiro o teste que reproduz o bug (e falha), depois a correção; registro em `fix.md`.
4. `/speckit-bug-test`: reprodução e suíte do projeto; resultado em `test.md`.

O relatório fica no projeto da causa raiz. Se a correção exigir os dois projetos, cada um ganha o seu relatório, com o back primeiro. Se o diagnóstico mostrar que não é bug, e sim comportamento novo, volte ao fluxo de features.

### Orquestração com sub-agentes

Quando a ferramenta oferece sub-agentes:

- O agente principal orquestra: conversa com o usuário, dispara cada etapa, aplica os portões e consolida os resultados. Ele não escreve artefatos nem código.
- Cada etapa roda em um sub-agente próprio, que invoca a skill da etapa. O prompt informa o projeto, a pasta da feature (os scripts aceitam `SPECIFY_FEATURE_DIRECTORY=<projeto>/specs/NNN-nome`) e as decisões que o usuário já tomou.
- Depois de cada etapa que produz artefato ou código, um sub-agente revisor, novo e sem o contexto do autor, confere o resultado contra os `AGENTS.md` e os artefatos anteriores. O revisor não edita: aponta achados com severidade, local e recomendação. O autor corrige, e a revisão se repete até não restar achado CRITICAL ou HIGH; depois de 3 rodadas, o usuário decide.
- Sub-agentes não falam com o usuário. Quando a skill mandar perguntar algo, o sub-agente para e devolve as perguntas, cada uma com opções e uma recomendação; o orquestrador pergunta ao usuário e retoma o mesmo sub-agente com as respostas.
- Sem sub-agentes, rode as etapas em sequência no mesmo agente e faça a revisão como um passo separado ao fim de cada uma.

| Etapa | O revisor confere |
| --- | --- |
| specify, clarify | A spec cobre o pedido sem escopo a mais; requisitos testáveis e sem detalhes de implementação; critérios de sucesso mensuráveis; segurança e performance consideradas |
| plan | _Constitution Check_ fiel aos `AGENTS.md`; decisões justificadas no `research.md`; contratos coerentes com o outro projeto; testes previstos |
| checklist | Atua como revisor dos checklists: marca `[x]` só no que verificou, e o que depende do usuário vira pergunta |
| tasks, converge | Sem revisor próprio: o `/speckit-analyze` revisa as tasks, e o `/speckit-converge` já confere a implementação contra a spec |
| implement, bug-fix | Testes escritos antes do código; suíte, lint e build passando; segurança, performance e fronteiras do projeto; aderência ao plan ou ao diagnóstico |
| bug-assess | Causa raiz sustentada por evidência; correção mínima; teste de regressão previsto |

### Dúvidas

Qualquer dúvida (na spec, no plan, na implementação, num conflito entre regras, num teste que quebra fora do escopo ou na necessidade de mexer no outro projeto) vai para o usuário antes de seguir, com as opções, as consequências de cada uma e uma recomendação. Nunca suponha nem decida sozinho algo que mude escopo, comportamento, contrato da API, dados ou segurança.

## Idioma

- Artefatos novos (specs, plans, tasks, checklists e relatórios de bug) e os `AGENTS.md` são escritos em português (pt-BR).
- As specs já existentes, em inglês, ficam como estão: a tradução será feita à parte. Não traduza nem reescreva esses arquivos de passagem.
- Mantenha exatamente como nos templates os identificadores e marcadores que as skills e os scripts reconhecem: `FR-001`, `SC-001`, `US1`, `T001`, `CHK001`, `[P]`, `[X]`, `[NEEDS CLARIFICATION: …]`.
- Nomes de pasta (`NNN-nome` das features, `<slug>` dos bugs) ficam em kebab-case e sem acentos, por exemplo `002-metas-de-orcamento`.
- No código, siga as convenções existentes: identificadores e comentários em inglês, textos exibidos ao usuário em português.

## Governança

- Esta constituição (este arquivo, `back/AGENTS.md` e `front/AGENTS.md`) prevalece sobre convenções informais e precedentes. Numa mudança que cruza os projetos, o `AGENTS.md` de cada um governa a sua parte, e nenhum dispensa o outro.
- Alterações passam por `/speckit-constitution`, registram o motivo e atualizam a versão abaixo, que vale para os três arquivos: MAJOR remove um princípio ou o redefine para permitir o que era proibido; MINOR acrescenta ou amplia um princípio; PATCH muda a redação sem mudar o que é exigido.
- Toda revisão confere a aderência a estas regras. Uma violação bloqueia a entrega até ser corrigida ou até o usuário aprovar uma exceção registrada no `plan.md`.
- Specs anteriores a 2026-10-04 citam as constituições 1.0.0 de cada projeto (no histórico do git, em `<projeto>/.specify/memory/constitution.md`). Os princípios de lá correspondem aos atuais assim: I (API) → IV, B2 e F2; II (fronteiras) → V, B1 e F1; III (migrations) → B3 e F3; IV (testes) → I.

**Versão**: 2.0.0 | **Ratificada**: 2026-09-21 | **Última alteração**: 2026-10-04

## Manutenção do Spec Kit

- Adaptações deste repositório:
  - as skills `speckit-*` leem os `AGENTS.md` no lugar de `.specify/memory/constitution.md`, e o `/speckit-specify` cria as specs no `specs/` do projeto;
  - o `.specify/scripts/bash/create-new-feature.sh` usa o `specs/` do projeto de onde é executado (ou o de `SPECIFY_SPECS_DIR`);
  - a extensão `bug` grava os relatórios em `<projeto>/bugs/<slug>/`: o ajuste está nos comandos em `.specify/extensions/bug/commands/`, no README e no `extension.yml` dela, e nas skills `speckit-bug-*` geradas a partir dos comandos.
- Por causa delas, o `specify integration status` lista arquivos modificados, o que é esperado, e um upgrade as desfaz. O `specify integration upgrade claude` para sem alterar nada enquanto houver skills modificadas e só segue com `--force`, que sobrescreve skills, scripts e templates. O `specify extension update` substitui a extensão `bug` sem aviso. Depois de qualquer um deles, use o `git diff` para reaplicar as adaptações sobre os arquivos novos.
- O `specify init --here --force` também sobrescreve as adaptações e recria `.specify/memory/constitution.md`. Se esse arquivo reaparecer, apague-o: a constituição vive nos `AGENTS.md`.
- Com uma extensão instalada, os scripts precisam de um Python 3 funcional para ler `.specify/extensions/.registry`. Se `python3` não funcionar (no Windows, costuma ser só o atalho da Microsoft Store), aponte `SPECKIT_PYTHON_EXECUTABLE` para um Python 3 com PyYAML, como o do `specify-cli` instalado pelo uv.
