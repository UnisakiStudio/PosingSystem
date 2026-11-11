# Unisaki Posing System

Wspólny system póz dla Kawaii Posing Tool, Cool Posing Tool i innych narzędzi. Za pomocą Modular Avatar i NDMF tworzy potrzebne menu oraz animacje podczas przesyłania awatara.

Ten README dotyczy tworzonej wersji v3.2.0. Data oficjalnego wydania nie została ustalona. Zainstalowana wersja jest widoczna u góry Inspectora narzędzia póz. Zobacz również [nowe funkcje i historię zmian](CHANGELOG.md).

## Język interfejsu i dokumentacja offline

Wybierz język w polu „Language / 言語” u góry Inspectora. Jeden wybór obejmuje Inspector i generowane menu VRChat. Aby zmienić język w grze, prześlij awatara ponownie.

Jeżeli nie wybrano języka, używany jest język komputera, a dla nieobsługiwanego języka — angielski. Obsługiwane są Windows, macOS i Linux. Ostatni wybór jest zapisywany w EditorPrefs i używany także w innych projektach tego samego użytkownika systemu na tym komputerze. Wszystkie narzędzia do pozowania i awatary używają tego samego języka. Zmiana w Inspectorze dowolnego narzędzia aktualizuje wspólne ustawienie; wybory języka poszczególnych awatarów są ignorowane. Nie jest synchronizowany z innymi komputerami. Sposób przechowywania licencji nie zmienia się. Przycisk języka pojawia się tylko przy pierwszym potwierdzeniu instalacji, gdy nie zapisano języka. Późniejszych zmian dokonuje się w Inspectorze.

README, historię zmian i instrukcję można wybrać z dołączonej [listy języków](../index.html). Przyciski instrukcji i historii zmian w Inspectorze również uwzględniają wybrany język. Oryginalne warunki użytkowania i tłumaczenia pomocnicze nadal są dostarczane osobno.

## Zmiana póz w trybie pulpitu

Skrócono opóźnienie zastosowania wybranej pozy przy przechodzeniu między staniem, kucaniem i siedzeniem w trybie pulpitu. Zmiana działa podczas zwykłego budowania i budowania wstępnego bez specjalnych poleceń ani dodatkowych ustawień. VR i szczególne stany zachowują dotychczasową obsługę. Przed przesłaniem awatara zaktualizuj dane budowania wstępnego ze starej wersji.

## Instalacja i aktualizacja

Dodaj narzędzie póz przez VCC lub ALCOM zgodnie z zakupionymi plikami i instrukcją danego produktu. Ten system jest instalowany jako zależność narzędzia póz.

Wymagania wersji v3.2.0:

- Projekt awatara VRChat w Unity 2022.3
- VRChat Avatars SDK 3.10.4 lub nowszy
- Modular Avatar z gałęzi 1.x, w wersji co najmniej 1.15.1
- NDMF (Non-Destructive Modular Framework) z gałęzi 1.x, w wersji co najmniej 1.14.0

Jeżeli używasz Kawaii Posing Tool, zaktualizuj go do 3.2.0 w ALCOM / VCC i sprawdź, czy Unisaki Posing System również ma wersję 3.2.0. Jeśli korzystasz z budowania wstępnego, naciśnij „Aktualizuj budowanie wstępne”, zapisz scenę, a następnie prześlij awatara.

Cool Posing Tool 1.2.0 korzysta ze wspólnego systemu 3.2.0. Aktualizuj produkt i wspólny system razem do zgodnych wersji.

### Dla użytkowników dawnej wersji 8bit

Nowe instalacje korzystają teraz ze zwykłego Prefabu. Wyłącz niepotrzebne pozycje we Funkcjach, aby zmniejszyć zużycie. Sam wybór póz w podstawowej konfiguracji zużywa 8 bit.

Dawny Prefab 8bit pozostaje w „Editor/Legacy” każdego produktu, aby zachować odwołania istniejących awatarów i presetów. Dalsze używanie dawnej wersji 8bit nie wymaga usuwania ani zastępowania Prefabu. Wybór wersji 8bit w menu dodawania Unity został usunięty.

## Okno ustawień

Wybierz w Hierarchy „可愛いポーズ (Kawaii Posing)”, „カッコいいポーズ (Cool Posing)” lub inne narzędzie wewnątrz awatara, aby otworzyć ustawienia.

| Karta | Ustawienia |
| --- | --- |
| Funkcje | Pozycje menu w grze, udogodnienia, ikony i zużycie ExpressionParameters |
| Pozy | Wybór, dodawanie i kolejność póz, podgląd w Scene, szczegóły póz i dostosowywanie animacji |
| Konfiguracja awatara | Korekcja proporcji ciała i kolan, import oryginalnych póz i własnych ruchów, presety |

Jeśli pozycja ma dodatkowe ustawienia, naciśnij „Otwórz ustawienia”: pola pojawią się pod nią. Ustawienia oddechu również są we Funkcjach.

Górny panel pomocy i historii zmian otwiera instrukcję, wsparcie i historię zmian. Historię można przełączać między produktem a Unisaki Posing System.

## Funkcje i zużycie ExpressionParameters

Pozycje wybrane w „Funkcje > Funkcje menu” pojawiają się w menu gry. Na przykład „Odbicie” pozwala przełączać odbicie w VRChat. Sam wybór tej pozycji w Inspectorze nie wymusza stałego odbicia wszystkich póz.

| Funkcja | Dodatkowe bity |
| --- | ---: |
| Odbicie | 1 bit |
| Bez gestów | 1 bit |
| Animuj oddech | 0 bit |
| Oddech (przełącznik w grze) | 1 bit |
| Wysokość stóp | 8 bit |
| Wysokość ziemi | 8 bit |
| Tryb zdjęć (tylko na własnym ekranie) | 0 bit |
| Synchronizuj tryb zdjęć | 16 bit |
| Pokazuj głowę w trybie zdjęć | 0 bit |
| Wyłącz kołysanie w spoczynku VRChat | 0 bit |
| Ogranicz deformację pozy przy obrocie głowy | 1 bit (wspólny z obracaniem) |
| Obróć się | 10 bit (w tym 1 bit wspólny z ograniczeniem deformacji) |

Obracanie używa 1 bit na przełącznik, 8 bit na nachylenie głowy i 1 bit na jej blokadę. Jeśli ograniczenie deformacji pozy już działa, dodanie obracania zwiększa zużycie o 9 bit. Wspólna część nie jest liczona dwa razy, a wynik aktualizuje się po zmianie zaznaczeń.

Wyświetlana suma dotyczy tylko narzędzi póz zainstalowanych na tym awatarze. Nie obejmuje całego awatara z ubraniami i innymi funkcjami. Wspólne ustawienia kilku narzędzi również nie są liczone ponownie. Gdy liczba póz przekracza podstawowy zakres numeracji, ich wybór wymaga dodatkowych bitów.

### Wysokość stóp i wysokość ziemi

- „Wysokość stóp”: reguluje wysokość po stronie stóp, zachowując pozycję głowy. Służy do dopasowania stóp do podłogi lub łóżka.
- „Wysokość ziemi”: przesuwa całe ciało wraz z głową w górę i w dół. 50% to pierwotna wysokość, 0% — o 3m niżej, 100% — o 3m wyżej. Wartość początkowa wynosi 50%.

Obie pozycje są w menu „Ustawienia” w grze, gdzie Wysokość ziemi następuje po Wysokości stóp. Odsunięcie wysokości ziemi od środka przesuwa ciało w górę lub w dół. Powrót w okolice 50% przywraca pierwotną wysokość i kończy korekcję. Nie ma osobnego przełącznika unoszenia. Korekcja punktu widzenia działa przy rozpoczęciu unoszenia oraz podczas regulacji wysokości ziemi.

Dla zwykłych animacji regulacja wysokości stóp nie tworzy już powtórnie ruchu całego ciała na potrzeby zmiany wysokości. Zmniejsza to rozmiar tworzonych animacji, ale nie oznacza, że cały awatar zawsze maleje do 1/3.

### Tryb zdjęć

1. Włącz „Funkcje > Funkcje menu > Tryb zdjęć”.
2. Otwórz Tryb zdjęć w VRChat i wybierz pozę przez „Wybierz pozę”.
3. Suwakiem „Odległość” przesuń awatara do 3m do przodu, a suwakiem „Kierunek” obróć go. Jeśli odległość jest większa od 0, awatar zwraca się do ciebie.
4. W razie potrzeby włącz „Blokuj stopy”. Na początku blokada nie jest wymuszana, więc możesz wcześniej dopasować pozycję. Jeśli dodano „Bez gestów” do menu, na tym samym ekranie można też zmieniać kształt dłoni.
5. Wybierz „Zakończ zdjęcia”, aby wyzerować odległość i kierunek oraz zwolnić blokadę stóp.

Ruch i obrót w trybie zdjęć zużywają 0 bit, gdy działają tylko na własnym ekranie. Aby inni widzieli pozycję i kierunek, włącz „Udogodnienia > Synchronizuj tryb zdjęć”. Odległość i kierunek razem zużywają 16 bit.

Jeśli głowa własnego awatara znika z bliska, włącz „Udogodnienia > Pokazuj głowę w trybie zdjęć”. Ustawienie wpływa tylko na twój ekran podczas zdjęć. Po zakończeniu wraca zwykła widoczność.

### Zmiana pozycji leżącej skokiem

Jeśli wybrano pozę na plecach i na brzuchu, można je przełączać skokiem w miejscu. Gdy wybrano tylko jedną, pozostaje aktywna. Jeśli nie chcesz przełączania skokiem, wyłącz „Zmieniaj pozycję leżącą skokiem” w Unity, w „Funkcje → Udogodnienia”. Dodatkowe zużycie parametrów synchronizowanych wynosi 0 bit. Przy kilku narzędziach póz wyłączenie w dowolnym z nich obejmuje cały awatar.

### Obróć się

W obsługiwanych pozycjach leżących ciało płynnie obraca się zgodnie z przechyleniem głowy w lewo lub w prawo. Funkcja nie dodaje automatycznej obsługi wszystkich leżących póz.

1. Włącz „Funkcje > Funkcje menu > Obróć się”.
2. Na karcie Pozy otwórz szczegóły odpowiedniej pozy leżącej i włącz „Włącz obracanie”.
3. Przypisz animację na boku w polu „Boczna poza obracania”. Obracanie w lewo i w prawo jest tworzone z oryginalnej i bocznej pozy. W razie potrzeby dopasuj „Siła korekcji podłoża”.
4. W VRChat wybierz obsługiwaną pozę i włącz „Obróć się”. Przed użyciem zwolnij blokadę głowy.

W pozach z obracaniem przechylenie głowy steruje pozycją odtwarzania animacji, więc zwykłe ustawienie MotionTime jest niedostępne. Jeśli używasz gotowej animacji obracania, przypisz ją w zwykłym polu animacji, pozostawiając Boczną pozę obracania pustą. Przygotuj animację, w której 0 oznacza lewy bok, 0.5 — plecy, a 1 — prawy bok.

### Oddech i standardowe kołysanie

„Funkcje > Udogodnienia > Animuj oddech” dodaje ruch zgodny z oddechem, na przykład klatki piersiowej. Przez Otwórz ustawienia można wybrać dla każdej kategorii poz: Brak, Słabe, Średnie, Silne, Szybkie lub Własne. Szybkie oznacza przyspieszony oddech. Własne pozwala przypisać przygotowaną przez siebie animację.

Sam ruch oddechu zużywa 0 bit. Aby zatrzymywać i wznawiać go w VRChat, włącz też „Funkcje menu > Oddech”. Synchronizacja przełącznika wykorzystuje 1 bit. Zmiana siły nie zwiększa zużycia.

Szyja, barki i ramiona poruszają się wraz z klatką piersiową, ograniczając przesunięcie głowy i dłoni. Pozycja nie jest całkowicie zablokowana, więc dobierz siłę do budowy ciała i pozy.

„Udogodnienia > Wyłącz kołysanie w spoczynku VRChat” ogranicza standardowe kołysanie ciała w bezruchu. Domyślnie jest wyłączone i zachowuje oddech narzędzia oraz inne własne ruchy. Można używać tej opcji również bez oddechu; nie zwiększa liczby bitów. Nie zatrzymuje kołysania wynikającego z innych przyczyn niż standardowa animacja spoczynku.

Przy kilku narzędziach póz wyłączenie standardowego kołysania w dowolnym z nich obejmuje cały awatar. Aby je przywrócić, wyłącz tę opcję we wszystkich narzędziach i prześlij awatara ponownie.

### Ogranicz deformację pozy przy obrocie głowy

Użyj tej opcji, jeśli różnica kierunku szyi względem oryginalnej pozy deformuje ciało po zwolnieniu blokady głowy. Znajduje się w „Funkcje > Udogodnienia” i nie dodaje osobnego menu gry. Przy zablokowanej głowie używa kierunku oryginalnej pozy. Leżenie na plecach jest wyłączone z korekcji, aby zachować pierwotne skierowanie w górę.

### Ikony menu

- „Generuj ikony poz”: tworzy obrazy awatara w każdej pozie i pokazuje je w menu. Ułatwia rozpoznawanie póz, ale tworzenie obrazów wydłuża budowanie.
- „Zmniejsz rozmiar ikon”: obniża rozdzielczość obrazów, aby zmniejszyć rozmiar awatara.

W kompilacjach dla Androida ikony poz nie są tworzone. Po zmianie ubrania lub wyglądu użyj Ponownie zrób ikony. Obrazy listy poz aktualizuje Odśwież miniatury poz. Operacja zajmuje czas, więc przeczytaj potwierdzenie przed uruchomieniem.

## Dodawanie, porządkowanie i podgląd póz

Wybierz kategorię po lewej stronie karty Pozy. Górna karta z lupą wyszukuje nazwy we wszystkich kategoriach. Szczegóły pozwalają razem ustawić animację, ikonę, MotionTime i inne opcje.

Kilka animacji można dodać, przeciągając je do obszaru dodawania lub wybierając w Project i używając polecenia importowania zaznaczonych klipów. Powtórzone odwołania do tego samego klipu są pomijane.

Aby zmienić kolejność, wybierz miejsce docelowe w „Szczegóły > Kolejność”. Dostępne są też przyciski w górę i w dół. Stan rozwinięcia szczegółów pozostaje zachowany po przesunięciu.

Podgląd przy każdej pozie pokazuje ją w Scene bez otwierania okna dostosowania. Zakończenie przywraca pierwotny widok. Uproszczony podgląd nie uruchamia budowania MA ani NDMF, więc może różnić się od ostatecznego ubrania, FloorAdjuster i innych efektów budowania. Sprawdź ubranie i kontakt z podłożem również po zbudowaniu awatara.

## Dostosowywanie animacji i automatyczna korekcja kończyn (IK)

Otwórz Dostosowywanie animacji na karcie Pozy, aby dokładnie ustawić ciało i kończyny z podglądem. Po zakończeniu zapisz animację korekcji poleceniem zapisu w tym oknie.

W obsługiwanych produktach i pozach automatyczna korekcja dopasowuje pozycje kończyn do budowy awatara. Włącz automatyczną korekcję kończyn w oknie dostosowania animacji steruje całą funkcją, a Włącz IK tej pozy — pojedynczą pozą. Dla pozy bez automatycznej korekcji pojawia się odpowiednia informacja.

Aby ręcznie dopracować wynik automatyczny, naciśnij Wypal IK do korekcji. Bieżący wynik IK zostanie zapisany w nowej animacji korekcji, a IK tej pozy na danym awatarze zostanie wyłączone. Oryginalna animacja pozostaje. Przycisk jest dostępny, gdy korekcja ogólna i danej pozy są włączone oraz istnieją odpowiednie ustawienia IK.

W Konfiguracji awatara opcja Zapobiegaj wyginaniu kolan do tyłu przełącza korekcję kolan. W Szczegółach ustaw Minimalne zgięcie kolana (stopnie). Włącz korekcję awatara jest dostępne, gdy istnieją odpowiednie dane korekcji. Nakładające się korekcje nie zawsze mogą działać razem z automatyczną korekcją kończyn.

## Oryginalne pozy, własne ruchy i presety

Te opcje znajdują się na karcie Konfiguracja awatara.

- „Używaj oryginalnych poz stojących, kucających i na brzuchu”: automatycznie importuje pierwotne pozy awatara.
- „Własne ruchy”: otwórz ustawienia importu i osobno przypisz ruch dla każdego typu, np. stania, kucania (Crouching) i leżenia na brzuchu (Prone). Zwykle przypisuje się jeden ruch do typu.
- „Połącz blokady śledzenia z innymi narzędziami”: włącz, jeśli blokady nie działają poprawnie wraz z innymi funkcjami awatara.

Ustawienia importowania własnych ruchów są osobną funkcją od automatycznego importowania oryginalnych póz.

Presety są na dole karty. Wyszukaj po nazwie i naciśnij Zastosuj ten preset, aby zastąpić ustawienia poz. Służą przede wszystkim do konfiguracji początkowej i zastępują bieżące ustawienia. Zapisz wykonane korekcje przed zmianą.

Podczas wyświetlania listy presetów wczytywane są tylko nazwy i informacje o zgodnych awatarach. Animacje korekcji wczytują się dopiero przy zastosowaniu wybranego presetu. Nawet pierwsze utworzenie listy nie wczytuje zawczasu animacji wszystkich awatarów.

Lista dzieli się na Zgodny awatar, Presety użytkownika i Oficjalne presety Unisaki Studio. Ukryj dołączone presety to wspólna opcja widoku w danym projekcie. Dodanie, zmiana lub usunięcie presetu aktualizuje listę. Jeśli widok się nie zmienił, naciśnij Odśwież.

## Budowanie wstępne i ostrzeżenia

Gdy Używaj oryginalnych poz stojących, kucających i na brzuchu jest włączone, ruchy są wykrywane z Base awatara przy każdym generowaniu. Wyniki nie trafiają do ręcznych ustawień importu, więc przy samym imporcie automatycznym lista pozostaje nieprzypisana. Zmiany ruchów Base i przełączenie automatycznego importu są uwzględniane w następnym generowaniu.

Automatyczny import podczas przesyłania dotyczy kopii budowania. Nie zmienia oryginalnego obiektu sceny, ustawień importu, źródłowych Controllerów ani ruchów. Jeśli istnieje wynik budowania wstępnego i dane wejściowe się nie zmieniły, jest używany ponownie.

Ręczne przypisanie ma pierwszeństwo dla danego typu; uwzględnia też obrót, animację korekcji i pozycję odtwarzania. Wyłączenie Użyj lub odpięcie przypisanego ruchu zatrzymuje automatyczny import typu. Wyczyść wybór ręczny usuwa jego ustawienia i korekcje, więc przy włączonym automatycznym imporcie kolejne generowanie wykryje ruch ponownie. Czynność można cofnąć przez Undo. Przy kilku dawnych ustawieniach tego samego typu pierwszeństwo ma poprawne ustawienie wybrane w Inspectorze.

Ustawień dodanych dawnym automatycznym importem nie da się odróżnić od ręcznie zapisanych i skorygowanych, więc nie są usuwane automatycznie. Użyj Wyczyść wybór ręczny tylko dla typów, które chcesz przywrócić do automatycznego importu. Jeśli kilka produktów jest na tym samym awatarze, ich ręczne ustawienia także wpływają na wspólne oryginalne pozy.

Budowanie wstępne skraca czas budowania awatara dzięki ponownemu użyciu wygenerowanych danych. Możesz je utworzyć lub odświeżyć przez Aktualizuj budowanie wstępne. Zwykłe budowanie awatara również tworzy potrzebne dane, więc ręczne budowanie wstępne za każdym razem nie jest konieczne.

Zmiana ustawień wymaga aktualizacji. Informują o tym Sprawdź ustawienia u góry Inspectora oraz znacznik w Hierarchy. Po zmianie poz lub IK aktualizacja budowania wstępnego może skrócić kolejne budowania.

Wyświetlanie ostrzeżeń jest wewnątrz ramki Sprawdź ustawienia. Można ukryć konkretne rodzaje ostrzeżeń albo znacznik Hierarchy. Ukrycie nie rozwiązuje problemu ustawień ani nie aktualizuje wygenerowanych danych. Błędy i wymagane generowanie pozostają bez zmian.

## Uwagi dotyczące używania

- Przeznaczone do śledzenia 3 punktów i trybu pulpitu. Blokada poz podczas pełnego śledzenia ciała nie jest obsługiwana.
- Budowa ciała i modyfikacje mogą wymagać dodatkowego dopasowania kontaktu kończyn oraz wyglądu ubrania.
- Szczególne warstwy Base i inne mechanizmy zmiany poz mogą nie działać zgodnie z oczekiwaniami.
- Ruch i obrót trybu zdjęć oraz wysokość ziemi korygują pozy narzędzia, a także standardowe i własne ruchy bez wybranej pozy. Sprawdź w samym VRChat punkt widzenia i zachowanie po zastąpieniu standardowych ruchów.

## Warunki użytkowania i wsparcie

Kod źródłowy i zasoby tego repozytorium nie są otwartym oprogramowaniem. Używanie wymaga legalnego zakupu produktu i przyjęcia [dołączonych warunków](../../../%E3%82%86%E3%81%AB%E3%81%95%E3%81%8D%E3%83%9D%E3%83%BC%E3%82%BA%E3%82%B7%E3%82%B9%E3%83%86%E3%83%A0%E5%88%A9%E7%94%A8%E8%A6%8F%E7%B4%84%EF%BC%88%E6%97%A5%E6%9C%AC%E8%AA%9E%E7%89%88%EF%BC%89.pdf).

- [Instrukcja Kawaii Posing Tool](https://docs.unisakistudio.jp/kawaiiposing/manual/)
- [Instrukcja Cool Posing Tool](https://docs.unisakistudio.jp/coolposing/manual/)
- [Kontakt ze wsparciem](https://help.unisakistudio.jp/)

Zgłaszając błąd, podaj wersje produktu i Unisaki Posing System, Unity, VRChat SDK, Modular Avatar i NDMF, używany awatar, kroki odtworzenia i treść błędu Console.
