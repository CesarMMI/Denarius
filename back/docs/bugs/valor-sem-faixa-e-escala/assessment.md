# Avaliação do bug: a API não valida a faixa nem a escala do valor da transação

- **Slug**: valor-sem-faixa-e-escala · **Projeto**: back · **Criada em**: 2026-10-09
- **Origem**: desvio registrado em `front/specs/002-transacoes/research.md:336` e `spec.md:101,353`
- **Veredito**: válido · **Severidade**: high

## Sintoma e reprodução

Reproduzido em 2026-10-09 no `main` (`9b435b7`), com `POST /api/transactions` numa base descartável, apagada depois:
- `10.125` responde `201` e devolve 10.125, mas o banco grava 10.13, sem aviso.
- `-0.001` responde `201`, mas o banco grava **0,00**. A partir daí, `GET /api/transactions` e `GET /api/categories` respondem `400` ("não pode ser zero").
- Valores a partir de 10^16 respondem `500`: o PostgreSQL dá o erro `22003`, estouro de numeric.

O esperado, pelo princípio II, é a API recusar com `400` todo valor que ela não consegue guardar exatamente.

## Causa raiz (confiança alta)

- `src/Denarius.Domain/Entities/Transaction.cs:60-66`: `ValidateValue` só recusa o zero.
- `src/Denarius.Infrastructure/Persistence/Configurations/TransactionConfiguration.cs:23-25`: a coluna `numeric(18,2)` arredonda as casas a mais e estoura a partir de 10^16. O estouro vira `500` em `GlobalExceptionHandler.cs:35`.
- O EF monta a `Transaction` pelo construtor público, que valida o valor (a pilha do log passa por `ValidateValue`). Por isso, um zero gravado quebra toda consulta que carrega transações.
- O contrato `specs/002-transacoes/contracts/transactions-api.yaml` não documenta nenhum limite para o `value`.

## Correção proposta

`ValidateValue`, usado na criação e na edição, passa a lançar `DomainException` (`400`) quando o valor tem mais de duas casas decimais ou quando o módulo passa da nova constante pública `ValueMaxAbsolute`. O contrato ganha `minimum`, `maximum` e `multipleOf: 0.01`. Não há migration.

Os testes vêm antes, em `TransactionTests.cs`: casas a mais e valores acima do máximo são recusados na criação e na edição; ±0,01, ±máximo e `10.120` são aceitos.

## Riscos

- O contrato fica mais restrito só para valores que hoje já falham ou são gravados diferentes do enviado. O front já os recusa (FR-014 da 003).
- Fora do escopo: o EF continuar montando a entidade pelo construtor que valida. Na base de dev as listas funcionam, então nela não há zero gravado.

## Decisões do usuário (portão, 2026-10-09)

- P1: (A) o máximo é 9.999.999.999.999,99, o mesmo do front. A alternativa (B) era o limite da coluna.
- P2: (A) mais de duas casas são recusadas com `400`. A alternativa (B) era arredondar.
