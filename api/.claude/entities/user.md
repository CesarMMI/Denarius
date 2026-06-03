# User

Representa um usuário do sistema. É a entidade raiz de todas as outras — contas, categorias e transações pertencem a um usuário.

## Propriedades

| Propriedade    | Descrição                                  |
|----------------|--------------------------------------------|
| `Id`           | Identificador único do usuário             |
| `Email`        | Endereço de e-mail — único no sistema      |
| `PasswordHash` | Hash da senha — nunca a senha em texto puro |
| `Name`         | Nome de exibição do usuário                |
| `CreatedAt`    | Data e hora de criação                     |
| `UpdatedAt`    | Data e hora da última atualização          |

## Regras de negócio

### Criação

- `Email` deve ser válido (formato `local@domínio.tld`) — exatamente um `@`, local e domínio não vazios, domínio com pelo menos um `.` antes do sufixo
- `PasswordHash` não pode ser vazio — a entidade recebe o hash já computado, nunca a senha em texto puro
- `Name` não pode ser vazio e deve ter no mínimo 5 caracteres

### Atualização de nome

- Disponível via `UpdateName(name)`
- `Name` não pode ser vazio e deve ter no mínimo 2 caracteres

### Troca de senha

- Disponível via `ChangePassword(passwordHash)`
- O novo `PasswordHash` não pode ser vazio

## Relacionamentos

- Possui zero ou mais `Account`
- Possui zero ou mais `Category`
- Possui zero ou mais `Transaction`
