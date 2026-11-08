# Ændringshistorik

## [3.2.0] - Ikke udgivet

### Tilføjet

- Inspector, redigeringshandlinger, installationsdialoger, standardpositurnavne og genererede VRChat-menuer kan vises på 30 sprog.
- Inspector og spilmenu bruger fælles sprogvalg. Først bruges computerens sprog, og seneste valg genbruges i andre projekter for samme OS-bruger.
- README, ændringshistorik og offlinevejledning kan åbnes på valgt sprog.
- Visningsnavne for 128 standardpositurer og funktionsbeskrivelser følger eksisterende terminologi.

### Kompatibilitet

- Eksisterende avatarindstillinger, egne navne, assetreferencer, parameteridentifikatorer og licenslagring bevares.

## [3.1.2] - 2026-11-09

- Rettet, at ansigtet blev ved at vende opad efter oprejsning fra maveliggende med ”Reducér forvrængning fra hovedrotation”.
- Hovedlås kan skiftes via EX-menuens ”Indstillinger → Automatisk hovedlås på ryggen” selv med Fodhøjde fra. Punkter, som forsvandt i 3.1.0/3.1.1, vender tilbage ved build efter opdatering.
- ”Opdatér forberegning” virker nu også med selve avataren deaktiveret i Unity.
- Rettet manglende positurgenerering efter opdatering, hvis en ældre fælles positur-Controller lå i Assets. Egne Controller-indstillinger bevares.
- Ved deaktiveret værktøj eller forælder, som hindrer fælles positur-Animator, vises målets navn. Åbn stedet via ”Kontrollér indstillinger → Vælg mål”.
- Ændrede forberegningsindstillinger kan sammenlignes med sidste lagring via ”Detaljer → Se ændringer siden lagring”.
- Ved scriptfejl, som stopper build (PS050), vises op til 5 oprindelige Unity-fejl. Via forespørgsel om fejlen kan du gennemse, redigere og sende et udkast med Unity-/værktøjsversioner og fejlnummer.

Brugte du forberegning, skal du trykke ”Opdatér forberegning” efter opdateringen, gemme scenen og bygge og uploade avataren igen.

## [3.1.1] - 2026-11-08

- Aktive Gulvhøjde og Vend dig vises under Indstillinger i VRChats EX-menu, selv om Spejlvend er fra.
- Korrekte vendinger bevares for Ben strakt ud, Afslappet og Ét knæ op efter omarrangering, sletning eller omdøbning i ældre versioner, også efter fuld Unpack hvis oprindelsen kan identificeres. Egne positurer, bevidst deaktiverede vendinger og erstattede sidebevægelser bevares.
- Rettet kort maveliggende ved avatarvalg i VRChat og kort rygliggende ved hop med kun mavepositurer valgt. Hoppeskift slås fra under ”Funktioner → Praktiske funktioner → Skift liggende positurer ved at hoppe” og er aktivt som standard.
- Mistet reference til fælles positur-Animator Controller giver advarsel og genskabes fra indstillinger, når muligt. Referencer delt mellem avatarer bevares.
- Ved kompileringsfejl stoppes build i begyndelsen med forklaring. Automatisk oprydning af genererede Controllers udsættes, mens scriptfejl forhindrer referencekontrol.
- Officielt Prefab prioriteres ved tilføjelse, selv om et andet produktnavngivet Prefab findes. Forberegning fortsætter også ved forskelle kun i fælles Controllers linjeskift.
- Write Defaults for værktøjets FX-lag, såsom Gulvhøjde, følger avatarens indstillinger.

Brugte du forberegning, skal du efter opdateringen trykke ”Opdatér forberegning” på scenens værktøj, gemme scenen og bygge og uploade avataren igen.

## [3.1.0] - 2026-11-07

- Tilføjet automatisk lemjustering (IK) til kropsbygningen for understøttede positurer, avatarspecifik korrektion og beskyttelse mod bagudbøjede knæ.
- Position og retning kan justeres i Fototilstand med valg af synkronisering og visning af eget hoved.
- Gulvhøjde flytter hele kroppen op til 3m op/ned, separat fra Fodhøjde.
- Tilføjet vejrtrækning, stop af standardsvajen og reduceret forvrængning fra hovedrotation.
- Inspector viser funktioner og parameterforbrug. Søgning, massetilføjelse, omarrangering og let Scene-forhåndsvisning understøttes.
- Hurtigere skift mellem stående, hug og siddende på skrivebordet.
- Forbedrede forberegningsadvarsler efter ændringer, Inspector-rulning og lange stop under genereret oprydning.
- Rettet unødige opdateringsadvarsler og ny generering ved upload med automatisk import og uændret forberegning. Resultater gælder den genererede avatar uden ændring af originalens importindstillinger.
- Tilføjet Vend dig via hovedhældning og bevægelsesvalg pr. positur.
- Forbedret overførsel af ældre forudindstillingers vendinger, import af egne bevægelser og håndtering af genererede assets.

## [3.1.0-beta.2] - Åben beta (2026-11-05)

- IK-længdereference har nu fast længde samt højdeforskel (Y) og dybdeforskel (Z) mellem Head og Viewpoint.
- Inspector-advarsler vises også til venstre for objektnavne i Hierarchy: manglende forberegning, ændringer og aktuelle advarsler fra import og forudindstillinger.
- Rettet lange Unity-stop ved automatisk oprydning, eksempelvis ved sceneåbning, når Controllers fra andre scener blev indlæst.
- Positur-/funktionsændringer sletter ikke længere forberegningshistorik. Ændringer efter forberegning vises i stedet for aldrig udført, også som Hierarchy-advarsel.
- Sammenligningsresultater deles mellem Inspector og Hierarchy for lavere rullebelastning ved uændrede indstillinger og refererede assets. Builds sammenligner stadig hver gang.

## [3.1.0-beta.1] - Åben beta (2026-11-04)

- Normalt positurskift på skrivebordet bruger nu én evaluering i almindelige builds og forberegninger. VR og særlige forhold bevarer den hidtidige vej.
- Forberegninger fra alpha.3 og før kræver nu opdatering.
- Synspunkt bevares ved gulvhøjdeændring i Fototilstand, og nyeste gulvhøjde gendannes efter afslutning.

- Automatisk import registrerer kun ved generering og ændrer ikke manuelle importindstillinger ved forberegning/build.
- Automatisk imports til/fra, ændret Base og ryddede manuelle valg medtages i ny generering. Delte manuelle valg og justeringer bevares på tværs af produkter, og gamle importerede bevægelser vender tilbage til standardpositurer.
- Genererede forberegnings-Controllers ligger i én fil pr. scene-GUID og avatar. Referencer delt med kopier, andre scener eller Prefab adskilles før opdatering.
- Build bruger NDMF-midlertidige assets og øger ikke antallet af genererede redigeringsfiler.
- Genererede data kontrolleres ved sceneåbning og lagring. Ugemte ændringer, Undo/Redo og eksterne referencer bevares; kun tydeligt ejede ubrugte filer flyttes til papirkurven.
- Rotation, justeringsanimation og afspilningsposition for importerede bevægelser anvendes. Rotation, RootT/RootQ og Humanoid-justering deler behandling med almindelige positurer.
- Indlejrede BlendTree-justeringer kopieres til importmålet, så originalbevægelse og gangblanding bevares.
- Afspilningsposition for kun stående udfoldes til en statisk positur ved original billedfrekvens uden at stoppe gangafspilning.

## [3.1.0-alpha.2] - Lukket test (2026-10-26)

### Rettelser og ændringer

- Fototilstand og Gulvhøjde korrigerer også standard-/egne bevægelser uden valgt værktøjspositur.
- Gulvhøjde udvidet til 3m op/ned: 50% er oprindelig højde og standard, 0% er 3m ned og 100% er 3m op.
- Rettet ændring af kilde-BlendTree ved overskrivning af kun stående.
- Positurer uden IK-regler bruger manuel korrektion uden unødige genereringsadvarsler.
- Rettet blandede justeringskladder for ens navne og oversete bevægelsesændringer i samme asset.
- Rettet ugyldigt startpositurnummer, som valgte en anden startpositur.
- AnimatorOverrideController uden støtte giver vejledning i stedet for undtagelse ved import.
- Udtrykkeligt deaktiveret Vend dig bevares fra ældre forudindstillinger.
- Funktionsikoner tydeligere i lyst Editor-tema.

Kontrol af synspunkt og erstattede standardbevægelser i VRChat-klienten pågår.

## [3.1.0-alpha.1] - Før offentlig udgivelse

### Tilføjet

- Tilføjet til/fra pr. funktion og ExpressionParameters-forbrug.
- Tilføjet Fototilstand til fremadflytning og rotation.
- Tilføjet Fototilstand-synkronisering: kun lokalt 0 bit, synkroniseret 16 bit.
- Tilføjet valg om at holde eget avatarhoved synligt i Fototilstand.
- Tilføjet Gulvhøjde til hele kroppen inklusive hovedet, 3m op/ned. 50% er oprindelig højde og standard, 0% er 3m ned og 100% er 3m op.
- Tilføjet Vend dig via hovedhældning og animation pr. positur.
- Tilføjet vejrtrækning med Svag, Middel, Stærk, Hurtig og Egen.
- Vejrtrækningsbevægelse og til/fra-menu kan vælges separat: bevægelse 0 bit, synkroniseret til/fra 1 bit.
- Tilføjet stop af VRChats svajen i stilstand.
- Tilføjet reduceret forvrængning fra hovedrotation.
- Tilføjet automatisk lemjustering (IK) efter kropsbygning og til/fra for avatar og positur.
- Tilføjet ”Indarbejd IK i justeringer” til at gemme IK-resultat i justeringsanimation.
- Tilføjet avatarspecifik korrektion, beskyttelse mod bagudbøjede knæ og mindste bøjevinkel.
- Tilføjet let Scene-forhåndsvisning fra listen.
- Tilføjet kategorifaner, søgning på tværs, massetilføjelse og målplacering ved omarrangering.
- Produktets og fællessystemets historik kan vises i Inspector.

### Ændret

- Hurtigere forudindstillinger forkorter ventetid ved tilføjelse og listevisning.
- Tilføjelse, ændring og sletning af forudindstillinger opdaterer listen automatisk.
- Inspector opdelt i Funktioner, Positurer og Avataropsætning.
- Tilføjet beskrivelser og ikoner med ensartet åbning af detaljer.
- Vejrtrækningsdetaljer flyttet til funktioner og forudindstillinger til bunden af Avataropsætning.
- Egne bevægelser og automatisk import af oprindelige positurer er separate indstillinger.
- Færre dublerede helkropsanimationer til Fodhøjde.
- Positurbilledopdatering er en knap med bekræftelse.
- Advarselsvisning samlet under Kontrollér indstillinger.
- Nye særlige 8bit-Prefab afskaffet til fordel for funktionsvalg i almindelige Prefab; eksisterende referencer bevares.
- Mindste VRChat Avatars SDK-version ændret til 3.10.4.

### Rettet

- Standard-/egne bevægelser uden valgt positur korrigeres også af Fototilstand og Gulvhøjde.
- Rettet ændret kilde-BlendTree ved overskrivning af kun stående.
- Uden gyldig IK-regel bruges manuel justering, og unødige IK-advarsler undgås.
- Rettet blandede justeringer for ens navne og manglende ny generering ved bevægelsesskift i samme asset.
- Et deaktiveret startpositurnummer vælger ikke længere en anden positur, der genbruger nummeret.
- Import af oprindelige positurer viser vejledning i stedet for undtagelse for ikke-understøttet AnimatorOverrideController.
- Udtrykkeligt deaktiverede vendinger bevares fra ældre forudindstillinger.
- Bedre ikonbaggrund i Inspector gør funktioner tydelige i lyst Editor-tema.

- Rettet manglende ikoner for Vend dig, Gulvhøjde og Fototilstand ved avatartilføjelse.
- Rettet mistede vendeindstillinger fra ældre forudindstillinger.
- Rettet flytning af navneinput til anden positur ved åbning af andre detaljer.
- Rettet lukkede detaljer ved omarrangering samt Undo/Redo for indstillingsændringer.
- Rettet overlappende originalavatar og dobbelt placering i forhåndsvisning.
- Rettet forhåndsvisningsfejl med manglende komponenter.
- Rettet forkert IK-retning/placering af lemmer ved forskellig knogleretning og skala.
- Rettet advarsler og forberegningsstatus, der ikke opdateredes straks efter ændringer.
- Rettet genbrug af gammel forberegning efter IK-ændring.
- Rettet buildfejl ved generering af ekstra funktioner i visse opsætninger.

## [3.0.16] - 2026-09-14
- Mindste NDMF-version hævet til 1.14.0, fordi API’et `VirtualAnimatorController.SetParameter` bruges.

## [3.0.15] - 2026-09-13
- Rettet store ændringer af kropsposition og knæbøjning ved skift af nakkelås i rygliggende på avatarer med ændret Hips-skala og FloorAdjuster.

## [3.0.14] - 2026-09-09
- Rettet manglende Gestus fra-menu fra det andet positurværktøj på samme avatar.
- Rettet USSPS_Locomotion, der kunne gå tilbage til Standing trods valgt positur som SleepDown.
- Rettet forkert højde og X/Z-position med FloorAdjuster, inklusive behandlingsrækkefølgen i MA- og AAO-versionerne.
- Automatisk gendannelse til standard-PosingSystem_Locomotion ved slettet forberegnet AnimatorController i Generated.
- Rettet buildfejl uden stående positurer.
- Forudindstillinger sorteres efter avatarnavn.

## [3.0.13] - 2026-09-04
- Rettet ustabil tilbagevenden til stående i VR, hvis man rejste sig straks efter begyndende hug, hvilket skabte en selvovergang til stående.

## [3.0.12] - 2026-09-04
- Rettet falsk registrering af ændrede indstillinger ved NDMF-build uden tidligere forberegning.
- Rettet `AddAssetToSameFile`-fejl ved tilføjelse af genererede assets til midlertidig AnimatorController under NDMF-build.

## [3.0.11] - 2026-08-31
- Rettet mislykket behandling, når forberegningsavatarens Scene blev ugyldig i NDMF 1.14.7 eller nyere.
- Rettet `MissingReferenceException`, når NDMF-fejlvisning refererede til en destrueret LocalizationAsset.

## [3.0.10] - 2026-08-29
- Forudindstillingsassets indlæses først ved åbning af valgvinduet; cache genbruges ved uændret GUID-fortegnelse.
- Hierarchy-advarsels-/fejlikoner genberegnes kun ved ændring, og produktmenuers assetsøgning bruger fælles cache. Mindsker Editor-frysninger med få sekunders mellemrum i scener med mange værktøjer.
- Rettet `ArgumentException` ved dobbelte parameterværdier i forberegning af flere positurværktøjer på samme avatar.

## [3.0.9] - 2026-08-26
- Forhindret NullReferenceException, når VRCAvatarDescriptor ikke fås fra PosingSystem under konvertering, med vejledning om nødvendige buildlogs.
- Rettet `Avatar is null`, når senere forberegning/build genbrugte forhåndsvisning af en midlertidig Avatar oprettet/slettet af NDMF. Konvertering adskilt fra forhåndsvisningscache, og navnekonflikter mellem midlertidige NDMF-assets forhindret.
- Rettet en forhåndsvisningsreference, der ikke blev gemt fra Prefab ”可愛いポーズ”, som ophobede skjulte avatarer. Forhåndsvisninger udelukkes fra scenelagring; gamle ryddes ved sceneindlæsning, overgang til Play og scriptgenindlæsning.

## [3.0.8] - 2026-08-18
- Rettet Unity Assertion og risiko for ugyldige positurdata ved ny højdekorrigering af forberegnede bevægelser med eksempelvis FloorAdjuster.
- Sikrere højdemåling, som ikke påvirker originalavataren eller andre værktøjers AnimationMode.

## [3.0.7] - 2026-08-12
- Rettet forskudt synspunkt og unormal knæbøjning på ryggen sammen med højdeværktøjer som FloorAdjuster.
- Rettet fejl med PosingSystem uden for avataren (tak PancakeTorto1se).
- Rettet fortsat behandling efter mislykket avatarupload, som gav fejl.
- Rettet falsk indstillingsændring straks efter forberegning.
- Forbedret automatisk import af bevægelsesanimationer.
- Advarsel ved visse særlige avatarrigge.
- Tilføjet ændringsbekræftelse og spejling af justeringsværdier i animationsjustering (tak kraihd).
- Tilføjet vejledning ved fejl i andre værktøjer og kontrol af fejlfølsomme steder.

## [3.0.6] - 2026-03-17
- Rettet stærkt forvrængede positurer på visse avatarer med egen oprindelig positurstyring.

## [3.0.5] - 2026-01-31
- Rettet Unity-nedbrud med visse ExpressionMenus.
- Rettet avatarfejl med integrations-Prefab for ごろ寝EX (tak あずき).
- Rettet forkert rotation ved sammenlægning af justeringsanimationer.

## [3.0.4] - 2026-01-11
- Forbedret visse handlinger med avatarer i Prefabs.
- Tilføjet licenssletning.
- Rettet mange positurer med forkert nakke.
- Inspector åbner lidt hurtigere.
- Ikoner genereres også under forberegning; tekst på avataropdateringsknap ændret.
- Advarsel skjules, hvis forberegning aldrig er kørt, da mange sandsynligvis ikke bruger den.
- Unødvendige ressourcer fjernet.
- Rettet fingre, som ikke kunne justeres.

## [3.0.3] - 2026-01-04
- Rettet anvendelse af forkert forudindstilling i forhold til valget (tak あずき).

## [3.0.2] - 2026-01-02
- Understøttelse af Unitys mørke tema.
- Rettet forvrænget positur ved rotation i animationsjustering.
- Genererede menuikoners størrelse ændret og komprimering aktiveret; tilføjet mulighed for endnu mindre ikoner.
- Genindført sporingsintegration og automatisk import af avatarrørelser, som var fjernet i 3.0.0.

## [3.0.1] - 2025-12-29
- Rettet fejl i standardbevægelsesindstillinger.
- Rettet avatarpositurer, som forvrængedes af sig selv.

## [3.0.0] - 2025-12-28
- Tilføjet forberegning.
- Tilføjet animationsjustering.
- Tilføjet forudindstillinger.

## [2.2.5] - 2025-05-08
- Løst kompatibilitetsproblemer med visse værktøjer.

## [2.2.4] - 2025-04-04
- Rettet manglende positurskift med visse avatarer og ModularAvatar-versioner.

## [2.2.3] - 2025-03-13
- Rettet forkerte bevægelser på visse avatarer, eksempelvis hop med 森羅.
- Rettet lejlighedsvis forkert standardværdi for Gestus fra.

## [2.2.2] - 2025-03-06
- Rettet fejl i visse miljøer.

## [2.2.1] - 2025-02-05
- Rettet Unity-nedbrud.
- Version tilføjet til værktøjsnavn i NDMF Console.

## [2.2.0] - 2025-02-02
- Tilføjet valg om at slå gestus fra og bruge animationens håndform.

## [2.1.6] - 2025-02-02
- Rettet ødelagte positurer med visse andre værktøjer.
- Rettet indstillingsafhængige fejl.

## [2.1.5] - 2024-12-10
- Løst konflikter med visse værktøjer.
- Tilføjet knap til fjernelse af Unitypackage-versionen.

## [2.1.4] - 2024-12-08
- Rettet fejl på visse avatarer.
- Rettet ubrugelig fodlås ved værktøjs-/avatarkonflikter.
- Rettet avatarpositur, som vendte tilbage til VRC-standard på skrivebordet.

## [2.1.3] - 2024-12-01
- Rettet manglende positurskift sammen med avatarer/værktøjer, der bruger forkert VRMode-type.
- Tilføjet NDMF-Console-visning af værktøjskonflikter på grund af forskellige parametertyper.

## [2.1.2] - 2024-12-01
- Tilføjet NDMF-Console-forklaring, når andre værktøjers fejl medfører Unisaki Posing System-fejl ved build.
- Rettet buildfejl på visse avatarer.
- Forbedret adfærd lige efter avatarindlæsning.
- Avatarens oprindelige bevægelsesbehandling fjernes, fordi den kan forhindre funktionen på visse avatarer.

## [2.1.1] - 2024-11-20
- Rettet MergeAnimator-relaterede fejl i PosingOverride.

## [2.1.0] - 2024-11-20
- Løst fødder/hofter under jorden på skrivebordet, så alle positurer kan vises korrekt.
- Tilføjet positurskift uafhængigt af hovedhøjde.
- Tilføjet tilstand uden ikongenerering til Quest-upload.
- Tilføjet flere advarsler ved avatarproblemer.
- Løst konflikter mellem låse og AFK eller andre værktøjers armlås.
- Tilføjet automatisk nakkelås på ryggen.

## [2.0.0] - 2024-11-12
- Udgivet VPM/VCC-version.
- Ændret værktøjsnavn ved fejl #1.
- Tilføjet brugsbetingelser #5.
