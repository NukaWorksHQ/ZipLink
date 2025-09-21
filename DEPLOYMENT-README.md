# Guide de Déploiement ZipLink

## Configuration initiale du serveur

### 1. Préparer le serveur (une seule fois)

Connectez-vous à votre serveur et exécutez :

```bash
# Télécharger le script de préparation
curl -fsSL https://raw.githubusercontent.com/powerm1nt/ZipLink/master/scripts/setup-server.sh -o setup-server.sh
chmod +x setup-server.sh
./setup-server.sh
```

Ou copiez le script `scripts/setup-server.sh` sur votre serveur et exécutez-le.

### 2. Étiqueter les nœuds pour la base de données

Après avoir exécuté le script de préparation, étiquetez vos nœuds :

```bash
# Lister les nœuds
docker node ls

# Étiqueter le nœud primaire pour la base de données
docker node update --label-add database.primary=true <NODE-ID-1>

# Étiqueter le nœud réplique pour la base de données
docker node update --label-add database.replica=true <NODE-ID-2>
```

### 3. Configurer AWS CLI

```bash
aws configure
```

Utilisez les mêmes credentials que ceux configurés dans GitHub Actions.

## Configuration GitHub Actions

### Secrets requis

Configurez ces secrets dans votre repository GitHub (Settings > Secrets and variables > Actions) :

#### AWS ECR

- `AWS_ACCESS_KEY_ID`
- `AWS_SECRET_ACCESS_KEY`
- `AWS_REGION` (ex: eu-west-3)
- `ECR_REPOSITORY_BASE` (ex: 123456789012.dkr.ecr.eu-west-3.amazonaws.com)

#### Serveur Docker Swarm

- `SWARM_MANAGER_HOST` (IP ou nom d'hôte)
- `SWARM_MANAGER_USER` (nom d'utilisateur SSH)
- `SWARM_SSH_PRIVATE_KEY` (clé privée SSH au format PEM)

#### Application

- `DB_SA_PASSWORD` (mot de passe SA de SQL Server)
- `DB_USER_PASSWORD` (mot de passe utilisateur applicatif)
- `JWT_KEY` (clé secrète JWT, minimum 32 caractères)
- `JWT_ISSUER` (ex: ZipLink)
- `JWT_AUDIENCE` (ex: ZipLink-Users)
- `DOMAIN` (votre domaine, optionnel)
- `ALLOWED_HOSTS` (hosts autorisés, optionnel)

## Déploiement automatique

Le déploiement se fait automatiquement quand vous poussez sur la branche `main`.

### Processus de déploiement

1. **Build des images Docker** : Server et Web
2. **Push vers AWS ECR** avec tag basé sur le commit SHA
3. **Transfert des fichiers** : docker-compose.yml et scripts vers le serveur
4. **Génération du .env.production** depuis les secrets GitHub
5. **Déploiement Docker Swarm** avec 2 répliques par service

### Vérifier le déploiement

Connectez-vous au serveur et vérifiez :

```bash
# Status des services
docker service ls

# Détails du stack
docker stack ps ziplink

# Logs des services
docker service logs ziplink_ziplink-server
docker service logs ziplink_ziplink-web
docker service logs ziplink_ziplink-db-primary
```

## Dépannage

### Erreur "No such file or directory"

Si vous obtenez cette erreur lors du déploiement :

```
bash: line 8: cd: /opt/ziplink: No such file or directory
```

Exécutez le script de préparation du serveur :

```bash
./scripts/setup-server.sh
```

### Problèmes de permissions

```bash
sudo chown -R $USER:$USER /opt/ziplink
chmod +x /opt/ziplink/scripts/*.sh
```

### Services qui ne démarrent pas

Vérifiez les logs :

```bash
docker service logs --tail 50 ziplink_ziplink-server
```

Vérifiez les variables d'environnement :

```bash
cd /opt/ziplink
cat .env.production
```

### Rollback en cas de problème

```bash
cd /opt/ziplink
./scripts/cluster-management.sh rollback
```

## Monitoring

### Health checks

Les services incluent des health checks automatiques. Vérifiez leur état :

```bash
docker service ps ziplink_ziplink-server
docker service ps ziplink_ziplink-web
```

### Métriques

Si le monitoring est activé, accédez à Prometheus sur le port 9090.

## Mise à l'échelle

Pour modifier le nombre de répliques :

```bash
cd /opt/ziplink
./scripts/cluster-management.sh scale
```

Ou manuellement :

```bash
docker service scale ziplink_ziplink-server=3
docker service scale ziplink_ziplink-web=3
```
