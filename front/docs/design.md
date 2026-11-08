# Design

Identidade visual e padrões de interface do front. Vale para toda feature; a spec ou o plan de uma feature só descreve o que difere daqui, e o `plan.md` atualiza este arquivo quando a mudança é permanente. As regras de código (Material primeiro, desktop como alvo) estão em F4 do `AGENTS.md` do front.

## Base

- **Angular Material** com o tema do projeto (`src/styles.scss`). Use os tokens `--mat-sys-*` para cor, tipografia e forma, e evite valores soltos.
- **Temas claro e escuro** seguem a preferência do sistema. Cores calculadas usam `light-dark(claro, escuro)`, como em `shared/mat-chip-color`.
- **Campos de formulário** com aparência `outline` (padrão em `app.config.ts`). **Tooltips** com 500 ms de atraso para aparecer e 200 ms para sumir.
- **Idioma e formatos**: `pt-BR`. Dinheiro em BRL (`R$ 8.600,00`), data como `dd/mm/aaaa` (`dd/MM` em listas compactas), meses por extenso onde são nomeados.
- **Alvo**: navegadores desktop. Telas estreitas só quando a spec pedir.

## Estrutura da aplicação

- Menu lateral fixo, com o ícone e o nome "Denarius" no topo. Itens, em ordem: **Relatórios** (`insights`), **Transações** (`receipt_long`), **Categorias** (`sell`). O item da página aberta fica marcado.
- A página inicial (`/`) é a de Transações.
- Cada página tem um cabeçalho (`shared/page-header`) com o título e as ações da página. As ações de cabeçalho são "Recarregar", "Exibir filtros"/"Ocultar filtros" e o botão de criar ("Nova categoria", "Nova transação").
- Rotas são públicas: renomear ou remover uma exige redirecionamento (F2).

## Padrões de tela

- **Listas** são tabelas Material, todas as linhas de uma vez, sem paginação, rolando só a lista (cabeçalho e filtros ficam à vista).
- **Filtros** começam ocultos, ocultar não limpa, e cada filtro preenchido tem um botão para limpá-lo. Filtro vazio não é enviado.
- **Estados** de uma lista ou bloco: carregando (indicador no lugar das linhas só quando não há linhas a mostrar; numa recarga, as linhas atuais ficam até a nova versão chegar), vazio ("Nenhuma categoria encontrada.") e erro ("Não foi possível carregar as categorias."). Cada bloco independente tem os seus três estados, e o de erro tem um botão que tenta de novo só ele.
- **Criar e editar** abrem um diálogo com "Cancelar" e "Salvar". Campos obrigatórios têm mensagem curta no campo ("Informe um nome"), e campos com limite mostram um contador "N/100".
- **Excluir** é imediato, sem confirmação, com "Desfazer" na mensagem. O botão fica desabilitado, com dica, quando a regra de negócio proíbe.
- **Mensagens** (snackbar): sucesso por 3 segundos sem ação ("Categoria salva."); exclusão por 5 segundos com "Desfazer"; erro por 5 segundos com "Fechar", mostrando a mensagem da API e, sem ela, um texto genérico da ação ("Não foi possível salvar a categoria.").

## Cor

- **Valor negativo** (saída) em vermelho (`--mat-sys-error`, classe global `.negative`). Entradas sem destaque.
- **Categoria**: aparece como chip com o nome (`shared/mat-chip-color`). A diretiva usa os tons 90 e 30 da cor da categoria, o claro no fundo e o escuro no texto, trocados no tema escuro, com cerca de 7,2:1 de contraste, acima do AAA. A cor vem da API; o front não a infere de outra fonte.
- **Gráficos** (Chart.js, só no chunk da rota que os usa): categorias nas cores delas e "Outras" em cor neutra; despesas no mesmo vermelho em todos os gráficos; na comparação acumulada, o mês atual em `primary` e o anterior em `secondary`. Valores e eixos em pt-BR.
- **Variações** em cards de resumo: seta, percentual e nome do mês anterior; a cor é semântica (subir é bom para receita e saldo, e ruim para despesa).

## Componentes compartilhados

Em `src/app/shared/`: `page-header`, `month-field` (seletor de mês, mostrado como MM/aaaa), `sort-menu`, `mat-chip-color`, `datepicker-intl` (calendário em português), `date-utils`, `reload-when-idle` e `shallow-equal`. Antes de criar um componente, procure aqui; o que serve a mais de uma feature vai para este diretório (F1).
