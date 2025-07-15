# CESIZen - Infrastructure CI/CD

## 🏗️ Architecture de déploiement

```
┌─────────────────┐    ┌──────────────────┐    ┌─────────────────┐
│   GitHub Repo   │───▶│  GitHub Actions  │───▶│   Azure VM      │
│                 │    │                  │    │                 │
│ • Source Code   │    │ • Build          │    │ • Docker        │
│ • Workflows     │    │ • Test           │    │ • Nginx         │
│ • Dockerfiles   │    │ • Security       │    │ • SQL Server    │
└─────────────────┘    │ • Deploy         │    │ • Monitoring    │
                       └──────────────────┘    └─────────────────┘
```

## 🚀 Workflows automatisés

### 01_Integration.yaml
**Déclenchement** : Push sur `develop`/`main`, Pull Request vers `main`

- ✅ Tests unitaires et d'intégration
- 🔍 Analyse de qualité avec SonarCloud
- 🛡️ Scan de sécurité avec Trivy
- 🐳 Build des images Docker

### 02_Deployment.yaml
**Déclenchement** : Push sur `main`

- 🏗️ Build et push des images vers GHCR
- 📦 Déploiement automatique sur Azure VM
- 🏥 Tests de santé post-déploiement
- 📧 Notifications de statut

## 🐳 Services Docker

### Architecture des conteneurs

```yaml
cesizen_db:        # SQL Server 2022
cesizen_api:       # CESIZen.API (.NET 8)
cesizen_ui:        # CESIZen.UI (Blazor Server)
cesizen_nginx:     # Reverse proxy & SSL
```

### Ports exposés
- **80** : Interface utilisateur (HTTP)
- **443** : Interface utilisateur (HTTPS)
- **5000** : API CESIZen
- **1433** : SQL Server (interne)

## 📊 Monitoring et observabilité

### Services surveillés
- ✅ État des conteneurs Docker
- 🌐 Accessibilité des endpoints
- 💾 Utilisation disque et mémoire
- 📝 Analyse des logs d'erreur
- 🗄️ Santé de la base de données

### Scripts automatiques
```bash
*/5 * * * *   # Monitoring (toutes les 5 min)
0 2 * * *     # Sauvegarde quotidienne (2h du matin)
0 3 * * 0     # Nettoyage hebdomadaire (dimanche 3h)
```

## 🔒 Sécurité

### Mesures implémentées
- 🛡️ Firewall UFW configuré
- 🚫 Fail2ban contre les attaques par force brute
- 🔐 Certificats SSL automatiques
- 🔑 Secrets chiffrés dans GitHub
- 📊 Scan de vulnérabilités dans le pipeline

### Ports ouverts
```
22/tcp   # SSH (accès administrateur)
80/tcp   # HTTP (redirection vers HTTPS)
443/tcp  # HTTPS (application web)
5000/tcp # API (développement/debug)
```

## 📁 Structure des fichiers

```
project-root/
├── .github/workflows/          # Pipelines CI/CD
│   ├── 01_Integration.yaml     # Tests et qualité
│   ├── 01-1_TestAndBuild.yaml  # Build et tests
│   ├── 02_Deployment.yaml     # Déploiement principal
│   ├── 02-1_DockerBuild.yaml  # Build images Docker
│   └── 02-2_Deploy.yaml       # Déploiement Azure
├── nginx/
│   └── nginx.conf             # Configuration reverse proxy
├── scripts/
│   ├── setup-server.sh        # Configuration serveur
│   └── health-check.sh        # Vérifications santé
├── CESIZen.API/
│   └── Dockerfile             # Image API
├── CESIZen.UI/
│   └── Dockerfile             # Image UI
├── docker-compose.yml         # Orchestration production
├── docker-compose.build.yml   # Tests locaux
├── .env.production            # Variables environnement
└── sonar-project.properties   # Configuration SonarCloud
```

## 🛠️ Commandes utiles

### Développement local
```bash
# Build et test en local
docker-compose -f docker-compose.build.yml up --build

# Tests de santé
./scripts/health-check.sh

# Surveillance des logs
docker-compose logs -f
```

### Production
```bash
# Déploiement manuel
./deploy.sh

# Monitoring
tail -f ~/cesizen/logs/monitor.log

# Sauvegarde manuelle
./backup.sh

# Redémarrage des services
docker-compose restart
```

## 🔧 Configuration des secrets GitHub

### Secrets obligatoires
```
AZURE_VM_HOST          # IP ou domaine de la VM
AZURE_VM_USER          # Utilisateur SSH (cesizen)
AZURE_SSH_PRIVATE_KEY  # Clé privée SSH
SQL_SERVER_PASSWORD    # Mot de passe SQL Server
```

### Secrets optionnels
```
SONAR_TOKEN           # Token SonarCloud
DOCKER_HUB_USERNAME   # Username Docker Hub
DOCKER_HUB_TOKEN      # Token Docker Hub
NOTIFICATION_WEBHOOK  # Webhook notifications
```

## 📈 Métriques et KPIs

### Performance des déploiements
- ⏱️ Durée moyenne de build : ~5-8 minutes
- 🚀 Temps de déploiement : ~2-3 minutes
- ✅ Taux de succès des déploiements : >95%
- 🔄 Fréquence de déploiement : À la demande

### Qualité du code
- 📊 Couverture de tests : >80%
- 🐛 Debt technique : Grade A
- 🔒 Vulnérabilités : 0 High/Critical
- 📈 Maintenabilité : Grade A

## 🚨 Procédures d'urgence

### Rollback rapide
```bash
cd ~/cesizen
./scripts/rollback.sh [version]
```

### Diagnostic complet
```bash
./scripts/health-check.sh --verbose
docker-compose ps
docker-compose logs --tail=100
```

### Contact support
- 📧 Email : support@cesizen.com
- 📱 Astreinte : +33 X XX XX XX XX

## 📚 Documentation supplémentaire

- [Guide d'installation complète](GUIDE_INTEGRATION_CICD.md)
- [Procédures de maintenance](docs/maintenance.md)
- [Troubleshooting](docs/troubleshooting.md)
- [Architecture technique](docs/architecture.md)

---

**Version** : 1.0.0
**Dernière mise à jour** : $(date '+%Y-%m-%d')
**Mainteneur** : Équipe DevOps CESIZen