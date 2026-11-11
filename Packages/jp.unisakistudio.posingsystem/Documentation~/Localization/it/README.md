# Unisaki Posing System

Sistema di pose comune usato da Kawaii Posing, Cool Posing e altri strumenti. Usa Modular Avatar e NDMF per generare i menu e le animazioni necessari durante il caricamento dell’avatar.

Questo README riguarda la versione v3.2.0 in sviluppo. La data di uscita ufficiale non è ancora stabilita. La versione installata compare in alto nell’Inspector dello strumento. Consulta anche [aggiunte e modifiche](CHANGELOG.md).

## Lingua dell’interfaccia e documentazione offline

Scegli la lingua in «Language / 言語», in cima all’Inspector. Una sola scelta si applica sia all’Inspector sia ai menu VRChat generati. Carica nuovamente l’avatar per cambiare la lingua nel gioco.

Senza una scelta esplicita viene usata la lingua del computer; se non è supportata, viene usato l’inglese. Sono supportati Windows, macOS e Linux. L’ultima scelta è salvata in EditorPrefs e riutilizzata negli altri progetti dello stesso utente del sistema su questo computer. Tutti gli strumenti di posa e gli avatar usano la stessa lingua. Cambiarla nell’Inspector di qualsiasi strumento aggiorna la preferenza comune; le scelte di lingua dei singoli avatar vengono ignorate. Non viene sincronizzata con altri computer. Il sistema di archiviazione della licenza rimane invariato. Il pulsante della lingua appare solo nella conferma iniziale di installazione se non è stata salvata una lingua. Le modifiche successive si effettuano nell’Inspector.

Scegli la lingua di README, cronologia e guida dall’[elenco delle lingue](../index.html) incluso. Anche i collegamenti alla guida e alle novità nell’Inspector seguono la lingua scelta. Le condizioni d’uso originali e le traduzioni di riferimento restano disponibili separatamente.

## Cambio di postura in modalità desktop

È stato ridotto il ritardo prima dell’applicazione della posa scelta passando tra in piedi, accovacciato e seduto in modalità desktop. Il miglioramento si applica a build normali e precalcolo, senza comandi speciali o impostazioni aggiuntive. In VR e nelle situazioni particolari rimane il comportamento precedente. Aggiorna i vecchi precalcoli prima di caricare l’avatar.

## Installazione e aggiornamento

Aggiungi lo strumento tramite VCC o ALCOM seguendo i file e le istruzioni forniti con l’acquisto. Questo sistema viene installato come dipendenza dello strumento per pose.

Per v3.2.0 sono richiesti i seguenti componenti.

- Progetto avatar VRChat con Unity 2022.3
- VRChat Avatars SDK 3.10.4 o successivo
- Modular Avatar serie 1.x, versione 1.15.1 o successiva
- NDMF (Non-Destructive Modular Framework) serie 1.x, versione 1.14.0 o successiva

Se usi Kawaii Posing, aggiornalo a 3.2.0 in ALCOM / VCC e verifica che anche Unisaki Posing System sia alla versione 3.2.0. Se usavi il precalcolo, premi «Aggiorna precalcolo», salva la scena e carica l’avatar.

Cool Posing 1.2.0 usa il sistema comune 3.2.0. Aggiorna insieme prodotto e sistema comune alle versioni corrispondenti.

### Per chi usava la versione 8bit

I nuovi inserimenti usano soltanto il Prefab normale. Disattiva le opzioni inutili in «Funzioni» per ridurre il consumo. La sola selezione delle pose richiede 8 bit di base.

I vecchi Prefab 8bit rimangono in «Editor/Legacy» di ogni prodotto per conservare i riferimenti degli avatar e dei preset esistenti. Non devi eliminare o sostituire un Prefab per continuare a usare una versione 8bit già installata. Il menu di aggiunta di Unity non propone più la versione 8bit.

## Come leggere la configurazione

Nella Hierarchy seleziona 「可愛いポーズ」, 「カッコいいポーズ」 o l’oggetto equivalente all’interno dell’avatar per configurarlo.

| Scheda | Contenuti |
| --- | --- |
| Funzioni | Voci da aggiungere ai menu nel gioco, funzioni pratiche, icone e consumo di ExpressionParameters |
| Pose | Scelta, aggiunta e ordine delle pose, anteprima nella Scene, dettagli di ogni posa e regolazione animazione |
| Configurazione avatar | Correzioni della corporatura e delle ginocchia, importazione delle pose originali o di movimenti personali e preset |

Le funzioni con opzioni dettagliate hanno un comando per aprire le impostazioni, che compaiono sotto la voce. Anche il respiro si configura in «Funzioni».

Da «Aiuto / Novità» in alto puoi aprire la guida, l’assistenza e la cronologia. Puoi alternare la cronologia del prodotto e quella di Unisaki Posing System.

## Funzioni e consumo di ExpressionParameters

Le opzioni scelte in «Funzioni > Funzioni del menu» compaiono nel gioco. Per esempio, aggiungere «Specchio» consente di attivare e disattivare lo specchio in VRChat. Selezionarlo nell’Inspector non obbliga tutte le pose a rimanere specchiate.

| Funzione | Bit aggiuntivi |
| --- | ---: |
| Specchio | 1 bit |
| Blocca gesti | 1 bit |
| Animare il respiro | 0 bit |
| Respiro (attivazione nel gioco) | 1 bit |
| Altezza piedi | 8 bit |
| Altezza suolo | 8 bit |
| Modalità foto (solo vista locale) | 0 bit |
| Sincronizzare la Modalità foto | 16 bit |
| Tenere visibile la testa in Modalità foto | 0 bit |
| Disattivare l'oscillazione da fermo di VRChat | 0 bit |
| Ridurre le deformazioni dovute alla rotazione della testa | 1 bit (condiviso con Girarsi) |
| Girarsi | 10 bit (di cui 1 bit condiviso con la riduzione delle deformazioni) |

Girarsi richiede 1 bit per l’attivazione, 8 bit per l’inclinazione della testa e 1 bit per il blocco del collo. Se la riduzione delle deformazioni è già attiva, aggiungere Girarsi aumenta quindi il consumo di 9 bit. La parte condivisa non viene contata due volte; il totale cambia quando modifichi le caselle.

Il totale mostrato riguarda gli strumenti per pose installati nell’avatar, non l’intero avatar con abiti e altri sistemi. Le impostazioni comuni non sono contate più volte usando più strumenti. Se il numero di pose supera l’intervallo di numerazione di base, servono bit aggiuntivi anche per la selezione.

### Altezza piedi e Altezza suolo

- «Altezza piedi»: regola l’altezza dal lato dei piedi mantenendo la testa ferma. Serve per allineare i piedi al pavimento o a un letto.
- «Altezza suolo»: sposta verticalmente tutto il corpo, testa inclusa. 50% è l’altezza originale, 0% corrisponde a 3m sotto e 100% a 3m sopra. Il valore iniziale è 50%.

Entrambe si trovano in «Impostazioni» nel gioco; «Altezza suolo» segue «Altezza piedi». Spostando Altezza suolo dal centro, l’avatar sale o scende. Tornando vicino a 50%, riprende l’altezza originale e termina la correzione. Non esiste un interruttore separato per fluttuare. La vista viene corretta all’inizio del sollevamento e durante la regolazione dell’altezza del suolo.

Per le animazioni normali, la regolazione dell’altezza dei piedi non genera più duplicati dell’intero movimento corporeo. Questo riduce i dati di animazione generati, ma non significa che la dimensione totale di ogni avatar diventi un terzo.

### Modalità foto

1. Attiva «Funzioni > Funzioni del menu > Modalità foto».
2. In VRChat apri «Modalità foto» e scegli una posa in «Scegli posa».
3. «Distanza» sposta l’avatar fino a 3m in avanti; «Direzione» lo ruota. Con una distanza maggiore di 0, l’avatar si rivolge verso di te.
4. Attiva «Blocca piedi» quando necessario. Il blocco non è imposto all’avvio, così puoi regolare prima la posizione. Se hai aggiunto «Blocca gesti», puoi cambiare anche la forma delle mani nella stessa schermata.
5. «Esci da foto» azzera distanza e direzione e sblocca i piedi.

Movimento e rotazione della Modalità foto richiedono 0 bit se usati soltanto nella tua vista. Per mostrarli agli altri, attiva «Funzioni pratiche > Sincronizzare la Modalità foto». Distanza e direzione richiedono 16 bit complessivi.

Se la testa del tuo avatar scompare quando è vicina, attiva «Funzioni pratiche > Tenere visibile la testa in Modalità foto». Si applica soltanto alla tua vista durante le foto e ripristina il comportamento normale all’uscita.

### Cambiare posa sdraiata saltando

Se hai selezionato sia una posa supina sia una prona, puoi alternarle saltando sul posto. Con una sola orientazione selezionata, la posa rimane invariata. Per non cambiarla con un salto, disattiva «Cambiare posa sdraiata saltando» in «Funzioni → Funzioni pratiche» di Unity. Il consumo aggiuntivo di parametri sincronizzati è 0 bit. Negli avatar con più strumenti, disattivarlo in uno si applica all’intero avatar.

### Girarsi

Nelle pose sdraiate compatibili, inclinando la testa a destra o a sinistra anche il corpo ruota gradualmente sul fianco. Questa funzione non rende automaticamente compatibili tutte le pose sdraiate.

1. Attiva «Funzioni > Funzioni del menu > Girarsi».
2. In «Pose», apri i «Dettagli» della posa sdraiata desiderata e attiva «Abilita Girarsi».
3. Assegna un’animazione laterale a «Posa laterale per girarsi». La posa originale e quella sul fianco vengono usate per creare le rotazioni a destra e a sinistra. Regola l’«Intensità della correzione del contatto a terra» se necessario.
4. In VRChat scegli la posa compatibile e attiva «Girarsi». Usala con il collo sbloccato.

Nelle pose con Girarsi, l’inclinazione della testa determina la posizione di riproduzione, quindi le normali impostazioni MotionTime non sono disponibili. Per usare un’animazione di rotolamento già completa, assegnala al normale campo animazione e lascia vuota «Posa laterale per girarsi». Prepara un’animazione in cui 0 corrisponde al lato sinistro, 0.5 alla posizione supina e 1 al lato destro.

### Respiro e oscillazione predefinita

«Funzioni > Funzioni pratiche > Animare il respiro» fa muovere il petto e altre parti del corpo con la respirazione. Apri le impostazioni per scegliere Nessuno, Debole, Medio, Forte, Rapido o Personalizzato per ciascuna categoria di pose. Rapido indica una respirazione veloce. Personalizzato consente di assegnare la tua animazione di respiro.

Il solo movimento del respiro richiede 0 bit. Per fermarlo e riprenderlo in VRChat, attiva anche «Funzioni del menu > Respiro». La sincronizzazione dell’attivazione richiede 1 bit. Cambiare intensità non aumenta il consumo.

Collo, spalle e braccia seguono il petto per limitare lo spostamento di testa e mani. Non si tratta di un blocco completo della posizione: scegli l’intensità adatta alla corporatura e alla posa.

«Funzioni pratiche > Disattivare l'oscillazione da fermo di VRChat» riduce l’oscillazione corporea aggiunta normalmente da VRChat durante l’attesa. È disattivata inizialmente e conserva il respiro dello strumento e gli altri movimenti personalizzati. Puoi usarla anche senza il respiro e non richiede bit aggiuntivi. Non elimina le oscillazioni causate da elementi diversi dall’animazione di attesa standard.

Con più strumenti per pose, disattivare l’oscillazione standard in uno di essi si applica a tutto l’avatar. Per ripristinarla, disattiva questa opzione in tutti gli strumenti e carica nuovamente l’avatar.

### Ridurre le deformazioni dovute alla rotazione della testa

Usa questa opzione se, sbloccando il collo, la posa si deforma perché la sua direzione differisce da quella originale. Si trova in «Funzioni > Funzioni pratiche» e non aggiunge un menu specifico nel gioco. Con il collo bloccato viene usata la direzione della posa originale. Le pose supine sono escluse per mantenere l’orientamento iniziale verso l’alto.

### Icone del menu

- «Generare le icone delle pose»: crea un’immagine dell’avatar in ogni posa e la mostra nel menu. Le pose diventano più riconoscibili, ma la creazione delle immagini allunga la build.
- «Ridurre le dimensioni delle icone»: abbassa la risoluzione delle immagini per ridurre le dimensioni dell’avatar.

Le build Android non generano icone delle pose. Dopo aver modificato un abito, per esempio, usa «Ricrea icone». Le immagini dell’elenco si aggiornano con «Aggiorna miniature delle pose». Sono operazioni che possono richiedere tempo: leggi la conferma prima di eseguirle.

## Aggiungere, riordinare e visualizzare le pose

Scegli una categoria a sinistra di «Pose». La scheda superiore con la lente cerca i nomi in tutte le categorie. Apri «Dettagli» per configurare insieme animazione, icona, MotionTime e altre opzioni.

Per aggiungere più animazioni insieme, trascinale nella zona di aggiunta oppure selezionale in Project e premi «Importa clip selezionati in Project». I riferimenti duplicati allo stesso clip vengono esclusi.

Per cambiare l’ordine, scegli la destinazione in «Dettagli > Ordine». Puoi usare anche i pulsanti su e giù. Lo stato aperto o chiuso dei dettagli rimane invariato dopo lo spostamento.

«Anteprima» mostra la posa nella Scene senza aprire la finestra di regolazione. Terminandola torna la visualizzazione originale. Questa anteprima semplice non esegue la build MA o NDMF: può quindi differire dal risultato finale con abiti aggiunti durante la build o sistemi come FloorAdjuster. Verifica abiti e contatto a terra anche dopo la build dell’avatar.

## Regolazione animazione e regolazione automatica degli arti (IK)

Apri «Pose > Regolazione animazione» per regolare corpo e arti in dettaglio con l’anteprima. Al termine salva l’animazione di regolazione usando i comandi di salvataggio della finestra.

Nei prodotti e nelle pose compatibili, la regolazione automatica adatta gli arti alla corporatura dell’avatar. «Abilita regolazione automatica degli arti» nella finestra agisce globalmente, mentre «Abilita IK per questa posa» agisce sulla singola posa. Un messaggio segnala le pose senza regolazione automatica disponibile.

Per rifinire manualmente un risultato automatico, premi «Integra IK nelle regolazioni». Il risultato IK attuale viene scritto in una nuova animazione di regolazione e l’IK della posa viene disattivata per quell’avatar. L’animazione originale rimane. Il pulsante è disponibile quando la regolazione globale e quella della posa sono attive ed esistono impostazioni IK applicabili.

«Impedire la piegatura inversa delle ginocchia» in «Configurazione avatar» attiva la correzione delle ginocchia. Imposta l’angolo nei dettagli, in «Flessione minima del ginocchio (gradi)». «Attivare la correzione dell'avatar» è disponibile se esistono dati di correzione corrispondenti. Alcune correzioni non possono essere combinate con la regolazione automatica degli arti.

## Pose originali, movimenti personali e preset

Queste opzioni si configurano in «Configurazione avatar».

- «Usare le pose originali in piedi, accovacciate e prone»: importa automaticamente le pose già impostate nell’avatar.
- «Usa i tuoi movimenti»: apri le impostazioni d’importazione e assegna separatamente un movimento a ogni tipo, come in piedi, accovacciato (Crouching) o prono (Prone). In generale, imposta un movimento per tipo.
- «Integrare i blocchi del tracciamento con altri strumenti»: attivalo se i blocchi non funzionano correttamente insieme ad altri sistemi.

Le impostazioni per assegnare movimenti personali sono separate dall’importazione automatica delle pose originali.

I preset sono in fondo alla scheda. Cercali per nome e premi «Applica questo preset» per cambiare le impostazioni delle pose. Servono soprattutto alla configurazione iniziale. Sostituiscono i dati attuali, quindi salva eventuali regolazioni prima di cambiare preset.

Per mostrare l’elenco vengono caricati soltanto i nomi e le informazioni sugli avatar compatibili. Le animazioni di regolazione si caricano quando applichi il preset scelto. Anche la prima creazione dell’elenco non precarica le animazioni di tutti gli avatar.

L’elenco distingue «Compatibile con l'avatar», «Preset utente» e «Preset ufficiali Unisaki Studio». «Nascondi preset inclusi» è un’impostazione di visualizzazione comune al progetto. Aggiungere, modificare o eliminare preset aggiorna l’elenco. Se non cambia, premi «Ricarica».

## Precalcolo e avvisi

Con «Usare le pose originali in piedi, accovacciate e prone» attivo, a ogni generazione vengono rilevati i movimenti dalla Base dell’avatar. I risultati non vengono riscritti nelle impostazioni manuali: se usi soltanto l’importazione automatica, l’elenco resta senza assegnazioni. I cambiamenti dei movimenti Base e l’attivazione dell’importazione automatica vengono applicati alla generazione successiva.

Durante il caricamento, l’importazione automatica si applica alla copia della build. Non modifica gli oggetti originali nella scena, le impostazioni d’importazione, i Controller sorgente o i movimenti. Se esiste un precalcolo e i dati in ingresso non cambiano, il suo risultato viene riutilizzato.

I tipi con assegnazioni manuali danno priorità a queste ultime, applicando anche rotazione, animazione di regolazione e posizione di riproduzione. Disattivare «Usa» o rimuovere un movimento già assegnato impedisce l’importazione automatica di quel tipo. «Cancella selezione manuale» rimuove impostazioni e regolazioni del tipo: se l’importazione automatica è attiva, lo rileverà nuovamente alla prossima generazione. L’operazione è annullabile con Undo. Se esistono più vecchie impostazioni dello stesso tipo, viene preferita quella attiva selezionata nell’Inspector.

Le assegnazioni aggiunte nell’elenco dalla vecchia importazione automatica non vengono eliminate automaticamente perché non sono distinguibili da quelle registrate o regolate a mano. Usa «Cancella selezione manuale» soltanto per i tipi che vuoi riportare all’importazione automatica. Se più prodotti sono installati sullo stesso avatar, anche le assegnazioni manuali degli altri prodotti si applicano alle pose originali comuni.

Il precalcolo riduce il tempo della build riutilizzando i dati generati. Crealo o aggiornalo con «Aggiorna precalcolo». La normale build dell’avatar genera comunque i dati necessari: non serve eseguire manualmente il precalcolo ogni volta.

Una modifica delle impostazioni può richiedere l’aggiornamento, segnalato in alto da «Controlla impostazioni» e dai simboli nella Hierarchy. Dopo aver modificato pose o impostazioni IK, aggiorna il precalcolo per ridurre i tempi delle build successive.

«Impostazioni di visualizzazione avvisi» è nel riquadro «Controlla impostazioni». Puoi nascondere singole categorie di avvisi o i simboli della Hierarchy. Nasconderli non risolve i problemi e non aggiorna i dati generati. Gli errori e le generazioni necessarie restano attivi.

## Note d’uso

- Progettato per il tracciamento a 3 punti e il desktop. Il blocco delle pose durante il tracciamento completo del corpo non è supportato.
- Corporatura e modifiche possono richiedere regolazioni aggiuntive per il contatto degli arti e l’aspetto degli abiti.
- Livelli Base particolari o altri sistemi di postura possono dare risultati inattesi.
- Movimento e rotazione della Modalità foto e altezza del suolo correggono le pose dello strumento e i movimenti standard o personali senza posa selezionata. Verifica nel client VRChat i movimenti standard sostituiti e il comportamento della vista.

## Condizioni d’uso e assistenza

Il codice sorgente e le risorse di questo repository non sono open source. Per usarli occorre acquistare regolarmente il prodotto e accettare le [condizioni d’uso incluse](../../../%E3%82%86%E3%81%AB%E3%81%95%E3%81%8D%E3%83%9D%E3%83%BC%E3%82%BA%E3%82%B7%E3%82%B9%E3%83%86%E3%83%A0%E5%88%A9%E7%94%A8%E8%A6%8F%E7%B4%84%EF%BC%88%E6%97%A5%E6%9C%AC%E8%AA%9E%E7%89%88%EF%BC%89.pdf).

- [Manuale Kawaii Posing](https://docs.unisakistudio.jp/kawaiiposing/manual/)
- [Manuale Cool Posing](https://docs.unisakistudio.jp/coolposing/manual/)
- [Assistenza](https://help.unisakistudio.jp/)

Per segnalare un problema, indica le versioni del prodotto e di Unisaki Posing System, quelle di Unity, VRChat SDK, Modular Avatar e NDMF, l’avatar usato, i passaggi per riprodurlo e gli errori della Console.
