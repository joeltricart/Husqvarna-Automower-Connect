# Architecture technique

## Vue d’ensemble

`Husqvarna Automower Connect` est une application Windows 11 WinUI 3 structurée en couches.

Objectifs d’architecture :

- isoler l’interface de l’API Husqvarna ;
- rendre la logique métier testable ;
- centraliser l’authentification ;
- sécuriser le stockage des tokens ;
- préparer les évolutions sans complexifier la V1.

## Choix WinUI 3 / .NET

### Décision

Utiliser :

- C# ;
- .NET 8 ou supérieur ;
- WinUI 3 ;
- Windows App SDK.

### Justification

- application Windows 11 moderne ;
- intégration native avec l’écosystème Windows ;
- support de MVVM ;
- accès possible aux mécanismes de stockage sécurisé Windows ;
- bonne compatibilité avec Visual Studio.

## Architecture MVVM

### Principes

- Les vues affichent l’état.
- Les ViewModels orchestrent les interactions utilisateur.
- Les services applicatifs portent les cas d’usage.
- L’infrastructure gère les dépendances externes.
- Les modèles internes ne dépendent pas des DTO API.

### Flux type

```text
Vue WinUI
  -> ViewModel
    -> Service applicatif Core
      -> Interface Core
        -> Implémentation Infrastructure
          -> API Husqvarna / Stockage sécurisé / Logs
```

## Projets

```text
HusqvarnaAutomowerConnect.App
HusqvarnaAutomowerConnect.Core
HusqvarnaAutomowerConnect.Infrastructure
HusqvarnaAutomowerConnect.Tests
```

## Modules principaux

### Module UI

Projet : `HusqvarnaAutomowerConnect.App`

Contient :

- `App.xaml`
- `MainWindow`
- vues :
  - `DashboardPage`
  - `SettingsPage`
  - `LoginPage`
  - `MowerDetailsPage`
  - `LogsPage` si nécessaire
- ViewModels :
  - `DashboardViewModel`
  - `SettingsViewModel`
  - `LoginViewModel`
  - `MowerDetailsViewModel`

Responsabilités :

- afficher les données ;
- gérer la navigation ;
- déclencher les commandes ;
- afficher les états de chargement ;
- afficher les erreurs utilisateur ;
- respecter les libellés français.

### Module Core

Projet : `HusqvarnaAutomowerConnect.Core`

Contient :

- modèles internes ;
- interfaces ;
- services métier ;
- résultats typés ;
- erreurs applicatives.

Interfaces proposées :

- `IMowerService`
- `IHusqvarnaApiClient`
- `IHusqvarnaAuthClient`
- `ISecureTokenStore`
- `IAppSettingsStore`
- `IClock`

Services proposés :

- `MowerService`
- `AuthService`
- `CommandAvailabilityService`

### Module Infrastructure

Projet : `HusqvarnaAutomowerConnect.Infrastructure`

Contient :

- `HusqvarnaApiClient`
- `HusqvarnaAuthClient`
- `SecureTokenStore`
- `LocalSettingsStore`
- `ApiErrorMapper`
- DTO API ;
- mappers.

Responsabilités :

- gérer HTTP ;
- ajouter les headers requis ;
- renouveler les tokens ;
- transformer les DTO en modèles internes ;
- protéger les secrets ;
- gérer les erreurs API.

### Module Tests

Projet : `HusqvarnaAutomowerConnect.Tests`

Contient :

- tests unitaires Core ;
- tests des mappers Infrastructure ;
- tests d’erreurs API avec HTTP mocké ;
- tests des règles de sécurité liées aux logs ;
- tests des ViewModels sans dépendance API réelle.

## Couche API

### Interface

`IHusqvarnaApiClient` doit exposer des méthodes métier, pas des détails HTTP bruts.

Méthodes envisagées :

- `GetMowersAsync(CancellationToken)`
- `GetMowerAsync(string mowerId, CancellationToken)`
- `SendCommandAsync(string mowerId, MowerCommand command, CancellationToken)`
- `GetScheduleAsync(string mowerId, CancellationToken)` si disponible et utile en V1.

### Implémentation

`HusqvarnaApiClient` doit :

- utiliser `HttpClient` injecté ;
- ajouter l’access token ;
- ajouter l’Application Key selon la documentation officielle ;
- ajouter les headers de provider si requis ;
- gérer les timeouts ;
- mapper les erreurs ;
- ne jamais exposer les DTO à l’UI.

## Couche Auth

### Responsabilités

- construire l’URL d’autorisation OAuth2 ;
- gérer le callback OAuth2 selon le flux choisi ;
- échanger le code contre une session ;
- rafraîchir l’access token ;
- révoquer ou supprimer la session à la déconnexion ;
- exposer l’état connecté / non connecté.

### Interfaces

- `IHusqvarnaAuthClient`
- `IAuthSessionStore`
- `ISecureTokenStore`

### Points à vérifier

Avant implémentation, vérifier dans la documentation officielle :

- flux OAuth2 recommandé pour application desktop ;
- nécessité ou non de PKCE ;
- endpoint d’autorisation ;
- endpoint token ;
- endpoint revoke ;
- scopes ;
- règles de redirect URI ;
- durée des tokens ;
- payload exact de refresh.

## Couche stockage sécurisé

### Objectif

Stocker localement les tokens sans les exposer en clair dans le dépôt ou les logs.

### Interface

`ISecureTokenStore`

Méthodes envisagées :

- `SaveAsync(AuthSession session, CancellationToken)`
- `LoadAsync(CancellationToken)`
- `DeleteAsync(CancellationToken)`

### Implémentation

Utiliser un mécanisme Windows sécurisé :

- Windows Credential Manager ;
- `PasswordVault` ;
- autre équivalent natif Windows validé.

Le choix exact doit être documenté dans `docs/DECISIONS.md`.

## Couche configuration

Configuration locale :

- Application Key ;
- Redirect URI ;
- intervalle de rafraîchissement ;
- niveau de log ;
- activation éventuelle des notifications.

Règles :

- configuration sensible non versionnée ;
- secrets hors dépôt ;
- valeurs par défaut sûres ;
- validation au démarrage.

## Gestion des erreurs

### Types d’erreur internes

Prévoir une représentation typée :

- `NetworkError`
- `Unauthorized`
- `Forbidden`
- `RateLimited`
- `ApiUnavailable`
- `InvalidConfiguration`
- `UnsupportedCommand`
- `NoMowerFound`
- `UnknownApiError`

### Mapping HTTP

- `400` : requête invalide ou payload incorrect.
- `401` : token absent, expiré ou invalide.
- `403` : permissions insuffisantes, API non connectée ou accès refusé.
- `404` : robot ou ressource introuvable.
- `415` : type de contenu incorrect.
- `429` : quota ou rate limit.
- `500` : erreur serveur.
- `503` : service indisponible.

### UX

L’UI doit afficher un message court en français et proposer une action utile si possible.

Exemples :

- “Session expirée. Veuillez vous reconnecter.”
- “Commande indisponible pour ce robot.”
- “Service Husqvarna temporairement indisponible.”
- “Trop de requêtes. Le prochain essai sera effectué plus tard.”

## Schéma textuel des flux

### Démarrage application

```text
Lancement
  -> chargement configuration locale
  -> chargement session sécurisée
  -> si session présente : tentative refresh si nécessaire
  -> si connecté : chargement robots
  -> sinon : écran de connexion/configuration
```

### Rafraîchissement tableau de bord

```text
Timer de rafraîchissement
  -> vérification connexion
  -> appel GetMowers
  -> mapping vers modèles internes
  -> mise à jour ViewModel
  -> affichage dernière mise à jour
```

### Envoi d’une commande

```text
Clic utilisateur
  -> vérification disponibilité commande
  -> appel SendCommand
  -> réponse API acceptée ou erreur
  -> feedback utilisateur
  -> rafraîchissement différé du robot
```

### Token expiré

```text
Appel API reçoit 401
  -> tentative de refresh token
  -> si succès : rejouer l’appel une seule fois
  -> si échec : supprimer session locale et demander reconnexion
```

## Dépendances recommandées

- `CommunityToolkit.Mvvm`
- `Microsoft.Extensions.DependencyInjection`
- `Microsoft.Extensions.Http`
- `Microsoft.Extensions.Logging`
- `Microsoft.Extensions.Options`
- `xunit`
- `FluentAssertions`
- `RichardSzalay.MockHttp` ou équivalent
- Librairie validée pour Credential Manager si nécessaire

Toute dépendance doit être justifiée et maintenable.

## Points de vigilance

- Ne pas exposer les tokens.
- Ne pas disperser les appels API dans l’UI.
- Ne pas supposer que tous les robots supportent toutes les commandes.
- Ne pas rendre le refresh trop fréquent.
- Ne pas boucler indéfiniment sur `401`.
- Ne pas modifier le planning en V1.
- Ne pas laisser les exceptions HTTP remonter directement jusqu’à l’UI.
- Ne pas bloquer le thread UI.
- Ne pas faire d’appels API concurrents inutiles.
- Ne pas mélanger données API brutes et modèles affichés.
