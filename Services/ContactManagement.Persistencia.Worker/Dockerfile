# Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copiar projetos por pasta (ajustado para a estrutura correta)
COPY ["Services/ContactManagement.Persistencia.Worker/ContactManagement.Persistencia.Worker.csproj", "Services/ContactManagement.Persistencia.Worker/"]
COPY ["Shared/ContactManagement.Infrastructure/ContactManagement.Infrastructure.csproj", "Shared/ContactManagement.Infrastructure/"]
COPY ["Shared/ContactManagement.Domain/ContactManagement.Domain.csproj", "Shared/ContactManagement.Domain/"]
COPY ["Shared/ContactManagement.Messages/ContactManagement.Messages.csproj", "Shared/ContactManagement.Messages/"]

# Restaurar dependências
RUN dotnet restore "Services/ContactManagement.Persistencia.Worker/ContactManagement.Persistencia.Worker.csproj"

# Copia apenas os arquivos necessários para build
COPY ["Services/ContactManagement.Persistencia.Worker/", "Services/ContactManagement.Persistencia.Worker/"]
COPY ["Shared/ContactManagement.Infrastructure/", "Shared/ContactManagement.Infrastructure/"]
COPY ["Shared/ContactManagement.Domain/", "Shared/ContactManagement.Domain/"]
COPY ["Shared/ContactManagement.Messages/", "Shared/ContactManagement.Messages/"]

# Publicar o projeto
WORKDIR "/src/Services/ContactManagement.Persistencia.Worker"
RUN dotnet publish -c Release -o /app/publish

# Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "ContactManagement.Persistencia.Worker.dll"]
