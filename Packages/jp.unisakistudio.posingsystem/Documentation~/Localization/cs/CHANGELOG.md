# Historie změn

## [3.2.0] - Nevydáno

### Přidáno

- Zobrazení Inspectoru, úprav, instalačních dialogů, standardních názvů póz a generované nabídky VRChat ve 30 jazycích.
- Sjednocen výběr jazyka Inspectoru a herní nabídky. Výchozí je jazyk počítače a poslední volba platí i pro další projekty stejného uživatele OS.
- README, historie změn a offline návod se otevírají ve vybraném jazyce.
- Připraveny názvy všech 128 standardních póz a popisy funkcí podle stávající terminologie.

### Kompatibilita

- Zachovávají se existující nastavení avatara, vlastní názvy, odkazy na prostředky, identifikátory parametrů a způsob ukládání licencí.

## [3.1.2] - 2026-11-09

- Opraveno přetrvávající natočení obličeje vzhůru po zvednutí z lehu na břiše při zapnutém omezení deformace pózy podle otočení hlavy.
- Zamykání hlavy lze přepínat v „Nastavení → Automaticky zamknout hlavu na zádech“ v nabídce EX i při vypnuté výšce chodidel. Položka zmizelá ve 3.1.0 nebo 3.1.1 se vrátí po sestavení aktualizovaného avatara.
- Aktualizovat předgenerování funguje i při odškrtnutém hlavním objektu avatara v Unity.
- Opravena nemožnost generovat pózy po aktualizaci, pokud v Assets zůstal starý společný Controller póz. Zachovávají se i vlastní úpravy Controlleru.
- Pokud vypnutý nástroj nebo jeho rodič znemožní použití společného Animatoru póz, zobrazí se název cíle. Otevřete jej přes „Kontrola nastavení → Vybrat cíl“.
- Při oznámení změny nastavení předgenerování lze rozdíly zjistit v „Podrobnosti → Zobrazit změny od předgenerování“.
- Když sestavení zastaví chyba skriptu (PS050), zobrazí se až 5 původních chyb Unity. Zeptat se na tuto chybu otevře koncept s verzemi Unity a podporovaných nástrojů a číslem chyby, který lze zkontrolovat, upravit a odeslat.

Pokud jste používali předgenerování, po aktualizaci stiskněte Aktualizovat předgenerování, uložte scénu a avatara znovu sestavte a nahrajte.

## [3.1.1] - 2026-11-08

- Zapnuté Výška podlahy a Přetočit se jsou nyní v Nastavení nabídky EX ve VRChat viditelné i při vypnutém zrcadlení.
- Po seřazení, odstranění či přejmenování póz ve staré verzi se zachovávají správná přetočení pro Nohy vpřed, Uvolnění a Jedno koleno vzhůru. Platí to i po úplném Unpack, pokud lze původní pózu rozpoznat. Vlastní pózy, ručně vypnutá přetočení a nahrazené boční pohyby zůstávají.
- Opravena krátká póza na břiše při výběru avatara ve VRChat. Opraven i krátký leh na zádech při skoku, pokud je vybrán pouze leh na břiše. Přepínání skokem lze vypnout v „Funkce → Usnadnění → Měnit ležící pózy skokem“. Ve výchozím stavu je zapnuté.
- Pokud se po aktualizaci ztratí odkaz společného Animator Controlleru póz a lze jej obnovit, zobrazí se varování a Controller se vytvoří z nastavení. Zachovávají se i odkazy Controlleru sdíleného více avatary.
- Chyby kompilace zastaví sestavení hned na začátku s vysvětlením příčiny. Dokud chyba skriptu brání kontrole odkazů, automatický úklid vytvořených Controllerů se odloží.
- Při přidání nástroje se vybírá oficiální Prefab, i když projekt obsahuje jiný stejného názvu. Předgenerování pokračuje i tehdy, když se společný Controller liší jen konci řádků.
- Write Defaults vrstev FX přidávaných nástrojem, například výšky podlahy, se přizpůsobuje nastavení avatara.

Pokud jste používali předgenerování, po aktualizaci stiskněte Aktualizovat předgenerování na nástroji ve scéně, uložte scénu a avatara znovu sestavte a nahrajte.

## [3.1.0] - 2026-11-07

- Přidána automatická korekce končetin (IK) podporovaných póz podle proporcí, korekce avatara a zabránění ohybu kolen dozadu.
- Režim focení umožňuje upravit polohu a směr avatara. Lze zvolit synchronizaci polohy a směru i zobrazení vlastní hlavy při focení.
- Výška podlahy posouvá celé tělo až 3m nahoru či dolů a doplňuje výšku chodidel.
- Přidáno dýchání, vypnutí výchozího pohupování v klidu a omezení deformace pózy podle směru hlavy.
- Používané funkce a spotřeba parametrů jsou viditelné v Inspectoru. Přidáno hledání póz, hromadné přidání, řazení a lehký náhled v Scene.
- Zrychleno přepínání stoje, podřepu a sedu v režimu plochy.
- Zlepšena varování předgenerování po změně nastavení, zátěž posouvání Inspectoru a dlouhá zastavení při úklidu prostředků.
- Opraveno varování na nezměněné předgenerování a opakované vytváření při nahrávání se zapnutým automatickým importem. Výsledek importu se používá na vytvářeného avatara, nemění nastavení importu původního objektu.
- Přidáno přetočení podle náklonu hlavy a nastavení odpovídajících pohybů jednotlivých póz.
- Zlepšeno převzetí přetočení starých předvoleb, import vlastních pohybů a správa vytvořených prostředků.

## [3.1.0-beta.2] - Otevřená beta (2026-11-05)

- Do referencí délky polohy IK přidána pevná délka a výškový rozdíl (Y) i rozdíl vpřed/vzad (Z) mezi Head a Viewpoint.
- Varování Inspectoru se zobrazují i vlevo od názvu objektu v Hierarchy. Kromě chybějícího předgenerování a změn nastavení zahrnují také varování importu pohybů a operací předvoleb.
- Opraveno dlouhé zastavení Unity při automatickém úklidu například po otevření scény, způsobené načítáním vytvořených Controllerů jiných scén.
- Opraveno mizení historie předgenerování po změně seznamu póz či funkcí. Místo neprovedeného předgenerování se zobrazí změna nastavení po něm a Hierarchy upozorní na nutnost aktualizace.
- Inspector a Hierarchy sdílejí výsledky porovnání předgenerování, čímž snižují zátěž posouvání, dokud se nastavení a odkazované prostředky nezmění. Při sestavení se porovnání nadále provádí pokaždé.

## [3.1.0-beta.1] - Otevřená beta (2026-11-04)

- Běžné přepínání póz v režimu plochy převedeno na jedno vyhodnocení. Funguje při běžném sestavení i předgenerování, VR a zvláštní podmínky zachovávají dosavadní cestu.
- Předgenerování z alpha.3 a starších verzí se rozpoznává jako vyžadující aktualizaci.
- Při změně výšky podlahy během focení se zachovává pohled a po ukončení se obnoví poslední výška podlahy.

- Automatický import nyní zjišťuje pohyby pouze při generování. Předgenerování a sestavení nepřidávají ani nemění ruční nastavení importu.
- Přepnutí automatického importu, změny Base a vymazání ručních nastavení se použijí při novém generování. Zachovávají se ruční přiřazení a korekce sdílené produkty, staré importované pohyby se vracejí k výchozím pózám.
- Vytvořené Controllery předgenerování jsou v jednom souboru na avatara s rozdělením podle GUID scény. Sdílené odkazy kopií, jiných scén a Prefabů se před aktualizací oddělují.
- Při sestavení se používá správa dočasných prostředků NDMF a nepřibývají vytvořené soubory pro úpravy.
- Prostředky se kontrolují při otevření a uložení scény. Při neuložených úpravách, Undo/Redo nebo externích odkazech se zachovávají. Do koše se přesunují jen nepoužívané soubory s jasným vlastnictvím.
- Uplatňuje se otočení importovaných pohybů, korekční animace a poloha přehrávání. Zpracování otočení, RootT/RootQ a korekce Humanoid je společné s běžnými pózami.
- Úpravy vnořených BlendTree se kopírují pro konkrétní cíl při zachování původního pohybu a nastavení prolínání chůze.
- Poloha přehrávání pro Pouze stoj se převádí na nehybnou pózu při původní snímkové frekvenci, aniž se zastaví přehrávání chůze.

## [3.1.0-alpha.2] - Uzavřená testovací verze (2026-10-26)

### Opravy a změny

- Korekce režimu focení a výšky podlahy se používají i na výchozí a vlastní pohyby bez vybrané pózy nástroje.
- Rozsah výšky podlahy rozšířen na 3m nahoru i dolů. 50% je původní výška a počáteční hodnota, 0% o 3m níže a 100% o 3m výše.
- Opraveno změnění zdrojového BlendTree při přepsání Pouze stoj.
- U póz bez pravidla IK se používá ruční korekce a potlačují zbytečná varování generování.
- Opraveno směšování konceptů korekcí stejnojmenných póz a přehlížení změn pohybu v jednom prostředku.
- Opraven výběr jiné počáteční pózy podle čísla neplatné pózy.
- Import nepodporovaného AnimatorOverrideController zobrazí pokyny místo výjimky.
- Použití starých předvoleb zachovává výslovně vypnuté přetočení.
- Zlepšena viditelnost ikon funkcí ve světlém motivu Editoru.

Kontrola pohledu a chování po nahrazení výchozích pohybů v klientu VRChat pokračuje.

## [3.1.0-alpha.1] - Před zveřejněním

### Přidáno

- Přidány přepínače jednotlivých funkcí a zobrazení spotřeby ExpressionParameters.
- Přidán režim focení pro posun avatara vpřed a otáčení.
- Přidána volba synchronizace režimu focení: pouze lokálně 0 bit, se synchronizací 16 bit.
- Přidáno ponechání vlastní hlavy viditelné v režimu focení.
- Přidána Výška podlahy, která posouvá celé tělo včetně hlavy o 3m nahoru či dolů. 50% je původní výška a počáteční hodnota, 0% o 3m níže a 100% o 3m výše.
- Přidáno přetočení podle náklonu hlavy a nastavení odpovídajících animací jednotlivých póz.
- Přidán pohyb dýchání s volbami Slabé, Střední, Silné, Rychlé a Vlastní.
- Přidána samostatná volba pohybu dýchání a herního přepínače. Samotný pohyb 0 bit, synchronizace přepínače 1 bit.
- Přidáno vypnutí výchozího pohupování VRChat v klidu.
- Přidáno omezení deformace pózy kvůli rozdílnému směru hlavy.
- Přidána automatická korekce končetin (IK) podporovaných póz podle proporcí s celkovým i samostatným přepínačem.
- Přidáno Zapéct IK do korekcí pro uložení výsledku IK do korekční animace.
- Přidána korekce konkrétního avatara, zabránění ohybu kolen dozadu a minimální úhel ohybu.
- Přidán lehký náhled pózy ze seznamu v Scene.
- Přidány karty kategorií, hledání napříč kategoriemi, hromadné přidání animací a volba cílového pořadí.
- Přidáno zobrazení historie změn produktu a společného systému v Inspectoru.

### Změněno

- Zrychleno načítání předvoleb a zkráceno čekání při přidávání nástroje a zobrazení seznamu.
- Přidání, úpravy a mazání předvoleb se automaticky odrážejí v seznamu.
- Inspector rozdělen na 3 karty: Funkce, Pózy a Nastavení avatara.
- Přidány popisy a ikony nastavení a sjednoceno rozbalování podrobností.
- Podrobnosti dýchání přesunuty do funkcí, předvolby na konec Nastavení avatara.
- Odděleno jednotlivé přiřazování vlastních pohybů a automatický import původních póz avatara.
- Omezeny duplicity animací celého těla vytvářených pro výšku chodidel.
- Obnova obrázků póz změněna na tlačítko s potvrzením.
- Nastavení varování soustředěno do Kontroly nastavení.
- Odstraněno nové přidávání 8bit Prefabu, nahrazeno volbou funkcí běžného Prefabu. Existující odkazy zůstávají.
- Minimální verze VRChat Avatars SDK změněna na 3.10.4.

### Opraveno

- Výchozí a vlastní pohyby bez vybrané pózy nástroje také spadají do korekce režimu focení a výšky podlahy.
- Opraveno změnění zdrojového BlendTree při přepsání Pouze stoj.
- Bez platného pravidla IK se používá ruční korekce a potlačují zbytečná varování generování IK.
- Opraveno směšování úprav stejnojmenných póz a nepromítnutí náhrady pohybu uvnitř stejného prostředku do nového generování.
- Číslo vypnuté počáteční pózy znovu použité jinou pózou ji již nevybere jako počáteční.
- Nalezení nepodporovaného AnimatorOverrideController při importu původních póz zobrazí pokyny místo výjimky.
- Použití staré předvolby zachovává výslovně vypnuté přetočení.
- Zlepšeno pozadí ikon Inspectoru, aby byly funkce rozpoznatelné i ve světlém motivu Editoru.

- Opraveny chybějící ikony přetočení, výšky podlahy a režimu focení při přidání k avatarovi.
- Opravena ztráta nastavení přetočení podporovaných póz při použití předvolby staré verze.
- Opraven přesun zadávaného názvu na jinou pózu po otevření jiných podrobností při úpravě.
- Opraveno zavírání podrobností při řazení i Undo/Redo změn nastavení.
- Opraven překryv původního avatara v náhledu a dvojí přičítání polohy.
- Opravena chyba náhledu avatarů s chybějícími komponentami.
- Opraveny odchylky směru a polohy končetin IK způsobené rozdílnou orientací a měřítkem kostí.
- Opraveno neaktualizování varování a stavu předgenerování hned po změně nastavení.
- Opraveno znovupoužití starého předgenerování po změně IK.
- Opraveny chyby sestavení bránící generování doplňkových funkcí v některých konfiguracích.

## [3.0.16] - 2026-09-14
- Minimální závislost NDMF zvýšena na 1.14.0 kvůli použití API `VirtualAnimatorController.SetParameter`

## [3.0.15] - 2026-09-13
- Opraveny velké změny polohy těla a ohybu kolen při přepnutí zámku hlavy na zádech u avatarů se změnou měřítka Hips a FloorAdjuster

## [3.0.14] - 2026-09-09
- Opraveno mizení položky Gesta vyp. druhého a dalších nástrojů pro pózy na stejném avatarovi
- Opraven návrat vrstvy USSPS_Locomotion do Standing navzdory vybrané póze jako SleepDown
- Opraveny odchylky výšky a X/Z při použití FloorAdjuster a podpořeno pořadí zpracování verzí MA i AAO
- Po smazání předgenerovaného AnimatorController z Generated se automaticky obnoví standardní PosingSystem_Locomotion
- Opravena chyba sestavení po odstranění všech póz ve stoje
- Seznam předvoleb se řadí podle názvu avatara

## [3.0.13] - 2026-09-04
- Opraven přechod pózy ve stoje do sebe sama při návratu do stoje hned po zahájení přikrčení ve VR, který způsoboval nestabilní obnovu pózy

## [3.0.12] - 2026-09-04
- Opraveno chybné označení nástroje bez předgenerování jako změněného při sestavení NDMF
- Opravena chyba `AddAssetToSameFile` při přidávání vytvořených prostředků do dočasného AnimatorController během sestavení NDMF

## [3.0.11] - 2026-08-31
- Opravena neplatná Scene avatara pro předgenerování a selhání zpracování v NDMF 1.14.7 a novějším
- Opraveno `MissingReferenceException` při odkazu chybové obrazovky NDMF na zničený LocalizationAsset

## [3.0.10] - 2026-08-29
- Skutečné prostředky předvoleb se načítají až při otevření výběru předvolby; pokud se katalog GUID nezměnil, znovu se použije mezipaměť
- Varovné a chybové ikony Hierarchy se vyhodnocují jen při změně a hledání prostředků nabídky produktů používá společnou mezipaměť. Omezeno zamrzání Editoru každých několik sekund ve scénách s mnoha nástroji pro pózy
- Opraveno `ArgumentException` při registraci duplicitních hodnot parametrů během předgenerování více nástrojů na stejném avatarovi

## [3.0.9] - 2026-08-26
- Pokud při převodu nelze z PosingSystem získat VRCAvatarDescriptor, zabrání se NullReferenceException a zobrazí se pokyny k potřebnému protokolu sestavení
- Opravena chyba `Avatar is null`, když pozdější předgenerování nebo sestavení znovu použilo náhled s dočasným Avatar vytvořeným a smazaným NDMF. Převod oddělen od mezipaměti náhledu a zabráněno kolizím názvů dočasných prostředků NDMF mezi náhledy
- Opraveno neukládání odkazu náhledu Prefabu 可愛いポーズ (Kawaii Posing) do scény, které množilo skryté náhledové avatary. Náhledy vyloučeny z ukládání scény a staré náhledy se uklízejí při načtení scény, vstupu do Play a znovunačtení skriptů

## [3.0.8] - 2026-08-18
- Opraven Unity Assertion a možné poškození dat póz předgenerovaných pohybů při opakované korekci výšky FloorAdjuster a podobnými nástroji
- Zlepšena bezpečnost měření výšky, aby neovlivňovalo původního avatara ani AnimationMode dalších nástrojů

## [3.0.7] - 2026-08-12
- Opraven posun pohledu a abnormální ohyb kolen na zádech s FloorAdjuster a dalšími nástroji výšky
- Opravena chyba, pokud je PosingSystem mimo avatara (děkujeme PancakeTorto1se)
- Opravena chyba způsobená pokračováním po neúspěšném nahrání avatara
- Opraveno občasné chybné rozpoznání změn nastavení hned po předgenerování
- Zlepšen automatický import pohybových animací
- Přidána varování pro některé avatary se zvláštními rigy
- Do okna úpravy animace přidáno potvrzení změn a zrcadlení hodnot korekcí (děkujeme kraihd)
- Přidány pokyny při chybách jiných nástrojů a kontroly míst náchylných k chybám

## [3.0.6] - 2026-03-17
- Opraveno výrazné narušení póz některých avatarů s původním vlastním řízením póz

## [3.0.5] - 2026-01-31
- Opraven pád Unity s některými ExpressionMenus
- Opraveny problémy avatara při použití integračního Prefabu ごろ寝EX (děkujeme あずき)
- Opraveno nesprávné otočení při spojování korekčních animací

## [3.0.4] - 2026-01-11
- Částečně zlepšeno chování avatara při vložení do Prefabs
- Přidáno odstranění licencí
- Opravena nesprávná poloha krku v mnoha pózách
- Mírně zrychleno otevírání Inspectoru
- Ikony se vytvářejí i při předgenerování; změněn text tlačítka aktualizace avatara
- Varování se skrývají, pokud předgenerování ještě nikdy neproběhlo (protože ho mnoho uživatelů zřejmě nepoužívá)
- Odstraněny nepotřebné prostředky
- Opravena nemožnost upravit prsty

## [3.0.3] - 2026-01-04
- Opraveno použití jiné předvolby než vybrané (děkujeme あずき)

## [3.0.2] - 2026-01-02
- Přidána podpora tmavého motivu Unity
- Opravena deformace pózy při otáčení během úpravy animace
- Změněna velikost generovaných ikon nabídky, zapnuta komprese a přidána volba ještě menších ikon
- Obnoveno propojení zámků sledování a automatický import pohybů avatara odstraněné ve 3.0.0

## [3.0.1] - 2025-12-29
- Opraveny problémy nastavení výchozích pohybů
- Opravena samovolná deformace pózy avatara

## [3.0.0] - 2025-12-28
- Přidáno předgenerování
- Přidána úprava animace
- Přidány předvolby

## [2.2.5] - 2025-05-08
- Vyřešeny problémy kompatibility s některými funkcemi avatara

## [2.2.4] - 2025-04-04
- Opraveno nepřepínání póz u některých avatarů a verzí ModularAvatar

## [2.2.3] - 2025-03-13
- Opraveny nesprávné pohyby některých avatarů, například skok 森羅
- Opravena občasná nesprávná počáteční hodnota Gesta vyp.

## [2.2.2] - 2025-03-06
- Opravena chyba v některých prostředích

## [2.2.1] - 2025-02-05
- Opraven pád Unity
- K názvu nástroje v NDMF Console přidána verze

## [2.2.0] - 2025-02-02
- Přidána volba vypnout gesta a použít tvar rukou z animace

## [2.1.6] - 2025-02-02
- Opraveno poškození póz s některými nástroji
- Opraveny chyby závislé na nastavení

## [2.1.5] - 2024-12-10
- Vyřešeny konflikty s některými funkcemi avatara
- Přidáno tlačítko pro odstranění verze Unitypackage

## [2.1.4] - 2024-12-08
- Opravena chyba závislá na avatarovi
- Opraveno nefunkční zamykání nohou kvůli konfliktům s funkcemi nebo avatarem
- Opraven návrat pózy avatara na výchozí VRC v režimu plochy

## [2.1.3] - 2024-12-01
- Opraveno nepřepínání póz s avatary nebo funkcemi používajícími nesprávný typ VRMode
- Přidáno zobrazení konfliktů různých typů parametrů mezi funkcemi v NDMF-Console

## [2.1.2] - 2024-12-01
- Do NDMF-Console přidáno vysvětlení chyby Unisaki Posing System při sestavení avatara s chybou jiného nástroje
- Opravena chyba sestavení některých avatarů
- Zlepšeno chování hned po načtení avatara
- Původní zpracování pohybu avatara se nyní odstraňuje, protože některým avatarům bránilo ve fungování

## [2.1.1] - 2024-11-20
- Opraven problém MergeAnimator v PosingOverride

## [2.1.0] - 2024-11-20
- Vyřešeno zabořování chodidel a boků do země v režimu plochy, aby všechny pózy vypadaly správně
- Přidána změna pózy nezávislá na výšce hlavy
- Přidán režim bez generování ikon pro nahrávání avatarů Quest
- Přidána upozornění na některé problémy avatara
- Vyřešeny konflikty zámků s AFK, zamykáním paží jiných nástrojů a podobnými funkcemi
- Přidáno automatické zamykání hlavy na zádech

## [2.0.0] - 2024-11-12
- Vydána verze s podporou VPM/VCC
- Změněn zobrazovaný název nástroje při chybách #1
- Přidány podmínky použití #5
