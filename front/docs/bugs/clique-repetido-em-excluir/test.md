# Verificação do bug: o "Excluir" aceita clique repetido e nada mostra a exclusão ou a restauração em andamento

- **Slug**: clique-repetido-em-excluir
- **Testado em**: 2026-10-07
- **Avaliação**: ./assessment.md
- **Correção**: ./fix.md
- **Resultado**: verified

## Resumo

O bug não se reproduz mais: nas páginas de categorias e de transações, um segundo clique no "Excluir" da mesma linha
não envia outro `DELETE` (nem antes da resposta, nem depois dela e antes de a lista recarregar), a linha mostra o
spinner com "Excluir" e "Editar" desabilitados, a restauração mostra a barra de progresso e a recarga pedida durante
outra carga não se perde. Os 18 testes de reprodução falham sem a correção e passam com ela. A suíte completa (335
testes), o lint e o build do front passam, sem regressão nem aviso de budget.

## Verificações realizadas

Todos os comandos rodados em `front/`, sobre o estado commitado da correção (`a62d42f`, árvore de trabalho limpa).

| Verificação | Comando / ação | Resultado | Observações |
| --- | --- | --- | --- |
| Prova de detecção (correção revertida) | `git checkout a62d42f~1 --` nos 10 arquivos de produção das páginas e das tabelas (`categories-page`, `transactions-page`, `categories-table`, `transactions-table`: `.ts`, `.html` e, nas páginas, `.scss`); `npx ng test --watch=false` com `--include` dos 4 specs | fail (esperado) | `Tests 18 failed \| 90 passed (108)`: os 7 testes de reprodução de cada página e os 2 de cada tabela. A pasta `shared/reload-when-idle/` ficou no lugar. Os specs compilaram; as tabelas falharam em execução com `NG0303: Can't set value of the 'deleting' input`. Os arquivos foram restaurados com `git checkout HEAD -- front/src/app`, e o `git status` voltou a ficar limpo. |
| Guardas (correção revertida) | a mesma execução | pass | Nas duas páginas, passaram: "should keep "Excluir" of the other rows enabled…", "should enable "Excluir" again when the deletion is refused", "restoring › should hide the progress once the restoration is accepted" e "… is refused". O "should leave the other rows as they are" das tabelas falhou com o mesmo `NG0303`, porque depende do input novo `deleting` (desvio já registrado no `fix.md`). |
| Reprodução (depois da correção) | os 4 specs, mais `reload-when-idle.spec.ts` | pass | `Test Files 5 passed (5)`, `Tests 113 passed (113)`. Os testes de reprodução são o equivalente automatizado da reprodução da avaliação, que também foi feita por spec temporário. |
| Testes novos do `reloadWhenIdle` | `reload-when-idle.spec.ts` (incluído na linha acima) | pass | 5 testes. Não fizeram parte da prova de detecção: sem o `reload-when-idle.ts` o spec não compila (`TS2307`, registrado no `fix.md`). |
| Suíte de regressão | `npx ng test --watch=false` | pass | `Test Files 31 passed (31)`, `Tests 335 passed (335)`. |
| Lint | `npm run lint` | pass | "All files pass linting." |
| Build e budgets | `npm run build` | pass | Sem aviso nem erro de budget; "Initial total" 633,08 kB (aviso em 700 kB), igual à referência. |
| Conferência visual no navegador (spinner no `matIconButton` e barra de restauração sobre as linhas) | — | not-run | Pendência do `fix.md`. Os testes conferem o estado no DOM (botões desabilitados, spinner dentro do botão, `mat-progress-bar` presente), mas o tamanho e o alinhamento do spinner e a posição da barra não foram vistos num navegador. Os passos manuais da avaliação (API lenta pela limitação de rede do DevTools) continuam disponíveis. |

## Trechos da saída

```text
# specs das páginas e das tabelas, com a correção revertida
 × should disable its "Excluir" and "Editar", show a spinner in "Excluir" and emit nothing   (cada tabela)
   Error: NG0303: Can't set value of the 'deleting' input on the '_CategoriesTable' component. …
 × should leave the other rows as they are                                                     (cada tabela, guarda)
 × should not send another DELETE when "Excluir" is clicked again before the response          (cada página)
   AssertionError: expected [ TestRequest{ …(3) }, …(1) ] to have a length of 1 but got 2
 × should not send another DELETE after the deletion is accepted and before the list reloads   (cada página)
   AssertionError: expected [ TestRequest{ …(3) } ] to have a length of +0 but got 1
 × should disable "Excluir" and show a spinner in the row while it is being deleted            (cada página)
 × should disable "Editar" while the row is being deleted                                      (cada página)
   AssertionError: expected false to be true // Object.is equality
 × restoring › should show that the restoration is in progress                                 (cada página)
   AssertionError: expected null not to be null
 × when the list is already loading › should reload again when a deletion is accepted during a reload
 × when the list is already loading › should reload again when a deletion is accepted while the filters load the list
   Error: Expected one matching request for criteria "Match by function: ", found none.
 Test Files  4 failed (4)
      Tests  18 failed | 90 passed (108)

# os mesmos specs e o do reloadWhenIdle, com a correção
 Test Files  5 passed (5)
      Tests  113 passed (113)

# suíte completa
 Test Files  31 passed (31)
      Tests  335 passed (335)

# lint
All files pass linting.

# build
                    | Initial total       | 633.08 kB |               148.69 kB
Application bundle generation complete.
```

## Riscos residuais

- A aparência do spinner no botão e da barra de restauração não foi conferida no navegador (`not-run`).
- Os guardas "should leave the other rows as they are" das tabelas não puderam rodar contra o código antigo (dependem
  do input `deleting`); as outras linhas são protegidas também pelos guardas das páginas, que passaram antes e depois.
- O "Recarregar" do cabeçalho continua chamando o `reload()` direto e é ignorado se a lista já carrega (fora do escopo,
  pendência do `fix.md`).
- `plan.md`, `research.md`, `tasks.md` e checklists das features 002 e 003 podem ainda citar o desvio como pendente;
  ficam para o `/speckit-converge`.

## Recomendação

Encerrar o bug: os 18 testes de reprodução falham sem a correção e passam com ela, os guardas das páginas passam nos
dois casos, e a suíte (335 testes), o lint e o build do front estão verdes, sem aviso de budget. A conferência visual
no navegador é opcional e não impede o fechamento; em seguida, rodar o `/speckit-converge` das features 002 e 003.
