# Plan de travail Codex

## Objectif du backlog

Découper le développement en petites tâches permettant d’obtenir rapidement une V1 exécutable, puis d’ajouter les fonctionnalités de façon contrôlée.

Chaque tâche doit produire un résultat testable.

## Phase 0 — Initialisation

### Tâche 0.1 — Créer la solution

Objectif :

- créer la solution .NET avec la structure prévue.

Fichiers probables :

- `src/HusqvarnaAutomowerConnect.sln`
- `src/HusqvarnaAutomowerConnect.App`
- `src/HusqvarnaAutomowerConnect.Core`
- `src/HusqvarnaAutomowerConnect.Infrastructure`
- `src/HusqvarnaAutomowerConnect.Tests`

Critères d’acceptation :

- la solution compile ;
- les projets sont référencés correctement ;
- `HusqvarnaAutomowerConnect.App` référence `Core` et `Infrastructure` ;
- `Infrastructure` référence `Core` ;
- `Tests` référence `Core` et `Infrastructure`.

Tests attendus :

- test vide ou test de santé initial exécuté par `dotnet test`.

### Tâche 0.2 — Ajouter configuration qualité

Objectif :

- ajouter les bases de qualité de code.

Fichiers probables :

- `.editorconfig`
- `.gitignore`
- fichiers projet `.csproj`
- `README.md`

Critères d’acceptation :

- nullable activé ;
- warnings utiles activés ;
- `.gitignore` protège les secrets ;
- `dotnet format` peut être utilisé.

Tests attendus :

- build sans erreur.

## Phase 1 — Modèle et services Core

### Tâche 1.1 — Créer les modèles internes

Objectif :

- créer les entités décrites dans `docs/DATA_MODEL.md`.

Fichiers probables :

- `Mower.cs`
- `MowerStatus.cs`
- `BatteryInfo.cs`
- `MowerError.cs`
- `MowerLocation.cs`
- `MowerSchedule.cs`
- `CommandResult.cs`
- `AuthSession.cs`

Critères d’acceptation :

- modèles dans `HusqvarnaAutomowerConnect.Core` ;
- nullabilité explicite ;
- pas de dépendance UI ;
- enums avec valeur `Unknown`.

Tests attendus :

- tests de validation simples ;
- tests batterie hors plage si validation implémentée.

### Tâche 1.2 — Définir les interfaces Core

Objectif :

- isoler les dépendances externes.

Fichiers probables :

- `IHusqvarnaApiClient.cs`
- `IHusqvarnaAuthClient.cs`
- `ISecureTokenStore.cs`
- `IMowerService.cs`
- `IAppSettingsStore.cs`

Critères d’acceptation :

- interfaces async ;
- `CancellationToken` présent ;
- aucune référence WinUI ;
- aucune référence HTTP brute dans les interfaces exposées à l’UI.

Tests attendus :

- build.

### Tâche 1.3 — Service applicatif Mower

Objectif :

- créer `MowerService` pour orchestrer récupération robots et commandes.

Fichiers probables :

- `MowerService.cs`
- `CommandAvailabilityService.cs`

Critères d’acceptation :

- le service appelle les interfaces ;
- les erreurs sont retournées de façon typée ;
- commandes indisponibles gérées sans crash.

Tests attendus :

- aucun robot ;
- robot avec données partielles ;
- commande indisponible ;
- erreur API propagée proprement.

## Phase 2 — Configuration et sécurité

### Tâche 2.1 — Configuration locale

Objectif :

- charger les paramètres non sensibles.

Fichiers probables :

- `AppSettings.cs`
- `LocalSettingsStore.cs`
- `appsettings.example.json`

Critères d’acceptation :

- exemple sans secret réel ;
- fichier local ignoré ;
- validation Application Key ;
- validation intervalle refresh.

Tests attendus :

- configuration valide ;
- configuration absente ;
- intervalle invalide.

### Tâche 2.2 — Stockage sécurisé des tokens

Objectif :

- implémenter `ISecureTokenStore`.

Fichiers probables :

- `SecureTokenStore.cs`
- tests associés.

Critères d’acceptation :

- tokens stockés via mécanisme Windows sécurisé ;
- suppression fonctionnelle ;
- aucune écriture en clair dans un fichier projet ;
- erreurs gérées.

Tests attendus :

- sauvegarde ;
- chargement ;
- suppression ;
- token absent ;
- stockage indisponible mocké.

### Tâche 2.3 — Masquage des logs

Objectif :

- éviter toute fuite dans les logs.

Fichiers probables :

- `SensitiveDataRedactor.cs`
- configuration logging.

Critères d’acceptation :

- masquage des tokens ;
- masquage du header Authorization ;
- debug désactivé par défaut.

Tests attendus :

- `access_token` masqué ;
- `refresh_token` masqué ;
- `client_secret` masqué ;
- `Authorization` masqué.

## Phase 3 — Authentification

### Tâche 3.1 — Vérifier la documentation OAuth Husqvarna

Objectif :

- confirmer le flux OAuth2 exact avant codage.

Fichiers probables :

- `docs/HUSQVARNA_API.md`
- `docs/DECISIONS.md`

Critères d’acceptation :

- endpoints officiels confirmés ;
- headers confirmés ;
- redirect URI confirmé ;
- nécessité PKCE confirmée ou infirmée ;
- aucune hypothèse non documentée.

Tests attendus :

- aucun test applicatif requis, mais décision documentée.

### Tâche 3.2 — Implémenter client OAuth

Objectif :

- gérer connexion, refresh et déconnexion.

Fichiers probables :

- `HusqvarnaAuthClient.cs`
- `AuthService.cs`
- DTO OAuth.

Critères d’acceptation :

- construction URL d’autorisation ;
- échange code/token ;
- refresh ;
- suppression session ;
- aucune fuite logs.

Tests attendus :

- échange token mocké ;
- refresh réussi ;
- refresh échoué ;
- token expiré ;
- réponse invalide.

## Phase 4 — API Husqvarna

### Tâche 4.1 — Vérifier endpoints Automower officiels

Objectif :

- confirmer endpoints, headers et payloads.

Fichiers probables :

- `docs/HUSQVARNA_API.md`
- `docs/DECISIONS.md`

Critères d’acceptation :

- `GET /mowers` confirmé ;
- `GET /mowers/{id}` confirmé ;
- `POST /mowers/{id}/actions` confirmé ;
- ressources calendrier confirmées ;
- payloads d’actions documentés ou explicitement laissés non implémentés.

Tests attendus :

- aucun test applicatif requis, mais exemples mockés préparés ensuite.

### Tâche 4.2 — Implémenter récupération des robots

Objectif :

- récupérer et mapper les robots.

Fichiers probables :

- `HusqvarnaApiClient.cs`
- DTO mowers ;
- `MowerMapper.cs`.

Critères d’acceptation :

- appel HTTP via `HttpClient` injecté ;
- headers officiels ;
- mapping vers modèles internes ;
- données absentes gérées.

Tests attendus :

- liste avec un robot ;
- liste vide ;
- données partielles ;
- JSON avec valeur inconnue ;
- erreur 401/403/500.

### Tâche 4.3 — Implémenter commandes principales

Objectif :

- envoyer les commandes V1 confirmées.

Fichiers probables :

- `MowerCommandMapper.cs`
- `HusqvarnaApiClient.cs`
- tests commandes.

Critères d’acceptation :

- seules les commandes confirmées sont codées ;
- commandes indisponibles bloquées ;
- réponse `202` traitée comme acceptation ;
- erreurs mappées.

Tests attendus :

- pause acceptée ;
- retour station accepté ;
- reprise planning acceptée ;
- tonte temporaire avec durée ;
- commande non supportée ;
- 400/403/429/503.

## Phase 5 — UI V1

### Tâche 5.1 — Shell et navigation

Objectif :

- créer l’ossature WinUI.

Fichiers probables :

- `MainWindow`
- `App.xaml`
- `NavigationService`
- pages principales.

Critères d’acceptation :

- application démarre ;
- navigation vers connexion, tableau de bord, paramètres ;
- libellés français.

Tests attendus :

- tests ViewModel si possible ;
- test manuel démarrage.

### Tâche 5.2 — Écran paramètres

Objectif :

- gérer la configuration.

Fichiers probables :

- `SettingsPage`
- `SettingsViewModel`

Critères d’acceptation :

- Application Key saisissable ;
- intervalle configurable ;
- validation visible ;
- sauvegarde locale.

Tests attendus :

- validation Application Key absente ;
- intervalle invalide ;
- sauvegarde appelée.

### Tâche 5.3 — Écran connexion

Objectif :

- permettre connexion/déconnexion.

Fichiers probables :

- `LoginPage`
- `LoginViewModel`

Critères d’acceptation :

- bouton connexion ;
- état connecté/non connecté ;
- déconnexion supprime session ;
- messages d’erreur en français.

Tests attendus :

- connexion réussie mockée ;
- connexion échouée ;
- déconnexion.

### Tâche 5.4 — Tableau de bord

Objectif :

- afficher la liste des robots.

Fichiers probables :

- `DashboardPage`
- `DashboardViewModel`
- composants carte robot.

Critères d’acceptation :

- robots affichés ;
- état vide ;
- chargement ;
- rafraîchissement manuel ;
- dernière mise à jour.

Tests attendus :

- liste vide ;
- robot avec données complètes ;
- robot avec données partielles ;
- erreur réseau.

### Tâche 5.5 — Commandes robot

Objectif :

- ajouter les boutons d’action.

Fichiers probables :

- `MowerDetailsPage`
- `MowerDetailsViewModel`

Critères d’acceptation :

- boutons en français ;
- boutons désactivés si indisponibles ;
- feedback après commande ;
- rafraîchissement après commande.

Tests attendus :

- commande disponible ;
- commande indisponible ;
- commande acceptée ;
- commande échouée.

## Phase 6 — Stabilisation V1

### Tâche 6.1 — Tests d’intégration API mockée

Objectif :

- vérifier les flux complets sans API réelle.

Fichiers probables :

- tests d’intégration.

Critères d’acceptation :

- scénario connexion + liste robots ;
- scénario 401 + refresh ;
- scénario 429 ;
- scénario aucun robot.

Tests attendus :

- tous les scénarios listés.

### Tâche 6.2 — Tests manuels Windows

Objectif :

- valider l’application sur Windows 11.

Fichiers probables :

- `docs/TEST_PLAN.md`

Critères d’acceptation :

- checklist manuelle exécutée ;
- résultats documentés ;
- bugs critiques corrigés.

Tests attendus :

- lancement ;
- configuration ;
- connexion ;
- tableau de bord ;
- commandes si robot disponible ;
- déconnexion.

### Tâche 6.3 — Préparation release interne

Objectif :

- préparer une V1 utilisable localement.

Fichiers probables :

- `CHANGELOG.md`
- `README.md`
- `docs/SETUP.md`

Critères d’acceptation :

- documentation à jour ;
- changelog mis à jour ;
- sécurité vérifiée ;
- build Release OK.

Tests attendus :

- `dotnet test` ;
- checklist sécurité ;
- checklist manuelle.

## V1 exécutable rapidement

Pour obtenir une V1 le plus tôt possible, suivre cet ordre minimal :

1. solution et projets ;
2. modèles Core ;
3. configuration locale ;
4. stockage sécurisé ;
5. auth OAuth ;
6. client `GET /mowers` ;
7. tableau de bord ;
8. déconnexion ;
9. commandes principales ;
10. logs et tests d’erreur.

Ne pas commencer les fonctionnalités V1.1 avant une V1 stable.
