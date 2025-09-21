#!/bin/bash

# Script de déploiement ZipLink pour environnement de développement
# Utilisation: ./deploy-development.sh

set -e

echo "Démarrage de ZipLink en mode développement..."

# Vérification des prérequis
if ! command -v docker &> /dev/null; then
    echo "ERREUR: Docker n'est pas installé ou accessible"
    exit 1
fi

if ! command -v docker-compose &> /dev/null; then
    echo "ERREUR: Docker Compose n'est pas installé ou accessible"
    exit 1
fi

# Créer le fichier .env.development s'il n'existe pas
if [ ! -f .env.development ]; then
    echo "Création du fichier .env.development..."
    cat > .env.development << EOF
# Configuration de développement ZipLink
COMPOSE_PROJECT_NAME=ziplink-dev

# Base de données
DB_PASSWORD=Dev123!
DB_SA_PASSWORD=Dev123!

# JWT
JWT_KEY=development-key-do-not-use-in-production
JWT_ISSUER=ZipLink
JWT_AUDIENCE=https://localhost:7001

# Domaines
DOMAIN=localhost
ALLOWED_HOSTS=localhost,127.0.0.1

# Hot reload
DOTNET_WATCH_RESTART_ON_RUDE_EDIT=true
ASPNETCORE_LOGGING__CONSOLE__DISABLECOLORS=false
EOF
fi

# Charger les variables d'environnement
source .env.development

# Arrêter les services existants en développement
echo "Arrêt des services de développement existants..."
docker-compose -f docker-compose.yml -f docker-compose.override.yml down --remove-orphans

# Créer les répertoires nécessaires
mkdir -p ./nginx
mkdir -p ./monitoring

# Construire les images en mode développement
echo "Construction des images de développement..."
docker-compose -f docker-compose.yml -f docker-compose.override.yml build

# Démarrer les services
echo "Démarrage des services en développement..."
docker-compose -f docker-compose.yml -f docker-compose.override.yml up -d

# Attendre que les services soient prêts
echo "Attente du démarrage des services..."
sleep 20

# Vérification de la santé des services
echo "Vérification de la santé des services..."

# Vérifier la base de données
echo "Vérification de la base de données..."
max_attempts=10
attempt=1
while [ $attempt -le $max_attempts ]; do
    if docker-compose -f docker-compose.yml -f docker-compose.override.yml exec -T ziplink-db-primary /opt/mssql-tools/bin/sqlcmd -S localhost -U SA -P "Dev123!" -Q "SELECT 1" &> /dev/null; then
        echo "OK: Base de données opérationnelle"
        break
    else
        echo "Tentative $attempt/$max_attempts - Base de données en cours de démarrage..."
        sleep 5
        ((attempt++))
    fi
done

if [ $attempt -gt $max_attempts ]; then
    echo "ERREUR: Impossible de se connecter à la base de données"
    echo "Logs de la base de données:"
    docker-compose -f docker-compose.yml -f docker-compose.override.yml logs ziplink-db-primary
    exit 1
fi

# Vérifier l'API
echo "Vérification de l'API..."
attempt=1
while [ $attempt -le $max_attempts ]; do
    if curl -f http://localhost:3000/api/health &> /dev/null; then
        echo "OK: API opérationnelle"
        break
    else
        echo "Tentative $attempt/$max_attempts - API en cours de démarrage..."
        sleep 5
        ((attempt++))
    fi
done

# Vérifier le frontend
echo "Vérification du frontend..."
attempt=1
while [ $attempt -le $max_attempts ]; do
    if curl -f http://localhost:3000/ &> /dev/null; then
        echo "OK: Frontend opérationnel"
        break
    else
        echo "Tentative $attempt/$max_attempts - Frontend en cours de démarrage..."
        sleep 5
        ((attempt++))
    fi
done

# Afficher le statut final
echo ""
echo "Environnement de développement prêt!"
echo ""
echo "Status des services:"
docker-compose -f docker-compose.yml -f docker-compose.override.yml ps

echo ""
echo "Votre application de développement est accessible sur:"
echo "   Frontend: http://localhost:3000/"
echo "   API: http://localhost:3000/api/"
echo "   API directe: http://localhost:8080/"
echo "   Web directe: http://localhost:8090/"
echo "   Base de données: localhost:1433 (SA/Dev123!)"
echo ""
echo "Fonctionnalités de développement:"
echo "   - Hot reload activé pour le code source"
echo "   - Volumes montés pour édition en direct"
echo "   - Logs détaillés"
echo ""
echo "Commandes utiles:"
echo "   Voir les logs: docker-compose -f docker-compose.yml -f docker-compose.override.yml logs -f"
echo "   Arrêter: docker-compose -f docker-compose.yml -f docker-compose.override.yml down"
echo "   Rebuild: docker-compose -f docker-compose.yml -f docker-compose.override.yml build"
echo ""
echo "Les modifications de code seront automatiquement rechargées!"