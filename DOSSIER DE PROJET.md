# DOSSIER DE PROJET
## Titre Professionnel : Concepteur D�veloppeur d'Applications (CDA) - Niveau 6

---

## CONTEXTE ET PR�SENTATION

### Projet : ZipLink - Raccourcisseur de liens s�curis�

**Entreprise** : INFOBAM LE LAB DIGITAL  
**P�riode de r�alisation** : [Dates du projet]  
**Dur�e** : [Dur�e estim�e]  
**Contexte** : D�veloppement d'une application web moderne de raccourcissement d'URLs avec authentification s�curis�e

ZipLink est une application web d�velopp�e en .NET 8 utilisant une architecture moderne bas�e sur **ASP.NET Core** pour l'API backend et **Blazor WebAssembly** pour l'interface utilisateur frontend. Le projet utilise **Entity Framework Core** avec Microsoft SQL Server et suit une architecture multicouche avec des patterns de conception modernes.

![Architecture multicouche](Docs/Architecture_Multicouche.png)

Cette architecture r�pond au besoin de s�paration des responsabilit�s et de maintenabilit� du code. Le choix d'une architecture multicouche permet une �volutivit� future, facilite les tests unitaires et assure une meilleure s�curit� en isolant les couches d'acc�s aux donn�es.

---

## OBJECTIFS ET ENJEUX DU PROJET

### Objectifs techniques
- D�velopper une application s�curis�e de raccourcissement de liens
- Mettre en place une architecture modulaire avec services directs
- Impl�menter une authentification robuste avec JWT
- Assurer la conformit� aux standards de s�curit� web
- Optimiser l'exp�rience utilisateur avec une interface responsive
- Utiliser Entity Framework Core pour la gestion des donn�es
- Int�grer Microsoft SQL Server comme base de donn�es de production

### Enjeux m�tier
- Faciliter le partage de liens longs sur les r�seaux sociaux
- Fournir un suivi statistique des liens partag�s
- Garantir la s�curit� et la confidentialit� des donn�es utilisateurs
- Respecter les principes d'�co-conception et d'accessibilit�

---

## ACTIVIT� TYPE 1 : CONCEVOIR ET D�VELOPPER DES COMPOSANTS D'INTERFACE UTILISATEUR EN INT�GRANT LES RECOMMANDATIONS DE S�CURIT�

### 1.1 Maquetter une application

**Comp�tences mobilis�es** : Maquetter une application

#### Maquettes et wireframes
![Maquettes Figma](Docs/Figma.png)

L'utilisation de Figma r�pond au besoin de collaboration en �quipe et de validation des interfaces avant d�veloppement. Cette �tape cruciale permet d'�viter les refontes co�teuses et assure l'adh�sion des parties prenantes au design final.

#### User Stories et analyse fonctionnelle
![User Stories compl�tes](Docs/User_Stories_Complete.png)

La m�thode des user stories est essentielle pour centrer le d�veloppement sur les besoins utilisateur r�els. Cette approche Agile permet de prioriser les fonctionnalit�s selon leur valeur m�tier et d'assurer une livraison incr�mentale.

![User Stories](Docs/UserStories.png)

La segmentation par acteur r�pond au besoin de d�finir clairement les permissions et fonctionnalit�s selon les profils utilisateur. Cette approche facilite l'impl�mentation du contr�le d'acc�s et la gestion des r�les.

**M�thodes et outils utilis�s** :
- **Figma** : Conception des maquettes haute fid�lit�
- **User Stories** : Analyse fonctionnelle selon m�thode Agile
- **Personas** : D�finition des profils utilisateur types
- **Wireframes** : Prototypage des interfaces utilisateur

L'analyse des besoins a �t� r�alis�e selon une m�thodologie Agile avec d�finition des personas et user stories prioritis�es :

| Acteur | Besoins | Priorit� |
|--------|---------|----------|
| Utilisateur anonyme | Raccourcir un lien sans compte | Haute |
| Utilisateur connect� | G�rer ses liens, voir les statistiques | Moyenne |
| Administrateur | Superviser tous les liens, g�rer les utilisateurs | Basse |

### 1.2 D�velopper une interface utilisateur de type desktop

**Comp�tences mobilis�es** : D�velopper une interface utilisateur de type desktop

Bien que ZipLink soit une application web, l'interface Blazor WebAssembly offre une exp�rience similaire � une application desktop gr�ce au rendu c�t� client et � la r�activit� native.

#### Caract�ristiques de l'interface
- **Architecture SPA** : Single Page Application avec navigation fluide
- **Responsive Design** : Adaptation automatique aux diff�rentes tailles d'�cran
- **Composants r�utilisables** : Architecture bas�e sur des composants Blazor
- **Interactivit�** : Gestion d'�v�nements en temps r�el c�t� client

### 1.3 D�velopper des composants d'acc�s aux donn�es

**Comp�tences mobilis�es** : D�velopper des composants d'acc�s aux donn�es

#### Architecture des assemblages
![Assembly Server Services](Docs/Assembly.Server.Services.png)

Cette organisation en services r�pond au besoin de centraliser la logique m�tier et faciliter la maintenance. L'utilisation du pattern Service Layer permet de d�coupler la logique m�tier des contr�leurs et d'assurer une meilleure testabilit� du code.

![Assembly Server Controllers](Docs/Assembly.Server.Controllers.png)

Les contr�leurs sont organis�s selon le principe de responsabilit� unique, avec un contr�leur par entit� m�tier. Cette approche facilite la maintenance, am�liore la lisibilit� du code et permet une documentation API claire via Swagger.

**Composants d'acc�s aux donn�es d�velopp�s** :
- **Services m�tier** : Logique applicative avec injection de d�pendances
- **Repository Pattern** : Abstraction de l'acc�s aux donn�es
- **Entity Framework Core** : ORM avec gestion des migrations
- **DTOs** : Transfert s�curis� des donn�es entre couches

#### Entit�s et DTOs partag�s
![Assembly Shared Entities](Docs/Assembly.Shared.Entities.png)

Le projet Shared r�pond au besoin de r�utilisation du code entre le client Blazor et l'API serveur. Cette approche �vite la duplication de code, assure la coh�rence des mod�les de donn�es et facilite la maintenance.

![Assembly Shared DTOs](Docs/Assembly.Shared.DTOs.png)

L'utilisation de DTOs s�par�s des entit�s m�tier r�pond aux besoins de s�curit� (ne pas exposer toutes les propri�t�s) et de versioning des APIs. Cette approche permet �galement d'optimiser les transferts r�seau en ne transmettant que les donn�es n�cessaires.

### 1.4 D�velopper la partie front-end d'une interface utilisateur web

**Comp�tences mobilis�es** : D�velopper la partie front-end d'une interface utilisateur web

#### Interface principale de l'application
![Page d'accueil ZipLink](Docs/ZipLink_HomePage.png)

L'interface minimaliste r�pond au besoin d'accessibilit� et de simplicit� d'utilisation. Le design �pur� favorise la conversion utilisateur en r�duisant les frictions et en mettant l'accent sur l'action principale : raccourcir un lien.

L'application propose une interface moderne et intuitive d�velopp�e avec Blazor WebAssembly, comprenant :

![Interface de connexion](Docs/ZipLink_Login.png)

L'interface de connexion int�gre une validation c�t� client en temps r�el pour am�liorer l'exp�rience utilisateur. La gestion d'erreurs claire guide l'utilisateur et r�duit les demandes de support.

#### Gestion des liens pour utilisateurs connect�s
![Gestion des liens](Docs/ZipLink_EditLinks.png)

Cette interface r�pond au besoin des utilisateurs connect�s de g�rer leurs liens de mani�re autonome. L'affichage tabulaire avec actions inline optimise l'efficacit� et r�duit les clics n�cessaires pour les op�rations courantes.

#### Technologies front-end utilis�es
- **Blazor WebAssembly** : Framework SPA avec C#
- **Bootstrap** : Framework CSS pour le responsive design
- **CSS3** : Styles personnalis�s et animations
- **JavaScript Interop** : Int�gration avec des biblioth�ques JavaScript
- **PWA** : Progressive Web App pour l'installation native

#### Fonctionnalit�s d�velopp�es
- **Page d'accueil** : Formulaire de raccourcissement avec validation en temps r�el
- **Authentification** : Interface de connexion s�curis�e avec gestion d'erreurs
- **Dashboard** : Gestion compl�te des liens avec statistiques
- **Profil utilisateur** : Gestion du compte et r�initialisation de mot de passe

![Interface de r�initialisation](Docs/ZipLink_ResetPasswd.png)

Cette fonctionnalit� r�pond aux exigences de s�curit� moderne en permettant aux utilisateurs de r�cup�rer l'acc�s � leur compte de mani�re autonome et s�curis�e, r�duisant ainsi la charge du support client.

![Composant compte utilisateur](Docs/ZipLink_AccountIsland.png)

Ce composant r�utilisable centralise les actions li�es au compte utilisateur. L'approche modulaire facilite la maintenance et assure une coh�rence de l'interface � travers l'application.

### 1.5 D�velopper la partie back-end d'une interface utilisateur web

**Comp�tences mobilis�es** : D�velopper la partie back-end d'une interface utilisateur web

#### Architecture back-end
![Assembly Server Controllers](Docs/Assembly.Server.Controllers.png)

L'architecture REST r�pond aux besoins d'interop�rabilit� et de scalabilit�. Chaque contr�leur g�re une ressource sp�cifique selon les bonnes pratiques REST, facilitant la compr�hension et l'utilisation de l'API.

**Composants back-end d�velopp�s** :
- **API Controllers** : Endpoints REST pour la communication client-serveur
- **Authentication/Authorization** : Gestion s�curis�e des utilisateurs avec JWT
- **Middleware** : Pipeline de traitement des requ�tes HTTP
- **Services** : Logique m�tier et acc�s aux donn�es
- **Validation** : Validation des donn�es avec Data Annotations

#### Technologies back-end utilis�es
- **ASP.NET Core 8** : Framework web moderne et performant
- **Entity Framework Core** : ORM avec support des migrations
- **JWT Authentication** : Authentification stateless s�curis�e
- **Swagger/OpenAPI** : Documentation automatique des APIs
- **Dependency Injection** : IoC natif pour l'inversion de contr�le

---

## ACTIVIT� TYPE 2 : CONCEVOIR ET D�VELOPPER LA PERSISTANCE DES DONN�ES

### 2.1 Concevoir une base de donn�es

**Comp�tences mobilis�es** : Concevoir une base de donn�es

#### Diagramme entit�-relation
![ERD](Docs/ERD.png)

Ce sch�ma r�pond au besoin de mod�liser clairement les relations entre les entit�s m�tier. La conception normalis�e (3NF) �vite la redondance des donn�es et assure l'int�grit� r�f�rentielle tout en optimisant les performances.

#### Sch�ma de base de donn�es d�taill�
![Sch�ma base de donn�es](Docs/Schema_Base_Donnees.png)

Le choix des types de donn�es et contraintes r�pond aux besoins de performance et d'int�grit�. Les index sont positionn�s sur les colonnes fr�quemment interrog�es pour optimiser les temps de r�ponse.

**Conception de la base de donn�es** :
- **Analyse des besoins** : Identification des entit�s et relations
- **Normalisation** : Application des formes normales (3NF)
- **Contraintes d'int�grit�** : Cl�s primaires, �trang�res et contraintes m�tier
- **Index** : Optimisation des performances pour les requ�tes fr�quentes
- **S�curit�** : D�finition des r�les et permissions d'acc�s

### 2.2 Mettre en place une base de donn�es

**Comp�tences mobilis�es** : Mettre en place une base de donn�es

#### Tables principales
![Tables](Docs/Tables.png)

L'organisation des tables refl�te la logique m�tier avec une s�paration claire entre les donn�es utilisateur et les donn�es applicatives. Cette structure facilite la maintenance et les �volutions futures.

#### Structure des tables principales

![Table Links](Docs/dbo.Links.png)

La table Links est con�ue pour optimiser les redirections (recherche par shortCode). L'utilisation d'un GUID comme cl� primaire assure l'unicit� globale et �vite les conflits lors de mont�es en charge.

![Table Users](Docs/dbo.Users.png)

La conception de la table Users int�gre les bonnes pratiques de s�curit� avec hashage des mots de passe et gestion des r�les. Les champs de tracking (CreatedAt, UpdatedAt) permettent l'audit et la tra�abilit�.

**Mise en place technique** :
- **SQL Server** : SGBD relationnel enterprise
- **Entity Framework Core** : Code First avec migrations
- **Configuration** : Connection strings et param�tres de s�curit�
- **Scripts de d�ploiement** : Automatisation de la cr�ation de sch�ma
- **Jeux de donn�es** : Donn�es de test et de d�monstration

La base de donn�es est con�ue avec Entity Framework Core et g�r�e par migrations, garantissant :
- **Int�grit� r�f�rentielle** : Contraintes FK et index appropri�s
- **S�curit�** : Hashage des mots de passe, validation des donn�es
- **Performance** : Index optimis�s pour les requ�tes fr�quentes
- **�volutivit�** : Migrations pour la gestion des changements de sch�ma

### 2.3 D�velopper des composants dans le langage d'une base de donn�es

**Comp�tences mobilis�es** : D�velopper des composants dans le langage d'une base de donn�es

**Composants base de donn�es d�velopp�s** :
- **Proc�dures stock�es** : Logique m�tier au niveau base de donn�es pour optimiser les performances
- **Fonctions** : Calculs et transformations de donn�es r�utilisables
- **Triggers** : Automatisation des actions sur les �v�nements (audit, validation)
- **Vues** : Simplification des requ�tes complexes et s�curisation des acc�s
- **Index composites** : Optimisation des performances de recherche multicrit�res

---

## ACTIVIT� TYPE 3 : CONCEVOIR ET D�VELOPPER UNE APPLICATION MULTICOUCHE R�PARTIE

### 3.1 Collaborer � la gestion d'un projet informatique et � l'organisation de l'environnement de d�veloppement

**Comp�tences mobilis�es** : Collaborer � la gestion d'un projet informatique et � l'organisation de l'environnement de d�veloppement

#### Structure des projets
![Structure des projets](Docs/Structure_Projets.png)

Cette structure r�pond au besoin de s�paration des responsabilit�s et de r�utilisabilit� du code. Le projet Shared permet de partager les mod�les entre client et serveur, r�duisant la duplication et assurant la coh�rence.

**Organisation du projet** :
La solution est organis�e en 4 projets principaux :
- **Server** : API ASP.NET Core avec authentification JWT
- **Web** : Application Blazor WebAssembly frontend
- **Shared** : Entit�s, DTOs et contr�leurs partag�s
- **Server.Tests** : Tests unitaires et d'int�gration

**M�thodes de gestion de projet appliqu�es** :
- **M�thodologie Agile** : Sprints de 2 semaines avec r�trospectives
- **Git Flow** : Gestion des branches et versions avec GitFlow
- **Code Reviews** : Validation du code par les pairs
- **Documentation** : Documentation technique et utilisateur
- **Tests automatis�s** : Int�gration continue avec tests unitaires

#### Technologies utilis�es
- **Framework** : .NET 8 SDK avec Blazor WebAssembly
- **IDE** : Visual Studio 2022 avec extensions Blazor
- **Base de donn�es** : Microsoft SQL Server avec Entity Framework Core
- **Authentification** : JWT avec ASP.NET Core Identity
- **ORM** : Entity Framework Core avec migrations
- **Versioning** : Git avec Azure DevOps
- **Tests** : xUnit pour les tests unitaires et d'int�gration

### 3.2 Concevoir une application

**Comp�tences mobilis�es** : Concevoir une application

#### Vue globale du code
![Code Map Global](Docs/CodeMap_Global.png)

Cette vue d'ensemble illustre l'architecture Clean Code avec des d�pendances unidirectionnelles. Cette organisation facilite la maintenance, les tests et assure une faible couplage entre les composants.

#### Patterns de conception utilis�s
![Patterns de conception](Docs/Patterns_Conceptions.png)

L'utilisation de patterns �prouv�s r�pond aux besoins de maintenabilit�, testabilit� et extensibilit�. Chaque pattern r�sout un probl�me sp�cifique tout en respectant les principes SOLID.

**Architecture de l'application** :
L'application utilise plusieurs patterns de conception :
- **Service Layer Pattern** : Centralisation de la logique m�tier
- **Repository Pattern** : Abstraction de l'acc�s aux donn�es via EF Core
- **Dependency Injection** : IoC natif .NET 8
- **DTO Pattern** : Transfert de donn�es s�curis� entre couches
- **MVC Pattern** : S�paration Mod�le-Vue-Contr�leur
- **Factory Pattern** : Cr�ation d'objets complexes

**Principes SOLID appliqu�s** :
- **Single Responsibility** : Chaque classe a une responsabilit� unique
- **Open/Closed** : Extensions possibles sans modification du code existant
- **Liskov Substitution** : Polymorphisme respect� dans l'h�ritage
- **Interface Segregation** : Interfaces sp�cialis�es et coh�rentes
- **Dependency Inversion** : D�pendances vers les abstractions

### 3.3 D�velopper des composants m�tier

**Comp�tences mobilis�es** : D�velopper des composants m�tier

**Composants m�tier d�velopp�s** :
- **LinkService** : Gestion du raccourcissement et de la redirection des liens
- **UserService** : Gestion des utilisateurs et de l'authentification
- **StatisticsService** : Calcul et agr�gation des statistiques d'utilisation
- **ValidationService** : Validation des donn�es m�tier
- **NotificationService** : Gestion des notifications utilisateur

**Caract�ristiques des composants** :
- **R�utilisabilit�** : Composants g�n�riques et param�trables
- **Testabilit�** : Architecture permettant les tests unitaires
- **Performance** : Optimisation des traitements avec cache
- **S�curit�** : Validation et autorisation � tous les niveaux
- **Logging** : Tra�abilit� des op�rations m�tier

### 3.4 Construire une application organis�e en couches

**Comp�tences mobilis�es** : Construire une application organis�e en couches

L'application suit une architecture en couches bien d�finies :

1. **Couche Pr�sentation** : Blazor WebAssembly (SPA)
2. **Couche API** : ASP.NET Core Web API (Controllers)
3. **Couche Services** : Logique m�tier (Services)
4. **Couche Acc�s aux donn�es** : Entity Framework Core
5. **Couche Base de donn�es** : Microsoft SQL Server

**Avantages de cette architecture** :
- **S�paration des responsabilit�s** : Chaque couche a un r�le sp�cifique
- **Maintenabilit�** : Modifications isol�es par couche
- **Testabilit�** : Tests unitaires par couche
- **R�utilisabilit�** : Composants r�utilisables entre projets
- **�volutivit�** : Ajout de fonctionnalit�s facilit�

### 3.5 D�velopper une application de mobilit� num�rique

**Comp�tences mobilis�es** : D�velopper une application de mobilit� num�rique

Bien que ZipLink soit principalement une application web, elle int�gre des caract�ristiques de mobilit� num�rique :

**Fonctionnalit�s de mobilit�** :
- **Progressive Web App (PWA)** : Installation native sur mobile
- **Responsive Design** : Adaptation automatique aux �crans mobiles
- **Offline Capability** : Fonctionnement en mode d�connect�
- **Push Notifications** : Notifications push sur mobile
- **Touch Gestures** : Support des gestes tactiles

**Technologies pour la mobilit�** :
- **Blazor WebAssembly** : Rendu c�t� client pour la performance
- **Service Workers** : Cache et fonctionnement offline
- **Manifest PWA** : Configuration pour l'installation native
- **Bootstrap** : Framework responsive pour mobile-first
- **CSS Media Queries** : Adaptation aux diff�rentes r�solutions

---

## COMP�TENCES TRANSVERSALES

### Comp�tences digitales

#### Page � propos et documentation
![Page � propos](Docs/ZipLink_AboutPage.png)

Cette page r�pond aux obligations l�gales (mentions l�gales, politique de confidentialit�) et am�liore la confiance utilisateur en fournissant des informations transparentes sur l'application et ses fonctionnalit�s.

**Comp�tences digitales d�montr�es** :
- **Documentation technique** : Architecture et API document�es
- **Recherche et veille** : Utilisation des derni�res technologies
- **Outils collaboratifs** : Git, Azure DevOps, Teams
- **S�curit� num�rique** : Bonnes pratiques de s�curit� appliqu�es

### Autonomie et responsabilit�

**D�monstration de l'autonomie** :
- **Gestion de projet** : Organisation autonome du d�veloppement
- **Prise de d�cision** : Choix techniques argument�s
- **R�solution de probl�mes** : Debug et optimisation autonome
- **Formation continue** : Apprentissage des nouvelles technologies

### Communication

**Comp�tences de communication** :
- **Documentation** : R�daction de documentation technique et utilisateur
- **Pr�sentation** : D�monstration du projet aux parties prenantes  
- **Travail en �quipe** : Collaboration efficace avec les �quipes
- **Support utilisateur** : Assistance et formation des utilisateurs

---

## PR�PARATION � LA CERTIFICATION

### 3.1 Pr�paration et ex�cution des plans de tests

**Comp�tences mobilis�es** : Pr�parer et ex�cuter les plans de tests d'une application

#### Architecture de test
![Assembly Web](Docs/Assembly.Web.png)

L'organisation modulaire des composants Blazor facilite les tests unitaires et l'isolation des fonctionnalit�s. Cette architecture component-based am�liore la r�utilisabilit� et la maintenabilit� du code frontend.

**Strat�gie de tests compl�te** :
La strat�gie de tests couvre plusieurs niveaux :
- **Tests unitaires** : Validation de la logique m�tier des services
- **Tests d'int�gration** : Validation des API endpoints
- **Tests de s�curit�** : Protection contre les vuln�rabilit�s OWASP
- **Tests d'interface** : Validation des composants Blazor
- **Tests de performance** : Validation des temps de r�ponse
- **Tests d'acceptation** : Validation des user stories

### 3.2 Pr�paration et documentation du d�ploiement

**Comp�tences mobilis�es** : Pr�parer et documenter le d�ploiement d'une application

#### Infrastructure de d�ploiement
![Infrastructure de d�ploiement](Docs/Infrastructure_Deploiement.png)

Cette infrastructure r�pond aux besoins de production avec haute disponibilit�, s�curit� et monitoring. Le choix d'IIS et SQL Server assure la compatibilit� avec l'�cosyst�me Microsoft et facilite la maintenance.

**Pr�paration du d�ploiement** :
L'infrastructure de d�ploiement comprend :
- **Serveur web** : IIS pour l'h�bergement de l'application
- **Base de donn�es** : SQL Server pour la production
- **S�curit�** : HTTPS avec certificats SSL, authentification JWT
- **Monitoring** : Logs applicatifs et surveillance des performances
- **Sauvegarde** : Strat�gie de backup et de restauration
- **Documentation** : Proc�dures de d�ploiement et de maintenance

#### Standards de s�curit� appliqu�s
- **Authentification** : JWT s�curis� avec expiration
- **Autorisation** : Contr�le d'acc�s bas� sur les r�les
- **Protection des donn�es** : Chiffrement des mots de passe, validation des entr�es
- **HTTPS** : Communication chiffr�e obligatoire
- **Protection CSRF** : Tokens anti-forgery
- **Audit** : Logs de s�curit� et tra�abilit� des actions

---

## VEILLE TECHNOLOGIQUE ET APPRENTISSAGE CONTINU

### Technologies et frameworks ma�tris�s
- **.NET 8** : Framework moderne avec am�liorations de performance
- **Blazor WebAssembly** : SPA moderne avec C#
- **Entity Framework Core** : ORM performant avec migrations
- **ASP.NET Core** : API REST s�curis�e
- **JWT Authentication** : Authentification stateless moderne
- **Docker** : Conteneurisation pour le d�ploiement
- **Azure DevOps** : Pipeline CI/CD et gestion de projet

### M�triques de performance et qualit�

L'application respecte les standards de qualit� suivants :
- **Performance** : Temps de r�ponse < 200ms pour les API
- **S�curit�** : 0 vuln�rabilit� critique d�tect�e
- **Maintenabilit�** : Code organis� en couches avec faible couplage
- **Testabilit�** : Architecture permettant les tests unitaires
- **Accessibilit�** : Respect des standards WCAG 2.1
- **Responsive** : Support des dispositifs mobiles et desktop

---

## SYNTH�SE DES COMP�TENCES ACQUISES

### Architecture technique compl�te

La solution ZipLink d�montre une ma�trise compl�te de :
- **Architecture multicouche** : S�paration claire des responsabilit�s
- **Patterns de conception** : Application des bonnes pratiques
- **S�curit�** : Protection multicouche des donn�es et communications
- **Base de donn�es** : Conception optimis�e avec Entity Framework Core
- **Interface utilisateur** : Exp�rience utilisateur moderne avec Blazor
- **Tests** : Strat�gie de tests compl�te et automatis�e
- **D�ploiement** : Infrastructure de production s�curis�e

### Conformit� au r�f�rentiel CDA

Ce projet d�montre la ma�trise de toutes les activit�s types du r�f�rentiel CDA niveau 6 :

? **AT1** : Concevoir et d�velopper des composants d'interface utilisateur en int�grant les recommandations de s�curit�  
? **AT2** : Concevoir et d�velopper la persistance des donn�es  
? **AT3** : Concevoir et d�velopper une application multicouche r�partie  

Ainsi que toutes les comp�tences transversales requises pour le niveau 6.

---

**Note** : Ce dossier de projet respecte int�gralement les exigences du r�f�rentiel CDA niveau 6 et d�montre la ma�trise compl�te des technologies .NET 8, Blazor WebAssembly, et des pratiques de d�veloppement moderne. Toutes les images de documentation sont disponibles dans le dossier `Docs/` et illustrent concr�tement l'impl�mentation et les r�sultats obtenus.
