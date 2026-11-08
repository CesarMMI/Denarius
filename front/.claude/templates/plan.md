# Plano: <Nome da feature>

<!-- Como a spec será entregue. Cite arquivo:linha e nomes de teste em vez de copiar código. Apague as seções que não se aplicam. -->

**Spec**: `docs/features/NNN-nome/spec.md`

## Conferência com as regras

<!-- Uma linha por item; só o que a feature toca. Exceção a uma regra exige aprovação do usuário, registrada aqui. -->

- Organização (F1): feature e pastas afetadas:
- Contrato (F2 e IV): endpoints consumidos, só os que `back/docs/api.md` e o OpenAPI já oferecem. Se faltar algo, a mudança é do back e precisa da aprovação do usuário antes:
- Rotas (F2): rota nova ou renomeada, e o redirecionamento do caminho antigo:
- Banco (F3): migration necessária no back (e o rollback), ou nenhuma:
- Material (F4): componentes e tokens usados; o que é código próprio e por quê:
- Segurança (II): sem `innerHTML` dinâmico nem `bypassSecurityTrust*`:
- Performance (III): meta da spec, budgets do `angular.json`, carregamento sob demanda:
- Dependência nova: motivo, manutenção, vulnerabilidades:

## Mudanças

- **Telas e componentes**: o que é criado ou alterado, e onde (`src/app/<feature>/`).
- **Serviços e tipos**: chamadas HTTP e tipos que espelham o contrato.
- **`docs/design.md`**: o que muda de permanente na identidade visual ou nos padrões de interface.

## Decisões

<!-- O que foi escolhido, as alternativas descartadas e o porquê. Pesquisa longa vai para research.md. -->

## Testes previstos

<!-- Um `*.spec.ts` ao lado de cada arquivo novo ou alterado. Cada requisito da spec tem ao menos um teste. -->

## Riscos

## Tarefas

<!-- Os testes vêm antes da implementação que eles cobrem (princípio I). [P] marca tarefa que pode rodar em paralelo. -->

- [ ] T001 Teste: ...
- [ ] T002 Implementar: ...
- [ ] T0NN Atualizar `docs/design.md` com o que mudou de permanente (obrigatória, ou registrar que nada mudou)
