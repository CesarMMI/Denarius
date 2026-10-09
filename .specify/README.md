# Manutenção do Spec Kit

Notas para manter o Spec Kit deste repositório. A constituição e o fluxo de trabalho vivem nos `AGENTS.md`.

- Adaptações deste repositório:
  - as skills `speckit-*` leem os `AGENTS.md` no lugar de `.specify/memory/constitution.md`, e o `/speckit-specify` cria as specs no `specs/` do projeto;
  - o `.specify/scripts/bash/create-new-feature.sh` usa o `specs/` do projeto de onde é executado (ou o de `SPECIFY_SPECS_DIR`);
  - a extensão `bug` grava os relatórios em `<projeto>/bugs/<slug>/`: o ajuste está nos comandos em `.specify/extensions/bug/commands/`, no README e no `extension.yml` dela, e nas skills `speckit-bug-*` geradas a partir dos comandos.
- Por causa delas, o `specify integration status` lista arquivos modificados, o que é esperado, e um upgrade as desfaz. O `specify integration upgrade claude` para sem alterar nada enquanto houver skills modificadas e só segue com `--force`, que sobrescreve skills, scripts e templates. O `specify extension update` substitui a extensão `bug` sem aviso. Depois de qualquer um deles, use o `git diff` para reaplicar as adaptações sobre os arquivos novos.
- O `specify init --here --force` também sobrescreve as adaptações e recria `.specify/memory/constitution.md`. Se esse arquivo reaparecer, apague-o: a constituição vive nos `AGENTS.md`.
- Com uma extensão instalada, os scripts precisam de um Python 3 funcional para ler `.specify/extensions/.registry`. Se `python3` não funcionar (no Windows, costuma ser só o atalho da Microsoft Store), aponte `SPECKIT_PYTHON_EXECUTABLE` para um Python 3 com PyYAML, como o do `specify-cli` instalado pelo uv.
