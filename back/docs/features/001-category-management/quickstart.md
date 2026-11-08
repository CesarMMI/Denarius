# Guia rápido: validação da gestão de categorias

Esta feature já está implementada; este guia confere que ela continua funcionando, e não como
construí-la. As regras de cada campo estão em [data-model.md](./data-model.md); os formatos
completos de requisição/resposta estão em [contracts/categories-api.yaml](./contracts/categories-api.yaml).

## Pré-requisitos

- SDK do .NET 10
- PostgreSQL acessível pela connection string em `ConnectionStrings:DenariusDb` (já configurada
  para o desenvolvimento local em `src/Denarius.WebAPI/appsettings.Development.json`)
- Esquema do banco atualizado:
  ```
  dotnet ef database update --project src/Denarius.Infrastructure --startup-project src/Denarius.WebAPI
  ```

## Validação automatizada (a principal — não precisa de servidor rodando)

Rode só as suítes relacionadas a Category:

```
dotnet test tests/Denarius.Domain.Tests --filter FullyQualifiedName~CategoryTests
dotnet test tests/Denarius.Application.Tests --filter FullyQualifiedName~Categories
```

Esperado: todos os testes passam. Juntas, elas exercitam todos os requisitos funcionais do
`spec.md` (FR-001…FR-015), exceto a própria camada HTTP (veja `research.md` → Lacuna de cobertura
de testes).

Ou a suíte completa, como pede o Princípio IV da constituição:

```
dotnet test
```

## Smoke test manual de ponta a ponta (também exercita a camada HTTP)

1. Suba a API: `dotnet run --project src/Denarius.WebAPI` (o perfil HTTP escuta em
   `http://localhost:5276`, conforme o `launchSettings.json`).
2. Crie uma categoria:
   ```
   curl -i -X POST http://localhost:5276/api/categories \
     -H "Content-Type: application/json" \
     -d '{"name":"Lazer","color":"#FF0000"}'
   ```
   Esperado: `201 Created`, um cabeçalho `Location: /api/categories/{id}` e um corpo que devolve a
   categoria com `transactionCount: 0` e `balance: 0`.
3. Liste as categorias: `curl http://localhost:5276/api/categories` — espere a categoria nova no
   array, ordenado por nome em ordem crescente por padrão (História de usuário 1, História de
   usuário 3).
4. Atualize-a:
   ```
   curl -i -X PUT http://localhost:5276/api/categories/{id} \
     -H "Content-Type: application/json" \
     -d '{"name":"Diversao","color":"#0F0"}'
   ```
   Esperado: `200`, nome alterado e cor normalizada para `#00FF00`.
5. Exclua-a: `curl -i -X DELETE http://localhost:5276/api/categories/{id}` — espere `204`.
6. Confirme a remoção: repita o passo 3 — a categoria não deve mais estar na lista.
7. Caso-limite de validação: repita o passo 2 com `"name":""` ou `"color":"not-a-color"` — espere
   `400` com um corpo `ProblemDetails` explicando o que está errado (História de usuário 1,
   cenário 5).
8. Caso-limite da proteção de exclusão: crie uma categoria, registre uma transação nela via
   `POST /api/categories` → `POST /api/transactions` (API de transações — feature própria, fora do
   escopo aqui) e tente excluí-la — espere `400`; a categoria deve continuar na lista do passo 3.
9. Estatísticas de uso: repita o passo 8 mais uma vez com um valor de transação conhecido e depois
   faça `GET /api/categories?dateRef=<esse mês>` — espere que `transactionCount`/`balance`
   reflitam só as transações desse mês (História de usuário 2).
