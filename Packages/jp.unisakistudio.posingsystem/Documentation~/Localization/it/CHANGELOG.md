# Cronologia delle modifiche

## [3.2.0] - Non pubblicata

### Aggiunte

- Inspector, comandi di modifica, finestre d’installazione, nomi delle pose standard e menu VRChat generati disponibili in 30 lingue.
- Scelta della lingua unificata per Inspector e menu nel gioco. Inizialmente viene usata la lingua del computer; l’ultima scelta è riutilizzata negli altri progetti dello stesso utente del sistema.
- Apertura di README, cronologia e guida offline nella lingua scelta.
- Preparati i nomi visualizzati delle 128 pose standard e le descrizioni di tutte le funzioni, mantenendo la terminologia esistente.

### Compatibilità

- Conservati impostazioni esistenti dell’avatar, nomi personalizzati, riferimenti agli asset, identificatori dei parametri e archiviazione della licenza.

## [3.1.2] - 2026-11-09

- Corretto il viso che restava rivolto verso l’alto dopo essersi alzati da proni con «Ridurre le deformazioni dovute alla rotazione della testa» attivo.
- Il blocco della testa è ora modificabile in «Impostazioni → Blocco automatico della testa da supini» nel menu EX anche disattivando «Altezza piedi». Gli avatar che avevano perso la voce in 3.1.0 o 3.1.1 la recuperano dopo aggiornamento e nuova build.
- «Aggiorna precalcolo» funziona anche con l’oggetto principale dell’avatar disattivato in Unity.
- Corretta l’occasionale impossibilità di generare pose dopo un aggiornamento se in Assets rimaneva un vecchio Controller comune delle pose. Conservate anche le impostazioni dei Controller modificati personalmente.
- Quando lo strumento o un suo genitore disattivato impedisce l’uso dell’Animator di posa condiviso, viene mostrato il nome dell’oggetto interessato. Puoi raggiungerlo da «Controlla impostazioni → Seleziona oggetto».
- Quando il precalcolo segnala impostazioni modificate, i dettagli consentono di vedere quali voci differiscono dall’ultimo salvataggio.
- Se un errore di script blocca la build (PS050), vengono mostrati fino a 5 errori originali rilevati da Unity. Il comando per chiedere assistenza apre una bozza con versioni di Unity e degli strumenti compatibili e numero d’errore, che puoi verificare, modificare e inviare.

Se usavi il precalcolo, dopo l’aggiornamento premi «Aggiorna precalcolo», salva la scena e ricrea e ricarica l’avatar.

## [3.1.1] - 2026-11-08

- «Altezza suolo» e «Girarsi», se attivi, compaiono in «Impostazioni» del menu EX di VRChat anche con «Specchio» disattivato.
- Conservati i corretti rotolamenti per «Gambe distese», «Relax» e «Un ginocchio alzato» anche dopo riordino, eliminazione o rinomina nelle vecchie versioni. Funziona anche dopo un Unpack completo se la posa originale è identificabile. Restano invariati pose personali, rotolamenti disattivati esplicitamente e movimenti laterali sostituiti.
- Corretto il breve passaggio alla posa prona selezionando l’avatar in VRChat e quello alla posa supina saltando con sole pose prone selezionate. Disattiva il cambio con salto in «Funzioni → Funzioni pratiche → Cambiare posa sdraiata saltando» se necessario. È attivo inizialmente.
- Se dopo un aggiornamento viene perso il riferimento all’Animator Controller comune e può essere recuperato, un avviso lo segnala e il Controller viene ricreato dalle impostazioni. Conservati anche i riferimenti condivisi da più avatar.
- In presenza di errori di compilazione, la build si ferma subito indicandone la causa. La pulizia automatica dei Controller generati viene sospesa finché gli errori impediscono di verificarne i riferimenti.
- Aggiungendo uno strumento viene preferito il Prefab ufficiale anche se il progetto contiene un altro Prefab con lo stesso nome. Il precalcolo prosegue anche quando nel Controller comune differiscono soltanto le interruzioni di riga.
- I Write Defaults dei livelli FX aggiunti dallo strumento, per esempio Altezza suolo, seguono ora le impostazioni dell’avatar.

Se usavi il precalcolo, dopo l’aggiornamento premi «Aggiorna precalcolo» sullo strumento nella scena, salva e ricrea e ricarica l’avatar.

## [3.1.0] - 2026-11-07

- Aggiunta la regolazione automatica degli arti (IK) secondo la corporatura per le pose compatibili, con correzioni specifiche dell’avatar e prevenzione della piegatura inversa delle ginocchia.
- Posizione e direzione dell’avatar regolabili in Modalità foto. Puoi scegliere la sincronizzazione e la visibilità della tua testa durante le foto.
- «Altezza suolo» sposta tutto il corpo fino a 3m verso l’alto o il basso, separatamente da «Altezza piedi».
- Aggiunti respiro, disattivazione dell’oscillazione standard da fermo e riduzione delle deformazioni dovute alla direzione del collo.
- Funzioni usate e consumo dei parametri visibili nell’Inspector. Aggiunti ricerca delle pose, aggiunta in gruppo, riordino e anteprima semplice nella Scene.
- Cambio tra in piedi, accovacciato e seduto più veloce in desktop.
- Migliorati avvisi di precalcolo dopo modifiche, scorrimento dell’Inspector e lunghe pause durante la pulizia dei dati generati.
- Corretto l’avviso di aggiornamento e la rigenerazione al caricamento con importazione automatica attiva nonostante il precalcolo invariato. I risultati si applicano all’avatar generato senza modificare le impostazioni dell’oggetto originale.
- Aggiunto Girarsi inclinando la testa e impostazioni dei movimenti compatibili per ogni posa.
- Migliorati trasferimento dei vecchi preset di rotolamento, importazione dei movimenti personali e gestione degli asset generati.

## [3.1.0-beta.2] - Beta aperta (2026-11-05)

- Aggiunte la lunghezza fissa e le differenze verticale (Y) e avanti/indietro (Z) tra Head e Viewpoint come riferimenti di lunghezza della posizione IK.
- Avvisi dell’Inspector mostrati anche a sinistra dei nomi nella Hierarchy, inclusi precalcolo mancante o superato e avvisi delle operazioni su importazione e preset.
- Corrette le lunghe pause di Unity quando la pulizia automatica, per esempio aprendo una scena, caricava Controller generati di altre scene.
- Corretta la perdita della cronologia del precalcolo modificando pose o funzioni. Ora viene segnalata una modifica successiva al precalcolo invece di un precalcolo mai eseguito, con avviso anche nella Hierarchy.
- Risultati del confronto del precalcolo condivisi tra Inspector e Hierarchy per ridurre il carico dello scorrimento quando impostazioni e asset non cambiano. Il confronto durante la build continua a essere eseguito ogni volta.

## [3.1.0-beta.1] - Beta aperta (2026-11-04)

- Cambio di postura normale in desktop passato a una sola valutazione. Si applica a build normali e precalcolo; VR e casi particolari mantengono il percorso precedente.
- Precalcoli alpha.3 e precedenti riconosciuti come da aggiornare.
- Vista mantenuta cambiando l’altezza del suolo durante le foto, con ritorno all’ultima altezza al termine.

- Importazione automatica limitata al rilevamento durante la generazione, senza aggiungere o modificare impostazioni manuali nel precalcolo o nella build.
- Rigenerazione sensibile all’attivazione dell’importazione automatica, alle modifiche di Base e alla cancellazione delle assegnazioni manuali. Conservate assegnazioni e regolazioni condivise tra prodotti e ripristinabili i movimenti standard al posto di quelli vecchi importati.
- Controller generati dal precalcolo gestiti in un file per avatar e GUID di scena. I riferimenti condivisi per duplicazione, tra scene o da Prefab vengono separati prima dell’aggiornamento.
- Uso degli asset temporanei NDMF durante la build, senza aumentare i file generati per l’editing.
- Verifica dei dati generati aprendo o salvando una scena. Modifiche non salvate, Undo/Redo e riferimenti esterni vengono conservati; soltanto i file inutilizzati di proprietà certa vanno nel cestino.
- Applicati rotazione, animazioni di regolazione e posizione di riproduzione dei movimenti importati. Elaborazione di rotazione, RootT/RootQ e regolazioni Humanoid unificata con le pose normali.
- Regolazioni dei BlendTree annidati duplicate appositamente per la destinazione, conservando movimento originale e miscelazione della camminata.
- La posizione di riproduzione «solo in piedi» viene convertita in posa statica alla frequenza originale senza fermare la riproduzione della camminata.

## [3.1.0-alpha.2] - Test chiuso (2026-10-26)

### Correzioni e modifiche

- Correzioni di Modalità foto e altezza suolo applicate anche ai movimenti standard e personali senza posa dello strumento selezionata.
- Altezza suolo estesa a 3m sopra e sotto. 50% è altezza originale e iniziale, 0% indica 3m sotto e 100% 3m sopra.
- Corretta la modifica del BlendTree sorgente durante la sostituzione «solo in piedi».
- Uso delle correzioni manuali per pose senza regole IK, evitando avvisi di generazione superflui.
- Corretti la confusione tra bozze di pose omonime e il mancato rilevamento di movimenti cambiati nello stesso asset.
- Corretto il numero di una posa iniziale disattivata che selezionava inizialmente un’altra posa.
- Mostrata una spiegazione invece di un’eccezione importando AnimatorOverrideController non supportati.
- Conservato Girarsi esplicitamente disattivato applicando vecchi preset.
- Icone delle funzioni più leggibili nel tema chiaro dell’Editor.

Proseguono le verifiche nel client VRChat della vista e dei movimenti standard sostituiti.

## [3.1.0-alpha.1] - Prima della pubblicazione

### Aggiunte

- Aggiunti attivazione per funzione e visualizzazione del consumo di ExpressionParameters.
- Aggiunta Modalità foto con movimento in avanti e rotazione dell’avatar.
- Aggiunta la sincronizzazione opzionale: 0 bit solo locale, 16 bit sincronizzati.
- Aggiunta l’opzione per mantenere visibile la propria testa durante le foto.
- Aggiunta «Altezza suolo», che muove tutto il corpo, testa inclusa, di 3m sopra o sotto. 50% è altezza originale e iniziale, 0% è 3m sotto e 100% 3m sopra.
- Aggiunti Girarsi inclinando la testa e animazioni compatibili per posa.
- Aggiunti movimento del respiro e intensità Debole, Medio, Forte, Rapido e Personalizzato.
- Scelta separata del movimento e del menu di attivazione del respiro: 0 bit per il solo movimento, 1 bit per sincronizzare l’attivazione.
- Aggiunta la disattivazione dell’oscillazione standard da fermo di VRChat.
- Aggiunta la riduzione delle deformazioni dovute alla diversa direzione del collo.
- Aggiunta regolazione automatica degli arti (IK) secondo la corporatura per le pose compatibili, attivabile globalmente o per posa.
- Aggiunto «Integra IK nelle regolazioni» per salvare il risultato nell’animazione di regolazione.
- Aggiunte correzioni dell’avatar, prevenzione della piegatura inversa delle ginocchia e flessione minima.
- Aggiunta anteprima semplice nella Scene dall’elenco delle pose.
- Aggiunti schede per categoria, ricerca trasversale, aggiunta di più animazioni e scelta della destinazione nell’ordine.
- Aggiunta la cronologia del prodotto e del sistema comune nell’Inspector.

### Modifiche

- Caricamento dei preset più rapido, con meno attesa aggiungendo lo strumento o aprendo l’elenco.
- Elenco aggiornato automaticamente dopo aggiunta, modifica o eliminazione di preset.
- Inspector organizzato in 3 schede: «Funzioni», «Pose» e «Configurazione avatar».
- Aggiunte descrizioni e icone alle impostazioni e apertura dei dettagli uniformata.
- Dettagli del respiro spostati nelle funzioni e preset in fondo alla configurazione avatar.
- Separati assegnazione dei movimenti personali e importazione automatica delle pose originali.
- Ridotti i duplicati di animazioni dell’intero corpo generati per Altezza piedi.
- Aggiornamento delle immagini delle pose tramite pulsante con conferma.
- Preferenze degli avvisi raccolte in «Controlla impostazioni».
- Eliminata l’aggiunta di nuovi Prefab 8bit dedicati in favore delle opzioni del Prefab normale, conservando i riferimenti esistenti.
- Versione minima di VRChat Avatars SDK portata a 3.10.4.

### Correzioni

- Correzioni di Modalità foto e altezza suolo applicate ai movimenti standard o personali senza posa selezionata.
- Corretta la modifica del BlendTree originale sostituendo «solo in piedi».
- Uso delle regolazioni manuali per pose senza regole IK valide, evitando avvisi IK inutili.
- Corretti la mescolanza delle regolazioni tra pose omonime e il movimento sostituito nello stesso asset non recepito dalla rigenerazione.
- Il numero di una posa iniziale disattivata non seleziona più un’altra posa che lo riutilizza.
- Mostrata una spiegazione invece di un’eccezione se l’importazione originale trova un AnimatorOverrideController non supportato.
- Conservate le impostazioni di Girarsi esplicitamente disattivate nei vecchi preset.
- Migliorato lo sfondo delle icone nell’Inspector per riconoscerle anche nel tema chiaro.

- Corrette le icone mancanti di Girarsi, Altezza suolo e Modalità foto aggiungendo lo strumento a un avatar.
- Corretta la perdita delle impostazioni di Girarsi delle pose compatibili applicando vecchi preset.
- Corretto il testo di un nome in modifica trasferito a un’altra posa aprendo i suoi dettagli.
- Corretti chiusura dei dettagli riordinando le pose e Undo/Redo delle impostazioni.
- Corretti sovrapposizione dell’avatar originale nell’anteprima e doppio conteggio della posizione di inserimento.
- Corretti errori nell’anteprima di avatar con componenti mancanti.
- Corretti scostamenti IK di posizione e direzione degli arti causati da assi e scale degli scheletri diversi.
- Corretti avvisi e stato del precalcolo non aggiornati subito dopo una modifica.
- Corretto il riutilizzo di un vecchio precalcolo dopo la modifica delle impostazioni IK.
- Corretti errori di build nella generazione di funzioni aggiuntive in alcune configurazioni.

## [3.0.16] - 2026-09-14
- Dipendenza minima da NDMF aggiornata a 1.14.0 per usare l’API `VirtualAnimatorController.SetParameter`.

## [3.0.15] - 2026-09-13
- Corretti i grandi cambiamenti di posizione del corpo e flessione delle ginocchia attivando il blocco del collo da supini su avatar con scala Hips modificata e FloorAdjuster.

## [3.0.14] - 2026-09-09
- Corretta la scomparsa delle voci «Blocca gesti» dal secondo strumento in poi quando più strumenti sono installati sullo stesso avatar.
- Corretto il ritorno occasionale del livello USSPS_Locomotion a Standing nonostante una posa come SleepDown selezionata.
- Corretti gli scostamenti di altezza e posizione X/Z con FloorAdjuster, rispettando l’ordine delle versioni MA e AAO.
- Ripristino automatico di PosingSystem_Locomotion standard se viene eliminato l’AnimatorController precalcolato in Generated.
- Corretto l’errore di build quando sono eliminate tutte le pose in piedi.
- Elenco preset ordinato per nome dell’avatar.

## [3.0.13] - 2026-09-04
- Corretta l’autotransizione della posa in piedi che rendeva instabile il ripristino tornando in piedi subito dopo aver iniziato ad accovacciarsi in VR.

## [3.0.12] - 2026-09-04
- Corretta la falsa segnalazione di impostazioni modificate nella build NDMF di uno strumento senza precalcolo.
- Corretto l’errore `AddAssetToSameFile` aggiungendo asset generati a un AnimatorController temporaneo durante la build NDMF.

## [3.0.11] - 2026-08-31
- Corretto il fallimento con NDMF 1.14.7 o successivo dovuto alla Scene non valida dell’avatar per il precalcolo.
- Corretta `MissingReferenceException` quando la finestra d’errore NDMF accedeva a un LocalizationAsset già distrutto.

## [3.0.10] - 2026-08-29
- Caricamento degli asset delle definizioni preset rinviato fino all’apertura del selettore; cache riutilizzata se l’indice GUID non cambia.
- Rivalutazione delle icone di avviso ed errore nella Hierarchy soltanto dopo modifiche e cache condivisa per la ricerca degli asset del menu prodotto. Ridotti i blocchi dell’Editor ogni pochi secondi nelle scene con molti strumenti.
- Corretta `ArgumentException` registrando valori di parametri duplicati nel precalcolo con più strumenti sullo stesso avatar.

## [3.0.9] - 2026-08-26
- Evitata NullReferenceException se la conversione non ottiene VRCAvatarDescriptor da PosingSystem, indicando i log di build necessari all’indagine.
- Corretto l’errore `Avatar is null` quando precalcolo o build riutilizzavano l’anteprima di un Avatar temporaneo creato e cancellato da NDMF. Separata la conversione dalla cache di anteprima e prevenuti conflitti di nomi degli asset temporanei tra anteprime.
- Corretto il riferimento di anteprima del Prefab 「可愛いポーズ」 non salvato nella scena, che moltiplicava gli avatar nascosti. Anteprime escluse dal salvataggio e recupero di quelle vecchie al caricamento della scena, al passaggio in Play e al ricaricamento degli script.

## [3.0.8] - 2026-08-18
- Corretti Unity Assertion e il rischio di dati di postura non validi durante nuove correzioni di altezza con FloorAdjuster o simili sui movimenti precalcolati.
- Misurazione dell’altezza resa più sicura per non interferire con l’avatar originale o lo stato AnimationMode di altri strumenti.

## [3.0.7] - 2026-08-12
- Corretti lo scostamento della vista e la flessione anomala delle ginocchia da supini con strumenti di altezza come FloorAdjuster.
- Corretto l’errore con PosingSystem fuori dall’avatar (grazie a PancakeTorto1se).
- Corretto l’errore causato dal proseguimento dopo il fallimento del caricamento dell’avatar.
- Corretta l’occasionale falsa segnalazione di impostazioni cambiate subito dopo un precalcolo.
- Migliorata l’importazione automatica dei movimenti di spostamento.
- Aggiunti avvisi per avatar con alcuni rig particolari.
- Aggiunti conferma delle modifiche e specchio dei valori alla finestra di regolazione (grazie a kraihd).
- Aggiunti indicazioni quando altri strumenti hanno errori e controlli dei punti soggetti a problemi.

## [3.0.6] - 2026-03-17
- Corrette pose gravemente deformate su alcuni avatar con controllo della postura già integrato.

## [3.0.5] - 2026-01-31
- Corretto il crash di Unity con alcuni ExpressionMenus.
- Corretti problemi dell’avatar con il Prefab d’integrazione ごろ寝EX (grazie a あずき).
- Corrette rotazioni errate nella fusione delle animazioni di regolazione.

## [3.0.4] - 2026-01-11
- Migliorati alcuni comportamenti degli avatar inseriti in Prefabs.
- Aggiunta la rimozione della licenza.
- Corretti problemi del collo in numerose pose.
- Ridotto leggermente il carico di apertura dell’Inspector.
- Creazione delle icone anche durante il precalcolo e modifica del testo del pulsante di aggiornamento avatar.
- Avvisi nascosti se il precalcolo non è mai stato eseguito, dato che molti utenti sembrano non usarlo.
- Eliminate risorse inutili.
- Corretta l’impossibilità di regolare le dita.

## [3.0.3] - 2026-01-04
- Corretto il preset applicato diverso da quello scelto (grazie a あずき).

## [3.0.2] - 2026-01-02
- Supporto per il tema scuro di Unity.
- Corrette le pose deformate dopo rotazioni nella regolazione animazione.
- Modificate dimensioni e compressione delle icone di menu generate e aggiunta un’opzione per icone ancora più piccole.
- Reintrodotte integrazione del tracciamento e importazione automatica dei movimenti dell’avatar, rimosse in 3.0.0.

## [3.0.1] - 2025-12-29
- Corretti problemi della configurazione dei movimenti predefiniti.
- Corrette le pose dell’avatar che si deformavano spontaneamente.

## [3.0.0] - 2025-12-28
- Aggiunto il precalcolo.
- Aggiunta la regolazione animazione.
- Aggiunti i preset.

## [2.2.5] - 2025-05-08
- Risolti problemi di compatibilità con alcuni sistemi.

## [2.2.4] - 2025-04-04
- Corretto il blocco del cambio posa con alcuni avatar e versioni di ModularAvatar.

## [2.2.3] - 2025-03-13
- Corretti movimenti anomali di alcuni avatar, come il salto di 森羅.
- Corretti valori iniziali talvolta errati di Blocca gesti.

## [2.2.2] - 2025-03-06
- Corretti errori in alcuni ambienti.

## [2.2.1] - 2025-02-05
- Corretto un crash di Unity.
- Versione aggiunta al nome dello strumento in NDMF Console.

## [2.2.0] - 2025-02-02
- Aggiunta la possibilità di bloccare i gesti e usare le mani dell’animazione.

## [2.1.6] - 2025-02-02
- Corrette pose deformate con alcuni altri strumenti.
- Corretti errori dipendenti dalle impostazioni.

## [2.1.5] - 2024-12-10
- Risolti conflitti con alcuni sistemi.
- Aggiunto un pulsante per rimuovere la versione Unitypackage.

## [2.1.4] - 2024-12-08
- Corretti errori su alcuni avatar.
- Corretto il blocco piedi inutilizzabile per conflitti con sistemi o avatar.
- Corrette le pose che tornavano a quelle standard VRC in desktop.

## [2.1.3] - 2024-12-01
- Corretto il mancato cambio posa con avatar o sistemi che usano VRMode con un tipo errato.
- Aggiunta la segnalazione in NDMF-Console dei conflitti tra sistemi causati da tipi di parametri diversi.

## [2.1.2] - 2024-12-01
- Aggiunta una spiegazione in NDMF-Console quando errori di altri strumenti causano errori di Unisaki Posing System durante la build dell’avatar.
- Corretti errori di build su alcuni avatar.
- Migliorato il comportamento subito dopo il caricamento dell’avatar.
- Eliminata l’elaborazione originale dello spostamento dell’avatar, che impediva il funzionamento su alcuni avatar.

## [2.1.1] - 2024-11-20
- Corretti problemi relativi a MergeAnimator in PosingOverride.

## [2.1.0] - 2024-11-20
- Risolti piedi e bacino sotto il terreno in desktop, consentendo pose corrette.
- Aggiunto il cambio posa indipendente dall’altezza della testa.
- Aggiunta una modalità senza icone per caricare avatar Quest.
- Aggiunti alcuni avvisi in caso di problemi dell’avatar.
- Risolti conflitti dei blocchi con AFK e sistemi di blocco braccia di altri strumenti.
- Aggiunto il blocco automatico del collo da supini.

## [2.0.0] - 2024-11-12
- Pubblicata la versione compatibile VPM/VCC.
- Modificato il nome dello strumento mostrato in caso di errore #1.
- Aggiunte le condizioni d’uso #5.
