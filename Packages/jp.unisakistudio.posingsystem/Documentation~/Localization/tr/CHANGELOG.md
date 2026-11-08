# Değişiklik geçmişi

## [3.2.0] - Yayınlanmadı

### Eklenenler

- Inspector, düzenleme işlemleri, kurulum iletişimleri, standart poz adları ve oluşturulan VRChat menüleri 30 dilde görüntülenebilir.
- Inspector ve oyun menüsünün dil seçimi birleştirildi. Başlangıçta bilgisayarın dili kullanılır ve son seçim aynı işletim sistemi kullanıcısının diğer projelerinde de uygulanır.
- README, değişiklik geçmişi ve çevrimdışı kılavuz seçilen dilde açılabilir.
- 128 standart pozun görüntüleme adları ve özellik açıklamaları mevcut terimlere göre hazırlandı.

### Uyumluluk

- Avatarın mevcut ayarları, özel adlar, varlık başvuruları, parametre kimlikleri ve lisans saklama yöntemi korunur.

## [3.1.2] - 2026-11-09

- Baş dönüşünden kaynaklanan poz bozulmasını azalt açıkken yüzüstünden kalktıktan sonra yüzün yukarı bakmaya devam etmesi düzeltildi.
- Ayak yüksekliği kapalı olsa da EX menüsündeki “Ayarlar → Sırtüstüyken başı otomatik kilitle” ile baş kilidi değiştirilebilir. 3.1.0 ve 3.1.1’de kaybolan öğe, güncelleme sonrası avatar derlenince geri gelir.
- Unity’de avatarın ana nesnesinin işareti kaldırılmış olsa da Ön oluşturmayı güncelle kullanılabilir.
- Eski ortak poz Controller’ı Assets’te kaldığında güncelleme sonrası poz oluşturulamaması düzeltildi. Kendiniz düzenlediğiniz Controller ayarları da korunur.
- Araç veya üst nesnesi devre dışı olduğundan ortak poz Animator’ı kullanılamıyorsa hedefin adı gösterilir. “Ayarları kontrol et → Hedefi seç” ile konumuna gidebilirsiniz.
- Ön oluşturmada ayar değişikliği bildirildiğinde “Ayrıntılar → Ön oluşturmadan beri değişenleri göster” ile önceki ayarlardan farklar incelenebilir.
- Betik hatası derlemeyi durdurduğunda (PS050), Unity’nin bulduğu en fazla 5 özgün hata gösterilir. Bu hata hakkında sor, Unity ve desteklenen araçların sürümleri ile hata numarasını içeren, kontrol edip düzenleyerek gönderebileceğiniz bir taslak açar.

Ön oluşturma kullandıysanız güncellemeden sonra Ön oluşturmayı güncelle’ye basın, sahneyi kaydedin, avatarı yeniden derleyip yükleyin.

## [3.1.1] - 2026-11-08

- Aynala kapalı olsa da etkin Zemin yüksekliği ve Dön, VRChat EX menüsünün Ayarlar bölümünde görünür.
- Eski sürümde pozlar sıralansa, silinse veya yeniden adlandırılsa da Bacaklar Uzun, Rahat ve Bir Diz Yukarı doğru dönüşlerini korur. Tam Unpack yapılmışsa da özgün poz tanınabildiği sürece korunur. Kendi pozlarınız, elle kapattığınız dönüşler ve değiştirdiğiniz yan hareketler korunur.
- VRChat’te avatar seçildiğinde kısa süre yüzüstü olması düzeltildi. Yalnız yüzüstü seçiliyken zıplayınca kısa süre sırtüstü olması da düzeltildi. Zıplama geçişi “Özellikler → Kolaylıklar → Zıplayarak yatış pozunu değiştir”den kapatılabilir; varsayılan olarak açıktır.
- Güncelleme sonrası ortak poz Animator Controller başvurusu kaybolduysa ve kurtarılabiliyorsa uyarı gösterilip ayarlardan yeniden oluşturulur. Birden fazla avatarın paylaştığı Controller başvuruları da korunur.
- Projede derleme hatası varsa derlemenin başında neden gösterilip durdurulur. Betik hatası başvuruların incelenmesini engellerken oluşturulan Controller’ların otomatik temizliği ertelenir.
- Projede ürünle aynı adlı Prefab olsa da araç eklenirken resmî Prefab önceliklidir. Ortak Controller yalnızca satır sonlarında farklıysa ön oluşturma sürer.
- Aracın eklediği zemin yüksekliği gibi FX katmanlarının Write Defaults değeri avatar ayarlarına uyarlandı.

Ön oluşturma kullandıysanız güncellemeden sonra sahnedeki araçta Ön oluşturmayı güncelle’ye basın, sahneyi kaydedin ve avatarı yeniden derleyip yükleyin.

## [3.1.0] - 2026-11-07

- Desteklenen pozların uzuvlarını vücut yapısına uyduran otomatik düzeltme (IK), avatar düzeltmesi ve dizin ters bükülmesini önleme eklendi.
- Fotoğraf modunda avatarın konumu ve yönü ayarlanabilir. Konum/yön eşitlemesi ve fotoğraf sırasında kendi başınızın görünmesi seçilebilir.
- Zemin yüksekliği tüm vücudu en fazla 3m yukarı veya aşağı taşır; ayak yüksekliğinden ayrı amaçla kullanılabilir.
- Nefes hareketi, varsayılan bekleme sallanmasını kapatma ve baş yönünden kaynaklanan poz bozulmasını azaltma eklendi.
- Kullanılan özellikler ve parametre maliyeti Inspector’da görülebilir. Poz arama, toplu ekleme, sıralama ve Scene’de hafif önizleme eklendi.
- Masaüstünde ayakta, çömelme ve oturma geçişleri hızlandırıldı.
- Ayar değişikliği sonrası ön oluşturma uyarıları, Inspector kaydırma yükü ve oluşturulan verilerin temizliğindeki uzun durmalar iyileştirildi.
- Otomatik içe aktarma açıkken ayarlar değişmediği hâlde ön oluşturmanın uyarı verip yüklemede yeniden oluşturulması düzeltildi. İçe aktarma sonucu üretilen avatara uygulanır; özgün nesnenin içe aktarma ayarları değişmez.
- Baş eğimiyle dönüş ve poz başına ilgili hareket ayarları eklendi.
- Eski ön ayarların dönüş ayarlarını koruma, kendi hareketlerinizi içe aktarma ve oluşturulan varlık yönetimi iyileştirildi.

## [3.1.0-beta.2] - Açık beta (2026-11-05)

- IK konum uzunluğu dayanaklarına sabit uzunluk ve Head ile Viewpoint arasındaki yükseklik (Y) ve ileri/geri farkı (Z) eklendi.
- Inspector uyarıları Hierarchy’de nesne adının solunda da görünür. Ön oluşturmanın yapılmaması ve ayar değişikliğine ek olarak hareket içe aktarma ve ön ayar işlemlerinin uyarıları da desteklenir.
- Sahne açılışı gibi otomatik temizliklerde başka sahnelerin oluşturulmuş Controller’larının yüklenmesi yüzünden Unity’nin uzun süre durması düzeltildi.
- Poz listesi veya özellikler değişince ön oluşturma geçmişinin kaybolması düzeltildi. Yapılmadı yerine ön oluşturmadan sonra ayarların değiştiği belirtilir; Hierarchy de güncelleme gerektiğini gösterir.
- Inspector ve Hierarchy, ayarlar ve başvurulan varlıklar değişmediği sürece kaydırma yükünü azaltmak için ön oluşturma karşılaştırmasını paylaşır. Derleme karşılaştırması eskisi gibi her seferinde yapılır.

## [3.1.0-beta.1] - Açık beta (2026-11-04)

- Masaüstünün normal poz geçişleri tek değerlendirme yöntemine geçirildi. Normal derlemede ve ön oluşturmada etkindir; VR ve özel koşullar önceki yolu korur.
- alpha.3 ve önceki ön oluşturmalar güncelleme gerektiren olarak tanınır.
- Fotoğraf sırasında zemin yüksekliği değişince bakış noktası korunur; fotoğraf bitince son zemin yüksekliğine dönülür.

- Otomatik içe aktarma yalnızca üretim sırasında algılamaya çevrildi; ön oluşturma ve derleme elle içe aktarma ayarı eklemez veya değiştirmez.
- Otomatik içe aktarmanın açılması/kapanması, Base değişiklikleri ve elle ayarların temizlenmesi yeniden üretime yansır. Ürünler arasında ortak elle atamalar ve düzeltmeler korunur; eski içe aktarılan hareketler standart pozlara döner.
- Ön oluşturmanın Controller’ları sahne GUID’sine göre avatar başına tek dosyada yönetilir. Kopyaların, diğer sahnelerin ve Prefab’ların ortak başvuruları güncellemeden önce ayrılır.
- Derlemede NDMF’nin geçici varlık yönetimi kullanılır; düzenleme için üretilen dosyalar çoğaltılmaz.
- Sahne açılırken ve kaydedilirken veriler incelenir. Kaydedilmemiş düzenleme, Undo/Redo veya dış başvuru varsa korunur; yalnızca sahipliği açıkça belirli kullanılmayan dosyalar çöp kutusuna taşınır.
- İçe aktarılan hareketin dönüşü, düzeltme animasyonu ve oynatma konumu uygulanır. Dönüş, RootT/RootQ ve Humanoid düzeltmesi normal pozlarla ortak işlenir.
- İç içe BlendTree düzeltmeleri hedefe özel kopyalanır; özgün hareket ve yürüme karışım ayarları korunur.
- Yalnız ayakta oynatma konumu özgün kare hızında sabit poza dönüştürülür ve yürüme oynatmasını durdurmaz.

## [3.1.0-alpha.2] - Kapalı test sürümü (2026-10-26)

### Düzeltmeler ve değişiklikler

- Araç pozu seçilmemişken varsayılan ve özel hareketlere de fotoğraf modu ve zemin yüksekliği düzeltmeleri uygulanır.
- Zemin yüksekliği 3m yukarı/aşağı aralığına genişletildi. 50% özgün yükseklik ve başlangıç değeri, 0% 3m aşağısı, 100% 3m yukarısıdır.
- Yalnız ayakta geçersiz kılmasında kaynak BlendTree’nin değişmesi düzeltildi.
- IK kuralı olmayan pozlarda elle düzeltme kullanılır ve gereksiz üretim uyarıları bastırılır.
- Aynı adlı pozların düzeltme taslaklarının karışması ve aynı varlık içindeki hareket değişikliklerinin atlanması düzeltildi.
- Geçersiz başlangıç pozu numarasının başka bir pozu başlangıçta seçmesi düzeltildi.
- Desteklenmeyen AnimatorOverrideController içe aktarıldığında istisna yerine açıklama gösterilir.
- Eski ön ayarlar uygulanırken açıkça kapatılan dönüş korunur.
- Açık Editor temasında özellik simgelerinin görünürlüğü iyileştirildi.

VRChat istemcisinde bakış noktası ve varsayılan hareketler değiştirildikten sonraki davranışın kontrolü sürüyor.

## [3.1.0-alpha.1] - Yayın öncesi

### Eklenenler

- Özellik başına açma/kapatma ve ExpressionParameters kullanım göstergesi eklendi.
- Avatarı öne taşıyıp döndüren fotoğraf modu eklendi.
- Fotoğraf modu eşitleme seçeneği eklendi: yalnızca yerel 0 bit, eşitlemede 16 bit.
- Fotoğraf modunda kendi avatarınızın başını gizlememe ayarı eklendi.
- Baş dâhil tüm vücudu 3m yukarı/aşağı taşıyan Zemin yüksekliği eklendi. 50% özgün yükseklik ve başlangıç değeri, 0% 3m aşağısı, 100% 3m yukarısıdır.
- Baş eğimiyle dönüş ve poz başına ilgili animasyon ayarları eklendi.
- Nefes hareketi ile Hafif, Orta, Güçlü, Hızlı ve Özel seçenekleri eklendi.
- Nefes hareketi ve oyundaki açma/kapatma menüsü ayrı seçilebilir. Yalnız hareket 0 bit, anahtar eşitlemesi 1 bit.
- VRChat’in varsayılan bekleme sallanmasını kapatma eklendi.
- Baş yönü farkından kaynaklanan poz bozulmasını azaltma eklendi.
- Desteklenen pozları vücuda uyduran otomatik uzuv düzeltmesi (IK), genel ve poz başına açma/kapatma eklendi.
- IK sonucunu düzeltme animasyonuna kaydeden IK’yi düzeltmeye işle eklendi.
- Avatar düzeltmesi, dizin ters bükülmesini önleme ve en küçük bükülme açısı eklendi.
- Poz listesinden Scene’de hafif önizleme eklendi.
- Kategori sekmeleri, kategoriler arası arama, birden çok animasyonu toplu ekleme ve hedef sıra seçimi eklendi.
- Inspector’da ürün ve ortak sistemin değişiklik geçmişini gösterme eklendi.

### Değişenler

- Ön ayar yüklemesi hızlandırılarak araç ekleme ve ön ayar listesinde bekleme süresi kısaltıldı.
- Ön ayar ekleme, düzenleme ve silme listeye otomatik yansır.
- Inspector, Özellikler, Pozlar ve Avatar Kurulumu adlı 3 sekmeye ayrıldı.
- Ayarlara açıklama ve simgeler eklendi; ayrıntıların açılıp kapanması birleştirildi.
- Nefes ayrıntıları özelliklere, ön ayarlar Avatar Kurulumu’nun sonuna taşındı.
- Kendi hareketlerinizi ayrı ayrı atama ve avatarın özgün pozlarını otomatik içe aktarma ayrı ayarlara ayrıldı.
- Ayak yüksekliği için oluşturulan tüm vücut animasyonlarının yinelenmesi azaltıldı.
- Poz görüntüsü güncellemesi onay iletişimi bulunan düğmeye çevrildi.
- Uyarı ayarları Ayarları kontrol et içinde toplandı.
- Yeni 8bit Prefab ekleme kaldırılıp normal Prefab’ın özellik seçimine geçildi. Mevcut başvurular korunur.
- VRChat Avatars SDK’nın minimum sürümü 3.10.4 oldu.

### Düzeltilenler

- Araç pozu seçilmemişken varsayılan ve özel hareketler de fotoğraf modu ve zemin yüksekliği düzeltmesine dâhil edildi.
- Yalnız ayakta geçersiz kılmasının kaynak BlendTree’yi değiştirmesi düzeltildi.
- Geçerli IK kuralı olmayan pozlarda elle düzeltme kullanılır ve gereksiz IK üretim uyarıları bastırılır.
- Aynı adlı pozların düzeltmelerinin karışması ve aynı varlık içinde hareket değişiminin yeniden üretime yansımaması düzeltildi.
- Kapatılmış başlangıç pozunun numarası başka pozda kullanılsa da artık başlangıçta seçilmez.
- Özgün pozları içe aktarırken desteklenmeyen AnimatorOverrideController algılanırsa istisna yerine açıklama gösterilir.
- Eski ön ayarı uygulamak açıkça kapatılmış dönüşü korur.
- Açık Editor temasında da işlev simgelerini ayırt etmek için Inspector simge arka planı iyileştirildi.

- Avatara eklerken Dön, Zemin yüksekliği ve Fotoğraf modu simgelerinin görünmemesi düzeltildi.
- Eski sürüm ön ayarı uygulanınca desteklenen pozların dönüş ayarlarının kaybolması düzeltildi.
- Poz adı düzenlenirken başka ayrıntı açınca girilen adın başka poza geçmesi düzeltildi.
- Poz sıralamada ayrıntıların kapanması ve ayarların Undo/Redo işlemleri düzeltildi.
- Önizlemede özgün avatarın üst üste görünmesi ve yerleştirme konumunun iki kez eklenmesi düzeltildi.
- Eksik bileşenli avatarların önizleme hatası düzeltildi.
- Kemik yönü ve ölçeği farkından kaynaklanan IK uzuv yön ve konum sapmaları düzeltildi.
- Ayar değişir değişmez uyarı ve ön oluşturma durumunun güncellenmemesi düzeltildi.
- IK değişikliğinden sonra eski ön oluşturma sonucunun yeniden kullanılması düzeltildi.
- Bazı yapılarda ek özelliklerin üretilememesine yol açan derleme hataları düzeltildi.

## [3.0.16] - 2026-09-14
- `VirtualAnimatorController.SetParameter` API’si kullanıldığı için NDMF minimum bağımlılığı 1.14.0’a yükseltildi

## [3.0.15] - 2026-09-13
- Hips ölçeği değiştirilmiş ve FloorAdjuster kullanan avatarlarda sırtüstü baş kilidi değişince vücut konumunun ve diz bükülmesinin aşırı değişmesi düzeltildi

## [3.0.14] - 2026-09-09
- Aynı avatara birden fazla poz aracı kurulunca ikinci ve sonrakilerin Jestleri kapat menü öğesinin kaybolması düzeltildi
- SleepDown gibi bir poz atansa da USSPS_Locomotion katmanının Standing’e dönmesi düzeltildi
- FloorAdjuster ile poz yüksekliği ve X/Z konumu sapmaları düzeltildi; MA ve AAO sürümlerinin işlem sırası desteklendi
- Generated’daki ön oluşturulmuş AnimatorController silinirse standart PosingSystem_Locomotion’a otomatik dönülür
- Tüm ayakta pozlar silinmiş yapıda derleme hatası düzeltildi
- Ön ayarlar avatar adına göre sıralanır

## [3.0.13] - 2026-09-04
- VR’de çömelmeye başlar başlamaz ayağa dönünce ayakta pozun kendine geçişiyle poz dönüşünün kararsızlaşması düzeltildi

## [3.0.12] - 2026-09-04
- Ön oluşturma yapılmamış aracı NDMF derlerken ayarların değiştiğinin sanılması düzeltildi
- NDMF derlemesindeki geçici AnimatorController’a üretilen varlık eklenince oluşan `AddAssetToSameFile` hatası düzeltildi

## [3.0.11] - 2026-08-31
- NDMF 1.14.7 ve sonrasında ön oluşturma avatarının Scene’inin geçersizleşmesiyle işlemin başarısız olması düzeltildi
- NDMF hata ekranının yok edilmiş LocalizationAsset’e başvurmasından doğan `MissingReferenceException` düzeltildi

## [3.0.10] - 2026-08-29
- Ön ayarın gerçek varlıkları seçim arayüzü açılana kadar yüklenmez; GUID kataloğu değişmemişse önbellek yeniden kullanılır
- Hierarchy uyarı/hata simgeleri yalnızca değişiklikte yeniden değerlendirilir, ürün menüsü varlık araması ortak önbelleğe alındı. Çok poz aracı olan sahnelerde Editor’ın birkaç saniyede bir donması azaltıldı
- Aynı avatarda birden fazla poz aracıyla ön oluşturma sırasında yinelenen parametre değerleri kaydedilirken oluşan `ArgumentException` düzeltildi

## [3.0.9] - 2026-08-26
- Dönüştürmede PosingSystem’den VRCAvatarDescriptor alınamazsa NullReferenceException önlenir ve inceleme için gereken derleme günlüğü gösterilir
- NDMF’nin oluşturup sildiği geçici Avatar’a bağlı önizlemenin sonraki ön oluşturma/derlemede kullanılıp `Avatar is null` ile bozulması düzeltildi. Dönüştürme önizleme önbelleğinden ayrıldı; farklı önizlemelerin geçici NDMF varlık adlarının çakışması da önlendi
- 可愛いポーズ (Kawaii Posing) Prefab önizleme başvurusunun sahneye kaydedilmemesiyle gizli önizleme avatarlarının sürekli çoğalması düzeltildi. Önizleme sahne kaydından çıkarıldı; eski önizlemeler sahne yükleme, Play’e geçiş ve betik yenilemede temizlenir

## [3.0.8] - 2026-08-18
- FloorAdjuster gibi araçların yüksekliği yeniden düzeltmesi sırasında ön oluşturulmuş poz hareketlerinde Unity Assertion ve poz verisinin bozulma olasılığı düzeltildi
- Yükseklik ölçümünün özgün avatara veya diğer araçların AnimationMode durumuna karışmaması için güvenlik iyileştirildi

## [3.0.7] - 2026-08-12
- FloorAdjuster ve diğer yükseklik araçlarıyla bakış noktası sapması ve sırtüstü dizlerin anormal bükülmesi düzeltildi
- PosingSystem avatar dışında olduğunda oluşan hata düzeltildi (PancakeTorto1se’ye teşekkürler)
- Avatar yükleme başarısızlığından sonra işlemin sürmesinden doğan hata düzeltildi
- Ön oluşturma hemen sonrasında ayarların değiştiğinin yanlış sanılması düzeltildi
- Hareket animasyonlarının otomatik içe aktarılması iyileştirildi
- Özel rig kullanan bazı avatarlara uyarı eklendi
- Animasyon Düzeltmesi’ne değişiklik onayı ve düzeltme değerlerini aynalama eklendi (kraihd’e teşekkürler)
- Diğer araçların hataları için açıklamalar ve hata olasılığı yüksek yerlere kontroller eklendi

## [3.0.6] - 2026-03-17
- Başlangıçta kendine özgü poz kontrolü olan bazı avatarların pozlarının çok bozulması düzeltildi

## [3.0.5] - 2026-01-31
- Bazı ExpressionMenus yapılarının Unity’yi çökertmesi düzeltildi
- ごろ寝EX bütünleştirme Prefab’ıyla avatar sorunları düzeltildi (あずき’ye teşekkürler)
- Düzeltme animasyonlarını birleştirirken dönüşün bozulması düzeltildi

## [3.0.4] - 2026-01-11
- Avatar Prefabs’a yerleştirilince davranış kısmen iyileştirildi
- Lisans silme eklendi
- Birçok pozda boynun bozulması düzeltildi
- Inspector açılışı biraz hızlandırıldı
- Ön oluşturmada da simgeler oluşturulur; avatar güncelleme düğmesinin metni değiştirildi
- Ön oluşturma hiç yapılmadıysa uyarı gizlenir (çok kişinin ön oluşturma kullanmadığı düşünüldüğünden)
- Gereksiz kaynaklar kaldırıldı
- Parmakların ayarlanamaması düzeltildi

## [3.0.3] - 2026-01-04
- Seçilenden farklı ön ayarın uygulanması düzeltildi (あずき’ye teşekkürler)

## [3.0.2] - 2026-01-02
- Unity koyu teması desteklendi
- Animasyon Düzeltmesi’nde dönüş yapınca pozun bozulması düzeltildi
- Üretilen menü simge boyutu değiştirildi, sıkıştırma açıldı ve daha küçük simge ayarı eklendi
- 3.0.0’da kaldırılan takip kilidi bütünleştirmesi ve avatar hareketini otomatik içe aktarma yeniden eklendi

## [3.0.1] - 2025-12-29
- Varsayılan hareket ayarı işlevinin sorunları düzeltildi
- Avatar pozunun kendi kendine bozulması düzeltildi

## [3.0.0] - 2025-12-28
- Ön oluşturma eklendi
- Animasyon Düzeltmesi eklendi
- Ön ayarlar eklendi

## [2.2.5] - 2025-05-08
- Bazı avatar özellikleriyle uyumluluk sorunları çözüldü

## [2.2.4] - 2025-04-04
- Bazı avatar ve ModularAvatar sürümlerinde pozun değişmemesi düzeltildi

## [2.2.3] - 2025-03-13
- 森羅’nın zıplaması gibi bazı avatar hareketlerinin bozulması düzeltildi
- Jestleri kapat başlangıç değerinin bazen yanlış olması düzeltildi

## [2.2.2] - 2025-03-06
- Bazı ortamlarda oluşan hata düzeltildi

## [2.2.1] - 2025-02-05
- Unity çökmesi düzeltildi
- NDMF Console’daki araç adına sürüm eklendi

## [2.2.0] - 2025-02-02
- Jestleri kapatıp animasyondaki el şeklini kullanma ayarı eklendi

## [2.1.6] - 2025-02-02
- Bazı araçlarla birlikte pozların bozulması düzeltildi
- Ayarlara bağlı hatalar düzeltildi

## [2.1.5] - 2024-12-10
- Bazı özelliklerle çakışmalar çözüldü
- Unitypackage sürümünü kaldırma düğmesi eklendi

## [2.1.4] - 2024-12-08
- Avatara bağlı hata düzeltildi
- Özellik veya avatar çakışmasıyla ayak kilidinin kullanılamaması düzeltildi
- Masaüstünde avatar pozunun VRC varsayılanına dönmesi düzeltildi

## [2.1.3] - 2024-12-01
- VRMode’u yanlış türde kullanan avatar veya özelliklerle pozun değişmemesi düzeltildi
- Farklı parametre türü çakışmalarını NDMF-Console’da gösterme eklendi

## [2.1.2] - 2024-12-01
- Diğer araçta hata varken avatar derlemesinde Unisaki Posing System hatası oluştuğunda NDMF-Console’a açıklama eklendi
- Bazı avatarların derleme hatası düzeltildi
- Avatar yüklenir yüklenmez davranış iyileştirildi
- Bazı avatarları çalışamaz hâle getirdiği için avatarın özgün hareket işlemi artık kaldırılır

## [2.1.1] - 2024-11-20
- PosingOverride’daki MergeAnimator sorunu düzeltildi

## [2.1.0] - 2024-11-20
- Masaüstünde ayakların ve kalçanın zemine gömülmesi giderilerek her pozun düzgün alınması sağlandı
- Baş yüksekliğini dikkate almadan poz değiştirme eklendi
- Quest avatarı yüklemek için simge oluşturmayan mod eklendi
- Avatar sorunları için çeşitli uyarılar eklendi
- AFK ve diğer araçların kol kilidi gibi özelliklerle kilitleme çakışmaları çözüldü
- Sırtüstüyken otomatik baş kilidi ayarı eklendi

## [2.0.0] - 2024-11-12
- VPM/VCC destekli sürüm yayınlandı
- Hata sırasında gösterilen araç adı değiştirildi #1
- Kullanım koşulları eklendi #5
