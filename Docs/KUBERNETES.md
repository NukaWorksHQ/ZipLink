# ZipLink - Déploiement Kubernetes (K3s)

Cette documentation décrit la migration de ZipLink de Docker Compose vers Kubernetes (K3s) avec Kustomize et préparation pour Flux GitOps.

## 🏗️ Architecture

La nouvelle architecture K3s comprend :

- **Base de données** : PostgreSQL (migration depuis MSSQL)
- **Backend** : API ASP.NET Core 8
- **Frontend** : Blazor WebAssembly
- **Ingress** : NGINX Ingress Controller
- **Monitoring** : Prometheus
- **Orchestration** : Kustomize pour la gestion des environnements

## 📁 Structure des Manifestes

```
k8s/
├── base/                           # Manifestes de base
│   ├── namespace.yaml             # Namespace ziplink
│   ├── configmap.yaml             # Configuration partagée
│   ├── secrets.yaml               # Secrets (JWT, BDD)
│   ├── postgres-pvc.yaml          # Stockage PostgreSQL
│   ├── postgres.yaml              # Déploiement PostgreSQL
│   ├── server.yaml                # API Backend
│   ├── web.yaml                   # Frontend Blazor
│   ├── ingress.yaml               # Ingress NGINX
│   ├── monitoring.yaml            # Prometheus
│   └── kustomization.yaml         # Configuration Kustomize base
├── overlays/
│   ├── development/               # Environnement de développement
│   │   ├── kustomization.yaml
│   │   └── patches/               # Patches spécifiques dev
│   └── production/                # Environnement de production
│       ├── kustomization.yaml
│       └── patches/               # Patches spécifiques prod
└── flux/                          # Configuration Flux GitOps
    ├── clusters/
    ├── apps/
    └── infrastructure/
```

## 🚀 Déploiement

### Prérequis

1. **K3s installé et fonctionnel**

   ```bash
   curl -sfL https://get.k3s.io | sh -
   ```

2. **kubectl configuré**

   ```bash
   sudo cp /etc/rancher/k3s/k3s.yaml ~/.kube/config
   sudo chown $USER ~/.kube/config
   ```

3. **Kustomize installé**
   ```bash
   curl -s "https://raw.githubusercontent.com/kubernetes-sigs/kustomize/master/hack/install_kustomize.sh" | bash
   sudo mv kustomize /usr/local/bin/
   ```

### Déploiement Simple

```bash
# Déploiement en développement (défaut)
./ops-k3s.sh deploy

# Déploiement en production
ENVIRONMENT=production ./ops-k3s.sh deploy
```

### Commandes Disponibles

```bash
# Déployer l'application
./ops-k3s.sh deploy

# Vérifier le statut
./ops-k3s.sh status

# Voir les logs
./ops-k3s.sh logs server      # Logs de l'API
./ops-k3s.sh logs web         # Logs du frontend
./ops-k3s.sh logs postgres    # Logs PostgreSQL
./ops-k3s.sh logs all         # Tous les logs

# Redémarrer des services
./ops-k3s.sh restart server   # Redémarrer l'API
./ops-k3s.sh restart all      # Redémarrer tout

# Supprimer l'application
./ops-k3s.sh destroy

# Construire les images
./ops-k3s.sh build

# Aide
./ops-k3s.sh help
```

## 🔧 Configuration

### Environnements

#### Développement

- 1 réplique pour chaque service
- Ressources limitées
- Base de données `ziplink_dev`
- Domaine `ziplink-dev.local`

#### Production

- 3 répliques pour l'API
- 2 répliques pour le frontend
- Ressources augmentées
- Certificats SSL avec cert-manager
- Domaine `ziplink.production.com`

### Variables d'Environnement

| Variable      | Description                  | Défaut        |
| ------------- | ---------------------------- | ------------- |
| `ENVIRONMENT` | Environnement de déploiement | `development` |

### Secrets

Les secrets sont encodés en base64 dans `k8s/base/secrets.yaml` :

```bash
# JWT Key (exemple)
echo -n "your-jwt-key" | base64

# PostgreSQL Connection String
echo -n "Host=ziplink-postgres-service;Database=ziplink;Username=ziplink_user;Password=ziplink_password;Port=5432" | base64
```

## 🗄️ Migration de Base de Données

### De MSSQL vers PostgreSQL

1. **Packages NuGet modifiés** :

   - Supprimé : `Microsoft.EntityFrameworkCore.SqlServer`
   - Ajouté : `Npgsql.EntityFrameworkCore.PostgreSQL`

2. **Configuration modifiée** :

   - `AppDbContext.cs` : Remplacement de `GETUTCDATE()` par `NOW()`
   - `appsettings.json` : Nouvelle chaîne de connexion PostgreSQL

3. **Nouvelles migrations requises** :
   ```bash
   dotnet ef migrations add PostgreSQLMigration
   ```

## 🌐 Accès aux Services

### Développement

- **Application** : http://ziplink-dev.local
- **API** : http://ziplink-dev.local/api
- **Prometheus** : http://ziplink-dev.local/monitoring

### Production

- **Application** : https://ziplink.production.com
- **API** : https://ziplink.production.com/api

## 📊 Monitoring

Prometheus est déployé automatiquement et collecte les métriques de :

- API Backend (`/metrics`)
- Frontend Blazor (`/metrics`)
- PostgreSQL
- NGINX Ingress

## 🔄 Flux GitOps (Préparation)

La structure Flux est préparée dans `k8s/flux/` pour le déploiement automatique :

```bash
# Installation Flux (après déploiement manuel)
flux bootstrap github \
  --owner=powerm1nt \
  --repository=ZipLink \
  --branch=master \
  --path=k8s/flux/clusters/production
```

## 🛠️ Développement

### Construire les Images Localement

```bash
# Construire et importer dans K3s
./ops-k3s.sh build
```

### Tests de Déploiement

```bash
# Tester la configuration Kustomize
kustomize build k8s/overlays/development

# Dry-run du déploiement
kustomize build k8s/overlays/development | kubectl apply --dry-run=client -f -
```

## 🔍 Troubleshooting

### Vérifier l'État du Cluster

```bash
kubectl get nodes
kubectl get pods --all-namespaces
kubectl get events --sort-by=.metadata.creationTimestamp
```

### Logs Détaillés

```bash
# Logs de tous les pods
kubectl logs -f deployment/ziplink-server -n ziplink

# Décrire un pod problématique
kubectl describe pod <pod-name> -n ziplink
```

### Problèmes Courants

1. **Images non trouvées** : Vérifier que `./ops-k3s.sh build` a été exécuté
2. **PostgreSQL ne démarre pas** : Vérifier les PVC et les permissions
3. **Ingress ne fonctionne pas** : Vérifier l'installation de NGINX Ingress Controller

## 📝 Notes de Migration

### Différences avec Docker Compose

| Docker Compose                | Kubernetes             |
| ----------------------------- | ---------------------- |
| `docker-compose up`           | `./ops-k3s.sh deploy`  |
| `docker-compose logs`         | `./ops-k3s.sh logs`    |
| `docker-compose down`         | `./ops-k3s.sh destroy` |
| Variables d'environnement     | ConfigMaps + Secrets   |
| Volumes nommés                | PersistentVolumeClaims |
| Service discovery automatique | Services Kubernetes    |

### Avantages de K3s

- ✅ Scaling automatique
- ✅ Health checks natifs
- ✅ Rolling updates
- ✅ Configuration déclarative
- ✅ Intégration GitOps
- ✅ Monitoring intégré
- ✅ Sécurité renforcée
