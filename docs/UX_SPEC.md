# Spécification UX

## Objectif UX

Fournir une application Windows 11 simple, claire et rassurante pour superviser un robot Husqvarna Automower.

L’utilisateur doit comprendre en moins de quelques secondes :

- s’il est connecté ;
- quels robots sont disponibles ;
- ce que fait le robot ;
- si une action est possible ;
- si une erreur demande son attention.

## Langue

Tous les libellés visibles doivent être en français.

Aucun libellé technique anglais ne doit apparaître dans l’interface finale sauf nom officiel incontournable.

## Navigation

Navigation simple :

```text
Connexion / Configuration
  -> Tableau de bord
    -> Détail robot
    -> Paramètres
    -> Logs diagnostic facultatifs
```

## Écrans nécessaires

### 1. Écran de connexion

Objectif :

- guider l’utilisateur vers la connexion Husqvarna.

Éléments :

- titre : “Connexion Husqvarna”
- texte : “Connectez votre compte Husqvarna Automower Connect pour afficher vos robots.”
- bouton principal : “Se connecter”
- lien ou bouton secondaire : “Configurer l’accès développeur”
- état si configuration incomplète : “Configuration Husqvarna incomplète.”

Messages :

- “Connexion en cours…”
- “Connexion réussie.”
- “Connexion annulée.”
- “Impossible de se connecter. Vérifiez la configuration Husqvarna.”

### 2. Écran paramètres

Objectif :

- saisir les paramètres nécessaires.

Champs :

- “Application Key Husqvarna”
- “URI de redirection”
- “Intervalle de rafraîchissement”
- “Niveau de journalisation”

Optionnel si requis par le flux OAuth :

- “Application Secret”

Boutons :

- “Enregistrer”
- “Tester la configuration”
- “Réinitialiser”

Textes d’aide :

- “Créez une application dans le portail Husqvarna Developer.”
- “Connectez les API Authentication API et Automower Connect API.”
- “Ne partagez jamais votre secret d’application.”

Validations :

- Application Key requise.
- Intervalle minimum recommandé : 30 secondes.
- Intervalle par défaut : 60 secondes.
- Afficher un avertissement si l’intervalle est trop court.

### 3. Tableau de bord

Objectif :

- afficher une synthèse des robots.

Éléments globaux :

- état connexion ;
- bouton “Rafraîchir” ;
- dernière mise à jour globale ;
- accès paramètres.

Carte robot :

- nom ;
- état ;
- batterie ;
- activité ;
- mode ;
- erreur ;
- dernière mise à jour ;
- bouton “Voir le détail”.

Libellés :

- “État”
- “Batterie”
- “Activité”
- “Mode”
- “Erreur”
- “Dernière mise à jour”
- “Position”

### 4. Détail robot

Objectif :

- afficher plus d’informations et les commandes.

Sections :

- résumé ;
- batterie ;
- état courant ;
- erreurs ;
- position GPS si disponible ;
- planning si disponible ;
- commandes.

Boutons de commande :

- “Mettre en pause”
- “Retour à la station”
- “Reprendre le planning”
- “Tondre temporairement”

Chaque bouton doit être :

- activé uniquement si l’action est disponible ;
- désactivé avec explication si non disponible ;
- accompagné d’un feedback après clic.

### 5. Logs diagnostic

Écran facultatif en V1.

Objectif :

- permettre un diagnostic local sans exposer de secrets.

Éléments :

- liste des événements récents ;
- niveau de log actuel ;
- bouton “Ouvrir le dossier des logs” ;
- message : “Les secrets et tokens sont masqués automatiquement.”

## États vides

### Aucun robot

Titre :

```text
Aucun robot trouvé
```

Texte :

```text
Aucun robot n’est associé à ce compte Husqvarna. Vérifiez que votre robot est visible dans l’application Automower Connect.
```

Actions :

- “Rafraîchir”
- “Se déconnecter”
- “Ouvrir les paramètres”

### Non connecté

Titre :

```text
Non connecté
```

Texte :

```text
Connectez votre compte Husqvarna pour afficher vos robots.
```

Action :

- “Se connecter”

### Donnée absente

Libellé :

```text
Non disponible
```

Ne jamais afficher `null`, `undefined`, `NaN` ou une exception brute.

## États d’erreur

### Erreur réseau

```text
Connexion impossible. Vérifiez votre accès Internet puis réessayez.
```

### Session expirée

```text
Session expirée. Veuillez vous reconnecter.
```

### Accès refusé

```text
Accès refusé. Vérifiez que l’application Husqvarna Developer est bien connectée aux API nécessaires.
```

### Trop de requêtes

```text
Trop de requêtes envoyées à Husqvarna. Le rafraîchissement est temporairement ralenti.
```

### Service indisponible

```text
Service Husqvarna temporairement indisponible. Réessayez plus tard.
```

### Commande indisponible

```text
Cette commande n’est pas disponible pour ce robot ou dans son état actuel.
```

### Erreur robot

```text
Le robot signale une erreur. Consultez le détail pour plus d’informations.
```

## Feedback après commande

### Commande envoyée

```text
Commande envoyée. L’état du robot sera mis à jour après le prochain rafraîchissement.
```

### Commande acceptée mais état inchangé

```text
Commande acceptée par l’API. Le robot peut mettre quelques instants à changer d’état.
```

### Commande échouée

```text
La commande n’a pas pu être envoyée. Réessayez ou consultez les logs.
```

## Boutons et libellés

### Navigation

- “Tableau de bord”
- “Paramètres”
- “Se connecter”
- “Se déconnecter”
- “Rafraîchir”
- “Retour”

### Commandes robot

- “Mettre en pause”
- “Retour à la station”
- “Reprendre le planning”
- “Tondre temporairement”

### Statuts génériques

- “Connecté”
- “Non connecté”
- “Chargement…”
- “Dernière mise à jour :”
- “Non disponible”
- “Erreur”
- “Aucune erreur connue”

## Accessibilité minimale

- Contraste suffisant.
- Navigation clavier possible.
- Boutons désactivés accompagnés d’une explication accessible.
- Textes non basés uniquement sur la couleur.
- Taille de police lisible.
- Indicateur de chargement avec texte.
- Nom accessible pour chaque bouton.
- Éviter les icônes seules sans libellé.

## Comportement hors ligne

Si l’application est hors ligne :

- conserver le dernier état connu en lecture seule ;
- afficher clairement que les données ne sont peut-être plus à jour ;
- désactiver les commandes ;
- proposer “Réessayer”.

Message :

```text
Hors ligne. Les dernières données connues sont affichées, mais les commandes sont désactivées.
```

## Rafraîchissement

- Rafraîchissement manuel via bouton.
- Rafraîchissement automatique selon intervalle.
- Ne pas lancer un nouveau rafraîchissement si le précédent est encore en cours.
- Afficher “Rafraîchissement…” pendant l’appel.
- En cas d’erreur, garder les dernières données connues.

## Notifications Windows

### V1

Les alertes peuvent rester dans l’application.

### V1.1

Prévoir notifications pour :

- robot bloqué ;
- erreur critique ;
- batterie faible ;
- tonte terminée.

Chaque notification doit être activable/désactivable dans les paramètres si implémentée.
