#!/bin/bash

# Script de gestion des clusters Docker Swarm pour ZipLink
# Utilisation: ./cluster-management.sh [init|deploy|scale|status|stop]

set -e

STACK_NAME="ziplink"
COMPOSE_FILE="docker-compose.yml"

show_help() {
    echo "Script de gestion du cluster Docker Swarm ZipLink"
    echo ""
    echo "Usage: $0 {init|deploy|scale|status|stop|remove|logs}"
    echo ""
    echo "Commandes:"
    echo "  init     - Initialiser le cluster Swarm"
    echo "  deploy   - Déployer la stack ZipLink"
    echo "  scale    - Mettre à l'échelle les services"
    echo "  status   - Afficher le statut du cluster"
    echo "  stop     - Arrêter tous les services"
    echo "  remove   - Supprimer la stack complètement"
    echo "  logs     - Afficher les logs des services"
    echo ""
}

init_swarm() {
    echo "Initialisation du cluster Docker Swarm..."
    
    if docker info | grep -q "Swarm: active"; then
        echo "OK: Swarm déjà initialisé"
    else
        echo "Initialisation du mode Swarm..."
        docker swarm init
        echo "OK: Swarm initialisé avec succès"
    fi
    
    # Créer les réseaux nécessaires
    echo "Création des réseaux..."
    docker network create --driver overlay --attachable ziplink-network || echo "OK: Réseau ziplink-network existe déjà"
    
    # Créer les volumes
    echo "Création des volumes..."
    docker volume create ziplink_db_primary_data || echo "OK: Volume ziplink_db_primary_data existe déjà"
    docker volume create ziplink_db_replica_data || echo "OK: Volume ziplink_db_replica_data existe déjà"
    docker volume create prometheus_data || echo "OK: Volume prometheus_data existe déjà"
    
    echo "OK: Cluster Swarm initialisé et configuré"
    echo ""
    echo "Étape suivante: Configurez les étiquettes des nœuds"
    echo "   ./scripts/setup-cluster-nodes.sh setup --auto"
}

deploy_stack() {
    echo "Déploiement de la stack ZipLink..."
    
    if [ ! -f "$COMPOSE_FILE" ]; then
        echo "ERREUR: Fichier $COMPOSE_FILE introuvable"
        exit 1
    fi
    
    # Vérifier que les variables d'environnement sont chargées
    if [ ! -f ".env.production" ]; then
        echo "ERREUR: Fichier .env.production introuvable"
        echo "   Créez le fichier .env.production avec les bonnes variables"
        exit 1
    fi
    
    # Charger les variables d'environnement
    echo "Chargement des variables d'environnement..."
    export $(cat .env.production | grep -v '^#' | xargs)
    
    # Vérifier que les nœuds sont étiquetés
    echo "Vérification des étiquettes des nœuds..."
    
    # Méthode alternative pour trouver les nœuds avec les labels
    PRIMARY_NODE=$(docker node ls -q | xargs -I {} docker node inspect {} --format '{{if .Spec.Labels}}{{if eq (index .Spec.Labels "database.primary") "true"}}{{.ID}}{{end}}{{end}}' | head -1)
    REPLICA_NODE=$(docker node ls -q | xargs -I {} docker node inspect {} --format '{{if .Spec.Labels}}{{if eq (index .Spec.Labels "database.replica") "true"}}{{.ID}}{{end}}{{end}}' | head -1)
    
    # Afficher tous les labels pour debug
    echo "Labels actuels des nœuds :"
    docker node ls -q | xargs -I {} docker node inspect {} --format '{{ .ID }} {{ .Description.Hostname }} {{ range $k, $v := .Spec.Labels }}{{ $k }}={{ $v }} {{ end }}'
    
    if [ -z "$PRIMARY_NODE" ]; then
        echo "ERREUR: Aucun nœud étiqueté pour la base de données primaire détecté"
        echo "   Tentative de résolution automatique..."
        
        # Auto-résolution : prendre le nœud manager comme primaire
        MANAGER_NODE=$(docker node ls --filter "role=manager" --format "{{.ID}}" | head -1)
        if [ -n "$MANAGER_NODE" ]; then
            echo "   Attribution du label database.primary=true au nœud manager: $MANAGER_NODE"
            docker node update --label-add database.primary=true $MANAGER_NODE
            PRIMARY_NODE=$MANAGER_NODE
        else
            echo "   Aucun nœud manager trouvé, attribution au premier nœud disponible"
            FIRST_NODE=$(docker node ls --format "{{.ID}}" | head -1)
            docker node update --label-add database.primary=true $FIRST_NODE
            PRIMARY_NODE=$FIRST_NODE
        fi
    fi
    
    if [ -z "$REPLICA_NODE" ]; then
        echo "ERREUR: Aucun nœud étiqueté pour la base de données réplique détecté"
        echo "   Tentative de résolution automatique..."
        
        # Auto-résolution : prendre un nœud différent du primaire
        AVAILABLE_NODE=$(docker node ls --format "{{.ID}}" | grep -v "$PRIMARY_NODE" | head -1)
        if [ -n "$AVAILABLE_NODE" ]; then
            echo "   Attribution du label database.replica=true au nœud: $AVAILABLE_NODE"
            docker node update --label-add database.replica=true $AVAILABLE_NODE
            REPLICA_NODE=$AVAILABLE_NODE
        else
            echo "   Un seul nœud disponible, utilisation du même pour la réplique"
            docker node update --label-add database.replica=true $PRIMARY_NODE
            REPLICA_NODE=$PRIMARY_NODE
        fi
    fi
    
    echo "✓ Nœud primaire configuré : $PRIMARY_NODE"
    echo "✓ Nœud réplique configuré : $REPLICA_NODE"
    
    # Convertir le docker-compose pour Swarm avec variables d'environnement
    echo "Préparation du fichier de déploiement Swarm..."
    cat > docker-stack.yml << EOF
version: '3.8'

services:
  ziplink-db-primary:
    image: mcr.microsoft.com/mssql/server:2022-latest
    environment:
      - ACCEPT_EULA=Y
      - SA_PASSWORD=${DB_SA_PASSWORD}
      - MSSQL_PID=Express
      - MSSQL_AGENT_ENABLED=true
    volumes:
      - ziplink_db_primary_data:/var/opt/mssql
    networks:
      - ziplink-network
    deploy:
      replicas: 1
      placement:
        constraints: [node.labels.database.primary == true]
      restart_policy:
        condition: on-failure
        delay: 10s
        max_attempts: 3
    healthcheck:
      test: ["/opt/mssql-tools/bin/sqlcmd", "-S", "localhost", "-U", "SA", "-P", "${DB_SA_PASSWORD}", "-Q", "SELECT 1"]
      interval: 30s
      timeout: 10s
      retries: 3
      start_period: 60s

  ziplink-db-replica:
    image: mcr.microsoft.com/mssql/server:2022-latest
    environment:
      - ACCEPT_EULA=Y
      - SA_PASSWORD=${DB_SA_PASSWORD}
      - MSSQL_PID=Express
      - MSSQL_AGENT_ENABLED=true
    volumes:
      - ziplink_db_replica_data:/var/opt/mssql
    networks:
      - ziplink-network
    deploy:
      replicas: 1
      placement:
        constraints: [node.labels.database.replica == true]
      restart_policy:
        condition: on-failure
        delay: 10s
        max_attempts: 3

  ziplink-server:
    image: \${REGISTRY_URL}/ziplink/server:\${SERVER_IMAGE_TAG}
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ASPNETCORE_URLS=http://+:8080
      - ConnectionStrings__DefaultConnection=Server=ziplink-db-primary,1433;Database=ZIPLINK;User Id=SA;Password=\${DB_SA_PASSWORD};TrustServerCertificate=True;
      - Jwt__Key=\${JWT_KEY}
      - Jwt__Issuer=\${JWT_ISSUER}
      - Jwt__Audience=\${JWT_AUDIENCE}
    networks:
      - ziplink-network
    deploy:
      replicas: 2
      update_config:
        parallelism: 1
        delay: 10s
        failure_action: rollback
        order: start-first
      restart_policy:
        condition: on-failure
        delay: 5s
        max_attempts: 3
      placement:
        max_replicas_per_node: 1
      resources:
        limits:
          cpus: '0.5'
          memory: 512M
        reservations:
          cpus: '0.25'
          memory: 256M

  ziplink-web:
    image: \${REGISTRY_URL}/ziplink/web:\${WEB_IMAGE_TAG}
    networks:
      - ziplink-network
    deploy:
      replicas: 2
      update_config:
        parallelism: 1
        delay: 10s
        failure_action: rollback
        order: start-first
      restart_policy:
        condition: on-failure
        delay: 5s
        max_attempts: 3
      placement:
        max_replicas_per_node: 1
      resources:
        limits:
          cpus: '0.25'
          memory: 256M
        reservations:
          cpus: '0.125'
          memory: 128M

  ziplink-loadbalancer:
    image: nginx:alpine
    ports:
      - "${NGINX_HTTP_PORT:-80}:80"
      - "${NGINX_HTTPS_PORT:-443}:443"
    configs:
      - source: nginx_config
        target: /etc/nginx/nginx.conf
    networks:
      - ziplink-network
    deploy:
      replicas: 1
      placement:
        constraints: [node.role == manager]
      restart_policy:
        condition: on-failure

  ziplink-monitor:
    image: prom/prometheus:latest
    ports:
      - "${MONITORING_PORT:-9090}:9090"
    configs:
      - source: prometheus_config
        target: /etc/prometheus/prometheus.yml
    volumes:
      - prometheus_data:/prometheus
    networks:
      - ziplink-network
    deploy:
      replicas: 1
      placement:
        constraints: [node.role == manager]

configs:
  nginx_config:
    file: ./nginx/nginx.conf
  prometheus_config:
    file: ./monitoring/prometheus.yml

volumes:
  ziplink_db_primary_data:
    external: true
  ziplink_db_replica_data:
    external: true
  prometheus_data:
    external: true

networks:
  ziplink-network:
    external: true
EOF
    
    # Vérifier que les images du registre sont disponibles
    echo "Vérification des images du registre..."
    if [ -n "$REGISTRY_URL" ] && [ -n "$SERVER_IMAGE_TAG" ] && [ -n "$WEB_IMAGE_TAG" ]; then
        echo "✓ Configuration registre détectée:"
        echo "  Registry: $REGISTRY_URL"
        echo "  Server: $REGISTRY_URL/ziplink/server:$SERVER_IMAGE_TAG"
        echo "  Web: $REGISTRY_URL/ziplink/web:$WEB_IMAGE_TAG"
    else
        echo "ATTENTION: Variables de registre manquantes, utilisation des images locales"
    fi
    
    # Vérifier et créer les fichiers de configuration nécessaires
    echo "Vérification des fichiers de configuration..."
    
    # Créer le fichier prometheus.yml s'il n'existe pas
    if [ ! -f "./monitoring/prometheus.yml" ]; then
        echo "Création du fichier prometheus.yml manquant..."
        mkdir -p ./monitoring
        cat > ./monitoring/prometheus.yml << 'EOF'
global:
  scrape_interval: 15s
  evaluation_interval: 15s

scrape_configs:
  - job_name: 'prometheus'
    static_configs:
      - targets: ['localhost:9090']

  - job_name: 'ziplink-server'
    static_configs:
      - targets: ['ziplink-server:8080']
    metrics_path: '/health'
    scrape_interval: 30s
EOF
    fi
    
    # Déployer la stack
    echo "Déploiement de la stack..."
    docker stack deploy -c docker-stack.yml $STACK_NAME
    
    echo "OK: Stack déployée avec succès"
}

scale_services() {
    echo "📊 Mise à l'échelle des services..."
    
    echo "Mise à l'échelle du serveur API à 3 répliques..."
    docker service scale ${STACK_NAME}_ziplink-server=3
    
    echo "Mise à l'échelle du web frontend à 3 répliques..."
    docker service scale ${STACK_NAME}_ziplink-web=3
    
    echo "✅ Services mis à l'échelle"
}

show_status() {
    echo "📊 Statut du cluster ZipLink..."
    echo ""
    
    echo "🐳 Informations Swarm:"
    docker info | grep -A 5 "Swarm:"
    echo ""
    
    echo "🌐 Nœuds du cluster:"
    docker node ls
    echo ""
    
    echo "🏷️  Étiquettes des nœuds:"
    for node_id in $(docker node ls -q); do
        hostname=$(docker node inspect "$node_id" --format '{{.Description.Hostname}}')
        labels=$(docker node inspect "$node_id" --format '{{range $key, $value := .Spec.Labels}}{{$key}}={{$value}} {{end}}')
        echo "  $hostname: $labels"
    done
    echo ""
    
    echo "📦 Services de la stack:"
    docker stack services $STACK_NAME 2>/dev/null || echo "Aucune stack déployée"
    echo ""
    
    echo "🔄 Tâches en cours:"
    docker stack ps $STACK_NAME --no-trunc 2>/dev/null || echo "Aucune tâche en cours"
    echo ""
    
    echo "🌐 Réseaux:"
    docker network ls | grep ziplink
    echo ""
    
    echo "💾 Volumes:"
    docker volume ls | grep ziplink
}

stop_services() {
    echo "⏹️  Arrêt des services ZipLink..."
    
    docker stack services $STACK_NAME --format "table {{.Name}}" | grep -v NAME | while read service; do
        echo "Arrêt du service: $service"
        docker service scale $service=0
    done
    
    echo "✅ Tous les services arrêtés"
}

remove_stack() {
    echo "🗑️  Suppression complète de la stack ZipLink..."
    
    docker stack rm $STACK_NAME
    
    echo "⏳ Attente de la suppression complète..."
    sleep 10
    
    # Nettoyer le fichier temporaire
    rm -f docker-stack.yml
    
    echo "✅ Stack supprimée"
}

show_logs() {
    echo "📝 Logs des services ZipLink..."
    
    if [ -z "$2" ]; then
        echo "Services disponibles:"
        docker stack services $STACK_NAME --format "table {{.Name}}" 2>/dev/null || echo "Aucune stack déployée"
        echo ""
        echo "Usage: $0 logs <service_name>"
        return
    fi
    
    docker service logs -f "${STACK_NAME}_$2"
}

# Vérifier que Docker Swarm est disponible
if ! command -v docker &> /dev/null; then
    echo "❌ Docker n'est pas installé"
    exit 1
fi

case "${1:-}" in
    init)
        init_swarm
        ;;
    deploy)
        deploy_stack
        ;;
    scale)
        scale_services
        ;;
    status)
        show_status
        ;;
    stop)
        stop_services
        ;;
    remove)
        remove_stack
        ;;
    logs)
        show_logs "$@"
        ;;
    *)
        show_help
        exit 1
        ;;
esac