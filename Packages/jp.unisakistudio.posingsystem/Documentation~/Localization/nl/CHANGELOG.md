# Wijzigingsgeschiedenis

## [3.2.0] - Niet uitgebracht

### Toegevoegd

- Inspector, bewerking, installatiedialogen, standaardposenamen en VRChat-menu’s in 30 talen.
- Eén taalkeuze voor Inspector en spelmenu; aanvankelijk de computertaal en daarna de laatste keuze voor dezelfde systeemgebruiker.
- README, geschiedenis en offline handleiding openen in de gekozen taal.
- 128 standaardnamen en functiebeschrijvingen afgestemd op bestaande termen.

### Compatibiliteit

- Bestaande avataropties, eigen namen, assetverwijzingen, parameter-ID’s en licentieopslag behouden.

## [3.1.2] - 2026-11-09

- Omhoog gericht gezicht na opstaan uit buikligging met hoofdrotatiecorrectie hersteld.
- Hoofdvergrendeling onder Instellingen → Automatisch hoofd vast op rug blijft beschikbaar zonder Voethoogte. In 3.1.0/3.1.1 verdwenen items keren na update/build terug.
- Voorberekening bijwerken werkt met uitgeschakelde avatarroot.
- Oude gedeelde Controller in Assets verhindert geen generatie na update meer; eigen bewerkingen blijven.
- Uitgeschakeld tool-/ouderobject dat gedeelde Animator blokkeert wordt bij naam getoond en via Instellingen controleren geselecteerd.
- Voorberekeningsdetails tonen welke opties sinds opslag gewijzigd zijn.
- Bij scriptfouten (PS050) tot 5 oorspronkelijke Unity-fouten zichtbaar. De hulpaanvraag opent een te controleren/bewerken concept met versies en foutnummer om te verzenden.

Werk gebruikte voorberekening na de update bij, sla de scène op en bouw/upload opnieuw.

## [3.1.1] - 2026-11-08

- Actieve Vloerhoogte en Omdraaien verschijnen in EX-Instellingen zonder Spiegelen.
- Juiste draaiing voor Benen gestrekt, Ontspannen en Eén knie omhoog behouden na oud ordenen/verwijderen/hernoemen of volledige Unpack met herkenbare bron. Eigen poses, bewust uitgeschakeld Omdraaien en vervangen zijbewegingen blijven.
- Korte buikligging bij avatarselectie en korte rugligging na sprong met alleen buikpose verholpen. Sprongwissel uit te zetten onder Functies → Gemaksfuncties → Liggende poses wisselen door springen; standaard aan.
- Herstelbare verdwenen Animator Controller-verwijzing geeft waarschuwing en wordt uit opties herbouwd; gedeelde verwijzingen blijven.
- Compilatiefouten stoppen de build vroeg met uitleg. Automatisch opruimen wacht zolang referenties niet te controleren zijn.
- Officiële Prefab gekozen bij naamgenoten in het project. Alleen andere regeleinden in de Controller verhinderen voorberekening niet meer.
- Write Defaults van extra FX-lagen zoals Vloerhoogte volgen avataropties.

Werk de gebruikte voorberekening na de update bij op de tool in de scène, sla op en bouw/upload opnieuw.

## [3.1.0] - 2026-11-07

- IK voor ondersteunde poses, avatarspecifieke correcties en bescherming tegen achterwaartse kniebuiging.
- Fotomodus met positie/richting, synchronisatie en zichtbaar eigen hoofd naar keuze.
- Vloerhoogte beweegt het hele lichaam 3m omhoog/omlaag, los van Voethoogte.
- Ademhaling, onderdrukking van standaardrustbeweging en correctie bij afwijkende nekrichting.
- Functies/verbruik in Inspector, zoeken, meer clips, ordenen en eenvoudige Scene-voorbeelden.
- Desktopwissel staan/hurken/zitten versneld.
- Voorberekeningsmeldingen, scrolllast en opruimpauzes verbeterd.
- Onterechte melding en regeneratie bij upload met automatische import en ongewijzigde voorberekening hersteld. Import werkt op de gegenereerde avatar zonder originele handmatige opties te veranderen.
- Omdraaien door hoofdhelling en bewegingen per pose toegevoegd.
- Overname van oude voorinstellingen, eigen import en gegenereerde assets verbeterd.

## [3.1.0-beta.2] - Open bèta (2026-11-05)

- Vaste lengte en hoogte-/diepteverschil (Y/Z) tussen Head en Viewpoint als IK-lengtereferentie.
- Waarschuwingen ook links van Hierarchy-namen, inclusief ontbrekende/verouderde voorberekening en import-/voorinstellingsmeldingen.
- Lang vastlopen door laden van Controllers uit andere scènes tijdens automatisch opruimen hersteld.
- Verlies van voorberekeningshistorie na pose-/functiewijziging hersteld. Nu wijziging na berekening gemeld in plaats van nooit uitgevoerd, ook in Hierarchy.
- Vergelijkingsresultaten voor Inspector/Hierarchy gedeeld om scrollen zonder wijzigingen te verlichten; bij build altijd vergelijken.

## [3.1.0-beta.1] - Open bèta (2026-11-04)

- Normale desktopwissel met één evaluatie voor build en voorberekening; VR en uitzonderingen behouden de oude route.
- Alpha.3 en oudere voorberekeningen als te vernieuwen herkend.
- Blik behouden bij Vloerhoogte in Fotomodus en daarna naar laatste hoogte terug.

- Automatische import detecteert alleen bij generatie en wijzigt geen handmatige opties tijdens build/voorberekening.
- Regeneratie volgt automatische activering, Base-wijzigingen en handmatig wissen, met behoud van gedeelde aanpassingen en herstel van standaardbewegingen.
- Eén Controllerbestand per avatar en scène-GUID; verwijzingen van duplicaten/andere scènes/Prefabs gescheiden voor bijwerken.
- Tijdelijke NDMF-assets bij build, zonder extra bewerkingsbestanden.
- Controle bij openen/opslaan; niet-opgeslagen werk, Undo/Redo en externe verwijzingen beschermd. Alleen zeker ongebruikte eigen bestanden naar prullenbak.
- Rotatie, aanpassing en afspeelpositie van import toegepast; rotatie, RootT/RootQ en Humanoid-verwerking gedeeld met gewone poses.
- Geneste BlendTree-aanpassing gekopieerd voor het doel met behoud van bron en loopmenging.
- Alleen-staan-afspeelpositie op originele beeldsnelheid tot statische pose uitgewerkt zonder lopen te stoppen.

## [3.1.0-alpha.2] - Gesloten test (2026-10-26)

### Correcties en wijzigingen

- Fotomodus/Vloerhoogte ook voor normale/eigen bewegingen zonder pose.
- Vloerhoogte tot 3m boven/onder: 50% origineel/begin, 0% 3m lager, 100% 3m hoger.
- Bron-BlendTree gewijzigd bij alleen-staan-override hersteld.
- Handmatige correctie zonder IK-regels en onnodige meldingen onderdrukt.
- Verwarde concepten bij gelijke namen en gemiste bewegingswissels binnen één asset hersteld.
- Foute startselectie via nummer van uitgeschakelde beginpose hersteld.
- Uitleg in plaats van uitzondering bij niet-ondersteunde AnimatorOverrideController.
- Bewust uitgeschakeld Omdraaien behouden in oude voorinstellingen.
- Functiepictogrammen duidelijker in licht thema.

Controle van blik en vervangen standaardbewegingen in de VRChat-client loopt door.

## [3.1.0-alpha.1] - Voor publicatie

### Toegevoegd

- Activering per functie en ExpressionParameters-verbruik.
- Fotomodus voor verplaatsen/draaien.
- Synchronisatie optioneel: 0 bit lokaal, 16 bit gedeeld.
- Eigen hoofd zichtbaar in Fotomodus.
- Vloerhoogte beweegt lichaam/hoofd 3m boven/onder, 50% origineel/begin, 0% 3m lager, 100% 3m hoger.
- Omdraaien door hoofdhelling en animaties per pose.
- Ademhaling Zwak/Gemiddeld/Sterk/Snel/Aangepast.
- Beweging en menu apart: 0 bit beweging, 1 bit gesynchroniseerd schakelen.
- Standaardwiegen van VRChat uitschakelen.
- Vervorming door nekrichting corrigeren.
- Automatische ledemaat-IK voor ondersteunde poses, globaal of per pose.
- IK in aanpassingen vastleggen slaat resultaat als animatie op.
- Avatarcorrecties, knieachterwaartsbescherming en minimale buiging.
- Eenvoudig voorbeeld in Scene.
- Categorietabs, zoeken, meer clips en doelvolgorde.
- Product-/systeemgeschiedenis in Inspector.

### Wijzigingen

- Voorinstellingen sneller bij tool toevoegen en lijst openen.
- Lijst automatisch vernieuwd na toevoegen/bewerken/verwijderen.
- Inspector in 3 tabs: Functies, Poses en Avatarinstellingen.
- Uitleg/icons toegevoegd en details uniform openklapbaar.
- Ademhaling naar functies en voorinstellingen onderaan avataropties.
- Eigen bewegingstoewijzing los van automatische oorspronkelijke import.
- Minder dubbele voetanimaties van het hele lichaam.
- Miniatuurbijwerking met bevestiging.
- Waarschuwingen bij Instellingen controleren.
- Nieuwe 8bit-Prefabs vervangen door normale functiekeuze, bestaande verwijzingen behouden.
- Minimum VRChat Avatars SDK 3.10.4.

### Correcties

- Fotomodus en vloer ook voor bewegingen zonder pose.
- Originele BlendTree behouden bij alleen-staan-override.
- Handmatige aanpassing zonder geldige IK, onnodige IK-meldingen weg.
- Gelijke posenamen en onzichtbare bewegingswissels binnen een asset hersteld.
- Hergebruik van uitgeschakeld startnummer kiest niet onterecht een andere pose.
- Melding bij niet-ondersteunde AnimatorOverrideController tijdens oorspronkelijke import.
- Omdraaien bewust uit behouden in oude voorinstellingen.
- Pictogramachtergrond verbeterd voor licht thema.

- Ontbrekende Omdraaien-/Vloerhoogte-/Fotomodus-iconen bij toevoegen hersteld.
- Verloren Omdraaien met oude voorinstellingen hersteld.
- Naamsinvoer springt niet naar een andere pose bij openen van details.
- Dichte details na ordenen en Undo/Redo hersteld.
- Overlappend origineel en dubbel getelde plaatsing in voorbeeld hersteld.
- Voorbeeldfouten met ontbrekende componenten hersteld.
- IK-positie/-richting door botassen en schalen hersteld.
- Waarschuwingen/voorberekening direct na wijzigingen vernieuwd.
- Geen oude voorberekening na IK-wijzigingen.
- Build-fouten van extra functies in sommige configuraties hersteld.

## [3.0.16] - 2026-09-14
- Minimum NDMF 1.14.0 vanwege de API `VirtualAnimatorController.SetParameter`.

## [3.0.15] - 2026-09-13
- Grote lichaams-/knieverplaatsing bij nekvergrendeling op de rug met Hips-schaalwijziging en FloorAdjuster hersteld.

## [3.0.14] - 2026-09-09
- Verdwenen Gebaren uit vanaf de tweede tool op één avatar hersteld.
- Ongewenste terugkeer van USSPS_Locomotion naar Standing ondanks SleepDown en dergelijke hersteld.
- Hoogte/X/Z met FloorAdjuster gecorrigeerd, inclusief MA-/AAO-volgorde.
- PosingSystem_Locomotion automatisch hersteld na verwijderen van voorberekende AnimatorController in Generated.
- Build-fout zonder staande poses hersteld.
- Voorinstellingen op avatarnaam gesorteerd.

## [3.0.13] - 2026-09-04
- Zelftransitie naar staan die houdingsherstel instabiel maakte bij direct opstaan na beginnen met hurken in VR hersteld.

## [3.0.12] - 2026-09-04
- Foutieve wijzigingsdetectie zonder voorberekening tijdens NDMF-build hersteld.
- `AddAssetToSameFile` bij toevoegen van assets aan tijdelijke AnimatorController tijdens NDMF hersteld.

## [3.0.11] - 2026-08-31
- Ongeldige Scene voor voorberekeningsavatar met NDMF 1.14.7+ hersteld.
- `MissingReferenceException` door vernietigd LocalizationAsset in NDMF-foutvenster hersteld.

## [3.0.10] - 2026-08-29
- Voorinstellingsassets pas laden bij openen van selectie; cache hergebruiken bij gelijke GUID-index.
- Hierarchy-iconen alleen na wijzigingen evalueren en menuzoeken delen in cache; periodiek vastlopen in scènes met veel tools verminderd.
- `ArgumentException` door dubbele parameters bij meerdere tools in één voorberekening hersteld.

## [3.0.9] - 2026-08-26
- NullReferenceException voorkomen als VRCAvatarDescriptor niet uit PosingSystem kan worden verkregen, met verwijzing naar benodigde logs.
- `Avatar is null` door hergebruik van NDMF-tijdelijke voorbeeldavatars in latere builds/voorberekeningen hersteld. Conversie los van voorbeeldcache en botsende tijdelijke namen voorkomen.
- Niet-opgeslagen voorbeeldverwijzing van 「可愛いポーズ」 en groeiend aantal verborgen avatars hersteld. Voorbeelden uitgesloten van scèneopslag en oude opgeruimd bij laden, Play en scriptverversing.

## [3.0.8] - 2026-08-18
- Unity Assertion en risico op ongeldige posedata bij hercorrectie van hoogte met FloorAdjuster e.d. hersteld.
- Hoogtemeting beschermd tegen invloed op originele avatar en AnimationMode van andere tools.

## [3.0.7] - 2026-08-12
- Verschoven blik en abnormale knieën in rugligging met hoogtetools zoals FloorAdjuster hersteld.
- Fout met PosingSystem buiten avatar hersteld (dank aan PancakeTorto1se).
- Doorgaan na mislukte upload en vervolgerrors hersteld.
- Onterechte wijzigingen direct na voorberekening hersteld.
- Automatische bewegingsimport verbeterd.
- Waarschuwingen voor bijzondere rigs toegevoegd.
- Wijzigingsbevestiging en spiegelen van aanpassingswaarden toegevoegd (dank aan kraihd).
- Uitleg bij fouten van andere tools en controles op foutgevoelige punten toegevoegd.

## [3.0.6] - 2026-03-17
- Sterk vervormde houdingen op avatars met eigen postuurbesturing hersteld.

## [3.0.5] - 2026-01-31
- Unity-crash met bepaalde ExpressionMenus hersteld.
- Problemen met ごろ寝EX-integratieprefab hersteld (dank aan あずき).
- Rotatiefout bij combineren van aanpassingsanimaties hersteld.

## [3.0.4] - 2026-01-11
- Avatar in Prefabs deels verbeterd.
- Licentieverwijdering toegevoegd.
- Nekproblemen in veel poses hersteld.
- Inspector iets lichter te openen.
- Pictogrammen ook bij voorberekening en tekst van avatarbijwerking gewijzigd.
- Geen waarschuwing zonder ooit voorberekenen, omdat veel gebruikers dit niet lijken te doen.
- Overbodige bronnen verwijderd.
- Niet-aanpasbare vingers hersteld.

## [3.0.3] - 2026-01-04
- Verkeerde voorinstelling toegepast hersteld (dank aan あずき).

## [3.0.2] - 2026-01-02
- Donker thema ondersteund.
- Posevervorming na draaien in animatieaanpassing hersteld.
- Pictogramformaat/compressie gewijzigd en kleiner formaat optioneel.
- Trackingintegratie en automatische import teruggebracht na verwijdering in 3.0.0.

## [3.0.1] - 2025-12-29
- Standaardbewegingsopties hersteld.
- Spontaan vervormde poses hersteld.

## [3.0.0] - 2025-12-28
- Voorberekening toegevoegd.
- Animatieaanpassing toegevoegd.
- Voorinstellingen toegevoegd.

## [2.2.5] - 2025-05-08
- Compatibiliteit met bepaalde systemen verbeterd.

## [2.2.4] - 2025-04-04
- Niet wisselende poses bij bepaalde avatars en ModularAvatar-versies hersteld.

## [2.2.3] - 2025-03-13
- Foute bewegingen zoals springen van 森羅 hersteld.
- Verkeerde beginwaarde van Gebaren uit hersteld.

## [2.2.2] - 2025-03-06
- Fouten in bepaalde omgevingen hersteld.

## [2.2.1] - 2025-02-05
- Unity-crash hersteld.
- Versie bij naam in NDMF Console toegevoegd.

## [2.2.0] - 2025-02-02
- Gebaren uit voor animatiehanden toegevoegd.

## [2.1.6] - 2025-02-02
- Vervorming met bepaalde tools hersteld.
- Instellingsafhankelijke fouten hersteld.

## [2.1.5] - 2024-12-10
- Conflicten met bepaalde systemen opgelost.
- Verwijderknop Unitypackage toegevoegd.

## [2.1.4] - 2024-12-08
- Avatarafhankelijke fouten hersteld.
- Onbruikbare voetvergrendeling door conflicten hersteld.
- Terugkeer naar VRC-standaard in desktop hersteld.

## [2.1.3] - 2024-12-01
- Posewissel met onjuist VRMode-type in avatars/tools hersteld.
- Parameterconflicten van verschillende typen getoond in NDMF-Console.

## [2.1.2] - 2024-12-01
- Uitleg in NDMF-Console bij fouten van andere tools die het systeem tijdens build laten falen.
- Build-fouten bij bepaalde avatars hersteld.
- Gedrag direct na laden verbeterd.
- Originele locomotie verwijderd wegens onbruikbaarheid op bepaalde avatars.

## [2.1.1] - 2024-11-20
- MergeAnimator-problemen in PosingOverride hersteld.

## [2.1.0] - 2024-11-20
- Inzakkende voeten/heupen op desktop hersteld voor alle poses.
- Wisselen onafhankelijk van hoofdhoogte toegevoegd.
- Geen pictogrammen voor Quest-upload optioneel.
- Avatarwaarschuwingen toegevoegd.
- Conflicten met AFK en armvergrendeling van andere tools opgelost.
- Automatische nekvergrendeling op de rug toegevoegd.

## [2.0.0] - 2024-11-12
- VPM/VCC-versie uitgebracht.
- Weergavenaam bij fouten gewijzigd #1.
- Voorwaarden toegevoegd #5.
