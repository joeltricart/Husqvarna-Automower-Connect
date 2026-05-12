# Setup developpeur

## Objectif

Preparer un environnement Windows 11 pour developper et executer `Husqvarna Automower Connect`.

## Ce qu'un developpeur doit avoir

- Windows 11 a jour ;
- .NET SDK 8 ou superieur ;
- Visual Studio 2022 recent ;
- le workload Desktop .NET ;
- les composants Windows App SDK / WinUI 3 ;
- acces Internet ;
- un compte Husqvarna ;
- un robot visible dans Automower Connect ;
- acces au portail Husqvarna Developer.

## Verifications rapides

```powershell
dotnet --list-sdks
git --version
```

Si `dotnet --list-sdks` ne retourne rien, le SDK .NET n'est pas installe.

## Portail Husqvarna Developer

Dans le portail :

1. creer une nouvelle application ;
2. renseigner l'Application Key ;
3. connecter au minimum :
   - Authentication API ;
   - Automower Connect API ;
4. verifier la redirect URI acceptee ;
5. regenerer l'Application Secret si elle a deja ete exposee.

Important :

- le depot ne doit jamais contenir de secret ;
- le secret est stocke localement dans Windows ;
- l'Application Key reste dans la configuration locale.

## Configuration locale

Le fichier local attendu est :

```text
appsettings.Local.json
```

Exemple :

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

Ce fichier ne doit pas etre commit.

## Dossier local de l'application

L'application ecrit ses donnees locales dans :

```text
%LOCALAPPDATA%\HusqvarnaAutomowerConnect
```

Contenu typique :

- `logs\startup.log` ;
- les fichiers locaux de configuration ;
- les binaires de build ;
- le stockage securise Windows pour les jetons et secrets.

Ce dossier est local a la machine et ne doit pas etre versionne.

## Commandes utiles

### Restauration

```powershell
dotnet restore .\src\HusqvarnaAutomowerConnect.sln
```

### Build

```powershell
dotnet build .\src\HusqvarnaAutomowerConnect.sln -c Debug
```

### Tests

```powershell
dotnet test .\src\HusqvarnaAutomowerConnect.sln -c Debug
```

### Lancement dans Visual Studio

1. ouvrir `src/HusqvarnaAutomowerConnect.sln` ;
2. definir `HusqvarnaAutomowerConnect.App` comme projet de demarrage ;
3. lancer en Debug.

## Lancement local apres build

Le binaire de developpement est ecrit sous un dossier local du profil utilisateur, par exemple :

```text
%LOCALAPPDATA%\HusqvarnaAutomowerConnect\bin\HusqvarnaAutomowerConnect.App\Debug\net8.0-windows10.0.19041.0\win-x64
```

Le chemin exact peut varier selon la configuration du SDK et la machine.

## Problemes frequents

### SDK .NET absent

Symptome :

- `dotnet` existe, mais aucun SDK n'est liste ;
- `dotnet restore`, `dotnet build` ou `dotnet test` echouent.

Solution :

- installer le SDK .NET 8 ou superieur ;
- relancer `dotnet --list-sdks`.

### Windows App SDK absent

Symptome :

- erreurs de compilation WinUI 3 ;
- runtime Windows App SDK manquant.

Solution :

- verifier les composants Visual Studio ;
- installer ou reparer Windows App SDK ;
- recompresser/restaurer le projet.

### Crash au rendu WinUI

Symptome :

- ecran vide ;
- fermeture de la fenetre ;
- `COMException` dans les logs.

Solution :

- verifier `%LOCALAPPDATA%\HusqvarnaAutomowerConnect\logs\startup.log` ;
- reproduire une seule action a la fois ;
- verifier que l'action est executee sur le thread UI ;
- reconstruire la vue plutot que muter des controles deja montes.

### Authentification impossible

Causes possibles :

- Application Key incorrecte ;
- API non connectee dans le portail ;
- redirect URI non conforme ;
- secret local absent ;
- jeton local absent ou expire.

Actions :

- verifier le portail Husqvarna Developer ;
- supprimer la session locale ;
- relancer la connexion.

### Erreur 403

Causes possibles :

- API Automower Connect non connectee ;
- headers officiels incorrects ;
- compte non autorise.

Action :

- verifier les headers et le portail.

### Erreur 429

Cause :

- trop de requetes.

Action :

- augmenter l'intervalle de rafraichissement ;
- verifier qu'un seul rafraichissement tourne.

## Rappel securite

- ne jamais commiter les secrets ;
- ne jamais commiter les tokens ;
- ne jamais logger de valeurs sensibles ;
- ne jamais utiliser `%LOCALAPPDATA%\HusqvarnaAutomowerConnect` comme source de verite du depot.
