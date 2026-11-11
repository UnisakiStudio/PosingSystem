# Unisaki Posing System

Kawaii Posing Toolin, Cool Posing Toolin ja muiden tuotteiden yhteinen asentojärjestelmä. Se luo avatarin latauksessa tarvittavat valikot ja animaatiot Modular Avatarin ja NDMF:n avulla.

Tämä README koskee kehitteillä olevaa versiota v3.2.0. Virallista julkaisupäivää ei ole päätetty. Asennetun version näet asentotyökalun Inspectorin yläosasta. Katso myös [uudet toiminnot ja muutosloki](CHANGELOG.md).

## Näyttökieli ja offline-ohjeet

Valitse näyttökieli Inspectorin yläosasta kohdasta ”Language / 言語”. Sama valinta koskee Inspectoria ja luotavaa VRChat-valikkoa. Lataa avatar uudelleen palveluun, jotta pelissä näkyvä kieli vaihtuu.

Jos kieltä ei ole valittu, käytetään tietokoneen kieltä tai englantia, jos kieltä ei tueta. Windows, macOS ja Linux ovat tuettuja. Viimeksi valittu kieli tallennetaan EditorPrefsiin, ja sitä käytetään myös tämän tietokoneen saman käyttöjärjestelmäkäyttäjän muissa projekteissa. Kaikki poseeraustyökalut ja avatarit käyttävät samaa kieltä. Kielen vaihtaminen minkä tahansa työkalun Inspectorissa päivittää yhteisen asetuksen; avatarikohtaisia kielivalintoja ei käytetä. Valintaa ei synkronoida muihin tietokoneisiin. Lisenssin tallennustapa ei muutu. Kielipainike näkyy vain ensimmäisessä asennusvahvistuksessa, jos kieltä ei ole tallennettu. Myöhemmät kielenvaihdot tehdään Inspectorissa.

README-tiedoston, muutoslokin ja käyttöohjeen voit valita mukana toimitetusta [kieliluettelosta](../index.html). Myös Inspectorin käyttöohje ja muutosloki noudattavat valittua kieltä. Alkuperäiset käyttöehdot ja viitteelliset käännökset toimitetaan edelleen erikseen.

## Asennon vaihtaminen työpöytätilassa

Työpöytätilassa seisoma-, kyykky- ja istuma-asentojen vaihdon viivettä on lyhennetty, jotta valittu asento tulee voimaan nopeammin. Muutos toimii tavallisessa koonnissa ja esikoonnissa ilman erillistä komentoa tai lisäasetuksia. VR-tilan ja erityistilanteiden aiempi toiminta säilyy. Päivitä vanhalla versiolla tehty esikoonti ennen avatarin lataamista.

## Asennus ja päivittäminen

Lisää asentotyökalu VCC:n tai ALCOMin kautta tuotteen ostotiedostojen ja asennusohjeiden mukaisesti. Tämä järjestelmä asennetaan asentotyökalun riippuvuutena.

Version v3.2.0 vaatimukset:

- Unity 2022.3:lla tehty VRChat-avatarprojekti
- VRChat Avatars SDK 3.10.4 tai uudempi
- Modular Avatar 1.x, vähintään 1.15.1
- NDMF (Non-Destructive Modular Framework) 1.x, vähintään 1.14.0

Jos käytät Kawaii Posing Toolia, päivitä se versioon 3.2.0 ALCOMissa / VCC:ssä ja varmista, että myös Unisaki Posing System on päivittynyt versioon 3.2.0. Jos käytit esikoontia, paina ”Päivitä esikoonti”, tallenna kohtaus ja lataa sitten avatar palveluun.

Cool Posing Tool 1.2.0 käyttää yhteistä järjestelmää 3.2.0. Päivitä sekä tuote että yhteinen järjestelmä yhteensopiviin versioihin.

### Aiemman 8bit-version käyttäjille

Uudet asennukset käyttävät nyt tavallista Prefabia. Voit vähentää muistinkulutusta poistamalla tarpeettomia toimintoja Toiminnot-välilehdessä. Pelkkä asennon valinta kuluttaa perustilanteessa 8 bit.

Vanha 8bit-Prefab säilyy kunkin tuotteen ”Editor/Legacy”-kansiossa, jotta olemassa olevien avatarien ja esiasetusten viittaukset toimivat. Vanhaa 8bit-versiota käyttäessä Prefabia ei tarvitse poistaa tai korvata. 8bit-version valinta Unityn uuden asennuksen valikosta on poistettu.

## Asetusnäkymä

Valitse Hierarchysta avatarin sisältä esimerkiksi ”可愛いポーズ (Kawaii Posing)” tai ”カッコいいポーズ (Cool Posing)” muokataksesi asetuksia.

| Välilehti | Asetukset |
| --- | --- |
| Toiminnot | Pelivalikon kohteet, lisätoiminnot, kuvakkeet ja ExpressionParameters-muistinkulutus |
| Asennot | Asentojen valinta, lisääminen ja järjestäminen, esikatselu Scenessä, asentokohtaiset tiedot ja animaatioiden muokkaus |
| Avatarin asetukset | Vartalon mittasuhteiden ja polvien korjaukset, alkuperäisten asentojen ja omien liikkeiden tuonti sekä esiasetukset |

Jos toiminnolla on lisäasetuksia, avaa sen alapuolelle asetuskentät avaamalla asetukset. Myös hengityksen asetukset löytyvät Toiminnot-välilehdestä.

Yläosan ohje- ja muutoslokkipaneelista voit avata käyttöohjeet, tuen ja muutoslokin. Voit vaihtaa tuotteen ja Unisaki Posing Systemin muutoslokien välillä.

## Toiminnot ja ExpressionParameters-muistinkulutus

Toiminnot-välilehden Valikkotoiminnot-kohdassa valitut toiminnot näkyvät pelivalikossa. Esimerkiksi ”Peilaa” lisää peliin peilauksen kytkimen. Sen valitseminen Inspectorissa ei tarkoita, että kaikki asennot olisivat aina peilattuja.

| Toiminto | Lisäkulutus bitteinä |
| --- | ---: |
| Peilaa | 1 bit |
| Eleet pois | 1 bit |
| Animoi hengitys | 0 bit |
| Hengitys (kytkin pelissä) | 1 bit |
| Jalkojen kork. | 8 bit |
| Lattian kork. | 8 bit |
| Kuvatila (vain omalla näytöllä) | 0 bit |
| Synkronoi Kuvatila | 16 bit |
| Pidä pää näkyvissä Kuvatilassa | 0 bit |
| Poista VRChatin oletushuojunta | 0 bit |
| Vähennä pään kierrosta aiheutuvaa asennon vääristymää | 1 bit (yhteinen Käänny-toiminnon kanssa) |
| Käänny | 10 bit (joista 1 bit yhteinen vääristymän vähentämisen kanssa) |

Käänny-toiminto käyttää kytkimeen 1 bit, pään kallistukseen 8 bit ja pään lukitukseen 1 bit. Jos asennon vääristymän vähentäminen on jo käytössä, Käänny-toiminnon lisääminen kuluttaa 9 bit lisää. Yhteistä osuutta ei lasketa kahdesti, ja näyttö päivittyy valintoja muutettaessa.

Näytettävä yhteismäärä koskee tähän avatariin asennettuja asentotyökaluja. Se ei ole koko avatarin, asujen ja muiden toimintojen yhteiskulutus. Myös usean asentotyökalun yhteiset asetukset lasketaan vain kerran. Jos asentojen määrä ylittää perusnumeroinnin alueen, myös asennon valinta vaatii lisäbittejä.

### Jalkojen korkeus ja lattian korkeus

- ”Jalkojen kork.”: säätää jalkojen puolen korkeutta pitäen pään paikallaan. Käytä jalkojen kohdistamiseen lattiaan tai sänkyyn.
- ”Lattian kork.”: siirtää koko kehoa pää mukaan lukien pystysuunnassa. 50% on alkuperäinen korkeus, 0% on 3m alempana ja 100% on 3m ylempänä. Oletusarvo on 50%.

Molemmat löytyvät pelin Asetukset-valikosta, jossa ”Lattian kork.” seuraa kohtaa ”Jalkojen kork.”. Lattian korkeuden siirtäminen keskeltä liikuttaa kehoa ylös tai alas. Paluu lähelle 50% palauttaa alkuperäisen korkeuden ja lopettaa korjauksen. Erillistä leijumisen kytkintä ei ole. Näkymän korjaus tehdään leijumisen alkaessa ja lattian korkeutta säädettäessä.

Tavallisissa animaatioissa jalkojen korkeuden säätö on muutettu siten, ettei koko kehon liikettä luoda päällekkäin korkeussäätöä varten. Tämä vähentää luotujen animaatioiden datamäärää, mutta ei tarkoita, että koko avatarin koko pienenisi aina 1/3-osaan.

### Kuvatila

1. Ota käyttöön ”Toiminnot > Valikkotoiminnot > Kuvatila”.
2. Avaa VRChatissa Kuvatila ja valitse asento kohdasta ”Valitse asento”.
3. Siirry Etäisyys-säätimellä enintään 3m eteenpäin ja käännä Suunta-säätimellä. Kun etäisyys on yli 0, avatar kääntyy sinua kohti.
4. Ota tarvittaessa ”Lukitse jalat” käyttöön. Jalkoja ei lukita automaattisesti alussa, joten voit säätää sijaintia ennen lukitusta. Jos ”Eleet pois” on lisätty valikkoon, voit vaihtaa myös käden muotoa samassa näkymässä.
5. Valitse ”Lopeta kuvaus” palauttaaksesi etäisyyden ja suunnan sekä vapauttaaksesi jalkojen lukituksen.

Kuvatilan liike ja kääntö kuluttavat 0 bit, kun niitä käytetään vain omalla näytöllä. Näyttääksesi sijainnin ja suunnan muille ota käyttöön ”Lisätoiminnot > Synkronoi Kuvatila”. Etäisyys ja suunta kuluttavat yhteensä 16 bit.

Jos lähellä olevan oman avatarisi pää katoaa näkyvistä, ota käyttöön ”Lisätoiminnot > Pidä pää näkyvissä Kuvatilassa”. Asetus koskee vain omaa näyttöäsi kuvauksen aikana. Lopettaminen palauttaa tavallisen näkyvyyden.

### Makuuasennon vaihtaminen hyppäämällä

Kun sekä selin- että vatsamakuulle on valittu asento, voit vaihtaa niiden välillä hyppäämällä paikallasi. Jos valittuna on vain toinen, kyseinen asento säilyy. Jos et halua vaihtaa hyppäämällä, poista Unityssa ”Toiminnot → Lisätoiminnot” -kohdasta ”Vaihda makuuasentoa hyppäämällä” käytöstä. Lisäkulutus synkronoituihin parametreihin on 0 bit. Jos avatarissa on useita asentotyökaluja, yhdenkin työkalun pois käytöstä oleva valinta koskee koko avataria.

### Käänny

Tuetuissa makuuasennoissa keho kääntyy pehmeästi, kun kallistat päätä sivulle. Toiminto ei automaattisesti tue kaikkia makuuasentoja.

1. Ota käyttöön ”Toiminnot > Valikkotoiminnot > Käänny”.
2. Avaa haluamasi makuuasennon tiedot Asennot-välilehdessä ja ota ”Ota Käänny käyttöön” käyttöön.
3. Määritä sivuttainen animaatio kohtaan ”Käänny-toiminnon sivuasento”. Käännöt vasemmalle ja oikealle luodaan alkuperäisen ja sivuttaisen asennon avulla. Säädä tarvittaessa Kosketuskorjauksen voimakkuutta.
4. Valitse VRChatissa tuettu asento ja ota Käänny käyttöön. Vapauta pään lukitus ennen käyttöä.

Käänny-toimintoa käyttävissä asennoissa pään kallistus määrittää animaation toistokohdan, joten tavallista MotionTime-asetusta ei voi käyttää. Jos käytät valmista kääntymisanimaatiota, määritä se tavalliseen animaatiokenttään ja jätä Käänny-toiminnon sivuasento tyhjäksi. Valmistele animaatio, jossa toistokohta 0 on vasemmalla, 0.5 on selinmakuu ja 1 on oikealla.

### Hengitys ja oletushuojunta

”Toiminnot > Lisätoiminnot > Animoi hengitys” saa esimerkiksi rinnan liikkumaan hengityksen tahdissa. Avaa asetukset ja valitse kullekin asentoluokalle ”Ei mitään / Heikko / Keskivahva / Vahva / Nopea / Oma”. Nopea tarkoittaa nopeatahtista hengitystä. Oma-valinnassa voit määrittää itse tekemäsi hengitysanimaation.

Pelkkä hengitysliike kuluttaa 0 bit. Jos haluat pysäyttää ja jatkaa hengitystä VRChatissa, ota myös ”Valikkotoiminnot > Hengitys” käyttöön. Kytkimen synkronointi kuluttaa 1 bit. Voimakkuuden muuttaminen ei lisää bittimäärää.

Kaula, hartiat ja kädet liikkuvat rinnan mukana pään ja käsien siirtymisen vähentämiseksi. Sijaintia ei lukita täysin, joten valitse voimakkuus kehon ja asennon mukaan.

”Lisätoiminnot > Poista VRChatin oletushuojunta” vähentää VRChatin oletusarvoista kehon huojuntaa paikallaan. Oletuksena se on pois käytöstä, ja työkalun oma hengitys sekä muut mukautetut liikkeet säilyvät. Sen voi valita myös ilman hengitysliikettä, eikä bittimäärä kasva. Asetus ei pysäytä muusta kuin oletusarvoisesta joutokäynnistä johtuvaa huojuntaa.

Jos käytät useita asentotyökaluja, oletushuojunnan poisto missä tahansa niistä koskee koko avataria. Palauta huojunta poistamalla asetus käytöstä kaikissa työkaluissa ja lataamalla avatar uudelleen palveluun.

### Vähennä pään kierrosta aiheutuvaa asennon vääristymää

Käytä tätä, jos alkuperäisestä asennosta poikkeava pään suunta vääristää asentoa, kun pään lukitus vapautetaan. Asetus on kohdassa ”Toiminnot > Lisätoiminnot”, eikä se lisää omaa pelivalikkoa. Pään ollessa lukittuna käytetään alkuperäisen asennon suuntaa. Selinmakuu on rajattu korjauksen ulkopuolelle, jotta alkuperäinen ylöspäin katsominen säilyy.

### Valikkokuvakkeet

- ”Luo asentokuvakkeet”: luo avataria kussakin asennossa esittävät kuvat valikkoon. Asentojen tunnistaminen helpottuu, mutta kuvien luominen pidentää koontiaikaa.
- ”Pienennä kuvakkeita”: pienentää kuvien tarkkuutta avatarin koon vähentämiseksi.

Android-koonnissa asentokuvakkeita ei luoda. Käytä asun tai muun ulkoasun muuttamisen jälkeen toimintoa ”Ota kuvakkeet uudelleen”. Asentoluettelon kuvat päivitetään toiminnolla ”Päivitä asentojen pikkukuvat”. Toiminto vie aikaa, joten lue vahvistusikkuna ennen suorittamista.

## Asentojen lisääminen, järjestäminen ja esikatselu

Valitse asentoluokka Asennot-välilehden vasemmalta puolelta. Ylin suurennuslasivälilehti hakee asentoja nimellä kaikista luokista. Lisätiedot-kohdassa voit määrittää animaation, kuvakkeen, MotionTime-asetuksen ja muut tiedot yhdessä.

Voit lisätä useita animaatioita kerralla vetämällä ne lisäyskenttään tai valitsemalla ne Projectista ja painamalla valittujen leikkeiden tuontipainiketta. Saman leikkeen kaksoiskappaleet ohitetaan.

Vaihda järjestystä valitsemalla kohde Lisätiedot-kohdan Järjestys-valikosta. Voit käyttää myös ylös- ja alas-painikkeita. Tietojen avaus- tai sulkutila säilyy siirrossa.

Asentokohtainen Esikatselu näyttää asennon Scenessä avaamatta säätöikkunaa. Esikatselun lopettaminen palauttaa alkuperäisen näkymän. Tämä kevyt esikatselu ei suorita MA:n tai NDMF:n koontia, joten esimerkiksi koonnissa lisättävä asu tai FloorAdjuster voi antaa erilaisen lopputuloksen. Tarkista lopullinen asu ja lattiakosketus myös avatarin koonnin jälkeen.

## Animaatiosäätö ja raajojen automaattinen säätö (IK)

Avaa Asennot-välilehden Animaatiosäätö säätääksesi kehoa ja raajoja tarkasti esikatselussa. Tallenna säätöanimaatio säätämisen jälkeen ikkunan tallennustoiminnolla.

Tuetuissa tuotteissa ja asennoissa automaattinen säätö mukauttaa raajojen sijainnin avatarin mittasuhteisiin. Animaatiosäätöikkunan ”Ota raajojen automaattinen säätö käyttöön” kytkee koko toiminnon, ja ”Ota IK käyttöön tälle asennolle” kytkee sen asentokohtaisesti. Jos asennolle ei ole automaattista säätöä, siitä ilmoitetaan.

Jos haluat viimeistellä automaattisen säädön käsin, paina ”Tallenna IK säätöihin”. Toiminto kirjoittaa senhetkisen IK-tuloksen uuteen säätöanimaatioon ja poistaa kyseisen avatarin asennon IK:n käytöstä. Alkuperäinen animaatio säilyy. Painiketta voi käyttää, kun sekä yleinen että asentokohtainen automaattisäätö ovat käytössä ja sovellettavat IK-asetukset löytyvät.

Avatarin asetusten ”Estä polvien taittuminen taakse” kytkee polvikorjauksen. Lisätietojen kohdassa ”Polven vähimmäistaivutus (asteina)” voit määrittää taivutuskulman. ”Ota avatarikorjaus käyttöön” on käytettävissä, jos sopivat korjaustiedot löytyvät. Päällekkäisiä korjauksia ei aina voi käyttää yhdessä raajojen automaattisäädön kanssa.

## Alkuperäiset asennot, omat liikkeet ja esiasetukset

Nämä asetukset löytyvät Avatarin asetukset -välilehdestä.

- ”Käytä alkuperäisiä seisoma-, kyykky- ja vatsa-asentoja”: tuo avatarin alkuperäiset asennot automaattisesti.
- ”Käytä omia liikkeitä”: avaa tuontiasetukset ja määritä erikseen esimerkiksi seisomisen, kyykyn (Crouching) ja vatsamakuun (Prone) liikkeet. Yleensä kullekin tyypille määritetään yksi liike.
- ”Yhdistä seurantalukitukset muihin työkaluihin”: ota käyttöön, jos lukitustoiminnot eivät toimi oikein muiden avatarin toimintojen kanssa.

Omien liikkeiden määrittäminen tuontiasetuksissa on eri toiminto kuin alkuperäisten asentojen automaattinen tuonti.

Esiasetukset ovat välilehden lopussa. Hae nimellä ja vaihda asentoasetukset painamalla ”Käytä tätä esiasetusta”. Ne on tarkoitettu pääasiassa alkuasetuksiin ja korvaavat nykyiset asetukset. Tallenna tekemäsi säädöt ennen vaihtamista.

Esiasetusluetteloa näytettäessä ladataan vain nimet ja yhteensopivien avatarien tiedot. Säätöanimaatiot ladataan vasta valittua esiasetusta käytettäessä. Kaikkien avatarien animaatioita ei ladata ennakkoon edes luetteloa ensimmäistä kertaa luotaessa.

Luettelo jakautuu ryhmiin ”Yhteensopiva avatarin kanssa”, ”Käyttäjän esiasetukset” ja ”Unisaki Studion viralliset esiasetukset”. ”Piilota mukana tulevat esiasetukset” on saman projektin yhteinen näyttöasetus. Esiasetuksen lisääminen, muokkaaminen tai poistaminen päivittää luettelon. Jos näkymä ei muutu, paina ”Lataa uudelleen”.

## Esikoonti ja varoitukset

Kun ”Käytä alkuperäisiä seisoma-, kyykky- ja vatsa-asentoja” on käytössä, liikkeet tunnistetaan avatarin Base-tasosta jokaisella luontikerralla. Tunnistettuja tuloksia ei kirjoiteta manuaalisiin tuontiasetuksiin, joten pelkkää automaattituontia käytettäessä luettelo jää tyhjäksi. Base-liikkeiden muutokset ja automaattituonnin kytkentä tulevat voimaan seuraavassa luonnissa.

Latauksen aikainen automaattituonti kohdistuu koontikopioon. Se ei muuta kohtauksen alkuperäistä objektia, tuontiasetuksia eikä lähteen Controlleria tai liikkeitä. Jos esikoonti on tehty eikä syötteitä ole muutettu, tulosta käytetään uudelleen.

Manuaalinen määritys ohittaa automaattituonnin kyseiselle tyypille, ja myös kierto, säätöanimaatio ja toistokohdan määritys huomioidaan. ”Käytä”-valinnan poistaminen tai määritetyn liikkeen irrottaminen pysäyttää tyypin automaattituonnin. ”Tyhjennä manuaalinen valinta” poistaa tyypin asetukset ja säädöt, jolloin käytössä oleva automaattituonti tunnistaa ne uudelleen seuraavassa luonnissa. Toiminnon voi perua Undolla. Jos samalle tyypille on useita vanhoja asetuksia, Inspectorissa valittu kelvollinen asetus on etusijalla.

Vanhan automaattituonnin luetteloon lisäämiä asetuksia ei voida erottaa käsin rekisteröidyistä tai säädetyistä asetuksista, joten niitä ei poisteta automaattisesti. Käytä toimintoa ”Tyhjennä manuaalinen valinta” vain tyypeille, jotka haluat palauttaa automaattituontiin. Jos samassa avatarissa on useita tuotteita, myös muiden tuotteiden manuaaliset asetukset vaikuttavat yhteisiin alkuperäisiin asentoihin.

Esikoonti lyhentää avatarin koontiaikaa käyttämällä luotua dataa uudelleen. Voit luoda tai päivittää sen toiminnolla ”Päivitä esikoonti”. Myös tavallinen avatarin koonti luo tarvittavan datan, joten esikoontia ei tarvitse tehdä käsin joka kerta.

Asetusten muuttaminen edellyttää päivitystä. Tästä ilmoitetaan Inspectorin yläosan ”Tarkista asetukset” -kohdassa ja Hierarchyn merkillä. Asento- tai IK-asetusten muuttamisen jälkeen esikoonnin päivittäminen voi lyhentää seuraavien koontien kestoa.

”Varoitusten näyttöasetukset” on ”Tarkista asetukset” -kehyksen sisällä. Voit piilottaa yksittäisiä varoitustyyppejä tai Hierarchyn merkin. Piilottaminen ei ratkaise asetusongelmia eikä päivitä luotua dataa. Virheet ja tarvittava datan luonti käsitellään edelleen.

## Huomioita käytöstä

- Tarkoitettu 3 pisteen seurantaan ja työpöytäkäyttöön. Asennon lukitusta kokovartaloseurannassa ei tueta.
- Avatarin mittasuhteet ja muutokset voivat edellyttää raajojen kosketuksen tai asun lisäsäätöjä.
- Erityiset Base-tasot tai muut asentoa muuttavat toiminnot voivat aiheuttaa odottamattoman lopputuloksen.
- Kuvatilan liike, kierto ja lattian korkeus korjaavat työkalun asentoja sekä oletus- ja omia liikkeitä silloin, kun asentoa ei ole valittu. Tarkista VRChatissa korvattujen oletusliikkeiden ja näkymän toiminta käytännössä.

## Käyttöehdot ja tuki

Tämän repositorion lähdekoodi ja resurssit eivät ole avointa lähdekoodia. Käyttö edellyttää tuotteen laillista ostamista ja [mukana toimitettujen käyttöehtojen](../../../%E3%82%86%E3%81%AB%E3%81%95%E3%81%8D%E3%83%9D%E3%83%BC%E3%82%BA%E3%82%B7%E3%82%B9%E3%83%86%E3%83%A0%E5%88%A9%E7%94%A8%E8%A6%8F%E7%B4%84%EF%BC%88%E6%97%A5%E6%9C%AC%E8%AA%9E%E7%89%88%EF%BC%89.pdf) hyväksymistä.

- [Kawaii Posing Toolin käyttöohje](https://docs.unisakistudio.jp/kawaiiposing/manual/)
- [Cool Posing Toolin käyttöohje](https://docs.unisakistudio.jp/coolposing/manual/)
- [Ota yhteyttä](https://help.unisakistudio.jp/)

Liitä virheilmoitukseen tuotteen ja Unisaki Posing Systemin versiot, Unityn, VRChat SDK:n, Modular Avatarin ja NDMF:n versiot, käytetty avatar, toistamisohjeet sekä Consolen virheteksti.
