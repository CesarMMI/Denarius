#!/usr/bin/env bash
set -euo pipefail

# Navigate to project root regardless of where the script is called from
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
cd "$SCRIPT_DIR/.."

OK="✅"
FAIL="❌"
WARN="⚠️ "
ARROW="▶"

step() { echo ""; echo "$ARROW $*"; }
ok()   { echo "$OK $*"; }
fail() { echo "$FAIL $*"; exit 1; }
warn() { echo "$WARN $*"; }

echo "========================================"
echo "  Denarius API — Init & Sanity Check"
echo "========================================"

# ── 1. Restore ───────────────────────────────────────────────────────────────
step "Restaurando dependências..."
dotnet restore \
  || fail "dotnet restore falhou. Verifique a conectividade com o NuGet e tente novamente."
ok "Restore concluído."

# ── 2. Build ─────────────────────────────────────────────────────────────────
step "Compilando a solução..."
dotnet build --no-restore \
  || fail "dotnet build falhou. Corrija os erros de compilação antes de continuar."
ok "Build concluído sem erros."

# ── 3. Testes ────────────────────────────────────────────────────────────────
declare -a TEST_PROJECTS=(
  "tests/Denarius.Api.Tests"
  "tests/Denarius.Application.Tests"
  "tests/Denarius.Domain.Tests"
  "tests/Denarius.Infrastructure.Tests"
)
declare -a TEST_LABELS=(
  "API (integração de endpoints)"
  "Application (use cases)"
  "Domain (entidades)"
  "Infrastructure (repositórios com SQLite)"
)

TESTS_RUN=0
for i in "${!TEST_PROJECTS[@]}"; do
  step "Rodando testes: ${TEST_LABELS[$i]}..."
  dotnet test "${TEST_PROJECTS[$i]}" \
    || fail "Testes de ${TEST_LABELS[$i]} falharam. Corrija os testes antes de continuar."
  ok "Testes de ${TEST_LABELS[$i]} passaram."
  TESTS_RUN=$((TESTS_RUN + 1))
done

# ── 4. Verificação de inicialização (não bloqueante) ─────────────────────────
step "Verificando inicialização da aplicação (timeout 10s)..."
warn "Este passo requer MySQL disponível. Falha de conexão não é bloqueante."

STARTUP_LOG=$(mktemp)
dotnet run --project src/Denarius.Api --no-build > "$STARTUP_LOG" 2>&1 &
APP_PID=$!
sleep 10

if kill -0 "$APP_PID" 2>/dev/null; then
  kill "$APP_PID" 2>/dev/null
  wait "$APP_PID" 2>/dev/null || true
  ok "Aplicação iniciou e permaneceu ativa por 10s."
else
  warn "A aplicação encerrou antes do timeout de 10s."
  warn "Provável causa: banco de dados não disponível. Continuando."
  echo ""
  echo "  --- Últimas linhas da saída da aplicação ---"
  tail -20 "$STARTUP_LOG" | sed 's/^/  /'
  echo "  --------------------------------------------"
fi
rm -f "$STARTUP_LOG"

# ── Resumo ────────────────────────────────────────────────────────────────────
echo ""
echo "========================================"
echo "  Resumo"
echo "========================================"
echo "$OK $TESTS_RUN projetos de teste executados — todos passaram."
echo "$OK Restore e build concluídos sem erros."
echo ""
echo "Ambiente pronto para desenvolvimento."
