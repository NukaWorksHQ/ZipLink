#!/bin/bash

# Script de configuration des nœuds Docker Swarm pour ZipLink
# Utilisation: ./setup-cluster-nodes.sh

set -e

echo "Configuration des étiquettes des nœuds Docker Swarm pour ZipLink"

show_help() {
    echo ""
    echo "Usage: $0 {setup|list|remove} [options]"
    echo ""
    echo "Commandes:"
    echo "  setup     - Configurer les étiquettes des nœuds"
    echo "  list      - Lister tous les nœuds et leurs étiquettes"
    echo "  remove    - Supprimer les étiquettes ZipLink"
    echo ""
    echo "Options pour 'setup':"
    echo "  --primary-node <node-id>    - ID du nœud pour la DB principale"
    echo "  --replica-node <node-id>    - ID du nœud pour la DB réplique"
    echo "  --auto                      - Configuration automatique (2 premiers nœuds)"
    echo ""
    echo "Exemples:"
    echo "  $0 setup --auto"
    echo "  $0 setup --primary-node node1 --replica-node node2"
    echo "  $0 list"
    echo "  $0 remove"
    echo ""
}

list_nodes() {
    echo "Liste des nœuds Docker Swarm:"
    echo ""
    docker node ls --format "table {{.ID}}\t{{.Hostname}}\t{{.Status}}\t{{.Availability}}\t{{.ManagerStatus}}"
    echo ""
    
    echo "Étiquettes actuelles des nœuds:"
    for node_id in $(docker node ls -q); do
        hostname=$(docker node inspect "$node_id" --format '{{.Description.Hostname}}')
        echo ""
        echo "Nœud: $hostname ($node_id)"
        
        # Récupérer les étiquettes
        labels=$(docker node inspect "$node_id" --format '{{range $key, $value := .Spec.Labels}}{{$key}}={{$value}} {{end}}')
        if [ -n "$labels" ]; then
            echo "  Étiquettes: $labels"
        else
            echo "  Étiquettes: Aucune"
        fi
    done
    echo ""
}

setup_labels() {
    local primary_node="$1"
    local replica_node="$2"
    
    if [ -z "$primary_node" ] || [ -z "$replica_node" ]; then
        echo "❌ Nœuds primaire et réplique requis"
        return 1
    fi
    
    echo "🏷️  Configuration des étiquettes..."
    
    # Vérifier que les nœuds existent
    if ! docker node inspect "$primary_node" &>/dev/null; then
        echo "❌ Nœud primaire '$primary_node' introuvable"
        return 1
    fi
    
    if ! docker node inspect "$replica_node" &>/dev/null; then
        echo "❌ Nœud réplique '$replica_node' introuvable"
        return 1
    fi
    
    if [ "$primary_node" = "$replica_node" ]; then
        echo "❌ Les nœuds primaire et réplique doivent être différents"
        return 1
    fi
    
    # Appliquer les étiquettes
    echo "📌 Configuration du nœud primaire: $primary_node"
    docker node update --label-add database.primary=true "$primary_node"
    docker node update --label-add database.replica=false "$primary_node"
    docker node update --label-add ziplink.role=database-primary "$primary_node"
    
    echo "📌 Configuration du nœud réplique: $replica_node"
    docker node update --label-add database.primary=false "$replica_node"
    docker node update --label-add database.replica=true "$replica_node"
    docker node update --label-add ziplink.role=database-replica "$replica_node"
    
    # Étiqueter les autres nœuds pour les services applicatifs
    for node_id in $(docker node ls -q); do
        if [ "$node_id" != "$primary_node" ] && [ "$node_id" != "$replica_node" ]; then
            hostname=$(docker node inspect "$node_id" --format '{{.Description.Hostname}}')
            echo "📌 Configuration du nœud applicatif: $hostname ($node_id)"
            docker node update --label-add database.primary=false "$node_id"
            docker node update --label-add database.replica=false "$node_id"
            docker node update --label-add ziplink.role=application "$node_id"
        fi
    done
    
    echo ""
    echo "✅ Configuration des étiquettes terminée!"
    echo ""
    echo "📊 Résumé de la configuration:"
    echo "  🗄️  Nœud DB Primaire: $(docker node inspect "$primary_node" --format '{{.Description.Hostname}}') ($primary_node)"
    echo "  🗄️  Nœud DB Réplique: $(docker node inspect "$replica_node" --format '{{.Description.Hostname}}') ($replica_node)"
    
    # Lister les autres nœuds
    other_nodes=$(docker node ls -q | grep -v "$primary_node" | grep -v "$replica_node")
    if [ -n "$other_nodes" ]; then
        echo "  💻 Nœuds applicatifs:"
        for node_id in $other_nodes; do
            hostname=$(docker node inspect "$node_id" --format '{{.Description.Hostname}}')
            echo "    - $hostname ($node_id)"
        done
    fi
}

auto_setup() {
    echo "🤖 Configuration automatique des nœuds..."
    
    # Récupérer la liste des nœuds
    nodes=($(docker node ls -q))
    
    if [ ${#nodes[@]} -lt 2 ]; then
        echo "❌ Il faut au moins 2 nœuds pour configurer la réplication"
        echo "   Nœuds disponibles: ${#nodes[@]}"
        return 1
    fi
    
    # Utiliser les 2 premiers nœuds
    primary_node="${nodes[0]}"
    replica_node="${nodes[1]}"
    
    echo "🎯 Sélection automatique:"
    echo "  Primaire: $(docker node inspect "$primary_node" --format '{{.Description.Hostname}}') ($primary_node)"
    echo "  Réplique: $(docker node inspect "$replica_node" --format '{{.Description.Hostname}}') ($replica_node)"
    echo ""
    
    read -p "Continuer avec cette configuration? [y/N] " -n 1 -r
    echo
    if [[ $REPLY =~ ^[Yy]$ ]]; then
        setup_labels "$primary_node" "$replica_node"
    else
        echo "❌ Configuration annulée"
        return 1
    fi
}

remove_labels() {
    echo "🗑️  Suppression des étiquettes ZipLink..."
    
    for node_id in $(docker node ls -q); do
        hostname=$(docker node inspect "$node_id" --format '{{.Description.Hostname}}')
        echo "🧹 Nettoyage du nœud: $hostname ($node_id)"
        
        docker node update --label-rm database.primary "$node_id" 2>/dev/null || true
        docker node update --label-rm database.replica "$node_id" 2>/dev/null || true
        docker node update --label-rm ziplink.role "$node_id" 2>/dev/null || true
    done
    
    echo "✅ Toutes les étiquettes ZipLink supprimées"
}

# Vérifier que Docker Swarm est actif
if ! docker info | grep -q "Swarm: active"; then
    echo "❌ Docker Swarm n'est pas initialisé"
    echo "   Exécutez 'docker swarm init' d'abord"
    exit 1
fi

case "${1:-}" in
    setup)
        if [ "$2" = "--auto" ]; then
            auto_setup
        elif [ "$2" = "--primary-node" ] && [ "$4" = "--replica-node" ]; then
            setup_labels "$3" "$5"
        else
            echo "❌ Options invalides pour 'setup'"
            show_help
            exit 1
        fi
        ;;
    list)
        list_nodes
        ;;
    remove)
        remove_labels
        ;;
    *)
        show_help
        exit 1
        ;;
esac