# auth-login — Tasks

Feature já implementada — tarefas abaixo documentam o estado atual e servem como checklist de regressão.

- [x] Input/Output/Interface do use case (`LoginInput`, `LoginOutput`, `ILoginUseCase`)
      Verificação: `dotnet build` compila sem erros; `LoginOutput` expõe `Token` e `User`
      (`UserOutput`), nunca `PasswordHash`.
- [x] `LoginUseCase` busca usuário por e-mail, verifica senha via `IPasswordHasher.Verify`
      e gera token via `ITokenService.GenerateToken`
      Verificação: `tests/Denarius.Application.Tests/UseCases/Auth/LoginUseCaseTests.cs`
      — happy path, e-mail desconhecido (não chama `Verify`), senha incorreta (não chama
      `GenerateToken`).
- [x] E-mail desconhecido e senha incorreta lançam a mesma `InvalidCredentialsException`
      Verificação: `LoginUseCaseTests.Execute_WithUnknownEmail_ThrowsInvalidCredentialsException`
      e `Execute_WithWrongPassword_ThrowsInvalidCredentialsException`.
- [x] Registro de DI do use case em `Denarius.Application/DependencyInjection.cs`
      Verificação: `dotnet build` resolve `ILoginUseCase` sem erro em runtime.
- [x] Endpoint `POST /api/auth/login` mapeado em `AuthEndpoints.MapAuthEndpoints`
      Verificação: `tests/Denarius.Api.Tests/Endpoints/AuthEndpointsTests.cs` — retorna 200
      com `LoginOutput` no happy path.
- [x] Geração real do JWT (`JwtTokenService`) usa `Jwt:Secret`/`Issuer`/`Audience`/`ExpiryMinutes`
      e inclui claims `sub`, `email`, `name`, `jti`
      Verificação: `Denarius.Infrastructure/Auth/JwtTokenService.cs`; sem teste unitário
      dedicado a esta feature (coberto indiretamente pelos testes de integração de
      endpoints autenticados).
- [x] Mapeamento de exceção → HTTP via `ExceptionMiddleware`
      Verificação: `InvalidCredentialsException` (`AppException`) retorna 400, não 401 —
      decisão intencional documentada em `plan.md`.
- [ ] Nenhuma pendência conhecida — suíte completa (`dotnet test`) deve passar sem falhas antes
      de qualquer nova alteração neste fluxo.
      Verificação: `dotnet test` na raiz de `api/` sem falhas.
