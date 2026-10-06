# Guia rápido: validação da gestão de transações

Esta feature já está implementada; este guia confere que ela continua funcionando, e não como construí-la. Os tipos e
as conversões estão em [data-model.md](./data-model.md), e as requisições e os endereços, em
[contracts/transactions-ui.md](./contracts/transactions-ui.md). Onde o comportamento atual é um desvio conhecido da
spec, o passo diz o que se vê **hoje**; a correção é de outro fluxo (ver o `plan.md`).

## Pré-requisitos

- Node 24 e as dependências instaladas (`npm ci` em `front/`).
- Para a verificação manual: a API do backend em `http://localhost:5276` (a URL de `environment.development.ts`), com o
  banco atualizado, como nos pré-requisitos e no passo 1 de
  [back/specs/002-transaction-management/quickstart.md](../../../back/specs/002-transaction-management/quickstart.md),
  e pelo menos duas categorias (por exemplo, "Mercado" e "Salário"; passo 2 do mesmo guia).

## 1. Validação automatizada

A partir de `front/`:

```
npx ng test --watch=false --include='src/app/transactions/**/*.spec.ts' --include='src/app/shared/**/*.spec.ts' --include='src/app/app.spec.ts'
npx ng test --watch=false
npm run lint
npm run build
```

Esperado: os specs da feature, os de `shared/` e o `app.spec.ts` passam; o lint passa limpo; o build não emite erro nem
aviso de budget e a página aparece como um chunk lazy próprio (`transactions-page`). Só os budgets do `angular.json` são
portão (inicial com aviso em 700 kB e erro em 1 MB; estilos por componente com aviso em 4 kB e erro em 8 kB); não há
budget para chunk lazy, e os números abaixo são só referência.

Situação conferida em 2026-10-05 (HEAD `a93db31`): o primeiro comando passa (11 arquivos, 104 testes); lint e build
passam (Initial total 633,08 kB; `transactions-page` 39,88 kB); a suíte completa tem 222 de 223 testes passando. A falha
é em `src/app/reports/services/chart-theme.service.spec.ts:29` (`primaryVariant`), fora desta feature e anterior a este
plano; ela está no fluxo de bugs do front, e o implement desta feature só começa com a suíte completa verde. Depois do
implement, o primeiro comando inclui também os testes de caracterização C1 a C13 e os ajustes A1 a A3 do `plan.md`.

## 2. Verificação manual

Suba o front com `npm start` e abra `http://localhost:4200/` numa janela de 1366×768, a referência desktop, a mesma do
painel de relatórios. Os passos que dizem "Hoje (desvio …)" descrevem o comportamento atual de um desvio conhecido; os
desvios que bloqueiam a entrega estão no Resultado do portão do `plan.md`.

1. **Lista (História 1)**: o endereço vira `/transactions`, "Transações" fica marcado no menu e a lista vem das mais
   recentes para as mais antigas, com data dd/mm/aaaa, descrição, etiqueta da categoria na cor dela e valor em reais; as
   saídas aparecem negativas e em vermelho. Duas transações vizinhas do mesmo dia não têm divisória entre elas. Com
   muitas transações e os filtros à vista, só a lista rola, e o cabeçalho e os filtros continuam à vista, com a grade
   dos filtros numa linha: é aqui que a rolagem real e a altura limitada pelo `app.scss` são conferidas, porque o teste
   C5 afirma só a estrutura e os estilos.
2. **Criar e editar (História 2)**: "Nova transação" abre com "Saída", valor vazio, a data de hoje e a primeira
   categoria. Salve "12,50" com a descrição `"  Pão  "` (com espaços nas pontas): aparece "Transação criada." e a linha
   "Pão" com "-R$ 12,50". "Editar" nela abre com "12,50"; mude para "Entrada" e "200" e salve: "Transação salva." e "R$
   200,00". Hoje (desvio da FR-016) o diálogo fecha assim que "Salvar" é escolhido, antes da resposta da API.
3. **Validação do formulário**: "12,345", "8.600,00" e "-12" mostram "Informe um valor válido" e o diálogo continua
   aberto; "12.50" é aceito. Hoje (desvio da FR-014) "0" passa e a API recusa com "O valor da transação não pode ser
   zero." (não bloqueia); digitar "05/09/2026" na data salva 9 de maio, e "99999999999999,99" é salvo como R$
   99.999.999.999.999,98 (os dois bloqueiam a entrega).
4. **Excluir e desfazer (História 3)**: "Excluir" numa linha a remove na hora e mostra "Transação excluída." com
   "Desfazer"; escolher "Desfazer" em até 5 s a traz de volta com os mesmos dados e mostra "Transação restaurada.". Hoje
   (desvio da FR-020, que bloqueia a entrega), nada indica a exclusão em andamento, e um clique duplo em "Excluir"
   mostra "Transação não encontrada." no lugar da mensagem com "Desfazer".
5. **Filtros e ordenação (História 4)**: "Exibir filtros" mostra Descrição, Tipo, Categoria e Mês. Digitar parte de uma
   descrição recarrega a lista depois de uma pausa; "Saídas", uma categoria e um mês restringem a lista; cada "Limpar …"
   desfaz o seu filtro. No menu de ordenação, "Menor valor primeiro" põe a maior saída no topo e o botão passa a se
   chamar "Ordenar: Menor valor primeiro". "Ocultar filtros" mantém a lista filtrada. Hoje (desvio da FR-031),
   escolher de novo a mesma ordenação ou o mesmo mês faz o spinner aparecer, porque a consulta é refeita.
6. **Endereço (História 5)**: abra `/transactions?month=2026-09`, `/transactions?categoryId=<id de uma categoria>` e os
   dois juntos: os filtros abrem à vista, preenchidos, e a lista vem filtrada. `/transactions?month=setembro` abre sem
   filtro de mês e com os filtros ocultos. O "Ver todas" do painel de relatórios e o link da quantidade de transações
   da página de categorias chegam aqui da mesma forma.
7. **Erros**: pare a API e escolha "Recarregar": a lista mostra "Não foi possível carregar as transações." e "Nova
   transação" mostra "Não foi possível carregar as categorias. Tente novamente."; suba a API e escolha "Recarregar" de
   novo para voltar à lista. Edite uma transação já excluída em outra aba: aparece "Transação não encontrada.".
8. **Consultas (SC-007)**: com a aba Rede das ferramentas do navegador aberta, ao abrir a página há uma consulta a
   `/api/transactions` e uma a `/api/categories`; cada mudança de filtro ou de ordenação faz uma nova consulta só a
   `/api/transactions`.
