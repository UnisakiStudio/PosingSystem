# Änderungsverlauf

## [3.2.0] - Unveröffentlicht

### Hinzugefügt

- Inspector, Bearbeitungsbefehle, Installationsdialoge, Standardposennamen und erzeugte VRChat-Menüs in 30 Sprachen verfügbar.
- Gemeinsame Sprachauswahl für Inspector und Spielmenü. Anfangs gilt die Computersprache, die letzte Auswahl wird für andere Projekte desselben Betriebssystembenutzers übernommen.
- README, Änderungsverlauf und Offline-Anleitung öffnen sich in der gewählten Sprache.
- Anzeigenamen der 128 Standardposen und Funktionsbeschreibungen mit der bestehenden Terminologie abgestimmt.

### Kompatibilität

- Vorhandene Avatareinstellungen, eigene Namen, Assetreferenzen, Parameterkennungen und Lizenzspeicherung bleiben erhalten.

## [3.1.4] - 2026-11-11

- Behoben, dass avatarspezifisch zugewiesene Hock- und Bauchlageanimationen nicht angewendet wurden und stattdessen die VRChat-Standardposen erschienen. Dies betrifft sowohl automatischen Import als auch manuelle Zuweisungen.
- Behoben, dass mit MA Bone Proxy befestigte Haare, Brillen und andere Teile beim Bearbeiten oder Vorschauen von Posen an ihrer ursprünglichen Position blieben. Sie folgen nun der Vorschaupose, während Einstellungen und Platzierung des Originalavatars erhalten bleiben.
- Fehler bei der blickpunktbasierten Posenkorrektur während der Bildaufnahme für Posen wie „仰向け寝_お疲れ“ behoben. Die Bilder verwenden eine Animation mit den beim Build angewendeten Posen- und Bodenkorrekturen.

Aktualisiere Unisaki Posing System in ALCOM / VCC auf 3.1.4. Kawaii Posing 3.1.2 und Cool Posing 1.1.0 können unverändert weiterverwendet werden. Falls du die Vorberechnung nutzt, klicke nach dem Update auf „Vorberechnung aktualisieren“, speichere die Szene und erstelle und lade den Avatar erneut hoch.

## [3.1.3] - 2026-11-09

- „Posenvorschaubilder aktualisieren“ und Vorberechnung an einem duplizierten Avatar löschen nicht mehr die Posenbilder des Originalavatars.
- Fehlerhafte Anzeige „Einstellungen geändert“ nach bloßem erneuten Aufnehmen der Bilder behoben, wenn die Vorberechnung vor dem ersten Speichern der Szene oder im Prefab-Bearbeitungsmodus ausgeführt wurde.
- Behoben, dass eine Vorberechnung vorzeitig abbrechen konnte, wenn Posendaten einer vorherigen Vorberechnung übrig geblieben waren.
- Öffnen der Liste ohne vorhandene Posenbilder beschleunigt. Außerdem die Wartephase überarbeitet, die Unity zu Beginn einer Vorberechnung oder eines Builds bis zum Abschluss der Paketabfrage blockierte.
- Bearbeitungseinstellungen deaktivierter Posenwerkzeuge im Avatar werden nun auch bei PC- und Android-Builds entfernt.
- „Hilfe zu diesem Fehler anfragen“ kann nun spätere Diagnosenummern und andere Angaben aus der Console in den Entwurf aufnehmen, auch wenn die ersten Fehler keine Ursache erkennen lassen. Bei Fehlern in den Clip-Einstellungen von „Umdrehen“ enthält der Entwurf auch zu prüfende Stellen.

Aktualisiere Unisaki Posing System in ALCOM / VCC auf 3.1.3. Kawaii Posing 3.1.2 und Cool Posing 1.1.0 können unverändert weiterverwendet werden. Da diese Produkte eine ältere gemeinsame Systemversion angeben, kann beim Update eine Konfliktbestätigung erscheinen. Falls du die Vorberechnung nutzt, klicke nach dem Update auf „Vorberechnung aktualisieren“, speichere die Szene und erstelle und lade den Avatar erneut hoch.

## [3.1.2] - 2026-11-09

- Behoben: Mit „Verformungen durch Kopfdrehung verringern“ blieb das Gesicht nach dem Aufstehen aus Bauchlage nach oben gerichtet.
- Die Kopffixierung kann im EX-Menü unter „Einstellungen → Automatische Kopffixierung in Rückenlage“ auch bei deaktivierter Fußhöhe umgeschaltet werden. In 3.1.0 oder 3.1.1 verschwundene Einträge kehren nach Aktualisierung und neuem Build zurück.
- „Vorberechnung aktualisieren“ funktioniert auch bei deaktiviertem Avatar-Hauptobjekt in Unity.
- Behoben: Ein alter gemeinsamer Posencontroller in Assets konnte die Posenerzeugung nach Updates verhindern. Eigene Controller-Anpassungen bleiben erhalten.
- Ist das Werkzeug oder ein übergeordnetes Objekt deaktiviert und der gemeinsame Posen-Animator dadurch unbenutzbar, wird der betroffene Name angezeigt. „Einstellungen prüfen → Objekt auswählen“ führt dorthin.
- Bei veränderten Einstellungen der Vorberechnung lassen sich in den Details die seit dem Speichern geänderten Punkte ansehen.
- Bei einem Build-Stopp durch Skriptfehler (PS050) werden bis zu 5 ursprüngliche Unity-Fehler angezeigt. Die Supportfunktion öffnet einen bearbeitbaren Entwurf mit Unity-/Werkzeugversionen und Fehlernummer, den du prüfen und absenden kannst.

Falls du die Vorberechnung verwendet hast, klicke nach dem Update auf „Vorberechnung aktualisieren“, speichere die Szene und erstelle und lade den Avatar erneut hoch.

## [3.1.1] - 2026-11-08

- Aktivierte Bodenhöhe und Umdrehen erscheinen nun unter „Einstellungen“ im VRChat-EX-Menü, auch wenn Spiegeln aus ist.
- Die richtigen Umdrehbewegungen für „Beine ausgestreckt“, „Entspannt“ und „Ein Knie hoch“ bleiben auch nach altem Sortieren, Löschen oder Umbenennen erhalten. Auch nach vollständigem Unpack gilt dies, sofern die Originalpose erkennbar ist. Eigene Posen, absichtlich deaktiviertes Umdrehen und ersetzte Seitenbewegungen bleiben erhalten.
- Kurze Bauchlage bei der VRChat-Avatarauswahl sowie kurze Rückenlage beim Springen mit ausschließlich gewählter Bauchlage behoben. Der Sprungwechsel ist unter „Funktionen → Komfortfunktionen → Liegepose durch Springen wechseln“ abschaltbar und anfangs aktiv.
- Bei verlorener, wiederherstellbarer Referenz des gemeinsamen Animator Controller wird gewarnt und aus den Einstellungen neu aufgebaut. Auch von mehreren Avataren geteilte Referenzen bleiben erhalten.
- Bei Kompilierfehlern stoppt der Build gleich zu Beginn mit einer Erklärung. Automatische Bereinigung erzeugter Controller wartet, solange Skriptfehler die Referenzprüfung verhindern.
- Beim Hinzufügen wird das offizielle Prefab auch bei gleichnamigen Projekt-Prefabs bevorzugt. Nur abweichende Zeilenenden im gemeinsamen Controller verhindern die Vorberechnung nicht mehr.
- Write Defaults zusätzlicher FX-Layer, etwa für die Bodenhöhe, folgen den Avatareinstellungen.

Falls du die Vorberechnung verwendest, aktualisiere sie nach dem Update am Werkzeug in der Szene, speichere die Szene und erstelle und lade den Avatar erneut hoch.

## [3.1.0] - 2026-11-07

- Automatische Gliedmaßenanpassung (IK) an den Körperbau für unterstützte Posen sowie avatarspezifische Korrekturen und Schutz vor rückwärts knickenden Knien hinzugefügt.
- Position und Richtung sind im Fotomodus einstellbar; Synchronisierung und Sichtbarkeit des eigenen Kopfes sind wählbar.
- Bodenhöhe bewegt den gesamten Körper bis 3m auf oder ab, unabhängig von der Fußhöhe.
- Atmung, Unterdrückung des normalen Leerlaufschwankens und Korrektur von Halsrichtungsverformungen hinzugefügt.
- Funktionen und Parameterverbrauch im Inspector sichtbar; Posensuche, Mehrfachimport, Sortierung und einfache Scene-Vorschau ergänzt.
- Wechsel zwischen Stehen, Hocken und Sitzen im Desktop beschleunigt.
- Warnungen nach Vorberechnungsänderungen, Inspector-Scrollen und lange Bereinigungspausen verbessert.
- Falsche Aktualisierungswarnung und Neugenerierung beim Upload bei unveränderter Vorberechnung mit aktiver automatischer Übernahme behoben. Übernommene Daten gelten für den erzeugten Avatar, ohne die Importoptionen des Originals zu verändern.
- Umdrehen durch Kopfneigung und zugehörige Bewegungseinstellungen je Pose hinzugefügt.
- Übernahme alter Umdrehvoreinstellungen, eigener Bewegungsimport und Verwaltung erzeugter Assets verbessert.

## [3.1.0-beta.2] - Offene Beta (2026-11-05)

- Feste Länge sowie Höhen- (Y) und Vor-/Zurückabstand (Z) zwischen Head und Viewpoint als IK-Längenreferenzen ergänzt.
- Inspector-Warnungen auch links neben Hierarchy-Objektnamen angezeigt, einschließlich fehlender/veralteter Vorberechnung sowie Warnungen aus Bewegungsimport und Voreinstellungsaktionen.
- Lange Unity-Blockaden behoben, wenn die automatische Bereinigung etwa beim Szenenöffnen erzeugte Controller anderer Szenen lud.
- Verlust des Vorberechnungsverlaufs nach Posen- oder Funktionsänderungen behoben. Statt fehlender Ausführung wird nun eine Änderung nach der Vorberechnung gemeldet, auch mit Hierarchy-Warnung.
- Vorberechnungsvergleiche für Inspector und Hierarchy geteilt, um das Scrollen bei unveränderten Einstellungen und Referenzen zu entlasten. Beim Build wird weiterhin jedes Mal verglichen.

## [3.1.0-beta.1] - Offene Beta (2026-11-04)

- Normale Desktop-Haltungswechsel auf eine Auswertung umgestellt, für Build und Vorberechnung. VR und Sonderfälle behalten den bisherigen Ablauf.
- Vorberechnungen aus alpha.3 und älter als aktualisierungsbedürftig erkannt.
- Blickposition beim Ändern der Bodenhöhe im Fotomodus erhalten; danach Rückkehr zur zuletzt gewählten Bodenhöhe.

- Automatische Übernahme auf Erkennung beim Erzeugen beschränkt; Build und Vorberechnung verändern oder ergänzen keine manuellen Importoptionen.
- Aktivierung der Übernahme, Base-Änderungen und Aufheben manueller Auswahl werden bei Neugenerierung berücksichtigt. Produktübergreifende manuelle Einstellungen bleiben erhalten, alte importierte Bewegungen können zu Standardhaltungen zurückkehren.
- Erzeugte Vorberechnungscontroller in einer Datei je Avatar und Szenen-GUID verwaltet. Geteilte Referenzen durch Duplikate, andere Szenen oder Prefabs werden vor Updates getrennt.
- NDMF-Temporärassets beim Build genutzt, ohne zusätzliche Bearbeitungsdateien zu erzeugen.
- Erzeugte Daten beim Öffnen und Speichern geprüft. Ungespeicherte Änderungen, Undo/Redo und externe Referenzen bleiben geschützt; nur eindeutig zugeordnete unbenutzte Dateien gehen in den Papierkorb.
- Rotation, Anpassungsanimation und Abspielposition importierter Bewegungen angewendet. Rotation, RootT/RootQ und Humanoid-Anpassung mit normalen Posen vereinheitlicht.
- Anpassungen verschachtelter BlendTrees für das Importziel dupliziert, ohne Quellbewegung oder Geh-Mischung zu verändern.
- Abspielposition „nur Stehen“ bei ursprünglicher Bildrate zur Standpose aufgelöst, ohne die Geh-Wiedergabe anzuhalten.

## [3.1.0-alpha.2] - Geschlossener Test (2026-10-26)

### Korrekturen und Änderungen

- Korrekturen des Fotomodus und der Bodenhöhe auch auf normale/eigene Bewegungen ohne gewählte Werkzeugpose angewendet.
- Bodenhöhe auf 3m auf-/abwärts erweitert: 50% ist Original- und Anfangshöhe, 0% liegt 3m darunter, 100% 3m darüber.
- Veränderung des Quell-BlendTree beim Überschreiben von „nur Stehen“ behoben.
- Manuelle Korrektur für Posen ohne IK-Regeln genutzt und überflüssige Generierungswarnungen unterdrückt.
- Verwechslung von Anpassungsentwürfen gleichnamiger Posen und übersehene Bewegungsänderungen innerhalb eines Assets behoben.
- Falsche Anfangsauswahl über die Nummer einer deaktivierten Startpose behoben.
- Erklärung statt Ausnahme beim Import nicht unterstützter AnimatorOverrideController angezeigt.
- Absichtlich deaktiviertes Umdrehen beim Anwenden alter Voreinstellungen erhalten.
- Funktionsicons im hellen Editor-Design besser sichtbar.

Prüfungen der Blickposition und ersetzter Standardbewegungen im VRChat-Client laufen weiter.

## [3.1.0-alpha.1] - Vor Veröffentlichung

### Hinzugefügt

- Aktivierung je Funktion und ExpressionParameters-Verbrauch hinzugefügt.
- Fotomodus mit Vorwärtsbewegung und Drehung des Avatars hinzugefügt.
- Foto-Synchronisierung wählbar: 0 bit nur lokal, 16 bit synchronisiert.
- Sichtbarkeit des eigenen Kopfes im Fotomodus wählbar.
- Bodenhöhe für 3m Auf-/Abbewegung des ganzen Körpers einschließlich Kopf hinzugefügt: 50% ist Original- und Anfangshöhe, 0% 3m darunter, 100% 3m darüber.
- Umdrehen nach Kopfneigung und zugehörige Animationseinstellungen je Pose hinzugefügt.
- Atembewegung mit Schwach, Mittel, Stark, Schnell und Benutzerdefiniert hinzugefügt.
- Atembewegung und Ein-/Aus-Menü getrennt wählbar: 0 bit Bewegung allein, 1 bit für synchronisiertes Umschalten.
- Unterdrückung der normalen VRChat-Leerlaufbewegung hinzugefügt.
- Korrektur von Verformungen durch abweichende Halsrichtung hinzugefügt.
- Automatische Gliedmaßenanpassung (IK) für unterstützte Posen und globale/posenweise Aktivierung ergänzt.
- „IK in Anpassungen übernehmen“ speichert das IK-Ergebnis in einer Anpassungsanimation.
- Avatarspezifische Haltungskorrekturen, Schutz vor Rückwärtsknicken und minimale Kniebeugung ergänzt.
- Einfache Scene-Vorschau aus der Posenliste ergänzt.
- Kategorienregister, übergreifende Suche, Mehrfachimport und Zielreihenfolge hinzugefügt.
- Produkt- und Systemänderungsverlauf im Inspector ergänzt.

### Änderungen

- Voreinstellungen schneller geladen, mit weniger Wartezeit beim Hinzufügen und Öffnen der Liste.
- Liste nach Hinzufügen, Bearbeiten und Löschen automatisch erneuert.
- Inspector in 3 Register aufgeteilt: Funktionen, Posen und Avatareinrichtung.
- Erklärungen und Icons ergänzt und Aufklappen von Detaileinstellungen vereinheitlicht.
- Atemdetails in die Funktionen und Voreinstellungen ans Ende der Avatareinrichtung verschoben.
- Eigene Bewegungszuweisung von automatischer Übernahme ursprünglicher Haltungen getrennt.
- Doppelte Ganzkörperanimationen für Fußhöhe reduziert.
- Posenbildaktualisierung als Schaltfläche mit Bestätigung gestaltet.
- Warnoptionen unter „Einstellungen prüfen“ gesammelt.
- Neues Hinzufügen spezieller 8bit-Prefabs zugunsten der Funktionsauswahl im normalen Prefab entfernt; bestehende Referenzen erhalten.
- Minimales VRChat Avatars SDK auf 3.10.4 angehoben.

### Korrekturen

- Korrekturen des Fotomodus und der Bodenhöhe betreffen auch normale/eigene Bewegungen ohne Werkzeugpose.
- Änderung des Quell-BlendTree beim Überschreiben von „nur Stehen“ behoben.
- Manuelle Anpassung für Posen ohne gültige IK-Regeln verwendet, unnötige IK-Warnungen unterdrückt.
- Vermischte Anpassungen gleichnamiger Posen und nicht neu erzeugte Bewegungstausche innerhalb eines Assets behoben.
- Wiederverwendete Nummern deaktivierter Startposen lösen keine falsche Anfangsauswahl mehr aus.
- Hinweis statt Ausnahme bei nicht unterstützten AnimatorOverrideControllern in der Übernahme.
- Ausdrücklich ausgeschaltetes Umdrehen alter Voreinstellungen erhalten.
- Iconhintergründe für Erkennbarkeit im hellen Editor-Design verbessert.

- Fehlende Icons für Umdrehen, Bodenhöhe und Fotomodus beim Hinzufügen behoben.
- Verlorene Umdreheinstellungen unterstützter Posen nach alten Voreinstellungen behoben.
- Namenseingabe wechselt beim Öffnen anderer Details nicht mehr zu einer anderen Pose.
- Geschlossene Details nach Sortieren sowie Undo-/Redo-Probleme der Einstellungen behoben.
- Überlagerung des Originalavatars und doppelt addierte Platzierungsposition in der Vorschau behoben.
- Vorschaufehler bei fehlenden Komponenten behoben.
- IK-Ausrichtungs- und Positionsfehler durch unterschiedliche Knochenachsen und Skalierungen behoben.
- Warnungen und Vorberechnungsstatus nach Änderungen sofort aktualisiert.
- Veraltete Vorberechnung nach IK-Änderungen nicht mehr wiederverwendet.
- Build-Fehler beim Erzeugen zusätzlicher Funktionen in bestimmten Konfigurationen behoben.

## [3.0.16] - 2026-09-14
- Minimales NDMF auf 1.14.0 angehoben, da die API `VirtualAnimatorController.SetParameter` verwendet wird.

## [3.0.15] - 2026-09-13
- Starke Änderungen von Körperposition oder Kniebeugung beim Umschalten der Halsfixierung in Rückenlage bei Avataren mit geänderter Hips-Skalierung und FloorAdjuster behoben.

## [3.0.14] - 2026-09-09
- Verschwundene „Gesten aus“-Einträge ab dem zweiten Posenwerkzeug im selben Avatar behoben.
- Ungewollte Rückkehr von USSPS_Locomotion zu Standing trotz gewählter Pose wie SleepDown behoben.
- Höhen- und X/Z-Versatz mit FloorAdjuster korrigiert, einschließlich der Verarbeitungsreihenfolge von MA und AAO.
- Automatische Wiederherstellung des normalen PosingSystem_Locomotion bei gelöschtem vorberechnetem AnimatorController in Generated.
- Build-Fehler nach Entfernen aller Stehposen behoben.
- Voreinstellungen nach Avatarnamen sortiert.

## [3.0.13] - 2026-09-04
- Selbstübergang der Stehhaltung behoben, der beim sofortigen Wiederaufstehen nach Beginn des Hockens im VR-Modus die Haltungsrückkehr instabil machte.

## [3.0.12] - 2026-09-04
- Fälschlich erkannte Einstellungsänderung beim NDMF-Build ohne Vorberechnung behoben.
- Fehler `AddAssetToSameFile` beim Einfügen erzeugter Assets in einen temporären AnimatorController während des NDMF-Builds behoben.

## [3.0.11] - 2026-08-31
- Ungültige Scene des Vorberechnungsavatars und dadurch scheiternde Verarbeitung mit NDMF 1.14.7 und neuer behoben.
- `MissingReferenceException` durch bereits zerstörtes LocalizationAsset in der NDMF-Fehleranzeige behoben.

## [3.0.10] - 2026-08-29
- Voreinstellungsassets erst beim Öffnen der Auswahl geladen; Cache bei unverändertem GUID-Index wiederverwendet.
- Hierarchy-Warn-/Fehlericons nur bei Änderungen neu geprüft und Menü-Assetsuche gemeinsam zwischengespeichert. Periodische Editor-Blockaden in Szenen mit vielen Posenwerkzeugen verringert.
- `ArgumentException` durch doppelte Parameterregistrierung bei Vorberechnung mehrerer Werkzeuge auf einem Avatar behoben.

## [3.0.9] - 2026-08-26
- NullReferenceException verhindert, wenn VRCAvatarDescriptor während der Konvertierung nicht von PosingSystem ermittelt werden kann; auf nötige Build-Logs hingewiesen.
- Fehler `Avatar is null` durch Wiederverwendung von Vorschauen temporärer, von NDMF erzeugter und gelöschter Avatare in späterer Vorberechnung oder Build behoben. Konvertierung vom Vorschaucache getrennt und Namenskollisionen temporärer NDMF-Assets zwischen Vorschauen verhindert.
- Nicht gespeicherte Vorschauverweise des 「可愛いポーズ」-Prefabs und sich dadurch anhäufende unsichtbare Avatare behoben. Vorschauen von der Szenenspeicherung ausgeschlossen und alte Vorschauen beim Szenenladen, Play-Wechsel und Skriptneuladen bereinigt.

## [3.0.8] - 2026-08-18
- Unity Assertion und mögliche ungültige Posendaten bei erneuter Höhenkorrektur vorberechneter Bewegungen durch FloorAdjuster oder Ähnliches behoben.
- Höhenmessung gegen Eingriffe in Originalavatar oder AnimationMode anderer Werkzeuge abgesichert.

## [3.0.7] - 2026-08-12
- Blickversatz und übermäßige Kniebeugung in Rückenlage mit Höhenwerkzeugen wie FloorAdjuster behoben.
- Fehler bei PosingSystem außerhalb des Avatars behoben (Danke an PancakeTorto1se).
- Fortsetzung nach fehlgeschlagenem Avatar-Upload und Folgefehler verhindert.
- Falsch erkannte Änderungen direkt nach Vorberechnung behoben.
- Automatischen Import von Fortbewegungen verbessert.
- Warnungen für bestimmte besondere Avatar-Rigs ergänzt.
- Änderungsbestätigung und Spiegeln von Anpassungswerten im Animationsfenster ergänzt (Danke an kraihd).
- Hinweise auf Fehler anderer Werkzeuge und Prüfungen fehleranfälliger Stellen ergänzt.

## [3.0.6] - 2026-03-17
- Stark verzerrte Haltungen bestimmter Avatare mit eigener Haltungssteuerung behoben.

## [3.0.5] - 2026-01-31
- Unity-Abstürze mit bestimmten ExpressionMenus behoben.
- Avatarprobleme mit dem ごろ寝EX-Integrationsprefab behoben (Danke an あずき).
- Falsche Rotation beim Zusammenführen von Anpassungsanimationen behoben.

## [3.0.4] - 2026-01-11
- Verhalten von Avataren in Prefabs teilweise verbessert.
- Lizenzlöschung hinzugefügt.
- Halsprobleme zahlreicher Posen behoben.
- Öffnen des Inspectors etwas entlastet.
- Icons auch bei Vorberechnung erstellt und Text der Avatar-Aktualisierung geändert.
- Warnungen ohne jemals ausgeführte Vorberechnung ausgeblendet, da viele Nutzer sie offenbar nicht verwenden.
- Unnötige Ressourcen entfernt.
- Nicht anpassbare Finger korrigiert.

## [3.0.3] - 2026-01-04
- Anwendung einer anderen als der gewählten Voreinstellung behoben (Danke an あずき).

## [3.0.2] - 2026-01-02
- Dunkles Unity-Design unterstützt.
- Verzerrte Posen nach Rotation in der Animationsanpassung behoben.
- Größe erzeugter Menüicons geändert, Komprimierung aktiviert und noch kleinere Iconoption ergänzt.
- In 3.0.0 entfernte Tracking-Integration und automatische Übernahme von Avatarbewegungen wieder hinzugefügt.

## [3.0.1] - 2025-12-29
- Fehler der Standardbewegungseinstellungen behoben.
- Unbeabsichtigte Verformung von Avatarhaltungen behoben.

## [3.0.0] - 2025-12-28
- Vorberechnung hinzugefügt.
- Animationsanpassung hinzugefügt.
- Voreinstellungen hinzugefügt.

## [2.2.5] - 2025-05-08
- Kompatibilitätsprobleme mit bestimmten Systemen gelöst.

## [2.2.4] - 2025-04-04
- Nicht wechselnde Haltungen mit bestimmten Avataren und ModularAvatar-Versionen behoben.

## [2.2.3] - 2025-03-13
- Fehlerhafte Bewegungen einzelner Avatare, etwa beim Springen mit 森羅, behoben.
- Teilweise falschen Anfangswert von Gesten aus behoben.

## [2.2.2] - 2025-03-06
- Fehler in bestimmten Umgebungen behoben.

## [2.2.1] - 2025-02-05
- Unity-Absturz behoben.
- Version zum Werkzeugnamen in NDMF Console hinzugefügt.

## [2.2.0] - 2025-02-02
- Option zum Deaktivieren der Gesten zugunsten der Animationshandform ergänzt.

## [2.1.6] - 2025-02-02
- Beschädigte Posen zusammen mit bestimmten Werkzeugen behoben.
- Einstellungsabhängige Fehler behoben.

## [2.1.5] - 2024-12-10
- Konflikte mit bestimmten Systemen gelöst.
- Schaltfläche zum Entfernen der Unitypackage-Version ergänzt.

## [2.1.4] - 2024-12-08
- Fehler bestimmter Avatare behoben.
- Durch Konflikte unbenutzbare Fußfixierung behoben.
- Ungewollte Rückkehr zu VRC-Standardhaltungen im Desktop behoben.

## [2.1.3] - 2024-12-01
- Ausbleibende Posenwechsel mit Avataren/Systemen, die VRMode mit falschem Typ verwenden, behoben.
- Anzeige von Konflikten verschiedener Parametertypen zwischen Systemen in NDMF-Console ergänzt.

## [2.1.2] - 2024-12-01
- Erklärung in NDMF-Console ergänzt, wenn Fehler anderer Werkzeuge beim Avatar-Build einen Fehler im Unisaki Posing System verursachen.
- Build-Fehler bestimmter Avatare behoben.
- Verhalten direkt nach Avatar-Laden verbessert.
- Ursprüngliche Avatar-Fortbewegungslogik entfernt, da sie bei manchen Avataren die Funktion verhinderte.

## [2.1.1] - 2024-11-20
- MergeAnimator-Probleme in PosingOverride behoben.

## [2.1.0] - 2024-11-20
- Einsinken von Füßen und Hüfte im Desktop behoben, damit alle Haltungen sauber möglich sind.
- Haltungswechsel unabhängig von der Kopfhöhe ergänzt.
- Modus ohne Iconerzeugung für Quest-Avatare ergänzt.
- Warnungen bei Avatarproblemen hinzugefügt.
- Fixierungskonflikte mit AFK und Armfixierungen anderer Werkzeuge gelöst.
- Automatische Halsfixierung in Rückenlage ergänzt.

## [2.0.0] - 2024-11-12
- VPM/VCC-kompatible Version veröffentlicht.
- Werkzeugname in Fehleranzeigen geändert #1.
- Nutzungsbedingungen ergänzt #5.
