# Endringslogg

## [3.2.0] - Ikke utgitt

### Lagt til

- Inspector, redigering, installasjonsdialoger, standardpositurnavn og genererte VRChat-menyer kan vises på 30 språk.
- Felles språkvalg for Inspector og spillmeny. Maskinens språk brukes først, og siste valg gjenbrukes i andre prosjekter for samme OS-bruker.
- README, endringslogg og offlineveiledning kan åpnes på valgt språk.
- Visningsnavn for 128 standardpositurer og funksjonsbeskrivelser følger eksisterende terminologi.

### Kompatibilitet

- Eksisterende avatarinnstillinger, egne navn, assetreferanser, parameteridentifikatorer og lisenslagring beholdes.

## [3.1.4] - 2026-11-11

- Rettet at avatarspesifikke animasjoner tilordnet huk og mageleie ikke ble brukt, slik at VRChats standardpositurer ble vist i stedet. Dette gjelder både automatisk import og manuelle tildelinger.
- Rettet at hår, briller og andre deler festet med MA Bone Proxy ble stående på opprinnelig plass under redigering eller forhåndsvisning av positurer. De følger nå forhåndsvisningens positur mens originalavatarens innstillinger og plassering beholdes.
- Rettet feil i positurkorrigering basert på synspunktets posisjon ved bildetaking av positurer som «仰向け寝_お疲れ». Bildene bruker en animasjon med positur- og gulvkorrigeringene som brukes under bygg.

Oppdater Unisaki Posing System til 3.1.4 i ALCOM / VCC. Kawaii Posing 3.1.2 og Cool Posing 1.1.0 kan fortsatt brukes uendret. Brukte du forhåndsbygg, trykker du «Oppdater forhåndsbygget» etter oppdateringen, lagrer scenen og bygger og laster opp avataren på nytt.

## [3.1.3] - 2026-11-09

- «Oppdater positurminiatyrer» og forhåndsbygg på en duplisert avatar sletter ikke lenger originalavatarens positurbilder.
- Rettet visning av «Innstillinger endret» etter bare å ta bildene på nytt, når forhåndsbygget var utført før scenens første lagring eller i Prefab-redigeringsmodus.
- Rettet at forhåndsbygg kunne stoppe underveis hvis positurdata fra et tidligere forhåndsbygg lå igjen.
- Lettere åpning av listen når positurbilder ennå mangler. Også ventingen som låste Unity til pakkeopplysningene var hentet ved start av forhåndsbygg eller bygg, er gjennomgått.
- Redigeringsinnstillinger for positurverktøy som er deaktivert i avataren, fjernes nå også ved PC- og Android-bygg.
- «Spør om denne feilen» kan nå ta med senere diagnosenumre og andre opplysninger fra Console i utkastet, selv om de første feilene ikke viser en forståelig årsak. Ved feil i klippinnstillingene for «Snu deg» inneholder utkastet også punkter som må kontrolleres.

Oppdater Unisaki Posing System til 3.1.3 i ALCOM / VCC. Kawaii Posing 3.1.2 og Cool Posing 1.1.0 kan fortsatt brukes uendret. Fordi produktene angir en tidligere versjon av fellessystemet, kan en konfliktbekreftelse vises under oppdatering. Brukte du forhåndsbygg, trykker du «Oppdater forhåndsbygget» etter oppdateringen, lagrer scenen og bygger og laster opp avataren på nytt.

## [3.1.2] - 2026-11-09

- Rettet vedvarende oppovervendt ansikt etter oppreisning fra mageleie med «Reduser positurforvrengning fra hoderotasjon».
- Hodelås kan endres i EX-menyens «Innstillinger → Automatisk hodelås i ryggleie» selv med Fothøyde av. Valg som forsvant i 3.1.0/3.1.1 kommer tilbake ved bygg etter oppdatering.
- «Oppdater forhåndsbygget» virker også når selve avataren er deaktivert i Unity.
- Rettet manglende positurgenerering etter oppdatering med gammel felles positur-Controller i Assets. Egne Controller-innstillinger beholdes.
- Når deaktivert verktøy eller overordnet hindrer felles positur-Animator, vises målets navn. Åpne via «Kontroller innstillinger → Velg mål».
- Endrede forhåndsbygginnstillinger kan sammenlignes med siste lagring under «Detaljer → Se endringer siden lagring».
- Ved byggstopp på grunn av skriptfeil (PS050) vises opptil 5 opprinnelige Unity-feil. Via forespørsel om feilen kan du gjennomgå, redigere og sende et utkast med Unity-/verktøyversjoner og feilnummer.

Brukte du forhåndsbygg, trykker du «Oppdater forhåndsbygget» etter oppdatering, lagrer scenen og bygger og laster opp på nytt.

## [3.1.1] - 2026-11-08

- Aktive Gulvhøyde og Snu deg vises i Innstillinger i VRChats EX-meny selv med Speilvend av.
- Riktig snuing beholdes for Bena ut, Avslappet og Ett kne opp etter endret rekkefølge, sletting eller navnebytte i eldre versjoner, også etter full Unpack hvis opphavet kan identifiseres. Egne positurer, uttrykkelig deaktivert snuing og erstattede sidebevegelser beholdes.
- Rettet kort mageleie ved avatarvalg i VRChat og kort ryggleie ved hopp med bare magepositurer valgt. Hoppbytte slås av under «Funksjoner → Praktiske funksjoner → Bytt liggende positur ved å hoppe» og er på som standard.
- Tapt gjenopprettbar felles positur-Animator Controller-referanse gir advarsel og bygges opp fra innstillinger. Referanser delt av flere avatarer beholdes.
- Kompileringsfeil stopper bygget med forklaring i starten. Automatisk opprydding av genererte Controllers utsettes mens skriptfeil hindrer referansekontroll.
- Offisielt Prefab prioriteres ved tillegg selv med annet Prefab med produktnavnet i prosjektet. Forhåndsbygg fortsetter også ved bare ulike linjeskift i felles Controller.
- Write Defaults for tilførte FX-lag, som Gulvhøyde, følger avatarens innstillinger.

Brukte du forhåndsbygg, trykker du «Oppdater forhåndsbygget» på scenens verktøy etter oppdatering, lagrer og bygger og laster opp på nytt.

## [3.1.0] - 2026-11-07

- Lagt til automatisk lemjustering (IK) etter kroppsbygning for støttede positurer, avatarkorrigering og vern mot bakoverbøyde knær.
- Fotomodus justerer posisjon/retning med valg av synkronisering og visning av eget hode.
- Gulvhøyde flytter hele kroppen opptil 3m opp/ned, atskilt fra Fothøyde.
- Lagt til pust, stopp av standard hvilebevegelse og redusert forvrengning fra hoderotasjon.
- Inspector viser funksjoner og parameterforbruk. Søk, massetillegg, sortering og enkel Scene-forhåndsvisning støttes.
- Raskere stå-/huk-/sittebytte i skrivebordsmodus.
- Bedre forhåndsbyggadvarsler etter endringer, Inspector-rulling og lange stopp ved opprydding.
- Rettet unødige oppdateringsadvarsler og ny generering ved opplasting med automatisk import og uendret forhåndsbygg. Resultatet gjelder generert avatar uten å endre originalobjektets importinnstillinger.
- Lagt til Snu deg ved hodehelling og bevegelsesvalg per positur.
- Bedre overføring av eldre forhåndsinnstillingers snuing, import av egne bevegelser og genererte assets.

## [3.1.0-beta.2] - Åpen beta (2026-11-05)

- IK-lengdereferanse støtter fast lengde og høydeforskjell (Y)/dybdeforskjell (Z) mellom Head og Viewpoint.
- Inspector-advarsler vises også til venstre for Hierarchy-objektnavn: manglende forhåndsbygg, endringer og gjeldende import-/forhåndsinnstillingsadvarsler.
- Rettet lange Unity-stopp når automatisk opprydding, eksempelvis ved sceneåpning, lastet Controllers fra andre scener.
- Endret liste eller funksjoner sletter ikke forhåndsbygghistorikken. Endring etter forhåndsbygg vises i stedet for aldri utført, også som Hierarchy-advarsel.
- Sammenligningsresultater deles mellom Inspector og Hierarchy for mindre rullebelastning ved uendrede innstillinger og refererte assets. Bygg sammenligner fortsatt hver gang.

## [3.1.0-beta.1] - Åpen beta (2026-11-04)

- Vanlig positurbytte på skrivebordet bruker én evaluering ved vanlige bygg og forhåndsbygg. VR og spesielle forhold beholder tidligere vei.
- Forhåndsbygg fra alpha.3 eller før krever oppdatering.
- Synspunkt beholdes ved endret gulvhøyde i Fotomodus, og siste gulvhøyde gjenopprettes etter avslutning.

- Automatisk import registrerer bare ved generering og endrer ikke manuelle importinnstillinger ved forhåndsbygg/bygg.
- Automatisk import på/av, Base-endring og fjernet manuelt valg medtas ved ny generering. Delte manuelle valg og justeringer beholdes mellom produkter, og eldre importerte bevegelser går tilbake til standardpositurer.
- Genererte forhåndsbygg-Controllers ligger i én fil per scene-GUID og avatar. Referanser delt med kopier, andre scener eller Prefab skilles før oppdatering.
- Bygg bruker NDMF-midlertidige assets og øker ikke antall genererte redigeringsfiler.
- Genererte data kontrolleres ved sceneåpning og lagring. Ulagrede endringer, Undo/Redo og eksterne referanser beholdes; bare tydelig eide ubrukte filer flyttes til papirkurven.
- Rotasjon, justeringsanimasjon og avspillingsposisjon for importerte bevegelser brukes. Rotasjon, RootT/RootQ og Humanoid-justering deler behandling med vanlige positurer.
- Nestede BlendTree-justeringer kopieres for importmålet slik at originalbevegelse og gangblanding beholdes.
- Avspillingsposisjon for bare stående utvides til statisk positur med opprinnelig bildefrekvens uten å stoppe gange.

## [3.1.0-alpha.2] - Lukket test (2026-10-26)

### Rettelser og endringer

- Fotomodus og Gulvhøyde korrigerer også standard-/egne bevegelser uten valgt verktøypositur.
- Gulvhøyde utvidet til 3m opp/ned: 50% er opprinnelig høyde og standard, 0% er 3m ned og 100% er 3m opp.
- Rettet endring av kilde-BlendTree ved overskriving av bare stående.
- Positurer uten IK-regler bruker manuell korrigering uten unødige genereringsadvarsler.
- Rettet blandede justeringsutkast for like navn og oversette bevegelsesendringer i samme asset.
- Rettet ugyldig startpositurnummer som valgte en annen startpositur.
- Ikke-støttet AnimatorOverrideController gir veiledning i stedet for unntak ved import.
- Uttrykkelig deaktivert Snu deg beholdes fra eldre forhåndsinnstillinger.
- Funksjonsikoner tydeligere i lyst Editor-tema.

Kontroll av synspunkt og erstattede standardbevegelser i VRChat-klienten pågår.

## [3.1.0-alpha.1] - Før offentlig utgivelse

### Lagt til

- Lagt til på/av per funksjon og ExpressionParameters-forbruk.
- Lagt til Fotomodus for fremflytting og rotasjon.
- Lagt til Fotomodus-synkronisering: bare lokalt 0 bit, synkronisert 16 bit.
- Lagt til valg om å beholde eget avatarhode synlig i Fotomodus.
- Lagt til Gulvhøyde for hele kroppen med hodet, 3m opp/ned. 50% er opprinnelig høyde og standard, 0% er 3m ned og 100% er 3m opp.
- Lagt til Snu deg via hodehelling og animasjon per positur.
- Lagt til pust med Svak, Middels, Sterk, Rask og Egendefinert.
- Pustebevegelse og på/av-meny kan velges separat: bare bevegelse 0 bit, synkronisert på/av 1 bit.
- Lagt til stopp av VRChats standard hvilebevegelse.
- Lagt til redusert forvrengning fra hoderotasjon.
- Lagt til automatisk lemjustering (IK) etter kroppsbygning og på/av for avatar og positur.
- Lagt til «Lagre IK i justeringer» for IK-resultat i justeringsanimasjon.
- Lagt til avatarkorrigering, vern mot bakoverbøyde knær og minste bøyevinkel.
- Lagt til enkel forhåndsvisning i Scene fra listen.
- Lagt til kategorifaner, søk på tvers, massetillegg og målplassering ved sortering.
- Produktets og fellessystemets endringslogg kan vises i Inspector.

### Endret

- Raskere forhåndsinnstillinger reduserer ventetid ved tillegg og listevisning.
- Tillegg, endring og sletting av forhåndsinnstillinger oppdaterer listen automatisk.
- Inspector delt i Funksjoner, Positurer og Avataroppsett.
- Beskrivelser og ikoner med ensartet åpning av detaljer.
- Pustedetaljer flyttet til funksjonsoppsett og forhåndsinnstillinger til bunnen av Avataroppsett.
- Egne bevegelser og automatisk import av originale positurer er separate valg.
- Færre dupliserte helkroppsanimasjoner for Fothøyde.
- Positurbildeoppdatering er en knapp med bekreftelse.
- Advarselsvisning samlet i Kontroller innstillinger.
- Nye særskilte 8bit-Prefab avviklet til fordel for funksjonsvalg i vanlig Prefab. Eksisterende referanser beholdes.
- Minste VRChat Avatars SDK-versjon endret til 3.10.4.

### Rettet

- Standard-/egne bevegelser uten valgt positur korrigeres også av Fotomodus og Gulvhøyde.
- Rettet endring av kilde-BlendTree ved overskriving av bare stående.
- Uten gyldig IK-regel brukes manuell justering uten unødige IK-advarsler.
- Rettet blandede justeringer for like navn og manglende ny generering ved bevegelsesbytte i samme asset.
- Deaktivert startpositurnummer velger ikke en annen positur som gjenbruker nummeret.
- Import av originale positurer gir veiledning i stedet for unntak for ikke-støttet AnimatorOverrideController.
- Uttrykkelig deaktivert snuing beholdes fra eldre forhåndsinnstillinger.
- Bedre ikonbakgrunn i Inspector gjør funksjonene tydelige i lyst Editor-tema.

- Rettet manglende ikoner for Snu deg, Gulvhøyde og Fotomodus ved avatartillegg.
- Rettet tapte innstillinger for snuing fra eldre forhåndsinnstillinger.
- Rettet navneinput som flyttet til annen positur ved åpning av detaljer.
- Rettet lukkede detaljer ved sortering og Undo/Redo for endringer.
- Rettet overlappende originalavatar og dobbel plassering i forhåndsvisning.
- Rettet forhåndsvisningsfeil ved manglende komponenter.
- Rettet IK-retning/plassering for lemmer ved ulik beinretning og skala.
- Rettet advarsler/forhåndsbyggstatus som ikke oppdaterte straks etter endring.
- Rettet gjenbruk av gammelt forhåndsbygg etter IK-endring.
- Rettet byggfeil ved generering av tilleggsfunksjoner i enkelte oppsett.

## [3.0.16] - 2026-09-14
- Minste NDMF-versjon hevet til 1.14.0 fordi API-et `VirtualAnimatorController.SetParameter` brukes.

## [3.0.15] - 2026-09-13
- Rettet store endringer i kroppsstilling og knebøyning ved bytte av nakkelås i ryggleie på avatarer med endret Hips-skala og FloorAdjuster.

## [3.0.14] - 2026-09-09
- Rettet manglende Gester av-meny fra andre og senere positurverktøy på samme avatar.
- Rettet USSPS_Locomotion som kunne gå tilbake til Standing tross valgt positur som SleepDown.
- Rettet feil høyde og X/Z-posisjon med FloorAdjuster og støtter MA-/AAO-utgavenes behandlingsrekkefølge.
- Automatisk gjenoppretting til standard PosingSystem_Locomotion ved slettet forhåndsbygd AnimatorController i Generated.
- Rettet byggfeil uten stående positurer.
- Forhåndsinnstillinger sorteres etter avatarnavn.

## [3.0.13] - 2026-09-04
- Rettet ustabil retur til stående i VR ved oppreisning straks etter påbegynt huk, som utløste selvovergang til stående.

## [3.0.12] - 2026-09-04
- Rettet feilaktig registrering av endrede innstillinger ved NDMF-bygg uten forhåndsbygg.
- Rettet `AddAssetToSameFile`-feil ved tillegg av genererte assets til midlertidig AnimatorController under NDMF-bygg.

## [3.0.11] - 2026-08-31
- Rettet mislykket behandling ved ugyldig Scene på forhåndsbyggavataren med NDMF 1.14.7 eller nyere.
- Rettet `MissingReferenceException` når NDMF-feilvisning refererte til destruert LocalizationAsset.

## [3.0.10] - 2026-08-29
- Faktiske forhåndsinnstillingsassets lastes først når valgvisningen åpnes. Cache gjenbrukes ved uendret GUID-liste.
- Hierarchy-advarsels-/feilikoner vurderes bare ved endringer, og produktmenyers assetsøk bruker felles cache. Reduserer Editor-frysinger med få sekunders mellomrom i scener med mange verktøy.
- Rettet `ArgumentException` ved dupliserte parameterverdier under forhåndsbygg med flere verktøy på samme avatar.

## [3.0.9] - 2026-08-26
- Hindrer NullReferenceException når VRCAvatarDescriptor ikke kan hentes fra PosingSystem under konvertering, med veiledning om nødvendige bygglogger.
- Rettet `Avatar is null` når senere forhåndsbygg/bygg gjenbrukte forhåndsvisning av midlertidig Avatar opprettet/slettet av NDMF. Konvertering skilt fra visningscache, og navnekollisjoner mellom midlertidige NDMF-assets fra flere visninger hindres.
- Rettet ulagret forhåndsvisningsreferanse fra Prefab «可愛いポーズ», som samlet skjulte avatarer. Forhåndsvisninger unntas fra scenelagring, og gamle ryddes ved scenelasting, Play-overgang og skriptomlasting.

## [3.0.8] - 2026-08-18
- Rettet Unity Assertion og fare for ugyldige positurdata ved gjentatt høydekorrigering av forhåndsbygde bevegelser med for eksempel FloorAdjuster.
- Sikrere høydemåling som ikke forstyrrer originalavataren eller andre verktøys AnimationMode.

## [3.0.7] - 2026-08-12
- Rettet forskjøvet synspunkt og unormal knebøyning på ryggen sammen med høydeverktøy som FloorAdjuster.
- Rettet feil med PosingSystem utenfor avataren (takk PancakeTorto1se).
- Rettet fortsatt behandling etter mislykket opplasting som førte til feil.
- Rettet feilaktig oppdagelse av endringer straks etter forhåndsbygg.
- Bedre automatisk import av forflytningsbevegelser.
- Advarsel for enkelte spesielle avatarrigger.
- Lagt til endringsbekreftelse og speiling av justeringsverdier i animasjonsjustering (takk kraihd).
- Lagt til veiledning ved feil i andre verktøy og kontroll av feilutsatte steder.

## [3.0.6] - 2026-03-17
- Rettet sterkt forvrengte positurer på enkelte avatarer med egen opprinnelig positurstyring.

## [3.0.5] - 2026-01-31
- Rettet Unity-krasj med enkelte ExpressionMenus.
- Rettet avatarfeil med integrasjons-Prefab for ごろ寝EX (takk あずき).
- Rettet feil rotasjon ved sammenslåing av justeringsanimasjoner.

## [3.0.4] - 2026-01-11
- Forbedret enkelte handlinger med avatarer i Prefabs.
- Lagt til lisenssletting.
- Rettet mange positurer med feil nakke.
- Inspector åpnes litt raskere.
- Ikoner genereres også under forhåndsbygg; tekst på avataroppdateringsknapp endret.
- Advarsel skjules hvis forhåndsbygg aldri er utført, siden mange trolig ikke bruker det.
- Unødvendige ressurser fjernet.
- Rettet fingre som ikke kunne justeres.

## [3.0.3] - 2026-01-04
- Rettet bruk av annen forhåndsinnstilling enn valgt (takk あずき).

## [3.0.2] - 2026-01-02
- Støtte for Unitys mørke tema.
- Rettet forvrengt positur ved rotasjon i animasjonsjustering.
- Endret menyikonstørrelse og aktivert komprimering, med valg av enda mindre ikoner.
- Gjeninnført sporingssamordning og automatisk import av avatarrørelser, fjernet i 3.0.0.

## [3.0.1] - 2025-12-29
- Rettet standardbevegelsesinnstillinger.
- Rettet avatarpositurer som ble forvrengt av seg selv.

## [3.0.0] - 2025-12-28
- Lagt til forhåndsbygg.
- Lagt til animasjonsjustering.
- Lagt til forhåndsinnstillinger.

## [2.2.5] - 2025-05-08
- Løst kompatibilitetsproblemer med enkelte verktøy.

## [2.2.4] - 2025-04-04
- Rettet manglende positurbytte med enkelte avatarer og ModularAvatar-versjoner.

## [2.2.3] - 2025-03-13
- Rettet feil bevegelser på enkelte avatarer, for eksempel hopp med 森羅.
- Rettet iblant feil standardverdi for Gester av.

## [2.2.2] - 2025-03-06
- Rettet feil i enkelte miljøer.

## [2.2.1] - 2025-02-05
- Rettet Unity-krasj.
- Versjon lagt til i verktøynavnet i NDMF Console.

## [2.2.0] - 2025-02-02
- Lagt til valg for Gester av og bruk av animasjonens håndform.

## [2.1.6] - 2025-02-02
- Rettet ødelagte positurer med enkelte andre verktøy.
- Rettet innstillingsavhengige feil.

## [2.1.5] - 2024-12-10
- Løst enkelte verktøykonflikter.
- Lagt til knapp for fjerning av Unitypackage-versjonen.

## [2.1.4] - 2024-12-08
- Rettet feil på enkelte avatarer.
- Rettet ubrukelig fotlås ved verktøy-/avatarkonflikter.
- Rettet retur til VRC-standardpositur på skrivebordet.

## [2.1.3] - 2024-12-01
- Rettet manglende positurbytte med avatarer/verktøy som bruker feil VRMode-type.
- Lagt til visning i NDMF-Console av verktøykonflikter fra ulike parametertyper.

## [2.1.2] - 2024-12-01
- Lagt til NDMF-Console-forklaring når andre verktøys feil fører til Unisaki Posing System-feil under bygg.
- Rettet byggfeil på enkelte avatarer.
- Bedre oppførsel straks etter avatarlasting.
- Opprinnelig avatarforflytning fjernes fordi den kan hindre funksjon på enkelte avatarer.

## [2.1.1] - 2024-11-20
- Rettet MergeAnimator-relaterte feil i PosingOverride.

## [2.1.0] - 2024-11-20
- Løst føtter/hofter under bakken på skrivebordet slik at alle positurer vises riktig.
- Lagt til positurbytte uavhengig av hodehøyde.
- Lagt til modus uten ikongenerering for Quest-opplasting.
- Lagt til flere advarsler om avatarproblemer.
- Løst låsekonflikter med AFK og andre verktøys armlås.
- Lagt til automatisk nakkelås i ryggleie.

## [2.0.0] - 2024-11-12
- Utgitt VPM/VCC-versjon.
- Endret visningsnavn ved feil #1.
- Lagt til bruksvilkår #5.
