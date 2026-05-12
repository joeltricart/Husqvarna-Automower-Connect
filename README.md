# Husqvarna Automower Connect

Application Windows 11 de controle et supervision d'un robot-tondeuse Husqvarna Automower via l'API officielle Husqvarna Developer / Automower Connect.

## Ce que contient le depot

- le code source de l'application WinUI 3 ;
- les projets `Core`, `Infrastructure`, `App` et `Tests` ;
- la documentation de produit, d'architecture, de securite et de tests ;
- les fichiers de configuration exemple.

## Ce que le depot ne contient pas

- les binaires generes (`bin/`, `obj/`) ;
- les secrets personnels ;
- la configuration locale de ta machine ;
- les jetons OAuth ;
- le dossier `%LOCALAPPDATA%\HusqvarnaAutomowerConnect`, qui est cree localement apres execution.

## Pre-requis pour un developpeur

- Windows 11 ;
- .NET SDK 8 ou superieur ;
- Visual Studio 2022 recent avec le workload Windows App SDK / WinUI 3 ;
- acces au portail Husqvarna Developer ;
- compte Husqvarna possedant des robots visibles dans Automower Connect ;
- API a connecter dans le portail :
  - Authentication API ;
  - Automower Connect API.

## Lancer le projet en local

```powershell
git clone <url-du-repo>
cd Husqvarna-Automower-Connect
dotnet restore
dotnet test .\src\HusqvarnaAutomowerConnect.sln
dotnet build .\src\HusqvarnaAutomowerConnect.sln -c Debug
```

Puis ouvrir `src/HusqvarnaAutomowerConnect.sln` dans Visual Studio et lancer `HusqvarnaAutomowerConnect.App`.

## Dossier local genere par l'application

Au premier lancement, l'application ecrit ses donnees locales ici :

```text
%LOCALAPPDATA%\HusqvarnaAutomowerConnect
```

Contenu attendu :

- `logs\startup.log` pour les diagnostics locaux ;
- `appsettings.Local.json` ou l'equivalent local selon la configuration ;
- le stockage securise Windows pour les secrets et jetons ;
- les binaires de build local.

Ce dossier reste local a la machine et ne doit pas etre versionne.

## Configuration Husqvarna Developer

Dans le portail Husqvarna Developer :

1. creer une application ;
2. renseigner l'Application Key ;
3. connecter au minimum :
   - Authentication API ;
   - Automower Connect API ;
4. verifier la redirect URI acceptee par le portail ;
5. regenerer l'Application Secret si elle a ete exposee.

## Configuration locale

Le minimum attendu est :

```json
{
  "Husqvarna": {
    "ApplicationKey": "",
    "RedirectUri": "http://localhost",
    "RefreshIntervalSeconds": 60
  },
  "Logging": {
    "MinimumLevel": "Information"
  }
}
```

Ne pas commiter un fichier local contenant une vraie Application Key.

## Securite

- ne jamais commiter d'Application Secret ;
- ne jamais commiter d'access token ou de refresh token ;
- ne jamais logger de valeurs sensibles ;
- stocker les jetons uniquement via un stockage Windows securise ;
- supprimer la session locale a la deconnexion.

## Etat du projet

La V1 couvre :

- connexion Husqvarna ;
- tableau de bord des robots ;
- detail robot ;
- commandes principales ;
- configuration locale ;
- logs locaux ;
- tests unitaires.

Pour les instructions completes de setup et de diagnostic, voir [docs/SETUP.md](docs/SETUP.md).
