# Sécurité

## Objectif

Garantir que l’application `Husqvarna Automower Connect` ne fuite aucun secret, token ou information sensible tout en restant utilisable localement.

## Données sensibles

Sont considérés comme sensibles :

- access token ;
- refresh token ;
- Application Secret ;
- Application Key personnelle ;
- code d’autorisation OAuth ;
- réponse OAuth brute ;
- headers `Authorization` ;
- configuration locale contenant des identifiants ;
- logs HTTP complets ;
- identifiants de compte si présents dans une réponse.

## Gestion des secrets

### Interdictions

Ne jamais stocker dans le dépôt :

- `appsettings.Local.json` ;
- `.env` ;
- access token ;
- refresh token ;
- client secret ;
- Application Secret ;
- Application Key personnelle ;
- logs locaux ;
- captures contenant des secrets.

### Configuration locale

La configuration sensible doit être fournie via :

- secrets utilisateur .NET ;
- variables d’environnement ;
- fichier local ignoré par Git ;
- stockage sécurisé Windows.

Le fichier `.gitignore` doit protéger les noms courants :

- `appsettings.Local.json`
- `.env`
- `*.user`
- `*.log`
- dossiers de logs locaux.

## Stockage local sécurisé

### Exigence

Les tokens doivent être stockés via un mécanisme Windows sécurisé :

- Windows Credential Manager ;
- `PasswordVault` ;
- autre équivalent Windows explicitement documenté.

### Interface obligatoire

L’application doit utiliser une abstraction :

```text
ISecureTokenStore
```

Aucun composant UI ne doit accéder directement au stockage sécurisé.

### Règles

- Le refresh token doit être stocké de façon sécurisée.
- L’access token peut être gardé en mémoire et stocké uniquement si nécessaire.
- La suppression de session doit supprimer les tokens.
- Les erreurs de stockage sécurisé doivent être gérées proprement.

## Logs

### Interdictions

Les logs ne doivent jamais contenir :

- access token ;
- refresh token ;
- client secret ;
- code OAuth ;
- header Authorization complet ;
- payload OAuth complet ;
- fichier de configuration sensible complet.

### Autorisé

Les logs peuvent contenir :

- timestamp ;
- catégorie de l’événement ;
- statut HTTP ;
- endpoint logique sans query sensible ;
- durée d’appel ;
- type d’erreur ;
- identifiant technique de corrélation si non sensible.

### Nettoyage

Tout logger HTTP doit appliquer un masquage.

Exemples de remplacement :

```text
Authorization: Bearer ***
access_token: ***
refresh_token: ***
client_secret: ***
```

## Permissions OAuth minimales

Codex doit vérifier dans la documentation officielle les scopes ou permissions disponibles.

Règles :

- demander uniquement les permissions nécessaires à la V1 ;
- ne pas demander de permissions de modification du calendrier si la V1 ne modifie pas le planning ;
- documenter toute permission demandée dans ce fichier.

## Token expiré ou révoqué

### Access token expiré

Comportement attendu :

1. détecter expiration connue ou `401` ;
2. tenter un refresh une seule fois ;
3. rejouer l’appel original une seule fois si refresh réussi ;
4. afficher un message de reconnexion si refresh échoue.

### Refresh token expiré ou révoqué

Comportement attendu :

1. supprimer la session locale ;
2. afficher “Session expirée. Veuillez vous reconnecter.” ;
3. revenir à l’écran de connexion ;
4. ne pas afficher de détail technique sensible.

## Déconnexion

La déconnexion doit :

- révoquer la session côté API si un endpoint officiel existe et est applicable ;
- supprimer les tokens locaux ;
- vider l’état en mémoire ;
- revenir à l’écran de connexion ;
- conserver uniquement les paramètres non sensibles si l’utilisateur le souhaite.

## Sécurité API

### À vérifier avant implémentation

- endpoint token ;
- endpoint revoke ;
- headers requis ;
- scopes ;
- méthode de refresh ;
- exigences PKCE ;
- exigences redirect URI ;
- format de content type ;
- règles de quotas.

### Interdictions

- ne pas utiliser d’API non officielle ;
- ne pas reverse-engineer l’application mobile ;
- ne pas contourner les quotas ;
- ne pas stocker les identifiants Husqvarna utilisateur/mot de passe.

## Checklist sécurité avant release

Avant toute release :

- [ ] Aucun secret dans Git.
- [ ] `.gitignore` protège les fichiers sensibles.
- [ ] Les tokens sont stockés via stockage sécurisé Windows.
- [ ] Les logs masquent les secrets.
- [ ] Les tests vérifient le masquage des logs.
- [ ] Les erreurs OAuth ne révèlent pas les tokens.
- [ ] La déconnexion supprime la session locale.
- [ ] Les permissions OAuth sont minimales.
- [ ] Les endpoints API ont été vérifiés dans la documentation officielle.
- [ ] Les dépendances sont à jour.
- [ ] Aucune télémétrie externe n’est activée.
- [ ] Le niveau debug est désactivé par défaut.

## Règles de contribution

Toute contribution doit :

- éviter les secrets dans les commits ;
- ajouter ou mettre à jour les tests ;
- documenter les nouvelles dépendances ;
- mettre à jour ce fichier si une donnée sensible nouvelle apparaît ;
- ne pas publier de logs bruts dans les issues ou PR ;
- ne pas ajouter de capture écran contenant une Application Key ou un token.

## Réaction en cas de fuite accidentelle

Si un secret est commité :

1. considérer le secret compromis ;
2. le révoquer côté Husqvarna Developer ;
3. supprimer le secret de l’historique si nécessaire ;
4. générer un nouveau secret ;
5. ajouter une règle `.gitignore` ou un test de prévention ;
6. documenter l’incident dans la PR ou issue interne sans republier le secret.
