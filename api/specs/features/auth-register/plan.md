# auth-register — Plan

Esta feature já está implementada. Este documento reflete o código real.

## Abordagem técnica

Fluxo Clean Architecture padrão do projeto (ver `specs/ARCHITECTURE.md`):

```
POST /api/auth/register
  → AuthEndpoints.MapAuthEndpoints (Denarius.Api/Endpoints/AuthEndpoints.cs)
  → RegisterUserRequest → RegisterUserInput
  → IRegisterUserUseCase → RegisterUserUseCase (Denarius.Application/UseCases/Auth/RegisterUserUseCase.cs)
      1. IUserRepository.ExistsByEmailAsync(email) — checa duplicidade
      2. valida tamanho da senha (>= 3) — lança InvalidPasswordException se não
      3. IPasswordHasher.Hash(password) — gera o hash (BCrypt, ver Denarius.Infrastructure/Auth)
      4. new User(email, passwordHash, name) — entidade valida email e nome, lança
         InvalidEmailException / InvalidNameException se inválidos
      5. IUserRepository.AddAsync(user)
      6. IUnitOfWork.CommitAsync() — única chamada de commit do use case
  → UserOutput.FromEntity(user)
  → Results.Created("/api/users/{id}", result) — 201
```

## Decisões locais

- A checagem de e-mail duplicado (`ExistsByEmailAsync`) roda **antes** da validação de senha
  e antes da criação da entidade `User` — evita hash de senha e trabalho de validação
  desnecessário quando o registro já vai falhar por duplicidade.
- A validação de tamanho mínimo de senha (`>= 3` caracteres) vive no use case, não na
  entidade `User` — porque `User` recebe apenas o hash já computado, nunca a senha em
  texto puro, então não tem como validar o tamanho da senha original.
- Validação de `Email` e `Name` vive inteiramente na entidade `User` (construtor) — o
  use case não duplica essas regras, apenas deixa a exceção de domínio propagar.
- Resposta HTTP usa `Results.Created` com `Location: /api/users/{id}`, mesmo não havendo
  endpoint `GET /api/users/{id}` implementado — segue a convenção REST para criação de
  recurso.

## Edge cases e como são tratados

- E-mail com múltiplos `@`, sem domínio, ou sem `.` no domínio → `InvalidEmailException`
  (lançada pela entidade `User`, ver `IsValidEmail` em `Denarius.Domain/Entities/User.cs`).
- Nome só com espaços em branco → `InvalidNameException` (mesma checagem de nome vazio).
- Senha vazia (`""`) → cai na mesma validação de tamanho mínimo (`InvalidPasswordException`),
  não em uma checagem separada de "campo obrigatório".

## Alternativas descartadas

Nenhuma decisão de projeto alternativa registrada — implementação segue diretamente o
padrão de use case já estabelecido para os demais aggregates (Account, Category, Transaction).
