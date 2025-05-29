# Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["ContactManagement.Consulta.API/ContactManagement.Consulta.API.csproj", "ContactManagement.Consulta.API/"]
COPY ["ContactManagement.Domain/ContactManagement.Domain.csproj", "ContactManagement.Domain/"]
COPY ["ContactManagement.Infrastructure/ContactManagement.Infrastructure.csproj", "ContactManagement.Infrastructure/"]
COPY ["ContactManagement.Messages/ContactManagement.Messages.csproj", "ContactManagement.Messages/"]
RUN dotnet restore "ContactManagement.Consulta.API/ContactManagement.Consulta.API.csproj"

COPY . .
WORKDIR "/src/ContactManagement.Consulta.API"
RUN dotnet publish -c Release -o /app/publish

# Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "ContactManagement.Consulta.API.dll"]
