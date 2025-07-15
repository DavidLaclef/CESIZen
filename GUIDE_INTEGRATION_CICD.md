# Guide d'intégration CI/CD pour CESIZen

Ce guide vous accompagne dans l'intégration complète de l'infrastructure CI/CD pour déployer votre application CESIZen sur une VM Azure.

## 📋 Prérequis

### Infrastructure
- VM Azure (Ubuntu 20.04 LTS ou plus récent)
- Minimum 4 GB RAM, 2 vCPU, 50 GB de stockage
- Accès SSH à la VM
- Nom de domaine (optionnel mais recommandé)

### Services
- Compte GitHub avec votre repository CESIZen
- Compte SonarCloud (optionnel pour l'analyse de qualité)
- Compte Docker Hub ou GitHub Container Registry

## 🚀 Étape 1 : Configuration du repository GitHub

### 1.1 Ajouter les fichiers CI/CD à votre projet

Copiez tous les fichiers suivants dans votre repository :

```
votre-projet/
├── .github/
│   └── workflows/
│       ├── 01_Integration.yaml
│       ├── 01-1_TestAndBuild.yaml
│       ├── 02_Deployment.yaml
│       ├── 02-1_DockerBuild.yaml
│       └── 02-2_Deploy.yaml
├── CESIZen.API/
│   └── Dockerfile
├── CESIZen.UI/
│   └── Dockerfile
├── nginx/
│   └── nginx.conf
├── scripts/
│   ├── setup-server.sh
│   └── health-check.sh
├── docker-compose.yml
├── .env.production
└── sonar-project.properties
```

### 1.2 Configurer les secrets GitHub

Allez dans **Settings > Secrets and variables > Actions** de votre repository et ajoutez :

#### Secrets obligatoires :
```
AZURE_VM_HOST=IP_ou_domaine_de_votre_VM
AZURE_VM_USER=cesizen
AZURE_SSH_PRIVATE_KEY=contenu_de_votre_clé_privée_SSH
SQL_SERVER_PASSWORD=Strong!Passw0rd123
```

#### Secrets optionnels :
```
SONAR_TOKEN=votre_token_sonarcloud
DOCKER_HUB_USERNAME=votre_username_dockerhub
DOCKER_HUB_TOKEN=votre_token_dockerhub
```

### 1.3 Mise à jour des fichiers de configuration

#### Dans `docker-compose.yml` :
Remplacez `ghcr.io/votre-username/` par votre nom d'utilisateur GitHub :
```yaml
image: ghcr.io/VOTRE_USERNAME/cesizen-api:latest
image: ghcr.io/VOTRE_USERNAME/cesizen-ui:latest
```

#### Dans `sonar-project.properties` :
Remplacez `votre-organization` par votre organisation SonarCloud.

## 🖥️ Étape 2 : Configuration de la VM Azure

### 2.1 Création de la VM Azure

```bash
# Via Azure CLI (optionnel)
az vm create \
  --resource-group myResourceGroup \
  --name cesizen-vm \
  --image Ubuntu2004 \
  --admin-username cesizen \
  --generate-ssh-keys \
  --size Standard_B2s
```

### 2.2 Configuration SSH

Sur votre machine locale :
```bash
# Générer une paire de clés SSH si nécessaire
ssh-keygen -t rsa -b 4096 -C "cesizen@yourmail.com"

# Copier la clé publique sur la VM
ssh-copy-id cesizen@IP_DE_VOTRE_VM
```

### 2.3 Exécution du script de configuration

Connectez-vous à votre VM et exécutez :

```bash
# Se connecter à la VM
ssh cesizen@IP_DE_VOTRE_VM

# Télécharger et exécuter le script de configuration
curl -fsSL https://raw.githubusercontent.com/VOTRE_USERNAME/cesizen/main/scripts/setup-server.sh -o setup-server.sh
chmod +x setup-server.sh
./setup-server.sh cesizen

# Redémarrer la session pour appliquer les groupes Docker
logout
```

### 2.4 Configuration manuelle post-installation

Reconnectez-vous et finalisez la configuration :

```bash
ssh cesizen@IP_DE_VOTRE_VM
cd ~/cesizen

# Configurer les variables d'environnement
cp .env.production .env
nano .env  # Modifier selon vos besoins

# Tester la configuration Docker
docker --version
docker-compose --version
docker ps
```

## 🔧 Étape 3 : Configuration des services externes

### 3.1 Configuration SonarCloud (optionnel)

1. Allez sur [SonarCloud](https://sonarcloud.io)
2. Connectez-vous avec votre compte GitHub
3. Créez une nouvelle organisation
4. Importez votre repository CESIZen
5. Copiez le token généré dans les secrets GitHub (`SONAR_TOKEN`)

### 3.2 Configuration du domaine (recommandé)

Si vous avez un nom de domaine :

```bash
# Sur la VM, mettre à jour nginx.conf
nano ~/cesizen/nginx/nginx.conf

# Remplacer "server_name _;" par "server_name votre-domaine.com;"
```

### 3.3 Configuration SSL avec Let's Encrypt

```bash
# Installer Certbot
sudo apt install certbot python3-certbot-nginx

# Arrêter nginx temporairement
docker-compose down nginx

# Obtenir le certificat
sudo certbot certonly --standalone -d votre-domaine.com

# Copier les certificats
sudo cp /etc/letsencrypt/live/votre-domaine.com/fullchain.pem ~/cesizen/nginx/ssl/cert.pem
sudo cp /etc/letsencrypt/live/votre-domaine.com/privkey.pem ~/cesizen/nginx/ssl/key.pem
sudo chown cesizen:cesizen ~/cesizen/nginx/ssl/*

# Redémarrer les services
docker-compose up -d
```

## 🚀 Étape 4 : Premier déploiement

### 4.1 Déploiement manuel initial

```bash
# Sur la VM
cd ~/cesizen

# Créer le fichier docker-compose.yml initial
cat > docker-compose.yml << 'EOF'
version: '3.8'

services:
  cesizen_db:
    container_name: cesizen_sqlserver
    image: mcr.microsoft.com/mssql/server:2022-latest
    environment:
      - ACCEPT_EULA=Y
      - SA_PASSWORD=${SQL_SERVER_PASSWORD:-Strong!Passw0rd123}
      - MSSQL_PID=Express
    ports:
      - "1433:1433"
    volumes:
      - sqlserver_data:/var/opt/mssql
    healthcheck:
      test: ["CMD-SHELL", "/opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P ${SA_PASSWORD} -Q 'SELECT 1'"]
      interval: 10s
      timeout: 5s
      retries: 5
      start_period: 30s
    restart: unless-stopped

volumes:
  sqlserver_data:
    driver: local
EOF

# Démarrer seulement la base de données pour commencer
docker-compose up -d cesizen_db

# Vérifier que la base de données fonctionne
docker-compose logs cesizen_db
```

### 4.2 Test du pipeline CI/CD

1. Faites un commit et push sur la branche `main` :
```bash
git add .
git commit -m "feat: add CI/CD infrastructure"
git push origin main
```

2. Vérifiez dans GitHub Actions que les workflows se déclenchent
3. Le déploiement automatique devrait avoir lieu

### 4.3 Vérification du déploiement

```bash
# Sur la VM, vérifier les services
cd ~/cesizen
docker-compose ps

# Exécuter le script de santé
./health-check.sh

# Vérifier les logs
docker-compose logs --tail=50
```

## 📊 Étape 5 : Monitoring et maintenance

### 5.1 Configuration du monitoring

Le monitoring automatique est configuré via cron :

```bash
# Vérifier les tâches cron
crontab -l

# Consulter les logs de monitoring
tail -f ~/cesizen/logs/monitor.log

# Consulter les logs de sauvegarde
tail -f ~/cesizen/logs/backup.log
```

### 5.2 Scripts de maintenance

```bash
# Script de santé manuel
~/cesizen/health-check.sh

# Sauvegarde manuelle
~/cesizen/backup.sh

# Surveillance des ressources
htop
df -h
docker stats
```

### 5.3 Gestion des logs

```bash
# Voir les logs de l'application
docker-compose logs -f cesizen_ui
docker-compose logs -f cesizen_api

# Nettoyer les logs Docker
docker system prune -f

# Configurer la rotation des logs
sudo nano /etc/logrotate.d/docker-containers
```

## 🔒 Étape 6 : Sécurisation

### 6.1 Mise à jour des mots de passe

```bash
# Générer un mot de passe fort
openssl rand -base64 32

# Mettre à jour dans .env
nano ~/cesizen/.env

# Mettre à jour dans GitHub Secrets
# Settings > Secrets > SQL_SERVER_PASSWORD
```

### 6.2 Configuration du firewall

```bash
# Vérifier les règles UFW
sudo ufw status

# Ajouter des règles si nécessaire
sudo ufw allow from IP_SPECIFIQUE to any port 1433
```

### 6.3 Surveillance des accès

```bash
# Consulter les logs d'accès
sudo tail -f /var/log/auth.log

# Configurer fail2ban
sudo nano /etc/fail2ban/jail.local
```

## 🚨 Étape 7 : Dépannage

### 7.1 Problèmes courants

#### Services qui ne démarrent pas :
```bash
# Vérifier les logs
docker-compose logs

# Redémarrer un service spécifique
docker-compose restart cesizen_api

# Reconstruire les images
docker-compose build --no-cache
```

#### Problèmes de connectivité :
```bash
# Tester la connectivité réseau
curl -I http://localhost:80
curl -I http://localhost:5000

# Vérifier les ports
netstat -tlnp | grep :80
netstat -tlnp | grep :5000
```

#### Problèmes de base de données :
```bash
# Se connecter à SQL Server
docker exec -it cesizen_sqlserver /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P 'VotreMotDePasse'

# Vérifier l'espace disque
df -h
docker system df
```

### 7.2 Commandes de diagnostic

```bash
# Status complet du système
~/cesizen/health-check.sh

# Informations Docker
docker info
docker-compose config

# Logs système
sudo journalctl -u docker
```

### 7.3 Procédure de rollback

En cas de problème avec un déploiement :

```bash
# Revenir à la version précédente
cd ~/cesizen/backups
ls -la  # Voir les sauvegardes disponibles

# Restaurer une sauvegarde
tar -xzf backup_YYYYMMDD_HHMMSS.tar.gz
cp backup_YYYYMMDD_HHMMSS/docker-compose.yml ../
docker-compose down
docker-compose up -d
```

## 📋 Checklist de déploiement

- [ ] VM Azure configurée et accessible
- [ ] Scripts de configuration exécutés
- [ ] Secrets GitHub configurés
- [ ] Repository mis à jour avec les fichiers CI/CD
- [ ] Premier déploiement réussi
- [ ] Tests de santé passants
- [ ] Monitoring configuré
- [ ] Sauvegardes programmées
- [ ] SSL configuré (si domaine)
- [ ] Firewall configuré
- [ ] Documentation équipe mise à jour

## 🎯 Workflow de développement

### Branches et déploiements

- `develop` → Tests et intégration continue
- `main` → Déploiement automatique en production
- Pull requests → Tests automatiques + SonarCloud

### Commandes utiles

```bash
# Déploiement manuel (si besoin)
curl -X POST \
  -H "Accept: application/vnd.github.v3+json" \
  -H "Authorization: token VOTRE_TOKEN" \
  https://api.github.com/repos/VOTRE_USERNAME/cesizen/actions/workflows/02_Deployment.yaml/dispatches \
  -d '{"ref":"main"}'

# Surveillance continue
watch -n 30 'docker-compose ps && echo "---" && df -h'
```

## 📞 Support et contacts

### Ressources utiles

- [Documentation Docker](https://docs.docker.com/)
- [Documentation ASP.NET Core](https://docs.microsoft.com/aspnet/core/)
- [GitHub Actions](https://docs.github.com/actions)
- [Azure VM](https://docs.microsoft.com/azure/virtual-machines/)

### Maintenance préventive

- **Quotidien** : Vérification automatique des services
- **Hebdomadaire** : Vérification manuelle des logs et performances
- **Mensuel** : Mise à jour des packages système et Docker
- **Trimestriel** : Révision des sauvegardes et tests de restauration

---

🎉 **Félicitations !** L'infrastructure CI/CD pour CESIZen est maintenant opérationnelle. L'application se déploie automatiquement à chaque push sur la branche `main` et bénéficie d'un monitoring continu. (ça on sait pas eheh)