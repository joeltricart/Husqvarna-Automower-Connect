# Intégration Husqvarna API

## Objectif

Documenter les règles d’intégration avec l’API officielle Husqvarna Developer / Automower Connect.

Ce fichier ne remplace pas la documentation officielle. Codex doit vérifier les endpoints, headers, payloads, scopes et statuts dans la documentation officielle avant de coder.

## Sources officielles vérifiées le 12 mai 2026

- Portail Husqvarna Developer : `https://developer.husqvarnagroup.cloud/`
- OpenAPI Authentication API : `https://docs.developer.husqvarnagroup.cloud/authentication-api/swagger.yml`
- OpenAPI Automower Connect API : `https://docs.developer.husqvarnagroup.cloud/automower-connect-api/swagger.yml`

## Principe général

L’application doit utiliser uniquement les API officielles Husqvarna :

- Authentication API pour OAuth2 ;
- Automower Connect API pour les robots et commandes.

Le compte Husqvarna utilisé doit être celui qui possède ou voit les robots dans l’application Automower Connect.

## Sécurité d’accès API

L’application doit uniquement envoyer les éléments officiellement confirmés.

### Authentication API

Points confirmés par le swagger :

- serveur : `https://api.authentication.husqvarnagroup.dev/v1`
- endpoint token : `POST /oauth2/token`
- endpoint revoke : `POST /oauth2/revoke`
- content type : `application/x-www-form-urlencoded`

`POST /oauth2/token` documente les champs suivants :

- `grant_type` requis ;
- `client_id` requis ;
- `client_secret` documenté pour `authorization_code` et `client_credentials` ;
- `code` pour `authorization_code` ;
- `redirect_uri` pour `authorization_code` ;
- `refresh_token` pour `refresh_token` ;
- `scope` optionnel.

Valeurs de `grant_type` documentées :

- `authorization_code`
- `client_credentials`
- `refresh_token`

Réponse de succès documentée :

- statut `201`
- `access_token`
- `scope`
- `expires_in`
- `refresh_token`
- `provider`
- `user_id`
- `token_type`

`POST /oauth2/revoke` documente :

- header `Authorization: Bearer <access_token>` ;
- body `application/x-www-form-urlencoded` avec champ `token`.

### Automower Connect API

Points confirmés par le swagger :

- serveur : `https://api.amc.husqvarna.dev/v1`
- header `X-Api-Key` requis ;
- header `Authorization: Bearer <token>` requis ;
- header `Authorization-Provider: husqvarna` requis ;
- content type des commandes : `application/vnd.api+json`.

## OAuth2

### Objectifs

L’application doit permettre :

- connexion utilisateur ;
- obtention d’un access token ;
- obtention et stockage sécurisé d’un refresh token si fourni ;
- renouvellement automatique ;
- déconnexion propre.

### Points encore à confirmer officiellement

Les fichiers OpenAPI accessibles ne documentent pas encore clairement :

- l’endpoint navigateur d’autorisation utilisateur ;
- l’usage ou non de PKCE ;
- la redirect URI recommandée pour une application desktop ;
- les scopes exacts à demander pour la V1.

Tant que ces points ne sont pas confirmés par une source officielle exploitable, le client OAuth ne doit pas supposer un endpoint d’autorisation ni un schéma PKCE.

### Règles

- Ne pas demander le mot de passe Husqvarna directement dans l’application si un flux OAuth navigateur est possible.
- Ne pas stocker l’Application Secret en clair.
- Ne pas logger les paramètres OAuth sensibles.
- Ne pas rejouer indéfiniment une requête après `401`.

## Endpoints Automower confirmés

### Liste des robots

```text
GET /mowers
```

Usage attendu :

- lister les robots liés à l’utilisateur associé à l’access token ;
- récupérer un snapshot suffisant pour le tableau de bord si disponible.

### Détail d’un robot

```text
GET /mowers/{id}
```

Usage attendu :

- récupérer le détail d’un robot précis ;
- rafraîchir un robot sélectionné ;
- obtenir des données potentiellement absentes de la liste.

### Actions robot

```text
POST /mowers/{id}/actions
```

Actions officiellement documentées :

- `Pause`
- `ResumeSchedule`
- `Park`
- `ParkUntilNextSchedule`
- `ParkUntilFurtherNotice`
- `Start`
- `StartInWorkArea`

Payloads confirmés :

- `Pause`, `ResumeSchedule`, `ParkUntilNextSchedule`, `ParkUntilFurtherNotice` sans attribut additionnel ;
- `Start` avec `attributes.duration` ;
- `StartInWorkArea` avec `attributes.duration` et `attributes.workAreaId` ;
- `Park` avec `attributes.duration` ou `attributes.externalReason`.

Point important pour la V1 :

- le besoin UX “retour à la station” ne correspond pas à un seul nom d’action officiel ;
- le swagger distingue au moins `ParkUntilNextSchedule` et `ParkUntilFurtherNotice` ;
- la V1 doit exposer explicitement le comportement choisi, sans renommer une action de façon trompeuse.

Réponse de succès documentée :

- statut `202`
- acceptation asynchrone, pas d’état final immédiat garanti.

### Calendrier

```text
POST /mowers/{id}/calendar
```

Le swagger confirme que cette requête remplace le calendrier envoyé côté robot.

Règle V1 :

- ne pas modifier le planning ;
- lecture ou affichage simple uniquement si disponible plus tard ;
- aucune écriture calendrier sans tâche dédiée.

### Messages / erreurs

```text
GET /mowers/{id}/messages
```

Usage possible :

- récupérer les derniers messages ou erreurs ;
- améliorer l’affichage des alertes.

### Ressources avancées confirmées mais hors V1

Le swagger expose aussi des ressources hors périmètre V1 :

- `/mowers/{id}/settings`
- `/mowers/{id}/stayOutZones`
- `/mowers/{id}/workAreas`
- `/mowers/{id}/workAreas/{workAreaId}`
- `/mowers/{id}/workAreas/{workAreaId}/calendar`
- `/mowers/{id}/statistics/resetCuttingBladeUsageTime`
- `/mowers/{id}/errors/confirm`

Ces endpoints ne doivent pas être implémentés en V1 sauf tâche explicite.

## Ressources attendues

### `mowers`

Données attendues :

- identifiant ;
- nom ;
- modèle si disponible ;
- état système ;
- batterie ;
- activité ;
- mode ;
- erreur ;
- position ;
- calendrier ou références calendrier ;
- dernière mise à jour.

Toutes les données doivent être considérées comme potentiellement absentes.

### `actions`

Les noms d’action exacts ne doivent plus être supposés : seuls ceux listés dans le swagger sont autorisés.

## Mapping des données API vers modèles internes

### Principes

- Les DTO API restent dans `HusqvarnaAutomowerConnect.Infrastructure`.
- Les modèles internes restent dans `HusqvarnaAutomowerConnect.Core`.
- Les ViewModels ne consomment jamais les DTO API directement.
- Les champs absents doivent produire des valeurs nullables ou un état “Non disponible”.

### Mapping indicatif

| API | Modèle interne | Règle |
|---|---|---|
| Identifiant robot | `Mower.Id` | Obligatoire si fourni par API |
| Nom robot | `Mower.Name` | Fallback : “Robot sans nom” |
| Batterie | `BatteryInfo.LevelPercent` | Nullable |
| État | `MowerStatus.State` | Enum interne avec valeur `Unknown` |
| Activité | `MowerStatus.Activity` | Enum interne avec valeur `Unknown` |
| Mode | `MowerStatus.Mode` | Enum interne avec valeur `Unknown` |
| Erreur | `MowerError` | Nullable |
| Position | `MowerLocation` | Nullable |
| Dernière mise à jour | `MowerStatus.UpdatedAt` | Nullable si API absente |

## Stratégie de refresh token

### Déclencheurs

- Au démarrage si session existante.
- Avant un appel API si l’access token est expiré ou proche expiration.
- Après un `401`, une seule tentative de refresh peut être effectuée.

### Règles

- Ne jamais effectuer plusieurs refresh concurrents.
- Protéger le refresh par un mécanisme de verrou applicatif.
- En cas d’échec du refresh :
  - supprimer la session locale ;
  - afficher “Session expirée. Veuillez vous reconnecter.” ;
  - ne pas boucler.
- Sauvegarder le nouveau refresh token si l’API en retourne un.
- Ne jamais logger la réponse OAuth complète.

## Gestion des erreurs API

### Erreurs à gérer

| Code | Interprétation utilisateur | Action application |
|---|---|---|
| 400 | Requête invalide | Message générique + log technique nettoyé |
| 401 | Session expirée ou invalide | Refresh puis reconnexion si échec |
| 403 | Accès refusé ou API non autorisée | Vérifier configuration portail |
| 404 | Ressource introuvable | Rafraîchir la liste des robots |
| 415 | Format de requête incorrect | Bug d’intégration à corriger |
| 429 | Trop de requêtes | Espacer les appels |
| 500 | Erreur serveur | Réessayer plus tard |
| 503 | Service indisponible | Réessayer plus tard |

### Réseau

Gérer :

- absence d’Internet ;
- timeout ;
- DNS ;
- annulation utilisateur ;
- fermeture de l’application.

## Disponibilité variable des fonctions

L’application doit considérer qu’une fonction peut être indisponible selon :

- modèle du robot ;
- génération du robot ;
- connectivité ;
- pays ou compte ;
- permissions API ;
- état courant du robot ;
- configuration dans l’application Automower Connect ;
- évolution de l’API.

Si une fonction est indisponible :

- ne pas masquer l’ensemble de l’application ;
- désactiver uniquement l’action concernée ;
- afficher une explication simple.

## Quotas et fréquence d’appel

Codex doit vérifier les règles de quota officielles lorsqu’elles deviennent accessibles dans une documentation officielle exploitable.

Règles applicatives par défaut :

- intervalle de refresh configurable ;
- défaut recommandé : 60 secondes ;
- pas de polling agressif ;
- pas de boucle de retry rapide ;
- backoff en cas de `429` ou `503`.

## Consigne stricte

Codex ne doit jamais coder un endpoint, un header, un payload ou une action Husqvarna sans l’avoir vérifié dans la documentation officielle Husqvarna.

Si la documentation est ambiguë :

1. créer un commentaire technique dans le code ;
2. documenter l’incertitude dans ce fichier ;
3. écrire un test avec API mockée ;
4. éviter de livrer une commande potentiellement incorrecte en V1.
## ComplÃ©ment officiel confirmÃ©

Le portail Husqvarna Developer expose le flux dâ€™autorisation officiel sur :

```text
GET /oauth2/authorize
```

Base confirmÃ©e :

- `https://api.authentication.husqvarnagroup.dev/v1`

ParamÃ¨tres observÃ©s dans le portail officiel :

- `client_id`
- `redirect_uri`
- `language`

ConsÃ©quence pour la V1 :

- lâ€™application ouvre le navigateur sur cet endpoint officiel ;
- le callback local utilise `http://localhost` ;
- le secret dâ€™application reste hors du dÃ©pÃ´t et est stockÃ© localement dans le coffre Windows ;
- le flux PKCE nâ€™est pas supposÃ© tant quâ€™il nâ€™est pas explicitement documentÃ©.
