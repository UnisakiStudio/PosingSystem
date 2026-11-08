# Unisaki Posing System

Gedeeld posesysteem voor Kawaii Posing, Cool Posing en verwante tools. Met Modular Avatar en NDMF maakt het bij de avatarupload de benodigde menu’s en animaties.

Deze README geldt voor v3.2.0 in ontwikkeling. De officiële releasedatum staat nog niet vast. De versie staat boven in de Inspector. Zie ook [toevoegingen en wijzigingen](CHANGELOG.md).

## Weergavetaal en offline documentatie

Kies de taal bij Language / 言語 boven in de Inspector. Eén keuze geldt voor de Inspector en de gegenereerde VRChat-menu’s. Upload de avatar opnieuw om de taal in het spel te veranderen.

Zonder expliciete keuze wordt de computertaal gebruikt; niet-ondersteunde talen gebruiken Engels. Windows, macOS en Linux worden ondersteund. De laatste keuze wordt in EditorPrefs bewaard voor andere projecten van dezelfde systeemgebruiker op deze computer. Een expliciete keuze in een bestaande avatar blijft behouden. Er is geen synchronisatie met andere computers. Licenties blijven op dezelfde manier opgeslagen.

Kies de taal van README, wijzigingen en handleiding in de meegeleverde [talenlijst](../index.html). De handleiding en wijzigingen in de Inspector volgen die keuze. Originele voorwaarden en referentievertalingen blijven afzonderlijk beschikbaar.

## Houding wisselen op desktop

De vertraging voordat de gekozen pose verschijnt bij wisselen tussen staan, hurken en zitten is verkort. Dit werkt voor gewone builds en voorberekening, zonder speciale opdrachten of extra instellingen. VR en bijzondere situaties behouden de oude verwerking. Werk oude voorberekeningen bij voordat je uploadt.

## Installeren en bijwerken

Voeg de posetool met VCC of ALCOM toe volgens de gekochte bestanden en instructies. Dit systeem wordt als afhankelijkheid geïnstalleerd.

Voor v3.2.0 zijn de volgende onderdelen nodig.

- VRChat-avatarproject met Unity 2022.3
- VRChat Avatars SDK 3.10.4 of hoger
- Modular Avatar 1.x, versie 1.15.1 of hoger
- NDMF (Non-Destructive Modular Framework) 1.x, versie 1.14.0 of hoger

Werk Kawaii Posing in ALCOM / VCC bij naar 3.2.0 en controleer dat Unisaki Posing System ook 3.2.0 is. Gebruik je voorberekening, klik dan op Voorberekening bijwerken, sla de scène op en upload de avatar.

Cool Posing 1.2.0 gebruikt het gedeelde systeem 3.2.0. Werk product en systeem samen bij naar de bijbehorende versies.

### Voor gebruikers van de 8bit-versie

Nieuwe installaties gebruiken alleen de normale Prefab. Schakel overbodige functies uit om verbruik te beperken. Alleen poseselectie gebruikt standaard 8 bit.

Oude 8bit-Prefabs blijven in Editor/Legacy van elk product staan om verwijzingen van avatars en voorinstellingen te behouden. Je hoeft een bestaande Prefab niet te verwijderen of vervangen. De 8bit-versie is niet meer via het toevoegmenu beschikbaar.

## De instellingen lezen

Selecteer 「可愛いポーズ」, 「カッコいいポーズ」 of het bijbehorende object in je avatar in de Hierarchy.

| Tabblad | Instellingen |
| --- | --- |
| Functies | Spelmenu’s, gemaksfuncties, pictogrammen en ExpressionParameters-verbruik |
| Poses | Selectie, toevoegen, volgorde, Scene-voorbeeld, details en animatieaanpassing |
| Avatarinstellingen | Lichaams- en kniecorrecties, oorspronkelijke/eigen bewegingen en voorinstellingen |

Met de koppeling voor instellingen open je details onder de betreffende optie. Ademhaling staat ook onder Functies.

Hulp / Wijzigingen bovenaan opent handleiding, ondersteuning en geschiedenis. Je kunt wisselen tussen de geschiedenis van het product en Unisaki Posing System.

## Functies en ExpressionParameters-verbruik

Keuzes onder Functies > Menufuncties verschijnen in het spel. Spiegelen toevoegen maakt het effect in VRChat schakelbaar; het dwingt niet elke pose voortdurend te spiegelen.

| Functie | Extra bits |
| --- | ---: |
| Spiegelen | 1 bit |
| Gebaren uit | 1 bit |
| Ademhaling animeren | 0 bit |
| Ademhaling in het spel schakelen | 1 bit |
| Voethoogte | 8 bit |
| Vloerhoogte | 8 bit |
| Fotomodus alleen lokaal | 0 bit |
| Fotomodus synchroniseren | 16 bit |
| Hoofd zichtbaar houden in Fotomodus | 0 bit |
| Standaardwiegen van VRChat uitschakelen | 0 bit |
| Vervorming door hoofdrotatie verminderen | 1 bit (gedeeld met Omdraaien) |
| Omdraaien | 10 bit (waarvan 1 bit gedeeld met de vervormingscorrectie) |

Omdraaien gebruikt 1 bit voor activering, 8 bit voor hoofdhelling en 1 bit voor nekvergrendeling. Met vervormingscorrectie al actief kost Omdraaien toevoegen dus 9 bit extra. Het gedeelde deel telt niet dubbel; de weergave volgt de gekozen functies.

Het totaal betreft de posetools in de avatar, niet de hele avatar met kleding en andere systemen. Gedeelde opties van meerdere tools tellen niet dubbel. Meer poses dan het basisnummerbereik kunnen extra selectiebits vereisen.

### Voethoogte en Vloerhoogte

- Voethoogte: verschuift de voetkant met behoud van de hoofdpositie. Gebruik dit voor aansluiting op vloer of bed.
- Vloerhoogte: verplaatst het hele lichaam inclusief hoofd omhoog/omlaag. 50% is oorspronkelijk, 0% is 3m lager en 100% is 3m hoger. De beginwaarde is 50%.

Beide staan onder Instellingen in het spel, Vloerhoogte na Voethoogte. Buiten het midden verplaatst het hele lichaam; terug bij 50% herstelt de hoogte en stopt de correctie. Er is geen aparte zweefschakelaar. De blik wordt gecorrigeerd bij het begin van zweven en tijdens hoogtebediening.

Bij normale animaties dupliceert de voethoogteaanpassing niet langer de volledige lichaamsbeweging. Dat beperkt animatiedata, maar maakt niet iedere hele avatar automatisch drie keer kleiner.

### Fotomodus

1. Activeer Functies > Menufuncties > Fotomodus.
2. Open in VRChat Fotomodus en kies een pose bij Pose kiezen.
3. Afstand verplaatst tot 3m voorwaarts; Richting draait. Boven 0 kijkt de avatar naar je toe.
4. Zet Voeten vast wanneer nodig. Dat wordt niet direct afgedwongen, zodat je eerst kunt positioneren. Met Gebaren uit in het menu kun je ook handvormen hier wisselen.
5. Foto afsluiten herstelt afstand/richting en ontgrendelt de voeten.

Fotobeweging en -rotatie kosten 0 bit alleen lokaal. Activeer Gemaksfuncties > Fotomodus synchroniseren zodat anderen positie/richting zien. Samen gebruiken ze 16 bit.

Verdwijnt je nabije avatarkop, activeer dan Gemaksfuncties > Hoofd zichtbaar houden in Fotomodus. Het geldt alleen voor je eigen zicht tijdens foto’s en keert daarna terug naar normaal.

### Liggende poses wisselen door springen

Met rug- en buikpose beide gekozen kun je wisselen door ter plaatse te springen. Met slechts één blijft die pose behouden. Schakel Liggende poses wisselen door springen uit onder Functies → Gemaksfuncties in Unity indien gewenst. Extra gesynchroniseerd verbruik is 0 bit. Bij meerdere tools geldt uitschakelen in één voor de hele avatar.

### Omdraaien

In ondersteunde liggende poses rolt het lichaam soepel mee als je je hoofd zijwaarts kantelt. Niet elke ligpose wordt automatisch ondersteund.

1. Activeer Functies > Menufuncties > Omdraaien.
2. Open Details van de gewenste pose en zet Omdraaien inschakelen aan.
3. Wijs een zijanimatie toe aan Zijpose voor omdraaien. Originele en zijpose vormen de draai naar links/rechts. Pas Sterkte van grondcorrectie zo nodig aan.
4. Kies de pose in VRChat en activeer Omdraaien met ontgrendelde nek.

Bij Omdraaien bepaalt de hoofdhelling de afspeelpositie; gewone MotionTime is dan niet beschikbaar. Gebruik voor een complete draai-animatie het normale animatieveld en laat Zijpose voor omdraaien leeg. Maak links op 0, rugligging op 0.5 en rechts op 1.

### Ademhaling en standaardwiegen

Functies > Gemaksfuncties > Ademhaling animeren beweegt borst en andere delen mee. Kies per categorie Geen, Zwak, Gemiddeld, Sterk, Snel of Aangepast. Snel is versneld ademen; Aangepast gebruikt je eigen animatie.

De beweging alleen kost 0 bit. Zet ook Menufuncties > Ademhaling aan voor stoppen/hervatten in VRChat. Het schakelen gebruikt 1 gesynchroniseerde bit. Andere sterkte kost niet meer bits.

Nek, schouders en armen bewegen mee met de borst om hoofd- en handverplaatsing te beperken. Het is geen volledige positievergrendeling; kies naar lichaamsbouw en pose.

Gemaksfuncties > Standaardwiegen van VRChat uitschakelen onderdrukt de normale lichaamsbeweging in rust. Standaard uit; toolademhaling en eigen bewegingen blijven behouden. Het werkt ook zonder ademhaling en kost geen extra bits. Andere oorzaken van wiegen worden niet uitgeschakeld.

Met meerdere tools geldt uitschakelen van standaardwiegen in één voor de hele avatar. Zet het in alle tools uit en upload opnieuw om het te herstellen.

### Vervorming door hoofdrotatie verminderen

Gebruik dit als ontgrendelen van de nek de pose vervormt doordat de richting afwijkt van het origineel. Onder Functies > Gemaksfuncties, zonder apart spelmenu. Met vergrendelde nek geldt de oorspronkelijke richting. Rugligging is uitgezonderd om omhoog kijken te behouden.

### Menupictogrammen

- Posepictogrammen genereren: maakt afbeeldingen van je avatar in elke pose voor het menu. Herkenbaarder, maar de beelden verlengen de build.
- Pictogramgrootte verminderen: verlaagt resolutie en avataromvang.

Android genereert geen posepictogrammen. Gebruik Pictogrammen opnieuw vastleggen na bijvoorbeeld kledingwijzigingen. Poseminiaturen bijwerken vernieuwt de lijstbeelden. Lees de bevestiging, want dit kan tijd kosten.

## Poses toevoegen, ordenen en bekijken

Kies links een categorie. De bovenste loep zoekt namen over alle categorieën. Details bundelt animatie, pictogram, MotionTime en andere opties.

Sleep meerdere animaties naar het toevoeggebied of selecteer ze in Project en klik In Project geselecteerde clips importeren. Dubbele verwijzingen naar dezelfde clip worden verwijderd.

Kies de bestemming onder Details > Volgorde of gebruik omhoog/omlaag. Open/dicht van de details blijft behouden na verplaatsen.

Voorbeeld toont de pose in Scene zonder het aanpassingsvenster. Na afsluiten keert de originele weergave terug. Dit voert MA/NDMF niet uit en kan afwijken van ingebouwde kleding of FloorAdjuster. Controleer kleding en grondcontact ook na de avatarbuild.

## Animatieaanpassing en automatische ledemaataanpassing (IK)

Open Poses > Animatieaanpassing om lichaam en ledematen met voorbeeld nauwkeurig te wijzigen. Sla daarna de aanpassingsanimatie op met de vensteropdrachten.

Ondersteunde producten/poses passen ledematen automatisch aan de bouw aan. Automatische ledemaataanpassing inschakelen geldt globaal, IK voor deze pose inschakelen individueel. Ontbrekende automatische ondersteuning wordt gemeld.

Klik IK in aanpassingen vastleggen voor handmatige afwerking. Het huidige resultaat komt in een nieuwe aanpassingsanimatie en de IK van die pose/avatar gaat uit. De bronanimatie blijft. Globale en pose-IK moeten aan zijn en toepasbare regels hebben.

Achterwaartse kniebuiging voorkomen onder Avatarinstellingen schakelt kniecorrectie. Details bevat Minimale kniebuiging (graden). Avatarspecifieke correctie inschakelen vereist passende data. Sommige correcties combineren niet met ledemaat-IK.

## Oorspronkelijke/eigen bewegingen en voorinstellingen

Deze staan onder Avatarinstellingen.

- Oorspronkelijke staande, hurkende en buikposes gebruiken importeert automatisch bestaande avatarhoudingen.
- Eigen bewegingen gebruiken opent aparte import per type, zoals staan, hurken (Crouching) of buikligging (Prone). Gebruik in principe één beweging per type.
- Trackingvergrendelingen met andere gereedschappen integreren helpt als vergrendeling met andere systemen slecht werkt.

Eigen bewegingen aanwijzen staat los van automatische overname van oorspronkelijke houdingen.

Voorinstellingen staan onderaan. Zoek op naam en kies Deze voorinstelling toepassen om de huidige pose-instellingen te vervangen. Vooral voor de eerste inrichting; sla eerdere aanpassingen eerst op.

De lijst laadt alleen namen en avatarinformatie. Aanpassingsanimaties pas bij toepassen van de gekozen voorinstelling, ook niet allemaal vooraf bij de eerste lijst.

De lijst onderscheidt Geschikt voor avatar, Gebruikersvoorinstellingen en Officiële Unisaki Studio-voorinstellingen. Meegeleverde voorinstellingen verbergen geldt projectbreed. Toevoegen/bewerken/verwijderen ververst de lijst; gebruik Opnieuw laden indien nodig.

## Voorberekening en waarschuwingen

Met oorspronkelijke poses actief wordt Base bij iedere generatie herkend. Resultaten worden niet naar handmatige opties teruggeschreven; alleen automatisch importeren laat de lijst leeg. Base- en activeringswijzigingen gelden bij de volgende generatie.

Bij upload werkt de automatiek op de buildkopie, zonder originele scèneobjecten, importopties, broncontroller of bewegingen te wijzigen. Ongewijzigde invoer hergebruikt de voorberekening.

Handmatige toewijzing gaat voor per type, inclusief rotatie, aanpassing en afspeelpositie. Gebruiken uitzetten of een toegewezen beweging verwijderen blokkeert automatische import van dat type. Handmatige keuze wissen verwijdert opties/aanpassingen; met automatiek aan volgt herdetectie. Undo herstelt. Bij meerdere oude entries geldt de actieve in de Inspector.

Oude automatisch toegevoegde entries blijven, omdat ze niet van handmatige of aangepaste te onderscheiden zijn. Wis alleen de typen die weer automatisch moeten. Handmatige opties van andere producten op dezelfde avatar gelden ook voor de gedeelde oorspronkelijke houdingen.

Voorberekening versnelt builds door gegenereerde data te hergebruiken. Maak/vernieuw met Voorberekening bijwerken. Normale builds genereren ook het nodige; handmatige voorbereiding is niet elke keer vereist.

Na wijzigingen melden Instellingen controleren en Hierarchy-markeringen benodigde vernieuwing. Werk de voorberekening bij na pose- of IK-wijzigingen om latere builds te versnellen.

Waarschuwingsweergave staat in Instellingen controleren. Je kunt typen of Hierarchy-iconen verbergen, maar dat repareert niets en vernieuwt geen data. Fouten en noodzakelijke generatie blijven actief.

## Gebruiksnotities

- Voor 3-puntstracking en desktop. Posevergrendeling met volledige lichaamstracking wordt niet ondersteund.
- Bouw en aanpassingen kunnen extra correcties vereisen voor ledemaatcontact en kleding.
- Speciale Base-lagen en andere posesystemen kunnen onverwacht reageren.
- Fotomodus en Vloerhoogte corrigeren toolposes en normale/eigen bewegingen zonder gekozen pose. Controleer vervangingen en gezichtspunt in VRChat.

## Voorwaarden en ondersteuning

Broncode en bronnen zijn niet open source. Koop het product rechtmatig en accepteer de [meegeleverde voorwaarden](../../../%E3%82%86%E3%81%AB%E3%81%95%E3%81%8D%E3%83%9D%E3%83%BC%E3%82%BA%E3%82%B7%E3%82%B9%E3%83%86%E3%83%A0%E5%88%A9%E7%94%A8%E8%A6%8F%E7%B4%84%EF%BC%88%E6%97%A5%E6%9C%AC%E8%AA%9E%E7%89%88%EF%BC%89.pdf).

- [Kawaii Posing-handleiding](https://docs.unisakistudio.jp/kawaiiposing/manual/)
- [Cool Posing-handleiding](https://docs.unisakistudio.jp/coolposing/manual/)
- [Ondersteuning](https://help.unisakistudio.jp/)

Vermeld bij fouten product-/systeemversie, Unity, VRChat SDK, Modular Avatar, NDMF, avatar, reproductiestappen en de Console-fout.
