#!/bin/bash

# Script pour initialiser la base de données ZipLink avec les variables d'environnement
# Ce script remplace les variables dans le fichier SQL avant de l'exécuter

set -e

# Vérifier que les variables d'environnement sont définies
if [ -z "$DB_USER_PASSWORD" ]; then
    echo "Erreur: La variable DB_USER_PASSWORD n'est pas définie"
    exit 1
fi

if [ -z "$SA_PASSWORD" ]; then
    echo "Erreur: La variable SA_PASSWORD n'est pas définie"
    exit 1
fi

# Répertoire des scripts SQL
SCRIPT_DIR="$(dirname "$0")"
SQL_FILE="$SCRIPT_DIR/01-init-database.sql"
TEMP_SQL_FILE="/tmp/01-init-database-processed.sql"

echo "Préparation du script d'initialisation de la base de données..."

# Remplacer les variables dans le fichier SQL
sed "s/\$(DB_USER_PASSWORD)/$DB_USER_PASSWORD/g" "$SQL_FILE" > "$TEMP_SQL_FILE"

# Attendre que SQL Server soit prêt
echo "Attente de la disponibilité de SQL Server..."
for i in {1..30}; do
    if /opt/mssql-tools/bin/sqlcmd -S localhost -U SA -P "$SA_PASSWORD" -Q "SELECT 1" &>/dev/null; then
        echo "SQL Server est disponible"
        break
    fi
    echo "Tentative $i/30..."
    sleep 2
done

# Exécuter le script SQL
echo "Exécution du script d'initialisation..."
/opt/mssql-tools/bin/sqlcmd -S localhost -U SA -P "$SA_PASSWORD" -i "$TEMP_SQL_FILE"

if [ $? -eq 0 ]; then
    echo "Base de données initialisée avec succès"
else
    echo "Erreur lors de l'initialisation de la base de données"
    exit 1
fi

# Nettoyer le fichier temporaire
rm -f "$TEMP_SQL_FILE"

echo "Script d'initialisation terminé"