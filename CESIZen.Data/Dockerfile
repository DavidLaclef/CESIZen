FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copier les fichiers de projet
COPY ["CESIZen.Data/CESIZen.Data.csproj", "CESIZen.Data/"]

# Restaurer les dépendances
RUN dotnet restore "CESIZen.Data/CESIZen.Data.csproj"

# Copier le code source
COPY ["CESIZen.Data/", "CESIZen.Data/"]

# Build du projet
WORKDIR "/src/CESIZen.Data"
RUN dotnet build "CESIZen.Data.csproj" -c Release

# Installer EF Core tools
RUN dotnet tool install --global dotnet-ef
ENV PATH="$PATH:/root/.dotnet/tools"

# Script pour appliquer les migrations
RUN echo '#!/bin/bash\n\
echo "Attente de la disponibilité de SQL Server..."\n\
sleep 30\n\
echo "Application des migrations..."\n\
dotnet ef database update --connection "$CONNECTION_STRING" || echo "Échec des migrations"\n\
echo "Migrations terminées"' > /migrate.sh && \
    chmod +x /migrate.sh

ENTRYPOINT ["/migrate.sh"]