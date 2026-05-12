# Spécification produit

## Objectif utilisateur

Permettre à l’utilisateur de superviser et contrôler simplement son robot-tondeuse Husqvarna Automower depuis Windows 11.

L’application doit donner une réponse rapide aux questions suivantes :

- mon robot est-il disponible ?
- que fait-il actuellement ?
- sa batterie est-elle suffisante ?
- y a-t-il une erreur ?
- puis-je le mettre en pause, le renvoyer à la station ou reprendre le planning ?
- la dernière information affichée est-elle récente ?

## Personas

### Persona 1 — Propriétaire particulier

- Possède un robot Husqvarna récent.
- Utilise déjà l’application mobile Automower Connect.
- Veut un accès pratique depuis son PC Windows.
- N’est pas développeur.
- Attend une interface claire en français.

### Persona 2 — Utilisateur technique

- Sait créer une application sur le portail Husqvarna Developer.
- Veut garder le contrôle sur la configuration locale.
- Est attentif à la sécurité des tokens.
- Peut lire des logs si nécessaire.

### Persona 3 — Mainteneur du projet

- Fait évoluer l’application avec Codex.
- A besoin de documentation précise.
- Ne veut pas que l’intégration API soit dispersée dans l’UI.
- Veut des tests pour éviter les régressions.

## Problèmes à résoudre

1. Centraliser les informations du robot dans une interface Windows.
2. Éviter la manipulation répétée de l’application mobile pour les actions simples.
3. Gérer proprement les cas d’erreur API.
4. Protéger les identifiants Husqvarna.
5. Créer une base de code maintenable et testable.
6. Éviter les suppositions dangereuses sur les capacités du robot.

## Fonctionnalités V1

### Authentification

- Connexion OAuth2 au compte Husqvarna.
- Gestion du refresh token.
- Déconnexion.
- Suppression locale de la session à la déconnexion.
- Message clair si l’authentification expire ou est révoquée.

### Configuration

- Saisie de l’Application Key Husqvarna.
- Champ Redirect URI si nécessaire.
- Champ Application Secret uniquement si le flux OAuth retenu l’exige.
- Choix de l’intervalle de rafraîchissement.
- Valeur par défaut recommandée : 60 secondes.
- Validation minimale des champs.

### Tableau de bord

Afficher pour chaque robot :

- nom ;
- identifiant interne non mis en avant ;
- état courant ;
- mode ;
- activité ;
- batterie ;
- erreur éventuelle ;
- dernière mise à jour ;
- position GPS si disponible.

### Commandes

Commandes prévues, uniquement si confirmées par la documentation officielle et supportées :

- pause ;
- retour à la station / parking ;
- reprise du planning ;
- démarrage ou tonte temporaire si disponible ;
- commande indisponible affichée comme désactivée.

Pour chaque commande :

- demander confirmation uniquement si l’action peut perturber le planning ;
- afficher un état “Commande envoyée” si l’API accepte la requête ;
- rafraîchir les données après l’envoi ;
- afficher une erreur claire en cas d’échec.

### Planning

- Afficher le calendrier de tonte si les données sont disponibles sans complexité excessive.
- Ne pas modifier le planning en V1.
- Prévoir l’évolution dans l’architecture.

### Notifications / alertes

En V1 :

- afficher les erreurs importantes dans l’application.

En V1.1 possible :

- notification Windows pour :
  - robot bloqué ;
  - erreur critique ;
  - batterie faible ;
  - tonte terminée.

### Journalisation

- Logs locaux pour diagnostic.
- Niveau debug désactivé par défaut.
- Aucun token ou secret dans les logs.
- Logs utiles pour :
  - statut HTTP ;
  - identifiant de requête si disponible ;
  - catégorie d’erreur ;
  - timestamp ;
  - durée d’appel.

## Fonctionnalités V1.1 / V2

### V1.1

- Notifications Windows.
- Meilleurs détails d’erreur.
- Affichage plus lisible du planning.
- Export manuel de logs nettoyés.
- Détection visuelle de commandes non supportées.

### V2

- Modification du planning.
- Gestion des zones de travail si supportée.
- Cartographie GPS plus avancée.
- Multi-comptes si besoin réel.
- Packaging installable.
- Publication éventuelle.

## Hors périmètre

- Automatisation cloud propre à l’application.
- Contrôle Bluetooth direct.
- Reverse engineering de l’application mobile.
- Utilisation d’API non officielles.
- Contournement des quotas.
- Support autre que Windows 11.
- Gestion professionnelle d’une flotte.
- Modification du calendrier en V1.
- Stockage de secrets en clair.

## Priorités

### Priorité haute

- Authentification sécurisée.
- Récupération des robots.
- Tableau de bord fiable.
- Gestion des erreurs.
- Tests de mapping et erreurs.
- Absence de fuite de secrets.

### Priorité moyenne

- Commandes principales.
- Configuration de l’intervalle de refresh.
- Logs locaux.
- Affichage simple du planning.

### Priorité basse

- Notifications Windows.
- Détails visuels avancés.
- Packaging.
- Modification de planning.

## Critères d’acceptation V1

### Authentification

- L’utilisateur peut lancer une connexion OAuth2.
- La session est restaurée après redémarrage si le refresh token est valide.
- La déconnexion supprime les tokens locaux.
- Si le refresh échoue, l’utilisateur est invité à se reconnecter.

### Tableau de bord

- Si des robots existent, ils sont listés.
- Si aucun robot n’existe, un état vide clair est affiché.
- Les champs absents ne provoquent pas de crash.
- La date de dernière mise à jour est visible.
- L’état hors ligne est visible.

### Commandes

- Les commandes sont désactivées si l’utilisateur n’est pas connecté.
- Les commandes indisponibles sont désactivées ou masquées avec explication.
- Une commande acceptée affiche un feedback.
- Une commande échouée affiche un message clair.

### Sécurité

- Aucun secret n’est présent dans le dépôt.
- Aucun token n’est écrit dans les logs.
- `.gitignore` protège les fichiers locaux sensibles.
- Les tests couvrent le masquage des données sensibles dans les logs.

### Tests

- `dotnet test` passe.
- Les mappings API principaux sont testés.
- Les erreurs 401, 403, 429, 500 et réseau sont testées.
- Les cas “aucun robot” et “données absentes” sont testés.
