# account-get-all

Permite que um usuário autenticado liste todas as contas financeiras que possui, sem paginação ou filtros.

> Entidade de referência: `specs/entities/account.md`

## Entrada
- Nenhuma — `UserId` vem do token JWT do usuário autenticado.

## Happy path
- Retorna a lista de todas as contas do usuário
- Contas inativas são incluídas na lista, junto com as ativas
- Usuário sem nenhuma conta cadastrada recebe uma lista vazia

## Erros
- Nenhum erro específico deste use case — apenas o caso genérico de ausência/invalidez do token JWT (401), tratado antes do use case ser executado.

## Fora de escopo
- Paginação, ordenação customizável ou filtros por query string (ex: apenas ativas, por moeda)
- Retornar dados de transações ou saldo agregado além do que já existe em cada conta individualmente
