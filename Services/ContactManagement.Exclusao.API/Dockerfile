# Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copia os arquivos .csproj
COPY ["Services/ContactManagement.Exclusao.API/ContactManagement.Exclusao.API.csproj", "Services/ContactManagement.Exclusao.API/"]
COPY ["Shared/ContactManagement.Messages/ContactManagement.Messages.csproj", "Shared/ContactManagement.Messages/"]

# Restaura dependências
RUN dotnet restore "Services/ContactManagement.Exclusao.API/ContactManagement.Exclusao.API.csproj"

# Copia apenas os arquivos necessários para build
COPY ["Services/ContactManagement.Exclusao.API/", "Services/ContactManagement.Exclusao.API/"]
COPY ["Shared/ContactManagement.Messages/", "Shared/ContactManagement.Messages/"]

# Publica o projeto
WORKDIR "/src/Services/ContactManagement.Exclusao.API"
RUN dotnet publish -c Release -o /app/publish

# Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "ContactManagement.Exclusao.API.dll"]
