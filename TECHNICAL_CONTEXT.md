# Contexte technique

## Environnement cible

- OS cible : Windows 11.
- Type d’application : application desktop native Windows.
- Usage initial : personnel/local.
- Connexion Internet requise pour l’API Husqvarna.
- Compte Husqvarna Automower Connect requis.
- Robot compatible Automower Connect requis.

## Stack technique cible

Stack recommandée et à conserver sauf contrainte explicite contraire :

- C# ;
- .NET 8 ou supérieur ;
- WinUI 3 ;
- Windows App SDK ;
- MVVM ;
- `CommunityToolkit.Mvvm` pour réduire le code boilerplate MVVM ;
- `Microsoft.Extensions.Http` pour `IHttpClientFactory` ;
- `Microsoft.Extensions.Logging` pour la journalisation ;
- `Microsoft.Extensions.Options` pour la configuration typée ;
- xUnit pour les tests ;
- mocks HTTP pour les tests d’intégration.

## Structure de dépôt cible

```text
/src
  /HusqvarnaAutomowerConnect.App
  /HusqvarnaAutomowerConnect.Core
  /HusqvarnaAutomowerConnect.Infrastructure
  /HusqvarnaAutomowerConnect.Tests

/docs
  PRODUCT_SPEC.md
  ARCHITECTURE.md
  HUSQVARNA_API.md
  SECURITY.md
  UX_SPEC.md
  DATA_MODEL.md
  TASKS.md
  TEST_PLAN.md
  SETUP.md
  DECISIONS.md

.github
  pull_request_template.md

AGENTS.md
README.md
PROJECT.md
TECHNICAL_CONTEXT.md
CHANGELOG.md
.gitignore
```

## Responsabilités par projet

### `HusqvarnaAutomowerConnect.App`

Contient :

- vues WinUI 3 ;
- ViewModels ;
- navigation ;
- ressources de langue française ;
- intégration avec les services applicatifs via interfaces.

Ne contient pas :

- logique HTTP brute ;
- secrets ;
- parsing direct de réponses API Husqvarna dans les vues.

### `HusqvarnaAutomowerConnect.Core`

Contient :

- modèles de domaine internes ;
- interfaces ;
- cas d’usage ;
- règles métier ;
- résultats typés ;
- erreurs applicatives.

Ne dépend pas de WinUI.

### `HusqvarnaAutomowerConnect.Infrastructure`

Contient :

- client API Husqvarna ;
- implémentation OAuth ;
- stockage sécurisé ;
- journalisation fichier ;
- configuration locale ;
- mapping DTO API vers modèles internes.

Ne contient pas de logique d’interface.

### `HusqvarnaAutomowerConnect.Tests`

Contient :

- tests unitaires ;
- tests de mapping ;
- tests d’erreurs ;
- tests avec API mockée ;
- tests de services applicatifs.

## Dépendances externes

### Obligatoires

- API Husqvarna Developer / Automower Connect.
- Authentication API Husqvarna.
- Portail développeur Husqvarna pour créer une application et obtenir les identifiants nécessaires.

### Recommandées

- `CommunityToolkit.Mvvm`.
- `Microsoft.Extensions.Http`.
- `Microsoft.Extensions.Logging`.
- `Microsoft.Extensions.Options`.
- `xunit`.
- `FluentAssertions`.
- `RichardSzalay.MockHttp` ou équivalent pour mocker HTTP.
- API Windows Credential Manager ou `PasswordVault`, derrière une interface `ISecureTokenStore`.

## Configuration locale

Prévoir une configuration locale non versionnée, par exemple :

- `appsettings.Local.json` ;
- secrets utilisateur .NET ;
- stockage chiffré local ;
- variables d’environnement pour développement.

Le dépôt ne doit jamais contenir :

- access token ;
- refresh token ;
- client secret ;
- Application Key personnelle ;
- fichier de configuration sensible ;
- export de logs contenant des données sensibles.

## Contraintes sécurité

- Aucun secret en clair dans le code.
- Aucun token dans les logs.
- Refresh token stocké uniquement via stockage sécurisé.
- Déconnexion = suppression de la session locale.
- Réauthentification demandée si refresh impossible.
- Permissions OAuth minimales.
- Endpoints, headers et payloads Husqvarna à vérifier dans la documentation officielle avant codage.

## Contraintes UX

- Tous les libellés visibles sont en français.
- Les erreurs doivent être compréhensibles pour un utilisateur non développeur.
- Les commandes non supportées ne doivent pas être proposées comme disponibles.
- L’application doit rester lisible même si certaines données API sont absentes.
- L’état hors ligne doit être explicite.

## Contraintes performance

- Ne pas rafraîchir l’API trop souvent.
- Intervalle de rafraîchissement configurable.
- Valeur par défaut recommandée : 60 secondes.
- Empêcher les appels concurrents inutiles.
- Prévoir annulation des requêtes lors de la fermeture ou d’un changement de compte.
- Gérer les réponses `429 Too Many Requests`.

## Limites connues

- Certaines commandes dépendent du modèle du robot, de son état, du compte et des permissions API.
- Certaines données peuvent être absentes ou nulles.
- Le calendrier peut être complexe à modifier correctement.
- L’API peut accepter une commande de façon asynchrone sans effet immédiat visible.
- La connectivité du robot peut varier selon Bluetooth, cellulaire ou Wi-Fi.
