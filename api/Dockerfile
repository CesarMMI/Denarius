# syntax=docker/dockerfile:1

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY src/Denarius.Domain/Denarius.Domain.csproj src/Denarius.Domain/
COPY src/Denarius.Application/Denarius.Application.csproj src/Denarius.Application/
COPY src/Denarius.Infrastructure/Denarius.Infrastructure.csproj src/Denarius.Infrastructure/
COPY src/Denarius.Api/Denarius.Api.csproj src/Denarius.Api/

RUN dotnet restore src/Denarius.Api/Denarius.Api.csproj

COPY src/ src/

RUN dotnet publish src/Denarius.Api/Denarius.Api.csproj \
    -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

ENV ASPNETCORE_HTTP_PORTS=8080
ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 8080

COPY --from=build /app/publish .

USER app
ENTRYPOINT ["dotnet", "Denarius.Api.dll"]
