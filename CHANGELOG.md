# Changelog

Toutes les modifications notables de ce projet sont documentees dans ce fichier.

Le format suit les principes de [Keep a Changelog](https://keepachangelog.com/fr/1.1.0/) et le projet vise le versionnage semantique lorsque les premieres versions seront publiees.

## [Non publie]

### Added

- Cadrage initial du projet.
- Documentation produit.
- Documentation architecture.
- Documentation securite.
- Documentation UX.
- Plan de taches Codex.
- Plan de test.
- Journal initial des decisions techniques.
- Creation de la solution `HusqvarnaAutomowerConnect` et des quatre projets principaux.
- Socle Core : modeles internes, interfaces, service de disponibilite des commandes et orchestration `MowerService`.
- Client Automower V1 : lecture de la liste des robots, lecture d'un robot, envoi des actions officielles confirmees, retry unique sur 401, tests de mapping et d'erreurs API.
- Shell WinUI 3 de navigation avec vues en francais pour la connexion, le tableau de bord, les parametres et le detail robot.
- Tableau de bord branche sur `GET /mowers` avec chargement reel, etat vide et affichage des robots en francais.
- Client OAuth minimal pour le refresh et la revocation, branche via `IHttpClientFactory` avec des clients types.
- Liaison du detail robot a l'identifiant choisi depuis le tableau de bord.
- Ecran `Parametres` temporairement simplifie avec logs de diagnostic detailles pour isoler le defaut d'affichage a l'ouverture.
- Ecrans `Parametres` et `Robot` temporairement passes en lecture seule pour contourner un `COMException` WinUI de mesure lie aux controles de saisie.
- Socle Infrastructure : configuration locale, masquage des secrets, mapping d'erreurs API et stockage securise Windows via `PasswordVault`.
- Shell WinUI 3 minimal en francais pour l'application desktop.
- Tests unitaires initiaux pour la validation de configuration, la disponibilite des commandes, `MowerService` et le masquage des secrets.

### Changed

- Standardisation du nom technique sur `HusqvarnaAutomowerConnect` pour les nouveaux artefacts.
- Mise a jour de `docs/SETUP.md` avec la solution reelle et la version Windows App SDK retenue.
- Mise a jour de `docs/HUSQVARNA_API.md` avec les endpoints, headers et payloads officiellement confirmes.
- Mise a jour de `docs/DECISIONS.md` avec la composition HTTP typee pour Husqvarna.
- Mise a jour de `docs/DECISIONS.md` avec la selection explicite du robot affiche en detail.
- Mise a jour de `README.md` et `docs/SETUP.md` pour donner a un developpeur les prerequis, commandes, chemins locaux et points de vigilance.

### Fixed

- Correction du double-parentage de `secretStatusText` dans l'ecran `Parametres`, qui provoquait un `COMException` au rendu.
- Refonte des vues WinUI pour reconstruire un arbre visuel neuf a chaque rendu et eviter les `COMException` lors des mises a jour de `Parametres`, `Connexion`, `Tableau de bord` et `Robot`.

### Security

- Regles initiales de gestion des secrets.
- Interdiction de logger tokens et secrets.
- Prevision du stockage securise local des tokens.
- Ajout d'un composant de redaction pour `Authorization`, `access_token`, `refresh_token` et `client_secret`.

## [0.1.0] - A venir

### Added

- Premiere version locale prevue :
  - authentification Husqvarna ;
  - liste des robots ;
  - tableau de bord ;
  - commandes principales ;
  - logs locaux securises ;
  - tests de base.

### Changed

- Non applicable.

### Fixed

- Non applicable.

### Security

- Non applicable.

### Added

- Flux OAuth officiel V1 avec ouverture du navigateur sur `GET /oauth2/authorize`, callback local `http://localhost`, et echange `authorization_code`.
- Stockage du secret d'application dans le coffre Windows via `PasswordVault`.
- Connexion locale complete avec enregistrement de la session OAuth et revocation locale.
- L'ecran `Parametres` ouvre desormais une boite de dialogue Windows Forms modale pour saisir la cle d'application, l'URI de redirection, l'intervalle de rafraichissement, le niveau de log et le secret d'application sans reintroduire le crash WinUI.
