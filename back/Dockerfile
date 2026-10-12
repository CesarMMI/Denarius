# Imagem de desenvolvimento: SDK + dotnet-ef. O código vem por volume (ver compose.yaml).
FROM mcr.microsoft.com/dotnet/sdk:10.0

RUN dotnet tool install --global dotnet-ef --version 10.0.11
ENV PATH="${PATH}:/root/.dotnet/tools" \
    DOTNET_CLI_TELEMETRY_OPTOUT=1 \
    DOTNET_NOLOGO=1

WORKDIR /app
