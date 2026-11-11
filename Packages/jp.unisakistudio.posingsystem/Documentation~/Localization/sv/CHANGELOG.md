# Ändringshistorik

## [3.2.0] - Ej utgiven

### Tillagt

- Inspector, redigeringsåtgärder, installationsdialoger, standardposnamn och genererade VRChat-menyer kan visas på 30 språk.
- Språkvalet för Inspector och spelmenyn är gemensamt. Datorns språk används först, och det senaste valet återanvänds i andra projekt för samma OS-användare.
- README, ändringshistorik och offlineanvisningar kan öppnas på valt språk.
- Visningsnamn för 128 standardposer och funktionsbeskrivningar har tagits fram med befintlig terminologi.

### Kompatibilitet

- Befintliga avatarinställningar, egna namn, assetreferenser, parameteridentifierare och licensens lagringssätt bevaras.

## [3.1.4] - 2026-11-11

- Åtgärdat att avatarspecifika animationer för hukande och magläge inte tillämpades, så att VRChats standardposer användes i stället. Det gäller både automatisk import och manuella tilldelningar.
- Åtgärdat att hår, glasögon och andra delar monterade med MA Bone Proxy stannade på sin ursprungliga plats vid redigering eller förhandsvisning av poser. De följer nu förhandsvisningsposen medan originalavatarens inställningar och placering bevaras.
- Åtgärdat fel i posekorrigering baserad på synvinkelns position vid bildtagning av poser som ”仰向け寝_お疲れ”. Bilderna använder en animation med de pose- och golvkorrigeringar som tillämpas vid bygge.

Uppdatera Unisaki Posing System till 3.1.4 i ALCOM / VCC. Kawaii Posing 3.1.2 och Cool Posing 1.1.0 kan användas vidare utan ändringar. Om du använde förberäkning trycker du på ”Uppdatera förberäkning” efter uppdateringen, sparar scenen och bygger och laddar upp avataren igen.

## [3.1.3] - 2026-11-09

- ”Uppdatera poseminiatyrer” och förberäkning på en duplicerad avatar tar inte längre bort originalavatarens posebilder.
- Åtgärdat att ”Inställningar ändrade” visades efter enbart ny bildtagning om förberäkningen hade gjorts före scenens första sparning eller i redigeringsläget för Prefab.
- Åtgärdat att förberäkningen kunde avbrytas mitt i om posedata från en tidigare förberäkning fanns kvar.
- Lättare öppning av listan när posebilder ännu saknas. Även väntan som låste Unity tills paketinformationen hämtats vid start av förberäkning eller bygge har setts över.
- Redigeringsinställningar för poseverktyg som är inaktiverade i avataren tas nu också bort vid PC- och Android-byggen.
- ”Fråga om det här felet” kan nu ta med senare diagnosnummer och andra uppgifter från Console i utkastet, även om de första felen inte avslöjar orsaken. Vid fel i klippinställningarna för ”Vänd dig” innehåller utkastet också punkter att kontrollera.

Uppdatera Unisaki Posing System till 3.1.3 i ALCOM / VCC. Kawaii Posing 3.1.2 och Cool Posing 1.1.0 kan användas vidare utan ändringar. Eftersom produkterna anger en äldre version av det gemensamma systemet kan en konfliktbekräftelse visas under uppdateringen. Om du använde förberäkning trycker du på ”Uppdatera förberäkning” efter uppdateringen, sparar scenen och bygger och laddar upp avataren igen.

## [3.1.2] - 2026-11-09

- Åtgärdat att ansiktet fortsatte peka uppåt efter uppresning från magläge med ”Minska förvrängning från huvudrotation” aktiverat.
- Huvudlås kan åter växlas via EX-menyns ”Inställningar → Automatiskt huvudlås i ryggläge” även med ”Fothöjd” avstängt. Poster som försvann i 3.1.0 eller 3.1.1 återkommer när avataren byggs efter uppdateringen.
- ”Uppdatera förberäkning” fungerar nu även om själva avataren är avmarkerad i Unity.
- Åtgärdat att posegenerering kunde misslyckas efter uppdatering när en äldre gemensam pose-Controller fanns kvar i Assets. Egna redigeringar av Controller-inställningar bevaras.
- Om verktyget eller dess förälder är inaktiverat och den gemensamma pose-Animatorn inte kan användas visas namnet. Öppna objektet via ”Kontrollera inställningar → Välj mål”.
- När förberäkningen visar ändrade inställningar kan skillnader mot föregående sparning ses via ”Detaljer → Kontrollera ändringar sedan sparning”.
- När skriptfel stoppar bygget (PS050) visas upp till 5 ursprungliga fel som Unity upptäckt. Via funktionen för att fråga om felet kan du granska, redigera och skicka ett utkast med Unity-/verktygsversioner och felnummer.

Om du använde förberäkning: tryck på ”Uppdatera förberäkning” efter uppdateringen, spara scenen och bygg och ladda upp avataren igen.

## [3.1.1] - 2026-11-08

- Aktiverade ”Golvhöjd” och ”Vänd dig” visas under ”Inställningar” i VRChats EX-meny även när ”Spegla” är avstängt.
- Rätt vändningar bevaras för ”Ben utsträckta”, ”Avslappnad” och ”Ett knä upp” även efter omordning, borttagning eller namnbyte i äldre versioner. Detta gäller också efter fullständig Unpack om ursprungsposen kan identifieras. Egna poser, uttryckligen avstängd vändning och utbytta sidrörelser bevaras.
- Åtgärdat ett kort magläge när avataren väljs i VRChat och ett kort ryggläge vid hopp med enbart magposer valda. Hoppväxling kan stängas av under ”Funktioner → Praktiska funktioner → Växla liggande poser genom att hoppa”. Den är på som standard.
- Om referensen till gemensam pose-Animator Controller försvinner efter uppdatering visas en varning och den återskapas från inställningarna när det är möjligt. Referenser som delas av flera avatarer bevaras.
- Vid kompileringsfel i projektet stoppas bygget med orsaksinformation i början. Automatisk städning av genererade Controllers pausas medan skriptfel hindrar referenskontroll.
- Det officiella Prefab prioriteras vid tillägg även om ett annat Prefab med samma produktnamn finns i projektet. Förberäkning kan också fortsätta om endast radsluten i den gemensamma Controllern skiljer sig.
- Write Defaults för FX-lager som poseverktyget lägger till, exempelvis golvhöjd, följer nu avatarens inställning.

Om du använde förberäkning: tryck på ”Uppdatera förberäkning” på verktyget i scenen efter uppdateringen, spara scenen och bygg och ladda upp avataren igen.

## [3.1.0] - 2026-11-07

- Tillagt automatisk lemanpassning (IK) efter kroppsbyggnad för stödda poser, avatarspecifik korrigering och skydd mot bakåtböjda knän.
- Avatarens position och riktning kan justeras i Fotoläge. Synkronisering och visning av det egna huvudet kan väljas.
- ”Golvhöjd” flyttar hela kroppen upp till 3m uppåt eller nedåt och kan användas separat från ”Fothöjd”.
- Tillagt andningsrörelser, avstängning av standardsvajning och korrigering av förvrängning från huvudrotation.
- Inspector visar valda funktioner och parameteranvändning. Possökning, massimport, omordning och enkel förhandsvisning i Scene stöds.
- Snabbare växling mellan stående, hukande och sittande i skrivbordsläge.
- Förbättrade förberäkningsvarningar efter ändringar, scrollprestanda i Inspector och långa stopp vid städning av genererade data.
- Åtgärdat onödiga uppdateringsvarningar och omgenerering vid uppladdning med automatisk import aktiverad och oförändrade inställningar. Importresultat tillämpas på den genererade avataren utan att ändra originalobjektets importinställningar.
- Tillagt Vänd dig genom huvudlutning och rörelseinställningar per pose.
- Förbättrat överföring av vändningsinställningar från äldre förinställningar, import av egna rörelser och hantering av genererade assets.

## [3.1.0-beta.2] - Öppen beta (2026-11-05)

- IK:s längdreferens stöder nu fast längd och höjdskillnad (Y) respektive djupskillnad (Z) mellan Head och Viewpoint.
- Inspector-varningar visas även till vänster om objektnamn i Hierarchy, inklusive utebliven förberäkning, ändrade inställningar och aktuella varningar vid rörelseimport eller förinställningsåtgärder.
- Åtgärdat långa Unity-stopp när automatisk städning, exempelvis vid scenöppning, läste in genererade Controllers från andra scener.
- Ändrade poser eller funktioner raderar inte längre förberäkningshistoriken. Ändrade inställningar efter förberäkning visas i stället för att den aldrig körts, och Hierarchy visar uppdateringsvarning.
- Jämförelseresultat delas mellan Inspector och Hierarchy för att minska scrollbelastning när inställningar och refererade assets är oförändrade. Vid bygge utförs jämförelsen fortfarande varje gång.

## [3.1.0-beta.1] - Öppen beta (2026-11-04)

- Normal poseväxling i skrivbordsläge använder nu en utvärdering. Detta gäller vanliga byggen och förberäkning; VR och särskilda villkor använder tidigare väg.
- Förberäkningar från alpha.3 eller äldre identifieras som uppdateringskrävande.
- Synvinkeln bevaras vid ändrad golvhöjd i Fotoläge, och den senaste golvhöjden återställs när Fotoläge avslutas.

- Automatisk import identifierar nu rörelser endast vid generering och lägger inte till eller ändrar manuella importinställningar vid förberäkning/bygge.
- På/av för automatisk import, Base-ändringar och borttagna manuella val påverkar omgenerering. Manuella val och justeringar som delas av flera produkter bevaras, och äldre importerade rörelser återställs till standardposer.
- Genererade förberäknings-Controllers hanteras i en fil per scen-GUID och avatar. Referenser som delas med kopior, andra scener eller Prefab separeras före uppdatering.
- Byggen använder NDMF:s tillfälliga assethantering och skapar inte fler genererade redigeringsfiler.
- Genererade data kontrolleras vid scenöppning och sparning. Osparade ändringar, Undo/Redo och externa referenser bevaras; endast tydligt ägda oanvända filer flyttas till papperskorgen.
- Rotation, justeringsanimation och uppspelningsposition för importerade rörelser tillämpas. Rotation, RootT/RootQ och Humanoid-justering använder samma hantering som vanliga poser.
- Justeringar av nästlade BlendTree kopieras för importmålet så att ursprungsrörelsen och gångens blandningsinställningar bevaras.
- Uppspelningsposition för endast stående utvecklas till en stillbildspose med ursprunglig bildfrekvens utan att stoppa gångens uppspelning.

## [3.1.0-alpha.2] - Slutet test (2026-10-26)

### Rättningar och ändringar

- Fotoläge och golvhöjd korrigerar även standardrörelser och egna rörelser när ingen verktygspose är vald.
- Golvhöjd utökad till 3m uppåt/nedåt: 50% är ursprungshöjd och standardvärde, 0% är 3m ned och 100% är 3m upp.
- Åtgärdat ändring av ursprungligt BlendTree vid överskrivning av endast stående.
- Poser utan IK-regler använder manuell korrigering utan onödiga genereringsvarningar.
- Åtgärdat sammanblandade justeringsutkast för likadana posnamn och missade rörelseändringar i samma asset.
- Åtgärdat att ett ogiltigt startposnummer kunde välja en annan startpose.
- Import av AnimatorOverrideController som inte stöds visar vägledning i stället för ett undantag.
- Uttryckligen avstängt Vänd dig bevaras när äldre förinställningar tillämpas.
- Funktionsikoner är tydligare i det ljusa Editor-temat.

Kontroll av synvinkel och ersatta standardrörelser i VRChat-klienten pågår fortfarande.

## [3.1.0-alpha.1] - Före offentlig utgivning

### Tillagt

- Tillagt på/av per funktion och visning av ExpressionParameters-användning.
- Tillagt Fotoläge för förflyttning framåt och rotation av avataren.
- Tillagt val av Fotoläge-synkronisering: endast lokalt använder 0 bit och synkronisering använder 16 bit.
- Tillagt möjlighet att behålla det egna avatarhuvudet synligt i Fotoläge.
- Tillagt ”Golvhöjd” för hela kroppen inklusive huvudet, 3m uppåt/nedåt. 50% är ursprungshöjd och standardvärde, 0% är 3m ned och 100% är 3m upp.
- Tillagt Vänd dig genom huvudlutning och animationsinställningar per pose.
- Tillagt andningsrörelser med Svag, Medel, Stark, Snabb och Egen.
- Andningsrörelser och på/av-meny i spelet kan väljas separat. Enbart rörelse använder 0 bit; på/av-synkronisering använder 1 bit.
- Tillagt avstängning av VRChats vanliga stillastående svajning.
- Tillagt korrigering av förvrängning från huvudrotation.
- Tillagt automatisk lemanpassning (IK) efter kroppsbyggnad och på/av för hela avataren eller enskilda poser.
- Tillagt ”Baka in IK i justeringar” för att spara IK-resultat i en justeringsanimation.
- Tillagt avatarspecifik posekorrigering, skydd mot bakåtböjda knän och minsta böjvinkel.
- Tillagt enkel förhandsvisning i Scene från poslistan.
- Tillagt kategoriflikar, sökning över kategorier, massimport av animationer och val av målposition vid omordning.
- Produktens och det gemensamma systemets ändringshistorik kan visas i Inspector.

### Ändrat

- Snabbare inläsning av förinställningar minskar väntetiden vid tillägg av verktyg och visning av listan.
- Tillägg, ändringar och borttagningar av förinställningar återspeglas automatiskt i listan.
- Inspector indelad i tre flikar: ”Funktioner”, ”Poser” och ”Avatarinställningar”.
- Inställningar har beskrivningar och ikoner, och utfällbara detaljer är enhetliga.
- Andningsdetaljer flyttade till funktionsinställningar och förinställningar till slutet av Avatarinställningar.
- Manuellt angivna egna rörelser och automatisk import av avatarens ursprungliga poser är separata inställningar.
- Färre dubblerade helkroppsanimationer för fothöjd.
- Uppdatering av posebilder är en knapp med bekräftelsedialog.
- Varningsvisning samlad under ”Kontrollera inställningar”.
- Nya särskilda 8bit-Prefab har tagits bort till förmån för funktionsval i vanliga Prefab. Befintliga referenser bevaras.
- Lägsta version av VRChat Avatars SDK ändrad till 3.10.4.

### Rättat

- Fotoläge och golvhöjd korrigerar även vanliga och egna rörelser när ingen verktygspose är vald.
- Åtgärdat att ursprungligt BlendTree ändrades när endast stående skrevs över.
- Poser utan giltiga IK-regler använder manuell justering utan onödiga IK-varningar.
- Åtgärdat sammanblandning av justeringar för poser med samma namn och utebliven omgenerering när en rörelse byttes inom samma asset.
- Ett inaktiverat startposnummer väljer inte längre en annan pose som återanvänder numret.
- Import av ursprungliga poser visar vägledning i stället för undantag vid AnimatorOverrideController som inte stöds.
- Uttryckligen avstängda vändningsinställningar bevaras från äldre förinställningar.
- Förbättrad bakgrund för Inspector-ikoner så att de kan urskiljas även i det ljusa Editor-temat.

- Åtgärdat saknade ikoner för Vänd dig, Golvhöjd och Fotoläge när verktyget läggs till i avataren.
- Åtgärdat förlorade vändningsinställningar för stödda poser från äldre förinställningar.
- Åtgärdat att redigering av posnamn kunde flytta inmatningen till en annan pose när dess detaljer öppnades.
- Åtgärdat stängda detaljer vid omordning och Undo/Redo för inställningsändringar.
- Åtgärdat överlappande originalavatar och dubbelräknad position i förhandsvisningen.
- Åtgärdat förhandsvisningsfel för avatarer med saknade komponenter.
- Åtgärdat felriktade eller förskjutna IK-lemmar vid skillnader i benriktning och skala.
- Åtgärdat utebliven direkt uppdatering av varningar och förberäkningsstatus efter ändringar.
- Åtgärdat återanvändning av gammal förberäkning efter ändrade IK-inställningar.
- Åtgärdat byggfel när tilläggsfunktioner inte kunde genereras i vissa konfigurationer.

## [3.0.16] - 2026-09-14
- Lägsta NDMF-version uppdaterad till 1.14.0 eftersom API:t `VirtualAnimatorController.SetParameter` används.

## [3.0.15] - 2026-09-13
- Åtgärdat kraftiga ändringar av kroppsposition och knäböjning vid växling av nacklås i ryggläge på avatarer med både ändrad Hips-skala och FloorAdjuster.

## [3.0.14] - 2026-09-09
- Åtgärdat att ”Gester av” försvann från det andra och senare poseverktyget på samma avatar.
- Åtgärdat att USSPS_Locomotion ibland återgick till Standing trots en vald pose som SleepDown.
- Åtgärdat fel posehöjd och X/Z-position med FloorAdjuster, med stöd för MA- och AAO-versionernas körordning.
- Automatisk återställning till vanliga PosingSystem_Locomotion om en förberäknad AnimatorController i Generated raderas.
- Åtgärdat byggfel när alla stående poser tagits bort.
- Förinställningslistan sorteras efter avatarnamn.

## [3.0.13] - 2026-09-04
- Åtgärdat instabil återgång till stående i VR när man reste sig direkt efter att ha börjat huka, vilket orsakade en självövergång till stående.

## [3.0.12] - 2026-09-04
- Åtgärdat felaktig klassificering som ändrade inställningar vid NDMF-bygge av ett verktyg utan förberäkning.
- Åtgärdat `AddAssetToSameFile`-fel när genererade assets lades till i en tillfällig AnimatorController under NDMF-bygge.

## [3.0.11] - 2026-08-31
- Åtgärdat misslyckad bearbetning när förberäkningsavatarens Scene blev ogiltig med NDMF 1.14.7 eller senare.
- Åtgärdat `MissingReferenceException` när NDMF:s felvy refererade till en förstörd LocalizationAsset.

## [3.0.10] - 2026-08-29
- Faktiska förinställningsassets läses först när urvalsvyn öppnas, och cache återanvänds om GUID-förteckningen inte ändrats.
- Hierarchys varnings-/felikoner omvärderas endast vid ändringar, och produktmenyernas assetsökningar använder gemensam cache. Förbättrar Editor-frysningar med några sekunders mellanrum i scener med många poseverktyg.
- Åtgärdat `ArgumentException` vid registrering av dubbla parametervärden under förberäkning av flera poseverktyg på samma avatar.

## [3.0.9] - 2026-08-26
- Förhindrar NullReferenceException när VRCAvatarDescriptor inte kan hämtas från PosingSystem under konvertering och hänvisar till byggloggar som behövs för felsökning.
- Åtgärdat `Avatar is null` när senare förberäkning eller bygge återanvände en förhandsvisning av en tillfällig Avatar som NDMF skapat och raderat. Konvertering separerad från förhandsvisningscache, och namnkonflikter mellan tillfälliga NDMF-assets i olika förhandsvisningar förhindras.
- Åtgärdat att förhandsvisningsreferenser för Prefab 「可愛いポーズ」 inte sparades i scenen och att dolda förhandsvisningsavatarer fortsatte samlas. Förhandsvisningar undantas från scensparning och gamla städas vid sceninläsning, övergång till Play och skriptomladdning.

## [3.0.8] - 2026-08-18
- Åtgärdat Unity Assertion och risk för ogiltiga posedata i förberäknade rörelser när höjden korrigeras igen av exempelvis FloorAdjuster.
- Säkrare höjdmätning som inte stör originalavataren eller andra verktygs AnimationMode-tillstånd.

## [3.0.7] - 2026-08-12
- Åtgärdat förskjuten synvinkel och onormal knäböjning i ryggläge tillsammans med höjdverktyg som FloorAdjuster.
- Åtgärdat fel när PosingSystem ligger utanför avataren (tack PancakeTorto1se).
- Åtgärdat att fortsatt bearbetning efter misslyckad avataruppladdning orsakade fel.
- Åtgärdat felaktig upptäckt av ändrade inställningar direkt efter förberäkning.
- Förbättrad automatisk import av förflyttningsrörelser.
- Varningar för vissa avatarer med särskilda riggar.
- Tillagt ändringsbekräftelse och spegling av justeringsvärden i animationsjusteringsfönstret (tack kraihd).
- Tillagt vägledning vid fel i andra verktyg och kontroller av felkänsliga delar.

## [3.0.6] - 2026-03-17
- Åtgärdat kraftigt förvrängda poser på vissa avatarer med egen ursprunglig posstyrning.

## [3.0.5] - 2026-01-31
- Åtgärdat Unity-krascher med vissa ExpressionMenus.
- Åtgärdat avatarproblem med integrations-Prefab för ごろ寝EX (tack あずき).
- Åtgärdat felaktig rotation vid sammanslagning av justeringsanimationer.

## [3.0.4] - 2026-01-11
- Vissa förbättringar för avatarer i Prefabs.
- Tillagt borttagning av licens.
- Åtgärdat många poser med felaktig nacke.
- Något snabbare öppning av Inspector.
- Ikoner skapas även vid förberäkning, och texten på avatarens uppdateringsknapp ändrad.
- Varningar döljs om förberäkning aldrig körts, eftersom många verkar avstå från den.
- Onödiga resurser borttagna.
- Åtgärdat att fingrar inte kunde justeras.

## [3.0.3] - 2026-01-04
- Åtgärdat att en annan förinställning än den valda tillämpades (tack あずき).

## [3.0.2] - 2026-01-02
- Stöd för Unitys mörka tema.
- Åtgärdat förvrängda poser vid rotation i animationsjustering.
- Ändrad storlek och aktiverad komprimering för genererade menyikoner, samt inställning för ännu mindre ikoner.
- Återinfört spårningsintegration och automatisk import av avatarrörelser, som togs bort i 3.0.0.

## [3.0.1] - 2025-12-29
- Åtgärdat fel i inställning av standardrörelser.
- Åtgärdat att avatarposer förvrängdes av sig själva.

## [3.0.0] - 2025-12-28
- Tillagt förberäkning.
- Tillagt animationsjustering.
- Tillagt förinställningar.

## [2.2.5] - 2025-05-08
- Löst kompatibilitetsproblem med vissa verktyg.

## [2.2.4] - 2025-04-04
- Åtgärdat utebliven poseväxling med vissa avatarer och ModularAvatar-versioner.

## [2.2.3] - 2025-03-13
- Åtgärdat felaktiga rörelser på vissa avatarer, exempelvis hopp med 森羅.
- Åtgärdat ibland felaktigt standardvärde för Gester av.

## [2.2.2] - 2025-03-06
- Åtgärdat fel i vissa miljöer.

## [2.2.1] - 2025-02-05
- Åtgärdat Unity-krascher.
- Version tillagd i verktygsnamnet i NDMF Console.

## [2.2.0] - 2025-02-02
- Tillagt val för att stänga av gester och använda animationens handform.

## [2.1.6] - 2025-02-02
- Åtgärdat förvrängda poser tillsammans med vissa verktyg.
- Åtgärdat inställningsberoende fel.

## [2.1.5] - 2024-12-10
- Löst konflikter med vissa verktyg.
- Tillagt knapp för borttagning av Unitypackage-versionen.

## [2.1.4] - 2024-12-08
- Åtgärdat fel på vissa avatarer.
- Åtgärdat oanvändbart fotlås vid konflikter med verktyg eller avatarer.
- Åtgärdat att avatarens pose återgick till VRC-standard i skrivbordsläge.

## [2.1.3] - 2024-12-01
- Åtgärdat utebliven poseväxling tillsammans med avatarer eller verktyg som använder fel typ för VRMode.
- Tillagt visning i NDMF-Console när olika parametertyper orsakar verktygskonflikter.

## [2.1.2] - 2024-12-01
- Tillagt förklaring i NDMF-Console när andra verktygs fel gör att Unisaki Posing System misslyckas vid avatarbygge.
- Åtgärdat byggfel på vissa avatarer.
- Förbättrat beteende direkt efter avatarinläsning.
- Avatarens ursprungliga förflyttningshantering tas bort eftersom den kan stoppa funktionen på vissa avatarer.

## [2.1.1] - 2024-11-20
- Åtgärdat MergeAnimator-relaterade fel i PosingOverride.

## [2.1.0] - 2024-11-20
- Löst att fötter och höfter hamnade under marken i skrivbordsläge så att alla poser kan visas korrekt.
- Tillagt poseväxling oberoende av huvudhöjd.
- Tillagt läge utan ikongenerering för uppladdning av Quest-avatarer.
- Tillagt flera varningar vid avatarproblem.
- Löst konflikter mellan låsfunktioner och AFK eller andra verktygs armlås.
- Tillagt automatiskt nacklås i ryggläge.

## [2.0.0] - 2024-11-12
- Utgiven version med VPM/VCC-stöd.
- Ändrat verktygets visningsnamn vid fel #1.
- Tillagt användarvillkor #5.
