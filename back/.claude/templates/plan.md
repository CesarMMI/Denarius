# Plano: <Nome da feature>

<!-- Como a spec será entregue. Cite arquivo:linha e nomes de teste em vez de copiar código. Apague as seções que não se aplicam. -->

**Spec**: `docs/features/NNN-nome/spec.md`

## Conferência com as regras

<!-- Uma linha por item; só o que a feature toca. Exceção a uma regra exige aprovação do usuário, registrada aqui. -->

- Camadas (B1):
- Compatibilidade da API (B2): aditiva ou incompatível? Se incompatível, versionamento e migração:
- Migrations (B3): rollback verificado:
- Segurança (II): validação de entrada, erros, dados fora dos logs:
- Performance (III): meta da spec e como será cumprida (filtro e agregação no banco):
- Dependência nova: motivo, manutenção, vulnerabilidades:

## Mudanças

- **Domínio**: entidades, value objects e regras que mudam.
- **Aplicação**: casos de uso e `IO/`.
- **Infraestrutura**: repositórios, consultas e migrations.
- **API**: endpoints e contrato (adições e o que fica como está). O contrato de uma feature vai em `contracts/` na pasta da feature, e o `docs/api.md` só ganha o que for convenção ou regra nova.

## Decisões

<!-- O que foi escolhido, as alternativas descartadas e o porquê. Pesquisa longa vai para research.md. -->

## Testes previstos

<!-- Por camada (Domain, Application, Infrastructure, WebAPI.Tests). Cada requisito da spec tem ao menos um teste. -->

## Riscos

## Tarefas

<!-- Os testes vêm antes da implementação que eles cobrem (princípio I). [P] marca tarefa que pode rodar em paralelo. -->

- [ ] T001 Teste: ...
- [ ] T002 Implementar: ...
- [ ] T0NN Atualizar `docs/dominio/` e `docs/api.md` com o que mudou de permanente (obrigatória)
