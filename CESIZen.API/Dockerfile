FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copier les fichiers de projet
COPY ["CESIZen.API/CESIZen.API.csproj", "CESIZen.API/"]
COPY ["CESIZen.Data/CESIZen.Data.csproj", "CESIZen.Data/"]

# Restaurer les dépendances
RUN dotnet restore "CESIZen.API/CESIZen.API.csproj"

# Copier tout le code source
COPY . .

# Build de l'application
WORKDIR "/src/CESIZen.API"
RUN dotnet build "CESIZen.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "CESIZen.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Variables d'environnement par défaut
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:80

ENTRYPOINT ["dotnet", "CESIZen.API.dll"]