# Unisaki Posing System

Gemeinsames Posensystem für Kawaii Posing, Cool Posing und weitere Werkzeuge. Es erzeugt mit Modular Avatar und NDMF beim Avatar-Upload die erforderlichen Menüs und Animationen.

Diese README gilt für die in Entwicklung befindliche v3.2.0. Ein offizieller Veröffentlichungstermin steht noch nicht fest. Die installierte Version steht oben im Inspector des Posenwerkzeugs. Siehe auch [Erweiterungen und Änderungen](CHANGELOG.md).

## Anzeigesprache und Offline-Dokumentation

Wähle die Anzeigesprache oben im Inspector unter „Language / 言語“. Die Auswahl gilt zugleich für den Inspector und die erzeugten VRChat-Menüs. Lade den Avatar erneut hoch, um die Sprache im Spiel zu ändern.

Ohne ausdrückliche Auswahl wird die Computersprache verwendet; nicht unterstützte Sprachen verwenden Englisch. Windows, macOS und Linux werden unterstützt. Die letzte Auswahl wird in EditorPrefs gespeichert und für andere Projekte desselben Betriebssystembenutzers auf diesem Computer wiederverwendet. Eine bereits ausdrücklich im Avatar gespeicherte Auswahl bleibt erhalten. Andere Computer werden nicht synchronisiert. Die Lizenzspeicherung bleibt unverändert.

README, Änderungsverlauf und Anleitung sind über die mitgelieferte [Sprachübersicht](../index.html) erreichbar. Auch die Verweise auf Anleitung und Änderungen im Inspector folgen der Auswahl. Die ursprünglichen Nutzungsbedingungen und ihre Referenzübersetzungen bleiben separat verfügbar.

## Haltungswechsel im Desktopmodus

Die Verzögerung bis zur Anwendung der gewählten Pose beim Wechsel zwischen Stehen, Hocken und Sitzen wurde im Desktopmodus verkürzt. Das gilt für normale Builds und die Vorberechnung, ohne besondere Befehle oder zusätzliche Einstellungen. Im VR-Modus und in Sonderfällen bleibt der bisherige Ablauf erhalten. Aktualisiere ältere Vorberechnungen vor dem Upload.

## Installation und Aktualisierung

Füge das Posenwerkzeug mit VCC oder ALCOM gemäß den Dateien und Anweisungen deines Kaufs hinzu. Dieses System wird als Abhängigkeit des Posenwerkzeugs installiert.

Für v3.2.0 gelten folgende Anforderungen.

- VRChat-Avatarprojekt mit Unity 2022.3
- VRChat Avatars SDK 3.10.4 oder neuer
- Modular Avatar 1.x ab Version 1.15.1
- NDMF (Non-Destructive Modular Framework) 1.x ab Version 1.14.0

Wenn du Kawaii Posing verwendest, aktualisiere es in ALCOM / VCC auf 3.2.0 und prüfe, dass auch Unisaki Posing System auf 3.2.0 steht. Bei vorhandener Vorberechnung klicke auf „Vorberechnung aktualisieren“, speichere die Szene und lade den Avatar hoch.

Cool Posing 1.2.0 verwendet das gemeinsame System 3.2.0. Aktualisiere Produkt und gemeinsames System zusammen auf die passenden Versionen.

### Für bisherige Nutzer der 8bit-Version

Neue Installationen verwenden einheitlich das normale Prefab. Deaktiviere unnötige Optionen unter „Funktionen“, um den Verbrauch zu senken. Nur die Posenauswahl benötigt grundsätzlich 8 bit.

Die alten 8bit-Prefabs bleiben unter „Editor/Legacy“ der Produkte erhalten, damit Referenzen vorhandener Avatare und Voreinstellungen bestehen bleiben. Du musst ein bestehendes Prefab weder löschen noch ersetzen, um die 8bit-Version weiterzuverwenden. Im Unity-Menü zum Hinzufügen wird die 8bit-Version nicht mehr angeboten.

## Aufbau der Einstellungen

Wähle in der Hierarchy 「可愛いポーズ」, 「カッコいいポーズ」 oder das entsprechende Werkzeugobjekt im Avatar aus.

| Register | Einstellungen |
| --- | --- |
| Funktionen | Einträge im Spielmenü, Komfortfunktionen, Icons und ExpressionParameters-Verbrauch |
| Posen | Auswahl, Hinzufügen und Reihenfolge der Posen, Scene-Vorschau, Details und Animationsanpassung |
| Avatareinrichtung | Körper- und Kniekorrekturen, Übernahme ursprünglicher oder eigener Bewegungen und Voreinstellungen |

Bei Einträgen mit weiteren Optionen öffnet der Einstellungslink die Felder direkt darunter. Auch die Atmung wird unter „Funktionen“ eingestellt.

Über „Hilfe / Änderungen“ oben öffnest du Anleitung, Support und Änderungsverlauf. Dort kannst du zwischen dem Verlauf des Produkts und des gemeinsamen Unisaki Posing System wechseln.

## Funktionen und ExpressionParameters-Verbrauch

Die unter „Funktionen > Menüfunktionen“ gewählten Einträge erscheinen im Spielmenü. „Spiegeln“ ermöglicht beispielsweise das Umschalten in VRChat. Das Aktivieren im Inspector zwingt nicht alle Posen dauerhaft in eine gespiegelte Darstellung.

| Funktion | Zusätzliche Bits |
| --- | ---: |
| Spiegeln | 1 bit |
| Gesten aus | 1 bit |
| Atmung animieren | 0 bit |
| Atmung im Spiel ein-/ausschalten | 1 bit |
| Fußhöhe | 8 bit |
| Bodenhöhe | 8 bit |
| Fotomodus nur lokal | 0 bit |
| Fotomodus synchronisieren | 16 bit |
| Kopf im Fotomodus sichtbar halten | 0 bit |
| VRChat-Schwanken im Stand deaktivieren | 0 bit |
| Verformungen durch Kopfdrehung verringern | 1 bit (mit Umdrehen geteilt) |
| Umdrehen | 10 bit (davon 1 bit mit der Verformungskorrektur geteilt) |

Umdrehen benötigt 1 bit zum Ein-/Ausschalten, 8 bit für die Kopfneigung und 1 bit für die Halsfixierung. Ist die Verformungskorrektur bereits aktiv, kommen durch Umdrehen daher nur 9 bit hinzu. Der gemeinsame Anteil wird nicht doppelt gezählt; die Anzeige folgt den gewählten Optionen.

Der angezeigte Gesamtwert betrifft die Posenwerkzeuge dieses Avatars, nicht den gesamten Avatar samt Kleidung und anderen Systemen. Gemeinsame Einstellungen mehrerer Posenwerkzeuge werden nicht doppelt gezählt. Überschreitet die Anzahl der Posen den grundlegenden Nummernbereich, benötigt auch die Posenauswahl weitere Bits.

### Fußhöhe und Bodenhöhe

- „Fußhöhe“: verändert die Höhe auf der Fußseite bei gleichbleibender Kopfposition. Nutze sie, um die Füße an Boden oder Bett anzupassen.
- „Bodenhöhe“: bewegt den ganzen Körper einschließlich Kopf auf oder ab. 50% ist die ursprüngliche Höhe, 0% liegt 3m darunter und 100% 3m darüber. Der Anfangswert ist 50%.

Beide stehen im Spiel unter „Einstellungen“, zuerst Fußhöhe, danach Bodenhöhe. Bewege die Bodenhöhe aus der Mitte, um auf- oder abzusteigen. Nahe 50% kehrt der Avatar zur ursprünglichen Höhe zurück und beendet die Korrektur. Es gibt keinen zusätzlichen Schwebeschalter. Die Blickposition wird beim Beginn des Schwebens und während der Höhenbedienung korrigiert.

Die Fußhöhenanpassung erzeugt bei normalen Animationen keine doppelten Ganzkörperbewegungen mehr. Das verringert die erzeugten Animationsdaten, bedeutet aber nicht, dass jeder gesamte Avatar nur noch ein Drittel so groß ist.

### Fotomodus

1. Aktiviere „Funktionen > Menüfunktionen > Fotomodus“.
2. Öffne in VRChat „Fotomodus“ und wähle unter „Pose wählen“ eine Pose.
3. „Abstand“ bewegt den Avatar bis zu 3m vorwärts, „Richtung“ dreht ihn. Bei einem Abstand größer als 0 blickt er zu dir.
4. Aktiviere bei Bedarf „Füße fixieren“. Die Füße werden beim Start nicht zwangsläufig fixiert, damit du die Position vorher anpassen kannst. Ist „Gesten aus“ im Menü, kannst du hier auch die Handform umschalten.
5. „Foto beenden“ setzt Abstand und Richtung zurück und löst die Fußfixierung.

Bewegung und Drehung im Fotomodus benötigen 0 bit, wenn nur deine eigene Ansicht betroffen ist. Aktiviere „Komfortfunktionen > Fotomodus synchronisieren“, damit andere Position und Richtung sehen. Beides zusammen benötigt 16 bit.

Wenn der Kopf deines nahen Avatars verschwindet, aktiviere „Komfortfunktionen > Kopf im Fotomodus sichtbar halten“. Das gilt nur für deine Ansicht während des Fotografierens; danach kehrt die normale Darstellung zurück.

### Liegepose durch Springen wechseln

Sind Rücken- und Bauchlage beide ausgewählt, kannst du durch Springen auf der Stelle wechseln. Bei nur einer ausgewählten Lage bleibt diese erhalten. Deaktiviere „Liegepose durch Springen wechseln“ unter „Funktionen → Komfortfunktionen“ in Unity, wenn du dies nicht möchtest. Der zusätzliche synchronisierte Verbrauch beträgt 0 bit. Bei mehreren Posenwerkzeugen gilt die Deaktivierung in einem davon für den gesamten Avatar.

### Umdrehen

In unterstützten Liegeposen rollt der Körper sanft mit, wenn du den Kopf seitlich neigst. Nicht jede Liegepose wird dadurch automatisch unterstützt.

1. Aktiviere „Funktionen > Menüfunktionen > Umdrehen“.
2. Öffne unter „Posen“ die „Details“ der gewünschten Liegepose und aktiviere „Umdrehen aktivieren“.
3. Weise „Seitliche Umdrehpose“ eine Seitenlageanimation zu. Aus Originalpose und Seitenlage werden die Drehungen nach links und rechts erzeugt. Passe bei Bedarf die „Stärke der Bodenkorrektur“ an.
4. Wähle in VRChat die unterstützte Pose und aktiviere „Umdrehen“. Löse dafür die Halsfixierung.

Bei Posen mit Umdrehen steuert die Kopfneigung die Abspielposition, deshalb ist die normale MotionTime-Einstellung nicht verfügbar. Für eine fertige Umdrehanimation weise sie dem normalen Animationsfeld zu und lasse „Seitliche Umdrehpose“ leer. Die Animation muss bei 0 links, bei 0.5 auf dem Rücken und bei 1 rechts liegen.

### Atmung und normales Schwanken

„Funktionen > Komfortfunktionen > Atmung animieren“ bewegt Brust und weitere Körperteile mit der Atmung. Öffne die Einstellungen und wähle je Posenkategorie Keine, Schwach, Mittel, Stark, Schnell oder Benutzerdefiniert. Schnell ist eine beschleunigte Atmung; Benutzerdefiniert verwendet deine eigene Atemanimation.

Die Atembewegung allein benötigt 0 bit. Aktiviere zusätzlich „Menüfunktionen > Atmung“, um sie in VRChat anzuhalten oder fortzusetzen. Das synchronisierte Ein-/Ausschalten benötigt 1 bit. Eine andere Stärke erhöht den Bitverbrauch nicht.

Hals, Schultern und Arme bewegen sich mit der Brust, um Kopf- und Handbewegungen zu verringern. Die Positionen sind nicht vollständig fixiert; wähle die Stärke passend zu Körperbau und Pose.

„Komfortfunktionen > VRChat-Schwanken im Stand deaktivieren“ unterdrückt die standardmäßige Körperbewegung im Leerlauf. Die Option ist anfangs aus und erhält die Atmung des Posenwerkzeugs sowie andere eigene Bewegungen. Sie ist auch ohne Atembewegung nutzbar und benötigt keine weiteren Bits. Schwanken mit anderen Ursachen als der normalen Leerlaufbewegung wird nicht unterdrückt.

Bei mehreren Posenwerkzeugen gilt diese Unterdrückung für den ganzen Avatar, sobald sie in einem Werkzeug aktiv ist. Zum Wiederherstellen deaktiviere die Option in allen Werkzeugen und lade erneut hoch.

### Verformungen durch Kopfdrehung verringern

Nutze diese Option, wenn sich die Pose nach dem Lösen der Halsfixierung wegen einer anderen Halsrichtung als im Original verformt. Sie steht unter „Funktionen > Komfortfunktionen“ und fügt kein eigenes Spielmenü hinzu. Bei fixiertem Hals gilt die ursprüngliche Richtung. Rückenlagen sind ausgenommen, damit ihre ursprüngliche Ausrichtung nach oben erhalten bleibt.

### Menü-Icons

- „Posen-Icons erzeugen“: erstellt Bilder deines Avatars in den Posen und zeigt sie im Menü. Das erleichtert die Auswahl, verlängert aber den Build um die Bilderzeugung.
- „Icongröße verringern“: senkt die Bildauflösung und damit die Avatargröße.

Android-Builds erzeugen keine Posen-Icons. Nach Änderungen etwa an der Kleidung verwende „Icons neu aufnehmen“. Die Bilder der Posenliste werden mit „Posenvorschaubilder aktualisieren“ erneuert. Lies vor dem Start die Bestätigung, da diese Vorgänge Zeit benötigen.

## Posen hinzufügen, sortieren und ansehen

Wähle links unter „Posen“ eine Kategorie. Das oberste Register mit der Lupe sucht Namen über alle Kategorien hinweg. „Details“ enthält Animation, Icon, MotionTime und weitere Einstellungen.

Mehrere Animationen kannst du in den Hinzufügebereich ziehen oder in Project auswählen und mit „In Project gewählte Clips übernehmen“ gemeinsam hinzufügen. Doppelte Referenzen auf denselben Clip werden entfernt.

Wähle zum Sortieren das Ziel unter „Details > Reihenfolge“. Auch die Auf-/Ab-Schaltflächen sind nutzbar. Der geöffnete oder geschlossene Zustand der Details bleibt beim Verschieben erhalten.

„Vorschau“ zeigt jede Pose in der Scene, ohne das Anpassungsfenster zu öffnen. Nach dem Beenden erscheint wieder der Originalzustand. Diese einfache Vorschau führt keinen MA- oder NDMF-Build aus und kann daher von später integrierter Kleidung oder etwa FloorAdjuster abweichen. Prüfe Kleidung und Bodenkontakt auch nach dem Avatar-Build.

## Animationsanpassung und automatische Gliedmaßenanpassung (IK)

Öffne unter „Posen“ die „Animationsanpassung“, um Körper und Gliedmaßen mit Vorschau genau einzustellen. Speichere die Anpassungsanimation anschließend über die Speicherbefehle des Fensters.

Bei unterstützten Produkten und Posen passt die Automatik die Gliedmaßen an den Körperbau an. „Automatische Gliedmaßenanpassung aktivieren“ im Anpassungsfenster schaltet sie insgesamt um; „IK für diese Pose aktivieren“ gilt nur für eine Pose. Fehlt eine automatische Anpassung, wird dies angezeigt.

Zum manuellen Verfeinern eines automatischen Ergebnisses klicke auf „IK in Anpassungen übernehmen“. Der aktuelle IK-Zustand wird in eine neue Anpassungsanimation geschrieben und die IK dieser Pose für diesen Avatar deaktiviert. Die Originalanimation bleibt erhalten. Der Befehl ist verfügbar, wenn globale und posenspezifische Automatik aktiv sind und anwendbare IK-Einstellungen vorliegen.

Unter „Avatareinrichtung“ schaltet „Rückwärtsknicken der Knie verhindern“ die Kniekorrektur um. Stelle den Winkel in den Details unter „Minimale Kniebeugung (Grad)“ ein. „Avatarspezifische Korrektur aktivieren“ ist mit passenden Korrekturdaten verfügbar. Manche Korrekturen lassen sich nicht mit der automatischen Gliedmaßenanpassung kombinieren.

## Ursprüngliche und eigene Bewegungen sowie Voreinstellungen

Diese Optionen stehen unter „Avatareinrichtung“.

- „Ursprüngliche Steh-, Hock- und Bauchlage verwenden“: übernimmt die ursprünglich im Avatar eingestellten Haltungen automatisch.
- „Eigene Bewegungen verwenden“: öffnet getrennte Importoptionen für Bewegungsarten wie Stehen, Hocken (Crouching) und Bauchlage (Prone). Weise grundsätzlich eine Bewegung je Art zu.
- „Tracking-Fixierungen mit anderen Werkzeugen integrieren“: aktiviere dies, wenn Fixierungen zusammen mit anderen Systemen nicht richtig funktionieren.

Die manuelle Zuweisung eigener Bewegungen ist von der automatischen Übernahme ursprünglicher Haltungen getrennt.

Voreinstellungen stehen unten im Register. Suche nach einem Namen und ersetze die Poseneinstellungen mit „Diese Voreinstellung anwenden“. Sie dienen vor allem der Ersteinrichtung und ersetzen den aktuellen Zustand. Speichere deshalb vorhandene Anpassungen vorher.

Für die Liste werden nur Namen und Informationen zu passenden Avataren geladen. Anpassungsanimationen werden erst beim Anwenden der gewählten Voreinstellung geladen. Auch die erste Listenerstellung lädt nicht sämtliche Avataranimationen vor.

Die Liste unterscheidet „Mit Avatar kompatibel“, „Eigene Voreinstellungen“ und „Offizielle Unisaki Studio-Voreinstellungen“. „Mitgelieferte Voreinstellungen ausblenden“ gilt projektweit. Hinzufügen, Bearbeiten und Löschen aktualisiert die Liste; verwende „Neu laden“, falls sich die Anzeige nicht ändert.

## Vorberechnung und Warnungen

Ist „Ursprüngliche Steh-, Hock- und Bauchlage verwenden“ aktiv, werden bei jeder Erzeugung Bewegungen aus Base erkannt. Das Ergebnis wird nicht in manuelle Importoptionen zurückgeschrieben; allein bei automatischer Übernahme bleibt die Liste daher ohne Zuweisung. Änderungen an Base oder der Aktivierung der Übernahme gelten bei der nächsten Erzeugung.

Beim Upload wirkt die automatische Übernahme auf die Build-Kopie. Originalobjekte der Szene, Importoptionen, Quellcontroller und Bewegungen bleiben unverändert. Bei unveränderten Eingaben wird das vorhandene Vorberechnungsergebnis wiederverwendet.

Manuelle Zuweisungen haben für ihre Bewegungsart Vorrang; Rotation, Anpassungsanimation und Abspielposition werden übernommen. Deaktiviertes „Verwenden“ oder das Entfernen einer zugewiesenen Bewegung stoppt die Automatik für diese Art. „Manuelle Auswahl aufheben“ entfernt Einstellungen und Anpassungen; bei aktiver Automatik wird die Art beim nächsten Erzeugen neu erkannt. Undo kann dies rückgängig machen. Bei mehreren alten Einstellungen derselben Art gilt die im Inspector gewählte aktive Einstellung.

Von der alten Automatik in die Liste geschriebene Zuweisungen werden nicht automatisch gelöscht, da sie nicht von manuellen Einträgen und Anpassungen unterscheidbar sind. Verwende „Manuelle Auswahl aufheben“ nur für Arten, die wieder automatisch erkannt werden sollen. Bei mehreren Produkten im Avatar beeinflussen auch deren manuelle Zuweisungen die gemeinsamen ursprünglichen Haltungen.

Die Vorberechnung verkürzt Avatar-Builds durch Wiederverwendung erzeugter Daten. Erstelle oder erneuere sie mit „Vorberechnung aktualisieren“. Ein normaler Build erzeugt ebenfalls alle nötigen Daten; eine manuelle Vorberechnung ist nicht jedes Mal erforderlich.

Nach Einstellungsänderungen können Aktualisierungen nötig sein. „Einstellungen prüfen“ oben im Inspector und Markierungen in der Hierarchy weisen darauf hin. Aktualisiere nach Posen- oder IK-Änderungen die Vorberechnung, um folgende Builds zu beschleunigen.

„Warnanzeige-Einstellungen“ steht im Bereich „Einstellungen prüfen“. Warnungsarten und Hierarchy-Markierungen lassen sich ausblenden. Das behebt keine Konfigurationsfehler und aktualisiert keine erzeugten Daten. Fehler und erforderliche Generierung bleiben aktiv.

## Nutzungshinweise

- Für Dreipunkt-Tracking und Desktop vorgesehen. Posenfixierung mit aktivem Ganzkörper-Tracking wird nicht unterstützt.
- Körperbau und Anpassungen können weitere Korrekturen für Gliedmaßenkontakt und Kleidung erfordern.
- Besondere Base-Layer oder andere Haltungssysteme können unerwartete Ergebnisse liefern.
- Bewegung und Drehung im Fotomodus sowie Bodenhöhe korrigieren Werkzeugposen und normale oder eigene Bewegungen ohne gewählte Pose. Prüfe im VRChat-Client ersetzte Standardbewegungen und Blickverhalten.

## Nutzungsbedingungen und Support

Quellcode und Ressourcen dieses Repositorys sind nicht Open Source. Die Nutzung erfordert einen regulären Kauf und die Zustimmung zu den [mitgelieferten Nutzungsbedingungen](../../../%E3%82%86%E3%81%AB%E3%81%95%E3%81%8D%E3%83%9D%E3%83%BC%E3%82%BA%E3%82%B7%E3%82%B9%E3%83%86%E3%83%A0%E5%88%A9%E7%94%A8%E8%A6%8F%E7%B4%84%EF%BC%88%E6%97%A5%E6%9C%AC%E8%AA%9E%E7%89%88%EF%BC%89.pdf).

- [Kawaii Posing-Anleitung](https://docs.unisakistudio.jp/kawaiiposing/manual/)
- [Cool Posing-Anleitung](https://docs.unisakistudio.jp/coolposing/manual/)
- [Support](https://help.unisakistudio.jp/)

Gib bei Fehlermeldungen Produkt- und Systemversion, die Versionen von Unity, VRChat SDK, Modular Avatar und NDMF, den Avatar, Reproduktionsschritte und den Console-Fehler an.
