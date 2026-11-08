# Modelo de dados: Gestão de categorias

O front não armazena nada. O seu modelo são os formatos que ele troca com a API de categorias, tipados em
`front/src/app/categories/types/` para espelhar
[`back/specs/001-category-management/contracts/categories-api.yaml`](../../../back/specs/001-category-management/contracts/categories-api.yaml),
e o estado da página, que vive enquanto ela está aberta. As regras de negócio (validação, exclusão, cálculo da
quantidade e do saldo) são do back ([data-model do back](../../../back/specs/001-category-management/data-model.md)).

## Tipos do contrato (`types/category.ts`)

```ts
interface Category {
	// CategoryOutput
	id: string; // uuid
	name: string; // como a API salvou (sem espaços nas pontas)
	color: string; // '#RRGGBB', normalizada pela API
	transactionCount: number; // no período: o mês do dateRef ou todo o período
	balance: number; // no mesmo período; decimal da API como número JSON
	canDelete: boolean; // false se há transações em qualquer mês, independentemente do dateRef
	createdAt: string; // ISO date-time
	updatedAt: string; // ISO date-time
}

type CategoryInput = Pick<Category, 'name' | 'color'>; // CreateCategoryInput / UpdateCategoryInput
```

| Campo       | Regra no front                                                                                                                                                                    | Regra na API (autoridade)                                                                                                                                                                                                                                                                                                 |
| ----------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `name`      | Obrigatório (`Validators.required`, "Informe um nome") e até 100 caracteres pelo `maxlength` do campo (`category-form.html:7-9`, `category-form.ts:23,28`); enviado como digitado | Recusa vazio ou só espaços; remove os espaços das pontas e só então conta o limite de 100 caracteres (`back/src/Denarius.Domain/Entities/Category.cs:34-40`); não exige nome único. Como o front conta também os espaços das pontas, o limite dele é o mais restrito: nada que ele aceita é recusado pela API por tamanho |
| `color`     | Começa em `#000000` e, numa categoria nova, passa à primeira cor da paleta quando ela chega (`category-form.ts:29,34-37`); vem de um botão da paleta ou do `input type="color"`   | Aceita hexadecimal com ou sem `#`, 3 ou 6 dígitos; recusa vazia ou inválida; devolve normalizada                                                                                                                                                                                                                          |
| `canDelete` | Desabilita o "Excluir" e troca a dica (`categories-table.html:33-44`)                                                                                                             | Recusa a exclusão com 400 e `detail` quando há transações                                                                                                                                                                                                                                                                 |

## Filtros da página (`types/category-filters.ts`)

Tipo só do front (não é do contrato); o `CategoriesService.list` o traduz em parâmetros de consulta
(`categories.service.ts:17-24`). Um filtro vazio não é enviado.

| Campo             | Tipo                                         | Vazio quando              | Parâmetro enviado                                                                                                                              |
| ----------------- | -------------------------------------------- | ------------------------- | ---------------------------------------------------------------------------------------------------------------------------------------------- |
| `name`            | `string`                                     | `''`                      | `name` (como digitado; a API ignora texto em branco)                                                                                           |
| `withTransaction` | `boolean \| ''`                              | `''` ("Todas")            | `withTransaction=true\|false`                                                                                                                  |
| `month`           | `Date \| null` (dia 1 do mês, horário local) | `null` ("Todos os meses") | `dateRef=YYYY-MM-DD` (`DateUtils.toDateKey`) — o contrato documenta `date-time`: desvio conhecido do back, só de documentação (D3 da pesquisa) |

Ordenação (`Sort` do Material): `active` ∈ `name | transactionCount | balance` (o `CategoryOrderField` do contrato) e
`direction` ∈ `asc | desc`, enviados como `orderBy` e `asc=true|false`.

## Estado da página (`CategoriesPage`)

| Estado           | Tipo                                       | Regra                                                                                                                                                                                                                                                                                                                                                                |
| ---------------- | ------------------------------------------ | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `filters`        | `WritableSignal<CategoryFilters>`          | Começa `{ name: '', withTransaction: '', month: null }` (`categories-page.ts:40`); ligado aos filtros por `[(filters)]`; continua valendo com o painel oculto                                                                                                                                                                                                        |
| `filtersVisible` | `WritableSignal<boolean>`                  | Começa `false` (`:41`); alternado pelo botão "Exibir filtros"/"Ocultar filtros"                                                                                                                                                                                                                                                                                      |
| `sort`           | `WritableSignal<Sort>`                     | Começa `{ active: 'name', direction: 'asc' }` (`:43`); ligado ao `app-sort-menu` por `[(sort)]`                                                                                                                                                                                                                                                                      |
| `categories`     | `HttpResourceRef<Category[] \| undefined>` | `httpResource` de `list(filters(), sort())` (`:53-55`): uma mudança de `filters` ou `sort` refaz a requisição e limpa o valor (spinner); o `reload()` (Recarregar, salvar, excluir, desfazer) mantém as linhas até a resposta. Uma mudança de `filters` ou `sort` com uma requisição em andamento cancela a anterior, e só a resposta da última combinação é exibida |

Nada disso vai para a URL; cada visita recomeça do padrão (Premissa da spec).

**Atualização (2026-10-08)**, depois dos bugs `front/bugs/consulta-repetida-sem-mudanca/` e
`front/bugs/clique-repetido-em-excluir/` (`verified`); conferido em `categories-page.ts`:

- `filters` e `sort` são criados com `{ equal: shallowEqual }` (`shared/shallow-equal/`): gravar o mesmo conteúdo
  (a mesma ordenação, o mesmo mês ou o mesmo texto no "Nome") não notifica, e o `httpResource` não refaz a consulta
  (FR-023, SC-006).
- `deleting: WritableSignal<ReadonlySet<string>>`, começa vazio: os ids das categorias em exclusão. O id entra no
  clique e só sai se a API recusar; depois de aceita a exclusão, continua até a linha sair da lista. Um clique numa
  linha cujo id já está em `deleting` não envia outro `DELETE`. A tabela recebe `[deleting]`, desabilita "Editar" e
  "Excluir" da linha (`disabledInteractive`) e troca o ícone do "Excluir" por um spinner (FR-012).
- `restoring: WritableSignal<number>`, começa em 0: quantas restaurações ("Desfazer") estão em andamento. Sobe ao
  enviar o `POST` e desce quando ele termina, aceito ou recusado; enquanto é maior que 0, o cartão da lista mostra uma
  `mat-progress-bar` ("Restaurando a categoria") (FR-013).
- As recargas depois de salvar, excluir e desfazer usam o `reloadWhenIdle` (`shared/reload-when-idle/`): se a lista
  já estiver carregando, a recarga fica pendente e sai quando essa carga termina com sucesso; se ela falhar, a recarga
  é descartada e o "Recarregar" continua sendo a saída. O "Recarregar" do cabeçalho segue chamando `reload()` direto.
- As citações de linha da tabela acima (`categories-page.ts:40`, `:41`, `:43` e `:53-55`) não valem mais.

## Paleta padrão

| Item   | Regra                                                                                                                                                                                                        |
| ------ | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ |
| Origem | `front/public/data/default-colors.json`: `#F4511E`, `#F6BF26`, `#43A047`, `#8E24AA`, `#1E88E5`, `#D81B60`, `#00897B`, `#FB8C00`, `#C0CA33`, `#78909C`, `#E53935`                                             |
| Carga  | `ColorsService` (`providedIn: 'root'`): um `httpResource` criado na primeira injeção, ou seja, na primeira abertura do diálogo; `colors()` é `[]` enquanto carrega ou após falha (`colors.service.ts:10-13`) |
| Uso    | Um botão por cor, com `aria-label` = código hexadecimal e `aria-pressed` = escolhida, num grupo "Cor"; a "Cor personalizada" (`input type="color"`) existe sempre (`category-form.html:12-27`)               |

## Transições

```text
Lista:     carregando ──► carregada (linhas | "Nenhuma categoria encontrada.")
                     └──► erro ("Não foi possível carregar as categorias.") ──Recarregar──► carregando
           filtro/ordenação mudou ──► carregando (sem linhas)      reload() ──► linhas mantidas até a resposta

Diálogo:   aberto ──Salvar (válido)──► fechado com CategoryInput ──► POST/PUT
                  ──Cancelar/Esc/fora──► fechado sem valor ──► nada
           (desvio conhecido FR-010: o esperado é fechar só depois de a API aceitar)

Exclusão:  DELETE ──204──► reload + "Categoria excluída." [Desfazer, 5 s] ──Desfazer──► POST {name, color}
                  └─4xx/5xx─► mensagem da API ou "Não foi possível excluir a categoria." [Fechar, 5 s]
```

**Atualização (2026-10-08)**: as transições acima continuam valendo, com estes acréscimos (ver o Estado da página):

```text
Lista:     filtro/ordenação com o mesmo conteúdo ──► nada (sem consulta, linhas mantidas)
           reload depois de salvar/excluir/desfazer com a lista carregando ──► pendente ──carga ok──► reload()
                                                                                       └─carga com erro─► descartado

Exclusão:  clique ──► id em `deleting` (linha bloqueada, spinner no "Excluir") ──► DELETE
                  ──204──► id continua em `deleting` até a linha sair da lista
                  └─4xx/5xx─► id sai de `deleting` (linha liberada) + mensagem de erro
           novo clique com o id em `deleting` ──► nada
           Desfazer ──► restoring + 1 (barra "Restaurando a categoria") ──POST termina──► restoring − 1
```
