# Checklist de segurança: Gestão de categorias

**Objetivo**: Testar a qualidade dos requisitos de segurança da página de categorias (spec retroativa): se o que está
escrito na spec e no plan sobre exibição de dados externos, validação, mensagens de erro, logs, transporte, asset
estático da paleta e dependências está completo, claro, consistente e verificável, conforme o princípio II do
`AGENTS.md` da raiz e as restrições de segurança do `front/AGENTS.md`
**Criado em**: 2026-10-06
**Feature**: [spec.md](../spec.md) · [plan.md](../plan.md) · [research.md](../research.md) ·
[data-model.md](../data-model.md) · [contracts/categories-ui.md](../contracts/categories-ui.md)

**Observação**: Checklist gerado pelo `/speckit-checklist` a partir da spec, do plan e dos artefatos da Fase 1. Os
itens testam os requisitos escritos, e não o código. Como a feature é retroativa, os desvios conhecidos e as exceções já
decididos pelo usuário não são reabertos: os itens só conferem se estão bem documentados (destino, bloqueio e critério
de correção).
**Responsável pela revisão**: Este checklist é um artefato de revisão da qualidade dos requisitos, de posse do revisor.
Marque `[x]` só quando o revisor concluir que o critério está atendido.
**Semântica das marcações**: `[x]` significa que o critério foi revisado e está atendido quanto à qualidade dos
requisitos. Não significa que a implementação esteja concluída.

## Completude dos requisitos

- [x] CHK001 A FR-021 lista todos os dados externos que a página exibe (nome da categoria no chip e no campo de edição,
      `detail` das mensagens da API, quantidade e saldo), ou deixa algum de fora? [Completude, Spec §FR-021, Spec
      §Casos-limite] — ok: FR-021 e Casos-limite cobrem os dois dados textuais (nomes e mensagens da API); quantidade e
      saldo são números formatados (FR-005), e o nome no diálogo é valor de campo de formulário, sem interpretação.
- [x] CHK002 Algum requisito define como a cor vinda da API, usada para pintar o chip e como nome acessível das cores,
      pode ser tratada: só como valor de cor, sem virar estilo ou marcação arbitrária? [Gap, Spec §FR-003, Spec
      §FR-020, Spec §FR-021] — ok: Data-model §Tipos do contrato (`color` '#RRGGBB', normalizada pela API), Spec
      §Premissas (a API recusa cores inválidas e devolve a cor normalizada) e FR-022 (a API é a autoridade sobre a cor);
      o nome acessível por código hexadecimal vale só para a paleta estática (FR-020).
- [x] CHK003 Está escrito que a validação do diálogo (nome obrigatório, até 100 caracteres) serve só à experiência e
      que a API valida tudo de novo, inclusive a cor livre da "Cor personalizada", que o front não valida? [Completude,
      Spec §Premissas, Spec §FR-022, Data-model §Tipos do contrato] — ok: Spec §Premissas ("serve à experiência do
      usuário; a API valida tudo de novo… recusa… cores inválidas") e Data-model, linha `color`.
- [x] CHK004 Há requisito, na spec ou no plan, de que nomes, saldos e quantidades não vão para logs, inclusive nos
      caminhos de erro, e não só a constatação de que hoje não há `console.*`? [Gap, Plan §Constitution Check II] — ok: segunda rodada (2026-10-06): o Plan §Constitution Check II cobre os caminhos de erro (todo `subscribe` trata o erro em `categories-page.ts:79,89`, e o `httpResource` guarda o erro no estado, então nada chega ao `ErrorHandler` global de `app.config.ts:10`).
- [x] CHK005 A exigência de HTTPS fora de desenvolvimento está documentada como requisito da página, com a origem da
      URL da API (`src/environments/`), e não só como o valor atual do `environment.ts`? [Completude, Plan
      §Constitution Check II] — ok: a exigência é da constituição (princípio II), vale para a aplicação inteira e não
      precisa ser repetida como requisito da página; o Plan §Constitution Check II registra a origem (`src/environments/`),
      o HTTPS do build de produção e a troca pelo `fileReplacements` (conferido em `angular.json:56-60`). Observação: o
      `environment.ts` de produção aponta para `https://localhost:7060/api`, ou seja, não há destino de implantação
      definido; isso é da aplicação, fora desta feature (igual na 003).
- [x] CHK006 A dependência do CORS do back (a origem do front na lista de origens permitidas) está registrada no plan
      como premissa da página? [Gap, Dependência, Plan §Constitution Check IV] — ok: segunda rodada (2026-10-06): os pré-requisitos do Quickstart registram o CORS do back em `Development` (`appsettings.Development.json:11-12`, `http://localhost:4200`) e a lista de produção como configuração de implantação, fora da feature.
- [x] CHK007 O plan registra que nenhuma dependência nova entra e o resultado do `npm audit` das dependências usadas
      (Material/CDK 21.2, Signal Forms), como pede o princípio II para dependências? [Completude, Plan §Contexto técnico]
      — ok: Plan §Contexto técnico ("Nenhuma dependência nova"). O princípio II pede `npm audit` só para dependência
      nova; exigir o resultado das existentes vai além da constituição.

## Clareza dos requisitos

- [x] CHK008 "Exibidos como texto, nunca interpretados" (FR-021) é objetivo o bastante para o revisor decidir se uma
      interpolação, um tooltip, um `aria-label` ou o texto do snack bar o atendem? [Clareza, Spec §FR-021] — ok: FR-021
      com Research §Exibição como texto (interpolação e texto no `MatSnackBar.open`; nada de `innerHTML`); dicas e
      `aria-label` da página são textos fixos (FR-020). A mensurabilidade fica no CHK017.
- [x] CHK009 A FR-014 deixa claro de onde vem a "mensagem de erro da API" (o `detail` do `ProblemDetails`) e o que
      acontece quando a resposta traz outro formato, como uma página HTML de um proxy? [Ambiguity, Spec §FR-014,
      Contracts §API consumida] — ok: Contracts §API consumida ("a página mostra o `detail`… Só sem `ProblemDetails`… a
      página usa os seus textos") e Research §Mensagens (`error.error?.detail`); um corpo HTML não tem `detail` e cai
      no texto da página (Spec §Casos-limite).
- [x] CHK010 Está claro que o texto da busca vai à API só como parâmetro de consulta codificado (`?name=`), e que o
      back o aplica por consulta parametrizada, sem que a página monte nada por concatenação? [Clareza, Contracts §API
      consumida, Spec §FR-017] — ok: Data-model §Filtros da página (o `CategoriesService.list` traduz os filtros em
      parâmetros de consulta) e Contracts (`list` devolve `{ url, params }`); a consulta parametrizada é do back
      (princípio II, LINQ em `CategoryRepository.cs:14-15`).
- [x] CHK011 O endereço público do link da quantidade (FR-004) está limitado a dados não financeiros (o id da categoria
      e o mês), sem nome, saldo ou quantidade, que iriam para o histórico e para logs de acesso? [Clareza, Spec §FR-004,
      Spec §Premissas] — ok: a FR-004 fixa o endereço exato (`/transactions?categoryId=<id>` e, com mês,
      `&month=YYYY-MM`); nenhum outro dado entra.

## Consistência dos requisitos

- [x] CHK012 A FR-021 é coerente com a restrição do `front/AGENTS.md` (nada de `innerHTML` com conteúdo dinâmico nem
      `bypassSecurityTrust*`) e com o princípio II (sanitização do Angular ligada), sem exceção registrada no plan?
      [Consistência, Spec §FR-021, Plan §Constitution Check II, Research §Exibição como texto] — ok: Plan §Constitution
      Check II e Research §Exibição como texto; nenhuma exceção de segurança no Acompanhamento de complexidade.
- [x] CHK013 Mostrar o `detail` da API sem filtro (FR-014) é coerente com "erros não expõem detalhes internos", dado
      que a página confia que o back só põe ali mensagens de domínio ou o texto genérico do 500? [Consistência,
      Assumption, Spec §FR-014, Research §Mensagens de sucesso e de erro] — ok: a premissa está no Research §Mensagens e
      no Contracts §API consumida, e foi conferida no back: `GlobalExceptionHandler.cs:25` põe "Ocorreu um erro
      inesperado." em todo 500 e, no 400/404, só a mensagem das exceções de domínio e de aplicação (textos fixos, como
      "Categoria não encontrada."); o 400 automático de validação do ASP.NET vem sem `detail` e cai no texto da página.
      Uma mensagem de domínio que vaze detalhe seria defeito do back (princípio II).
- [x] CHK014 A FR-006 (falha ao carregar a lista mostra só um texto fixo) e a FR-014 (falha ao salvar ou excluir mostra
      o `detail` da API) seguem uma regra comum e documentada sobre quando expor a mensagem da API? [Consistência, Spec
      §FR-006, Spec §FR-014] — ok: cada requisito é explícito sobre o texto que mostra, e os dois atendem ao princípio
      II (texto fixo ou `detail` sem detalhe interno, CHK013); a constituição não pede uma regra comum.
- [x] CHK015 O bloqueio da exclusão de categorias em uso está atribuído à API como autoridade, com o "Excluir"
      desabilitado só como experiência, de forma coerente entre FR-012, FR-022 e SC-003? [Consistência, Spec §FR-012,
      Spec §FR-022, Spec §SC-003] — ok: FR-022 (a API é a autoridade sobre a recusa da exclusão), SC-003 (botão
      desabilitado ou recusa da API) e US3, cenário 4 (lista desatualizada).
- [x] CHK016 O limite de 100 caracteres do front e o da API estão documentados com o mesmo valor e a mesma regra de
      contagem (com ou sem os espaços das pontas)? [Consistência, Spec §FR-008, Spec §Casos-limite, Data-model §Tipos do
      contrato] — ok: segunda rodada (2026-10-06): a coluna "Regra na API" do Data-model §Tipos do contrato diz que o back conta os 100 depois de remover os espaços das pontas (`Category.cs:34-40`) e que o limite do front, que conta os espaços, é o mais restrito.

## Qualidade dos critérios de aceitação

- [x] CHK017 A FR-021 tem um critério de aceitação verificável, já que o quickstart a dá como coberta "por construção,
      sem teste próprio"; essa ausência de teste está registrada como decisão, à luz do princípio I? [Mensurabilidade,
      Gap, Quickstart §1, Spec §FR-021] — ok: segunda rodada (2026-10-06): decisão do usuário registrada (Spec §Esclarecimentos 2026-10-06, Research §D12). O critério verificável é o exemplo dos Casos-limite (`<b>teste</b>` literal no rótulo), conferido pelo TC-10 do plan; o snack bar fica como garantia do Material, sem teste (Research §Exibição como texto, Quickstart, linha FR-021).
- [x] CHK018 Os textos de fallback da página ("Não foi possível salvar a categoria.", "Não foi possível excluir a
      categoria.", "Não foi possível carregar as categorias.") estão listados de forma que se possa conferir que nenhum
      expõe detalhe interno? [Mensurabilidade, Contracts §Mensagens, Spec §FR-006, Spec §FR-014] — ok: FR-006, FR-014 e
      Contracts §Mensagens os listam por extenso; nenhum traz detalhe técnico.

## Cobertura de cenários e casos-limite

- [x] CHK019 Está especificado o que a página mostra quando o `detail` da API é muito longo, vazio ou contém marcação,
      e se ele continua exibido como texto? [Edge Case, Gap, Spec §FR-014, Spec §FR-021] — ok: segunda rodada (2026-10-06): a marcação é coberta pela FR-021 e pelo exemplo dos Casos-limite; o `detail` vazio está no Research §Mensagens (mostraria mensagem vazia, inalcançável com o back atual); o longo também é inalcançável, porque os `detail` do back são textos fixos e curtos (mesma seção).
- [x] CHK020 O conteúdo inesperado da paleta estática (valores que não são hexadecimais, formato JSON diferente) tem
      tratamento definido, além da falha de rede silenciosa? [Edge Case, Gap, Spec §Casos-limite, Data-model §Paleta
      padrão] — ok: a paleta é um arquivo da própria aplicação, versionado, e não uma entrada externa; o conteúdo está
      fixado na Spec §Premissas (11 cores, nessa ordem) e no Data-model §Paleta padrão. Um tratamento para formato
      inesperado não é exigido.
- [x] CHK021 As respostas `404` (categoria excluída em outra aba) ao editar ou excluir estão cobertas pela regra geral
      da FR-014, com mensagem ao usuário e sem recarga? [Cobertura, Spec §FR-014, Contracts §API consumida] — ok: a
      FR-014 cobre qualquer recusa ou falha ao criar, editar, excluir ou desfazer, e o Contracts §API consumida diz que
      o 404 chega como `ProblemDetails` ("Categoria não encontrada.", `UpdateCategoryUseCase.cs:13`,
      `DeleteCategoryUseCase.cs:12`).
- [x] CHK022 A consequência do "Desfazer" criar uma categoria com outra identidade está documentada para os endereços
      já salvos ou compartilhados com o id antigo (`/transactions?categoryId=<id antigo>`)? [Edge Case, Gap, Spec
      §Premissas, Spec §FR-004, Spec §FR-013] — ok: Spec §Premissas (outra identidade; o lado que recebe o endereço é
      da 003), e a 003, nos Casos-limite, diz que uma categoria que não existe traz a lista vazia. Como só se exclui
      categoria sem transações, o endereço antigo já mostrava uma lista vazia; não há efeito de segurança nem de
      integridade.
- [x] CHK023 A integridade dos dados num "Desfazer" depois de a mensagem ser substituída, ou num "Desfazer" repetido,
      está definida (pode criar duplicatas, já que os nomes não precisam ser únicos)? [Edge Case, Spec §Casos-limite,
      Spec §Premissas] — ok: Casos-limite (só uma mensagem por vez; uma exclusão cuja mensagem foi substituída não pode
      mais ser desfeita) e US3, cenário 2 ("Desfazer" mostra "Categoria restaurada.", que substitui a mensagem da
      exclusão). Não há caminho para um segundo "Desfazer" nem para duplicata.

## Dependências e premissas

- [x] CHK024 A premissa de que autenticação e autorização estão fora do escopo está explícita e é coerente com o resto
      da aplicação, sem que a página dependa de uma proteção que não existe? [Assumption, Spec §Premissas] — ok: Spec
      §Premissas (última); o "Excluir" desabilitado é só experiência, e a autoridade é a API (FR-022).
- [x] CHK025 A paleta como asset estático de mesma origem, versionado com a aplicação (`front/public/data/`), está
      documentada como fonte confiável, com o motivo de não passar pela validação da API? [Assumption, Spec §Premissas,
      Research §Paleta padrão] — ok: Spec §Premissas ("vem com a aplicação, e não da API"), Research §Paleta padrão e
      Data-model §Paleta padrão. A cor escolhida na paleta passa pela validação da API ao ser salva, como qualquer cor.
- [x] CHK026 A exceção à F1 (o `CategoryForm` provoca o `GET` da paleta pelo `ColorsService`) tem o escopo limitado ao
      asset estático, de modo que toda chamada à API continua só nos services, superfície HTTP auditável que a F1
      protege? [Consistência, Plan §Acompanhamento de complexidade, Research §D4] — ok: Plan §Acompanhamento de
      complexidade ("a superfície HTTP auditável que a F1 protege continua só nos services") e Research §D4.

## Desvios conhecidos e exceções (documentação)

- [x] CHK027 O desvio do `dateRef` (contrato diz `date-time`, o back aceita e o front envia `YYYY-MM-DD`) está
      documentado com destino (fluxo de bugs do back), com o motivo de não bloquear e com a premissa de que o back
      valida o formato recebido? [Plan §Desvios conhecidos, Research §D3, Contracts §API consumida] — ok: Plan §Desvios
      conhecidos (destino e "não bloqueia"), Research §D3 e Contracts; o back aceita e testa `YYYY-MM-DD`
      (`CategoriesControllerTests.cs:211`). Validar o parâmetro é do back (princípio II), fora desta feature.
- [x] CHK028 Os desvios que não são de segurança (diálogo que fecha antes da resposta, consulta repetida, calendário em
      inglês, dia do "Mês", link sem o mês, busca sensível a maiúsculas) foram avaliados quanto ao impacto em segurança
      e integridade dos dados, e o resultado está registrado? [Gap, Plan §Desvios conhecidos] — ok: a coluna "Princípio
      afetado" do Plan §Desvios conhecidos classifica cada um (III, Idioma, IV ou conformidade com a spec), e o
      Constitution Check II passa; nenhum desvio grava ou altera dados de forma indevida.
- [x] CHK029 O plan registra como o princípio II será reconferido pelos bug-fix e pela refatoração que tocam
      `shared/` (`month-field`, `sort-menu`, filtros, `Category` movido para `shared/`), para que nenhum deles
      introduza exibição como HTML ou chamada HTTP fora dos services? [Gap, Plan §Dependências e riscos] — ok: a
      reconferência é garantida pela constituição (AGENTS.md raiz, Governança: toda revisão confere a aderência; o
      revisor de implement e de bug-fix confere segurança e fronteiras), e o Plan §Dependências e riscos exige rodar os
      specs de `categories/` nesses ciclos. Um teste de exibição como texto depende do CHK017.

## Observações

- Marque `[x]` só depois que a revisão confirmar que o critério de qualidade do requisito está atendido.
- Deixe desmarcados os itens que ainda precisam de esclarecimento, correção ou avaliação do revisor.
- O `/speckit-implement` lê o estado das marcações como portão e não deve alterá-las.
- O `checklists/requirements.md` tem ciclo próprio, mantido pelo `/speckit-specify` e pelo `/speckit-clarify`.
- Registre comentários e achados junto ao item.
- Os itens são numerados em sequência para facilitar a referência.
