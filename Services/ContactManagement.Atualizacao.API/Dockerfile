# Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copiar os arquivos .csproj
COPY ["Services/ContactManagement.Atualizacao.API/ContactManagement.Atualizacao.API.csproj", "Services/ContactManagement.Atualizacao.API/"]
COPY ["Shared/ContactManagement.Messages/ContactManagement.Messages.csproj", "Shared/ContactManagement.Messages/"]

# Restaurar dependências
RUN dotnet restore "Services/ContactManagement.Atualizacao.API/ContactManagement.Atualizacao.API.csproj"

# Copia apenas os arquivos necessários para build
COPY ["Services/ContactManagement.Atualizacao.API/", "Services/ContactManagement.Atualizacao.API/"]
COPY ["Shared/ContactManagement.Messages/", "Shared/ContactManagement.Messages/"]

# Publicar a aplicação
WORKDIR "/src/Services/ContactManagement.Atualizacao.API"
RUN dotnet publish -c Release -o /app/publish

# Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "ContactManagement.Atualizacao.API.dll"]
