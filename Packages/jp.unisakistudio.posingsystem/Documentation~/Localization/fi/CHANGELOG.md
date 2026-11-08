# Muutosloki

## [3.2.0] - Julkaisematon

### Lisätty

- Inspectorin, muokkaustoimintojen, asennusikkunoiden, vakioasentojen nimien ja luotavan VRChat-valikon näyttö 30 kielellä.
- Inspectorin ja pelivalikon kielivalinnat yhdistetty. Oletuksena käytetään tietokoneen kieltä, ja viimeinen valinta koskee myös saman käyttöjärjestelmäkäyttäjän muita projekteja.
- README, muutosloki ja offline-käyttöohje voidaan avata valitulla kielellä.
- Kaikkien 128 vakioasennon näyttönimet ja toimintojen kuvaukset laadittu olemassa olevien termien mukaisesti.

### Yhteensopivuus

- Avatarin nykyiset asetukset, omat nimet, resurssiviittaukset, parametrien tunnisteet ja lisenssin tallennustapa säilyvät.

## [3.1.2] - 2026-11-09

- Korjattu ongelma, jossa kasvot jäivät ylöspäin vatsamakuulta noustessa, kun ”Vähennä pään kierrosta aiheutuvaa asennon vääristymää” oli käytössä.
- Pään lukitusta voi nyt vaihtaa EX-valikon kohdassa ”Asetukset → Lukitse pää automaattisesti selällään” myös silloin, kun ”Jalkojen kork.” on pois käytöstä. Myös versioissa 3.1.0 ja 3.1.1 kadonnut kohde palautuu, kun avatar kootaan päivityksen jälkeen.
- ”Päivitä esikoonti” toimii nyt myös, kun avatarin pääobjektin valinta on poistettu Unityssa.
- Korjattu ongelma, jossa Assetsiin jäänyt vanha yhteinen asentojen Controller esti asentojen luonnin päivityksen jälkeen. Myös itse muokatun Controllerin asetukset säilyvät.
- Jos työkalun tai sen vanhemman käytöstä poisto estää yhteisen asentojen Animatorin käytön, kohteen nimi näytetään. Voit siirtyä siihen kohdasta ”Tarkista asetukset → Valitse kohde”.
- Kun esikoonti ilmoittaa asetusmuutoksista, voit tarkistaa eron edelliseen kertaan kohdasta ”Lisätiedot → Näytä esikoonnin jälkeen muuttuneet asetukset”.
- Kun skriptivirhe pysäyttää koonnin (PS050), näytetään enintään 5 Unityn havaitsemaa alkuperäistä virhettä. ”Kysy tästä virheestä” avaa tarkistettavan ja muokattavan viestiluonnoksen, jossa ovat Unityn ja tuettujen työkalujen versiot sekä virhenumero.

Jos käytit esikoontia, paina päivityksen jälkeen ”Päivitä esikoonti”, tallenna kohtaus ja kokoa sekä lataa avatar uudelleen.

## [3.1.1] - 2026-11-08

- Käytössä olevat ”Lattian kork.” ja ”Käänny” näkyvät nyt VRChatin EX-valikon Asetuksissa myös, kun ”Peilaa” on pois käytöstä.
- Vanhassa versiossa järjestettyjen, poistettujen tai uudelleennimettyjen asentojen ”Jalat ojossa”, ”Rentona” ja ”Yksi polvi ylös” oikeat kääntymiset säilyvät. Tämä toimii myös täysin Unpackatuille objekteille, jos alkuperäinen asento voidaan tunnistaa. Omat asennot, itse käytöstä poistetut kääntymiset ja vaihdetut sivuttaisliikkeet säilyvät.
- Korjattu ongelma, jossa avatar meni hetkeksi vatsamakuulle sitä VRChatissa valittaessa. Korjattu myös hetkellinen selinmakuu hypätessä, kun vain vatsamakuu oli valittuna. Hypyllä vaihtamisen voi poistaa käytöstä kohdasta ”Toiminnot → Lisätoiminnot → Vaihda makuuasentoa hyppäämällä”. Oletuksena toiminto on käytössä.
- Jos yhteisen asentojen Animator Controllerin viittaus irtoaa päivityksessä ja se voidaan palauttaa, näytetään varoitus ja Controller luodaan uudelleen asetuksista. Usean avatarin jakaman Controllerin viittaukset säilyvät.
- Projektin käännösvirheet pysäyttävät koonnin heti alussa ja näyttävät syyn. Jos skriptivirhe estää viittausten tarkistamisen, luotujen Controllerien automaattinen siivous lykätään.
- Työkalua lisättäessä valitaan virallinen Prefab, vaikka projektissa olisi samanniminen Prefab. Esikoonti jatkuu myös, jos yhteinen Controller eroaa vain rivinvaihdoiltaan.
- Asentotyökalun lisäämien FX-tasojen, kuten lattian korkeuden, Write Defaults mukautetaan avatarin asetuksiin.

Jos käytit esikoontia, paina päivityksen jälkeen kohtauksessa olevan työkalun ”Päivitä esikoonti” -painiketta, tallenna kohtaus ja kokoa sekä lataa avatar uudelleen.

## [3.1.0] - 2026-11-07

- Lisätty tuettujen asentojen raajojen automaattinen säätö (IK) avatarin mittasuhteisiin, avatarikohtainen korjaus ja polvien taakse taittumisen esto.
- Kuvatilassa voi säätää avatarin sijaintia ja suuntaa. Sijainnin ja suunnan synkronoinnin sekä oman pään näkyvyyden kuvauksen aikana voi valita.
- Lattian korkeudella voi siirtää koko kehoa enintään 3m ylös tai alas. Sitä voi käyttää eri tarkoituksiin kuin jalkojen korkeutta.
- Lisätty hengitysliike, oletusarvoisen joutokäyntihuojunnan poisto ja pään suunnan aiheuttaman asennon vääristymän vähentäminen.
- Käytössä olevat toiminnot ja parametrien muistinkulutus näkyvät Inspectorissa. Lisätty myös asentohaku, joukkolisäys, järjestäminen ja kevyt esikatselu Scenessä.
- Nopeutettu seisoma-, kyykky- ja istuma-asentojen vaihtoa työpöytätilassa.
- Parannettu asetusmuutosten jälkeisiä esikoontivaroituksia, Inspectorin vierityksen kuormitusta ja luotujen resurssien siivouksen pitkiä pysähdyksiä.
- Korjattu ongelma, jossa automaattituonti varoitti muuttumattoman esikoonnin päivitystarpeesta ja loi sen uudelleen latauksessa. Tuontitulos kohdistuu luotavaan avatariin eikä muuta alkuperäisen objektin tuontiasetuksia.
- Lisätty pään kallistuksella ohjattu kääntyminen ja sen asentokohtaiset liikeasetukset.
- Parannettu vanhojen esiasetusten kääntymisasetusten säilymistä, omien liikkeiden tuontia ja luotujen resurssien hallintaa.

## [3.1.0-beta.2] - Avoin beetaversio (2026-11-05)

- IK:n sijainnin pituusviitteisiin lisätty kiinteä pituus sekä Headin ja Viewpointin korkeusero (Y) ja syvyysero (Z).
- Inspectorin varoitukset näkyvät myös Hierarchyn objektinimen vasemmalla puolella. Esikoonnin puuttumisen ja asetusmuutosten lisäksi tuetaan liikkeiden tuonnin ja esiasetustoimintojen näyttämiä varoituksia.
- Korjattu ongelma, jossa esimerkiksi kohtauksen avaamisen yhteydessä tehtävä automaattinen siivous latasi muiden kohtausten luotuja Controllereita ja pysäytti Unityn pitkäksi aikaa.
- Korjattu esikoontihistorian katoaminen asentoluetteloa tai toimintoja muutettaessa. Esikoontia ei ilmoiteta suorittamattomaksi, vaan ilmoitetaan asetusten muuttuneen esikoonnin jälkeen. Myös Hierarchy näyttää päivitystarpeen.
- Inspector ja Hierarchy käyttävät yhteisiä esikoontivertailun tuloksia, mikä keventää vieritystä asetusten ja viitattujen resurssien pysyessä samoina. Koonnissa vertailu suoritetaan edelleen joka kerta.

## [3.1.0-beta.1] - Avoin beetaversio (2026-11-04)

- Työpöytätilan tavallinen asennonvaihto muutettu yhden arvioinnin malliin. Se toimii tavallisessa koonnissa ja esikoonnissa. VR ja erityistilanteet käyttävät aiempaa käsittelyä.
- alpha.3:n ja vanhempien versioiden esikoonnit tunnistetaan päivitettäviksi.
- Näkymä säilyy lattian korkeutta kuvauksen aikana muutettaessa. Kuvauksen jälkeen palataan uusimpaan lattian korkeuteen.

- Automaattituonti muutettu tunnistamaan liikkeet vain luonnin yhteydessä. Esikoonti ja koonti eivät enää lisää tai muuta manuaalisia tuontiasetuksia.
- Automaattituonnin kytkentä, Base-tason muutokset ja manuaalisten asetusten tyhjennys vaikuttavat uudelleenluontiin. Usean tuotteen yhteiset manuaaliset valinnat ja säädöt säilyvät, ja vanhat tuodut liikkeet palautetaan oletusasennoiksi.
- Esikoonnin luodut Controllerit hallitaan kohtauksen GUID:n ja avatarin mukaan yhdessä tiedostossa. Kopioitujen objektien, muiden kohtausten ja Prefabien jaetut viittaukset erotetaan ennen päivitystä.
- Koonnissa käytetään NDMF:n väliaikaisten resurssien hallintaa, eikä muokkauskäyttöön tarkoitettuja luotuja tiedostoja lisätä.
- Luodut resurssit tarkistetaan kohtauksen avaamisessa ja tallennuksessa. Ne säilytetään, jos niihin liittyy tallentamattomia muutoksia, Undo/Redo-historiaa tai ulkoisia viittauksia. Vain varmasti omistetut käyttämättömät tiedostot siirretään roskakoriin.
- Tuotujen liikkeiden kierto, säätöanimaatio ja toistokohdan määritys huomioidaan. Kierron, RootT/RootQ:n ja Humanoid-säädön käsittely on yhteinen tavallisten asentojen kanssa.
- Sisäkkäisten BlendTree-rakenteiden säädöt kopioidaan kohdetta varten säilyttäen alkuperäiset liikkeet ja kävelyn sekoitusasetukset.
- ”Vain seisten” -toistokohdan määritys muunnetaan staattiseksi asennoksi alkuperäisellä kuvataajuudella pysäyttämättä kävelyn toistoa.

## [3.1.0-alpha.2] - Suljettu testiversio (2026-10-26)

### Korjaukset ja muutokset

- Kuvatilaa ja lattian korkeuden korjausta sovelletaan myös oletus- ja omiin liikkeisiin silloin, kun työkalun asentoa ei ole valittu.
- Lattian korkeuden alue laajennettu 3m ylös ja alas. 50% on alkuperäinen korkeus ja oletusarvo, 0% on 3m alempana ja 100% on 3m ylempänä.
- Korjattu ongelma, jossa ”Vain seisten” -korvaus muutti tuonnin lähteen BlendTree-rakennetta.
- Asennoissa ilman IK-sääntöä käytetään manuaalisia korjauksia ja tarpeettomat luontivaroitukset estetään.
- Korjattu samannimisten asentojen säätöluonnosten sekoittuminen ja saman resurssin sisäisten liikevaihtojen jääminen huomaamatta.
- Korjattu ongelma, jossa virheellisen alkuasennon numero valitsi toisen asennon alkuasennoksi.
- Ei-tuetun AnimatorOverrideControllerin tuonti näyttää ohjeen poikkeuksen sijaan.
- Vanhoista esiasetuksista säilytetään erikseen käytöstä poistettu Käänny-asetus.
- Toimintokuvakkeiden näkyvyyttä parannettu Editorin vaaleassa teemassa.

Näkymän ja oletusliikkeiden korvaamisen jälkeisen toiminnan tarkistus VRChat-asiakasohjelmassa jatkuu.

## [3.1.0-alpha.1] - Ennen julkaisua

### Lisätty

- Lisätty toimintokohtaiset kytkimet ja ExpressionParameters-kulutuksen näyttö.
- Lisätty Kuvatila avatarin siirtämiseen eteenpäin ja kääntämiseen.
- Lisätty valittava Kuvatilan synkronointi. Vain paikallinen käyttö kuluttaa 0 bit, synkronointi 16 bit.
- Lisätty asetus, jolla oman avatarin pää pidetään näkyvissä Kuvatilassa.
- Lisätty Lattian kork. -toiminto, joka siirtää koko kehoa pää mukaan lukien 3m ylös tai alas. 50% on alkuperäinen korkeus ja oletusarvo, 0% on 3m alempana ja 100% on 3m ylempänä.
- Lisätty pään kallistuksen mukaan tapahtuva kääntyminen ja asentokohtaiset animaatioasetukset.
- Lisätty hengitysliike sekä Heikko-, Keskivahva-, Vahva-, Nopea- ja Oma-asetukset.
- Lisätty hengitysliikkeen ja pelin kytkinvalikon erilliset valinnat. Pelkkä liike kuluttaa 0 bit, kytkimen synkronointi 1 bit.
- Lisätty VRChatin oletusarvoisen joutokäyntihuojunnan poisto.
- Lisätty pään suunnan aiheuttaman asennon vääristymän vähentäminen.
- Lisätty tuettujen asentojen raajojen automaattinen säätö (IK) avatarin mittasuhteisiin sekä yleinen ja asentokohtainen kytkin.
- Lisätty ”Tallenna IK säätöihin” IK-tuloksen tallentamiseksi säätöanimaatioon.
- Lisätty avatarikohtainen asentokorjaus, polvien taakse taittumisen esto ja polven vähimmäistaivutuskulma.
- Lisätty kevyt asennon esikatselu asentoluettelosta Sceneen.
- Lisätty asentoluokkien välilehdet, luokkien yhteinen haku, useiden animaatioiden joukkolisäys ja siirtokohdan järjestysnumeron määritys.
- Lisätty tuotteen ja yhteisen järjestelmän muutoslokin näyttö Inspectorissa.

### Muutettu

- Nopeutettu esiasetusten latausta, mikä lyhentää odotusta työkalua lisättäessä ja esiasetusluetteloa avattaessa.
- Esiasetusten lisäys, muokkaus ja poisto päivittyvät automaattisesti luetteloon.
- Inspector järjestetty 3 välilehteen: Toiminnot, Asennot ja Avatarin asetukset.
- Asetuksiin lisätty kuvaukset ja kuvakkeet sekä yhtenäistetty lisäasetusten avaaminen ja sulkeminen.
- Hengityksen lisätiedot siirretty toimintoasetuksiin ja esiasetukset Avatarin asetusten loppuun.
- Omien liikkeiden yksittäiset valinnat ja avatarin alkuperäisten asentojen automaattinen tuonti erotettu omiksi asetuksikseen.
- Vähennetty jalkojen korkeussäädön luomien koko kehon animaatioiden päällekkäisyyttä.
- Asentokuvien päivitys muutettu vahvistusikkunan avaavaksi painikkeeksi.
- Varoitusten näyttöasetukset koottu Tarkista asetukset -kohdan sisälle.
- Uusien 8bit-Prefabien lisääminen poistettu ja korvattu tavallisen Prefabin toimintovalinnoilla. Olemassa olevat viittaukset säilyvät.
- VRChat Avatars SDK:n vähimmäisversio muutettu versioon 3.10.4.

### Korjattu

- Myös oletus- ja omat liikkeet ilman valittua työkalun asentoa huomioidaan Kuvatilan ja lattian korkeuden korjauksessa.
- Korjattu ”Vain seisten” -korvauksen aiheuttama muutos tuonnin lähteen BlendTree-rakenteeseen.
- Ilman kelvollista IK-sääntöä käytetään manuaalista säätöä ja estetään tarpeettomat IK-luontivaroitukset.
- Korjattu samannimisten asentojen säätöjen sekoittuminen ja saman resurssin sisäisen liikkeen vaihdon jääminen uudelleenluonnissa huomiotta.
- Käytöstä poistetun alkuasennon numeron siirtyminen toiselle asennolle ei enää valitse sitä alkuasennoksi.
- Alkuperäisten asentojen tuonnissa havaittu ei-tuettu AnimatorOverrideController näyttää ohjeen poikkeuksen sijaan.
- Vanhan esiasetuksen käyttö säilyttää erikseen käytöstä poistetun Käänny-asetuksen.
- Inspectorin kuvakkeiden taustaa parannettu, jotta toimintokuvakkeet erottuvat myös Editorin vaaleassa teemassa.

- Korjattu Käänny-, Lattian kork.- ja Kuvatila-kuvakkeiden puuttuminen avatariin lisättäessä.
- Korjattu tuettujen asentojen kääntymisasetusten katoaminen vanhan version esiasetusta käytettäessä.
- Korjattu asennon nimen muokkauksen siirtyminen toiseen asentoon toista lisätietonäkymää avattaessa.
- Korjattu lisätietojen sulkeutuminen asentoja järjestettäessä sekä asetusmuutosten Undo- ja Redo-käsittely.
- Korjattu alkuperäisen avatarin päällekkäinen näkyminen ja sijainnin lisääminen kahdesti esikatselussa.
- Korjattu esikatseluvirhe avatareissa, joista puuttuu komponentteja.
- Korjattu avatarin luiden suunta- ja mittakaavaerojen aiheuttamat raajojen suunnan ja sijainnin IK-poikkeamat.
- Korjattu varoitusten ja esikoontitilan päivittymättömyys heti asetusmuutoksen jälkeen.
- Korjattu vanhan esikoontituloksen uudelleenkäyttö IK-asetusten muuttamisen jälkeen.
- Korjattu joissakin kokoonpanoissa lisätoimintojen luonnin estävä koontivirhe.

## [3.0.16] - 2026-09-14
- NDMF:n vähimmäisriippuvuus päivitetty versioon 1.14.0, koska käytössä on `VirtualAnimatorController.SetParameter`-API

## [3.0.15] - 2026-09-13
- Korjattu kehon sijainnin tai polvien taivutuksen suuri muutos selinmakuun pään lukitusta vaihdettaessa avatareissa, joissa käytetään Hips-mittakaavan muutosta ja FloorAdjusteria

## [3.0.14] - 2026-09-09
- Korjattu Eleet pois -valikkokohteen katoaminen toisesta ja myöhemmistä asentotyökaluista samassa avatarissa
- Korjattu USSPS_Locomotion-tason palaaminen Standingiin esimerkiksi SleepDown-asennosta huolimatta
- Korjattu FloorAdjusterin kanssa esiintyvät asennon korkeuden ja X/Z-sijainnin poikkeamat sekä tuettu MA- ja AAO-versioiden suoritusjärjestys
- Parannettu palautusta: Generated-kansiosta poistetun esikootun AnimatorControllerin tilalle palautetaan automaattisesti oletusarvoinen PosingSystem_Locomotion
- Korjattu koontivirhe, kun kaikki seisoma-asennot on poistettu
- Esiasetusluettelo järjestetään avatarin nimen mukaan

## [3.0.13] - 2026-09-04
- Korjattu VR-tilassa heti kyykistymisen jälkeen seisomaan palatessa syntyvä seisoma-asennon siirtymä itseensä, joka teki palautumisesta epävakaan

## [3.0.12] - 2026-09-04
- Korjattu esikoonnittoman asentotyökalun virheellinen tunnistus muuttuneeksi NDMF-koonnissa
- Korjattu koonnin epäonnistuminen `AddAssetToSameFile`-virheeseen, kun luotuja resursseja lisättiin NDMF-koonnin väliaikaiseen AnimatorControlleriin

## [3.0.11] - 2026-08-31
- Korjattu esikoontiavatarin Scenen muuttuminen virheelliseksi ja käsittelyn epäonnistuminen NDMF 1.14.7:ssä ja uudemmissa
- Korjattu `MissingReferenceException`, joka syntyi NDMF-virhenäkymän viitatessa jo tuhottuun LocalizationAssetiin

## [3.0.10] - 2026-08-29
- Esiasetusten varsinaiset resurssit ladataan vasta esiasetusvalitsinta avattaessa. Välimuistia käytetään uudelleen, jos GUID-luettelo ei ole muuttunut
- Hierarchyn varoitus- ja virhekuvakkeet arvioidaan uudelleen vain muutoksissa, ja tuotevalikon resurssihaku käyttää yhteistä välimuistia. Tämä vähentää Editorin jäätymistä muutaman sekunnin välein kohtauksissa, joissa on paljon asentotyökaluja
- Korjattu `ArgumentException`, joka syntyi päällekkäisiä parametriarvoja rekisteröitäessä, kun samassa avatarissa oli useita asentotyökaluja esikoontia tehtäessä

## [3.0.9] - 2026-08-26
- Jos VRCAvatarDescriptoria ei saada PosingSystemistä muunnoksen aikana, estetään NullReferenceException ja ohjataan tarvittavan koontilokin tarkasteluun
- Korjattu `Avatar is null` -virhe, kun myöhempi esikoonti tai koonti käytti esikatselua, joka viittasi NDMF:n luomaan ja poistamaan väliaikaiseen Avatariin. Muunnos erotettu esikatseluvälimuistista, ja eri esikatselujen NDMF-väliaikaisresurssien nimiristiriidat estetty
- Korjattu 可愛いポーズ (Kawaii Posing) -Prefabin esikatseluviittausten tallentumattomuus kohtaukseen, joka kasvatti piilotettujen esikatseluavatarien määrää. Esikatselut on rajattu kohtauksen tallennuksen ulkopuolelle, ja vanhat esikatselut kerätään kohtauksen latauksessa, Play-tilaan siirryttäessä ja skriptien uudelleenlatauksessa

## [3.0.8] - 2026-08-18
- Korjattu Unity Assertion ja mahdollinen asentodatan vioittuminen esikootuissa asentoliikkeissä, kun FloorAdjuster tai vastaava korjaa korkeutta uudelleen
- Parannettu korkeusmittauksen turvallisuutta: se ei häiritse alkuperäistä avataria tai muiden työkalujen AnimationMode-tilaa

## [3.0.7] - 2026-08-12
- Korjattu näkymän sijainnin poikkeama ja polvien poikkeava taipuminen selinmakuulla käytettäessä FloorAdjusteria tai muita korkeussäätötyökaluja
- Korjattu virhe, kun PosingSystem on avatarin ulkopuolella (kiitos PancakeTorto1se)
- Korjattu virhe, joka syntyi käsittelyn jatkuessa avatarin latauksen epäonnistumisen jälkeen
- Korjattu joidenkin asetusten virheellinen tunnistus muuttuneiksi heti esikoonnin jälkeen
- Parannettu liikkumisanimaatioiden automaattituontia
- Lisätty varoitus joillekin erityisiä rigejä käyttäville avatareille
- Animaatiosäätöikkunaan lisätty muutosvahvistus ja säätöarvojen peilaus (kiitos kraihd)
- Lisätty ohjeita muiden työkalujen virhetilanteisiin ja tarkistuksia virhealttiisiin kohtiin

## [3.0.6] - 2026-03-17
- Korjattu asentojen voimakas vääristyminen joissakin avatareissa, joissa on ennestään oma asennonhallinta

## [3.0.5] - 2026-01-31
- Korjattu joidenkin ExpressionMenus-valikoiden aiheuttama Unityn kaatuminen
- Korjattu avatarin ongelmat ごろ寝EX-yhteensopivuus-Prefabia käytettäessä (kiitos あずき)
- Korjattu kierron virheet säätöanimaatioita yhdistettäessä

## [3.0.4] - 2026-01-11
- Parannettu osittain avatarin toimintaa Prefabs-kansioon siirrettäessä
- Lisätty lisenssin poistotoiminto
- Korjattu useiden asentojen poikkeava kaulan asento
- Nopeutettu Inspectorin avaamista hieman
- Kuvakkeet luodaan nyt myös esikoonnissa. Avatarin päivityspainikkeen tekstiä muutettu
- Varoitusta ei näytetä, jos esikoontia ei ole koskaan tehty (koska moni ei näytä käyttävän esikoontia)
- Poistettu tarpeettomia resursseja
- Korjattu sormien säätämisen estävä ongelma

## [3.0.3] - 2026-01-04
- Korjattu eri esiasetuksen käyttö kuin valittu esiasetus (kiitos あずき)

## [3.0.2] - 2026-01-02
- Lisätty Unityn tumman teeman tuki
- Korjattu asennon vääristyminen kierrossa animaatiosäädön aikana
- Muutettu luotujen valikkokuvakkeiden kokoa, otettu pakkaus käyttöön ja lisätty vielä pienempien kuvakkeiden asetus
- Palautettu versiossa 3.0.0 poistettu seurantalukitusten yhdistämistoiminto ja avatarin liikkeiden automaattituonti

## [3.0.1] - 2025-12-29
- Korjattu oletusliikkeiden asetustoiminnon ongelmat
- Korjattu avatarin asennon muuttuminen itsestään virheelliseksi

## [3.0.0] - 2025-12-28
- Lisätty esikoonti
- Lisätty animaatiosäätö
- Lisätty esiasetukset

## [2.2.5] - 2025-05-08
- Ratkaistu yhteensopivuusongelmia joidenkin avatarin toimintojen kanssa

## [2.2.4] - 2025-04-04
- Korjattu asentojen vaihtumisen estävä ongelma joillakin avatarin ja ModularAvatar-version yhdistelmillä

## [2.2.3] - 2025-03-13
- Korjattu joidenkin avatarien, kuten 森羅-hahmon hypyn, virheelliset liikkeet
- Korjattu ajoittain väärä Eleet pois -oletusarvo

## [2.2.2] - 2025-03-06
- Korjattu tietyissä ympäristöissä ilmenevä virhe

## [2.2.1] - 2025-02-05
- Korjattu Unityn kaatuminen
- Lisätty versio NDMF Consolessa näkyvään työkalun nimeen

## [2.2.0] - 2025-02-02
- Lisätty asetus eleiden poistamiseen ja animaation kädenmuotojen käyttämiseen

## [2.1.6] - 2025-02-02
- Korjattu asentojen rikkoutuminen tiettyjen työkalujen kanssa
- Korjattu asetuksista riippuva virhe

## [2.1.5] - 2024-12-10
- Ratkaistu ristiriitoja joidenkin avatarin toimintojen kanssa
- Lisätty painike Unitypackage-version poistamiseen

## [2.1.4] - 2024-12-08
- Korjattu avatarista riippuva virhe
- Korjattu jalkojen lukituksen estyminen avatarin tai muiden toimintojen ristiriidassa
- Korjattu avatarin asennon palautuminen VRC:n oletusasentoon työpöytätilassa

## [2.1.3] - 2024-12-01
- Korjattu asennon vaihtumisen estyminen käytettäessä avataria tai toimintoa, joka käyttää vääräntyyppistä VRMode-parametria
- Lisätty ilmoitus NDMF-Consoleen toimintojen parametrityyppien välisistä ristiriidoista

## [2.1.2] - 2024-12-01
- NDMF-Consoleen lisätty selitys tilanteeseen, jossa toisen työkalun virhe aiheuttaa avatarin koonnissa virheen Unisaki Posing Systemiin
- Korjattu joidenkin avatarien koontivirhe
- Parannettu toimintaa heti avatarin lataamisen jälkeen
- Avatarin alkuperäinen liikkumiskäsittely poistetaan nyt, koska se esti joidenkin avatarien toiminnan

## [2.1.1] - 2024-11-20
- Korjattu PosingOverriden MergeAnimatoriin liittyvä ongelma

## [2.1.0] - 2024-11-20
- Korjattu jalkojen tai lantion uppoaminen maahan työpöytätilassa, jotta kaikki asennot näkyvät oikein
- Lisätty mahdollisuus vaihtaa asentoa pään korkeudesta riippumatta
- Lisätty kuvakkeiden luonnin poistava tila Quest-avatarien latausta varten
- Lisätty useita varoituksia avatarin ongelmista
- Ratkaistu lukitustoimintojen ristiriidat esimerkiksi AFK:n ja muiden työkalujen käsien lukitusten kanssa
- Lisätty automaattinen pään lukitus selinmakuulla

## [2.0.0] - 2024-11-12
- Julkaistu VPM/VCC-tuettu versio
- Muutettu virhetilanteessa näytettävää työkalun nimeä #1
- Lisätty käyttöehdot #5
