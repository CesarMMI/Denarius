#Requires -Version 5.1
$ErrorActionPreference = "Stop"

$OK    = "✅"
$FAIL  = "❌"
$WARN  = "⚠️ "
$ARROW = "▶"

function Step { param([string]$msg) Write-Host ""; Write-Host "$ARROW $msg" }
function Ok   { param([string]$msg) Write-Host "$OK $msg" }
function Fail { param([string]$msg) Write-Host "$FAIL $msg"; exit 1 }
function Warn { param([string]$msg) Write-Host "$WARN $msg" }

Write-Host "========================================"
Write-Host "  Denarius API — Init & Sanity Check"
Write-Host "========================================"

# ── 1. Restore ───────────────────────────────────────────────────────────────
Step "Restaurando dependências..."
dotnet restore
if ($LASTEXITCODE -ne 0) { Fail "dotnet restore falhou. Verifique a conectividade com o NuGet e tente novamente." }
Ok "Restore concluído."

# ── 2. Build ─────────────────────────────────────────────────────────────────
Step "Compilando a solução..."
dotnet build --no-restore
if ($LASTEXITCODE -ne 0) { Fail "dotnet build falhou. Corrija os erros de compilação antes de continuar." }
Ok "Build concluído sem erros."

# ── 3. Testes ────────────────────────────────────────────────────────────────
$TestProjects = @(
  "tests/Denarius.Api.Tests",
  "tests/Denarius.Application.Tests",
  "tests/Denarius.Domain.Tests",
  "tests/Denarius.Infrastructure.Tests"
)
$TestLabels = @(
  "API (integração de endpoints)",
  "Application (use cases)",
  "Domain (entidades)",
  "Infrastructure (repositórios com SQLite)"
)

$TestsRun = 0
for ($i = 0; $i -lt $TestProjects.Length; $i++) {
  Step "Rodando testes: $($TestLabels[$i])..."
  dotnet test $TestProjects[$i]
  if ($LASTEXITCODE -ne 0) { Fail "Testes de $($TestLabels[$i]) falharam. Corrija os testes antes de continuar." }
  Ok "Testes de $($TestLabels[$i]) passaram."
  $TestsRun++
}

# ── 4. Verificação de inicialização (não bloqueante) ─────────────────────────
Step "Verificando inicialização da aplicação (timeout 10s)..."
Warn "Este passo requer MySQL disponível. Falha de conexão não é bloqueante."

$StartupLog = [System.IO.Path]::GetTempFileName()
$StartupErr = [System.IO.Path]::GetTempFileName()
$AppProc = Start-Process -FilePath "dotnet" `
  -ArgumentList @("run", "--project", "src/Denarius.Api", "--no-build") `
  -RedirectStandardOutput $StartupLog `
  -RedirectStandardError $StartupErr `
  -PassThru -NoNewWindow
Start-Sleep -Seconds 10

if (-not $AppProc.HasExited) {
  $AppProc.Kill()
  $AppProc.WaitForExit()
  Ok "Aplicação iniciou e permaneceu ativa por 10s."
} else {
  Warn "A aplicação encerrou antes do timeout de 10s."
  Warn "Provável causa: banco de dados não disponível. Continuando."
  Write-Host ""
  Write-Host "  --- Últimas linhas da saída da aplicação ---"
  $allOutput = @()
  if (Test-Path $StartupLog) { $allOutput += Get-Content $StartupLog }
  if (Test-Path $StartupErr) { $allOutput += Get-Content $StartupErr }
  $allOutput | Select-Object -Last 20 | ForEach-Object { Write-Host "  $_" }
  Write-Host "  --------------------------------------------"
}
Remove-Item $StartupLog -Force -ErrorAction SilentlyContinue
Remove-Item $StartupErr -Force -ErrorAction SilentlyContinue

# ── Resumo ────────────────────────────────────────────────────────────────────
Write-Host ""
Write-Host "========================================"
Write-Host "  Resumo"
Write-Host "========================================"
Write-Host "$OK $TestsRun projetos de teste executados — todos passaram."
Write-Host "$OK Restore e build concluídos sem erros."
Write-Host ""
Write-Host "Ambiente pronto para desenvolvimento."
