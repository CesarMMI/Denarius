# Auth — Use Cases

## RegisterUser

Registra um novo usuário no sistema.

### Entrada
- `Email`
- `Password`
- `Name`

### Happy path
- Senha transformada em hash via `IPasswordHasher`
- Usuário criado e persistido
- Retorna os dados públicos do usuário (`UserOutput`)

### Erros
- `Email` já cadastrado no sistema
- `Password` com menos de 3 caracteres
- `Email` inválido (validado pela entidade `User`)
- `Name` vazio ou com menos de 5 caracteres (validado pela entidade `User`)

---

## Login

Autentica um usuário e retorna um token JWT.

### Entrada
- `Email`
- `Password`

### Happy path
- Retorna o token JWT e os dados públicos do usuário (`LoginOutput`)

### Erros
- `Email` não encontrado — retorna `InvalidCredentials` (sem revelar se o e-mail existe)
- Senha incorreta — retorna `InvalidCredentials` (mesma mensagem genérica)

> Ambos os casos de falha lançam `InvalidCredentialsException` com a mesma mensagem para evitar enumeração de usuários.
