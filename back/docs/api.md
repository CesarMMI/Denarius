# API

O que o OpenAPI gerado não diz. Rotas, parâmetros, schemas e status de cada endpoint estão no OpenAPI da própria API, servido em desenvolvimento por `app.MapOpenApi()` (`/openapi/v1.json`, em `Denarius.WebAPI/Program.cs`). Os contratos históricos de cada feature ficam em `docs/features/*/contracts/`. Este arquivo guarda as convenções e as regras que valem para vários endpoints, e é o ponto de partida do que o front pode supor (princípio IV do `AGENTS.md` da raiz).

## Convenções

- **Rotas** em `api/<recurso>`, em camelCase (`CamelCaseRouteTransformer`): `api/categories`, `api/transactions`, `api/reports/expensesByCategory`.
- **Enums** nos parâmetros de consulta e no JSON em minúsculas quando têm nome público: o tipo da transação é `all`, `in` ou `out`.
- **Mês** é sempre `yyyy-MM` (`2026-09`), no tipo `YearMonth`. Um mês malformado é recusado na ligação do parâmetro, antes de qualquer caso de uso.
- **Mês nas listagens** (`dateRef`, em `categories` e `transactions`): é uma data; só o ano e o mês dela valem.
- **Dinheiro** é decimal exato. Valores projetados e percentuais saem arredondados para duas casas.
- **Datas de transação** são dias do calendário, sem fuso. O mês padrão dos relatórios e o "hoje" usam `America/Sao_Paulo`.
- **Listas** não têm paginação: a resposta traz todos os itens do filtro.
- **Cor** da categoria é `#RRGGBB` em maiúsculas, em qualquer endpoint que a devolva.

## Erros

O formato é `ProblemDetails` (`GlobalExceptionHandler`):

| Origem | Status | `detail` |
| --- | --- | --- |
| `NotFoundException` (categoria ou transação inexistente) | 404 | a mensagem da exceção |
| `DomainException` (regra de domínio violada) | 400 | a mensagem da exceção, em português |
| `AppException` | 400 | a mensagem da exceção |
| Qualquer outra exceção | 500 | "Ocorreu um erro inesperado.", sem detalhes internos |
| Parâmetro inválido na ligação do MVC (mês malformado, `months` fora de 1 a 24) | 400 | validação padrão do MVC |

## Regras que cruzam endpoints

- Excluir uma categoria com transações é recusado (400). Ver [categoria](dominio/categoria.md).
- Os relatórios leem só os meses que cobrem e fazem as somas no banco. Mês sem movimentação devolve zeros ou listas vazias, nunca erro.
- O relatório "transações do mês" devolve todas as transações do mês, da data mais recente para a mais antiga (na mesma data, a registrada por último primeiro), com o valor em módulo e o tipo à parte, e com a cor atual da categoria.
- "Despesas por categoria" lista as 7 maiores individualmente e soma o resto numa entrada final "Outras", sem identificador nem cor, quando há mais de 8 categorias com despesa.
- "Receitas vs despesas" cobre 12 meses por padrão, de 1 a 24, terminando no mês pedido e incluindo os meses zerados.

## Compatibilidade

- Mudanças são aditivas por padrão: endpoint novo ou campo novo e opcional não pedem versão.
- Remover ou renomear endpoint, mudar um formato ou a semântica de um status é incompatível, e só sai com versionamento e caminho de migração (B2 do `AGENTS.md` do back).
- O CORS aceita só as origens da seção `Cors` do `appsettings`.
