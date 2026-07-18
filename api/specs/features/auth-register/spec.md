# auth-register

Permite que uma pessoa crie uma conta no Denarius informando e-mail, senha e nome, para em seguida poder autenticar-se e usar o sistema.

> Entidade de referência: `specs/entities/user.md`

## Entrada
- `Email`
- `Password`
- `Name`

## Happy path
- `Email` ainda não está cadastrado no sistema
- `Password` tem 3 caracteres ou mais
- `Email` é válido e `Name` tem 5 caracteres ou mais (regras da entidade `User`)
- Senha é transformada em hash antes de ser persistida — a senha em texto puro nunca é armazenada
- Usuário é criado e persistido
- Resposta: dados públicos do usuário criado (`Id`, `Email`, `Name`, `CreatedAt`) — nunca o hash da senha

## Erros
- `Email` já cadastrado no sistema
- `Password` com menos de 3 caracteres
- `Email` em formato inválido
- `Name` vazio ou com menos de 5 caracteres

## Fora de escopo
- Confirmação de e-mail / verificação de conta
- Login automático após o registro (o usuário precisa autenticar-se separadamente via `POST /api/auth/login`)
- Políticas de complexidade de senha além do tamanho mínimo (maiúsculas, símbolos, etc.)
- Rate limiting ou proteção contra registro em massa
- Edição de perfil (nome, senha) — coberta por outras operações da entidade `User`, não por este fluxo de registro
