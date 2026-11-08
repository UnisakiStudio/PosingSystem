# Unisaki Posing System

Système de poses commun utilisé notamment par Kawaii Posing et Cool Posing. Il s’appuie sur Modular Avatar et NDMF pour générer les menus et animations nécessaires lors de l’envoi de l’avatar.

Ce README concerne la version v3.2.0 en développement. La date de sortie officielle n’est pas encore fixée. La version installée apparaît en haut de l’Inspector de l’outil de poses. Consultez également les [ajouts et modifications](CHANGELOG.md).

## Langue d’affichage et documentation hors ligne

Choisissez la langue d’affichage dans « Language / 言語 », tout en haut de l’Inspector. Ce choix s’applique à la fois à l’Inspector et aux menus VRChat générés. Renvoyez l’avatar pour modifier l’affichage dans le jeu.

Sans choix explicite, la langue de l’ordinateur est utilisée ; une langue non prise en charge est remplacée par l’anglais. Windows, macOS et Linux sont pris en charge. La dernière langue choisie est enregistrée dans EditorPrefs et réutilisée dans les autres projets du même utilisateur du système sur cet ordinateur. Un choix explicite déjà enregistré dans l’avatar est conservé. Ce réglage n’est pas synchronisé avec d’autres ordinateurs. Le stockage de la licence reste inchangé.

Choisissez la langue du README, de l’historique et du guide dans la [liste des langues](../index.html) fournie. Les liens du guide et de l’historique dans l’Inspector suivent également la langue sélectionnée. Les conditions d’utilisation originales et leurs traductions de référence restent disponibles séparément.

## Changement de pose en mode bureau

Le délai avant l’application de la pose choisie lors du passage entre les positions debout, accroupie et assise en mode bureau a été réduit. Cette amélioration s’applique aux builds ordinaires et au précalcul, sans commande spéciale ni réglage supplémentaire. Le traitement habituel est conservé en mode VR et dans les situations particulières. Actualisez les anciens précalculs avant d’envoyer l’avatar.

## Installation et mise à jour

Ajoutez l’outil de poses avec VCC ou ALCOM en suivant les fichiers et instructions fournis avec votre achat. Ce système est installé comme dépendance de l’outil de poses.

La version v3.2.0 nécessite l’environnement suivant.

- Projet d’avatar VRChat sous Unity 2022.3
- VRChat Avatars SDK 3.10.4 ou version ultérieure
- Modular Avatar 1.x, version 1.15.1 ou ultérieure
- NDMF (Non-Destructive Modular Framework) 1.x, version 1.14.0 ou ultérieure

Si vous utilisez Kawaii Posing, mettez-le à jour vers 3.2.0 dans ALCOM / VCC, puis vérifiez que Unisaki Posing System est également en 3.2.0. Si vous utilisiez le précalcul, cliquez sur « Actualiser le précalcul », enregistrez la scène, puis envoyez l’avatar.

Cool Posing 1.2.0 utilise le système commun 3.2.0. Mettez à jour ensemble le produit et le système commun vers les versions correspondantes.

### Pour les utilisateurs de la version 8bit

Les nouveaux ajouts utilisent désormais le Prefab standard. Désactivez les options inutiles dans « Fonctions » pour réduire la consommation. La sélection des poses seule utilise 8 bit de base.

Les anciens Prefabs 8bit restent dans « Editor/Legacy » de chaque produit afin de préserver les références des avatars et préréglages existants. Il n’est pas nécessaire de supprimer ou remplacer un Prefab pour continuer à utiliser une version 8bit déjà installée. Le menu d’ajout de Unity ne propose plus la version 8bit.

## Présentation de la configuration

Dans la Hierarchy, sélectionnez 「可愛いポーズ」, 「カッコいいポーズ」 ou l’objet équivalent situé dans votre avatar pour le configurer.

| Onglet | Réglages |
| --- | --- |
| Fonctions | Éléments ajoutés au menu du jeu, fonctions pratiques, icônes et consommation des ExpressionParameters |
| Poses | Choix, ajout et ordre des poses, aperçu dans la Scene, détails de chaque pose et ajustement de l’animation |
| Configuration de l'avatar | Corrections adaptées à la morphologie, correction des genoux, importation des poses d’origine ou de mouvements personnels et préréglages |

Pour les fonctions proposant des réglages détaillés, cliquez sur le lien d’ouverture des réglages : les champs apparaissent en dessous. Les réglages de respiration se trouvent aussi dans « Fonctions ».

La section d’aide et d’historique en haut permet d’ouvrir le guide, l’assistance et les modifications. Vous pouvez alterner entre l’historique du produit et celui de Unisaki Posing System.

## Fonctions et consommation des ExpressionParameters

Les options choisies dans « Fonctions > Fonctions du menu » apparaissent dans le menu du jeu. Par exemple, activer « Miroir » permet de basculer l’effet miroir dans le jeu. Le simple fait de cocher cette option dans l’Inspector n’applique pas en permanence un miroir à toutes les poses.

| Fonction | Bits supplémentaires |
| --- | ---: |
| Miroir | 1 bit |
| Sans gestes | 1 bit |
| Animer la respiration | 0 bit |
| Respiration (activation dans le jeu) | 1 bit |
| Hauteur pieds | 8 bit |
| Hauteur sol | 8 bit |
| Mode photo (vue locale uniquement) | 0 bit |
| Synchroniser le Mode photo | 16 bit |
| Garder la tête visible en Mode photo | 0 bit |
| Désactiver le balancement au repos de VRChat | 0 bit |
| Réduire les déformations liées à la rotation de la tête | 1 bit (partagé avec Se retourner) |
| Se retourner | 10 bit (dont 1 bit partagé avec la réduction des déformations) |

Se retourner utilise 1 bit pour l’activation, 8 bit pour l’inclinaison de la tête et 1 bit pour le verrouillage du cou. Si la réduction des déformations est déjà active, l’ajout de Se retourner consomme donc 9 bit supplémentaires. Le bit partagé n’est compté qu’une fois, et le total affiché se met à jour lorsque vous modifiez les options.

Le total affiché concerne les outils de poses installés sur cet avatar. Il ne représente pas la consommation de l’avatar entier avec ses vêtements et autres systèmes. Les réglages communs ne sont pas comptés plusieurs fois lorsque plusieurs outils de poses sont utilisés. Si le nombre de poses dépasse la plage de numérotation de base, leur sélection nécessite également des bits supplémentaires.

### Hauteur pieds et Hauteur sol

- « Hauteur pieds » : règle la hauteur du côté des pieds tout en conservant la position de la tête. Utilisez cette option pour aligner les pieds sur le sol ou un lit.
- « Hauteur sol » : déplace tout le corps verticalement, tête comprise. 50% correspond à la hauteur d’origine, 0% à 3m plus bas et 100% à 3m plus haut. La valeur initiale est 50%.

Ces deux réglages se trouvent dans « Réglages » dans le jeu ; « Hauteur sol » vient après « Hauteur pieds ». Éloigner « Hauteur sol » du centre déplace l’avatar verticalement. Revenir près de 50% rétablit la hauteur d’origine et termine la correction. Il n’y a pas d’interrupteur distinct de flottement. La vue est corrigée au début du flottement et pendant les changements de hauteur du sol.

Le réglage de hauteur des pieds ne génère plus de doublons des mouvements du corps entier pour les animations ordinaires. Cela réduit les données d’animation générées ; la taille totale de l’avatar n’est pas pour autant systématiquement divisée par 3.

### Mode photo

1. Activez « Fonctions > Fonctions du menu > Mode photo ».
2. Dans VRChat, ouvrez « Mode photo », puis choisissez une pose dans « Choisir pose ».
3. « Distance » déplace l’avatar jusqu’à 3m vers l’avant et « Direction » le fait pivoter. Lorsque la distance est supérieure à 0, l’avatar vous fait face.
4. Activez « Pieds fixes » si nécessaire. Le verrouillage des pieds n’est pas imposé au démarrage : vous pouvez ajuster la position avant de les fixer. Si « Sans gestes » a été ajouté au menu, vous pouvez aussi changer les gestes des mains dans ce même écran.
5. « Quitter photo » réinitialise la distance et la direction, puis déverrouille les pieds.

Le déplacement et la rotation du Mode photo utilisent 0 bit s’ils ne concernent que votre propre vue. Pour montrer la position et la direction aux autres personnes, activez « Fonctions pratiques > Synchroniser le Mode photo ». La distance et la direction utilisent 16 bit au total.

Si la tête de votre avatar disparaît lorsqu’il est proche de vous, activez « Fonctions pratiques > Garder la tête visible en Mode photo ». Cette option ne s’applique qu’à votre propre vue pendant les photos ; l’affichage habituel revient à la sortie du mode.

### Changer de pose allongée en sautant

Si des poses sur le dos et sur le ventre sont toutes deux sélectionnées, sautez sur place pour passer de l’une à l’autre. Avec une seule de ces orientations sélectionnée, la pose est conservée. Pour désactiver ce changement par saut, décochez « Changer de pose allongée en sautant » dans « Fonctions → Fonctions pratiques » de Unity. La consommation supplémentaire de paramètres synchronisés est de 0 bit. Si plusieurs outils de poses sont installés sur un avatar, désactiver cette option dans l’un d’eux s’applique à l’avatar entier.

### Se retourner

Dans les poses allongées compatibles, incliner la tête vers la gauche ou la droite fait aussi rouler progressivement le corps. Toutes les poses allongées ne deviennent pas automatiquement compatibles.

1. Activez « Fonctions > Fonctions du menu > Se retourner ».
2. Dans « Poses », ouvrez les « Détails » de la pose allongée voulue et activez « Activer le retournement ».
3. Affectez une animation de profil à « Pose latérale de retournement ». La pose d’origine et la pose de profil servent à créer les retournements vers la gauche et la droite. Ajustez la « Force de correction du contact au sol » si nécessaire.
4. Dans VRChat, choisissez une pose compatible et activez « Se retourner ». Utilisez cette fonction avec le verrouillage du cou désactivé.

Pour une pose utilisant Se retourner, l’inclinaison de la tête détermine la position de lecture de l’animation ; le réglage MotionTime ordinaire n’est donc plus utilisable. Pour utiliser une animation de retournement déjà terminée, affectez-la au champ d’animation normal et laissez « Pose latérale de retournement » vide. Préparez une animation où la position de lecture 0 correspond à la gauche, 0.5 au dos et 1 à la droite.

### Respiration et balancement par défaut

« Fonctions > Fonctions pratiques > Animer la respiration » fait bouger la poitrine et d’autres parties du corps au rythme de la respiration. Ouvrez les réglages pour choisir « Aucun », « Faible », « Moyenne », « Forte », « Rapide » ou « Personnalisé » pour chaque catégorie de poses. « Rapide » produit une respiration accélérée. « Personnalisé » permet de définir votre propre animation de respiration.

La respiration seule utilise 0 bit. Pour pouvoir l’arrêter et la relancer dans VRChat, activez aussi « Fonctions du menu > Respiration ». La synchronisation de l’activation utilise 1 bit. Changer l’intensité n’augmente pas la consommation.

Le cou, les épaules et les bras suivent également les mouvements de la poitrine afin de limiter le déplacement de la tête et des mains. Il ne s’agit pas d’un verrouillage complet : choisissez une intensité adaptée à la morphologie et à la pose.

« Fonctions pratiques > Désactiver le balancement au repos de VRChat » réduit le balancement du corps ajouté par défaut par VRChat pendant l’attente. Désactivée par défaut, cette option préserve la respiration propre à l’outil et les autres mouvements personnalisés. Elle peut être choisie même sans utiliser la respiration et ne consomme aucun bit supplémentaire. Elle n’arrête pas les mouvements provenant d’une autre source que l’animation d’attente standard.

Si plusieurs outils de poses sont utilisés, désactiver le balancement standard dans l’un d’eux s’applique à l’avatar entier. Pour le rétablir, désactivez ce réglage dans tous les outils, puis renvoyez l’avatar.

### Réduire les déformations liées à la rotation de la tête

Utilisez cette option si la pose se déforme lorsque vous libérez le cou, parce que son orientation diffère de celle de la pose d’origine. Elle se trouve dans « Fonctions > Fonctions pratiques » et n’ajoute aucun menu dédié dans le jeu. Lorsque le cou est verrouillé, l’orientation de la pose d’origine est utilisée. Les poses couchées sur le dos sont exclues afin de conserver leur orientation initiale vers le haut.

### Icônes du menu

- « Générer les icônes des poses » : crée une image de l’avatar dans chaque pose et l’affiche dans le menu. Les poses sont plus faciles à reconnaître, mais la génération des images allonge le build.
- « Réduire la taille des icônes » : diminue leur résolution pour réduire la taille de l’avatar.

Les icônes de poses ne sont pas générées pour les builds Android. Après avoir modifié une tenue, par exemple, utilisez « Reprendre les icônes ». Les images de la liste des poses se mettent à jour avec « Actualiser les vignettes des poses ». Ces opérations peuvent prendre du temps ; lisez la confirmation avant de les lancer.

## Ajouter, réordonner et prévisualiser les poses

Choisissez une catégorie à gauche de « Poses ». L’onglet avec une loupe, tout en haut, permet de chercher des noms dans toutes les catégories. Ouvrez « Détails » pour régler ensemble l’animation, l’icône, MotionTime et les autres options.

Pour ajouter plusieurs animations à la fois, glissez-les dans la zone d’ajout, ou sélectionnez-les dans Project et cliquez sur « Importer les clips sélectionnés dans Project ». Les doublons d’un même clip sont écartés.

Pour modifier l’ordre, choisissez la destination dans « Détails > Ordre ». Vous pouvez aussi utiliser les boutons de déplacement vers le haut et le bas. L’état ouvert ou fermé des détails est conservé après le déplacement.

Le bouton « Aperçu » de chaque pose permet de la voir dans la Scene sans ouvrir la fenêtre d’ajustement. Quitter l’aperçu rétablit l’affichage d’origine. Cet aperçu simple n’exécute pas le build MA ou NDMF : son résultat peut donc différer des vêtements ajoutés au build ou de systèmes comme FloorAdjuster. Vérifiez aussi les vêtements et le contact au sol après le build de l’avatar.

## Ajustement de l’animation et ajustement automatique des membres (IK)

Ouvrez « Poses > Ajustement de l'animation » pour régler précisément le corps et les membres avec un aperçu. Après vos modifications, enregistrez l’animation d’ajustement à l’aide des commandes de sauvegarde de la fenêtre.

Pour les produits et poses compatibles, l’ajustement automatique place les membres en fonction de la morphologie de l’avatar. « Activer l'ajustement automatique des membres » dans la fenêtre d’ajustement active ou désactive l’ensemble ; « Activer l'IK pour cette pose » agit pose par pose. Un message indique les poses pour lesquelles aucun ajustement automatique n’est disponible.

Pour terminer manuellement un résultat obtenu automatiquement, cliquez sur « Intégrer l'IK aux ajustements ». Le résultat IK actuel est écrit dans une nouvelle animation d’ajustement et l’IK de cette pose est désactivée pour cet avatar. L’animation d’origine est conservée. Ce bouton est disponible lorsque l’ajustement automatique global et celui de la pose sont actifs, avec des réglages IK applicables.

Dans « Configuration de l'avatar », « Empêcher les genoux de se plier à l'envers » active la correction des genoux. Réglez l’angle dans « Flexion minimale du genou (degrés) » des détails. « Activer la correction de l'avatar » est disponible lorsqu’il existe des données de correction compatibles. Certaines corrections ne peuvent pas être cumulées avec l’ajustement automatique des membres.

## Poses d’origine, mouvements personnels et préréglages

Ces éléments se règlent dans « Configuration de l'avatar ».

- « Utiliser les poses debout, accroupies et sur le ventre d'origine » : importe automatiquement les poses initialement définies dans l’avatar.
- « Utiliser vos propres mouvements » : ouvrez les réglages d’importation et affectez individuellement un mouvement à chaque type, notamment debout, accroupi (Crouching) ou couché sur le ventre (Prone). En règle générale, définissez un seul mouvement par type.
- « Intégrer les verrouillages de suivi aux autres outils » : activez cette option si les verrouillages fonctionnent mal avec d’autres systèmes.

L’importation de mouvements que vous définissez vous-même est une fonction distincte de l’importation automatique des poses d’origine.

Les préréglages se trouvent au bas de l’onglet. Recherchez-les par nom, puis cliquez sur « Appliquer ce préréglage » pour remplacer les réglages de poses. Ils servent surtout à la configuration initiale. Comme ils remplacent la configuration actuelle, enregistrez d’abord les ajustements que vous souhaitez conserver.

L’affichage de la liste ne charge que les noms et les informations sur les avatars compatibles. Les animations d’ajustement sont chargées au moment d’appliquer le préréglage choisi. Même lors de sa première création, la liste ne précharge pas les animations de tous les avatars.

La liste distingue « Compatible avec l'avatar », « Préréglages utilisateur » et « Préréglages officiels d'Unisaki Studio ». « Masquer les préréglages inclus » est un réglage d’affichage partagé par le projet. La liste se met à jour après l’ajout, la modification ou la suppression de préréglages. Si l’affichage ne change pas, cliquez sur « Recharger ».

## Précalcul et avertissements

Lorsque « Utiliser les poses debout, accroupies et sur le ventre d'origine » est actif, les mouvements sont détectés dans la couche Base de l’avatar à chaque génération. Les résultats ne sont pas recopiés dans les réglages d’importation manuels : la liste reste donc non affectée si vous utilisez seulement l’importation automatique. Modifier les mouvements de Base ou activer/désactiver l’importation automatique prend effet à la génération suivante.

Lors de l’envoi, l’importation automatique s’applique à la copie utilisée pour le build. Elle ne modifie ni les objets de la scène, ni les réglages d’importation, ni le Controller et les mouvements sources. Si un précalcul existe et que ses entrées n’ont pas changé, son résultat est réutilisé.

Pour les types ayant une sélection manuelle, celle-ci est prioritaire ; la rotation, l’animation d’ajustement et la position de lecture sont également appliquées. Désactiver « Utiliser » ou retirer un mouvement précédemment affecté empêche l’importation automatique de ce type. « Effacer la sélection manuelle » retire ses réglages et ajustements : si l’importation automatique est active, elle le détectera de nouveau à la génération suivante. Cette opération peut être annulée avec Undo. S’il existe plusieurs anciens réglages du même type, le réglage actif sélectionné dans l’Inspector est prioritaire.

Les réglages ajoutés à la liste par l’ancienne importation automatique ne sont pas supprimés automatiquement, car ils ne peuvent pas être distingués des réglages enregistrés ou ajustés manuellement. Utilisez « Effacer la sélection manuelle » uniquement pour les types que vous souhaitez remettre en importation automatique. Lorsque plusieurs produits sont installés sur le même avatar, les réglages manuels des autres produits s’appliquent aussi aux poses d’origine communes.

Le précalcul réutilise les données générées pour réduire le temps de build de l’avatar. Créez-le ou actualisez-le avec « Actualiser le précalcul ». Le build ordinaire génère lui aussi les données nécessaires : vous n’avez pas à lancer manuellement un précalcul à chaque fois.

Après une modification des réglages, une mise à jour peut être nécessaire. « Vérifier les réglages » en haut de l’Inspector et les marques dans la Hierarchy vous en avertissent. Après avoir modifié des poses ou des réglages IK, actualisez le précalcul pour réduire la durée des builds suivants.

« Réglages d'affichage des avertissements » se trouve dans « Vérifier les réglages ». Vous pouvez masquer certaines catégories d’avertissements ou les marques de la Hierarchy. Les masquer ne corrige pas les problèmes de configuration et n’actualise pas les données générées. Les erreurs et les opérations de génération nécessaires restent actives.

## Précautions d’utilisation

- Prévu pour le suivi à 3 points et le mode bureau. Le verrouillage de pose avec suivi du corps entier n’est pas pris en charge.
- Selon la morphologie et les modifications, le contact des mains et des pieds ou l’apparence des vêtements peut nécessiter des ajustements supplémentaires.
- Des couches Base particulières ou d’autres systèmes de changement de posture peuvent produire des résultats inattendus.
- Le déplacement et la rotation du Mode photo ainsi que la hauteur du sol corrigent les poses de l’outil et les mouvements standards ou personnels utilisés sans pose sélectionnée. Vérifiez dans VRChat les mouvements standards remplacés dans le jeu et le comportement du point de vue.

## Conditions d’utilisation et assistance

Le code source et les ressources de ce dépôt ne sont pas open source. Leur utilisation nécessite l’achat légitime du produit et l’acceptation des [conditions d’utilisation fournies](../../../%E3%82%86%E3%81%AB%E3%81%95%E3%81%8D%E3%83%9D%E3%83%BC%E3%82%BA%E3%82%B7%E3%82%B9%E3%83%86%E3%83%A0%E5%88%A9%E7%94%A8%E8%A6%8F%E7%B4%84%EF%BC%88%E6%97%A5%E6%9C%AC%E8%AA%9E%E7%89%88%EF%BC%89.pdf).

- [Manuel de Kawaii Posing](https://docs.unisakistudio.jp/kawaiiposing/manual/)
- [Manuel de Cool Posing](https://docs.unisakistudio.jp/coolposing/manual/)
- [Assistance](https://help.unisakistudio.jp/)

Pour signaler un problème, indiquez les versions du produit et de Unisaki Posing System, celles de Unity, VRChat SDK, Modular Avatar et NDMF, l’avatar utilisé, les étapes de reproduction et les erreurs de la Console.
