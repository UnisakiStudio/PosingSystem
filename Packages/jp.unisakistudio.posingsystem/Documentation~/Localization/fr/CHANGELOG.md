# Historique des modifications

## [3.2.0] - Non publié

### Ajouts

- Affichage de l’Inspector, des commandes d’édition, des dialogues d’installation, des noms de poses standards et des menus VRChat générés en 30 langues.
- Choix de langue unifié pour l’Inspector et les menus du jeu. La langue de l’ordinateur est utilisée initialement, puis le dernier choix est réutilisé dans les autres projets du même utilisateur du système.
- Ouverture du README, de l’historique et du guide hors ligne dans la langue choisie.
- Noms d’affichage des 128 poses standards et descriptions de toutes les fonctions harmonisés avec la terminologie existante.

### Compatibilité

- Conservation des réglages existants de l’avatar, des noms personnalisés, des références d’assets, des identifiants de paramètres et du stockage de la licence.

## [3.1.4] - 2026-11-11

- Correction des animations de position accroupie et ventrale propres à l’avatar qui n’étaient pas appliquées, laissant les poses standard de VRChat. Cela concerne l’importation automatique comme les affectations manuelles.
- Correction des cheveux, lunettes et autres éléments attachés avec MA Bone Proxy qui restaient à leur position d’origine pendant la modification ou l’aperçu des poses. Ils suivent désormais la pose de l’aperçu sans changer les réglages ni le placement de l’avatar d’origine.
- Correction de l’échec de la correction de pose basée sur la position du point de vue lors de la capture d’images de poses telles que « 仰向け寝_お疲れ ». Les images utilisent une animation intégrant les corrections de pose et de sol appliquées au build.

Dans ALCOM / VCC, mettez Unisaki Posing System à jour vers 3.1.4. Kawaii Posing 3.1.2 et Cool Posing 1.1.0 restent utilisables tels quels. Si vous utilisiez le précalcul, cliquez sur « Actualiser le précalcul » après la mise à jour, enregistrez la scène, puis reconstruisez et renvoyez l’avatar.

## [3.1.3] - 2026-11-09

- L’utilisation de « Actualiser les vignettes des poses » ou du précalcul sur un avatar dupliqué ne supprime plus les images de poses de l’avatar d’origine.
- Correction de l’affichage de « Réglages modifiés » après une simple nouvelle capture d’images lorsque le précalcul avait été effectué avant le premier enregistrement de la scène ou dans le mode d’édition de Prefab.
- Correction des précalculs pouvant s’arrêter en cours de route si des données de poses d’un précalcul précédent subsistaient.
- Allègement de l’ouverture de la liste lorsque les images de poses n’existent pas encore. Révision également de l’attente qui bloquait Unity jusqu’à la fin de la récupération des informations de paquets au début d’un précalcul ou d’un build.
- Les réglages d’édition des outils de poses désactivés dans l’avatar sont désormais supprimés des builds PC et Android.
- « Demander de l'aide sur cette erreur » peut désormais inclure les numéros de diagnostic et autres informations trouvés plus loin dans la Console, même si les premières erreurs ne permettent pas d’identifier la cause. Pour une erreur de réglage des clips de « Se retourner », le brouillon inclut aussi les points à vérifier.

Dans ALCOM / VCC, mettez Unisaki Posing System à jour vers 3.1.3. Kawaii Posing 3.1.2 et Cool Posing 1.1.0 restent utilisables tels quels. Comme ces produits indiquent une version antérieure du système commun, une confirmation de conflit peut apparaître pendant la mise à jour. Si vous utilisiez le précalcul, cliquez sur « Actualiser le précalcul » après la mise à jour, enregistrez la scène, puis reconstruisez et renvoyez l’avatar.

## [3.1.2] - 2026-11-09

- Correction du visage qui restait orienté vers le haut après s’être relevé d’une pose sur le ventre avec « Réduire les déformations liées à la rotation de la tête » actif.
- Le verrouillage de la tête peut désormais être changé dans « Réglages → Verrouillage automatique de la tête sur le dos » du menu EX, même si « Hauteur pieds » est désactivé. Les avatars dont cette entrée avait disparu en 3.1.0 ou 3.1.1 la récupèrent après mise à jour et nouveau build.
- « Actualiser le précalcul » fonctionne aussi lorsque l’objet principal de l’avatar est désactivé dans Unity.
- Correction de l’impossibilité occasionnelle de générer des poses après une mise à jour lorsqu’un ancien Controller de poses commun restait dans Assets. Les réglages des Controllers modifiés personnellement sont également conservés.
- Lorsque l’outil ou un parent désactivé empêche d’utiliser l’Animator de pose commun, le nom de l’objet concerné est affiché. « Vérifier les réglages → Sélectionner l’objet » permet de le retrouver.
- Lorsque le précalcul signale des réglages modifiés, les détails permettent désormais de consulter les éléments différents de la dernière sauvegarde.
- Lorsqu’une erreur de script bloque le build (PS050), jusqu’à 5 erreurs d’origine détectées par Unity sont affichées. Le bouton de demande d’assistance pour cette erreur ouvre un brouillon contenant les versions de Unity et des outils compatibles ainsi que le numéro d’erreur ; vous pouvez le vérifier, le modifier et l’envoyer.

Si vous utilisiez le précalcul, cliquez sur « Actualiser le précalcul » après la mise à jour, enregistrez la scène, puis reconstruisez et renvoyez l’avatar.

## [3.1.1] - 2026-11-08

- « Hauteur sol » et « Se retourner », lorsqu’ils sont activés, apparaissent désormais dans « Réglages » du menu EX de VRChat même si « Miroir » est désactivé.
- Les retournements corrects sont conservés pour les poses « Jambes tendues », « Relax » et « Un genou levé », même après réorganisation, suppression ou renommage dans une ancienne version. Ils sont également conservés après un Unpack complet si la pose d’origine peut être identifiée. Les poses personnelles, les retournements volontairement désactivés et les mouvements latéraux remplacés sont préservés.
- Correction du bref passage sur le ventre lors de la sélection de l’avatar dans VRChat, ainsi que du bref passage sur le dos lors d’un saut lorsque seules des poses sur le ventre sont choisies. Le changement par saut peut être désactivé dans « Fonctions → Fonctions pratiques → Changer de pose allongée en sautant ». Il est activé par défaut.
- Si la référence de l’Animator Controller de poses commun est perdue après une mise à jour et peut être restaurée, un avertissement apparaît et le Controller est reconstruit à partir des réglages. Les références partagées entre plusieurs avatars sont aussi préservées.
- En présence d’erreurs de compilation, le build s’arrête dès le début avec une explication. Le nettoyage automatique des Controllers générés est suspendu tant que les erreurs de script empêchent de vérifier les références.
- Lors de l’ajout d’un outil, le Prefab officiel est prioritaire même si le projet contient un autre Prefab du même nom. Le précalcul continue aussi lorsque seules les fins de ligne du Controller commun diffèrent.
- Les Write Defaults des couches FX ajoutées par l’outil, notamment la hauteur du sol, suivent désormais les réglages de l’avatar.

Si vous utilisiez le précalcul, cliquez après la mise à jour sur « Actualiser le précalcul » dans l’outil de la scène, enregistrez celle-ci, puis reconstruisez et renvoyez l’avatar.

## [3.1.0] - 2026-11-07

- Ajout de l’ajustement automatique des membres (IK) selon la morphologie pour les poses compatibles, des corrections propres à l’avatar et de la prévention de la flexion inversée des genoux.
- Le Mode photo permet de modifier la position et l’orientation de l’avatar. Leur synchronisation et l’affichage de votre propre tête pendant les photos sont configurables.
- « Hauteur sol » déplace le corps entier jusqu’à 3m vers le haut ou le bas, indépendamment de « Hauteur pieds ».
- Ajout de la respiration, de la suppression du balancement d’attente standard et de la réduction des déformations liées à l’orientation du cou.
- Affichage des fonctions utilisées et de la consommation des paramètres dans l’Inspector. Ajout de la recherche de poses, de l’ajout groupé, du réordonnancement et de l’aperçu simple dans la Scene.
- Accélération des changements debout, accroupi et assis en mode bureau.
- Amélioration des avertissements de précalcul après modification, du défilement dans l’Inspector et des longues pauses lors du nettoyage des données générées.
- Correction de l’avertissement de mise à jour et de la régénération à l’envoi avec importation automatique active malgré un précalcul inchangé. Les résultats importés s’appliquent à l’avatar généré sans modifier les réglages d’importation de l’objet d’origine.
- Ajout du retournement par inclinaison de la tête et des mouvements compatibles configurables pour chaque pose.
- Amélioration du transfert des réglages de retournement des anciens préréglages, de l’importation des mouvements personnels et de la gestion des assets générés.

## [3.1.0-beta.2] - Bêta ouverte (2026-11-05)

- Ajout d’une longueur fixe et des différences de hauteur (Y) et de profondeur (Z) entre Head et Viewpoint comme références de longueur de position de l’IK.
- Affichage des avertissements de l’Inspector à gauche des noms d’objets dans la Hierarchy. Cela comprend les précalculs absents ou périmés ainsi que les avertissements affichés pendant l’importation de mouvements et les opérations sur les préréglages.
- Correction des longues pauses de Unity lorsque le nettoyage automatique, notamment à l’ouverture d’une scène, chargeait des Controllers générés appartenant à d’autres scènes.
- Correction de la perte de l’historique de précalcul après modification des poses ou fonctions. Un message indique désormais que les réglages ont changé depuis le précalcul plutôt qu’un précalcul absent, avec un avertissement de mise à jour dans la Hierarchy.
- Partage des résultats de comparaison du précalcul entre l’Inspector et la Hierarchy pour alléger le défilement lorsque les réglages et assets référencés n’ont pas changé. La comparaison du build reste exécutée à chaque fois.

## [3.1.0-beta.1] - Bêta ouverte (2026-11-04)

- Passage des changements ordinaires de posture en mode bureau à un traitement en une seule évaluation. Il s’applique aux builds ordinaires et au précalcul ; la VR et les cas particuliers conservent le traitement précédent.
- Reconnaissance des précalculs alpha.3 et antérieurs comme nécessitant une mise à jour.
- Conservation du point de vue lors des changements de hauteur du sol en Mode photo, puis retour à la dernière hauteur du sol à la sortie du mode.

- L’importation automatique détecte désormais les mouvements uniquement à la génération et n’ajoute ni ne modifie les réglages manuels pendant le précalcul ou le build.
- La régénération prend en compte l’activation de l’importation automatique, les changements de Base et la suppression des réglages manuels. Les sélections et ajustements manuels partagés entre produits sont conservés, et les anciens mouvements importés peuvent revenir aux poses standards.
- Gestion des Controllers de précalcul dans un fichier par avatar et par GUID de scène. Les références partagées par duplication, entre scènes ou depuis des Prefabs sont séparées avant mise à jour.
- Utilisation des assets temporaires NDMF pendant le build, sans multiplier les fichiers générés pour l’édition.
- Vérification des données générées à l’ouverture et à l’enregistrement des scènes. Les modifications non enregistrées, l’historique Undo/Redo et les références externes sont préservés ; seuls les fichiers inutilisés dont le propriétaire est certain vont à la corbeille.
- Application de la rotation, des animations d’ajustement et des positions de lecture des mouvements importés. Traitement de la rotation, de RootT/RootQ et des ajustements Humanoid unifié avec celui des poses ordinaires.
- Duplication des ajustements de BlendTree imbriqués pour leur destination d’importation, tout en préservant les mouvements sources et les mélanges de marche.
- La position de lecture « debout uniquement » est convertie en pose fixe à la fréquence d’origine, sans arrêter la lecture du côté marche.

## [3.1.0-alpha.2] - Test fermé (2026-10-26)

### Corrections et modifications

- Application des corrections du Mode photo et de hauteur du sol aux mouvements standards ou personnels lorsqu’aucune pose de l’outil n’est choisie.
- Extension de la hauteur du sol à 3m vers le haut et le bas. 50% devient la hauteur initiale et d’origine, 0% correspond à 3m plus bas et 100% à 3m plus haut.
- Correction de la modification du BlendTree source lors du remplacement « debout uniquement ».
- Utilisation des corrections manuelles pour les poses sans règle IK, avec suppression des avertissements de génération inutiles.
- Correction des brouillons d’ajustement mélangés entre poses du même nom et des changements de mouvement non détectés dans un même asset.
- Correction de la sélection initiale d’une autre pose via le numéro d’une pose initiale désactivée.
- Affichage d’une explication au lieu d’une exception lors de l’importation d’un AnimatorOverrideController non pris en charge.
- Conservation des désactivations explicites de retournement lors de l’application d’anciens préréglages.
- Amélioration de la lisibilité des icônes de fonctions dans le thème clair de l’Editor.

Les vérifications du point de vue et des mouvements standards remplacés se poursuivent dans le client VRChat.

## [3.1.0-alpha.1] - Avant publication

### Ajouts

- Ajout de l’activation par fonction et de l’affichage de consommation des ExpressionParameters.
- Ajout du Mode photo pour déplacer l’avatar vers l’avant et le faire pivoter.
- Ajout du choix de synchronisation du Mode photo : 0 bit en local uniquement, 16 bit avec synchronisation.
- Ajout du maintien de votre propre tête visible pendant le Mode photo.
- Ajout de « Hauteur sol » pour déplacer tout le corps, tête comprise, de 3m vers le haut ou le bas. 50% est la hauteur initiale et d’origine, 0% correspond à 3m plus bas et 100% à 3m plus haut.
- Ajout du retournement selon l’inclinaison de la tête et des animations compatibles configurables par pose.
- Ajout de la respiration et des réglages Faible, Moyenne, Forte, Rapide et Personnalisé.
- Choix séparé de l’animation de respiration et du menu d’activation dans le jeu : 0 bit pour l’animation seule, 1 bit pour synchroniser l’activation.
- Ajout de la suppression du balancement produit par l’attente standard de VRChat.
- Ajout de la réduction des déformations liées à la différence d’orientation du cou.
- Ajout de l’ajustement automatique des membres (IK) selon la morphologie pour les poses compatibles et de son activation globale ou par pose.
- Ajout de « Intégrer l'IK aux ajustements » pour enregistrer les résultats IK dans une animation d’ajustement.
- Ajout des corrections de pose propres à l’avatar, de la prévention de flexion inversée des genoux et de leur flexion minimale.
- Ajout de l’aperçu simple des poses dans la Scene depuis la liste.
- Ajout d’onglets de catégories, de la recherche transversale, de l’importation groupée d’animations et du choix de destination dans l’ordre des poses.
- Ajout de l’historique du produit et du système commun dans l’Inspector.

### Modifications

- Accélération du chargement des préréglages pour réduire l’attente lors de l’ajout de l’outil et de l’affichage de la liste.
- Actualisation automatique de la liste après ajout, modification ou suppression d’un préréglage.
- Organisation de l’Inspector en 3 onglets : « Fonctions », « Poses » et « Configuration de l'avatar ».
- Ajout de descriptions et d’icônes aux réglages, avec ouverture uniforme des détails.
- Déplacement des détails de respiration dans les fonctions, et des préréglages au bas de la configuration de l’avatar.
- Séparation des réglages d’importation manuelle des mouvements personnels et d’importation automatique des poses d’origine.
- Réduction des doublons d’animations du corps entier générées pour la hauteur des pieds.
- Mise à jour des images de poses au moyen d’un bouton avec confirmation.
- Regroupement des préférences d’avertissement dans « Vérifier les réglages ».
- Suppression de l’ajout de nouveaux Prefabs 8bit dédiés au profit des options du Prefab standard. Les références existantes sont préservées.
- Version minimale du VRChat Avatars SDK portée à 3.10.4.

### Corrections

- Correction du Mode photo et de la hauteur du sol appliquée aussi aux mouvements standards ou personnels sans pose de l’outil sélectionnée.
- Correction de la modification du BlendTree source lors du remplacement « debout uniquement ».
- Utilisation des ajustements manuels pour les poses sans règle IK valide et suppression des avertissements IK inutiles.
- Correction du mélange d’ajustements entre poses du même nom et de la non-prise en compte d’un changement de mouvement dans un même asset lors de la régénération.
- Le numéro d’une pose initiale désactivée ne provoque plus la sélection initiale d’une autre pose qui le réutilise.
- Affichage d’une explication au lieu d’une exception si l’importation des poses d’origine rencontre un AnimatorOverrideController non pris en charge.
- Conservation des réglages de retournement explicitement désactivés lors de l’application d’anciens préréglages.
- Amélioration du fond des icônes de l’Inspector pour les identifier dans le thème clair de l’Editor.

- Correction des icônes Se retourner, Hauteur sol et Mode photo absentes lors de l’ajout à un avatar.
- Correction de la perte des réglages de retournement des poses compatibles lors de l’application d’anciens préréglages.
- Correction du transfert de la saisie vers une autre pose si ses détails sont ouverts pendant le renommage d’une pose.
- Correction de la fermeture des détails lors d’un réordonnancement et des opérations Undo/Redo des réglages.
- Correction du chevauchement avec l’avatar d’origine dans l’aperçu et de la double addition de la position de placement.
- Correction des erreurs d’aperçu pour les avatars ayant des composants manquants.
- Correction des écarts d’orientation et de position IK des membres dus aux différences d’axes et d’échelles des os d’avatar.
- Correction du retard d’actualisation des avertissements et de l’état du précalcul après modification.
- Correction de la réutilisation d’anciens résultats de précalcul après modification des réglages IK.
- Correction d’erreurs de build pendant la génération des fonctions supplémentaires dans certaines configurations.

## [3.0.16] - 2026-09-14
- Version minimale de NDMF portée à 1.14.0 pour utiliser l’API `VirtualAnimatorController.SetParameter`.

## [3.0.15] - 2026-09-13
- Correction des importants changements de position du corps ou de flexion des genoux lors du basculement du verrouillage du cou sur le dos, sur des avatars combinant une échelle Hips modifiée et FloorAdjuster.

## [3.0.14] - 2026-09-09
- Correction de la disparition des entrées « Sans gestes » du deuxième outil et des suivants lorsque plusieurs outils de poses sont installés sur le même avatar.
- Correction du retour occasionnel de la couche USSPS_Locomotion vers Standing malgré la sélection d’une pose comme SleepDown.
- Correction des décalages de hauteur et de position X/Z avec FloorAdjuster, en tenant compte de l’ordre de traitement des versions MA et AAO.
- Restauration automatique de PosingSystem_Locomotion standard si l’AnimatorController précalculé dans Generated a été supprimé.
- Correction de l’erreur de build lorsque toutes les poses debout sont supprimées.
- Tri de la liste des préréglages par nom d’avatar.

## [3.0.13] - 2026-09-04
- Correction de la transition d’une pose debout vers elle-même, qui rendait le retour de posture instable si l’on revenait debout juste après avoir commencé à s’accroupir en mode VR.

## [3.0.12] - 2026-09-04
- Correction de la détection erronée de réglages modifiés lors du build NDMF d’un outil sans précalcul.
- Correction de l’échec avec erreur `AddAssetToSameFile` lors de l’ajout d’assets générés à un AnimatorController temporaire pendant le build NDMF.

## [3.0.11] - 2026-08-31
- Correction de l’échec dû à une Scene invalide pour l’avatar de précalcul avec NDMF 1.14.7 et versions ultérieures.
- Correction de `MissingReferenceException` lorsque la fenêtre d’erreur NDMF référençait un LocalizationAsset déjà détruit.

## [3.0.10] - 2026-08-29
- Chargement différé des assets de définition des préréglages jusqu’à l’ouverture de leur sélection, et réutilisation du cache si l’index des GUID n’a pas changé.
- Réévaluation des icônes d’avertissement et d’erreur de la Hierarchy uniquement après modification, avec cache partagé des recherches d’assets du menu produit. Réduction des blocages périodiques de l’Editor dans les scènes contenant beaucoup d’outils de poses.
- Correction de `ArgumentException` lors de l’enregistrement de valeurs de paramètres en double pendant le précalcul de plusieurs outils sur un même avatar.

## [3.0.9] - 2026-08-26
- Prévention de NullReferenceException si VRCAvatarDescriptor ne peut pas être obtenu depuis PosingSystem pendant la conversion, avec indication des journaux de build nécessaires au diagnostic.
- Correction des échecs `Avatar is null` lorsque des précalculs ou builds réutilisaient l’aperçu d’un Avatar temporaire créé puis supprimé par NDMF. Séparation de la conversion et du cache d’aperçu, et prévention des collisions de noms d’assets temporaires NDMF entre aperçus.
- Correction des aperçus du Prefab 「可愛いポーズ」 dont les références n’étaient pas enregistrées dans la scène et qui multipliaient les avatars cachés. Les aperçus sont exclus de l’enregistrement de scène ; les anciens sont aussi récupérés au chargement de scène, au passage en Play et au rechargement des scripts.

## [3.0.8] - 2026-08-18
- Correction des Unity Assertion et des risques de données de posture invalides lors d’une nouvelle correction de hauteur par FloorAdjuster ou un outil similaire sur des mouvements précalculés.
- Sécurisation de la mesure de hauteur afin qu’elle n’interfère pas avec l’avatar d’origine ou l’état AnimationMode d’autres outils.

## [3.0.7] - 2026-08-12
- Correction du décalage de la vue et de la flexion anormale des genoux sur le dos avec des outils de hauteur comme FloorAdjuster.
- Correction de l’erreur lorsque PosingSystem se trouve hors de l’avatar (merci à PancakeTorto1se).
- Correction des erreurs dues à la poursuite du traitement après un échec d’envoi de l’avatar.
- Correction des faux changements de réglages parfois détectés immédiatement après un précalcul.
- Amélioration de l’importation automatique des mouvements de déplacement.
- Ajout d’avertissements pour les avatars utilisant certains rigs particuliers.
- Ajout d’une confirmation des modifications et d’un miroir des valeurs dans la fenêtre d’ajustement de l’animation (merci à kraihd).
- Ajout d’explications lorsqu’un autre outil produit des erreurs et de vérifications des points sujets aux erreurs.

## [3.0.6] - 2026-03-17
- Correction des poses fortement déformées sur certains avatars disposant déjà de leur propre système de posture.

## [3.0.5] - 2026-01-31
- Correction de plantages de Unity avec certains ExpressionMenus.
- Correction des problèmes d’avatar avec le Prefab d’intégration ごろ寝EX (merci à あずき).
- Correction des rotations incorrectes lors de la fusion d’animations d’ajustement.

## [3.0.4] - 2026-01-11
- Amélioration de certains comportements lorsqu’un avatar est placé dans des Prefabs.
- Ajout de la suppression de licence.
- Correction des problèmes de cou affectant de nombreuses poses.
- Légère réduction du coût d’ouverture de l’Inspector.
- Génération des icônes aussi pendant le précalcul et modification du texte du bouton de mise à jour de l’avatar.
- Masquage des avertissements si aucun précalcul n’a jamais été effectué, cette fonction semblant peu utilisée.
- Suppression de ressources inutiles.
- Correction de l’impossibilité d’ajuster les doigts.

## [3.0.3] - 2026-01-04
- Correction de l’application d’un autre préréglage que celui choisi (merci à あずき).

## [3.0.2] - 2026-01-02
- Prise en charge du thème sombre de Unity.
- Correction des déformations de pose après rotation dans l’ajustement de l’animation.
- Modification de la taille des icônes de menu générées, activation de leur compression et ajout d’une option pour des icônes encore plus petites.
- Rétablissement de l’intégration du suivi et de l’importation automatique des mouvements de l’avatar, supprimées en 3.0.0.

## [3.0.1] - 2025-12-29
- Correction de la configuration des mouvements par défaut.
- Correction des postures d’avatar qui se déformaient spontanément.

## [3.0.0] - 2025-12-28
- Ajout du précalcul.
- Ajout de l’ajustement de l’animation.
- Ajout des préréglages.

## [2.2.5] - 2025-05-08
- Résolution de problèmes de compatibilité avec certains systèmes.

## [2.2.4] - 2025-04-04
- Correction des changements de pose impossibles avec certains avatars et certaines versions de ModularAvatar.

## [2.2.3] - 2025-03-13
- Correction des mouvements incorrects sur certains avatars, notamment lors des sauts de 森羅.
- Correction des valeurs initiales parfois incorrectes de Sans gestes.

## [2.2.2] - 2025-03-06
- Correction d’erreurs dans certains environnements.

## [2.2.1] - 2025-02-05
- Correction d’un plantage de Unity.
- Ajout de la version au nom de l’outil affiché dans NDMF Console.

## [2.2.0] - 2025-02-02
- Ajout d’un réglage pour désactiver les gestes et utiliser les mains de l’animation.

## [2.1.6] - 2025-02-02
- Correction des poses déformées en combinaison avec certains outils.
- Correction d’erreurs dépendant des réglages.

## [2.1.5] - 2024-12-10
- Résolution de conflits avec certains systèmes.
- Ajout d’un bouton de suppression de la version Unitypackage.

## [2.1.4] - 2024-12-08
- Correction d’erreurs sur certains avatars.
- Correction du verrouillage des pieds inutilisable en cas de conflit avec des systèmes ou avatars.
- Correction du retour des poses aux postures standards de VRC en mode bureau.

## [2.1.3] - 2024-12-01
- Correction des changements de pose impossibles avec des avatars ou systèmes utilisant VRMode avec un type incorrect.
- Ajout d’un signalement dans NDMF-Console lorsque des systèmes entrent en conflit à cause de types de paramètres différents.

## [2.1.2] - 2024-12-01
- Ajout d’une explication dans NDMF-Console lorsqu’une erreur d’un autre outil entraîne une erreur de Unisaki Posing System pendant le build de l’avatar.
- Correction d’erreurs de build sur certains avatars.
- Amélioration du comportement juste après le chargement de l’avatar.
- Suppression du traitement de déplacement d’origine de l’avatar, qui empêchait le fonctionnement sur certains avatars.

## [2.1.1] - 2024-11-20
- Correction de problèmes liés à MergeAnimator dans PosingOverride.

## [2.1.0] - 2024-11-20
- Résolution de l’enfoncement des pieds et des hanches dans le sol en mode bureau afin de prendre proprement toutes les poses.
- Ajout du changement de pose indépendamment de la hauteur de la tête.
- Ajout d’un mode sans génération d’icônes pour l’envoi d’avatars Quest.
- Ajout de plusieurs avertissements en cas de problème avec l’avatar.
- Résolution des conflits entre les verrouillages et l’AFK ou les systèmes de verrouillage des bras d’autres outils.
- Ajout du verrouillage automatique du cou sur le dos.

## [2.0.0] - 2024-11-12
- Publication de la version compatible VPM/VCC.
- Modification du nom de l’outil affiché lors des erreurs #1.
- Ajout des conditions d’utilisation #5.
