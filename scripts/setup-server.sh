#!/bin/bash

# Script de configuration du serveur Azure
# À exécuter sur la VM Azure avec des privilèges sudo

set -e

echo "🚀 Configuration du serveur Azure pour CESIZen..."

# Variables
USER_NAME=${1:-cesizen}
APP_DIR="/home/$USER_NAME/cesizen"

# Couleurs pour les messages
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

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

# Vérifier que le script est exécuté avec sudo
if [[ $EUID -eq 0 ]]; then
   print_error "Ce script ne doit pas être exécuté en tant que root"
   print_status "Usage: ./setup-server.sh [username]"
   exit 1
fi

# Mettre à jour le système
print_status "Mise à jour du système..."
sudo apt update && sudo apt upgrade -y

# Installer les dépendances de base
print_status "Installation des dépendances de base..."
sudo apt install -y \
    curl \
    wget \
    git \
    unzip \
    software-properties-common \
    apt-transport-https \
    ca-certificates \
    gnupg \
    lsb-release \
    ufw \
    fail2ban \
    htop \
    nano \
    vim

# Installer Docker
print_status "Installation de Docker..."
if ! command -v docker &> /dev/null; then
    # Ajouter la clé GPG officielle de Docker
    curl -fsSL https://download.docker.com/linux/ubuntu/gpg | sudo gpg --dearmor -o /usr/share/keyrings/docker-archive-keyring.gpg

    # Ajouter le repository Docker
    echo "deb [arch=$(dpkg --print-architecture) signed-by=/usr/share/keyrings/docker-archive-keyring.gpg] https://download.docker.com/linux/ubuntu $(lsb_release -cs) stable" | sudo tee /etc/apt/sources.list.d/docker.list > /dev/null

    # Installer Docker Engine
    sudo apt update
    sudo apt install -y docker-ce docker-ce-cli containerd.io docker-buildx-plugin docker-compose-plugin

    # Ajouter l'utilisateur au groupe docker
    sudo usermod -aG docker $USER
    sudo usermod -aG docker $USER_NAME

    print_success "Docker installé avec succès"
else
    print_warning "Docker est déjà installé"
fi

# Installer Docker Compose (version standalone)
print_status "Installation de Docker Compose..."
if ! command -v docker-compose &> /dev/null; then
    DOCKER_COMPOSE_VERSION=$(curl -s https://api.github.com/repos/docker/compose/releases/latest | grep tag_name | cut -d '"' -f 4)
    sudo curl -L "https://github.com/docker/compose/releases/download/${DOCKER_COMPOSE_VERSION}/docker-compose-$(uname -s)-$(uname -m)" -o /usr/local/bin/docker-compose
    sudo chmod +x /usr/local/bin/docker-compose
    print_success "Docker Compose installé avec succès"
else
    print_warning "Docker Compose est déjà installé"
fi

# Configurer le firewall
print_status "Configuration du firewall..."
sudo ufw --force enable
sudo ufw default deny incoming
sudo ufw default allow outgoing
sudo ufw allow ssh
sudo ufw allow 80/tcp
sudo ufw allow 443/tcp
sudo ufw allow 5000/tcp  # API CESIZen
print_success "Firewall configuré"

# Configurer fail2ban
print_status "Configuration de fail2ban..."
sudo systemctl enable fail2ban
sudo systemctl start fail2ban

# Créer les répertoires de l'application
print_status "Création des répertoires de l'application..."
sudo mkdir -p $APP_DIR
sudo mkdir -p $APP_DIR/backups
sudo mkdir -p $APP_DIR/logs
sudo mkdir -p $APP_DIR/nginx/ssl
sudo chown -R $USER_NAME:$USER_NAME $APP_DIR

# Créer un certificat SSL auto-signé (temporaire)
print_status "Création d'un certificat SSL auto-signé..."
if [[ ! -f "$APP_DIR/nginx/ssl/cert.pem" ]]; then
    sudo openssl req -x509 -nodes -days 365 -newkey rsa:2048 \
        -keyout $APP_DIR/nginx/ssl/key.pem \
        -out $APP_DIR/nginx/ssl/cert.pem \
        -subj "/C=FR/ST=France/L=Paris/O=CESIZen/CN=localhost"
    sudo chown -R $USER_NAME:$USER_NAME $APP_DIR/nginx/ssl
    print_success "Certificat SSL créé"
fi

# Configurer les variables d'environnement
print_status "Configuration des variables d'environnement..."
if [[ ! -f "$APP_DIR/.env" ]]; then
    cat > $APP_DIR/.env << EOF
# Configuration CESIZen Production
SQL_SERVER_PASSWORD=Strong!Passw0rd123
ASPNETCORE_ENVIRONMENT=Production

# GitHub Container Registry
GITHUB_USERNAME=votre-username
GITHUB_TOKEN=votre-token

# Monitoring
LOG_LEVEL=Information
EOF
    sudo chown $USER_NAME:$USER_NAME $APP_DIR/.env
    sudo chmod 600 $APP_DIR/.env
    print_success "Variables d'environnement configurées"
fi

# Créer le script de monitoring
print_status "Création du script de monitoring..."
cat > $APP_DIR/monitor.sh << 'EOF'
#!/bin/bash

# Script de monitoring CESIZen
APP_DIR="/home/cesizen/cesizen"
LOG_FILE="$APP_DIR/logs/monitor.log"

log_message() {
    echo "$(date '+%Y-%m-%d %H:%M:%S') - $1" >> "$LOG_FILE"
}

check_service() {
    local service_name=$1
    local container_name=$2

    if docker ps | grep -q "$container_name"; then
        log_message "✅ $service_name est opérationnel"
        return 0
    else
        log_message "❌ $service_name est en panne"
        return 1
    fi
}

# Vérifier les services
cd "$APP_DIR"
check_service "Base de données" "cesizen_sqlserver"
check_service "API" "cesizen_api"
check_service "Interface utilisateur" "cesizen_ui"
check_service "Nginx" "cesizen_nginx"

# Vérifier l'espace disque
disk_usage=$(df /home | tail -1 | awk '{print $5}' | sed 's/%//')
if [ "$disk_usage" -gt 80 ]; then
    log_message "⚠️ Attention: Espace disque utilisé à $disk_usage%"
fi

# Nettoyer les logs anciens (garder 30 jours)
find "$APP_DIR/logs" -name "*.log" -mtime +30 -delete 2>/dev/null || true

# Nettoyer Docker
docker system prune -f >> "$LOG_FILE" 2>&1
EOF

chmod +x $APP_DIR/monitor.sh
sudo chown $USER_NAME:$USER_NAME $APP_DIR/monitor.sh

# Configurer la crontab pour le monitoring
print_status "Configuration du monitoring automatique..."
(crontab -u $USER_NAME -l 2>/dev/null; echo "*/5 * * * * $APP_DIR/monitor.sh") | crontab -u $USER_NAME -

# Créer le script de sauvegarde
print_status "Création du script de sauvegarde..."
cat > $APP_DIR/backup.sh << 'EOF'
#!/bin/bash

APP_DIR="/home/cesizen/cesizen"
BACKUP_DIR="$APP_DIR/backups"
DATE=$(date +%Y%m%d_%H%M%S)
LOG_FILE="$APP_DIR/logs/backup.log"

log_message() {
    echo "$(date '+%Y-%m-%d %H:%M:%S') - $1" | tee -a "$LOG_FILE"
}

log_message "🔄 Début de la sauvegarde..."

# Créer le répertoire de sauvegarde
mkdir -p "$BACKUP_DIR/$DATE"

# Sauvegarder la base de données
if docker container inspect cesizen_sqlserver >/dev/null 2>&1; then
    log_message "📦 Sauvegarde de la base de données..."
    docker exec cesizen_sqlserver /opt/mssql-tools/bin/sqlcmd \
        -S localhost -U sa -P "${SQL_SERVER_PASSWORD:-Strong!Passw0rd}" \
        -Q "BACKUP DATABASE [CESIZenDatabase] TO DISK = '/tmp/backup_$DATE.bak'" \
        >> "$LOG_FILE" 2>&1

    if [ $? -eq 0 ]; then
        docker cp cesizen_sqlserver:/tmp/backup_$DATE.bak "$BACKUP_DIR/$DATE/"
        log_message "✅ Sauvegarde de la base de données réussie"
    else
        log_message "❌ Échec de la sauvegarde de la base de données"
    fi
fi

# Sauvegarder les fichiers de configuration
cp -r $APP_DIR/docker-compose.yml $APP_DIR/nginx $APP_DIR/.env "$BACKUP_DIR/$DATE/" 2>/dev/null || true

# Compresser la sauvegarde
cd "$BACKUP_DIR"
tar -czf "backup_$DATE.tar.gz" "$DATE/"
rm -rf "$DATE/"

# Supprimer les sauvegardes anciennes (garder 7 jours)
find "$BACKUP_DIR" -name "backup_*.tar.gz" -mtime +7 -delete

log_message "✅ Sauvegarde terminée: backup_$DATE.tar.gz"
EOF

chmod +x $APP_DIR/backup.sh
sudo chown $USER_NAME:$USER_NAME $APP_DIR/backup.sh

# Programmer la sauvegarde quotidienne
(crontab -u $USER_NAME -l 2>/dev/null; echo "0 2 * * * $APP_DIR/backup.sh") | crontab -u $USER_NAME -

# Afficher les informations système
print_status "Informations système:"
echo "  - OS: $(lsb_release -d | cut -f2)"
echo "  - Kernel: $(uname -r)"
echo "  - RAM: $(free -h | awk '/^Mem:/ {print $2}')"
echo "  - Disk: $(df -h / | awk 'NR==2 {print $2}')"
echo "  - Docker: $(docker --version | cut -d' ' -f3 | tr -d ',')"
echo "  - Docker Compose: $(docker-compose --version | cut -d' ' -f3 | tr -d ',')"

print_success "Configuration du serveur terminée!"
print_status "Prochaines étapes:"
echo "  1. Redémarrer la session pour appliquer les groupes Docker"
echo "  2. Configurer les secrets GitHub pour le déploiement"
echo "  3. Mettre à jour le fichier docker-compose.yml avec vos images"
echo "  4. Lancer le premier déploiement"
echo ""
print_status "Répertoire de l'application: $APP_DIR"
print_status "Logs de monitoring: $APP_DIR/logs/"
print_status "Sauvegardes: $APP_DIR/backups/"