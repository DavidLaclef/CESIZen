# Dockerfile pour CESIZen.UI
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copier les fichiers de projet
COPY ["CESIZen.UI/CESIZen.UI.csproj", "CESIZen.UI/"]
COPY ["CESIZen.Data/CESIZen.Data.csproj", "CESIZen.Data/"]

# Restaurer les dépendances
RUN dotnet restore "CESIZen.UI/CESIZen.UI.csproj"

# Copier tout le code source
COPY . .

# Build de l'application
WORKDIR "/src/CESIZen.UI"
RUN dotnet build "CESIZen.UI.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "CESIZen.UI.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Variables d'environnement par défaut
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:80

ENTRYPOINT ["dotnet", "CESIZen.UI.dll"]