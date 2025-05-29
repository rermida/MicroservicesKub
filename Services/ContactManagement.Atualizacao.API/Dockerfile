# Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["ContactManagement.Atualizacao.API/ContactManagement.Atualizacao.API.csproj", "ContactManagement.Atualizacao.API/"]
COPY ["ContactManagement.Messages/ContactManagement.Messages.csproj", "ContactManagement.Messages/"]
RUN dotnet restore "ContactManagement.Atualizacao.API/ContactManagement.Atualizacao.API.csproj"

COPY . .
WORKDIR "/src/ContactManagement.Atualizacao.API"
RUN dotnet publish -c Release -o /app/publish

# Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "ContactManagement.Atualizacao.API.dll"]
