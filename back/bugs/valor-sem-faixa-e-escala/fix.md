# Correção: a API não valida a faixa nem a escala do valor da transação

- **Slug**: valor-sem-faixa-e-escala · **Corrigido em**: 2026-10-09 · **Avaliação**: ./assessment.md · **Status**: applied

## Mudanças

- `src/Denarius.Domain/Entities/Transaction.cs`: a nova constante `ValueMaxAbsolute` define o máximo, e `ValidateValue` recusa com `400` um valor com mais de duas casas decimais ou com módulo acima dele.
- `tests/Denarius.Domain.Tests/Entities/TransactionTests.cs`: `Constructor_ValueWithMoreThanTwoDecimalPlaces_*`, `Constructor_ValueAboveTheMaximum_*`, `Constructor_ValueAtTheLimits_*`, `ValueMaxAbsolute_*` e `Update_ValueWithMoreThanTwoDecimalPlacesOrAboveTheMaximum_*`.
- `specs/002-transaction-management/contracts/transactions-api.yaml` 1.2.0: o `value` da criação e da edição ganha `minimum`, `maximum` e `multipleOf`. A mudança só restringe valores que antes davam `500` ou eram gravados diferentes do enviado.

## Verificação

- Os 10 casos de recusa falhavam antes da correção. Depois dela, o `dotnet test` passou: 51 + 139 + 63.
- O revisor independente aprovou. Os achados de tamanho dos artefatos e o de uma linha em branco foram corrigidos.

## Desvios e pendência

- A mensagem de erro traz o máximo por extenso, sem depender da cultura do runtime.
- O `assessment.md` foi enxugado até o limite de 40 linhas, a pedido do revisor, sem mudar o conteúdo.
- Pendência no front, que depende de aprovação: as notas de "desvio conhecido" na spec 003.
