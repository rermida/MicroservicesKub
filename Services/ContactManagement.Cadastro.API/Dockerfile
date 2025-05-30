# Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copia os arquivos .csproj para restaurar dependências
COPY ["Services/ContactManagement.Cadastro.API/ContactManagement.Cadastro.API.csproj", "Services/ContactManagement.Cadastro.API/"]
COPY ["Shared/ContactManagement.Domain/ContactManagement.Domain.csproj", "Shared/ContactManagement.Domain/"]
COPY ["Shared/ContactManagement.Messages/ContactManagement.Messages.csproj", "Shared/ContactManagement.Messages/"]

# Restaura as dependências
RUN dotnet restore "Services/ContactManagement.Cadastro.API/ContactManagement.Cadastro.API.csproj"

# Copia apenas os arquivos necessários para build
COPY ["Services/ContactManagement.Cadastro.API/", "Services/ContactManagement.Cadastro.API/"]
COPY ["Shared/ContactManagement.Domain/", "Shared/ContactManagement.Domain/"]
COPY ["Shared/ContactManagement.Messages/", "Shared/ContactManagement.Messages/"]

# Publica o projeto
WORKDIR "/src/Services/ContactManagement.Cadastro.API"
RUN dotnet publish -c Release -o /app/publish

# Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "ContactManagement.Cadastro.API.dll"]
