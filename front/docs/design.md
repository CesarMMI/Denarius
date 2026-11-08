# Design

Identidade visual e padrões de interface do front. Vale para toda feature; a spec ou o plan de uma feature só descreve o que difere daqui, e o `plan.md` atualiza este arquivo quando a mudança é permanente. Comportamento de uma tela (filtros, regras de exclusão, fluxos) pertence à spec da feature, e regras de código ao `AGENTS.md`.

## Base

- **Angular Material** com o tema do projeto (`src/styles.scss`). Tokens `--mat-sys-*` para cor, tipografia e forma, sem valores soltos.
- **Temas claro e escuro**, seguindo a preferência do sistema. Cores calculadas usam `light-dark(claro, escuro)`.
- **Campos de formulário** com aparência `outline`. **Tooltips** com 500 ms de atraso para aparecer e 200 ms para sumir.
- **Idioma e formatos**: `pt-BR`. Dinheiro em BRL (`R$ 8.600,00`), data como `dd/mm/aaaa` (`dd/MM` em listas compactas), meses por extenso onde são nomeados.

## Layout

- Menu lateral fixo, com o ícone e o nome "Denarius" no topo. Itens: **Relatórios** (`insights`), **Transações** (`receipt_long`), **Categorias** (`sell`). O item da página aberta fica marcado.
- Cada página abre com um cabeçalho (`shared/page-header`): título em destaque e as ações da página à direita.
- Conteúdo em cards de cantos arredondados, com espaçamento consistente; títulos de bloco dentro do card.
- Tabelas Material; só a lista rola, e o cabeçalho e os filtros ficam à vista.

## Cor

- **Valor negativo** (saída) em vermelho (`--mat-sys-error`, classe global `.negative`). Entradas sem destaque.
- **Categoria** aparece como chip com o nome (`shared/mat-chip-color`): tons 90 e 30 da cor da categoria, o claro no fundo e o escuro no texto, trocados no tema escuro, com cerca de 7,2:1 de contraste (acima do AAA), qualquer que seja a cor. A cor vem da API.
- **Gráficos**: categorias nas cores delas e "Outras" em cor neutra; despesas no mesmo vermelho em todos; na comparação acumulada, mês atual em `primary` e mês anterior em `secondary`. Valores e eixos em pt-BR.
- **Variações** (cards de resumo): seta, percentual e nome do mês anterior; a cor é semântica, e subir é bom para receita e saldo e ruim para despesa.

## Estados e feedback

- **Carregando**: indicador no lugar das linhas, e numa recarga as linhas atuais continuam até a nova versão chegar. **Vazio** e **erro**: texto curto no lugar das linhas ("Nenhuma categoria encontrada.", "Não foi possível carregar as categorias."), e o erro de um bloco independente tem um botão que tenta de novo só ele.
- **Mensagens** (snackbar): sucesso por 3 segundos ("Categoria salva."); erro por 5 segundos com a ação "Fechar"; ação que se pode desfazer por 5 segundos com "Desfazer".
- **Diálogos** de criar e editar têm "Cancelar" e "Salvar". O erro de campo é uma frase curta no próprio campo ("Informe um nome"), e o campo com limite mostra um contador "N/100".
