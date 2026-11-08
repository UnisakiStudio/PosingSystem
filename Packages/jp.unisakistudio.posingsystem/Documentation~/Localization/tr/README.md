# Unisaki Posing System

Kawaii Posing Tool, Cool Posing Tool ve benzeri araçların kullandığı ortak poz sistemidir. Avatar yüklenirken gereken menü ve animasyonları Modular Avatar ve NDMF ile oluşturur.

Bu README, geliştirilmekte olan v3.2.0 içindir. Resmî yayın tarihi henüz belirlenmemiştir. Kurulu sürümü poz aracının Inspector’ının üstünde görebilirsiniz. [Yeni özellikler ve değişiklik geçmişine](CHANGELOG.md) de bakın.

## Görüntüleme dili ve çevrimdışı belgeler

Inspector’ın üstündeki “Language / 言語” alanından dili seçin. Tek seçim hem Inspector’ı hem oluşturulan VRChat menüsünü etkiler. Oyundaki dili değiştirmek için avatarı yeniden yükleyin.

Dil seçilmemişse bilgisayarın dili, desteklenmeyen dillerde İngilizce kullanılır. Windows, macOS ve Linux desteklenir. Son seçim EditorPrefs’e kaydedilir ve bu bilgisayardaki aynı işletim sistemi kullanıcısının diğer projelerinde de kullanılır. Avatara açıkça kaydedilmiş seçim korunur. Başka bilgisayarlara eşitlenmez. Lisans saklama yöntemi değişmez.

README, değişiklik geçmişi ve kullanım kılavuzu, paketteki [dil listesinden](../index.html) seçilebilir. Inspector’daki kılavuz ve değişiklik geçmişi de seçilen dili izler. Özgün kullanım koşulları ve başvuru çevirileri eskisi gibi ayrı sağlanır.

## Masaüstü modunda poz değiştirme

Masaüstü modunda ayakta durma, çömelme ve oturma arasında geçerken seçilen pozun uygulanma gecikmesi azaltıldı. Normal derlemede ve ön oluşturmada özel komut veya ek ayar gerektirmeden çalışır. VR ve özel durumlar önceki işlemleri korur. Yüklemeden önce eski sürümün ön oluşturmasını güncelleyin.

## Kurulum ve sürüm yükseltme

Satın aldığınız ürünün dosyalarını ve kurulum adımlarını izleyerek poz aracını VCC veya ALCOM üzerinden ekleyin. Bu sistem poz aracının bağımlılığı olarak kurulur.

v3.2.0 gereksinimleri:

- Unity 2022.3 VRChat avatar projesi
- VRChat Avatars SDK 3.10.4 veya sonrası
- Modular Avatar 1.x serisi, en az 1.15.1
- NDMF (Non-Destructive Modular Framework) 1.x serisi, en az 1.14.0

Kawaii Posing Tool kullanıyorsanız ALCOM / VCC’de 3.2.0’a güncelleyin ve Unisaki Posing System’in de 3.2.0 olduğunu doğrulayın. Ön oluşturma kullandıysanız Ön oluşturmayı güncelle düğmesine basın, sahneyi kaydedin ve avatarı yükleyin.

Cool Posing Tool 1.2.0 ortak 3.2.0 sürümünü kullanır. Ürünü ve ortak sistemi birbiriyle uyumlu sürümlere birlikte güncelleyin.

### Eski 8bit sürümünü kullananlar için

Yeni eklemelerde artık normal Prefab kullanılır. Özellikler’de gereksiz öğeleri kapatarak kullanımı azaltabilirsiniz. Yalnızca poz seçiminin temel maliyeti 8 bit’tir.

Eski 8bit Prefab, mevcut avatar ve ön ayar başvurularını korumak için her ürünün Editor/Legacy klasöründe tutulur. Eski 8bit sürümünü kullanmayı sürdürmek için Prefab’ı silmeniz veya değiştirmeniz gerekmez. Unity’nin yeni ekleme menüsündeki 8bit seçimi kaldırılmıştır.

## Ayar ekranını tanıma

Ayarları açmak için Hierarchy’de avatarın içindeki 可愛いポーズ (Kawaii Posing), カッコいいポーズ (Cool Posing) veya benzeri aracı seçin.

| Sekme | Ayarlar |
| --- | --- |
| Özellikler | Oyun menüsü öğeleri, kolaylıklar, simgeler ve ExpressionParameters kullanımı |
| Pozlar | Poz seçimi, ekleme ve sıralama, Scene önizlemesi, poz ayrıntıları ve animasyon ayarlama |
| Avatar Kurulumu | Vücut oranlarına ve dizlere yönelik düzeltmeler, özgün pozları ve kendi hareketlerinizi içe aktarma, ön ayarlar |

Ek ayarları olan bir öğede Ayarları aç’a basınca alanlar altında açılır. Nefes ayarları da Özellikler’dedir.

Üstteki yardım ve değişiklik geçmişi panelinden kılavuzu, desteği ve geçmişi açabilirsiniz. Geçmiş görünümü ürün ile Unisaki Posing System arasında değiştirilebilir.

## Özellikler ve ExpressionParameters kullanımı

“Özellikler > Menü özellikleri” altında seçtiğiniz öğeler oyun menüsünde görünür. Örneğin Aynala’yı eklemek VRChat’te aynalamayı değiştirmenizi sağlar. Inspector’da seçmek tüm pozları sürekli aynalanmış tutmaz.

| Özellik | Ek bit kullanımı |
| --- | ---: |
| Aynala | 1 bit |
| Jestleri kapat | 1 bit |
| Nefes animasyonu ekle | 0 bit |
| Nefes (oyun içi açma/kapatma) | 1 bit |
| Ayak yüksekliği | 8 bit |
| Zemin yüksekliği | 8 bit |
| Fotoğraf modu (yalnızca kendi ekranınız) | 0 bit |
| Fotoğraf modunu eşitle | 16 bit |
| Fotoğraf modunda başı görünür tut | 0 bit |
| VRChat bekleme sallanmasını kapat | 0 bit |
| Baş dönüşünden kaynaklanan poz bozulmasını azalt | 1 bit (dönüşle ortak) |
| Dön | 10 bit (1 bit’i poz bozulmasını azaltmayla ortak) |

Dönüş; açma/kapatma için 1 bit, baş eğimi için 8 bit ve baş kilidi için 1 bit kullanır. Poz bozulmasını azaltma zaten açıksa dönüş eklemek kullanımı 9 bit artırır. Ortak kısım iki kez sayılmaz ve seçimler değişince gösterge güncellenir.

Gösterilen toplam yalnızca o avatara kurulu poz araçlarına aittir. Kıyafetler ve diğer özellikler dâhil tüm avatarın toplamı değildir. Birden fazla poz aracının ortak ayarları da tekrar sayılmaz. Poz sayısı temel numara aralığını aşarsa poz seçimi için ek bit gerekir.

### Ayak yüksekliği ve zemin yüksekliği

- “Ayak yüksekliği”: başı yerinde tutarak ayak tarafının yüksekliğini ayarlar. Ayakları zemine veya yatağa uydurmak için kullanın.
- “Zemin yüksekliği”: baş dâhil tüm vücudu yukarı veya aşağı taşır. 50% özgün yükseklik, 0% 3m aşağısı, 100% 3m yukarısıdır. Başlangıç değeri 50%’dir.

İkisi de oyundaki Ayarlar menüsündedir; Zemin yüksekliği, Ayak yüksekliği’nden sonra gelir. Zemin yüksekliğini merkezden uzaklaştırmak vücudu yukarı/aşağı taşır; yaklaşık 50%’ye dönmek özgün yüksekliği geri getirip düzeltmeyi bitirir. Ayrı bir havada durma anahtarı yoktur. Bakış noktası düzeltmesi havada durmanın başlangıcında ve zemin yüksekliği ayarlanırken yapılır.

Normal animasyonlarda ayak yüksekliği artık yükseklik ayarı için tüm vücut hareketini tekrar tekrar üretmez. Bu, oluşturulan animasyonların veri miktarını azaltır; ancak tüm avatarın boyutunun her durumda 1/3’e düşeceği anlamına gelmez.

### Fotoğraf modu

1. “Özellikler > Menü özellikleri > Fotoğraf modu”nu açın.
2. VRChat’te Fotoğraf modu’nu açıp Poz seç üzerinden bir poz seçin.
3. Mesafe ile en fazla 3m öne taşıyın, Yön ile döndürün. Mesafe 0’dan büyükse avatar size bakar.
4. Gerekirse Ayakları kilitle’yi açın. Başlangıçta kilit zorlanmadığı için önce konumu ayarlayabilirsiniz. Jestleri kapat menüye eklendiyse aynı ekranda el şeklini de değiştirebilirsiniz.
5. Fotoğraftan çık ile mesafeyi ve yönü sıfırlayıp ayak kilidini açın.

Fotoğraf modundaki hareket ve dönüş yalnızca kendi ekranınızda kullanıldığında 0 bit’tir. Konum ve yönü başkalarına da göstermek için “Kolaylıklar > Fotoğraf modunu eşitle”yi açın. Mesafe ve yön toplam 16 bit kullanır.

Yakındaki kendi avatarınızın başı kayboluyorsa “Kolaylıklar > Fotoğraf modunda başı görünür tut”u açın. Yalnızca fotoğraf sırasında kendi ekranınıza uygulanır. Çıkınca normal görünüm geri gelir.

### Yatış pozunu zıplayarak değiştirme

Hem sırtüstü hem yüzüstü poz seçiliyse yerinizde zıplayarak aralarında geçebilirsiniz. Yalnızca biri seçiliyse o poz korunur. Zıplayarak değiştirmek istemiyorsanız Unity’de “Özellikler → Kolaylıklar” altındaki Zıplayarak yatış pozunu değiştir’i kapatın. Ek eşitlenen parametre maliyeti 0 bit’tir. Birden fazla poz aracı olan avatarda herhangi birinde kapatmak tüm avatarı etkiler.

### Dön

Desteklenen yatış pozlarında başı sağa/sola eğince vücut da yumuşakça döner. Bu özellik tüm yatış pozlarını otomatik olarak desteklemez.

1. “Özellikler > Menü özellikleri > Dön”ü açın.
2. Pozlar’da kullanmak istediğiniz yatış pozunun ayrıntılarını açıp Dönüşü etkinleştir’i seçin.
3. Yan dönüş pozu’na yan yatış animasyonu atayın. Özgün poz ve yan pozdan sağa/sola dönüşler oluşturulur. Gerekirse Zemin düzeltme gücü’nü ayarlayın.
4. VRChat’te desteklenen pozu seçip Dön’ü açın. Baş kilidini kapatarak kullanın.

Dönüş kullanan pozlarda baş eğimi animasyonun oynatma konumunu belirler; normal MotionTime kullanılamaz. Hazır bir dönüş animasyonu kullanıyorsanız normal animasyon alanına atayıp Yan dönüş pozu’nu boş bırakın. Oynatma konumu 0 sol taraf, 0.5 sırtüstü, 1 sağ taraf olan bir animasyon hazırlayın.

### Nefes ve varsayılan sallanma

“Özellikler > Kolaylıklar > Nefes animasyonu ekle” ile göğüs gibi bölgeler nefesle birlikte hareket eder. Ayarları aç üzerinden her poz kategorisi için Yok, Hafif, Orta, Güçlü, Hızlı veya Özel seçilebilir. Hızlı, hızlı nefes hareketidir. Özel ile kendi hazırladığınız nefes animasyonunu atayabilirsiniz.

Yalnızca nefes hareketi 0 bit’tir. VRChat’te durdurup sürdürmek için “Menü özellikleri > Nefes”i de açın. Açma/kapatmanın eşitlenmesi 1 bit kullanır. Gücü değiştirmek bit sayısını artırmaz.

Boyun, omuz ve kollar da göğüsle birlikte hareket ederek başın ve ellerin yer değiştirmesini azaltır. Konum tamamen sabitlenmediği için gücü vücuda ve poza göre seçin.

“Kolaylıklar > VRChat bekleme sallanmasını kapat”, VRChat’in varsayılan bekleme sırasında eklediği vücut sallanmasını bastırır. Başlangıçta kapalıdır; aracın kendi nefesini ve diğer özel hareketleri korur. Nefes hareketi kullanılmasa da seçilebilir ve bit maliyetini artırmaz. Varsayılan bekleme hareketi dışındaki nedenlerle oluşan sallanmayı durdurmaz.

Birden fazla poz aracı kullanıldığında, herhangi birinde varsayılan sallanmayı durdurmak tüm avatara uygulanır. Geri almak için tüm araçlarda bu ayarı kapatıp yeniden yükleyin.

### Baş dönüşünden kaynaklanan poz bozulmasını azalt

Özgün pozdan farklı boyun yönü nedeniyle baş kilidi açılınca poz bozuluyorsa kullanın. “Özellikler > Kolaylıklar” altındadır ve ayrı oyun menüsü eklemez. Baş kilitliyken özgün poz yönü kullanılır. Sırtüstü yatış, özgün yukarı yönü korumak için düzeltmeden hariç tutulur.

### Menü simgeleri

- “Poz simgeleri oluştur”: avatarın her pozu aldığı görüntüleri oluşturup menüde gösterir. Pozları ayırt etmeyi kolaylaştırır, ancak görüntü üretimi derlemeyi uzatır.
- “Simge boyutunu küçült”: görüntü çözünürlüğünü düşürerek avatarın boyutunu azaltır.

Android derlemelerinde poz simgeleri oluşturulmaz. Kıyafet veya görünüm değişince Simgeleri yeniden çek’i kullanın. Poz listesinin görüntüleri Poz küçük resimlerini güncelle ile yenilenir. İşlem uzun sürebileceği için çalıştırmadan önce onay iletişimini okuyun.

## Poz ekleme, sıralama ve önizleme

Pozlar sekmesinin solundan kategori seçin. En üstteki büyüteç sekmesi kategoriler arasında poz adı arar. Ayrıntılar’da animasyon, simge, MotionTime ve diğer seçenekler birlikte ayarlanabilir.

Birden fazla animasyonu ekleme alanına sürükleyerek veya Project’te seçip seçili klipleri içe aktarma komutunu kullanarak topluca ekleyebilirsiniz. Aynı klibe yinelenen başvurular çıkarılır.

Sırayı değiştirmek için “Ayrıntılar > Sıra”dan hedef konumu seçin. Yukarı/aşağı düğmeleri de kullanılabilir. Taşımadan sonra ayrıntıların açık/kapalı durumu korunur.

Her pozun Önizleme düğmesi, düzeltme penceresini açmadan pozu Scene’de gösterir. Bitirince özgün görünüm geri gelir. Bu hafif önizleme MA veya NDMF derlemesi yapmadığından, derlemede eklenen kıyafetler veya FloorAdjuster’ın son sonucu farklı olabilir. Son kıyafet ve zemin temasını avatar derlendikten sonra da kontrol edin.

## Animasyon Düzeltmesi ve otomatik uzuv düzeltmesi (IK)

Pozlar’daki Animasyon Düzeltmesi’ni açarak vücudu ve uzuvları önizlemeyle ayrıntılı ayarlayabilirsiniz. İşiniz bitince pencerenin kaydetme işlemiyle düzeltme animasyonunu kaydedin.

Desteklenen ürün ve pozlarda otomatik düzeltme, uzuv konumlarını avatarın vücut yapısına uyarlar. Animasyon Düzeltmesi’ndeki Otomatik uzuv düzeltmesini etkinleştir genel ayarı, Bu poz için IK’yi etkinleştir tek pozun ayarını değiştirir. Otomatik düzeltmesi olmayan pozlarda bu durum belirtilir.

Otomatik sonucu elle tamamlamak için IK’yi düzeltmeye işle’ye basın. O anki IK sonucu yeni bir düzeltme animasyonuna yazılır ve avatarın ilgili pozunun IK’si kapatılır. Özgün animasyon korunur. Düğme, genel ve ilgili pozun otomatik düzeltmesi açık ve uygulanabilir IK ayarı varsa kullanılabilir.

Avatar Kurulumu’ndaki Dizin ters bükülmesini önle diz düzeltmesini değiştirir. Ayrıntılardaki En küçük diz bükülmesi (derece) ile açıyı belirleyin. Avatar düzeltmesini etkinleştir, uyumlu düzeltme verisi varsa kullanılabilir. Otomatik uzuv düzeltmesiyle çakışan bazı düzeltmeler birlikte kullanılamayabilir.

## Özgün pozlar, kendi hareketleriniz ve ön ayarlar

Bunlar Avatar Kurulumu’nda ayarlanır.

- “Özgün ayakta, çömelme ve yüzüstü pozları kullan”: avatarın başlangıçta ayarlanmış pozlarını otomatik içe aktarır.
- “Kendi hareketlerinizi kullanın”: içe aktarma ayarlarını açıp ayakta durma, çömelme (Crouching) ve yüzüstü (Prone) gibi her tür için ayrı hareket atayın. Genellikle her türe tek hareket atanır.
- “Takip kilitlerini diğer araçlarla bütünleştir”: diğer özelliklerle birlikte kilitler doğru çalışmıyorsa açın.

Kendi hareketlerinizi atadığınız içe aktarma ayarları, özgün pozları otomatik içe aktarmadan ayrı bir işlevdir.

Ön ayarlar sekmenin en altındadır. Adla arayıp Bu ön ayarı uygula ile poz ayarlarını değiştirin. Özellikle kurulumdaki başlangıç ayarı içindir ve mevcut ayarları değiştirir; yapılmış düzeltmeler varsa geçişten önce kaydedin.

Ön ayar listesi gösterilirken yalnızca adlar ve uyumlu avatar bilgileri yüklenir; düzeltme animasyonları seçili ön ayar uygulanırken yüklenir. Liste ilk oluşturulurken bile tüm avatarların animasyonları önceden yüklenmez.

Liste Uyumlu avatar, Kullanıcı ön ayarları ve Resmî Unisaki Studio ön ayarları olarak ayrılır. Hazır ön ayarları gizle, aynı projede ortak bir görünüm ayarıdır. Ön ayar ekleme, düzenleme ve silme listeyi yeniler. Görünüm değişmezse Yenile’ye basın.

## Ön oluşturma ve uyarılar

Özgün ayakta, çömelme ve yüzüstü pozları kullan açıksa, her üretimde avatarın Base katmanından hareketler algılanır. Sonuçlar elle içe aktarma ayarlarına yazılmadığından, yalnızca otomatik içe aktarma kullanıldığında liste atanmamış kalır. Base hareketinin değişmesi ve otomatik içe aktarmanın açılıp kapanması sonraki üretime yansır.

Yükleme sırasındaki otomatik içe aktarma derleme kopyasına uygulanır. Sahnedeki özgün nesneyi, içe aktarma ayarlarını, kaynak Controller’ı veya hareketleri değiştirmez. Ön oluşturma varsa ve girdiler değişmemişse sonucu yeniden kullanılır.

Elle atama olan türlerde elle ayarlar önceliklidir; dönüş, düzeltme animasyonu ve oynatma konumu da uygulanır. Kullan kapatılan veya atanmış hareketi çıkarılan ayar, o türün otomatik içe aktarmasını durdurur. Elle seçimi temizle ile türün ayar ve düzeltmelerini kaldırırsanız otomatik içe aktarma açıksa sonraki üretimde yeniden algılanır. Bu işlem Undo ile geri alınabilir. Aynı tür için birden çok eski ayar varsa Inspector’da seçilmiş geçerli ayar önceliklidir.

Eski otomatik içe aktarmanın listeye eklediği ayarlar, elle kaydedilip düzeltilmiş ayarlardan ayırt edilemediği için otomatik silinmez. Yalnızca otomatik içe aktarmaya döndürmek istediğiniz türlerde Elle seçimi temizle’yi kullanın. Aynı avatarda birden fazla ürün varsa diğer ürünlerdeki elle ayarlar da ortak özgün pozları etkiler.

Ön oluşturma, oluşturulan verileri yeniden kullanarak avatarın derlenmesini hızlandırır. Ön oluşturmayı güncelle ile oluşturulup güncellenebilir. Normal avatar derlemesi de gereken verileri oluşturduğundan her seferinde elle ön oluşturma gerekmez.

Ayarları değiştirince güncelleme gerekir; Inspector’ın üstündeki Ayarları kontrol et ve Hierarchy işareti bunu bildirir. Poz veya IK ayarlarını değiştirdikten sonra ön oluşturmayı güncellemek sonraki derlemeleri kısaltabilir.

Uyarı gösterim ayarları, Ayarları kontrol et çerçevesinin içindedir. Uyarı türlerini veya Hierarchy işaretini gizleyebilirsiniz. Gizlemek ayar sorununu çözmez ve oluşturulan veriyi güncellemez. Hatalar ve gereken üretim işlemleri aynı kalır.

## Kullanım notları

- 3 noktalı takip ve masaüstü kullanımı içindir. Tam vücut takibi sırasında poz kilitleme desteklenmez.
- Vücut yapısı ve yapılan değişiklikler, uzuv teması ve kıyafet görünümünde ek ayar gerektirebilir.
- Özel Base katmanları veya diğer poz değiştiren sistemlerle birleşim beklenen gibi çalışmayabilir.
- Fotoğraf modunun hareket ve dönüşü ile zemin yüksekliği; araç pozlarını ve poz seçilmemişken varsayılan/özel hareketleri düzeltir. VRChat’te bakış noktasını ve varsayılan hareketlerin değişiminden sonraki davranışı uygulamada kontrol edin.

## Kullanım koşulları ve destek

Bu depodaki kaynak kodu ve kaynaklar açık kaynak değildir. Kullanmak için ürünü yasal olarak satın almanız ve [ekli koşulları](../../../%E3%82%86%E3%81%AB%E3%81%95%E3%81%8D%E3%83%9D%E3%83%BC%E3%82%BA%E3%82%B7%E3%82%B9%E3%83%86%E3%83%A0%E5%88%A9%E7%94%A8%E8%A6%8F%E7%B4%84%EF%BC%88%E6%97%A5%E6%9C%AC%E8%AA%9E%E7%89%88%EF%BC%89.pdf) kabul etmeniz gerekir.

- [Kawaii Posing Tool kılavuzu](https://docs.unisakistudio.jp/kawaiiposing/manual/)
- [Cool Posing Tool kılavuzu](https://docs.unisakistudio.jp/coolposing/manual/)
- [İletişim](https://help.unisakistudio.jp/)

Hata bildirirken ürün ve Unisaki Posing System sürümlerini, Unity/VRChat SDK/Modular Avatar/NDMF sürümlerini, kullanılan avatarı, tekrarlama adımlarını ve Console hata içeriğini ekleyin.
