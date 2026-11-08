# Transação

Uma movimentação de dinheiro: entrada (valor positivo) ou saída (valor negativo). Código: `Denarius.Domain/Entities/Transaction.cs`.

## Campos

| Campo | Regra |
| --- | --- |
| `Id` | `Guid`, gerado na criação |
| `Description` | Opcional. Sem espaços nas pontas; em branco vira nulo. No máximo 255 caracteres (`DescriptionMaxLength`) |
| `Date` | Obrigatória. É o dia do calendário, sem deslocamento por fuso horário |
| `Value` | Diferente de zero, com no máximo 2 casas decimais e módulo até 9.999.999.999.999,99 (`ValueMaxAbsolute`). O sinal define o tipo |
| `CategoryId` | Obrigatório e de uma [categoria](categoria.md) existente |
| `CreatedAt`, `UpdatedAt` | UTC |

## Regras

- **Tipo pelo sinal:** valor positivo é entrada (`in`), negativo é saída (`out`). Os relatórios mostram o valor em módulo (`Amount`) junto com o tipo.
- Valor zero, mais de 2 casas decimais ou acima do limite: `DomainException`. As casas extras não são arredondadas em silêncio, porque a coluna guarda só duas.
- Data ausente ou categoria vazia: `DomainException`. Categoria que não existe: `NotFoundException`, na criação e na edição.
- A exclusão é definitiva.
- **Datas nos relatórios:** a transação pertence ao mês e ao dia da própria data. "Hoje" e o mês padrão vêm do fuso `America/Sao_Paulo`.
- **Listagem:** a busca por descrição ignora maiúsculas e acentos, trata `%`, `_` e `\` como texto comum e é feita no banco. A ordem alfabética (descrição e nome da categoria) segue o português do Brasil, fixa em qualquer ambiente, pela collation `pt_br_icu`: sem acento e sem diferenciar maiúsculas primeiro, depois a letra sem acento antes da acentuada, depois a minúscula antes da maiúscula. Desempates: data e depois horário de criação, ambos decrescentes, exceto na ordenação por data crescente, em que também são crescentes. Transações sem descrição ficam no início da ordem crescente e no fim da decrescente.

## Origem

Specs [002-transaction-management](../features/002-transaction-management/spec.md) e [003-financial-reports](../features/003-financial-reports/spec.md).
