# Riwayat perubahan

## [3.2.0] - Belum dirilis

### Ditambahkan

- Inspector, operasi penyuntingan, dialog pemasangan, nama pose standar, dan menu VRChat yang dihasilkan kini tersedia dalam 30 bahasa.
- Pemilihan bahasa Inspector dan menu dalam permainan telah disatukan. Bahasa komputer digunakan pada awalnya, dan pilihan terakhir juga digunakan di proyek lain milik pengguna OS yang sama.
- README, riwayat perubahan, dan panduan luring kini dapat dibuka dalam bahasa terpilih.
- Nama tampilan 128 pose standar dan deskripsi setiap fitur disediakan sesuai istilah yang sudah ada.

### Kompatibilitas

- Pengaturan avatar yang ada, nama khusus, referensi aset, pengenal parameter, dan metode penyimpanan lisensi tetap dipertahankan.

## [3.1.2] - 2026-11-09

- Memperbaiki masalah ketika “Kurangi perubahan pose akibat rotasi kepala” aktif, wajah tetap menghadap ke atas setelah bangkit dari posisi tengkurap.
- Kunci kepala kini dapat dialihkan melalui “Pengaturan → Kunci Kepala Otomatis Saat Telentang” di menu EX meskipun “Tinggi kaki” nonaktif. Item yang hilang dari avatar pada 3.1.0 atau 3.1.1 juga muncul kembali setelah pembaruan dan build.
- “Perbarui prabuild” kini dapat digunakan meskipun objek avatar utama tidak dicentang di Unity.
- Memperbaiki masalah yang dapat mencegah pembuatan pose setelah pembaruan jika Controller pose bersama versi lama masih berada di Assets. Pengaturan Controller yang Anda edit sendiri juga dipertahankan.
- Jika alat atau induknya nonaktif dan Animator pose bersama tidak dapat digunakan, nama objek terkait ditampilkan. Anda dapat membukanya melalui “Periksa pengaturan → Pilih target”.
- Saat prabuild menunjukkan bahwa pengaturan telah berubah, Anda kini dapat memeriksa perbedaannya dari terakhir kali melalui “Rincian → Tampilkan pengaturan yang berubah sejak prabuild”.
- Saat kesalahan skrip menghentikan build (PS050), hingga 5 kesalahan asli yang dideteksi Unity ditampilkan. Melalui “Tanyakan tentang kesalahan ini”, Anda dapat memeriksa, mengedit, dan mengirim draf yang berisi versi Unity dan alat yang didukung beserta kode kesalahan.

Jika menggunakan prabuild, setelah pembaruan tekan “Perbarui prabuild”, simpan Scene, lalu lakukan build dan unggah avatar kembali.

## [3.1.1] - 2026-11-08

- “Tinggi lantai” dan “Berguling” yang diaktifkan kini muncul di “Pengaturan” menu EX VRChat meskipun “Cerminkan” nonaktif.
- Meskipun pose diurutkan ulang, dihapus, atau diganti namanya di versi lama, gerakan berguling yang benar tetap diwariskan ke “Kaki Lurus”, “Rileks”, dan “Satu Lutut Naik”. Ini juga berlaku setelah Unpack sepenuhnya jika pose asli dapat dikenali. Pose buatan sendiri, berguling yang Anda nonaktifkan, dan gerakan menyamping pengganti tetap dipertahankan.
- Memperbaiki masalah ketika avatar sesaat tengkurap saat dipilih di VRChat. Memperbaiki juga masalah ketika avatar sesaat telentang saat melompat dengan hanya pose tengkurap dipilih. Peralihan dengan melompat dapat dinonaktifkan melalui “Fitur → Kemudahan → Beralih pose berbaring dengan melompat”. Secara bawaan fitur ini aktif.
- Jika referensi Animator Controller untuk pose bersama hilang setelah pembaruan dan dapat dipulihkan, peringatan ditampilkan dan Controller dibuat ulang dari pengaturan. Referensi Controller yang digunakan bersama beberapa avatar juga dipertahankan.
- Jika proyek memiliki kesalahan kompilasi, build dihentikan pada awal dengan penjelasan penyebabnya. Pembersihan otomatis Controller yang dihasilkan ditunda selama referensi tidak dapat diperiksa akibat kesalahan skrip.
- Saat proyek berisi Prefab dengan nama yang sama dengan produk, Prefab resmi diprioritaskan ketika menambah alat. Prabuild juga dapat dilanjutkan jika Controller bersama hanya berbeda pada pergantian baris.
- Write Defaults pada layer FX yang ditambahkan alat pose, seperti Tinggi lantai, kini mengikuti pengaturan avatar.

Jika menggunakan prabuild, setelah pembaruan tekan “Perbarui prabuild” pada alat di Scene, simpan Scene, lalu lakukan build dan unggah avatar kembali.

## [3.1.0] - 2026-11-07

- Menambahkan penyesuaian anggota tubuh otomatis (IK) sesuai proporsi tubuh untuk pose yang didukung, koreksi khusus avatar, dan pencegahan lutut menekuk terbalik.
- Mode foto memungkinkan penyesuaian posisi dan arah avatar. Sinkronisasi posisi dan arah serta tampilan kepala sendiri selama pemotretan juga dapat dipilih.
- “Tinggi lantai” dapat menggerakkan seluruh tubuh hingga 3m ke atas dan ke bawah, terpisah dari “Tinggi kaki”.
- Menambahkan animasi napas, pengaturan untuk menghentikan goyangan diam bawaan, serta pengurangan perubahan pose akibat arah leher.
- Fitur yang digunakan dan penggunaan parameter dapat diperiksa di Inspector. Menambahkan pencarian pose, penambahan sekaligus, pengurutan, dan pratinjau sederhana di Scene.
- Mempercepat peralihan berdiri, setengah jongkok, dan duduk dalam mode desktop.
- Memperbaiki peringatan prabuild setelah perubahan pengaturan, beban pengguliran Inspector, dan jeda panjang saat membersihkan hasil yang dihasilkan.
- Memperbaiki masalah ketika impor otomatis ON, prabuild yang pengaturannya tidak berubah menunjukkan peringatan pembaruan dan dibuat ulang saat unggah. Hasil impor diterapkan pada avatar yang dihasilkan tanpa mengubah pengaturan impor objek asli.
- Menambahkan berguling mengikuti kemiringan kepala dan pengaturan gerakan yang sesuai untuk setiap pose.
- Memperbaiki pewarisan pengaturan berguling dari prasetel lama, impor gerakan sendiri, dan pengelolaan aset yang dihasilkan.

## [3.1.0-beta.2] - Beta terbuka (2026-11-05)

- Menambahkan panjang tetap serta selisih tinggi (Y) dan depan/belakang (Z) antara Head dan Viewpoint sebagai dasar panjang posisi IK.
- Peringatan Inspector kini juga muncul di sebelah kiri nama objek di Hierarchy. Mencakup prabuild yang belum dijalankan, perubahan pengaturan, dan peringatan yang sedang ditampilkan untuk impor gerakan serta operasi prasetel.
- Memperbaiki masalah ketika pembersihan otomatis hasil yang dihasilkan, misalnya saat membuka Scene, memuat Controller dari Scene lain dan menyebabkan Unity berhenti lama.
- Memperbaiki riwayat prabuild yang hilang setelah perubahan daftar pose atau fitur. Kini ditampilkan bahwa pengaturan berubah setelah prabuild, bukan bahwa prabuild belum dijalankan, dan Hierarchy juga menunjukkan peringatan pembaruan.
- Hasil perbandingan prabuild untuk Inspector dan Hierarchy kini dibagikan agar pengguliran lebih ringan selama pengaturan serta aset referensi tidak berubah. Perbandingan saat build tetap dijalankan setiap kali.

## [3.1.0-beta.1] - Beta terbuka (2026-11-04)

- Mengubah peralihan pose normal dalam mode desktop menjadi satu evaluasi. Berlaku untuk build biasa dan prabuild; VR dan kondisi khusus tetap menggunakan jalur sebelumnya.
- Prabuild dari alpha.3 atau versi sebelumnya kini dikenali sebagai perlu diperbarui.
- Mempertahankan sudut pandang saat Tinggi lantai diubah selama pemotretan, lalu kembali ke Tinggi lantai terbaru setelah pemotretan selesai.

- Impor otomatis kini hanya melakukan deteksi saat pembuatan, sehingga prabuild/build tidak menambah atau mengubah pengaturan impor manual.
- Status ON/OFF impor otomatis, perubahan Base, dan penghapusan pengaturan manual diterapkan saat pembuatan ulang. Pilihan manual serta penyesuaian bersama beberapa produk dipertahankan, dan gerakan impor lama dikembalikan ke pose bawaan.
- Controller prabuild dikelola dalam 1 file per avatar, dikelompokkan berdasarkan GUID Scene. Referensi bersama akibat duplikasi, Scene lain, atau Prefab dipisahkan sebelum pembaruan.
- Build menggunakan pengelolaan aset sementara NDMF tanpa menambah file hasil untuk penyuntingan.
- Hasil diperiksa saat Scene dibuka dan disimpan. File dengan perubahan belum disimpan, Undo/Redo, atau referensi eksternal dipertahankan; hanya file tidak terpakai dengan kepemilikan jelas yang dipindahkan ke tempat sampah.
- Rotasi, animasi penyesuaian, dan posisi pemutaran gerakan impor kini diterapkan. Pemrosesan rotasi, RootT/RootQ, dan penyesuaian Humanoid disatukan dengan pose biasa.
- Penyesuaian BlendTree bertingkat diduplikasi khusus untuk tujuan impor, sehingga gerakan asli dan pengaturan pencampuran untuk berjalan tetap dipertahankan.
- Posisi pemutaran “hanya berdiri” dikembangkan menjadi pose diam pada laju bingkai asli tanpa menghentikan pemutaran untuk berjalan.

## [3.1.0-alpha.2] - Uji tertutup (2026-10-26)

### Perbaikan dan perubahan

- Koreksi Mode foto dan Tinggi lantai kini juga diterapkan pada gerakan bawaan dan khusus ketika pose alat tidak dipilih.
- Memperluas Tinggi lantai hingga 3m ke atas dan ke bawah. 50% menjadi tinggi asli dan nilai awal, 0% berarti 3m lebih rendah, dan 100% berarti 3m lebih tinggi.
- Memperbaiki penimpaan “hanya berdiri” yang mengubah BlendTree sumber impor.
- Menggunakan koreksi manual untuk pose tanpa aturan IK dan meniadakan peringatan pembuatan yang tidak diperlukan.
- Memperbaiki tercampurnya draf penyesuaian pose bernama sama dan perubahan gerakan dalam aset yang sama yang terlewat.
- Memperbaiki nomor pose awal yang tidak valid menyebabkan pose lain dipilih pada awal.
- Menampilkan petunjuk, bukan pengecualian, saat mengimpor AnimatorOverrideController yang tidak didukung.
- Mempertahankan status berguling OFF yang ditentukan secara eksplisit saat menerapkan prasetel lama.
- Memperjelas ikon fitur pada tema Editor terang.

Pemeriksaan sudut pandang dan penggantian gerakan bawaan dalam klien VRChat masih berlangsung.

## [3.1.0-alpha.1] - Sebelum publikasi

### Ditambahkan

- Menambahkan ON/OFF per fitur dan tampilan penggunaan ExpressionParameters.
- Menambahkan Mode foto untuk memindahkan avatar ke depan dan memutarnya.
- Menambahkan pilihan sinkronisasi Mode foto: hanya lokal menggunakan 0 bit, sedangkan sinkronisasi menggunakan 16 bit.
- Menambahkan pengaturan agar kepala avatar sendiri tetap terlihat dalam Mode foto.
- Menambahkan “Tinggi lantai” untuk memindahkan seluruh tubuh, termasuk kepala, 3m ke atas dan ke bawah. 50% adalah tinggi asli dan nilai awal, 0% berarti 3m lebih rendah, dan 100% berarti 3m lebih tinggi.
- Menambahkan berguling sesuai kemiringan kepala dan pengaturan animasi yang sesuai untuk setiap pose.
- Menambahkan animasi napas beserta pilihan Lemah, Sedang, Kuat, Cepat, dan Khusus.
- Menambahkan pilihan terpisah untuk animasi napas dan menu ON/OFF dalam permainan. Animasi saja menggunakan 0 bit; sinkronisasi ON/OFF menggunakan 1 bit.
- Menambahkan pengaturan untuk menghentikan goyangan dari gerakan diam bawaan VRChat.
- Menambahkan pengaturan untuk mengurangi perubahan pose akibat perbedaan arah leher.
- Menambahkan penyesuaian anggota tubuh otomatis (IK) untuk mencocokkan pose yang didukung dengan proporsi tubuh, beserta ON/OFF seluruh avatar dan per pose.
- Menambahkan “Rekam hasil IK ke penyesuaian” untuk menyimpan hasil IK sebagai animasi penyesuaian.
- Menambahkan koreksi pose khusus avatar serta pencegahan lutut menekuk terbalik dan pengaturan sudut tekukan minimum.
- Menambahkan pratinjau sederhana untuk memeriksa pose di Scene dari daftar pose.
- Menambahkan tab kategori pose, pencarian lintas kategori, penambahan beberapa animasi sekaligus, dan penentuan posisi tujuan dalam urutan.
- Menambahkan tampilan riwayat perubahan produk dan sistem bersama di Inspector.

### Diubah

- Mempercepat pemuatan prasetel dan mengurangi waktu tunggu saat menambah alat atau menampilkan daftar prasetel.
- Daftar kini otomatis mencerminkan penambahan, penyuntingan, dan penghapusan prasetel.
- Menata Inspector dalam 3 tab: “Fitur”, “Pose”, dan “Pengaturan Avatar”.
- Menambahkan deskripsi dan ikon untuk pengaturan serta menyeragamkan tampilan buka/tutup pengaturan rinci.
- Memindahkan rincian napas ke pengaturan fitur dan prasetel ke bagian bawah Pengaturan Avatar.
- Memisahkan pilihan gerakan sendiri dan impor otomatis pose asli avatar menjadi pengaturan tersendiri.
- Mengurangi duplikasi animasi seluruh tubuh yang dihasilkan untuk penyesuaian Tinggi kaki.
- Mengubah pembaruan gambar pose menjadi tombol dengan dialog konfirmasi.
- Mengumpulkan pengaturan tampilan peringatan dalam “Periksa pengaturan”.
- Menghentikan penambahan Prefab khusus 8bit baru dan menyatukannya dengan pilihan fitur pada Prefab biasa. Referensi yang ada dipertahankan.
- Mengubah versi minimum VRChat Avatars SDK menjadi 3.10.4.

### Diperbaiki

- Gerakan bawaan dan khusus ketika pose alat tidak dipilih kini juga dikoreksi oleh Mode foto dan Tinggi lantai.
- Memperbaiki masalah penimpaan “hanya berdiri” yang mengubah BlendTree sumber impor.
- Menggunakan penyesuaian manual untuk pose tanpa aturan IK yang valid dan meniadakan peringatan pembuatan IK yang tidak diperlukan.
- Memperbaiki penyesuaian pose bernama sama yang tercampur serta penggantian gerakan dalam aset yang sama yang tidak diterapkan saat pembuatan ulang.
- Mencegah pose awal terpilih jika nomor pose awal yang dinonaktifkan digunakan kembali oleh pose lain.
- Menampilkan petunjuk, bukan pengecualian, saat impor pose asli mendeteksi AnimatorOverrideController yang tidak didukung.
- Mempertahankan pengaturan berguling yang secara eksplisit OFF saat menerapkan prasetel lama.
- Memperbaiki latar belakang ikon Inspector agar ikon fitur tetap dapat dikenali pada tema Editor terang.

- Memperbaiki ikon “Berguling”, “Tinggi lantai”, dan “Mode foto” yang tidak muncul saat alat ditambahkan ke avatar.
- Memperbaiki hilangnya pengaturan berguling pose yang didukung ketika menerapkan prasetel versi lama.
- Memperbaiki input yang berpindah ke pose lain jika rincian lain dibuka saat nama pose sedang diedit.
- Memperbaiki rincian yang tertutup saat pose diurutkan ulang serta pemrosesan Undo dan Redo perubahan pengaturan.
- Memperbaiki avatar asli yang tumpang tindih dalam pratinjau dan posisi penempatan yang ditambahkan dua kali.
- Memperbaiki kesalahan pratinjau pada avatar dengan komponen yang hilang.
- Memperbaiki arah dan posisi anggota tubuh IK yang bergeser akibat perbedaan arah serta skala tulang avatar.
- Memperbaiki peringatan atau status prabuild yang tidak langsung diperbarui setelah pengaturan berubah.
- Memperbaiki penggunaan kembali hasil prabuild lama setelah pengaturan IK diubah.
- Memperbaiki kesalahan build akibat kegagalan pembuatan fitur tambahan pada konfigurasi tertentu.

## [3.0.16] - 2026-09-14
- Memperbarui versi minimum dependensi NDMF menjadi 1.14.0 karena menggunakan API `VirtualAnimatorController.SetParameter`.

## [3.0.15] - 2026-09-13
- Memperbaiki perubahan besar posisi tubuh dan tekukan lutut saat kunci leher pose telentang dialihkan pada avatar yang menggabungkan perubahan skala Hips dan FloorAdjuster.

## [3.0.14] - 2026-09-09
- Memperbaiki item menu “Gestur mati” yang hilang pada alat kedua dan berikutnya ketika beberapa alat pose dipasang pada avatar yang sama.
- Memperbaiki layer USSPS_Locomotion yang terkadang kembali ke Standing meskipun pose seperti SleepDown ditentukan.
- Memperbaiki pergeseran tinggi serta posisi X/Z pose saat digunakan dengan FloorAdjuster, dan mendukung urutan pemrosesan versi MA dan AAO.
- Jika AnimatorController prabuild dalam Generated dihapus, kini otomatis kembali ke PosingSystem_Locomotion bawaan.
- Memperbaiki kesalahan build pada konfigurasi yang semua pose berdirinya dihapus.
- Mengurutkan daftar prasetel berdasarkan nama avatar.

## [3.0.13] - 2026-09-04
- Memperbaiki transisi ke dirinya sendiri pada pose berdiri yang membuat pemulihan pose tidak stabil ketika kembali berdiri segera setelah mulai jongkok dalam mode VR.

## [3.0.12] - 2026-09-04
- Memperbaiki penilaian keliru bahwa pengaturan berubah saat alat pose tanpa prabuild di-build dengan NDMF.
- Memperbaiki kegagalan dengan kesalahan `AddAssetToSameFile` akibat menambahkan aset hasil ke AnimatorController sementara selama build NDMF.

## [3.0.11] - 2026-08-31
- Memperbaiki kegagalan proses karena Scene avatar prabuild menjadi tidak valid pada NDMF 1.14.7 dan yang lebih baru.
- Memperbaiki `MissingReferenceException` ketika layar kesalahan NDMF merujuk LocalizationAsset yang sudah dihancurkan.

## [3.0.10] - 2026-08-29
- Menunda pemuatan aset definisi prasetel hingga UI pemilihan prasetel dibuka, dan menggunakan kembali cache jika katalog GUID tidak berubah.
- Mengevaluasi ulang ikon peringatan/kesalahan Hierarchy hanya ketika ada perubahan serta membagikan cache pencarian aset menu produk. Mengurangi Editor yang membeku setiap beberapa detik pada Scene dengan banyak alat pose.
- Memperbaiki `ArgumentException` saat nilai parameter duplikat didaftarkan dalam prabuild beberapa alat pose pada avatar yang sama.

## [3.0.9] - 2026-08-26
- Jika VRCAvatarDescriptor tidak dapat diperoleh dari PosingSystem selama konversi, mencegah NullReferenceException dan menunjukkan log build yang diperlukan untuk penyelidikan.
- Memperbaiki kegagalan `Avatar is null` ketika proses prabuild/build berikutnya menggunakan kembali pratinjau yang merujuk Avatar sementara buatan NDMF yang sudah dihapus. Memisahkan konversi dari cache pratinjau dan mencegah benturan nama aset sementara NDMF antarpratinjau.
- Memperbaiki referensi pratinjau Prefab 可愛いポーズ (Kawaii Posing) yang tidak tersimpan dalam Scene sehingga avatar pratinjau tersembunyi terus bertambah. Pratinjau kini dikecualikan dari penyimpanan Scene, dan pratinjau lama dibersihkan saat Scene dimuat, beralih ke Play, atau skrip dimuat ulang.

## [3.0.8] - 2026-08-18
- Memperbaiki kemungkinan data pose rusak dan Unity Assertion pada gerakan pose prabuild ketika tinggi dikoreksi kembali oleh FloorAdjuster atau alat serupa.
- Meningkatkan keamanan agar pengukuran tinggi tidak mengganggu AnimationMode avatar asli atau alat lain.

## [3.0.7] - 2026-08-12
- Memperbaiki sudut pandang yang bergeser dan lutut yang menekuk tidak wajar saat telentang jika digunakan dengan alat pengatur tinggi seperti FloorAdjuster.
- Memperbaiki kesalahan jika PosingSystem berada di luar avatar (terima kasih, PancakeTorto1se).
- Memperbaiki proses yang tetap berjalan dan menimbulkan kesalahan setelah unggah avatar gagal.
- Memperbaiki penilaian keliru bahwa pengaturan telah diperbarui meskipun prabuild baru saja selesai.
- Memperbaiki impor otomatis gerakan berpindah.
- Menambahkan peringatan untuk beberapa avatar dengan rig khusus.
- Menambahkan dialog konfirmasi perubahan dan pencerminan nilai penyesuaian pada jendela Penyesuaian Animasi (terima kasih, kraihd).
- Menambahkan petunjuk ketika alat lain mengalami kesalahan serta pemeriksaan di lokasi yang sering bermasalah.

## [3.0.6] - 2026-03-17
- Memperbaiki pose yang menjadi sangat kacau pada beberapa avatar yang memiliki kendali pose sendiri sejak awal.

## [3.0.5] - 2026-01-31
- Memperbaiki Unity yang mengalami crash pada ExpressionMenus tertentu.
- Memperbaiki masalah avatar saat menggunakan Prefab integrasi ごろ寝EX (terima kasih, あずき).
- Memperbaiki rotasi yang tidak benar saat menggabungkan animasi penyesuaian.

## [3.0.4] - 2026-01-11
- Memperbaiki sebagian perilaku avatar ketika dimasukkan ke dalam Prefabs.
- Menambahkan fitur penghapusan lisensi.
- Memperbaiki banyak pose dengan posisi leher yang tidak benar.
- Sedikit mengurangi beban saat membuka Inspector.
- Prabuild kini juga membuat ikon. Mengubah teks tombol pembaruan avatar.
- Menyembunyikan peringatan jika prabuild belum pernah dijalankan, karena banyak pengguna tampaknya tidak menggunakannya.
- Menghapus sumber daya yang tidak diperlukan.
- Memperbaiki jari yang tidak dapat disesuaikan.

## [3.0.3] - 2026-01-04
- Memperbaiki prasetel yang diterapkan berbeda dari yang dipilih (terima kasih, あずき).

## [3.0.2] - 2026-01-02
- Mendukung tema gelap Unity.
- Memperbaiki pose yang menjadi tidak benar saat melakukan rotasi di Penyesuaian Animasi.
- Mengubah ukuran ikon menu yang dihasilkan dan mengaktifkan kompresi, serta menambahkan pilihan ikon yang lebih kecil.
- Mengembalikan integrasi pelacakan dan impor otomatis gerakan avatar yang sempat dihapus pada 3.0.0.

## [3.0.1] - 2025-12-29
- Memperbaiki masalah pada fitur pengaturan gerakan bawaan.
- Memperbaiki pose avatar yang berubah menjadi tidak benar dengan sendirinya.

## [3.0.0] - 2025-12-28
- Menambahkan fitur prabuild.
- Menambahkan fitur Penyesuaian Animasi.
- Menambahkan fitur prasetel.

## [2.2.5] - 2025-05-08
- Menyelesaikan masalah kompatibilitas dengan beberapa alat.

## [2.2.4] - 2025-04-04
- Memperbaiki pose yang tidak dapat berubah pada kombinasi avatar dan versi ModularAvatar tertentu.

## [2.2.3] - 2025-03-13
- Memperbaiki gerakan yang tidak benar pada beberapa avatar, termasuk lompatan 森羅.
- Memperbaiki nilai awal Gestur mati yang terkadang tidak benar.

## [2.2.2] - 2025-03-06
- Memperbaiki kesalahan pada lingkungan tertentu.

## [2.2.1] - 2025-02-05
- Memperbaiki Unity yang mengalami crash.
- Menambahkan versi pada nama alat yang ditampilkan di NDMF Console.

## [2.2.0] - 2025-02-02
- Menambahkan pengaturan untuk menonaktifkan gestur dan menggunakan bentuk tangan dari animasi.

## [2.1.6] - 2025-02-02
- Memperbaiki pose yang rusak saat digunakan bersama alat tertentu.
- Memperbaiki kesalahan akibat pengaturan tertentu.

## [2.1.5] - 2024-12-10
- Menyelesaikan konflik dengan beberapa alat.
- Menambahkan tombol untuk menghapus versi Unitypackage.

## [2.1.4] - 2024-12-08
- Memperbaiki kesalahan pada avatar tertentu.
- Memperbaiki Kunci kaki yang tidak dapat digunakan akibat konflik dengan alat atau avatar.
- Memperbaiki pose avatar yang kembali ke pose bawaan VRC dalam mode desktop.

## [2.1.3] - 2024-12-01
- Memperbaiki pose yang tidak berubah jika digunakan bersama avatar atau alat yang menggunakan tipe VRMode yang salah.
- Menambahkan tampilan NDMF-Console untuk konflik antaralat akibat perbedaan tipe parameter.

## [2.1.2] - 2024-12-01
- Menambahkan penjelasan di NDMF-Console karena kesalahan alat lain menyebabkan kesalahan Unisaki Posing System saat build avatar.
- Memperbaiki kesalahan build pada beberapa avatar.
- Memperbaiki perilaku segera setelah avatar dimuat.
- Menghapus pemrosesan perpindahan asli avatar karena dapat mencegah alat bekerja pada avatar tertentu.

## [2.1.1] - 2024-11-20
- Memperbaiki masalah terkait MergeAnimator pada PosingOverride.

## [2.1.0] - 2024-11-20
- Mengatasi kaki atau pinggul yang tenggelam ke lantai dalam mode desktop sehingga semua pose dapat ditampilkan dengan baik.
- Menambahkan fitur mengubah pose tanpa memperhitungkan tinggi kepala.
- Menambahkan mode tanpa pembuatan ikon untuk unggah avatar Quest.
- Menambahkan beberapa peringatan jika avatar bermasalah.
- Menyelesaikan konflik penguncian dengan AFK atau alat pengunci lengan lain.
- Menambahkan pengaturan kunci leher otomatis saat telentang.

## [2.0.0] - 2024-11-12
- Merilis versi yang mendukung VPM/VCC.
- Mengubah nama tampilan alat saat terjadi kesalahan #1.
- Menambahkan ketentuan penggunaan #5.
