# Cet index est utilisé lors de l'exécution à partir de VS en mode rapide (par défaut pour la configuration de débogage)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Cette phase est utilisée pour générer le projet de service
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["CESIZen.UI/CESIZen.UI.csproj", "CESIZen.UI/"]
COPY ["CESIZen.Data/CESIZen.Data.csproj", "CESIZen.Data/"]
RUN dotnet restore "./CESIZen.UI/CESIZen.UI.csproj"
COPY . .
WORKDIR "/src/CESIZen.UI"
RUN dotnet build "./CESIZen.UI.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Cette étape permet de publier le projet de service à copier dans la phase finale
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./CESIZen.UI.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Cette phase est utilisée en production ou lors de l'exécution à partir de VS en mode normal (par défaut quand la configuration de débogage n'est pas utilisée)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "CESIZen.UI.dll"]