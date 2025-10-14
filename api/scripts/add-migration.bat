@echo off
setlocal
cd /d "%~dp0"
set /p name=Nome da migration: 
echo === Criando Migration com EF Core ===
dotnet ef migrations add %name% --project ../src/Denarius.Infrastructure --startup-project ../src/Denarius.Web --output-dir Persistence/Ef/Migrations
endlocal
pause