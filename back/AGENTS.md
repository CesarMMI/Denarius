# Denarius — back

API REST em .NET 10 (`net10.0`), com Clean Architecture, EF Core (code-first) e PostgreSQL (Npgsql). Este arquivo acrescenta as regras específicas do back às do [`AGENTS.md`](../AGENTS.md) da raiz (TDD, segurança, performance, contrato com o front e fluxo de trabalho).

## Arquitetura

Quatro projetos em `src/`, com dependências em um só sentido:

- `Denarius.Domain`: entidades, value objects e interfaces de repositório. Não depende de outro projeto nem de pacote de terceiros.
- `Denarius.Application`: casos de uso e contratos de entrada e saída (`IO/`). Depende só do `Domain`.
- `Denarius.Infrastructure`: persistência com EF Core, repositórios e migrations. Depende do `Domain` e do `Application`.
- `Denarius.WebAPI`: controllers, middleware, CORS e composição por injeção de dependência. Depende do `Application` e do `Infrastructure`.

Cada um tem o seu projeto xUnit em `tests/` (`Denarius.Domain.Tests`, `Denarius.Application.Tests` e `Denarius.WebAPI.Tests`); o `Denarius.WebAPI.Tests` usa `Microsoft.AspNetCore.TestHost` para testes de integração.

## Princípios

### B1. Camadas

- O código fica na camada da sua responsabilidade, e nenhuma camada é pulada: um controller não usa tipos do `Infrastructure`, e o `Domain` não ganha dependências.
- O `Application` declara o que precisa de infraestrutura (persistência, I/O externo) como interfaces; o `Infrastructure` as implementa.
- O `WebAPI` só compõe e expõe: regra de negócio fica no `Domain` ou no `Application`.

_Por quê:_ as camadas mantêm a regra de negócio testável isoladamente e a infraestrutura substituível.

### B2. Compatibilidade da API

- Rotas, DTOs, status codes e o formato de erro (`ProblemDetails`) continuam compatíveis com os consumidores atuais (princípio IV da raiz).
- Mudança incompatível (endpoint removido ou renomeado, formato alterado, semântica de status code diferente) só sai com versionamento e caminho de migração documentados. Adições compatíveis, como um endpoint novo ou um campo opcional, não pedem nova versão.
- Todo contrato novo ou alterado é documentado em `specs/NNN-nome/contracts/`.

_Por quê:_ a API é o único ponto de integração do front; uma quebra não coordenada derruba o front sem aviso.

### B3. Migrations reversíveis

- Toda migration em `Denarius.Infrastructure/Migrations` tem um caminho de volta verificado: um `Down()` que desfaz o `Up()` ou, quando isso não é possível (por exemplo, numa transformação destrutiva de dados), um procedimento manual documentado junto da migration.
- Migration com rollback não verificado não é entregue.

_Por quê:_ o Denarius guarda dados financeiros; sem um rollback revisado, um deploy ruim vira perda de dados.

## Restrições técnicas

- **Performance**: consultas de leitura com `AsNoTracking()`; filtros, agrupamentos e somas traduzidos para SQL, como no `TransactionRepository`; índice, criado por migration, para colunas usadas em filtros frequentes; I/O assíncrono de ponta a ponta. As listagens de transações e de categorias ainda filtram em memória: levá-las para o banco é trabalho de uma feature própria, e não um ajuste de passagem.
- **Segurança**: CORS só com as origens da seção `Cors` do `appsettings` de cada ambiente; exceções passam pelo `GlobalExceptionHandler`, e um erro inesperado responde 500 com mensagem genérica; `UseHttpsRedirection` fora de desenvolvimento; segredos locais em user-secrets (o `Denarius.WebAPI` já tem `UserSecretsId`).

## Testes e verificação

De dentro de `back/`:

- `dotnet test`: as três suítes passam, e comportamento novo ganha teste no projeto de testes da camada correspondente.
- Mudou um controller ou DTO: a entrega diz se a mudança é aditiva ou incompatível e, se for incompatível, como foi versionada.
- Criou uma migration: aplique e reverta localmente (`Up()` e `Down()`) antes de concluir.
