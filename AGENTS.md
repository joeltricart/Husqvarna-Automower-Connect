# AGENTS.md

## Rôle de Codex

Codex doit développer l’application `MowerControl`, une application Windows 11 de contrôle et supervision d’un robot-tondeuse Husqvarna Automower via l’API officielle Husqvarna Developer / Automower Connect.

Codex doit :

- lire la documentation du dépôt avant toute modification ;
- analyser le besoin avant de coder ;
- respecter strictement le périmètre V1 ;
- écrire un code maintenable, testé et sécurisé ;
- ne jamais inventer de comportement API non vérifié ;
- documenter les décisions techniques non triviales dans `docs/DECISIONS.md`.

Codex ne doit pas :

- coder directement contre des endpoints supposés ;
- stocker de secrets dans le dépôt ;
- ajouter une stack non demandée sans justification documentée ;
- remplacer WinUI 3 par une autre technologie ;
- introduire de télémétrie externe ;
- exposer des tokens dans les logs ;
- ignorer les tests lorsqu’une logique métier est modifiée.

## Fichiers à lire impérativement

Avant tout développement, lire dans cet ordre :

1. `PROJECT.md`
2. `TECHNICAL_CONTEXT.md`
3. `README.md`
4. `docs/PRODUCT_SPEC.md`
5. `docs/ARCHITECTURE.md`
6. `docs/HUSQVARNA_API.md`
7. `docs/SECURITY.md`
8. `docs/UX_SPEC.md`
9. `docs/DATA_MODEL.md`
10. `docs/TASKS.md`
11. `docs/TEST_PLAN.md`
12. `docs/SETUP.md`
13. `docs/DECISIONS.md`

## Stack technique

Stack cible obligatoire sauf décision explicite documentée :

- C# ;
- .NET 8 ou supérieur ;
- WinUI 3 ;
- Windows App SDK ;
- MVVM ;
- `CommunityToolkit.Mvvm` ;
- `IHttpClientFactory` ;
- stockage sécurisé local via Windows Credential Manager, `PasswordVault` ou équivalent Windows sécurisé ;
- tests xUnit.

## Structure attendue du dépôt

```text
/src
  /MowerControl.App
  /MowerControl.Core
  /MowerControl.Infrastructure
  /MowerControl.Tests

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

## Organisation des projets

### `MowerControl.App`

Responsabilités :

- vues WinUI 3 ;
- ViewModels ;
- commandes UI ;
- navigation ;
- ressources en français ;
- affichage des états, erreurs et feedbacks.

Interdictions :

- appels HTTP directs dans les vues ;
- secrets dans les fichiers XAML ou code-behind ;
- logique métier complexe dans le code-behind.

### `MowerControl.Core`

Responsabilités :

- modèles internes ;
- interfaces ;
- services applicatifs ;
- résultats de commande ;
- erreurs typées ;
- règles de validation.

Interdictions :

- dépendance à WinUI ;
- dépendance directe à l’API Husqvarna ;
- dépendance au stockage Windows.

### `MowerControl.Infrastructure`

Responsabilités :

- implémentation du client Husqvarna ;
- OAuth2 ;
- refresh token ;
- stockage sécurisé ;
- configuration ;
- logs ;
- mapping API vers modèles internes.

Interdictions :

- logique d’interface ;
- décisions de navigation ;
- libellés UI non nécessaires.

### `MowerControl.Tests`

Responsabilités :

- tests unitaires ;
- tests de mapping ;
- tests avec API mockée ;
- tests de sécurité des logs ;
- tests des cas d’erreur.

## Conventions de code

- Utiliser des noms explicites.
- Préférer des classes petites et testables.
- Utiliser `async`/`await` pour les appels I/O.
- Passer systématiquement un `CancellationToken` aux appels réseau.
- Éviter les singletons globaux hors injection de dépendances.
- Utiliser des interfaces pour les dépendances externes.
- Ne pas mélanger DTO API et modèles internes.
- Traiter la nullabilité explicitement.
- Activer nullable reference types.
- Écrire les messages utilisateur en français.
- Garder les exceptions techniques hors de l’interface utilisateur finale.

## Noms de classes suggérés

Codex peut utiliser ces noms s’ils restent adaptés :

- `Mower`
- `MowerStatus`
- `BatteryInfo`
- `MowerError`
- `MowerLocation`
- `MowerSchedule`
- `MowerCommand`
- `CommandResult`
- `AuthSession`
- `IHusqvarnaApiClient`
- `IHusqvarnaAuthClient`
- `ISecureTokenStore`
- `IMowerService`
- `IAppSettingsStore`
- `DashboardViewModel`
- `SettingsViewModel`
- `MowerDetailsViewModel`
- `ApiErrorMapper`

## Commandes de build, test et format

À adapter à la solution créée, mais viser :

```bash
dotnet restore
dotnet build ./src/MowerControl.sln --configuration Debug
dotnet test ./src/MowerControl.sln --configuration Debug
dotnet format ./src/MowerControl.sln --verify-no-changes
```

Pour release locale :

```bash
dotnet build ./src/MowerControl.sln --configuration Release
dotnet test ./src/MowerControl.sln --configuration Release
```

Si une commande ne fonctionne pas à cause de WinUI 3 ou du packaging Windows App SDK, documenter la commande correcte dans `docs/SETUP.md`.

## Règles de sécurité

Codex doit appliquer ces règles sans exception :

- ne jamais commiter de secret ;
- ne jamais commiter d’access token ;
- ne jamais commiter de refresh token ;
- ne jamais logger de token ;
- ne jamais logger de client secret ;
- masquer les identifiants sensibles dans les logs ;
- stocker les tokens uniquement via stockage sécurisé ;
- supprimer les tokens locaux à la déconnexion ;
- utiliser des permissions OAuth minimales ;
- gérer proprement token expiré, révoqué ou invalide ;
- documenter toute nouvelle donnée sensible dans `docs/SECURITY.md`.

## Règles API Husqvarna

Codex doit vérifier dans la documentation officielle Husqvarna avant de coder :

- URL de base active ;
- endpoints ;
- méthodes HTTP ;
- headers requis ;
- format exact des payloads ;
- types d’actions disponibles ;
- codes d’erreur ;
- limites de quotas ;
- exigences OAuth2 ;
- disponibilité des ressources selon modèle de robot.

Interdiction stricte :

- ne pas inventer d’endpoint ;
- ne pas inventer de payload ;
- ne pas supposer qu’une commande est supportée par tous les modèles ;
- ne pas coder une action destructive ou persistante non demandée ;
- ne pas modifier le calendrier en V1 sauf si une tâche dédiée le demande explicitement et que la documentation officielle a été vérifiée.

## Règles UX

- Interface en français.
- État de connexion visible.
- État vide clair si aucun robot.
- Boutons de commande désactivés si non disponibles.
- Confirmation visuelle après demande de commande.
- Message explicite si l’API accepte une commande mais que l’état du robot n’est pas encore mis à jour.
- Ne pas afficher de codes techniques seuls à l’utilisateur.
- Prévoir un détail technique accessible dans les logs, pas dans le message principal.

## Règles Git

- Une tâche = commits cohérents.
- Ne pas mélanger refactor massif et fonctionnalité.
- Mettre à jour les tests avec la logique métier.
- Mettre à jour la documentation si le comportement change.
- Ne pas commiter :
  - `bin/`
  - `obj/`
  - `.vs/`
  - fichiers locaux de secrets ;
  - logs locaux ;
  - tokens ;
  - exports de configuration personnelle.
- Mettre à jour `CHANGELOG.md` pour tout changement utilisateur visible.

## Définition du travail terminé

Une tâche est terminée uniquement si :

- le code compile ;
- les tests pertinents passent ;
- la logique nouvelle est testée ;
- les erreurs attendues sont gérées ;
- aucun secret n’est introduit ;
- l’UX est cohérente avec `docs/UX_SPEC.md` ;
- la documentation impactée est mise à jour ;
- les limites connues sont documentées ;
- le comportement reste dans le périmètre V1 ou la décision est justifiée.

## Tests obligatoires lors des modifications métier

Lorsqu’une logique métier est modifiée, Codex doit créer ou mettre à jour les tests correspondants.

Exemples :

- mapping API vers `MowerStatus` ;
- calcul de disponibilité d’une commande ;
- gestion d’un refresh token expiré ;
- interprétation d’une réponse `401`, `403`, `429` ou `500` ;
- masquage des secrets dans les logs ;
- comportement en absence de robot.

Aucune logique métier nouvelle ne doit rester sans test.
