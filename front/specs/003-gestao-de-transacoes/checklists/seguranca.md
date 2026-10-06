# Checklist de segurança: Gestão de transações

**Objetivo**: Testar a qualidade dos requisitos de segurança e de integridade dos dados financeiros da página de
transações (clareza, completude, consistência e mensurabilidade do que a spec e o plan escrevem), como base para a
aprovação do plan. Não testa o código.

**Criado em**: 2026-10-06

**Feature**: [spec.md](../spec.md) · [plan.md](../plan.md) · [research.md](../research.md) ·
[data-model.md](../data-model.md) · [contracts/transactions-ui.md](../contracts/transactions-ui.md)

**Observação**: Checklist gerado pelo `/speckit-checklist` a partir da spec, dos artefatos do plan e da constituição
(`AGENTS.md` da raiz, princípio II, e `front/AGENTS.md`, restrições técnicas de segurança).

**Responsável pela revisão**: Este checklist é um artefato de revisão da qualidade dos requisitos e pertence ao revisor.
Marque um item `[x]` só quando o revisor concluir que o critério de qualidade do requisito foi atendido.

**Semântica da marcação**: `[x]` quer dizer que o critério foi revisado e atendido quanto à qualidade do requisito. Não
quer dizer que a implementação está pronta.

## Validação de entrada: API e front

- [x] CHK001 A divisão da validação está explícita: as Premissas e a FR-014 dizem quais regras o formulário repete
      (data, categoria, limite da descrição, recusa do zero), quais são só de formato do front (valor sem sinal, até
      duas casas, sem separador de milhar, até 13 algarismos inteiros, data dia/mês/ano) e que a API é a autoridade nas
      regras de negócio, de modo que nenhum requisito dependa só da validação do front? [Clareza, Spec §Premissas,
      §FR-014; AGENTS.md §II] — ok: segunda rodada: a falta de validação de faixa e de escala do valor no back está
      registrada como desvio conhecido do back, sem bloqueio (Premissas, Casos-limite e Esclarecimentos de 2026-10-06;
      Plan §Desvios conhecidos).
- [x] CHK002 Os parâmetros do endereço (`categoryId` e `month`) são tratados como entrada externa, com o destino de cada
      um definido: o `month` validado no front (FR-029) e o `categoryId` repassado sem validação à API, que o recusa com
      `400`? A decisão de não validar o `categoryId` no front está justificada em termos do princípio II? [Completude,
      Spec §FR-028, §FR-029, §Casos-limite; Research §Abertura filtrada pelo endereço] — ok: FR-029, Casos-limite e
      Data-model §Endereço → estado inicial; Research §Abertura filtrada (`format: uuid` → `400`): a API valida, como
      pede o II, e validar no front seria só experiência.
- [x] CHK003 O plan ou o contrato definem que os valores vindos do endereço e dos filtros chegam à API só como
      parâmetros de consulta codificados, nunca concatenados ao caminho da URL, e que o `{id}` de `PUT`/`DELETE` vem só
      da lista devolvida pela API? [Gap, Contracts §API consumida] — ok: segunda rodada: Contracts §API consumida
      (`HttpParams` em `transactions.service.ts:18-23`; o `{id}` vem só da `Transaction` devolvida pela API) e Plan
      §Constitution Check II.
- [x] CHK004 O limite de 13 algarismos inteiros (FR-014) existe porque, acima dele, o valor gravado pode diferir do
      digitado e a API só recusa a partir de cerca de 10^16, com erro genérico. Essa falta de validação no back,
      contrária ao "a API valida toda entrada externa", está registrada como desvio do back com destino, como o
      `dateRef`, ou como premissa aceita? [Gap, Conflict, Spec §Casos-limite; Plan §Desvios conhecidos; AGENTS.md §II] —
      ok: segunda rodada: desvio conhecido do back, com destino no fluxo de bugs do back e sem bloqueio (Casos-limite,
      Premissas e Esclarecimentos de 2026-10-06; Plan §Desvios conhecidos; Research §Desvios conhecidos); a causa da
      alteração dos valores com 14 ou mais algarismos (`parseFloat`, `transaction-form.ts:64`) está no Plan, no Research
      e no Data-model.
- [x] CHK005 O limite de 255 caracteres da descrição e o tratamento de espaços (trim; só espaços vira "sem descrição")
      estão definidos de forma consistente entre a spec, o data-model e a regra do backend citada? [Consistência, Spec
      §FR-014, §FR-015, §Casos-limite; Data-model §Formulário → TransactionInput] — ok: FR-014, FR-015, Casos-limite e
      Data-model §Formulário; o back faz o mesmo (`Transaction.DescriptionMaxLength = 255`; trim e `null` em
      `ValidateDescription`).

## Integridade dos dados financeiros

- [x] CHK006 Os desvios que gravam um dado diferente do digitado (data digitada lida como mês/dia/ano ou como ISO em
      UTC; valores com 14 ou mais algarismos inteiros alterados sem aviso) têm, cada um, requisito, fluxo de destino,
      critério de correção (o teste que reproduz o bug nasce no `/speckit-bug-fix`) e situação de bloqueio documentados?
      [Completude, Spec §FR-014, §Casos-limite; Plan §Desvios conhecidos] — ok: segunda rodada: Plan §Desvios conhecidos
      ganhou a coluna "Bloqueia a entrega?" (data digitada e 14 ou mais algarismos: Sim; zero: Não), com a causa no
      `parseFloat` e o critério de "concluído" do Resultado do portão; o teste nasce no bug-fix.
- [x] CHK007 A classificação desses desvios como não bloqueantes é coerente com a Governança e com o princípio II
      ("vazar ou corromper esses dados é o pior defeito possível"), já que a tabela de desvios do plan cita o princípio
      II para o zero e o limite do valor, mas só as violações de III e de Idioma bloqueiam a entrega? Há justificativa
      registrada ou uma exceção aprovada no `plan.md`? [Conflict, Plan §Desvios conhecidos, §Constitution Check;
      AGENTS.md §Governança] — ok: segunda rodada: Plan §Constitution Check II passa a VIOLAÇÃO CONHECIDA que bloqueia
      (data digitada e 14 ou mais algarismos), e o zero fica como "Requisito", sem bloqueio, coerente com a FR-014, os
      Casos-limite e os Esclarecimentos de 2026-10-06.
- [x] CHK008 Os requisitos de "Desfazer" deixam claro que a restauração cria um registro novo (outro `id`, `createdAt` e
      `updatedAt`) e que isso basta para "a transação volta com a mesma data, valor, categoria e descrição"? [Clareza,
      Spec §FR-021, §SC-004; Data-model §Exclusão → "Desfazer"] — ok: Casos-limite (registro novo; a API não desfaz
      exclusões), Data-model §Exclusão → "Desfazer" e Research §Exclusão; a FR-021 e a SC-004 só prometem data, valor,
      categoria e descrição.
- [x] CHK009 A exclusão sem confirmação, protegida só por um "Desfazer" de 5 segundos que qualquer outra mensagem pode
      substituir (inclusive o erro de um clique duplo), está registrada como risco aceito de perda de dado financeiro?
      [Assumption, Spec §FR-020, §Casos-limite] — ok: Casos-limite (sem confirmação; mensagem substituída) e Research
      §Exclusão (alternativa rejeitada), aprovados como comportamento atual; na segunda rodada, o clique duplo passou a
      defeito do III que bloqueia a entrega (FR-020).
- [x] CHK010 A edição concorrente (vale a última gravação, sem aviso de conflito) está registrada como comportamento
      aceito, com o risco de sobrescrever um dado financeiro sem o usuário perceber? [Assumption, Spec §Casos-limite] —
      ok: Casos-limite (vale a última gravação, sem aviso), aprovado como comportamento atual (Premissas, último item).
- [x] CHK011 O ponto em aberto da FR-016 ("Cancelar", Esc ou clique fora durante a espera, e a resposta que chega depois
      de o diálogo fechar), que pode gravar uma transação que o usuário acredita ter cancelado, está marcado como não
      decidido, com destino no `/speckit-bug-assess` e sem que outro requisito ou artefato suponha uma resposta?
      [Completude, Spec §FR-016, §História 2 cenário 6; Data-model §Transições] — ok: FR-016, História 2 cenário 6,
      Casos-limite e Esclarecimentos de 2026-10-05; o Data-model §Transições descreve só o fluxo atual e remete a espera
      ao `/speckit-bug-assess`.

## Exibição segura de conteúdo

- [x] CHK012 A FR-032 inclui as mensagens da API, mas a SC-008 mede só descrições e nomes de categoria, e o C13 cobre só
      a tabela e as opções do filtro e do formulário. A exibição como texto das mensagens da API (snackbars) tem
      critério mensurável ou cobertura prevista? [Consistência, Spec §FR-032, §SC-008; Plan §Trabalho previsto C13] —
      ok: segunda rodada: a SC-008 inclui as mensagens da API e os snack bars, e o Plan (C13 e Constitution Check II) e
      o Research §Mensagens registram a garantia do Material (`SimpleSnackBar`), sem teste novo, como decidido.
- [x] CHK013 "Em 100% dos lugares da página" (SC-008) está definido com a lista dos lugares (célula da descrição,
      etiqueta da categoria, opções e valor escolhido do filtro "Categoria" e do formulário, snackbars)? [Clareza, Spec
      §SC-008] — ok: segunda rodada: a SC-008 lista os lugares, e o C13 cobre os que estão nos templates do projeto.
- [x] CHK014 A cor da categoria, vinda da API e aplicada como estilo da etiqueta, tem requisito ou premissa sobre o
      formato aceito (`#RRGGBB`) e sobre quem o garante (a API), para que um valor arbitrário não seja aplicado como
      CSS? [Gap, Spec §FR-005; Contracts §Componentes] — ok: Contracts §Componentes (`#RRGGBB`) e
      `categories-api.yaml:159-161` (hexadecimal normalizado, garantido pelo value object `Color` do back); a diretiva
      só deriva tons numéricos e nunca aplica o valor bruto.
- [x] CHK015 A proibição de `innerHTML` com conteúdo dinâmico e de `bypassSecurityTrust*` (restrições do
      `front/AGENTS.md`) está refletida no plan com o escopo da busca feita e com o teste que passa a fixá-la?
      [Rastreabilidade, Plan §Constitution Check II; front/AGENTS.md §Restrições técnicas] — ok: Plan §Constitution
      Check II (busca em `transactions/`, `shared/` e `categories/`; o C13 fixa); conferido: nenhum `innerHTML` nem
      `bypassSecurityTrust*` em `src/app`.

## Mensagens de erro sem detalhes internos

- [x] CHK016 A SC-006 define "detalhes técnicos" só por exemplos (código de status, resposta bruta). A definição basta
      para julgar outras formas, como o `title` do `ProblemDetails`, os nomes de campo do `ValidationProblemDetails` ou
      a mensagem do `HttpErrorResponse`? [Clareza, Spec §SC-006] — ok: Contracts §Erros fixa a regra positiva (só o
      `detail` ou a mensagem padrão), o que exclui o `title`, os `errors` e a mensagem do `HttpErrorResponse`; Research
      §Mensagens rejeita mostrar o `title` e o status.
- [x] CHK017 A premissa de que o `detail` da API nunca traz detalhe interno (stack trace, SQL, caminhos) está
      documentada e rastreada ao back (tratador global de exceções, contrato), já que o front exibe o `detail` como vem?
      [Assumption, Spec §FR-018, §FR-022; Research §Mensagens] — ok: Research §Mensagens e §Dados de teste
      (`GlobalExceptionHandler.cs:25`: `500` com "Ocorreu um erro inesperado.", 4xx com a mensagem da regra de negócio);
      conferido no back.
- [x] CHK018 Os requisitos de erro cobrem todos os modos de falha que o front pode receber nas mutações: erro de rede
      (status 0), `400` de model binding sem `detail`, `404` e `400` com `detail`, `500` com o `detail` genérico e
      resposta sem corpo JSON? [Cobertura, Spec §FR-018, §FR-022; Contracts §Erros] — ok: Contracts §Erros (com
      `detail`: `404`, `400` e `500`; sem `detail`: rede e `400` de model binding; qualquer resposta sem `detail` cai na
      mensagem padrão), FR-018 e FR-022.
- [x] CHK019 Está claro que, nas falhas de carregamento (qualquer status, inclusive o `400` do `categoryId` malformado),
      nenhuma parte do erro é exibida, só "Não foi possível carregar as transações."? [Clareza, Spec §FR-009; Contracts
      §Erros] — ok: FR-009 e Contracts §Erros (qualquer status, inclusive o `400` do `categoryId`).

## Dados financeiros fora de logs e do endereço

- [x] CHK020 A regra "dados financeiros não vão para logs" está declarada como requisito para o front (nenhum
      `console.*`, nenhuma telemetria ou serviço de relatório de erros), e não só constatada como fato no Constitution
      Check? [Completude, Plan §Constitution Check II; AGENTS.md §II] — ok: a regra do II vale para o front pela
      constituição, e o Plan §Constitution Check II a confere (nenhum `console.*`); conferido: nenhuma telemetria nem
      serviço de relatório de erros em `app.config.ts`.
- [x] CHK021 O texto da busca por descrição viaja na query string do `GET /transactions` e pode ser registrado em logs
      de acesso do servidor ou de um proxy. A spec ou o plan tratam essa exposição, ou registram que ela é aceita ou que
      cabe ao back? [Gap, Contracts §API consumida; AGENTS.md §II] — ok: segunda rodada: Research §Exposição da busca na
      query string e Contracts §API consumida (cabe ao back e à implantação; o back não registra URLs).
- [x] CHK022 Está definido que os filtros escolhidos na página não são gravados no endereço nem no histórico do
      navegador, e que só `categoryId` e `month` podem aparecer nele? [Clareza, Spec §Casos-limite; Data-model §Endereço
      → estado inicial] — ok: Casos-limite (o endereço só define os filtros iniciais), Research §Abertura filtrada e
      Data-model §Endereço → estado inicial.

## Transporte, origem e acesso

- [x] CHK023 O requisito de HTTPS fora do desenvolvimento tem evidência no plan, e o plan diz qual URL de API o build de
      produção usa e se ela é um valor real de implantação ou provisório? [Clareza, Plan §Constitution Check II;
      AGENTS.md §II] — ok: segunda rodada: Plan §Constitution Check II (URL de produção `https://localhost:7060/api`,
      provisória, com HTTPS).
- [x] CHK024 A dependência do CORS restrito às origens configuradas no back está registrada: qual origem do front
      precisa estar liberada e quem a mantém? [Gap, Plan §Constitution Check II; AGENTS.md §II] — ok: segunda rodada:
      Plan §Constitution Check II (origens liberadas na configuração do back, que é de implantação).
- [x] CHK025 A ausência de autenticação e de autorização ("um único usuário", Premissas) está registrada como premissa
      de segurança, com o risco aceito (quem alcança a API vê e muda as transações) e com o lugar onde ela será tratada,
      se for? [Assumption, Spec §Premissas] — ok: Premissas (único usuário; "quem pode ver ou mudar as transações está
      fora do escopo", aprovado com a spec); o risco é do projeto inteiro, e não desta feature.

## Dependências e fronteiras

- [x] CHK026 O plan registra que a feature não traz dependência nova e registra também a situação de vulnerabilidades
      conhecidas (`npm audit`) das dependências que ela usa (Angular, Material, CDK), como pede o princípio II? [Gap,
      Plan §Contexto técnico, §Constitution Check II; AGENTS.md §II] — ok: Plan §Contexto técnico e §Constitution Check
      II (nenhuma dependência nova); o II pede o `npm audit` só para dependência nova.
- [x] CHK027 A exceção aprovada à F1 (importar `categories/services`, `types/` e `testing/`) deixa documentado que a
      superfície HTTP continua auditável, com a URL de categorias só no `CategoriesService`, e que a exceção tem prazo
      (o ciclo de refatoração para `shared/`)? [Rastreabilidade, Plan §Acompanhamento de complexidade; Research
      §Fronteiras entre features] — ok: Research §Fronteiras entre features (URL de categorias só no
      `CategoriesService`) e Plan §Acompanhamento de complexidade (válida até o ciclo de refatoração para `shared/`).

## Notas

- Marque `[x]` só depois de a revisão confirmar que o critério de qualidade do requisito foi atendido.
- Deixe desmarcados os itens que ainda pedem esclarecimento, correção ou avaliação do revisor; um item que depende de
  decisão do usuário vira pergunta no portão do plan.
- Feature retroativa: os desvios conhecidos e a exceção à F1 já foram decididos pelo usuário. Os itens sobre eles
  conferem se estão bem documentados (destino, bloqueio da entrega e critério de correção), e não reabrem a decisão.
- O `/speckit-implement` lê o estado das caixas como portão e não altera as marcações.
- O `checklists/requirements.md` tem ciclo próprio, mantido pelo `/speckit-specify` e pelo `/speckit-clarify`.
- Registre achados e comentários ao lado do item; os IDs são sequenciais para facilitar a referência.
