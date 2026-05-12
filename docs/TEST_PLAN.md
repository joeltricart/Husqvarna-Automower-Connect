# Plan de test

## Objectif

Garantir que `Husqvarna Automower Connect` est stable, sécurisé et utilisable pour une V1 locale Windows 11.

## Niveaux de test

1. Tests unitaires.
2. Tests d’intégration avec API mockée.
3. Tests ViewModel.
4. Tests manuels Windows.
5. Vérifications sécurité.
6. Checklist avant release.

## Tests unitaires

### Core

À tester :

- création des modèles ;
- validation batterie ;
- validation position GPS ;
- mapping des enums inconnues vers `Unknown` ;
- disponibilité des commandes ;
- résultat de commande ;
- comportement avec données absentes.

Scénarios :

- robot complet ;
- robot sans nom ;
- robot sans batterie ;
- robot hors ligne ;
- robot en erreur ;
- commande non supportée.

### Services

À tester :

- `MowerService` avec liste vide ;
- `MowerService` avec erreur API ;
- envoi de commande accepté ;
- envoi de commande refusé ;
- propagation propre des erreurs ;
- absence de crash sur données nullables.

### Sécurité

À tester :

- masquage `access_token` ;
- masquage `refresh_token` ;
- masquage `client_secret` ;
- masquage header `Authorization` ;
- suppression session ;
- absence de token dans message utilisateur.

## Tests d’intégration avec API mockée

Utiliser un serveur mock ou un handler HTTP mocké.

### Scénario 1 — Liste robots réussie

Mock :

- réponse `200` avec un robot complet.

Attendu :

- robot affichable ;
- batterie mappée ;
- état mappé ;
- aucune erreur.

### Scénario 2 — Aucun robot

Mock :

- réponse `200` avec liste vide.

Attendu :

- état vide ;
- aucune exception ;
- message utilisateur correct.

### Scénario 3 — Données partielles

Mock :

- robot sans position ;
- batterie absente ;
- état inconnu.

Attendu :

- champs “Non disponible” côté UI ;
- enum `Unknown` ;
- pas de crash.

### Scénario 4 — Token expiré

Mock :

- premier appel `401` ;
- refresh token réussi ;
- second appel `200`.

Attendu :

- refresh effectué une seule fois ;
- appel rejoué ;
- session mise à jour.

### Scénario 5 — Refresh échoué

Mock :

- appel `401` ;
- refresh refusé.

Attendu :

- session locale supprimée ;
- message reconnexion ;
- pas de boucle.

### Scénario 6 — Rate limit

Mock :

- réponse `429`.

Attendu :

- message “Trop de requêtes” ;
- refresh ralenti ;
- pas de retry agressif.

### Scénario 7 — Service indisponible

Mock :

- réponse `503`.

Attendu :

- message clair ;
- dernières données conservées si disponibles.

### Scénario 8 — Commande acceptée

Mock :

- `POST /actions` retourne `202`.

Attendu :

- feedback “Commande envoyée” ;
- rafraîchissement planifié.

### Scénario 9 — Commande indisponible

Mock ou règle métier :

- action non supportée.

Attendu :

- bouton désactivé ;
- aucun appel API envoyé.

## Tests ViewModel

À tester sans WinUI réel si possible :

- état initial ;
- chargement ;
- erreur réseau ;
- état vide ;
- rafraîchissement ;
- commande acceptée ;
- commande refusée ;
- déconnexion.

## Tests manuels Windows

À exécuter sur Windows 11.

### Installation / lancement

- [ ] L’application démarre depuis Visual Studio.
- [ ] L’application démarre en configuration Debug.
- [ ] L’application démarre en configuration Release.
- [ ] Aucun crash au premier lancement.

### Configuration

- [ ] Application Key vide refusée.
- [ ] Intervalle invalide refusé.
- [ ] Paramètres sauvegardés.
- [ ] Paramètres restaurés au redémarrage.
- [ ] Aucun secret visible dans les fichiers du dépôt.

### Authentification

- [ ] Connexion Husqvarna possible.
- [ ] Annulation de connexion gérée.
- [ ] Session restaurée après redémarrage.
- [ ] Déconnexion supprime la session.
- [ ] Session expirée déclenche reconnexion.

### Tableau de bord

- [ ] Robots affichés.
- [ ] Aucun robot affiché proprement.
- [ ] Dernière mise à jour visible.
- [ ] Données absentes affichées comme “Non disponible”.
- [ ] Bouton rafraîchir fonctionne.
- [ ] Hors ligne affiché proprement.

### Commandes

- [ ] Pause fonctionne si supportée.
- [ ] Retour station fonctionne si supporté.
- [ ] Reprise planning fonctionne si supportée.
- [ ] Tonte temporaire fonctionne si supportée.
- [ ] Commandes indisponibles désactivées.
- [ ] Feedback visible après commande.
- [ ] Échec commande affiché proprement.

### Logs

- [ ] Logs créés localement.
- [ ] Aucun token dans les logs.
- [ ] Niveau debug désactivé par défaut.
- [ ] Erreurs API traçables sans secret.

## Cas d’erreur critiques

À couvrir :

- pas d’Internet ;
- DNS impossible ;
- timeout ;
- `401` ;
- `403` ;
- `404` ;
- `415` ;
- `429` ;
- `500` ;
- `503` ;
- réponse JSON invalide ;
- réponse JSON avec champs manquants ;
- robot absent ;
- robot hors ligne ;
- commande non supportée ;
- stockage sécurisé indisponible ;
- fichier de configuration invalide.

## Vérifications sécurité

Avant release :

- [ ] Recherche globale de `access_token`.
- [ ] Recherche globale de `refresh_token`.
- [ ] Recherche globale de `client_secret`.
- [ ] Recherche globale de vraies clés personnelles.
- [ ] Inspection `.gitignore`.
- [ ] Inspection logs générés.
- [ ] Déconnexion testée.
- [ ] Refresh token révoqué ou invalide testé.
- [ ] Pas de télémétrie externe.
- [ ] Pas de mot de passe Husqvarna demandé directement.

## Commandes de test

Commandes cibles :

```bash
dotnet restore
dotnet build ./src/HusqvarnaAutomowerConnect.sln --configuration Debug
dotnet test ./src/HusqvarnaAutomowerConnect.sln --configuration Debug
dotnet format ./src/HusqvarnaAutomowerConnect.sln --verify-no-changes
```

Si WinUI impose une commande différente, la documenter dans `docs/SETUP.md`.

## Checklist avant release V1

- [ ] Build Debug OK.
- [ ] Build Release OK.
- [ ] Tests unitaires OK.
- [ ] Tests d’intégration mockée OK.
- [ ] Tests manuels Windows exécutés.
- [ ] README à jour.
- [ ] SETUP à jour.
- [ ] CHANGELOG à jour.
- [ ] Sécurité validée.
- [ ] Aucun secret dans Git.
- [ ] Fonctionnalités V1 uniquement.
- [ ] Limites connues documentées.
