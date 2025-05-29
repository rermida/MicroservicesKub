# Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["ContactManagement.Persistencia.Worker/ContactManagement.Persistencia.Worker.csproj", "ContactManagement.Persistencia.Worker/"]
COPY ["ContactManagement.Infrastructure/ContactManagement.Infrastructure.csproj", "ContactManagement.Infrastructure/"]
COPY ["ContactManagement.Domain/ContactManagement.Domain.csproj", "ContactManagement.Domain/"]
COPY ["ContactManagement.Messages/ContactManagement.Messages.csproj", "ContactManagement.Messages/"]
RUN dotnet restore "ContactManagement.Persistencia.Worker/ContactManagement.Persistencia.Worker.csproj"

COPY . .
WORKDIR "/src/ContactManagement.Persistencia.Worker"
RUN dotnet publish -c Release -o /app/publish

# Runtime
FROM mcr.microsoft.com/dotnet/runtime:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "ContactManagement.Persistencia.Worker.dll"]
