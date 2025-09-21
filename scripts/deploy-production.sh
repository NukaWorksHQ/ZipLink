#!/bin/bash

# Script de déploiement ZipLink pour environnement de production
# Utilisation: ./deploy-production.sh

set -e

echo "Déploiement de ZipLink en production..."

# Vérification des prérequis
if ! command -v docker &> /dev/null; then
    echo "ERREUR: Docker n'est pas installé ou accessible"
    exit 1
fi

if ! command -v docker-compose &> /dev/null; then
    echo "ERREUR: Docker Compose n'est pas installé ou accessible"
    exit 1
fi

# Configuration des variables d'environnement pour la production
echo "Configuration de l'environnement de production..."

# Créer le fichier .env.production s'il n'existe pas
if [ ! -f .env.production ]; then
    echo "Création du fichier .env.production..."
    cat > .env.production << EOF
# Configuration de production ZipLink
COMPOSE_PROJECT_NAME=ziplink-prod
COMPOSE_FILE=docker-compose.yml

# Base de données - Utilisez des mots de passe forts en production
DB_SA_PASSWORD=\$(openssl rand -base64 32)
DB_USER_PASSWORD=\$(openssl rand -base64 32)

# JWT
JWT_KEY=\$(openssl rand -base64 64)
JWT_ISSUER=ZipLink
JWT_AUDIENCE=https://your-domain.com

# Domaines
DOMAIN=your-domain.com
ALLOWED_HOSTS=your-domain.com,*.your-domain.com

# Monitoring
ENABLE_MONITORING=true
EOF
    echo "ATTENTION: Veuillez éditer le fichier .env.production avec vos configurations spécifiques"
    echo "   Notamment les domaines et les clés de sécurité"
    read -p "Appuyez sur Entrée quand vous avez terminé..."
fi

# Charger les variables d'environnement
source .env.production

# Arrêter les services existants
echo "Arrêt des services existants..."
docker-compose -f docker-compose.yml down --remove-orphans

# Supprimer les anciennes images
echo "Nettoyage des anciennes images..."
docker system prune -f
docker image prune -f

# Construire les nouvelles images
echo "Construction des images..."
docker-compose -f docker-compose.yml build --no-cache

# Démarrer les services
echo "Démarrage des services en production..."
docker-compose -f docker-compose.yml up -d

# Attendre que les services soient prêts
echo "Attente du démarrage des services..."
sleep 30

# Vérification de la santé des services
echo "Vérification de la santé des services..."

# Vérifier la base de données
echo "Vérification de la base de données..."
if docker-compose -f docker-compose.yml exec -T ziplink-db-primary /opt/mssql-tools/bin/sqlcmd -S localhost -U SA -P "$DB_SA_PASSWORD" -Q "SELECT 1" &> /dev/null; then
    echo "OK: Base de données principale opérationnelle"
else
    echo "ERREUR: Problème avec la base de données principale"
    exit 1
fi

# Vérifier l'API
echo "Vérification de l'API..."
if curl -f http://localhost/api/health &> /dev/null; then
    echo "OK: API opérationnelle"
else
    echo "ERREUR: Problème avec l'API"
    exit 1
fi

# Vérifier le frontend
echo "Vérification du frontend..."
if curl -f http://localhost/ &> /dev/null; then
    echo "OK: Frontend opérationnel"
else
    echo "ERREUR: Problème avec le frontend"
    exit 1
fi

# Appliquer les migrations de base de données
echo "Application des migrations de base de données..."
docker-compose -f docker-compose.yml exec -T ziplink-server dotnet ef database update

# Afficher le statut final
echo ""
echo "Déploiement terminé avec succès!"
echo ""
echo "Status des services:"
docker-compose -f docker-compose.yml ps

echo ""
echo "Votre application est accessible sur:"
echo "   Frontend: http://localhost/"
echo "   API: http://localhost/api/"
echo "   Health Check: http://localhost/health"
echo "   Monitoring: http://localhost:9090/ (si activé)"
echo ""
echo "Pour surveiller les logs:"
echo "   docker-compose -f docker-compose.yml logs -f"
echo ""
echo "Pour arrêter les services:"
echo "   docker-compose -f docker-compose.yml down"