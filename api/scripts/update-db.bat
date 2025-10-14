@echo off
echo === Atualizando DB EF Core ===
dotnet ef database update --project ../src/Denarius.Infrastructure --startup-project ../src/Denarius.Web
pause
