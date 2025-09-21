# 🚀 ZipLink - Déploiement Docker avec Répliques

Configuration Docker Compose complète pour déployer ZipLink avec des répliques sur votre cluster Docker.

## 📋 Table des Matières

- [Architecture](#-architecture)
- [Prérequis](#-prérequis)
- [Configuration Rapide](#-configuration-rapide)
- [Variables d'Environnement](#-variables-denvironnement)
- [Déploiement](#-déploiement)
- [Gestion du Cluster](#-gestion-du-cluster)
- [Commandes Utiles](#-commandes-utiles)
- [Sécurité](#-sécurité)

## 🏗️ Architecture

```
┌─────────────────────┐    ┌─────────────────────┐
│   Load Balancer     │    │     Monitoring      │
│      (Nginx)        │    │   (Prometheus)      │
│                     │    │                     │
└──────┬──────────────┘    └─────────────────────┘
       │
       ▼
┌─────────────────────┐    ┌─────────────────────┐
│   Web Frontend      │    │   API Backend       │
│   (Blazor WASM)     │    │   (.NET 8)          │
│   2 Répliques       │    │   2 Répliques       │
└─────────────────────┘    └──────┬──────────────┘
                                  │
                                  ▼
┌─────────────────────┐    ┌─────────────────────┐
│  DB Primary         │    │   DB Replica        │
│  (SQL Server)       │    │   (SQL Server)      │
│  Nœud Spécifique    │    │   Nœud Différent    │
└─────────────────────┘    └─────────────────────┘
```

## 📦 Prérequis

- **Docker** 20.10+
- **Docker Compose** 2.0+
- **Make** (optionnel, pour les commandes simplifiées)
- **Au moins 2 nœuds** pour la réplication de base de données
- **4GB RAM minimum** par nœud

## ⚡ Configuration Rapide

### 1. Déploiement en Développement

```bash
# Utiliser Make (recommandé)
make dev

# Ou directement
./scripts/deploy-development.sh
```

### 2. Déploiement en Production

```bash
# Configuration des variables d'environnement
cp .env.production.example .env.production
# Éditez .env.production avec vos valeurs

# Déploiement
make prod

# Ou directement
./scripts/deploy-production.sh
```

### 3. Déploiement sur Cluster Swarm

```bash
# Initialiser le cluster
make cluster-init

# Configurer les nœuds pour la base de données
./scripts/setup-cluster-nodes.sh setup --auto

# Déployer la stack
make cluster-deploy
```

## 🔧 Variables d'Environnement

### Production (`.env.production`)

```bash
# Base de données - CHANGEZ CES MOTS DE PASSE !
DB_SA_PASSWORD=VotreSuperMotDePasseSecurise123!
DB_USER_PASSWORD=MotDePasseUtilisateur456!

# JWT - GÉNÉREZ DES CLÉS SÉCURISÉES !
JWT_KEY=VotreCléJWTTrèsLongueEtComplexe123456789
JWT_ISSUER=ZipLink
JWT_AUDIENCE=https://votre-domaine.com

# Domaines
DOMAIN=votre-domaine.com
ALLOWED_HOSTS=votre-domaine.com,*.votre-domaine.com

# Configuration des nœuds
DB_PRIMARY_NODE_LABEL=database.primary=true
DB_REPLICA_NODE_LABEL=database.replica=true
```

### Développement (`.env.development`)

```bash
# Mots de passe simples pour le développement
DB_SA_PASSWORD=Dev123!
JWT_KEY=development-key-do-not-use-in-production

# Domaines locaux
DOMAIN=localhost
ALLOWED_HOSTS=localhost,127.0.0.1
```

## 🚀 Déploiement

### Mode Développement

```bash
# Démarrage complet
make dev

# Reconstruction et démarrage
make dev-build

# Accès
# Frontend: http://localhost:3000
# API: http://localhost:8080
# DB: localhost:1433
```

### Mode Production

```bash
# Démarrage
make prod

# Mise à l'échelle
make scale-up    # 3 répliques
make scale-down  # 1 réplique

# Accès
# Application: http://localhost
# Monitoring: http://localhost:9090
```

### Cluster Docker Swarm

```bash
# 1. Initialiser le cluster
make cluster-init

# 2. Configurer les nœuds
./scripts/setup-cluster-nodes.sh setup --auto
# Ou manuellement :
./scripts/setup-cluster-nodes.sh setup --primary-node node1 --replica-node node2

# 3. Déployer
make cluster-deploy

# 4. Vérifier le statut
make cluster-status

# 5. Mettre à l'échelle
make cluster-scale
```

## 🎛️ Gestion du Cluster

### Configuration des Nœuds

Les bases de données sont déployées sur des nœuds spécifiques grâce aux étiquettes :

```bash
# Lister les nœuds et leurs étiquettes
./scripts/setup-cluster-nodes.sh list

# Configuration automatique (2 premiers nœuds)
./scripts/setup-cluster-nodes.sh setup --auto

# Configuration manuelle
./scripts/setup-cluster-nodes.sh setup --primary-node <node-id> --replica-node <node-id>

# Supprimer les étiquettes
./scripts/setup-cluster-nodes.sh remove
```

### Placement des Services

- **DB Primary** : Nœud avec `database.primary=true`
- **DB Replica** : Nœud avec `database.replica=true` (différent du primaire)
- **API/Web** : Répartis sur tous les nœuds disponibles (`max_replicas_per_node: 1`)
- **Load Balancer** : Nœud manager
- **Monitoring** : Nœud manager

## 🛠️ Commandes Utiles

### Avec Make

```bash
# Afficher l'aide
make help

# Développement
make dev                    # Démarrer en développement
make logs                   # Voir les logs
make restart               # Redémarrer
make stop                  # Arrêter

# Production
make prod                  # Démarrer en production
make scale                 # Mettre à l'échelle (2 répliques)
make scale-up             # 3 répliques
make scale-down           # 1 réplique

# Cluster
make cluster-init         # Initialiser Swarm
make cluster-deploy       # Déployer sur cluster
make cluster-status       # Statut du cluster
make cluster-stop         # Arrêter le cluster

# Maintenance
make clean                # Nettoyer
make backup-db           # Sauvegarder la DB
make migration           # Appliquer migrations
```

### Commandes Docker Compose

```bash
# Développement
docker-compose -f docker-compose.yml -f docker-compose.override.yml up -d
docker-compose -f docker-compose.yml -f docker-compose.override.yml logs -f

# Production
docker-compose -f docker-compose.yml up -d
docker-compose -f docker-compose.yml logs -f

# Mise à l'échelle
docker-compose -f docker-compose.yml up -d --scale ziplink-server=3 --scale ziplink-web=3
```

### Commandes Docker Swarm

```bash
# Déployer la stack
docker stack deploy -c docker-stack.yml ziplink

# Voir les services
docker stack services ziplink

# Voir les tâches
docker stack ps ziplink

# Mettre à l'échelle
docker service scale ziplink_ziplink-server=3

# Logs
docker service logs -f ziplink_ziplink-server
```

## 🔒 Sécurité

### ⚠️ IMPORTANT - À faire AVANT la production :

1. **Changez TOUS les mots de passe** dans `.env.production`
2. **Générez des clés JWT sécurisées** (64+ caractères)
3. **Configurez HTTPS** avec des certificats SSL
4. **Limitez l'accès aux ports** (firewall)
5. **Activez la surveillance** (logs, métriques)

### Génération de mots de passe sécurisés

```bash
# Mot de passe pour la base de données
openssl rand -base64 32

# Clé JWT
openssl rand -base64 64

# Ou utiliser pwgen
pwgen -s 32 1
```

### Configuration SSL/HTTPS

1. Placez vos certificats dans `./nginx/ssl/`
2. Décommentez la section HTTPS dans `nginx/nginx.conf`
3. Redémarrez le load balancer

## 📊 Monitoring

### Prometheus

- **URL** : http://localhost:9090
- **Métriques** : API, Load Balancer, Conteneurs
- **Alertes** : Configurables via `monitoring/prometheus.yml`

### Health Checks

Tous les services ont des health checks :

```bash
# Vérifier la santé des services
curl http://localhost/health
curl http://localhost:8080/health  # API directe
```

## 🔍 Dépannage

### Logs en temps réel

```bash
# Tous les services
make logs

# Service spécifique
docker-compose logs -f ziplink-server
docker service logs -f ziplink_ziplink-server  # Swarm
```

### Connexion aux conteneurs

```bash
# Shell dans le serveur
make shell-server

# Shell SQL Server
make shell-db

# Ou directement
docker-compose exec ziplink-server bash
docker-compose exec ziplink-db-primary /bin/bash
```

### Problèmes courants

1. **Erreur de connexion DB** :

   - Vérifiez que les variables d'environnement sont correctes
   - Attendez le démarrage complet de SQL Server (health check)

2. **Services non accessibles** :

   - Vérifiez les ports dans `.env`
   - Contrôlez les règles de firewall

3. **Répliques sur le même nœud** :
   - Vérifiez les étiquettes des nœuds
   - Utilisez `./scripts/setup-cluster-nodes.sh list`

## 📚 Documentation Supplémentaire

- [Docker Compose Documentation](https://docs.docker.com/compose/)
- [Docker Swarm Documentation](https://docs.docker.com/engine/swarm/)
- [SQL Server on Docker](https://docs.microsoft.com/en-us/sql/linux/sql-server-linux-docker-container-deployment)
- [Nginx Load Balancing](https://nginx.org/en/docs/http/load_balancing.html)

---

🎉 **Votre application ZipLink est maintenant prête pour la production avec des répliques !**
