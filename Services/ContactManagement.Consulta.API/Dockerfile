# Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copiar arquivos .csproj com caminhos relativos ao contexto de build (Microservices/)
COPY ["Services/ContactManagement.Consulta.API/ContactManagement.Consulta.API.csproj", "Services/ContactManagement.Consulta.API/"]
COPY ["Shared/ContactManagement.Domain/ContactManagement.Domain.csproj", "Shared/ContactManagement.Domain/"]
COPY ["Shared/ContactManagement.Infrastructure/ContactManagement.Infrastructure.csproj", "Shared/ContactManagement.Infrastructure/"]
COPY ["Shared/ContactManagement.Messages/ContactManagement.Messages.csproj", "Shared/ContactManagement.Messages/"]

# Restaurar dependências
RUN dotnet restore "Services/ContactManagement.Consulta.API/ContactManagement.Consulta.API.csproj"

# Copia apenas os arquivos necessários para build
COPY ["Services/ContactManagement.Consulta.API/", "Services/ContactManagement.Consulta.API/"]
COPY ["Shared/ContactManagement.Domain/", "Shared/ContactManagement.Domain/"]
COPY ["Shared/ContactManagement.Infrastructure/", "Shared/ContactManagement.Infrastructure/"]
COPY ["Shared/ContactManagement.Messages/", "Shared/ContactManagement.Messages/"]

# Publicar a aplicação
WORKDIR "/src/Services/ContactManagement.Consulta.API"
RUN dotnet publish -c Release -o /app/publish

# Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "ContactManagement.Consulta.API.dll"]
