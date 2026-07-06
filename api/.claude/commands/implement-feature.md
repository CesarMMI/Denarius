Você é um engenheiro sênior trabalhando neste projeto. Antes de qualquer coisa,
rode `./init.ps1` e pare se houver falhas.

## Entrada do usuário

O argumento recebido foi: $ARGUMENTS

### Interprete o argumento da seguinte forma:

**Se vazio:**
Leia `harness/feature_list.json` e liste as features com status `pending` ou
`in-progress`, numeradas. Pergunte qual o usuário quer implementar antes de prosseguir.

**Se "next", "nxt", "próximo" ou variações:**
Leia `harness/feature_list.json` e selecione automaticamente a feature com maior
prioridade seguindo esta ordem:
1. Status `in-progress` — retomar o que foi iniciado
2. Status `done` e `verified: false` — adicionar testes antes de novas features
3. Status `pending` — primeira da lista

Informe ao usuário qual feature foi selecionada e o motivo antes de prosseguir.

**Se um ID existente no feature_list.json:**
Selecione essa feature diretamente.

**Se texto livre que não corresponde a nenhum ID:**
Busque no `feature_list.json` por features com descrição semelhante.
- Se encontrar correspondência próxima: apresente a feature encontrada e pergunte
  se o usuário quer implementar essa ou criar uma nova
- Se não encontrar: informe que a feature não existe e pergunte se deseja criá-la
  como nova entrada no `feature_list.json` antes de implementar

---

## Antes de implementar

1. Leia o `CLAUDE.md` para relembrar arquitetura e convenções
2. Leia o `harness/progress.md` para entender o estado atual
3. Leia os markdowns de documentação relevantes para a feature selecionada
4. Se a feature não tiver documentação correspondente, crie o markdown antes de
   implementar, conforme a regra definida no `CLAUDE.md`
5. Apresente um plano de implementação ao usuário e aguarde confirmação

---

## Implementação

Siga o plano confirmado. A cada etapa concluída:
- Rode `dotnet build --no-restore` para garantir que não há erros de compilação
- Se houver erro, corrija antes de continuar

Ao finalizar a implementação:
- Rode `dotnet test` nos projetos relevantes para a feature
- Se houver falha, corrija antes de prosseguir

---

## Verificação final

Rode `./init.ps1` completo. Só prossiga se tudo passar.

---

## Atualização do harness

Com a implementação verificada, atualize os arquivos do harness:

**`harness/feature_list.json`**
- Se a feature existia: atualize `status` e `verified` conforme o resultado
- Se a feature foi criada agora: adicione a entrada com status `done` e
  `verified: true` se os testes passaram

**`harness/progress.md`**
- Atualize a seção "Última sessão" com o que foi feito
- Atualize "Próxima sessão" com a feature de maior prioridade restante
- Mova observações resolvidas para fora e adicione novas se surgiram

**`CLAUDE.md`**
- Se uma nova documentação foi criada, adicione a referência na seção correta

---

## Ao finalizar

Reporte no chat:
- ✅ O que foi implementado
- 🧪 Resultado dos testes
- 📄 Arquivos do harness que foram atualizados
- ⚠️ Qualquer observação aberta que o usuário deve revisar