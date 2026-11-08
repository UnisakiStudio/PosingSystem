# Unisaki Posing System

Felles positursystem for blant annet Kawaii Posing og Cool Posing. Det bruker Modular Avatar og NDMF til å generere nødvendige menyer og animasjoner ved avataropplasting.

Denne README gjelder v3.2.0 under utvikling. Endelig utgivelsesdato er ikke fastsatt. Installert versjon vises øverst i verktøyets Inspector. Se også [nye funksjoner og endringslogg](CHANGELOG.md).

## Visningsspråk og dokumentasjon uten nett

Velg språk med «Language / 言語» øverst i Inspector. Ett valg styrer både Inspector og den genererte VRChat-menyen. Last opp avataren på nytt for å endre språket i spillet.

Uten valg brukes datamaskinens språk, eller engelsk hvis språket ikke støttes. Windows, macOS og Linux støttes. Siste valg lagres i EditorPrefs og brukes i andre prosjekter for samme OS-bruker på denne maskinen. Avatarens lagrede uttrykkelige valg beholdes. Det synkroniseres ikke til andre maskiner. Lisenslagringen endres ikke.

Velg README, endringslogg og brukerveiledning fra medfølgende [språkliste](../index.html). Inspector-knappene for brukerveiledning og endringer følger også språket. Originale bruksvilkår og referanseoversettelser leveres separat som før.

## Positurbytte i skrivebordsmodus

Forsinkelsen før valgt positur vises ved bytte mellom stående, huk og sittende i skrivebordsmodus er redusert. Det gjelder vanlige bygg og forhåndsbygg uten egne kommandoer eller ekstra innstillinger. VR og spesielle tilstander bruker tidligere behandling. Oppdater gamle forhåndsbygg før opplasting.

## Installasjon og oppdatering

Følg kjøpsfilene og installasjonsanvisningene for produktet, og legg til verktøyet med VCC eller ALCOM. Dette systemet installeres som en avhengighet.

v3.2.0 krever følgende:

- Unity 2022.3-prosjekt for VRChat-avatarer
- VRChat Avatars SDK 3.10.4 eller nyere
- Modular Avatar 1.x fra 1.15.1
- NDMF (Non-Destructive Modular Framework) 1.x fra 1.14.0

Bruker du Kawaii Posing, oppdaterer du til 3.2.0 i ALCOM / VCC og kontrollerer at Unisaki Posing System også er 3.2.0. Bruker du forhåndsbygg, trykker du «Oppdater forhåndsbygget», lagrer scenen og laster opp avataren.

Cool Posing 1.2.0 bruker fellessystem 3.2.0. Oppdater produktet og fellessystemet sammen til samsvarende versjoner.

### Til brukere av 8bit-versjonen

Nye tillegg bruker vanlig Prefab. Slå av unødvendige valg under «Funksjoner» for lavere forbruk. Bare positurvalg bruker som utgangspunkt 8 bit.

Eldre 8bit-Prefab beholdes i hvert produkts «Editor/Legacy» for eksisterende avatar- og forhåndsinnstillingsreferanser. De trenger ikke slettes eller erstattes for videre bruk. Valg av 8bit i Unitys meny for nye tillegg er avviklet.

## Slik leser du innstillingene

Velg «可愛いポーズ», «カッコいいポーズ» eller tilsvarende objekt inne i avataren i Hierarchy for å konfigurere verktøyet.

| Fane | Innstillinger |
| --- | --- |
| Funksjoner | Spillmenyer, praktiske funksjoner, ikoner og ExpressionParameters-forbruk |
| Positurer | Valg, tillegg, rekkefølge, forhåndsvisning i Scene, detaljer og animasjonsjustering |
| Avataroppsett | Korrigering for kroppsbygning, knær, import av opprinnelige/egne bevegelser og forhåndsinnstillinger |

«Åpne innstillinger» viser detaljer under det aktuelle valget. Pusteinnstillinger finnes også under Funksjoner.

«Hjelp / Endringer» øverst åpner brukerveiledning, kontakt og endringslogg. Du kan bytte mellom produktets og Unisaki Posing Systems historikk.

## Funksjoner og ExpressionParameters-forbruk

Valg under «Funksjoner > Menyfunksjoner» vises i spillmenyen. «Speilvend» gjør det for eksempel mulig å bytte speiling i VRChat; valget i Inspector tvinger ikke alle positurer til alltid å være speilvendt.

| Funksjon | Ekstra bitforbruk |
| --- | ---: |
| Speilvend | 1 bit |
| Gester av | 1 bit |
| Animer pusten | 0 bit |
| Pust (på/av i spillet) | 1 bit |
| Fothøyde | 8 bit |
| Gulvhøyde | 8 bit |
| Fotomodus (bare lokalt) | 0 bit |
| Synkroniser Fotomodus | 16 bit |
| Behold hodet synlig i Fotomodus | 0 bit |
| Deaktiver VRChats hvilebevegelse | 0 bit |
| Reduser positurforvrengning fra hoderotasjon | 1 bit (deles med Snu deg) |
| Snu deg | 10 bit (1 deles med positurkorrigeringen) |

Snu deg bruker 1 bit til på/av, 8 bit til hodehelling og 1 bit til nakkelås. Hvis positurkorrigeringen allerede er aktiv, øker Snu deg forbruket med 9 bit. Delte bit telles ikke dobbelt, og visningen oppdateres når valg endres.

Summen omfatter positurverktøyene på avataren, ikke hele avataren med klær og andre verktøy. Felles innstillinger telles ikke dobbelt ved flere verktøy. Positurantall over det grunnleggende nummerområdet krever også ekstra valgbiter.

### Fothøyde og gulvhøyde

- «Fothøyde»: justerer fotsidens høyde med hodet på samme sted, for å tilpasse føttene til gulv eller seng.
- «Gulvhøyde»: flytter hele kroppen inkludert hodet opp/ned. 50% er opprinnelig høyde, 0% er 3m ned og 100% er 3m opp. Standard er 50%.

Begge finnes under Innstillinger i spillet, med Gulvhøyde etter Fothøyde. Gulvhøyde flytter opp/ned fra midten, og nær 50% går kroppen tilbake og korrigeringen avsluttes. Det finnes ingen separat svevebryter. Synspunktet korrigeres når sveving starter og mens gulvhøyden betjenes.

Fothøyde dupliserer ikke lenger helkroppsbevegelser for høydejustering av vanlige animasjoner. Det reduserer animasjonsdata, men betyr ikke at hele avataren alltid blir en tredjedel så stor.

### Fotomodus

1. Aktiver «Funksjoner > Menyfunksjoner > Fotomodus».
2. Åpne Fotomodus i VRChat og velg positur med «Velg positur».
3. Flytt opptil 3m frem med «Avstand» og roter med «Retning». Ved avstand over 0 vender avataren mot deg.
4. Aktiver «Lås føtter» ved behov. Føttene tvinges ikke låst ved start, så du kan plassere først. Hvis Gester av er lagt til, kan du også bytte håndform her.
5. «Avslutt foto» nullstiller avstand/retning og frigir føttene.

Bevegelse og rotasjon i Fotomodus bruker 0 bit når bare du ser det. Aktiver «Praktiske funksjoner > Synkroniser Fotomodus» for å vise posisjon og retning til andre. Det bruker 16 bit totalt.

Aktiver «Praktiske funksjoner > Behold hodet synlig i Fotomodus» hvis ditt eget avatarhode forsvinner nær kameraet. Det påvirker bare din visning under fotografering og går tilbake til normalt etterpå.

### Bytt liggende positur ved å hoppe

Med både rygg- og magepositur valgt kan du hoppe på stedet for å bytte. Med bare én valgt beholdes den. Slå av «Bytt liggende positur ved å hoppe» under Unitys «Funksjoner → Praktiske funksjoner» hvis du ikke ønsker hoppbytte. Det bruker 0 ekstra synkroniserte bit. Med flere positurverktøy gjelder deaktivering i ett av dem for hele avataren.

### Snu deg

I støttede liggende positurer snur kroppen seg jevnt når hodet vippes til venstre eller høyre. Alle liggende positurer støttes ikke automatisk.

1. Aktiver «Funksjoner > Menyfunksjoner > Snu deg».
2. Åpne «Detaljer» for ønsket liggende positur under «Positurer», og slå på «Aktiver Snu deg».
3. Angi en sideanimasjon i «Sidepositur for Snu deg». Snuing til begge sider lages fra opprinnelig positur og sideposituren. Juster «Styrke på kontaktkorrigering» ved behov.
4. Velg støttet positur i VRChat og aktiver Snu deg. Frigi nakkelåsen før bruk.

For positurer med Snu deg bestemmer hodehellingen animasjonens avspillingsposisjon, så vanlig MotionTime er utilgjengelig. For ferdige animasjoner for snuing angir du animasjonen i det vanlige feltet og lar «Sidepositur for Snu deg» stå tomt. Posisjon 0 skal være venstre side, 0.5 ryggleie og 1 høyre side.

### Pust og standard hvilebevegelse

«Funksjoner > Praktiske funksjoner > Animer pusten» beveger blant annet brystkassen med pusten. Under «Åpne innstillinger» kan hver kategori bruke Ingen, Svak, Middels, Sterk, Rask eller Egendefinert. Rask gir rask pust. Egendefinert lar deg angi en egen pusteanimasjon.

Bare pustebevegelsen bruker 0 bit. Aktiver også «Menyfunksjoner > Pust» for å stoppe og starte i VRChat. På/av-synkronisering bruker 1 bit. Styrken øker ikke bitforbruket.

Nakke, skuldre og armer følger brystkassen for å begrense bevegelse av hode og hender. Posisjonene er ikke helt låst, så velg styrke etter kroppsbygning og positur.

«Praktiske funksjoner > Deaktiver VRChats hvilebevegelse» demper kroppssvaien VRChat normalt legger til i hvile. Det er av som standard og beholder verktøyets pust og andre egne bevegelser. Det kan brukes uten pust og koster ingen ekstra bit. Det stopper ikke svai som skyldes noe annet enn standard hvileanimasjon.

Med flere positurverktøy gjelder stopp av hvilebevegelse i ett av dem for hele avataren. Slå av i alle verktøy og last opp på nytt for å gjenopprette den.

### Reduser positurforvrengning fra hoderotasjon

Bruk dette hvis posituren forvrenges når nakkelåsen frigis fordi hodet peker annerledes enn i originalposituren. Valget finnes under «Funksjoner > Praktiske funksjoner» uten egen spillmeny. Med låst nakke brukes originalretningen. Ryggleie unntas fra korrigering for å beholde opprinnelig retning oppover.

### Menyikoner

- «Generer positurikoner»: lager bilder av avataren i hver positur til menyen. Det blir lettere å skille dem, men genereringen øker byggetiden.
- «Reduser ikonstørrelsen»: senker oppløsningen for mindre avatarstørrelse.

Positurikoner genereres ikke for Android. Bruk «Ta ikonbilder på nytt» etter endringer av blant annet klær. Listen oppdateres med «Oppdater positurminiatyrer». Les bekreftelsen før disse tidkrevende handlingene.

## Legge til, ordne og forhåndsvise positurer

Velg kategori til venstre under Positurer. Forstørrelsesglassfanen øverst søker etter navn på tvers av kategorier. Detaljer samler animasjon, ikon, MotionTime og andre innstillinger.

Legg til flere animasjoner ved å dra dem til tilleggsfeltet, eller velg dem i Project og trykk «Importer klipp valgt i Project». Duplikater av samme klipp fjernes.

Velg målplassering under «Detaljer > Rekkefølge» for å ordne om. Opp-/nedknappene kan også brukes. Åpne eller lukkede detaljer beholdes under flytting.

Forhåndsvisning viser posituren i Scene uten justeringsvinduet og gjenoppretter originalvisningen etterpå. Denne enkle forhåndsvisningen kjører ikke MA- eller NDMF-bygg, så klær og for eksempel FloorAdjuster innarbeidet ved bygg kan gi annet sluttresultat. Kontroller også endelige klær og bakkekontakt etter avatarbygg.

## Animasjonsjustering og automatisk justering av armer og ben (IK)

Åpne Animasjonsjustering under Positurer for å finjustere kropp og lemmer med forhåndsvisning. Lagre justeringsanimasjonen med vinduets lagringsfunksjon når du er ferdig.

I støttede produkter og positurer tilpasses lemmenes plassering til kroppsbygningen. «Aktiver automatisk justering av armer og ben» styrer helheten, mens «Aktiver IK for denne posituren» styrer enkeltpositurer. Manglende automatisk justering opplyses for aktuelle positurer.

Trykk «Lagre IK i justeringer» for å arbeide videre manuelt fra automatisk justering. Nåværende IK-resultat skrives til en ny justeringsanimasjon, og IK slås av for avatarens aktuelle positur. Originalanimasjonen beholdes. Krever aktiv automatisk justering globalt og for posituren, samt relevante IK-innstillinger.

«Hindre bakoverbøyning av knær» under Avataroppsett styrer knekorrigering. Vinkelen angis under detaljen «Minste knebøyning (grader)». «Aktiver avatarkorrigering» krever støttede korrigeringsdata. Korrigeringer som overlapper automatisk justering kan være uforenlige.

## Opprinnelige positurer, egne bevegelser og forhåndsinnstillinger

Disse settes opp under Avataroppsett.

- «Bruk opprinnelige stå-, huk- og mageleiepositurer»: importerer automatisk avatarens opprinnelige positurer.
- «Bruk egne bevegelser»: åpne importinnstillinger og angi bevegelser separat for stående, huk (Crouching), mageleie (Prone) osv. Vanligvis angis én bevegelse per type.
- «Samordne sporingslåser med andre verktøy»: aktiver hvis låsene ikke fungerer sammen med andre verktøy.

Manuell bevegelsestildeling i importinnstillingene er atskilt fra automatisk import av opprinnelige positurer.

Forhåndsinnstillinger finnes nederst på fanen. Søk etter navn og bytt med «Bruk denne forhåndsinnstillingen». De brukes først og fremst til startoppsett og erstatter gjeldende innstillinger, så lagre egne justeringer før bytte.

Listen laster bare navn og opplysninger om støttede avatarer. Justeringsanimasjoner lastes først når valgt forhåndsinnstilling brukes. Heller ikke første listeopprettelse forhåndslaster alle avatarers animasjoner.

Listen deles i «Kompatibel med avatar», «Brukerens forhåndsinnstillinger» og «Offisielle forhåndsinnstillinger fra Unisaki Studio». «Skjul medfølgende forhåndsinnstillinger» deles i prosjektet. Listen oppdateres ved tillegg, endring og sletting. Trykk «Last inn på nytt» hvis visningen ikke endres.

## Forhåndsbygg og advarsler

Når «Bruk opprinnelige stå-, huk- og mageleiepositurer» er aktiv, registreres Base-bevegelser ved hver generering. Resultatet skrives ikke til manuelle importinnstillinger, så listen forblir uten tildeling ved bare automatisk import. Endret Base og automatisk import på/av gjelder neste generering.

Automatisk import ved opplasting gjelder byggkopien. Opprinnelig sceneobjekt, importinnstillinger og kilde-Controllers og bevegelser endres ikke. Et forhåndsbygg med uendret inndata gjenbrukes.

Manuelle tildelinger prioriteres for typen, med rotasjon, justeringsanimasjon og avspillingsposisjon. Deaktivert «Bruk» eller fjernet tildelt bevegelse stopper automatisk import for typen. «Fjern manuelt valg» fjerner innstillinger og justeringer, så aktiv automatisk import registrerer på nytt ved neste generering. Handlingen kan angres med Undo. Ved flere eldre innstillinger for samme type prioriteres gyldig innstilling valgt i Inspector.

Eldre automatisk opprettede innstillinger slettes ikke automatisk, da de ikke kan skilles fra manuelle og justerte. Bruk «Fjern manuelt valg» bare på typer som skal tilbake til automatisk import. Med flere produkter på avataren påvirker deres manuelle valg også felles originalpositurer.

Forhåndsbygg gjenbruker genererte data for kortere avatarbyggetid. Opprett eller oppdater med «Oppdater forhåndsbygget». Vanlige avatarbygg genererer også nødvendige data, så manuell forhåndsbygging kreves ikke hver gang.

Endrede innstillinger gir oppdateringsbehov under «Kontroller innstillinger» øverst i Inspector og med Hierarchy-merker. Oppdater forhåndsbygg etter positur-/IK-endringer for kortere senere bygg.

«Innstillinger for advarselsvisning» finnes under Kontroller innstillinger. Du kan skjule advarselstyper og Hierarchy-merker. Det løser ikke problemer eller oppdaterer genererte data; feil og nødvendig generering består.

## Merknader om bruk

- Beregnet for trepunktssporing og skrivebord. Positurlås med helkroppssporing støttes ikke.
- Kroppsbygning og tilpasninger kan kreve ekstra justering av bakkekontakt og klær.
- Spesielle Base-lag og andre positurverktøy kan gi uventede resultater.
- Bevegelse/rotasjon i Fotomodus og Gulvhøyde korrigerer verktøypositurer og standard-/egne bevegelser uten valgt positur. Kontroller synspunkt og standardbevegelser erstattet i VRChat i selve klienten.

## Bruksvilkår og støtte

Kildekode og ressurser er ikke åpen kildekode. Bruk krever gyldig produktkjøp og godkjenning av [medfølgende bruksvilkår](../../../%E3%82%86%E3%81%AB%E3%81%95%E3%81%8D%E3%83%9D%E3%83%BC%E3%82%BA%E3%82%B7%E3%82%B9%E3%83%86%E3%83%A0%E5%88%A9%E7%94%A8%E8%A6%8F%E7%B4%84%EF%BC%88%E6%97%A5%E6%9C%AC%E8%AA%9E%E7%89%88%EF%BC%89.pdf).

- [Kawaii Posing-brukerveiledning](https://docs.unisakistudio.jp/kawaiiposing/manual/)
- [Cool Posing-brukerveiledning](https://docs.unisakistudio.jp/coolposing/manual/)
- [Kontakt](https://help.unisakistudio.jp/)

Ved feilrapportering oppgir du produktets og Unisaki Posing Systems versjoner, Unity/VRChat SDK/Modular Avatar/NDMF-versjoner, avatar, trinn for å gjenskape feilen og feilmeldinger fra Console.
