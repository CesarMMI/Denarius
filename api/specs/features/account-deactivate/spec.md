# account-deactivate

Permite que um usuário autenticado desative uma conta financeira que possui, sem excluí-la.

> Entidade de referência: `specs/entities/account.md`

## Entrada
- `AccountId`

> `UserId` vem do token JWT do usuário autenticado, não é informado explicitamente pelo chamador.

## Happy path
- Conta com o `AccountId` informado existe e pertence ao usuário autenticado
- Conta marcada como inativa (`IsActive = false`)
- Resposta: sem conteúdo (204)
- Se a conta já estiver inativa, a operação não tem efeito adicional e ainda assim responde com sucesso (204) — é idempotente

## Erros
- Conta não encontrada
- Conta existe mas pertence a outro usuário (tratado como não encontrada — ver `plan.md`)

## Fora de escopo
- Excluir a conta permanentemente — contas com transações vinculadas não podem ser excluídas, apenas desativadas; esta feature nunca remove o registro
- Reativar uma conta desativada — não existe operação de reativação
- Alterar `Name`, `Color`, `CurrencyCode` ou `Balance` — feito exclusivamente pela feature de atualização de conta
- Qualquer distinção de resposta entre "conta não existe" e "conta pertence a outro usuário" — do ponto de vista do chamador, ambos os casos são idênticos
