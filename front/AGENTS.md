# Denarius — front

SPA em Angular 21 (componentes standalone, signals, signal forms e `httpResource`), com Angular Material e CDK, testada com Vitest via `@angular/build:unit-test`. A URL da API vem de `src/environments/`. Este arquivo acrescenta as regras específicas do front às do [`AGENTS.md`](../AGENTS.md) da raiz (TDD, segurança, performance, contrato com o back e fluxo de trabalho).

## Princípios

### F1. Organização das features

- Cada feature vive na sua pasta em `src/app/` (`categories/`, `transactions/`, `reports/`…), dividida em `pages/`, `components/`, `services/`, `types/` e `testing/`.
- URLs, parâmetros e chamadas HTTP ficam só nos `services/*.service.ts` da feature. As páginas guardam o estado (`httpResource`) e orquestram diálogos e snackbars; os componentes de `components/` (filtros, tabela, formulário) são de apresentação e não fazem HTTP.
- Uma feature não importa o interior de outra; o que serve a mais de uma vai para `shared/`.
- Uma feature nova segue o padrão existente (página, filtros, tabela, formulário em diálogo, service, types e rotas carregadas sob demanda a partir de `app.routes.ts`), usando só as partes de que precisa.

_Por quê:_ essas fronteiras mantêm cada feature testável isoladamente e concentram a superfície HTTP em um lugar auditável.

### F2. Contrato e rotas

- Os tipos em `types/` espelham o contrato do back (`back/specs/*/contracts/`), sem campos supostos.
- As rotas do front são públicas (o usuário as salva e compartilha): renomear ou remover uma rota exige redirecionar o caminho antigo.

_Por quê:_ um link salvo que quebra é a primeira regressão que o usuário vê.

### F3. Sem banco de dados

- O front não tem banco. Mudança de schema é entregue no back, com rollback (B3 em `back/AGENTS.md`). Se a feature depende de uma, o `plan.md` cita a migration e o rollback, e o front funciona com o schema de antes e de depois do rollback.

### F4. Angular Material primeiro

- O Angular Material é a base da interface: prefira seus componentes, tokens (`--mat-sys-*`) e tema a HTML, CSS ou TypeScript próprios, e mantenha o código próprio no mínimo de que a feature precisa.
- O alvo são navegadores desktop; layout responsivo ou mobile só quando a spec pedir.

## Restrições técnicas

- **Performance**: rotas carregadas sob demanda (`loadChildren`); bibliotecas pesadas, como o Chart.js, só no chunk da rota que as usa; budgets do `angular.json` respeitados: bundle inicial com aviso em 700 kB e erro em 1 MB, estilos por componente com aviso em 4 kB e erro em 8 kB.
- **Segurança**: nada de `innerHTML` com conteúdo dinâmico nem de `bypassSecurityTrust*`.
- **Qualidade**: o código passa no `ng lint` (angular-eslint) e segue o Prettier do projeto.

## Testes e verificação

De dentro de `front/`:

- `npx ng test --watch=false`: a suíte inteira passa, e cada arquivo novo ou alterado tem o seu `*.spec.ts` ao lado.
- `npm run lint`
- `npm run build`: sem erro de budget.
- Se o back também mudou, `dotnet test` em `back/` passa.
