# Unisaki Posing System

Gemensamt posesystem för Kawaii Posing, Cool Posing och andra verktyg. Modular Avatar och NDMF skapar menyer och animationer vid avataruppladdning.

Denna README gäller v3.2.0 under utveckling. Officiellt releasedatum är inte bestämt. Versionen visas överst i Inspector. Se även [tillägg och ändringar](CHANGELOG.md).

## Visningsspråk och offlinedokumentation

Välj språk under Language / 言語 överst i Inspector. Samma val styr Inspector och genererade VRChat-menyer. Ladda upp avataren igen för att ändra språk i spelet.

Utan uttryckligt val används datorns språk, annars engelska om språket saknas. Windows, macOS och Linux stöds. Senaste val sparas i EditorPrefs för andra projekt med samma systemanvändare på denna dator. Ett uttryckligt val i avataren behålls. Ingen synkronisering till andra datorer. Licenslagring ändras inte.

Välj språk för README, historik och handbok i medföljande [språklista](../index.html). Inspectors handbok och ändringar följer också valet. Originalvillkor och referensöversättningar finns fortsatt separat.

## Snabbare ställningsbyte på desktop

Fördröjningen innan vald pose används vid byte mellan stående, hukande och sittande har minskat. Gäller normala byggen och förberäkning utan särskilda kommandon eller inställningar. VR och specialfall behåller tidigare beteende. Uppdatera gamla förberäkningar före uppladdning.

## Installation och uppdatering

Lägg till poseverktyget med VCC eller ALCOM enligt köpta filer och instruktioner. Systemet installeras som ett beroende.

Krav för v3.2.0:

- VRChat-avatarprojekt i Unity 2022.3
- VRChat Avatars SDK 3.10.4 eller senare
- Modular Avatar 1.x, version 1.15.1 eller senare
- NDMF (Non-Destructive Modular Framework) 1.x, version 1.14.0 eller senare

Uppdatera Kawaii Posing till 3.2.0 i ALCOM / VCC och kontrollera att även Unisaki Posing System är 3.2.0. Använder du förberäkning, klicka Uppdatera förberäkning, spara scenen och ladda upp.

Cool Posing 1.2.0 använder gemensamma systemet 3.2.0. Uppdatera båda till motsvarande versioner.

### För tidigare användare av 8bit-versionen

Nya tillägg använder vanlig Prefab. Stäng av onödiga funktioner för mindre förbrukning. Enbart poseval kräver normalt 8 bit.

Gamla 8bit-Prefabs finns i Editor/Legacy för att bevara avatarers och förinställningars referenser. Befintliga behöver inte tas bort eller ersättas. Tilläggsmenyn erbjuder inte längre 8bit-versionen.

## Inställningarnas uppbyggnad

Välj 「可愛いポーズ」, 「カッコいいポーズ」 eller motsvarande objekt i avataren i Hierarchy.

| Flik | Innehåll |
| --- | --- |
| Funktioner | Spelmenyer, praktiska funktioner, ikoner och ExpressionParameters-förbrukning |
| Poser | Val, tillägg, ordning, Scene-förhandsvisning, detaljer och animationsjustering |
| Avatarinställningar | Kropps-/knäkorrigeringar, ursprungliga/egna rörelser och förinställningar |

Öppna inställningslänken för att visa detaljer under funktionen. Andning finns också under Funktioner.

Hjälp / Ändringar överst öppnar handbok, support och historik. Växla mellan produktens och det gemensamma systemets ändringar.

## Funktioner och ExpressionParameters-förbrukning

Val under Funktioner > Menyfunktioner visas i spelet. Spegla gör effekten omkopplingsbar i VRChat; det tvingar inte alla poser att alltid speglas.

| Funktion | Extra bitar |
| --- | ---: |
| Spegla | 1 bit |
| Gester av | 1 bit |
| Animera andning | 0 bit |
| Andning på/av i spelet | 1 bit |
| Fothöjd | 8 bit |
| Golvhöjd | 8 bit |
| Fotoläge endast lokalt | 0 bit |
| Synkronisera Fotoläge | 16 bit |
| Håll huvudet synligt i Fotoläge | 0 bit |
| Stäng av VRChats stillastående svajning | 0 bit |
| Minska förvrängning från huvudrotation | 1 bit (delas med Vänd dig) |
| Vänd dig | 10 bit (1 bit delas med förvrängningskorrigering) |

Vänd dig använder 1 bit för aktivering, 8 bit för huvudlutning och 1 bit för nacklås. Med korrigeringen aktiv kostar tillägget därför 9 bit. Delad bit räknas bara en gång; visningen följer alternativen.

Summan gäller avatarens poseverktyg, inte hela avataren med kläder och andra system. Gemensamma inställningar räknas inte dubbelt. Många poser över grundintervallet kan kräva fler valbitar.

### Fothöjd och Golvhöjd

- Fothöjd justerar fötternas sida med huvudet kvar. Använd för golv eller säng.
- Golvhöjd flyttar hela kroppen inklusive huvudet. 50% är ursprungligt, 0% 3m lägre och 100% 3m högre. Startvärde 50%.

Båda finns i Inställningar, Golvhöjd efter Fothöjd. Utanför mitten flyttas kroppen upp/ned; nära 50% återställs höjden och korrigeringen slutar. Ingen separat svävningsknapp. Blicken korrigeras när svävningen börjar och vid höjdändringar.

För normala animationer dupliceras inte längre hela kroppsrörelsen för fothöjden. Det minskar animationsdata, men betyder inte att hela avatarens storlek alltid blir en tredjedel.

### Fotoläge

1. Aktivera Funktioner > Menyfunktioner > Fotoläge.
2. I VRChat öppna Fotoläge och Välj pose.
3. Avstånd flyttar upp till 3m framåt och Riktning roterar. Över 0 vänder avataren mot dig.
4. Lås fötter vid behov. Det tvingas inte från början, så du kan placera först. Med Gester av i menyn kan även händer ändras här.
5. Avsluta foto återställer avstånd/riktning och låser upp fötterna.

Fotolägets rörelse/rotation kostar 0 bit endast lokalt. Aktivera Praktiska funktioner > Synkronisera Fotoläge för att andra ska se dem, totalt 16 bit.

Försvinner ditt närliggande avatarhuvud, aktivera Praktiska funktioner > Håll huvudet synligt i Fotoläge. Bara din fotovy påverkas och återställs sedan.

### Växla liggande poser genom att hoppa

Väljs både rygg och mage kan du hoppa på plats för att växla. Med bara en riktning behålls den. Stäng av Växla liggande poser genom att hoppa under Funktioner → Praktiska funktioner i Unity om du vill. Extra synkroniserad kostnad är 0 bit. Avstängning i ett av flera verktyg gäller hela avataren.

### Vänd dig

I liggande poser som stöds vänder sig kroppen mjukt när du lutar huvudet åt vänster eller höger. Funktionen stöder inte automatiskt alla liggande poser.

1. Aktivera ”Funktioner > Menyfunktioner > Vänd dig”.
2. Öppna ”Detaljer” för den liggande posen under ”Poser” och aktivera ”Aktivera Vänd dig”.
3. Ange en animation för sidoläge i ”Sidopose för vändning”. Vändningar åt båda håll skapas från ursprungsposen och sidoposen. Justera ”Markkorrigeringens styrka” vid behov.
4. Välj en pose som stöds i VRChat och aktivera ”Vänd dig”. Lås upp nacken innan du använder funktionen.

För poser med Vänd dig bestämmer huvudets lutning animationens uppspelningsposition, så den vanliga MotionTime-inställningen kan inte användas. För en färdig vändningsanimation anger du den i det vanliga animationsfältet och lämnar ”Sidopose för vändning” tomt. Förbered en animation där uppspelningsposition 0 är vänster, 0.5 är ryggläge och 1 är höger.

### Andning och normalt svaj

”Funktioner > Praktiska funktioner > Animera andning” får bland annat bröstkorgen att röra sig med andningen. Via ”Öppna inställningar” kan du välja ”Ingen”, ”Svag”, ”Medel”, ”Stark”, ”Snabb” eller ”Egen” för varje posekategori. ”Snabb” ger snabbare andning. Med ”Egen” kan du ange en egen andningsanimation.

Enbart andningsrörelsen använder 0 bit. Aktivera även ”Menyfunktioner > Andning” om du vill kunna pausa och återuppta andningen i VRChat. Synkronisering av på/av använder 1 bit. Styrkan påverkar inte antalet bitar.

Nacke, axlar och armar rör sig tillsammans med bröstkorgen för att minska huvudets och händernas förflyttning. Positionerna är inte helt låsta, så välj styrka efter kroppsform och pose.

”Praktiska funktioner > Stäng av VRChats stillastående svajning” minskar den kroppssvajning som VRChat lägger till vid stillastående. Inställningen är av som standard och behåller poseverktygets egen andning och andra anpassade rörelser. Den kan användas utan andningsrörelsen och kostar inga extra bitar. Den stoppar inte svajning som orsakas av något annat än den vanliga stillastående animationen.

Om flera poseverktyg används samtidigt räcker det att stoppa standardsvajningen i ett av dem för att påverka hela avataren. För att återställa den stänger du av inställningen i alla verktyg och laddar upp avataren igen.

### Minska förvrängning från huvudrotation

Använd detta om posen förvrängs när nacken låses upp eftersom huvudets riktning skiljer sig från ursprungsposens. Inställningen finns under ”Funktioner > Praktiska funktioner” och lägger inte till någon egen meny i spelet. Med låst nacke används ursprungsposens riktning. Ryggliggande poser undantas från korrigeringen för att bevara den ursprungliga uppåtriktningen.

### Menyikoner

- ”Generera poseikoner”: skapar bilder av avataren i varje pose och visar dem i menyn. Det blir lättare att skilja poser åt, men bildgenereringen förlänger byggtiden.
- ”Minska ikonstorlek”: sänker bildernas upplösning för att minska avatarens storlek.

Poseikoner genereras inte för Android. Använd ”Ta om ikoner” efter ändringar av exempelvis kläder. Bilderna i poslistan uppdateras med ”Uppdatera poseminiatyrer”. Läs bekräftelsedialogen innan du kör dessa tidskrävande åtgärder.

## Lägga till, ordna om och förhandsvisa poser

Välj posekategori till vänster under ”Poser”. Fliken med förstoringsglaset högst upp söker efter posnamn i alla kategorier. Under ”Detaljer” kan du ställa in animation, ikon, MotionTime och mer på ett ställe.

Lägg till flera animationer samtidigt genom att dra dem till tilläggsfältet, eller markera dem i Project och trycka på ”Importera klipp valda i Project”. Dubbletter av samma klipp tas bort.

Ändra ordning genom att välja en målposition under ”Detaljer > Ordning”. Du kan också använda uppåt- och nedåtknapparna. Detaljernas öppna eller stängda läge bevaras när posen flyttas.

Tryck på ”Förhandsvisning” för en pose för att se den i Scene utan att öppna justeringsfönstret. När förhandsvisningen avslutas återställs ursprungsvyn. Denna enkla förhandsvisning kör inte MA:s eller NDMF:s byggprocess, så slutresultatet för kläder och exempelvis FloorAdjuster som läggs till vid bygge kan skilja sig. Kontrollera även slutlig klädsel och markkontakt efter avatarbygget.

## Animationsjustering och automatisk justering av lemmar (IK)

Öppna ”Animationsjustering” under ”Poser” för att finjustera kropp och lemmar med förhandsvisning. Spara justeringsanimationen med fönstrets sparfunktion när du är klar.

I produkter och poser som stöder automatisk justering anpassas lemmarnas positioner till avatarens kroppsbyggnad. ”Aktivera automatisk justering av lemmar” i animationsjusteringen styr hela funktionen, medan ”Aktivera IK för den här posen” styr en enskild pose. För poser utan automatisk justering visas information om detta.

Tryck på ”Baka in IK i justeringar” om du vill utgå från den automatiska justeringen och slutföra arbetet manuellt. Det aktuella IK-resultatet skrivs till en ny justeringsanimation och IK stängs av för den aktuella posen på avataren. Ursprungsanimationen bevaras. Knappen är tillgänglig när automatisk justering är aktiverad både övergripande och för posen och tillämpliga IK-inställningar finns.

”Förhindra bakåtböjda knän” under ”Avatarinställningar” styr knäkorrigeringen. Vinkeln anges under ”Minsta knäböjning (grader)” i detaljerna. ”Aktivera avatarspecifik korrigering” kan användas när korrigeringsdata som stöds finns. Korrigeringar som överlappar automatisk justering av lemmar kan vara oförenliga.

## Ursprungliga poser, egna rörelser och förinställningar

Dessa ställs in under ”Avatarinställningar”.

- ”Använd ursprungliga stående, hukande och magposer”: importerar automatiskt avatarens ursprungliga poser.
- ”Använd egna rörelser”: öppna importinställningarna och ange rörelser separat för varje typ, exempelvis stående, hukande (Crouching) och magläge (Prone). Normalt anges en rörelse per typ.
- ”Integrera spårningslås med andra verktyg”: aktivera om låsfunktionerna inte fungerar väl tillsammans med andra verktyg.

Manuell rörelsetilldelning i importinställningarna är en separat funktion från automatisk import av ursprungliga poser.

Förinställningarna finns längst ned på fliken. Sök efter namn och byt poseinställningar med ”Tillämpa den här förinställningen”. De används främst vid inledande installation och ersätter de aktuella inställningarna. Spara därför eventuella egna justeringar innan du byter.

När förinställningslistan visas läses endast namn och information om stödda avatarer in. Justeringsanimationer läses in först när den valda förinställningen tillämpas. Inte heller första gången listan skapas förinläses animationer för alla avatarer.

Listan är uppdelad i ”Passar avataren”, ”Användarförinställningar” och ”Officiella Unisaki Studio-förinställningar”. ”Dölj medföljande förinställningar” är en gemensam visningsinställning inom projektet. Listan uppdateras när förinställningar läggs till, ändras eller tas bort. Tryck på ”Läs in igen” om visningen inte ändras.

## Förberäkning och varningar

När ”Använd ursprungliga stående, hukande och magposer” är aktiverat identifieras rörelser från avatarens Base vid varje generering. Resultatet skrivs inte tillbaka till de manuella importinställningarna, så listan förblir otilldelad om du bara använder automatisk import. Ändringar av Base-rörelser eller av på/av-läget för automatisk import tillämpas vid nästa generering.

Automatisk import vid uppladdning tillämpas på byggkopian. Det ursprungliga scenobjektet, importinställningarna och ursprungliga Controllers och rörelser ändras inte. Om förberäkning redan har gjorts och indata är oförändrade återanvänds resultatet.

Typer med manuella tilldelningar prioriterar dessa inställningar, inklusive rotation, justeringsanimation och uppspelningsposition. Om ”Använd” stängs av eller en tilldelad rörelse tas bort stoppas automatisk import för den typen. Med ”Rensa manuellt val” tas typens inställningar och justeringar bort. Om automatisk import är aktiverad identifieras rörelsen igen vid nästa generering. Åtgärden kan ångras med Undo. Om flera äldre inställningar finns för samma typ prioriteras den giltiga inställning som valts i Inspector.

Inställningar som äldre automatisk import har lagt till i listan tas inte bort automatiskt, eftersom de inte går att skilja från manuellt registrerade eller justerade inställningar. Använd ”Rensa manuellt val” endast för de typer du vill återgå till automatisk import för. Om flera produkter finns på samma avatar påverkar manuella inställningar i de andra produkterna också de gemensamma ursprungsposerna.

Förberäkning återanvänder genererade data för att förkorta avatarens byggtid. Skapa eller uppdatera dem med ”Uppdatera förberäkning”. Nödvändiga data genereras även vid ett vanligt avatarbygge, så du behöver inte förberäkna manuellt varje gång.

Ändrade inställningar markerar att en uppdatering behövs. Detta visas under ”Kontrollera inställningar” högst upp i Inspector och med markeringar i Hierarchy. Uppdatera förberäkningen efter ändringar av poser eller IK-inställningar för att minska kommande byggtider.

”Inställningar för varningsvisning” finns i rutan ”Kontrollera inställningar”. Du kan dölja olika varningstyper eller markeringarna i Hierarchy. Att dölja dem löser inte inställningsproblem eller uppdaterar genererade data. Fel och nödvändig generering kvarstår.

## Att tänka på vid användning

- Verktyget är avsett för 3-punktsspårning och skrivbordsläge. Poslåsning med helkroppsspårning stöds inte.
- Beroende på kroppsform och anpassningar kan lemmarnas markkontakt och klädernas utseende behöva justeras ytterligare.
- Särskilda Base-lager eller andra verktyg som ändrar poser kan ge oväntade resultat.
- Förflyttning och rotation i Fotoläge samt golvhöjd korrigerar verktygsposer och vanliga eller egna rörelser när ingen verktygspose är vald. Kontrollera standardrörelser som ersätts i VRChat och synvinkeln i den faktiska klienten.

## Användarvillkor och support

Källkoden och resurserna i detta repository är inte öppen källkod. Användning kräver ett giltigt köp av produkten och godkännande av [de medföljande användarvillkoren](../../../%E3%82%86%E3%81%AB%E3%81%95%E3%81%8D%E3%83%9D%E3%83%BC%E3%82%BA%E3%82%B7%E3%82%B9%E3%83%86%E3%83%A0%E5%88%A9%E7%94%A8%E8%A6%8F%E7%B4%84%EF%BC%88%E6%97%A5%E6%9C%AC%E8%AA%9E%E7%89%88%EF%BC%89.pdf).

- [Handbok för Kawaii Posing](https://docs.unisakistudio.jp/kawaiiposing/manual/)
- [Handbok för Cool Posing](https://docs.unisakistudio.jp/coolposing/manual/)
- [Kontakta oss](https://help.unisakistudio.jp/)

Vid felrapportering: ange produktens och Unisaki Posing Systems versioner, versionerna av Unity, VRChat SDK, Modular Avatar och NDMF, använd avatar, steg för att återskapa felet och felmeddelandena från Console.
