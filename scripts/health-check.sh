#!/bin/bash

# Script de vérification de santé
# Utilisé pour vérifier que tous les services fonctionnent correctement

set -e

# Configuration
APP_DIR="/home/cesizen/cesizen"
LOG_FILE="$APP_DIR/logs/health-check.log"
UI_URL="http://localhost:80"
API_URL="http://localhost:5000"
MAX_RETRIES=3
RETRY_DELAY=5

# Couleurs
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m'

# Fonctions utilitaires
log_message() {
    echo "$(date '+%Y-%m-%d %H:%M:%S') - $1" | tee -a "$LOG_FILE"
}

print_status() {
    echo -e "${BLUE}[INFO]${NC} $1"
}

print_success() {
    echo -e "${GREEN}[SUCCESS]${NC} $1"
}

print_warning() {
    echo -e "${YELLOW}[WARNING]${NC} $1"
}

print_error() {
    echo -e "${RED}[ERROR]${NC} $1"
}

# Fonction pour tester une URL
test_url() {
    local url=$1
    local service_name=$2
    local retries=0

    while [ $retries -lt $MAX_RETRIES ]; do
        if curl -f -s --max-time 10 "$url" > /dev/null; then
            print_success "$service_name est accessible ($url)"
            return 0
        else
            retries=$((retries + 1))
            if [ $retries -lt $MAX_RETRIES ]; then
                print_warning "$service_name non accessible, tentative $retries/$MAX_RETRIES..."
                sleep $RETRY_DELAY
            fi
        fi
    done

    print_error "$service_name n'est pas accessible après $MAX_RETRIES tentatives ($url)"
    return 1
}

# Fonction pour vérifier un container Docker
check_container() {
    local container_name=$1
    local service_name=$2

    if docker ps --format "table {{.Names}}" | grep -q "^$container_name$"; then
        local status=$(docker inspect --format='{{.State.Status}}' "$container_name")
        if [ "$status" = "running" ]; then
            print_success "$service_name est en cours d'exécution"
            return 0
        else
            print_error "$service_name est arrêté (statut: $status)"
            return 1
        fi
    else
        print_error "Container $container_name introuvable"
        return 1
    fi
}

# Fonction pour vérifier l'espace disque
check_disk_space() {
    local usage=$(df /home | tail -1 | awk '{print $5}' | sed 's/%//')
    local threshold=85

    if [ "$usage" -gt $threshold ]; then
        print_error "Espace disque critique: $usage% utilisé (seuil: $threshold%)"
        return 1
    elif [ "$usage" -gt 70 ]; then
        print_warning "Espace disque élevé: $usage% utilisé"
        return 0
    else
        print_success "Espace disque OK: $usage% utilisé"
        return 0
    fi
}

# Fonction pour vérifier la mémoire
check_memory() {
    local available=$(free | awk '/^Mem:/ {printf "%.0f", $7/$2 * 100.0}')
    local threshold=10

    if [ "$available" -lt $threshold ]; then
        print_error "Mémoire disponible critique: $available%"
        return 1
    elif [ "$available" -lt 20 ]; then
        print_warning "Mémoire disponible faible: $available%"
        return 0
    else
        print_success "Mémoire disponible OK: $available%"
        return 0
    fi
}

# Fonction pour vérifier la base de données
check_database() {
    local container_name="cesizen_sqlserver"

    if check_container "$container_name" "Base de données SQL Server"; then
        # Tester la connexion à la base de données
        if docker exec "$container_name" /opt/mssql-tools/bin/sqlcmd \
            -S localhost -U sa -P "${SQL_SERVER_PASSWORD:-Strong!Passw0rd}" \
            -Q "SELECT 1" > /dev/null 2>&1; then
            print_success "Base de données accessible"
            return 0
        else
            print_error "Base de données non accessible"
            return 1
        fi
    else
        return 1
    fi
}

# Fonction principale
main() {
    log_message "🏥 Début de la vérification de santé"

    local overall_status=0

    print_status "Vérification des containers Docker..."
    check_container "cesizen_sqlserver" "Base de données" || overall_status=1
    check_container "cesizen_api" "API CESIZen" || overall_status=1
    check_container "cesizen_ui" "Interface utilisateur CESIZen" || overall_status=1
    check_container "cesizen_nginx" "Nginx" || overall_status=1

    echo ""
    print_status "Vérification de l'accessibilité des services..."
    test_url "$UI_URL" "Interface utilisateur" || overall_status=1
    test_url "$API_URL/swagger" "API Swagger" || overall_status=1

    echo ""
    print_status "Vérification de la base de données..."
    check_database || overall_status=1

    echo ""
    print_status "Vérification des ressources système..."
    check_disk_space || overall_status=1
    check_memory || overall_status=1

    echo ""
    print_status "Vérification des logs d'erreur récents..."
    local error_count=$(docker-compose -f "$APP_DIR/docker-compose.yml" logs --since="1h" 2>&1 | grep -i "error\|exception\|fatal" | wc -l)
    if [ "$error_count" -gt 10 ]; then
        print_warning "Nombre élevé d'erreurs dans les logs: $error_count"
        overall_status=1
    else
        print_success "Logs OK: $error_count erreurs dans la dernière heure"
    fi

    echo ""
    if [ $overall_status -eq 0 ]; then
        print_success "✅ Toutes les vérifications ont réussi"
        log_message "✅ Vérification de santé réussie"
    else
        print_error "❌ Certaines vérifications ont échoué"
        log_message "❌ Vérification de santé échouée"

        # Afficher un résumé des services en erreur
        echo ""
        print_status "Résumé des services:"
        docker-compose -f "$APP_DIR/docker-compose.yml" ps

        # Afficher les derniers logs en cas d'erreur
        echo ""
        print_status "Derniers logs des services:"
        docker-compose -f "$APP_DIR/docker-compose.yml" logs --tail=10
    fi

    return $overall_status
}

# Vérifier que le script est exécuté dans le bon répertoire
if [ ! -f "$APP_DIR/docker-compose.yml" ]; then
    print_error "Fichier docker-compose.yml introuvable dans $APP_DIR"
    print_error "Assurez-vous que l'application est correctement déployée"
    exit 1
fi

# Exécuter la vérification
cd "$APP_DIR"
main "$@"