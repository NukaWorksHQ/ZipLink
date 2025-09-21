#!/bin/bash

# Script de préparation du serveur Docker Swarm pour ZipLink
# Exécutez ce script EN TANT QUE ROOT sur le serveur avant le premier déploiement
# Les déploiements automatiques GitHub Actions utilisent l'utilisateur root

set -e

echo "=== Préparation du serveur Docker Swarm pour ZipLink ==="
echo "Script exécuté en tant que: $(whoami)"

if [ "$EUID" -ne 0 ]; then
  echo "ATTENTION: Ce script doit être exécuté en tant que root pour les déploiements automatiques"
  echo "Utilisez: sudo ./setup-server.sh"
fi

# Vérifier si Docker est installé
if ! command -v docker &> /dev/null; then
    echo "Docker n'est pas installé. Installation..."
    curl -fsSL https://get.docker.com | bash
    sudo systemctl enable docker
    sudo systemctl start docker
    echo "Docker installé."
fi

# Vérifier si AWS CLI est installé
if ! command -v aws &> /dev/null; then
    echo "AWS CLI n'est pas installé. Installation..."
    curl "https://awscli.amazonaws.com/awscli-exe-linux-x86_64.zip" -o "awscliv2.zip"
    unzip awscliv2.zip
    sudo ./aws/install
    rm -rf aws awscliv2.zip
    echo "AWS CLI installé."
fi

# Créer la structure de répertoires
echo "Création de la structure de répertoires..."
mkdir -p /opt/ziplink/scripts
mkdir -p /opt/ziplink/data/db-primary
mkdir -p /opt/ziplink/data/db-replica

echo "Structure de répertoires créée dans /opt/ziplink"

# Initialiser Docker Swarm si pas déjà fait
if ! docker info | grep -q "Swarm: active"; then
    echo "Initialisation de Docker Swarm..."
    docker swarm init
    echo "Docker Swarm initialisé."
else
    echo "Docker Swarm est déjà initialisé."
fi

# Créer le réseau overlay
echo "Création du réseau overlay..."
if ! docker network ls | grep -q "ziplink-network"; then
    docker network create --driver overlay --attachable ziplink-network
    echo "Réseau ziplink-network créé."
else
    echo "Le réseau ziplink-network existe déjà."
fi

# Demander à l'utilisateur d'étiqueter les nœuds
echo ""
echo "=== Configuration des nœuds pour la base de données ==="
echo "Listez vos nœuds Docker Swarm :"
docker node ls

echo ""
echo "Vous devez maintenant étiqueter vos nœuds pour la réplication de base de données."
echo "Exécutez les commandes suivantes :"
echo ""
echo "# Pour désigner le nœud primaire de la base de données :"
echo "docker node update --label-add database.primary=true <NODE-ID-PRIMAIRE>"
echo ""
echo "# Pour désigner le nœud réplique de la base de données :"
echo "docker node update --label-add database.replica=true <NODE-ID-REPLIQUE>"
echo ""
echo "Remplacez <NODE-ID-PRIMAIRE> et <NODE-ID-REPLIQUE> par les ID réels de vos nœuds."

echo ""
echo "=== Configuration AWS ==="
echo "Configurez AWS CLI avec vos credentials :"
echo "aws configure"
echo ""
echo "Utilisez les mêmes credentials que ceux configurés dans GitHub Actions."

echo ""
echo "=== Préparation terminée ==="
echo "Le serveur est maintenant prêt pour les déploiements automatiques."
echo "Les prochains déploiements se feront automatiquement via GitHub Actions."