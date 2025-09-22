# Flux GitOps Configuration for ZipLink

Ce répertoire contient les configurations Flux CD pour le déploiement automatique de ZipLink sur Kubernetes.

## Structure

```
flux/
├── clusters/
│   ├── development/         # Configuration pour l'environnement de développement
│   │   └── ziplink-sync.yaml
│   └── production/          # Configuration pour l'environnement de production
│       └── ziplink-sync.yaml
└── README.md
```

## Installation de Flux

### Prérequis

- Cluster Kubernetes (K3s recommandé)
- kubectl configuré
- Accès au repository Git

### 1. Installation de Flux CLI

```bash
curl -s https://fluxcd.io/install.sh | sudo bash
```

### 2. Vérification des prérequis

```bash
flux check --pre
```

### 3. Bootstrap Flux sur le cluster

```bash
# Pour l'environnement de développement
flux bootstrap git \
  --url=https://github.com/powerm1nt/ZipLink.git \
  --branch=master \
  --path=flux/clusters/development \
  --username=powerm1nt \
  --token-auth

# Pour l'environnement de production
flux bootstrap git \
  --url=https://github.com/powerm1nt/ZipLink.git \
  --branch=master \
  --path=flux/clusters/production \
  --username=powerm1nt \
  --token-auth
```

### 4. Vérification du déploiement

```bash
# Vérifier les sources
flux get sources git

# Vérifier les kustomizations
flux get kustomizations

# Surveiller les déploiements
flux logs --follow --tail=10
```

## Configuration des environnements

### Développement

- Synchronisation : toutes les 1 minute
- Branche : master
- Path : k8s/overlays/development
- Health checks activés

### Production

- Synchronisation : toutes les 30 minutes
- Branche : master
- Path : k8s/overlays/production
- Health checks activés avec retry

## Utilisation

Une fois Flux configuré, les déploiements se font automatiquement :

1. **Push vers Git** : Modifiez les manifestes Kubernetes et poussez vers master
2. **Détection automatique** : Flux détecte les changements
3. **Déploiement** : Les ressources sont automatiquement déployées sur le cluster
4. **Health checks** : Flux vérifie la santé des déploiements

## Commandes utiles

```bash
# Forcer une synchronisation
flux reconcile source git ziplink-repo

# Vérifier l'état des déploiements
flux get all

# Suspendre/reprendre une synchronisation
flux suspend kustomization ziplink-development
flux resume kustomization ziplink-development

# Voir les logs
flux logs --level=info
```

## Troubleshooting

```bash
# Vérifier les événements Flux
kubectl get events -n flux-system

# Examiner les resources Flux
kubectl describe gitrepository ziplink-repo -n flux-system
kubectl describe kustomization ziplink-development -n flux-system
```
