# Makefile pour ZipLink
# Commandes simplifiées pour gérer l'application Docker

.PHONY: help dev prod build clean logs status stop restart scale cluster-init cluster-deploy cluster-status cluster-stop

# Couleurs pour l'affichage
GREEN=\033[0;32m
YELLOW=\033[1;33m
RED=\033[0;31m
NC=\033[0m # No Color

help: ## Afficher cette aide
	@echo "$(YELLOW)ZipLink - Commandes disponibles:$(NC)"
	@echo ""
	@grep -E '^[a-zA-Z_-]+:.*?## .*$$' $(MAKEFILE_LIST) | sort | awk 'BEGIN {FS = ":.*?## "}; {printf "$(GREEN)%-20s$(NC) %s\n", $$1, $$2}'
	@echo ""

# DÉVELOPPEMENT
dev: ## Démarrer en mode développement
	@echo "$(GREEN)Démarrage en mode développement...$(NC)"
	@chmod +x scripts/deploy-development.sh
	@./scripts/deploy-development.sh

dev-build: ## Reconstruire et démarrer en mode développement
	@echo "$(GREEN)Reconstruction et démarrage en développement...$(NC)"
	@docker-compose -f docker-compose.yml -f docker-compose.override.yml build --no-cache
	@docker-compose -f docker-compose.yml -f docker-compose.override.yml up -d

# PRODUCTION
prod: ## Démarrer en mode production
	@echo "$(GREEN)Démarrage en mode production...$(NC)"
	@chmod +x scripts/deploy-production.sh
	@./scripts/deploy-production.sh

prod-build: ## Reconstruire et démarrer en mode production
	@echo "$(GREEN)Reconstruction et démarrage en production...$(NC)"
	@docker-compose -f docker-compose.yml build --no-cache
	@docker-compose -f docker-compose.yml up -d

# BUILD
build: ## Construire toutes les images
	@echo "$(GREEN)Construction des images...$(NC)"
	@docker-compose build

build-server: ## Construire uniquement l'image du serveur
	@echo "$(GREEN)Construction de l'image serveur...$(NC)"
	@docker build -t ziplink/server:latest -f Server/Dockerfile .

build-web: ## Construire uniquement l'image web
	@echo "$(GREEN)Construction de l'image web...$(NC)"
	@docker build -t ziplink/web:latest -f Web/Dockerfile .

# MONITORING
logs: ## Afficher les logs (dev)
	@docker-compose -f docker-compose.yml -f docker-compose.override.yml logs -f

logs-prod: ## Afficher les logs (production)
	@docker-compose -f docker-compose.yml logs -f

logs-server: ## Afficher les logs du serveur uniquement
	@docker-compose logs -f ziplink-server

logs-web: ## Afficher les logs du web uniquement
	@docker-compose logs -f ziplink-web

logs-db: ## Afficher les logs de la base de données
	@docker-compose logs -f ziplink-db-primary

status: ## Afficher le statut des services
	@echo "$(GREEN)Statut des services:$(NC)"
	@docker-compose ps

# 🛑 CONTRÔLE
stop: ## Arrêter tous les services
	@echo "$(RED)⏹️  Arrêt des services...$(NC)"
	@docker-compose -f docker-compose.yml -f docker-compose.override.yml down

stop-prod: ## Arrêter les services de production
	@echo "$(RED)⏹️  Arrêt des services de production...$(NC)"
	@docker-compose -f docker-compose.yml down

restart: ## Redémarrer tous les services (dev)
	@echo "$(YELLOW)🔄 Redémarrage des services...$(NC)"
	@docker-compose -f docker-compose.yml -f docker-compose.override.yml restart

restart-prod: ## Redémarrer tous les services (production)
	@echo "$(YELLOW)🔄 Redémarrage des services de production...$(NC)"
	@docker-compose -f docker-compose.yml restart

restart-server: ## Redémarrer uniquement le serveur
	@docker-compose restart ziplink-server

restart-web: ## Redémarrer uniquement le web
	@docker-compose restart ziplink-web

# 📏 MISE À L'ÉCHELLE
scale: ## Mettre à l'échelle (production par défaut: 2 répliques)
	@echo "$(GREEN)📏 Mise à l'échelle des services...$(NC)"
	@docker-compose -f docker-compose.yml up -d --scale ziplink-server=2 --scale ziplink-web=2

scale-up: ## Augmenter à 3 répliques
	@echo "$(GREEN)📈 Augmentation à 3 répliques...$(NC)"
	@docker-compose -f docker-compose.yml up -d --scale ziplink-server=3 --scale ziplink-web=3

scale-down: ## Réduire à 1 réplique
	@echo "$(YELLOW)📉 Réduction à 1 réplique...$(NC)"
	@docker-compose -f docker-compose.yml up -d --scale ziplink-server=1 --scale ziplink-web=1

# 🐳 CLUSTER SWARM
cluster-init: ## Initialiser le cluster Swarm
	@echo "$(GREEN)🐳 Initialisation du cluster Swarm...$(NC)"
	@chmod +x scripts/cluster-management.sh
	@./scripts/cluster-management.sh init

cluster-deploy: ## Déployer sur le cluster Swarm
	@echo "$(GREEN)📦 Déploiement sur le cluster...$(NC)"
	@chmod +x scripts/cluster-management.sh
	@./scripts/cluster-management.sh deploy

cluster-scale: ## Mettre à l'échelle sur le cluster
	@echo "$(GREEN)📏 Mise à l'échelle du cluster...$(NC)"
	@chmod +x scripts/cluster-management.sh
	@./scripts/cluster-management.sh scale

cluster-status: ## Statut du cluster Swarm
	@echo "$(GREEN)📊 Statut du cluster...$(NC)"
	@chmod +x scripts/cluster-management.sh
	@./scripts/cluster-management.sh status

cluster-stop: ## Arrêter le cluster Swarm
	@echo "$(RED)⏹️  Arrêt du cluster...$(NC)"
	@chmod +x scripts/cluster-management.sh
	@./scripts/cluster-management.sh stop

cluster-remove: ## Supprimer complètement le cluster
	@echo "$(RED)🗑️  Suppression du cluster...$(NC)"
	@chmod +x scripts/cluster-management.sh
	@./scripts/cluster-management.sh remove

# 🧹 NETTOYAGE
clean: ## Nettoyer les conteneurs et images inutilisés
	@echo "$(YELLOW)🧹 Nettoyage...$(NC)"
	@docker system prune -f

clean-all: ## Nettoyage complet (attention: supprime tout)
	@echo "$(RED)🧹 Nettoyage complet...$(NC)"
	@read -p "Êtes-vous sûr? Cette action supprimera TOUS les conteneurs, images et volumes. [y/N] " -n 1 -r; \
	echo; \
	if [[ $$REPLY =~ ^[Yy]$$ ]]; then \
		docker system prune -af --volumes; \
	fi

clean-volumes: ## Supprimer les volumes de données
	@echo "$(RED)🗑️  Suppression des volumes...$(NC)"
	@read -p "Attention: Cela supprimera toutes les données! Continuer? [y/N] " -n 1 -r; \
	echo; \
	if [[ $$REPLY =~ ^[Yy]$$ ]]; then \
		docker-compose down -v; \
		docker volume rm ziplink_db_primary_data ziplink_db_replica_data prometheus_data 2>/dev/null || true; \
	fi

# 🔧 MAINTENANCE
migration: ## Appliquer les migrations de base de données
	@echo "$(GREEN)📦 Application des migrations...$(NC)"
	@docker-compose exec ziplink-server dotnet ef database update

shell-server: ## Ouvrir un shell dans le conteneur serveur
	@docker-compose exec ziplink-server /bin/bash

shell-db: ## Ouvrir un shell SQL Server
	@docker-compose exec ziplink-db-primary /opt/mssql-tools/bin/sqlcmd -S localhost -U SA -P 'ZipLink123!'

backup-db: ## Sauvegarder la base de données
	@echo "$(GREEN)💾 Sauvegarde de la base de données...$(NC)"
	@mkdir -p backups
	@docker-compose exec ziplink-db-primary /opt/mssql-tools/bin/sqlcmd -S localhost -U SA -P 'ZipLink123!' -Q "BACKUP DATABASE [ZIPLINK] TO DISK = N'/tmp/ziplink_backup.bak'"
	@docker cp $$(docker-compose ps -q ziplink-db-primary):/tmp/ziplink_backup.bak ./backups/ziplink_backup_$$(date +%Y%m%d_%H%M%S).bak
	@echo "$(GREEN)✅ Sauvegarde terminée dans ./backups/$(NC)"

# 🌐 ACCÈS RAPIDE
open: ## Ouvrir l'application dans le navigateur
	@echo "$(GREEN)🌐 Ouverture de l'application...$(NC)"
	@open http://localhost:3000 || xdg-open http://localhost:3000 || echo "Ouvrez http://localhost:3000 dans votre navigateur"

open-prod: ## Ouvrir l'application de production
	@echo "$(GREEN)🌐 Ouverture de l'application de production...$(NC)"
	@open http://localhost || xdg-open http://localhost || echo "Ouvrez http://localhost dans votre navigateur"

open-monitoring: ## Ouvrir Prometheus
	@echo "$(GREEN)📊 Ouverture du monitoring...$(NC)"
	@open http://localhost:9090 || xdg-open http://localhost:9090 || echo "Ouvrez http://localhost:9090 dans votre navigateur"

# Par défaut, afficher l'aide
.DEFAULT_GOAL := help