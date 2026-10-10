# Verificação: a API não valida a faixa nem a escala do valor da transação

- **Slug**: valor-sem-faixa-e-escala · **Testado em**: 2026-10-09 · **Resultado**: verified
- **Avaliação**: ./assessment.md · **Correção**: ./fix.md

## Verificações

| Verificação | Resultado |
| --- | --- |
| Reprodução da avaliação, com a API na porta 5299 numa base descartável (apagada depois) | pass. `POST` com `10.125`, `-0.001`, `12345678901234567.89`, `99999999999999999999` e `10000000000000` responde `400` com a mensagem certa, e `PUT` com `10.125` também. `±9999999999999.99`, `0.01` e `10.120` respondem `201`. |
| Listas depois das tentativas | pass. `GET /api/transactions` e `GET /api/categories` respondem `200`, e nenhum valor zero foi gravado. |
| Testes novos e suíte (`dotnet test`) | pass: 51 + 139 + 63 |
| Build (`dotnet build`) | pass, sem avisos |

## Riscos residuais

- Uma base que já tenha um valor zero gravado antes da correção continua quebrando as listas. Isso pede um ajuste manual nos dados. A base de dev não tem esse caso.

## Recomendação

Fechar o bug.
