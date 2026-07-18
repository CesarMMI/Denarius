# auth-login — Plan

Esta feature já está implementada. Este documento reflete o código real.

## Abordagem técnica

Fluxo Clean Architecture padrão do projeto (ver `specs/ARCHITECTURE.md`):

```
POST /api/auth/login
  → AuthEndpoints.MapAuthEndpoints (Denarius.Api/Endpoints/AuthEndpoints.cs)
  → LoginRequest → LoginInput
  → ILoginUseCase → LoginUseCase (Denarius.Application/UseCases/Auth/LoginUseCase.cs)
      1. IUserRepository.GetByEmailAsync(email) — busca o usuário; se null, lança
         InvalidCredentialsException
      2. IPasswordHasher.Verify(password, user.PasswordHash) — se falso, lança
         InvalidCredentialsException (mesma exceção do passo 1)
      3. ITokenService.GenerateToken(user.Id, user.Email, user.Name) — gera o JWT
         (implementação real: JwtTokenService, Denarius.Infrastructure/Auth) — assina
         com Jwt:Secret, inclui claims sub/email/name/jti, expira em Jwt:ExpiryMinutes
      4. new LoginOutput(token, UserOutput.FromEntity(user))
  → Results.Ok(result) — 200
```

Este use case não escreve no banco — não há `IUnitOfWork.CommitAsync()` envolvido.

## Decisões locais

- `GetByEmailAsync` retornando `null` e `Verify` retornando `false` lançam exatamente a
  mesma exceção (`InvalidCredentialsException`, mensagem "Invalid email or password.") —
  ver regra global de anti-enumeração em `specs/ARCHITECTURE.md`.
- Quando o e-mail não é encontrado, `IPasswordHasher.Verify` **não é chamado** — evita
  gastar um hash BCrypt (custoso por design) num usuário que não existe.
- `InvalidCredentialsException` herda `AppException`, não uma exceção 401-específica —
  o `ExceptionMiddleware` mapeia `AppException` para **400**, não 401. Decisão do projeto,
  não bug: evita usar o header `WWW-Authenticate` de 401, que por convenção HTTP poderia
  reforçar a distinção entre "não autenticado" vs "credenciais erradas".
- O token não é persistido em lugar nenhum (sem tabela de sessões) — é *stateless*, a
  validade é inteiramente responsabilidade da assinatura + expiração do JWT.

## Edge cases e como são tratados

- E-mail cadastrado mas com capitalização diferente da armazenada — não há normalização
  de case explícita no use case; o comportamento depende da collation da coluna `Email`
  no MariaDB (não coberto por teste automatizado desta feature).
- Senha vazia (`""`) — segue o mesmo caminho de `Verify` retornando `false`, sem checagem
  separada de campo obrigatório.
- Usuário existente porém `PasswordHash` corrompido/vazio — `Verify` trata como não-match,
  cai no mesmo `InvalidCredentialsException`.

## Alternativas descartadas

Nenhuma decisão de projeto alternativa registrada — implementação segue diretamente o
padrão de use case já estabelecido para os demais aggregates.
