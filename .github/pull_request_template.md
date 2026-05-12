# Pull Request

## Résumé

Décrire brièvement l’objectif de cette PR.

## Changements

- 
- 
- 

## Type de changement

- [ ] Fonctionnalité
- [ ] Correction
- [ ] Refactor
- [ ] Documentation
- [ ] Tests
- [ ] Sécurité
- [ ] Build / tooling

## Tests effectués

Commandes exécutées :

- [ ] `dotnet restore`
- [ ] `dotnet build ./src/MowerControl.sln`
- [ ] `dotnet test ./src/MowerControl.sln`
- [ ] `dotnet format ./src/MowerControl.sln --verify-no-changes`

Tests manuels :

- [ ] Lancement Windows 11
- [ ] Connexion
- [ ] Déconnexion
- [ ] Tableau de bord
- [ ] Commandes robot
- [ ] Cas d’erreur

Détails :

```text
Ajouter ici les résultats utiles.
```

## Impact sécurité

- [ ] Aucun secret ajouté.
- [ ] Aucun token loggé.
- [ ] Les logs restent nettoyés.
- [ ] Les permissions OAuth ne changent pas.
- [ ] La gestion des tokens reste sécurisée.
- [ ] `docs/SECURITY.md` mis à jour si nécessaire.

Commentaires sécurité :

```text
Indiquer tout point important.
```

## Impact UX

- [ ] Libellés en français.
- [ ] États d’erreur clairs.
- [ ] États vides gérés.
- [ ] Commandes indisponibles expliquées.
- [ ] Accessibilité minimale respectée.
- [ ] `docs/UX_SPEC.md` mis à jour si nécessaire.

## Impact API Husqvarna

- [ ] Aucun endpoint ajouté.
- [ ] Endpoint vérifié dans la documentation officielle.
- [ ] Payload vérifié dans la documentation officielle.
- [ ] Erreurs API gérées.
- [ ] Tests avec API mockée ajoutés ou mis à jour.

Détails :

```text
Préciser les endpoints ou comportements concernés.
```

## Documentation

- [ ] README mis à jour si nécessaire.
- [ ] Docs dans `/docs` mises à jour si nécessaire.
- [ ] CHANGELOG mis à jour si changement utilisateur visible.
- [ ] DECISIONS mis à jour si décision technique.

## Checklist finale

- [ ] Le périmètre V1 est respecté.
- [ ] Le code compile.
- [ ] Les tests pertinents passent.
- [ ] Les nouvelles logiques métier sont testées.
- [ ] Aucune donnée sensible n’est exposée.
- [ ] Les erreurs réseau/API sont gérées.
- [ ] La PR ne mélange pas des changements sans rapport.
