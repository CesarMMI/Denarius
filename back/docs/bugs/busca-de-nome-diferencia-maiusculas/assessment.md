# Avaliação do bug: a busca de categorias por nome diferencia maiúsculas de minúsculas

- **Slug**: busca-de-nome-diferencia-maiusculas · **Projeto**: back · **Criada em**: 2026-10-09
- **Origem**: desvio registrado na FR-017 de `front/docs/features/001-categorias/spec.md` (Esclarecimentos, linha 23) e em `plan.md:94`
- **Veredito**: válido · **Severidade**: medium

## Sintoma e reprodução

Reproduzido em 2026-10-09 no `main` (`09f48fe`). As requisições `GET /api/categories?name=…` rodaram numa base descartável, apagada depois, com as categorias "Mercado" e "Educação". `Mer`, `cado` e `Educa` encontram a categoria; `mer`, `MERCADO`, `educa` e `EDUCAÇÃO` não encontram nada.

O esperado é a busca não diferenciar maiúsculas de minúsculas, como a busca por descrição das transações (`ListTransactionsUseCase.cs:16`); os acentos ficam na decisão P1, abaixo.

## Causa raiz (confiança alta)

- `src/Denarius.Infrastructure/Repositories/CategoryRepository.cs:15`: o `c.Name.Contains(name)` vira uma comparação exata no PostgreSQL.
- `specs/001-categorias/contracts/categories-api.yaml:19`: o contrato deixa a regra "a cargo do banco", sem defini-la.
- Nenhum teste pega o problema: `ListCategoriesUseCaseTests.cs:37` usa um repositório mock, `CategoriesControllerTests.cs:211` só confere o binding, e não existe projeto de testes do `Infrastructure`.

## Correção proposta

- O filtro continua no banco e compara os dois lados em minúsculas (`lower(...)` no SQL; com a localidade `Portuguese_Brazil.1252`, `lower('Educação')` dá `educação`). O contrato vai para a 1.1.0 e passa a definir a regra, que só amplia os resultados; o front não muda.

## Decisões para o portão

- **P1, como comparar sem diferenciar maiúsculas:**
  - (A, recomendada) `ToLower()` nos dois lados. Não depende do provedor do banco nem trata `%` e `_` como curingas.
  - (B) `EF.Functions.ILike`. É específico do Npgsql e exige escapar `%`, `_` e `\` do texto buscado.
- **P2, onde fica o teste que reproduz o bug** (o defeito está na tradução para SQL, então só um banco real o reproduz):
  - (A, recomendada) Um projeto novo, `tests/Denarius.Infrastructure.Tests`, com o pacote `Testcontainers.PostgreSql`, que sobe um PostgreSQL descartável no Docker (29.6.1, já instalado) e testa o `CategoryRepository`. É uma dependência nova: o motivo fica registrado no `fix.md`, o `back/AGENTS.md` passa a citar o quarto projeto de testes, e a suíte passa a exigir o Docker.
  - (B) Um teste no `Denarius.WebAPI.Tests` que só confere o SQL gerado, com `ToQueryString()` e o Npgsql sem conexão. Não pede dependência nova, mas não prova o resultado da busca.

## Riscos

- Ninguém depende do comportamento atual. O front já espera a busca sem diferenciar maiúsculas (FR-017 da 002), e a busca só passa a encontrar mais categorias.
- Desempenho: o `lower()` impede o uso de índice, mas a coluna `Name` não tem índice, e a lista de categorias é pequena.
- Fora do escopo: o `name` não passa por `Trim()`, ao contrário da descrição das transações.

## Decisões do usuário (portão, 2026-10-09)

- P2: (A). P1: (A), e a busca também deixa de diferenciar acentos ("educacao" encontra "Educação"). A comparação passa a ser `lower(unaccent(...))` nos dois lados. A extensão `unaccent` (1.1, disponível no PostgreSQL 18.6 local) entra por migration, com `Down()`. Isso diverge da FR-017 da spec 001 do front e da busca por descrição das transações, que diferenciam acentos.
