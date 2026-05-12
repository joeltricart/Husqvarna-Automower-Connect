# Projet — Husqvarna Automower Connect

## Problème à résoudre

L’utilisateur possède un robot-tondeuse Husqvarna Automower et souhaite le superviser depuis un PC Windows 11 sans dépendre uniquement de l’application mobile Automower Connect.

Le besoin principal est de disposer d’une application locale simple, lisible et fiable pour :

- connecter un compte Husqvarna Automower Connect ;
- afficher l’état du ou des robots associés ;
- lancer les commandes principales disponibles via l’API officielle ;
- diagnostiquer les erreurs courantes sans exposer de secrets.

## Objectif du projet

Créer une application Windows 11 nommée `Husqvarna Automower Connect`, destinée à un usage personnel/local, avec une base technique propre et maintenable.

L’application doit être conçue pour une V1 simple, stable et extensible.

## Type d’application

Application desktop Windows 11 moderne.

Stack cible :

- C# ;
- .NET 8 ou supérieur ;
- WinUI 3 / Windows App SDK ;
- architecture MVVM ;
- client HTTP isolé pour l’API Husqvarna ;
- stockage sécurisé local des tokens.

## Utilisateurs cibles

### Utilisateur principal

Particulier ayant un robot-tondeuse Husqvarna compatible Automower Connect.

Besoins :

- voir rapidement l’état du robot ;
- lancer une commande sans ouvrir l’application mobile ;
- comprendre les erreurs ;
- éviter toute configuration technique lourde après l’installation initiale.

### Développeur / mainteneur

Personne qui fera évoluer l’application avec Codex.

Besoins :

- documentation claire ;
- architecture simple ;
- tests présents dès le départ ;
- séparation nette entre UI, logique métier et intégration API.

## Cas d’usage principaux

1. Configurer l’accès à l’API Husqvarna.
2. Se connecter au compte Husqvarna via OAuth2.
3. Lister les robots associés au compte.
4. Consulter l’état d’un robot :
   - nom ;
   - état courant ;
   - batterie ;
   - mode ;
   - activité ;
   - erreur éventuelle ;
   - dernière mise à jour ;
   - position GPS si disponible.
5. Envoyer une commande supportée :
   - pause ;
   - retour station ;
   - reprise du planning ;
   - tonte temporaire si disponible.
6. Afficher les erreurs importantes.
7. Consulter les logs locaux sans fuite de secrets.

## Périmètre V1 explicite

La V1 doit permettre :

- une authentification OAuth2 fonctionnelle ;
- le stockage sécurisé des tokens ;
- la récupération de la liste des robots ;
- l’affichage d’un tableau de bord minimal ;
- l’envoi des commandes principales confirmées dans la documentation officielle ;
- la gestion propre des erreurs API courantes ;
- une configuration locale non versionnée ;
- des tests unitaires sur la logique métier et le mapping API ;
- des tests d’intégration avec API mockée.

## Hors périmètre V1 explicite

Ne pas inclure en V1 :

- application mobile ;
- contrôle Bluetooth direct ;
- modification complète du calendrier de tonte ;
- gestion avancée des zones de travail ;
- cartographie avancée ;
- multi-utilisateur ;
- synchronisation cloud propre à l’application ;
- télémétrie externe ;
- auto-update ;
- publication Microsoft Store ;
- support macOS/Linux ;
- contournement des limites ou permissions de l’API Husqvarna.

## Critères de réussite

La V1 est considérée réussie si :

- l’application démarre sur Windows 11 ;
- l’utilisateur peut configurer son accès Husqvarna sans secret dans le dépôt ;
- l’utilisateur peut se connecter et se déconnecter proprement ;
- au moins un robot associé est affiché correctement quand l’API le retourne ;
- l’absence de robot est affichée clairement ;
- les commandes indisponibles sont désactivées ou expliquées ;
- les erreurs 401, 403, 429, 500 et réseau sont gérées sans crash ;
- aucun token, secret ou identifiant sensible n’est écrit dans les logs ;
- les libellés visibles sont en français ;
- les tests de base passent via `dotnet test`.
