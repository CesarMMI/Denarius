# Verificação: a busca de categorias por nome diferencia maiúsculas de minúsculas

- **Slug**: busca-de-nome-diferencia-maiusculas · **Testado em**: 2026-10-09 · **Resultado**: verified
- **Avaliação**: ./assessment.md · **Correção**: ./fix.md

## Verificações

| Verificação | Resultado |
| --- | --- |
| Reprodução da avaliação: API na porta 5299, numa base descartável com as migrations aplicadas e apagada depois | pass. `Mer`, `mer`, `MERCADO`, `cado`, `educa`, `educacao`, `EDUCACAO`, `EDUCAÇÃO` e `  mer  ` encontram só a categoria certa. `%`, `_` e `Lazer` não encontram nada, e um texto só com espaços lista tudo. |
| Testes novos e suíte (`dotnet test`) | pass: 51 + 140 + 63 + 10 |
| Build (`dotnet build`) | pass, sem avisos |

## Riscos residuais

- Pelo Git Bash, o `curl --data-urlencode` estraga o acento do texto digitado. Com o UTF-8 já codificado na URL (`EDUCA%C3%87%C3%83O`), a API responde certo.
- A base de dev precisa de `dotnet ef database update` antes de a API ser reiniciada. A suíte agora exige o Docker em execução.

## Recomendação

Fechar o bug.
