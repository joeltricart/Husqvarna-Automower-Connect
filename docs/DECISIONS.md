# DÃ©cisions techniques

Format ADR lÃ©ger.

Chaque dÃ©cision doit contenir :

- statut ;
- contexte ;
- dÃ©cision ;
- consÃ©quences positives ;
- consÃ©quences nÃ©gatives ;
- date.

## ADR-001 â€” Utiliser WinUI 3 pour lâ€™application Windows

Statut : acceptÃ©  
Date : initiale

### Contexte

Le projet cible Windows 11 et doit fournir une application desktop moderne.

### DÃ©cision

Utiliser WinUI 3 avec Windows App SDK.

### ConsÃ©quences positives

- interface Windows moderne ;
- intÃ©gration cohÃ©rente Windows 11 ;
- compatibilitÃ© MVVM ;
- bonne intÃ©gration Visual Studio.

### ConsÃ©quences nÃ©gatives

- environnement de build plus exigeant quâ€™une application console ;
- packaging Windows potentiellement plus complexe ;
- tests UI automatisÃ©s non prioritaires en V1.

## ADR-002 â€” Utiliser C# / .NET 8 ou supÃ©rieur

Statut : acceptÃ©  
Date : initiale

### Contexte

Lâ€™application cible Windows et doit Ãªtre maintenable.

### DÃ©cision

Utiliser C# et .NET 8 ou supÃ©rieur.

### ConsÃ©quences positives

- stack stable ;
- trÃ¨s bon support Windows ;
- outillage mature ;
- tests unitaires simples Ã  mettre en place.

### ConsÃ©quences nÃ©gatives

- dÃ©pendance Ã  lâ€™Ã©cosystÃ¨me .NET ;
- certains aspects WinUI nÃ©cessitent Visual Studio ou un tooling spÃ©cifique.

## ADR-003 â€” Architecture MVVM

Statut : acceptÃ©  
Date : initiale

### Contexte

Lâ€™application doit rester testable et Ã©volutive.

### DÃ©cision

Utiliser MVVM avec sÃ©paration vues, ViewModels, services et infrastructure.

### ConsÃ©quences positives

- logique UI testable ;
- sÃ©paration claire des responsabilitÃ©s ;
- moins de code-behind ;
- Ã©volutions plus simples.

### ConsÃ©quences nÃ©gatives

- structure initiale plus longue Ã  crÃ©er ;
- nÃ©cessite discipline sur les responsabilitÃ©s.

## ADR-004 â€” Stockage sÃ©curisÃ© local des tokens

Statut : acceptÃ©  
Date : initiale

### Contexte

Lâ€™application manipule des tokens OAuth Husqvarna.

### DÃ©cision

Stocker les tokens via un mÃ©canisme Windows sÃ©curisÃ©, derriÃ¨re `ISecureTokenStore`.

### ConsÃ©quences positives

- meilleure protection des tokens ;
- abstraction testable ;
- remplacement possible de lâ€™implÃ©mentation.

### ConsÃ©quences nÃ©gatives

- dÃ©pendance Ã  Windows ;
- tests automatisÃ©s nÃ©cessitant des mocks ;
- comportements diffÃ©rents selon packaging Ã©ventuel.

## ADR-005 â€” Client API isolÃ©

Statut : acceptÃ©  
Date : initiale

### Contexte

Lâ€™intÃ©gration Husqvarna ne doit pas Ãªtre dispersÃ©e dans lâ€™UI.

### DÃ©cision

CrÃ©er un client dÃ©diÃ© `IHusqvarnaApiClient` implÃ©mentÃ© dans `HusqvarnaAutomowerConnect.Infrastructure`.

### ConsÃ©quences positives

- HTTP centralisÃ© ;
- headers et erreurs centralisÃ©s ;
- tests mockÃ©s plus simples ;
- changement API plus facile Ã  absorber.

### ConsÃ©quences nÃ©gatives

- nÃ©cessite des DTO et mappers ;
- un peu plus de code initial.

## ADR-006 â€” Tests avec API mockÃ©e

Statut : acceptÃ©  
Date : initiale

### Contexte

Lâ€™API Husqvarna dÃ©pend dâ€™un compte, de robots rÃ©els et de quotas.

### DÃ©cision

Tester les flux API avec HTTP mockÃ© au lieu de dÃ©pendre de lâ€™API rÃ©elle en CI.

### ConsÃ©quences positives

- tests dÃ©terministes ;
- pas de secret en CI ;
- pas de consommation de quotas ;
- scÃ©narios dâ€™erreur faciles Ã  couvrir.

### ConsÃ©quences nÃ©gatives

- ne remplace pas les tests manuels avec compte rÃ©el ;
- les mocks doivent rester synchronisÃ©s avec la documentation officielle.

## ADR-007 â€” Modification du planning hors V1

Statut : acceptÃ©  
Date : initiale

### Contexte

La modification du calendrier peut remplacer lâ€™ensemble du planning cÃ´tÃ© API et comporte un risque de mauvaise manipulation.

### DÃ©cision

Ne pas modifier le planning en V1. Autoriser uniquement un affichage simple si les donnÃ©es sont disponibles.

### ConsÃ©quences positives

- V1 plus sÃ»re ;
- risque rÃ©duit de modifier involontairement le planning utilisateur ;
- complexitÃ© initiale rÃ©duite.

### ConsÃ©quences nÃ©gatives

- fonctionnalitÃ© incomplÃ¨te pour les utilisateurs avancÃ©s ;
- besoin de reprendre le sujet en V1.1/V2.

## ADR-008 â€” Standardiser le nom technique sur HusqvarnaAutomowerConnect

Statut : acceptÃ©  
Date : 2026-05-12

### Contexte

La documentation initiale emploie largement le nom `MowerControl`, tandis que la consigne utilisateur impose de ne retenir que le nom de projet â€œHusqvarna Automower Connectâ€.

### DÃ©cision

Utiliser `Husqvarna Automower Connect` comme nom produit et `HusqvarnaAutomowerConnect` comme identifiant technique pour la solution, les projets et les espaces de noms nouvellement crÃ©Ã©s.

### ConsÃ©quences positives

- cohÃ©rence avec la consigne utilisateur ;
- nommage explicite et directement liÃ© au produit ;
- moins dâ€™ambiguÃ¯tÃ© entre documentation et code livrÃ©.

### ConsÃ©quences nÃ©gatives

- la documentation initiale doit Ãªtre progressivement alignÃ©e ;
- certains exemples historiques restent encore rÃ©digÃ©s avec `MowerControl`.

## ADR-009 â€” DÃ©marrer la V1 avec un shell WinUI 3 non packagÃ©

Statut : acceptÃ©  
Date : 2026-05-12

### Contexte

La V1 vise un usage local Windows 11 et doit devenir exÃ©cutable rapidement sans ajouter dÃ¨s le dÃ©part la complexitÃ© complÃ¨te du packaging MSIX.

### DÃ©cision

Initialiser le projet `HusqvarnaAutomowerConnect.App` comme application WinUI 3 .NET 8 non packagÃ©e avec `WindowsPackageType=None`, tout en conservant `EnableMsixTooling=true` pour permettre une Ã©volution ultÃ©rieure.

### ConsÃ©quences positives

- dÃ©marrage plus simple pour la V1 locale ;
- moins de friction pendant la phase dâ€™implÃ©mentation ;
- possibilitÃ© de revenir vers un packaging plus tard.

### ConsÃ©quences nÃ©gatives

- certains comportements Windows peuvent diffÃ©rer dâ€™une application packagÃ©e ;
- les tests manuels devront valider explicitement le stockage sÃ©curisÃ© dans ce mode.

## ADR-010 â€” Retenir Windows App SDK stable 1.8.6 pour lâ€™initialisation

Statut : acceptÃ©  
Date : 2026-05-12

### Contexte

Le dÃ©pÃ´t demande de vÃ©rifier la version stable actuelle compatible avec .NET 8 avant implÃ©mentation.

### DÃ©cision

Retenir `Microsoft.WindowsAppSDK` `1.8.260317003` (Windows App SDK `1.8.6`), version stable listÃ©e par Microsoft Learn au 18 mars 2026.

### ConsÃ©quences positives

- base stable et actuelle pour le shell WinUI 3 ;
- cohÃ©rence avec la documentation Microsoft consultÃ©e ;
- pas de dÃ©pendance Ã  une version preview.

### ConsÃ©quences nÃ©gatives

- la version devra Ãªtre revue si Microsoft publie un correctif critique avant la premiÃ¨re release locale.

## ADR-011 â€” ImplÃ©menter le stockage sÃ©curisÃ© V1 avec PasswordVault

Statut : acceptÃ©  
Date : 2026-05-12

### Contexte

Le dÃ©pÃ´t impose un stockage sÃ©curisÃ© Windows pour les tokens et demande que le choix exact soit documentÃ©.

### DÃ©cision

Utiliser `Windows.Security.Credentials.PasswordVault` pour la V1, derriÃ¨re `ISecureTokenStore`.

### ConsÃ©quences positives

- composant natif Windows ;
- aucune dÃ©pendance tierce supplÃ©mentaire ;
- alignement direct avec les contraintes de sÃ©curitÃ© du dÃ©pÃ´t.

### ConsÃ©quences nÃ©gatives

- comportement Ã  valider explicitement en mode non packagÃ© ;
- tests automatisÃ©s limitÃ©s Ã  des doubles ou Ã  des tests manuels Windows.

## ADR-012 â€” Le client Automower rÃ©utilise la session locale et ne rejoue quâ€™un seul refresh

Statut : acceptÃ©  
Date : 2026-05-12

### Contexte

Lâ€™API Automower impose un `Authorization: Bearer <token>` et la V1 doit gÃ©rer proprement le cas du jeton expirÃ© sans boucler indÃ©finiment.

### DÃ©cision

Faire charger au client API la configuration et la session locale, rÃ©aliser un refresh automatique si aucun access token utilisable nâ€™est prÃ©sent, puis nâ€™autoriser quâ€™un seul retry supplÃ©mentaire aprÃ¨s un `401`.

### ConsÃ©quences positives

- comportement prÃ©visible en cas dâ€™expiration de token ;
- pas de boucle de rafraÃ®chissement ;
- le client reste testable avec HTTP mockÃ© ;
- la logique dâ€™authentification reste centralisÃ©e.

### ConsÃ©quences nÃ©gatives

- le client API dÃ©pend de lâ€™Ã©tat de session et du refresh ;
- un flux auth cassÃ© peut bloquer les appels API jusquâ€™Ã  reconnexion ;
- la rÃ©cupÃ©ration dâ€™un nouveau jeton se fait avant les appels mÃ©tier.
## ADR-013 â€” DÃ©duire la disponibilitÃ© des commandes Ã  partir de lâ€™Ã©tat et de la connectivitÃ©

Statut : acceptÃ©  
Date : 2026-05-12

### Contexte

Le swagger Automower confirme les actions disponibles, mais ne fournit pas de champ universel indiquant clairement si chaque commande est autorisÃ©e pour chaque robot et Ã  chaque instant.

### DÃ©cision

DÃ©river la disponibilitÃ© des commandes V1 de maniÃ¨re prudente Ã  partir de lâ€™Ã©tat courant, de la connectivitÃ© et des capacitÃ©s confirmÃ©es par lâ€™API, plutÃ´t que dâ€™inventer un attribut de support non documentÃ©.

### ConsÃ©quences positives

- aucune supposition non documentÃ©e sur le support des commandes ;
- boutons plus sÃ»rs dans lâ€™UI ;
- logique testable et centralisÃ©e ;
- comportement cohÃ©rent avec la contrainte de ne pas inventer de payload ou de capacitÃ©.

### ConsÃ©quences nÃ©gatives

- lâ€™Ã©tat affichÃ© peut Ãªtre plus prudent que la rÃ©alitÃ© du robot ;
- certains boutons peuvent Ãªtre dÃ©sactivÃ©s alors quâ€™une commande serait techniquement acceptÃ©e ;
- un affinage sera peut-Ãªtre nÃ©cessaire en V1.1 si Husqvarna documente davantage les transitions dâ€™Ã©tat.

## ADR-014 â€” Utiliser des clients HTTP typÃ©s pour les appels Husqvarna

Statut : acceptÃ©  
Date : 2026-05-12

### Contexte

Lâ€™application doit appeler deux bases distinctes Husqvarna, tout en gardant la logique HTTP testable et centralisÃ©e.

### DÃ©cision

Utiliser `IHttpClientFactory` avec des clients typÃ©s pour lâ€™API Automower et lâ€™API OAuth, puis injecter ces clients dans la couche Infrastructure plutÃ´t que de crÃ©er des `HttpClient` dispersÃ©s dans lâ€™UI.

### ConsÃ©quences positives

- configuration HTTP centralisÃ©e ;
- tests mockÃ©s simples ;
- gestion propre des bases dâ€™URL et des dÃ©pendances ;
- alignement avec la stack demandÃ©e.

### ConsÃ©quences nÃ©gatives

- dÃ©pendance supplÃ©mentaire au composant de composition ;
- lâ€™application dÃ©pend davantage du conteneur de services ;
- un flux dâ€™authentification navigateur reste volontairement non implÃ©mentÃ© tant que le point officiel nâ€™est pas documentÃ©.

## ADR-015 â€” Ouvrir le dÃ©tail robot Ã  partir de l'identifiant de la carte sÃ©lectionnÃ©e

Statut : acceptÃ©  
Date : 2026-05-12

### Contexte

Le tableau de bord affiche plusieurs robots et le dÃ©tail doit correspondre exactement au robot choisi par l'utilisateur.

### DÃ©cision

Propager l'identifiant du robot depuis la carte du tableau de bord jusqu'au ViewModel de dÃ©tail, puis charger explicitement ce robot au lieu d'en choisir un arbitrairement.

### ConsÃ©quences positives

- dÃ©tail cohÃ©rent avec la sÃ©lection utilisateur ;
- UX plus prÃ©visible ;
- rÃ©duction des ambiguÃ¯tÃ©s dans les tests et les retours de statut.

### ConsÃ©quences nÃ©gatives

- besoin de transporter un identifiant de robot dans la couche UI ;
- un peu plus de code de liaison entre vue tableau de bord et vue dÃ©tail.
## ADR Ã  complÃ©ter pendant le dÃ©veloppement

Ajouter une dÃ©cision si :

- une dÃ©pendance majeure est ajoutÃ©e ;
- le flux OAuth exact est validÃ© ;
- une fonctionnalitÃ© V1 est reportÃ©e.



## ADR-016 â€” Utiliser le flux OAuth officiel en authorization_code avec callback local

Statut : acceptÃ©  
Date : 2026-05-12

### Contexte

Le portail Husqvarna Developer expose le point dâ€™autorisation `GET /oauth2/authorize` sur la base `https://api.authentication.husqvarnagroup.dev/v1`. La V1 doit permettre une connexion officielle sans inventer de flux.

### DÃ©cision

Utiliser le navigateur systÃ¨me sur le flux `authorization_code` officiel, avec un callback local `http://localhost`, puis Ã©changer le code contre les jetons via `POST /oauth2/token`. Stocker le secret dâ€™application et les jetons dans un stockage Windows sÃ©curisÃ©.

### ConsÃ©quences positives

- connexion officielle et cohÃ©rente avec le portail Husqvarna ;
- aucun secret en clair dans le dÃ©pÃ´t ;
- UX desktop simple pour lâ€™utilisateur ;
- comportement testable par mocks.

### ConsÃ©quences nÃ©gatives

- dÃ©pendance au stockage sÃ©curisÃ© Windows ;
- besoin de renseigner un secret dâ€™application localement ;
- nÃ©cessite un callback loopback local et une gestion dâ€™erreur dâ€™authentification.

## ADR-017 â€” Simplifier l'écran Paramètres pour isoler le défaut d'affichage

Statut : acceptÃ©  
Date : 2026-05-12

### Contexte

L'écran `Paramètres` est le premier endroit signalé comme problématique lors de l'ouverture dans l'interface réelle. La priorité est d'isoler rapidement si le défaut vient d'un contrôle WinUI précis ou d'une navigation obsolète.

### DÃ©cision

Réduire temporairement l'écran `Paramètres` à une version minimale avec les champs essentiels et des logs de construction/chargement détaillés, afin de rendre la cause du problème observable sans multiplier les contrôles WinUI.

### ConsÃ©quences positives

- moins de contrôles susceptibles de casser au chargement ;
- logs plus précis sur le chemin d'ouverture ;
- diagnostic plus rapide ;
- surface de défaut réduite.

### ConsÃ©quences nÃ©gatives

- l'écran de paramètres est moins riche pendant l'isolation ;
- certains réglages sont affichés en lecture seule ;
- un retour à l'écran complet sera nécessaire une fois le défaut identifié.

## ADR-018 â€” Retirer temporairement les contrôles de saisie WinUI qui déclenchent un COMException de mesure

Statut : acceptÃ©  
Date : 2026-05-12

### Contexte

Les journaux montrent qu'un `COMException` non spécifié survient pendant `MeasureOverride` quand les écrans `Paramètres` et `Robot` contiennent des contrôles d'édition WinUI. Les mêmes écrans deviennent stables une fois ces contrôles retirés.

### DÃ©cision

Remplacer temporairement les contrôles de saisie des écrans concernés par des affichages en lecture seule et conserver uniquement les boutons sûrs pour stabiliser l'application V1 sur cette machine.

### ConsÃ©quences positives

- suppression du crash au rendu ;
- diagnostic plus simple ;
- le shell et la navigation restent utilisables ;
- les écrans restent visibles et testables.

### ConsÃ©quences nÃ©gatives

- l'édition locale des paramètres est temporairement indisponible dans l'UI ;
- la saisie de durée de tonte temporaire doit être repensée ;
- un mécanisme d'édition alternatif devra être réintroduit pour récupérer la fonctionnalité complète.

## ADR-019 — Réintroduire l'édition des paramètres avec des contrôles WinUI simples

Statut : accepté  
Date : 2026-05-12

### Contexte

Après stabilisation de l'interface, l'utilisateur doit pouvoir saisir directement la `Application Key`, l'URI de redirection, l'intervalle de rafraîchissement, le niveau de log et le secret d'application depuis l'application.

### Décision

Réintroduire l'édition dans l'écran `Paramètres` à l'aide de `TextBox` et `PasswordBox` simples, sans `NumberBox`, en conservant le secret d'application masqué et sans l'afficher en clair après chargement.

### Conséquences positives

- l'utilisateur peut de nouveau configurer l'application sans éditer un fichier local ;
- la saisie reste limitée à des contrôles WinUI simples ;
- le secret n'est jamais préchargé dans l'UI ;
- le flux reste conforme aux règles de sécurité du projet.

### Conséquences négatives

- l'écran `Paramètres` reste plus sensible aux régressions de rendu que la version lecture seule ;
- la validation d'un champ numérique reste gérée par l'UI ;
- le comportement doit être revérifié sur la machine cible après chaque changement WinUI.

## ADR-020 — Déplacer l'édition des paramètres dans une boîte de dialogue Windows Forms

Statut : accepté  
Date : 2026-05-12

### Contexte

Les contrôles de saisie WinUI réintroduisent un `COMException` lors du rendu sur la machine cible. L'écran WinUI doit rester stable tout en permettant malgré tout la saisie des paramètres locaux.

### Décision

Conserver l'écran `Paramètres` en lecture seule côté WinUI et ouvrir une boîte de dialogue modale Windows Forms pour saisir la clé d'application, l'URI de redirection, l'intervalle de rafraîchissement, le niveau de log et le secret d'application.

### Conséquences positives

- l'écran WinUI reste stable ;
- l'utilisateur peut quand même renseigner ses paramètres localement ;
- le secret peut être saisi dans un champ masqué sans réinventer un flux de stockage ;
- le changement métier reste limité à la couche UI.

### Conséquences négatives

- la solution ajoute une dépendance à Windows Forms dans l'application ;
- l'expérience utilisateur mélange deux styles d'interface Windows ;
- le contournement doit être réévalué si le problème WinUI disparaît.

## ADR-021 — Recréer les vues WinUI au lieu de modifier des contrôles déjà montés

Statut : accepté  
Date : 2026-05-12

### Contexte

Les logs ont montré des `COMException` au rendu quand les vues `Connexion`, `Paramètres`, `Tableau de bord` et `Robot` mutaient des `TextBlock`, des collections d'enfants ou des contenus déjà montés dans l'arbre visuel.

### Décision

Construire de nouveaux arbres visuels à chaque rafraîchissement de vue WinUI, puis remplacer le `Content` racine au lieu de modifier des contrôles persistants déjà affichés.

### Conséquences positives

- suppression des mutations XAML les plus fragiles ;
- réduction des `COMException` liées à `MeasureOverride` et `Clear()`;
- comportement plus prévisible lors des rechargements UI ;
- isolation plus simple des problèmes restants.

### Conséquences négatives

- plus d'allocation de contrôles à chaque rendu ;
- les vues perdent tout état visuel local non porté par le ViewModel ;
- le code UI devient plus verbeux, car il faut reconstruire l'arbre explicitement.
