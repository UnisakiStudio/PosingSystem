# Historia zmian

## [3.2.0] - Niewydane

### Dodano

- Wyświetlanie Inspectora, operacji edycji, okien instalacji, standardowych nazw poz i generowanego menu VRChat w 30 językach.
- Połączono wybór języka Inspectora i menu gry. Domyślnie używany jest język komputera, a ostatni wybór dotyczy także innych projektów tego samego użytkownika systemu.
- README, historia zmian i instrukcja offline otwierają się w wybranym języku.
- Przygotowano nazwy wszystkich 128 standardowych poz i opisy funkcji zgodnie z dotychczasową terminologią.

### Zgodność

- Zachowywane są istniejące ustawienia awatara, własne nazwy, odwołania do zasobów, identyfikatory parametrów i sposób przechowywania licencji.

## [3.1.4] - 2026-11-11

- Naprawiono błąd, przez który animacje przypisane do własnych pozycji kucania i leżenia na brzuchu awatara nie były stosowane, a zamiast nich używano domyślnych pozycji VRChat. Poprawka obejmuje import automatyczny i przypisania ręczne.
- Naprawiono pozostawanie włosów, okularów i innych elementów dołączonych przez MA Bone Proxy w pierwotnym miejscu podczas edycji i podglądu pozy. Teraz podążają za pozą podglądu przy zachowaniu ustawień i rozmieszczenia oryginalnego awatara.
- Naprawiono błąd korekty pozy na podstawie położenia punktu widzenia podczas wykonywania obrazów takich poz jak „仰向け寝_お疲れ”. Obrazy korzystają z animacji uwzględniających korektę pozy i regulację podłogi stosowane podczas budowania.

Zaktualizuj Unisaki Posing System do 3.1.4 w ALCOM / VCC. Kawaii Posing Tool 3.1.2 i Cool Posing Tool 1.1.0 mogą pozostać bez zmian. Jeśli korzystano z budowania wstępnego, po aktualizacji naciśnij „Aktualizuj budowanie wstępne”, zapisz scenę, a następnie ponownie zbuduj i prześlij awatara.

## [3.1.3] - 2026-11-09

- Polecenie „Odśwież miniatury poz” oraz budowanie wstępne na kopii awatara nie usuwają już obrazów poz oryginalnego awatara.
- Naprawiono pojawianie się komunikatu „Ustawienia zmienione” po samym ponownym wykonaniu obrazów, jeśli budowanie wstępne odbyło się przed pierwszym zapisem sceny lub w trybie edycji Prefab.
- Naprawiono błąd, przez który dane poz pozostałe z poprzedniego budowania wstępnego mogły przerwać kolejne budowanie wstępne.
- Zmniejszono obciążenie podczas otwierania listy, gdy nie ma jeszcze obrazów poz. Zmieniono także obsługę, która na początku budowania wstępnego lub zwykłego blokowała korzystanie z Unity do zakończenia pobierania informacji o pakietach.
- Ustawienia edycji narzędzi poz wyłączonych wewnątrz awatara są teraz także usuwane podczas budowania dla PC i Androida.
- „Zapytaj o ten błąd” uwzględnia teraz w szkicu kody diagnostyczne i inne dane z dalszych komunikatów, nawet jeśli na początku Console są błędy o nierozpoznanej przyczynie. Przy błędzie ustawień klipu obracania się szkic zawiera też miejsca do sprawdzenia.

Zaktualizuj Unisaki Posing System do 3.1.3 w ALCOM / VCC. Kawaii Posing Tool 3.1.2 i Cool Posing Tool 1.1.0 mogą pozostać bez zmian. Podczas aktualizacji może pojawić się potwierdzenie konfliktu, ponieważ produkty wskazują wcześniejszą wspólną wersję systemu. Jeśli korzystano z budowania wstępnego, po aktualizacji naciśnij „Aktualizuj budowanie wstępne”, zapisz scenę, a następnie ponownie zbuduj i prześlij awatara.

## [3.1.2] - 2026-11-09

- Naprawiono pozostawanie twarzy skierowanej w górę po wstaniu z leżenia na brzuchu, gdy ograniczenie deformacji przy obrocie głowy jest włączone.
- Blokadę głowy można przełączać w „Ustawienia → Automatyczna blokada głowy na plecach” menu EX nawet po wyłączeniu wysokości stóp. Pozycja znikająca w 3.1.0 i 3.1.1 wraca po zbudowaniu zaktualizowanego awatara.
- Aktualizuj budowanie wstępne działa również po odznaczeniu głównego obiektu awatara w Unity.
- Naprawiono brak generowania poz po aktualizacji, gdy stary wspólny Controller poz pozostawał w Assets. Zachowywane są także własne zmiany Controllera.
- Jeśli wyłączone narzędzie lub jego rodzic uniemożliwia użycie wspólnego Animatora poz, pokazywana jest nazwa celu. Można go otworzyć przez „Sprawdź ustawienia → Wybierz cel”.
- Gdy budowanie wstępne zgłasza zmianę ustawień, różnice można sprawdzić w „Szczegóły → Pokaż zmiany od budowania wstępnego”.
- Gdy błąd skryptu zatrzyma budowanie (PS050), pokazywanych jest do 5 oryginalnych błędów Unity. Zapytaj o ten błąd otwiera szkic z wersjami Unity i obsługiwanych narzędzi oraz numerem błędu, który można sprawdzić, edytować i wysłać.

Jeśli korzystano z budowania wstępnego, po aktualizacji naciśnij Aktualizuj budowanie wstępne, zapisz scenę, a następnie ponownie zbuduj i prześlij awatara.

## [3.1.1] - 2026-11-08

- Włączone Wysokość ziemi i Obróć się są teraz widoczne w Ustawieniach menu EX VRChat także po wyłączeniu Odbicia.
- Po przestawieniu, usunięciu lub zmianie nazw poz w starej wersji zachowywane są właściwe obroty dla Nogi w przód, Relaks i Jedno kolano w górę. Działa to także po pełnym Unpack, jeśli można rozpoznać oryginalną pozę. Własne pozy, samodzielnie wyłączone obracanie i zastąpione ruchy boczne pozostają.
- Naprawiono chwilową pozycję na brzuchu podczas wybierania awatara w VRChat. Naprawiono też chwilowe leżenie na plecach przy skoku, gdy wybrano tylko brzuch. Przełączanie skokiem można wyłączyć przez „Funkcje → Udogodnienia → Zmieniaj pozycję leżącą skokiem”. Domyślnie jest włączone.
- Jeśli po aktualizacji odłączy się odwołanie do wspólnego Animator Controllera poz i da się je odtworzyć, pokazywane jest ostrzeżenie, a Controller powstaje ponownie z ustawień. Zachowywane są odwołania do Controllera wspólnego dla kilku awatarów.
- Błędy kompilacji zatrzymują budowanie na samym początku z wyjaśnieniem przyczyny. Dopóki błąd skryptu uniemożliwia sprawdzenie odwołań, automatyczne porządkowanie wygenerowanych Controllerów jest wstrzymane.
- Podczas dodawania narzędzia wybierany jest oficjalny Prefab, nawet gdy projekt ma inny o takiej samej nazwie. Budowanie wstępne trwa także wtedy, gdy wspólny Controller różni się tylko zakończeniami linii.
- Write Defaults dodawanych przez narzędzie warstw FX, np. wysokości ziemi, jest dopasowywane do ustawień awatara.

Jeśli korzystano z budowania wstępnego, po aktualizacji naciśnij Aktualizuj budowanie wstępne na narzędziu w scenie, zapisz scenę, a następnie ponownie zbuduj i prześlij awatara.

## [3.1.0] - 2026-11-07

- Dodano automatyczną korekcję kończyn (IK) obsługiwanych poz do budowy ciała, korekcję awatara i zapobieganie wyginaniu kolan do tyłu.
- Tryb zdjęć pozwala zmieniać pozycję i kierunek awatara. Można wybrać synchronizację pozycji i kierunku oraz widoczność własnej głowy podczas zdjęć.
- Wysokość ziemi przesuwa całe ciało do 3m w górę lub w dół, uzupełniając wysokość stóp.
- Dodano oddech, wyłączanie standardowego kołysania w spoczynku i ograniczanie deformacji pozy od kierunku głowy.
- Używane funkcje i zużycie parametrów są widoczne w Inspectorze. Dodano wyszukiwanie poz, dodawanie zbiorcze, sortowanie i uproszczony podgląd w Scene.
- Przyspieszono zmiany między staniem, kucaniem i siedzeniem w trybie pulpitu.
- Poprawiono ostrzeżenia po zmianie ustawień budowania wstępnego, obciążenie przewijaniem Inspectora i długie zatrzymania przy porządkowaniu zasobów.
- Naprawiono ostrzeżenie o nieaktualnym wyniku bez zmiany ustawień oraz ponowne generowanie przy przesyłaniu z włączonym automatycznym importem. Wynik importu dotyczy tworzonego awatara, nie zmienia ustawień importu oryginalnego obiektu.
- Dodano obracanie sterowane przechyleniem głowy i przypisanie odpowiednich ruchów dla każdej pozy.
- Poprawiono zachowywanie obrotów starych presetów, import własnych ruchów i zarządzanie tworzonymi zasobami.

## [3.1.0-beta.2] - Otwarta beta (2026-11-05)

- Do podstaw długości pozycji IK dodano stałą długość oraz różnicę wysokości (Y) i głębokości (Z) między Head a Viewpoint.
- Ostrzeżenia Inspectora są widoczne także po lewej stronie nazwy obiektu w Hierarchy. Oprócz braku budowania wstępnego i zmiany ustawień obejmuje to ostrzeżenia importu ruchów i operacji presetów.
- Naprawiono długie zawieszenie Unity przy automatycznym porządkowaniu, np. po otwarciu sceny, wskutek wczytywania utworzonych Controllerów innych scen.
- Naprawiono znikanie historii budowania wstępnego po zmianie listy poz lub funkcji. Zamiast braku wykonania pokazywana jest zmiana ustawień po budowaniu wstępnym, także jako ostrzeżenie aktualizacji w Hierarchy.
- Inspector i Hierarchy współdzielą wyniki porównania budowania wstępnego, zmniejszając koszt przewijania, dopóki ustawienia i odwoływane zasoby są niezmienione. Przy budowaniu porównanie nadal odbywa się za każdym razem.

## [3.1.0-beta.1] - Otwarta beta (2026-11-04)

- Zwykłe przełączanie poz w trybie pulpitu używa jednego obliczenia. Działa przy zwykłym budowaniu i budowaniu wstępnym; VR i szczególne warunki zachowują dotychczasową ścieżkę.
- Wyniki budowania wstępnego z alpha.3 i starszych wersji są rozpoznawane jako wymagające aktualizacji.
- Punkt widzenia pozostaje zachowany przy zmianie wysokości ziemi podczas zdjęć, a po zakończeniu wraca do ostatniej wysokości ziemi.

- Automatyczny import wykrywa teraz ruchy tylko przy generowaniu. Budowanie zwykłe i wstępne nie dodają ani nie zmieniają ręcznych ustawień importu.
- Przełączanie automatycznego importu, zmiany Base i czyszczenie ustawień ręcznych są uwzględniane przy ponownym generowaniu. Wspólne dla kilku produktów ręczne przypisania i korekcje pozostają, a stare importowane ruchy wracają do standardowych poz.
- Wygenerowane Controllery budowania wstępnego są zarządzane w jednym pliku na awatar z podziałem według GUID sceny. Wspólne odwołania kopii, innych scen i Prefabów są rozdzielane przed aktualizacją.
- Budowanie korzysta z zarządzania zasobami tymczasowymi NDMF bez tworzenia dodatkowych plików przeznaczonych do edycji.
- Zasoby są sprawdzane przy otwieraniu i zapisie sceny. Niezapisane zmiany, Undo/Redo lub zewnętrzne odwołania powodują ich zachowanie. Do kosza trafiają tylko nieużywane pliki o jednoznacznym właścicielu.
- Uwzględniono obrót importowanych ruchów, animację korekcji i pozycję odtwarzania. Obsługa obrotu, RootT/RootQ i korekcji Humanoid jest wspólna ze zwykłymi pozami.
- Dostosowania zagnieżdżonych BlendTree są kopiowane dla danego celu z zachowaniem oryginalnych ruchów i ustawień mieszania chodu.
- Pozycja odtwarzania Tylko stanie jest rozwijana do nieruchomej pozy przy oryginalnej liczbie klatek, bez zatrzymywania odtwarzania chodu.

## [3.1.0-alpha.2] - Zamknięta wersja testowa (2026-10-26)

### Poprawki i zmiany

- Korekcje trybu zdjęć i wysokości ziemi obejmują też standardowe oraz własne ruchy bez wybranej pozy narzędzia.
- Zakres wysokości ziemi rozszerzono do 3m w górę i w dół. 50% jest pierwotną wysokością i wartością początkową, 0% — o 3m niżej, 100% — o 3m wyżej.
- Naprawiono modyfikowanie źródłowego BlendTree przy zastępowaniu Tylko stanie.
- Pozy bez reguł IK korzystają z korekcji ręcznej, a zbędne ostrzeżenia generowania są pomijane.
- Naprawiono mieszanie szkiców korekcji poz o tej samej nazwie i pomijanie zmian ruchu wewnątrz jednego zasobu.
- Naprawiono wybór innej pozy początkowej przez numer nieprawidłowej pozy.
- Import nieobsługiwanego AnimatorOverrideController pokazuje instrukcję zamiast wyjątku.
- Zastosowanie starych presetów zachowuje jawnie wyłączone obracanie.
- Poprawiono widoczność ikon funkcji w jasnym motywie Editora.

Trwa sprawdzanie punktu widzenia i działania po zastąpieniu standardowych ruchów w kliencie VRChat.

## [3.1.0-alpha.1] - Przed publikacją

### Dodano

- Dodano przełączniki poszczególnych funkcji i wyświetlanie zużycia ExpressionParameters.
- Dodano tryb zdjęć do przesuwania awatara do przodu i obracania.
- Dodano wybór synchronizacji trybu zdjęć: tylko lokalnie — 0 bit, synchronizacja — 16 bit.
- Dodano zachowanie widoczności własnej głowy w trybie zdjęć.
- Dodano Wysokość ziemi, przesuwającą całe ciało wraz z głową do 3m w górę lub w dół. 50% jest pierwotną wysokością i wartością początkową, 0% — o 3m niżej, 100% — o 3m wyżej.
- Dodano obracanie zgodnie z przechyleniem głowy i przypisanie animacji dla każdej pozy.
- Dodano ruch oddechu i ustawienia Słabe, Średnie, Silne, Szybkie oraz Własne.
- Dodano osobny wybór ruchu oddechu i przełącznika w grze. Sam ruch — 0 bit, synchronizacja przełącznika — 1 bit.
- Dodano wyłączenie standardowego kołysania w spoczynku VRChat.
- Dodano ograniczenie deformacji pozy wynikającej z różnic kierunku głowy.
- Dodano automatyczną korekcję kończyn (IK) obsługiwanych poz do budowy ciała oraz przełącznik ogólny i dla każdej pozy.
- Dodano Wypal IK do korekcji, zapisujące wynik IK w animacji korekcji.
- Dodano korekcję awatara, zapobieganie wyginaniu kolan do tyłu i minimalny kąt zgięcia.
- Dodano uproszczony podgląd pozy z listy w Scene.
- Dodano karty kategorii, wyszukiwanie między kategoriami, zbiorcze dodawanie animacji i wybór docelowego miejsca w kolejności.
- Dodano historię zmian produktu i wspólnego systemu w Inspectorze.

### Zmieniono

- Przyspieszono wczytywanie presetów, skracając oczekiwanie przy dodawaniu narzędzia i otwieraniu listy.
- Dodawanie, edycja i usuwanie presetów automatycznie odświeża listę.
- Inspector podzielono na 3 karty: Funkcje, Pozy i Konfiguracja awatara.
- Dodano opisy i ikony ustawień oraz ujednolicono rozwijanie szczegółów.
- Szczegóły oddechu przeniesiono do funkcji, a presety na koniec konfiguracji awatara.
- Rozdzielono indywidualne przypisania własnych ruchów i automatyczny import oryginalnych poz awatara.
- Ograniczono powielanie animacji całego ciała tworzonych do wysokości stóp.
- Odświeżanie obrazów poz zmieniono na przycisk z potwierdzeniem.
- Opcje ostrzeżeń zebrano w Sprawdź ustawienia.
- Usunięto nowe dodawanie osobnego Prefabu 8bit, zastępując je wyborem funkcji zwykłego Prefabu. Istniejące odwołania pozostają.
- Minimalną wersję VRChat Avatars SDK zmieniono na 3.10.4.

### Naprawiono

- Standardowe i własne ruchy bez wybranej pozy narzędzia również obejmuje korekcja trybu zdjęć i wysokości ziemi.
- Naprawiono modyfikowanie źródłowego BlendTree przy zastępowaniu Tylko stanie.
- Przy braku poprawnych reguł IK używana jest korekcja ręczna, a zbędne ostrzeżenia generowania IK są pomijane.
- Naprawiono mieszanie korekcji poz o tej samej nazwie oraz brak uwzględnienia wymiany ruchu wewnątrz tego samego zasobu przy ponownym generowaniu.
- Numer wyłączonej pozy początkowej użyty przez inną pozę nie powoduje już jej początkowego wybrania.
- Wykrycie nieobsługiwanego AnimatorOverrideController przy imporcie oryginalnych poz pokazuje instrukcję zamiast wyjątku.
- Zastosowanie starych presetów zachowuje jawnie wyłączone obracanie.
- Poprawiono tło ikon Inspectora, aby funkcje były rozpoznawalne także w jasnym motywie Editora.

- Naprawiono brak ikon obracania, wysokości ziemi i trybu zdjęć przy dodawaniu do awatara.
- Naprawiono utratę ustawień obracania obsługiwanych poz po użyciu presetu starej wersji.
- Naprawiono przechodzenie wpisywanej nazwy do innej pozy po otwarciu jej szczegółów w trakcie edycji.
- Naprawiono zamykanie szczegółów przy sortowaniu oraz Undo/Redo zmian ustawień.
- Naprawiono nakładanie się oryginalnego awatara w podglądzie i dwukrotne dodawanie pozycji.
- Naprawiono błąd podglądu awatarów z brakującymi komponentami.
- Naprawiono odchylenia kierunku i położenia kończyn IK wynikające z różnic orientacji i skali kości awatara.
- Naprawiono nieodświeżanie ostrzeżeń i stanu budowania wstępnego tuż po zmianie ustawień.
- Naprawiono używanie starego wyniku budowania wstępnego po zmianie IK.
- Naprawiono błędy budowania blokujące generowanie dodatkowych funkcji w niektórych konfiguracjach.

## [3.0.16] - 2026-09-14
- Minimalną zależność NDMF zwiększono do 1.14.0 ze względu na użycie API `VirtualAnimatorController.SetParameter`

## [3.0.15] - 2026-09-13
- Naprawiono znaczne zmiany położenia ciała i zgięcia kolan przy przełączaniu blokady głowy na plecach w awatarach ze zmienioną skalą Hips i FloorAdjuster

## [3.0.14] - 2026-09-09
- Naprawiono znikanie pozycji Bez gestów drugiego i kolejnych narzędzi póz w jednym awatarze
- Naprawiono powrót warstwy USSPS_Locomotion do Standing mimo wybrania pozy takiej jak SleepDown
- Naprawiono odchylenia wysokości i X/Z z FloorAdjuster oraz uwzględniono kolejność przetwarzania wersji MA i AAO
- Po usunięciu wstępnie zbudowanego AnimatorController z Generated automatycznie przywracany jest standardowy PosingSystem_Locomotion
- Naprawiono błąd budowania po usunięciu wszystkich poz stojących
- Presety są sortowane według nazwy awatara

## [3.0.13] - 2026-09-04
- Naprawiono przejście pozy stojącej do samej siebie przy powrocie do stania tuż po rozpoczęciu kucania w VR, destabilizujące powrót do pozy

## [3.0.12] - 2026-09-04
- Naprawiono błędne rozpoznawanie narzędzia bez budowania wstępnego jako zmienionego podczas budowania NDMF
- Naprawiono błąd `AddAssetToSameFile` przy dodawaniu wygenerowanych zasobów do tymczasowego AnimatorController podczas budowania NDMF

## [3.0.11] - 2026-08-31
- Naprawiono nieprawidłową Scene awatara budowania wstępnego i błąd przetwarzania w NDMF 1.14.7 i nowszych wersjach
- Naprawiono `MissingReferenceException` przy odwołaniu ekranu błędów NDMF do zniszczonego LocalizationAsset

## [3.0.10] - 2026-08-29
- Właściwe zasoby presetów są wczytywane dopiero przy otwieraniu wyboru presetu. Jeśli katalog GUID się nie zmienił, pamięć podręczna jest używana ponownie
- Ikony ostrzeżeń i błędów Hierarchy są oceniane tylko po zmianach, a wyszukiwanie zasobów menu produktu korzysta ze wspólnej pamięci podręcznej. Ograniczono zawieszanie Editora co kilka sekund w scenach z wieloma narzędziami póz
- Naprawiono `ArgumentException` przy rejestrowaniu powielonych wartości parametrów podczas budowania wstępnego kilku narzędzi póz w jednym awatarze

## [3.0.9] - 2026-08-26
- Gdy nie można pobrać VRCAvatarDescriptor z PosingSystem podczas konwersji, zapobiega się NullReferenceException i wskazuje potrzebny dziennik budowania
- Naprawiono błąd `Avatar is null`, gdy późniejsze budowanie zwykłe lub wstępne używało podglądu odwołującego się do tymczasowego Avatar utworzonego i usuniętego przez NDMF. Konwersję oddzielono od pamięci podglądu oraz zapobiegnięto konfliktom nazw tymczasowych zasobów NDMF między podglądami
- Naprawiono niezapisywanie odwołania podglądu Prefabu 可愛いポーズ (Kawaii Posing) w scenie, przez co mnożyły się ukryte awatary podglądu. Podglądy wykluczono z zapisu sceny, a stare są sprzątane przy wczytywaniu sceny, przejściu do Play i przeładowaniu skryptów

## [3.0.8] - 2026-08-18
- Naprawiono Unity Assertion i możliwe uszkodzenie danych poz wstępnie zbudowanych ruchów podczas ponownej korekcji wysokości przez FloorAdjuster i podobne narzędzia
- Poprawiono bezpieczeństwo pomiaru wysokości, aby nie zakłócał oryginalnego awatara ani AnimationMode innych narzędzi

## [3.0.7] - 2026-08-12
- Naprawiono przesunięcie punktu widzenia i nieprawidłowe zgięcie kolan na plecach z FloorAdjuster i innymi narzędziami wysokości
- Naprawiono błąd, gdy PosingSystem jest poza awatarem (dziękujemy PancakeTorto1se)
- Naprawiono błąd wynikający z kontynuowania po nieudanym przesłaniu awatara
- Naprawiono czasami błędne wykrywanie zmian ustawień tuż po budowaniu wstępnym
- Poprawiono automatyczny import ruchów lokomocji
- Dodano ostrzeżenia dla niektórych awatarów ze szczególnymi rigami
- Dodano potwierdzenie zmian i odbicie wartości korekcji w oknie dostosowania animacji (dziękujemy kraihd)
- Dodano wskazówki dla błędów innych narzędzi oraz sprawdzanie miejsc szczególnie podatnych na błędy

## [3.0.6] - 2026-03-17
- Naprawiono silne deformacje poz niektórych awatarów z własnym pierwotnym systemem sterowania pozą

## [3.0.5] - 2026-01-31
- Naprawiono awarie Unity z niektórymi ExpressionMenus
- Naprawiono problemy awatara z Prefabem integracji ごろ寝EX (dziękujemy あずき)
- Naprawiono nieprawidłowy obrót przy łączeniu animacji korekcji

## [3.0.4] - 2026-01-11
- Częściowo poprawiono działanie awatara po umieszczeniu w Prefabs
- Dodano usuwanie licencji
- Naprawiono niewłaściwe ułożenie szyi w wielu pozach
- Nieco przyspieszono otwieranie Inspectora
- Ikony są tworzone także przy budowaniu wstępnym. Zmieniono tekst przycisku aktualizacji awatara
- Ostrzeżenia są ukrywane, jeśli nigdy nie wykonano budowania wstępnego (ponieważ wiele osób prawdopodobnie go nie używa)
- Usunięto zbędne zasoby
- Naprawiono brak możliwości korekcji palców

## [3.0.3] - 2026-01-04
- Naprawiono stosowanie innego presetu niż wybrany (dziękujemy あずき)

## [3.0.2] - 2026-01-02
- Dodano obsługę ciemnego motywu Unity
- Naprawiono deformację pozy podczas obrotu przy dostosowywaniu animacji
- Zmieniono rozmiar generowanych ikon menu, włączono kompresję i dodano opcję jeszcze mniejszych ikon
- Przywrócono usunięte w 3.0.0 łączenie blokad śledzenia i automatyczny import ruchów awatara

## [3.0.1] - 2025-12-29
- Naprawiono problemy ustawiania ruchów domyślnych
- Naprawiono samoczynne deformacje pozy awatara

## [3.0.0] - 2025-12-28
- Dodano budowanie wstępne
- Dodano dostosowywanie animacji
- Dodano presety

## [2.2.5] - 2025-05-08
- Rozwiązano problemy zgodności z niektórymi funkcjami awatara

## [2.2.4] - 2025-04-04
- Naprawiono brak zmiany poz z niektórymi awatarami i wersjami ModularAvatar

## [2.2.3] - 2025-03-13
- Naprawiono nieprawidłowe ruchy niektórych awatarów, np. skok 森羅
- Naprawiono czasami niewłaściwą wartość początkową Bez gestów

## [2.2.2] - 2025-03-06
- Naprawiono błąd w niektórych środowiskach

## [2.2.1] - 2025-02-05
- Naprawiono awarie Unity
- Dodano wersję do nazwy narzędzia w NDMF Console

## [2.2.0] - 2025-02-02
- Dodano ustawienie wyłączenia gestów i używania kształtu dłoni z animacji

## [2.1.6] - 2025-02-02
- Naprawiono psucie poz z niektórymi narzędziami
- Naprawiono błędy zależne od ustawień

## [2.1.5] - 2024-12-10
- Usunięto konflikty z niektórymi funkcjami awatara
- Dodano przycisk usuwania wersji Unitypackage

## [2.1.4] - 2024-12-08
- Naprawiono błąd zależny od awatara
- Naprawiono brak blokady stóp wskutek konfliktów funkcji lub awatara
- Naprawiono powrót pozy awatara do standardowej VRC w trybie pulpitu

## [2.1.3] - 2024-12-01
- Naprawiono brak zmian poz z awatarami lub funkcjami używającymi niewłaściwego typu VRMode
- Dodano wyświetlanie konfliktów różnych typów parametrów między funkcjami w NDMF-Console

## [2.1.2] - 2024-12-01
- Dodano wyjaśnienie w NDMF-Console, gdy błąd innego narzędzia powoduje błąd Unisaki Posing System przy budowaniu awatara
- Naprawiono błąd budowania niektórych awatarów
- Poprawiono działanie tuż po wczytaniu awatara
- Oryginalna obsługa ruchu awatara jest teraz usuwana, bo uniemożliwiała działanie niektórych awatarów

## [2.1.1] - 2024-11-20
- Naprawiono problem MergeAnimator w PosingOverride

## [2.1.0] - 2024-11-20
- Rozwiązano zapadanie stóp i bioder w ziemię w trybie pulpitu, aby każda poza wyglądała poprawnie
- Dodano zmianę pozy niezależnie od wysokości głowy
- Dodano tryb bez generowania ikon do przesyłania awatarów Quest
- Dodano ostrzeżenia o niektórych problemach awatara
- Rozwiązano konflikty blokad z AFK, blokadą ramion innych narzędzi i podobnymi funkcjami
- Dodano automatyczną blokadę głowy na plecach

## [2.0.0] - 2024-11-12
- Wydano wersję obsługującą VPM/VCC
- Zmieniono nazwę narzędzia wyświetlaną przy błędach #1
- Dodano warunki użytkowania #5
