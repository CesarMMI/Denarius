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

# ── 4. Verificação da stack Docker (API + MariaDB) ───────────────────────────
Step "Verificando stack Docker (API + MariaDB)..."

$EnvFile    = Join-Path $PSScriptRoot ".env"
$EnvExample = Join-Path $PSScriptRoot ".env.example"

docker version | Out-Null
if ($LASTEXITCODE -ne 0) { Fail "Docker não encontrado ou não está rodando. Instale/inicie o Docker Desktop antes de continuar." }

if (-not (Test-Path $EnvFile)) {
  Copy-Item $EnvExample $EnvFile
  Warn "api/.env não existia; criado a partir de api/.env.example (valores de exemplo, ok para este sanity check)."
}

$ApiPort = 8080
$PortLine = Get-Content $EnvFile | Where-Object { $_ -match '^API_PORT=' }
if ($PortLine) { $ApiPort = ($PortLine -split '=', 2)[1].Trim() }

function FailDocker {
  param([string]$msg)
  Write-Host "$FAIL $msg"
  docker compose down -v
  Pop-Location
  exit 1
}

Push-Location $PSScriptRoot

docker compose build
if ($LASTEXITCODE -ne 0) { FailDocker "docker compose build falhou." }
Ok "Imagens Docker construídas."

docker compose up -d
if ($LASTEXITCODE -ne 0) { FailDocker "docker compose up falhou." }

Step "Aguardando MariaDB ficar healthy (timeout 60s)..."
$Healthy = $false
for ($i = 0; $i -lt 30; $i++) {
  $ContainerId = (docker compose ps -q mariadb).Trim()
  if ($ContainerId) {
    $Health = docker inspect --format='{{.State.Health.Status}}' $ContainerId
    if ($Health -eq "healthy") { $Healthy = $true; break }
  }
  Start-Sleep -Seconds 2
}
if (-not $Healthy) {
  Write-Host ""
  Write-Host "  --- Últimas linhas do log do mariadb ---"
  docker compose logs mariadb --no-log-prefix | Select-Object -Last 20 | ForEach-Object { Write-Host "  $_" }
  Write-Host "  -----------------------------------------"
  FailDocker "MariaDB não ficou healthy a tempo."
}
Ok "MariaDB healthy."

Step "Testando endpoint da API (timeout 30s)..."
$RegisterBody = '{"name":"Sanity Check","email":"sanity-check@denarius.local","password":"Senha123!"}'
$ApiUp = $false
for ($i = 0; $i -lt 15; $i++) {
  try {
    $Response = Invoke-WebRequest -Uri "http://localhost:$ApiPort/api/auth/register" -Method Post `
      -ContentType "application/json" -Body $RegisterBody -TimeoutSec 5 -UseBasicParsing
    if ($Response.StatusCode -eq 201) { $ApiUp = $true; break }
  } catch {
    Start-Sleep -Seconds 2
  }
}
if (-not $ApiUp) {
  Write-Host ""
  Write-Host "  --- Últimas linhas do log da api ---"
  docker compose logs api --no-log-prefix | Select-Object -Last 30 | ForEach-Object { Write-Host "  $_" }
  Write-Host "  --------------------------------------"
  FailDocker "API não respondeu com sucesso a tempo (registro de usuário de teste falhou)."
}
Ok "API respondendo — migrations aplicadas e banco acessível (registro de teste retornou 201)."

docker compose down -v
Pop-Location
Ok "Ambiente Docker desligado e limpo (containers e volume removidos)."

# ── Resumo ────────────────────────────────────────────────────────────────────
Write-Host ""
Write-Host "========================================"
Write-Host "  Resumo"
Write-Host "========================================"
Write-Host "$OK $TestsRun projetos de teste executados — todos passaram."
Write-Host "$OK Restore e build concluídos sem erros."
Write-Host "$OK Stack Docker (API + MariaDB) construída, testada de ponta a ponta e limpa."
Write-Host ""
Write-Host "Ambiente pronto para desenvolvimento."
