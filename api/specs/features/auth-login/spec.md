# auth-login

Permite que uma pessoa já cadastrada se autentique com e-mail e senha e receba um token JWT para acessar os endpoints protegidos do Denarius.

> Entidade de referência: `specs/entities/user.md`

## Entrada
- `Email`
- `Password`

## Happy path
- `Email` corresponde a um usuário cadastrado
- `Password` confere com o hash armazenado do usuário
- Resposta: token JWT e dados públicos do usuário (`Token`, `User` — `Id`, `Email`, `Name`, `CreatedAt`), nunca o hash da senha

## Erros
- `Email` não cadastrado no sistema — retorna erro genérico de credenciais inválidas
- `Password` não confere com o hash armazenado — retorna o mesmo erro genérico de credenciais inválidas

> As duas falhas acima são indistinguíveis para quem chama a API — mesma mensagem, mesmo status — para não revelar se um e-mail existe no sistema.

## Fora de escopo
- Emissão de refresh token ou renovação de sessão — o token expira e a pessoa precisa autenticar-se novamente
- Bloqueio de conta ou rate limiting após tentativas malsucedidas
- Recuperação/redefinição de senha
- Logout ou invalidação de token antes da expiração
- Autenticação multifator
