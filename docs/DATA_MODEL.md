# Modèle de données interne

## Principes

Les modèles internes doivent être stables, testables et indépendants des DTO Husqvarna.

Règles :

- DTO API dans `HusqvarnaAutomowerConnect.Infrastructure`.
- Modèles internes dans `HusqvarnaAutomowerConnect.Core`.
- ViewModels basés sur les modèles internes.
- Nullabilité explicite.
- Valeur `Unknown` pour les enums lorsque l’API retourne une valeur inconnue.
- Pas d’exception pour une donnée optionnelle absente.

## Entités principales

## `Mower`

Représente un robot.

| Champ | Type suggéré | Nullabilité | Description |
|---|---:|---:|---|
| `Id` | `string` | Non null | Identifiant API du robot |
| `Name` | `string` | Non null | Nom affichable |
| `Model` | `string` | Nullable | Modèle si disponible |
| `SerialNumber` | `string` | Nullable | Numéro de série si disponible |
| `Status` | `MowerStatus` | Nullable | État courant |
| `Battery` | `BatteryInfo` | Nullable | Informations batterie |
| `Location` | `MowerLocation` | Nullable | Position GPS |
| `Schedule` | `MowerSchedule` | Nullable | Planning si disponible |
| `Capabilities` | `MowerCapabilities` | Non null | Capacités déduites ou inconnues |
| `LastUpdatedAt` | `DateTimeOffset?` | Nullable | Dernière mise à jour connue |

Règles :

- `Id` est obligatoire pour agir sur le robot.
- `Name` doit avoir un fallback : “Robot sans nom”.
- `Capabilities` doit utiliser des valeurs prudentes si l’API ne confirme pas les actions.

## `MowerStatus`

Représente l’état courant.

| Champ | Type suggéré | Nullabilité | Description |
|---|---:|---:|---|
| `State` | `MowerState` | Non null | État général |
| `Activity` | `MowerActivity` | Non null | Activité courante |
| `Mode` | `MowerMode` | Non null | Mode courant |
| `Connected` | `bool?` | Nullable | Connectivité si disponible |
| `RestrictedReason` | `string` | Nullable | Raison de restriction si fournie |
| `Error` | `MowerError` | Nullable | Erreur active |
| `UpdatedAt` | `DateTimeOffset?` | Nullable | Timestamp API |

Enums internes suggérés :

```text
MowerState:
- Unknown
- Ready
- InOperation
- Paused
- Parked
- Error
- Offline

MowerActivity:
- Unknown
- Mowing
- GoingHome
- Charging
- Leaving
- ParkedInChargingStation
- Stopped

MowerMode:
- Unknown
- MainArea
- SecondaryArea
- Home
- Demo
```

Les valeurs exactes API doivent être mappées après vérification officielle.

## `BatteryInfo`

| Champ | Type suggéré | Nullabilité | Description |
|---|---:|---:|---|
| `LevelPercent` | `int?` | Nullable | Pourcentage de batterie |
| `IsCharging` | `bool?` | Nullable | En charge si disponible |
| `UpdatedAt` | `DateTimeOffset?` | Nullable | Timestamp batterie |

Validation :

- `LevelPercent` doit être entre 0 et 100 si présent.
- Si hors plage, mapper vers `null` ou erreur de mapping non bloquante.

## `MowerError`

| Champ | Type suggéré | Nullabilité | Description |
|---|---:|---:|---|
| `Code` | `string` | Nullable | Code API |
| `Message` | `string` | Nullable | Message normalisé |
| `Severity` | `MowerErrorSeverity` | Non null | Gravité interne |
| `OccurredAt` | `DateTimeOffset?` | Nullable | Date d’apparition |
| `IsConfirmable` | `bool?` | Nullable | Confirmable si API disponible |

Enum :

```text
MowerErrorSeverity:
- Unknown
- Info
- Warning
- Critical
```

Règles :

- Ne pas afficher un code seul à l’utilisateur.
- Utiliser un message français fallback.

## `MowerLocation`

| Champ | Type suggéré | Nullabilité | Description |
|---|---:|---:|---|
| `Latitude` | `double` | Non null | Latitude |
| `Longitude` | `double` | Non null | Longitude |
| `AccuracyMeters` | `double?` | Nullable | Précision si disponible |
| `UpdatedAt` | `DateTimeOffset?` | Nullable | Timestamp position |

Validation :

- Latitude entre -90 et 90.
- Longitude entre -180 et 180.
- Si invalide, ignorer la position.

## `MowerSchedule`

| Champ | Type suggéré | Nullabilité | Description |
|---|---:|---:|---|
| `Tasks` | `IReadOnlyList<ScheduleTask>` | Non null | Tâches planifiées |
| `UpdatedAt` | `DateTimeOffset?` | Nullable | Dernière mise à jour |

## `ScheduleTask`

| Champ | Type suggéré | Nullabilité | Description |
|---|---:|---:|---|
| `Id` | `string` | Nullable | Identifiant si fourni |
| `Days` | `IReadOnlySet<DayOfWeek>` | Non null | Jours actifs |
| `StartTime` | `TimeOnly` | Non null | Heure de début |
| `Duration` | `TimeSpan` | Non null | Durée |
| `WorkAreaId` | `string` | Nullable | Zone si disponible |
| `Enabled` | `bool?` | Nullable | Actif si disponible |

Règle V1 :

- affichage seulement ;
- aucune modification calendrier.

## `CommandResult`

Représente le résultat d’une commande envoyée au robot.

| Champ | Type suggéré | Nullabilité | Description |
|---|---:|---:|---|
| `Success` | `bool` | Non null | Requête acceptée ou non |
| `Command` | `MowerCommand` | Non null | Commande demandée |
| `Message` | `string` | Non null | Message utilisateur français |
| `TechnicalCode` | `string` | Nullable | Code technique non sensible |
| `AcceptedAt` | `DateTimeOffset?` | Nullable | Date d’acceptation |
| `ShouldRefresh` | `bool` | Non null | Indique s’il faut rafraîchir |

## `MowerCommand`

Représente une commande interne, indépendante du payload API.

```text
MowerCommandType:
- Pause
- ParkUntilNextSchedule
- ParkUntilFurtherNotice
- ResumeSchedule
- StartForDuration
```

| Champ | Type suggéré | Nullabilité | Description |
|---|---:|---:|---|
| `Type` | `MowerCommandType` | Non null | Type interne |
| `Duration` | `TimeSpan?` | Nullable | Durée si nécessaire |
| `WorkAreaId` | `string` | Nullable | Zone si supportée plus tard |

Règle :

- le mapping vers le payload API doit rester dans `Infrastructure`.
- les noms exacts d’actions API doivent être vérifiés avant implémentation.

## `MowerCapabilities`

| Champ | Type suggéré | Nullabilité | Description |
|---|---:|---:|---|
| `CanPause` | `bool` | Non null | Pause disponible |
| `CanPark` | `bool` | Non null | Retour station disponible |
| `CanResumeSchedule` | `bool` | Non null | Reprise planning disponible |
| `CanStartForDuration` | `bool` | Non null | Tonte temporaire disponible |
| `CanShowSchedule` | `bool` | Non null | Planning affichable |
| `CanEditSchedule` | `bool` | Non null | Toujours false en V1 |

Règle prudente :

- Si la disponibilité n’est pas confirmée, considérer `false` pour l’action concernée ou afficher “Disponibilité inconnue”.

## `AuthSession`

| Champ | Type suggéré | Nullabilité | Description |
|---|---:|---:|---|
| `AccessToken` | `string` | Non null | Token en mémoire |
| `RefreshToken` | `string` | Nullable | Token stocké sécurisé |
| `ExpiresAt` | `DateTimeOffset?` | Nullable | Expiration |
| `TokenType` | `string` | Nullable | Type, souvent Bearer |
| `Scopes` | `IReadOnlyList<string>` | Non null | Scopes si disponibles |

Règles :

- Ne jamais logger `AccessToken`.
- Ne jamais logger `RefreshToken`.
- Ne jamais exposer ce modèle directement à l’UI.
- Ne pas sérialiser en clair.

## Mapping depuis l’API

### Règles de mapping

- Accepter les champs inconnus sans crash.
- Mapper les enums inconnues vers `Unknown`.
- Mapper les timestamps invalides vers `null`.
- Conserver un log debug nettoyé pour les anomalies de mapping.
- Ne jamais bloquer tout le tableau de bord à cause d’un champ manquant.

### Tests de mapping obligatoires

Créer des tests pour :

- robot complet ;
- robot sans nom ;
- robot sans batterie ;
- robot sans position ;
- état inconnu ;
- erreur active ;
- timestamp absent ;
- batterie hors plage ;
- action non supportée.

## Validation

Validation minimale :

- `Mower.Id` non vide ;
- batterie entre 0 et 100 ;
- coordonnées GPS valides ;
- durée de commande temporaire positive ;
- intervalle de refresh raisonnable ;
- Application Key non vide avant connexion.

En cas de donnée API invalide :

- ne pas crasher ;
- ignorer le champ invalide ;
- journaliser sans secret ;
- afficher “Non disponible”.
