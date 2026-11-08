# Unisaki Posing System

Sistem pose bersama untuk Kawaii Posing Tool, Cool Posing Tool, dan alat terkait. Sistem ini memakai Modular Avatar dan NDMF untuk membuat menu serta animasi yang diperlukan saat avatar diunggah.

README ini ditujukan untuk v3.2.0 yang sedang dikembangkan. Tanggal rilis resmi belum ditetapkan. Versi terpasang terlihat di bagian atas Inspector alat pose. Lihat juga [fitur tambahan dan riwayat perubahan](CHANGELOG.md).

## Bahasa tampilan dan dokumen offline

Pilih bahasa melalui “Language / 言語” di bagian atas Inspector. Satu pilihan berlaku untuk Inspector dan menu VRChat yang dibuat. Unggah ulang avatar untuk mengubah bahasa di dalam gim.

Jika belum dipilih, bahasa komputer digunakan; bahasa yang tidak didukung memakai bahasa Inggris. Windows, macOS, dan Linux didukung. Pilihan terakhir disimpan di EditorPrefs dan digunakan di proyek lain milik pengguna OS yang sama pada komputer ini. Pilihan eksplisit yang sudah tersimpan pada avatar dipertahankan. Pilihan tidak disinkronkan ke komputer lain. Cara penyimpanan lisensi tidak berubah.

README, riwayat perubahan, dan panduan dapat dipilih melalui [daftar bahasa](../index.html) yang disertakan. Tombol panduan dan riwayat perubahan di Inspector juga mengikuti bahasa pilihan. Ketentuan asli dan terjemahan referensi tetap disediakan secara terpisah.

## Peralihan pose dalam mode desktop

Jeda hingga pose pilihan diterapkan saat beralih antara berdiri, berjongkok, dan duduk dalam mode desktop telah dikurangi. Berlaku pada build biasa dan prabuild tanpa perintah khusus atau pengaturan tambahan. VR dan kondisi khusus tetap memakai pemrosesan sebelumnya. Perbarui prabuild versi lama sebelum mengunggah.

## Pemasangan dan pembaruan versi

Tambahkan alat pose melalui VCC atau ALCOM sesuai file pembelian dan petunjuk setiap produk. Sistem ini dipasang sebagai dependensi alat pose.

Persyaratan v3.2.0:

- Proyek avatar VRChat Unity 2022.3
- VRChat Avatars SDK 3.10.4 atau lebih baru
- Modular Avatar seri 1.x, versi 1.15.1 atau lebih baru
- NDMF (Non-Destructive Modular Framework) seri 1.x, versi 1.14.0 atau lebih baru

Jika memakai Kawaii Posing Tool, perbarui ke 3.2.0 melalui ALCOM / VCC dan pastikan Unisaki Posing System juga menjadi 3.2.0. Jika sebelumnya memakai prabuild, tekan Perbarui prabuild, simpan scene, lalu unggah avatar.

Cool Posing Tool 1.2.0 memakai sistem bersama 3.2.0. Perbarui produk dan sistem bersama sekaligus ke versi yang sesuai.

### Untuk pengguna versi 8bit lama

Penambahan baru kini memakai Prefab biasa. Matikan item yang tidak diperlukan di Fitur untuk mengurangi penggunaan. Konfigurasi dasar yang hanya memilih pose memakai 8 bit.

Prefab 8bit lama tetap disimpan dalam Editor/Legacy setiap produk untuk mempertahankan referensi avatar dan prasetel yang sudah ada. Anda tidak perlu menghapus atau mengganti Prefab untuk terus memakai versi 8bit lama. Opsi versi 8bit dalam menu penambahan baru Unity telah dihapus.

## Mengenal layar pengaturan

Pilih objek 可愛いポーズ (Kawaii Posing), カッコいいポーズ (Cool Posing), atau alat terkait di dalam avatar melalui Hierarchy untuk mengatur.

| Tab | Pengaturan |
| --- | --- |
| Fitur | Item menu gim, kemudahan, ikon, dan penggunaan ExpressionParameters |
| Pose | Pemilihan, penambahan, dan urutan pose, pratinjau Scene, detail pose, serta penyesuaian animasi |
| Pengaturan Avatar | Koreksi proporsi tubuh dan lutut, impor pose asli/gerakan sendiri, serta prasetel |

Jika sebuah item memiliki pengaturan terperinci, tekan Buka pengaturan untuk membuka bidang di bawahnya. Pengaturan napas juga ada di Fitur.

Panel bantuan dan riwayat perubahan di atas membuka panduan, dukungan, serta riwayat perubahan. Anda dapat beralih antara riwayat produk dan Unisaki Posing System.

## Fitur dan penggunaan ExpressionParameters

Item yang dipilih dalam “Fitur > Fitur menu” muncul di menu gim. Misalnya, Cerminkan menambahkan sakelar untuk mengubah pencerminan di VRChat. Memilihnya di Inspector tidak memaksa semua pose selalu dicerminkan.

| Fitur | Bit tambahan |
| --- | ---: |
| Cerminkan | 1 bit |
| Gestur mati | 1 bit |
| Animasikan napas | 0 bit |
| Napas (sakelar dalam gim) | 1 bit |
| Tinggi kaki | 8 bit |
| Tinggi lantai | 8 bit |
| Mode foto (hanya layar sendiri) | 0 bit |
| Sinkronkan mode foto | 16 bit |
| Tetap tampilkan kepala dalam mode foto | 0 bit |
| Nonaktifkan goyangan diam VRChat | 0 bit |
| Kurangi perubahan pose akibat rotasi kepala | 1 bit (bersama berguling) |
| Berguling | 10 bit (1 bit bersama pengurangan perubahan pose) |

Berguling memakai 1 bit untuk sakelar, 8 bit untuk kemiringan kepala, dan 1 bit untuk kunci kepala. Jika pengurangan perubahan pose sudah aktif, penambahan Berguling memakai 9 bit tambahan. Bagian bersama tidak dihitung dua kali dan tampilan diperbarui saat pilihan berubah.

Jumlah yang ditampilkan hanya untuk alat pose yang dipasang pada avatar itu. Bukan total seluruh avatar termasuk pakaian dan fitur lain. Pengaturan bersama beberapa alat pose juga tidak dihitung berulang. Jika jumlah pose melebihi rentang nomor dasar, pemilihan pose memerlukan bit tambahan.

### Tinggi kaki dan tinggi lantai

- “Tinggi kaki”: menyesuaikan ketinggian sisi kaki sambil mempertahankan posisi kepala. Gunakan untuk menyelaraskan kaki dengan lantai atau tempat tidur.
- “Tinggi lantai”: memindahkan seluruh tubuh termasuk kepala ke atas/bawah. 50% adalah tinggi asli, 0% adalah 3m lebih rendah, dan 100% adalah 3m lebih tinggi. Nilai awal adalah 50%.

Keduanya ada di Pengaturan dalam gim, dengan Tinggi lantai setelah Tinggi kaki. Menggeser tinggi lantai dari tengah memindahkan tubuh ke atas/bawah; kembali mendekati 50% memulihkan tinggi asli dan mengakhiri koreksi. Tidak ada sakelar melayang terpisah. Koreksi sudut pandang dilakukan saat mulai melayang dan saat tinggi lantai dioperasikan.

Pada animasi biasa, tinggi kaki kini tidak menghasilkan gerakan seluruh tubuh berulang kali untuk pengaturan ketinggian. Ini mengurangi data animasi yang dibuat, tetapi tidak berarti ukuran seluruh avatar selalu menjadi 1/3.

### Mode foto

1. Aktifkan “Fitur > Fitur menu > Mode foto”.
2. Buka Mode foto di VRChat dan pilih pose melalui Pilih pose.
3. Gunakan Jarak untuk bergerak hingga 3m ke depan dan Arah untuk memutar. Saat jarak lebih besar dari 0, avatar menghadap Anda.
4. Aktifkan Kunci kaki bila perlu. Kaki tidak dipaksa terkunci di awal, sehingga posisi dapat disesuaikan lebih dahulu. Jika Gestur mati ditambahkan ke menu, bentuk tangan juga dapat diganti di layar yang sama.
5. Keluar foto mengembalikan jarak/arah dan membuka kunci kaki.

Gerakan dan rotasi mode foto memakai 0 bit jika hanya untuk layar sendiri. Untuk memperlihatkan posisi dan arah kepada orang lain, aktifkan “Kemudahan > Sinkronkan mode foto”. Jarak dan arah memakai total 16 bit.

Jika kepala avatar sendiri menghilang dari dekat, aktifkan “Kemudahan > Tetap tampilkan kepala dalam mode foto”. Hanya berlaku pada layar sendiri selama memotret. Tampilan normal kembali setelah keluar.

### Beralih pose berbaring dengan melompat

Jika pose telentang dan tengkurap sama-sama dipilih, Anda dapat beralih dengan melompat di tempat. Jika hanya satu dipilih, pose itu dipertahankan. Untuk menonaktifkannya, matikan Beralih pose berbaring dengan melompat pada “Fitur → Kemudahan” di Unity. Biaya parameter tersinkron tambahan adalah 0 bit. Pada avatar dengan beberapa alat pose, mematikannya di salah satu alat berlaku untuk seluruh avatar.

### Berguling

Dalam pose berbaring yang didukung, tubuh berguling halus mengikuti kepala yang dimiringkan ke kiri/kanan. Fitur ini tidak otomatis mendukung semua pose berbaring.

1. Aktifkan “Fitur > Fitur menu > Berguling”.
2. Di Pose, buka detail pose berbaring yang diinginkan dan aktifkan Aktifkan berguling.
3. Tetapkan animasi menyamping pada Pose berguling samping. Gerakan ke kiri dan kanan dibuat dari pose asli dan pose samping. Sesuaikan Kekuatan koreksi lantai bila perlu.
4. Di VRChat pilih pose yang didukung dan aktifkan Berguling. Nonaktifkan kunci kepala sebelum menggunakan.

Dalam pose dengan Berguling, kemiringan kepala menentukan posisi pemutaran animasi sehingga MotionTime biasa tidak tersedia. Jika menggunakan animasi berguling siap pakai, tetapkan pada bidang animasi biasa dan kosongkan Pose berguling samping. Siapkan animasi dengan posisi 0 di kiri, 0.5 telentang, dan 1 di kanan.

### Napas dan goyangan bawaan

Di “Fitur > Kemudahan > Animasikan napas”, dada dan bagian tubuh lain bergerak mengikuti napas. Buka “Buka pengaturan” untuk memilih “Tidak ada, Lemah, Sedang, Kuat, Cepat, Khusus” bagi setiap kategori pose. “Cepat” memberikan napas yang lebih cepat. Dengan “Khusus”, Anda dapat menentukan animasi napas sendiri.

Animasi napas saja menggunakan 0 bit. Untuk menghentikan dan melanjutkan napas di VRChat, aktifkan juga “Fitur menu > Napas”. Sinkronisasi ON/OFF menggunakan 1 bit. Mengubah kekuatannya tidak menambah jumlah bit.

Leher, bahu, lengan, dan bagian lain ikut bergerak bersama dada untuk mengurangi perpindahan kepala dan tangan. Posisi tidak dikunci sepenuhnya, jadi pilih kekuatan yang sesuai dengan bentuk tubuh dan pose.

“Kemudahan > Nonaktifkan goyangan diam VRChat” mengurangi goyangan tubuh bawaan VRChat saat diam. Secara bawaan fitur ini nonaktif dan tetap mempertahankan animasi napas alat pose serta gerakan khusus lainnya. Fitur ini juga dapat dipilih tanpa animasi napas dan tidak menambah penggunaan bit. Goyangan yang disebabkan hal lain selain gerakan diam bawaan tidak dihentikan oleh pengaturan ini.

Jika menggunakan beberapa alat pose sekaligus, menonaktifkan goyangan bawaan pada salah satunya berlaku untuk seluruh avatar. Untuk mengembalikannya, nonaktifkan pengaturan ini pada semua alat lalu unggah kembali avatar.

### Kurangi perubahan pose akibat rotasi kepala

Gunakan ini jika pose berubah saat kunci leher dilepas karena arah leher berbeda dari pose asli. Pengaturan ini berada di “Fitur > Kemudahan” dan tidak menambahkan menu khusus dalam permainan. Saat leher dikunci, arah pose asli digunakan. Pose tidur telentang dikecualikan dari koreksi agar tetap menghadap ke atas seperti pose asli.

### Gambar ikon menu

- “Buat ikon pose”: membuat gambar avatar dalam setiap pose untuk ditampilkan di menu. Pose lebih mudah dikenali, tetapi pembuatan gambar menambah waktu build.
- “Kurangi ukuran ikon”: menurunkan resolusi gambar untuk mengurangi ukuran avatar.

Ikon pose tidak dibuat untuk build Android. Setelah mengganti pakaian atau bagian lain, gunakan “Ambil ulang ikon”. Gambar untuk daftar pose diperbarui dengan “Perbarui gambar mini pose”. Proses ini memerlukan waktu, jadi baca dialog konfirmasi sebelum menjalankannya.

## Menambah, mengurutkan, dan melihat pratinjau pose

Pilih kategori pose di sisi kiri “Pose”. Tab kaca pembesar di bagian paling atas memungkinkan pencarian nama pose di semua kategori. Buka “Rincian” untuk mengatur animasi, ikon, MotionTime, dan lainnya sekaligus.

Untuk menambah beberapa animasi sekaligus, seret dan lepaskan ke area penambahan, atau pilih di Project lalu tekan “Muat klip yang dipilih di Project”. Klip yang sama tidak ditambahkan dua kali.

Untuk mengubah urutan, pilih posisi tujuan pada “Urutan” di “Rincian”. Anda juga dapat menggunakan tombol pindah ke atas atau ke bawah. Status terbuka atau tertutupnya rincian tetap dipertahankan setelah pemindahan.

Tekan “Pratinjau” pada setiap pose untuk memeriksanya di Scene tanpa membuka jendela penyesuaian. Saat pratinjau diakhiri, tampilan semula dipulihkan. Pratinjau sederhana ini tidak menjalankan proses build MA atau NDMF, sehingga hasilnya dapat berbeda dari pakaian atau FloorAdjuster yang diterapkan saat build. Periksa juga hasil akhir pakaian dan kontak lantai setelah build avatar.

## Penyesuaian Animasi dan penyesuaian anggota tubuh otomatis (IK)

Buka “Penyesuaian Animasi” di “Pose” untuk menyempurnakan tubuh dan anggota tubuh sambil melihat pratinjau. Setelah selesai, simpan animasi penyesuaian melalui perintah simpan di jendela tersebut.

Untuk produk dan pose yang mendukung penyesuaian anggota tubuh otomatis, posisi anggota tubuh disesuaikan dengan proporsi avatar. Gunakan “Aktifkan penyesuaian anggota tubuh otomatis” pada layar penyesuaian animasi untuk mengaktifkannya secara keseluruhan, dan “Aktifkan IK pose ini” untuk masing-masing pose. Pose yang tidak memiliki penyesuaian otomatis menampilkan keterangan tersebut.

Untuk menyelesaikan penyesuaian secara manual berdasarkan hasil otomatis, tekan “Rekam hasil IK ke penyesuaian”. Hasil IK saat itu ditulis ke animasi penyesuaian baru, dan IK untuk pose tersebut pada avatar itu dinonaktifkan. Animasi asli tetap dipertahankan. Tombol ini tersedia jika penyesuaian otomatis keseluruhan dan pose tersebut aktif serta terdapat pengaturan IK yang dapat diterapkan.

Di “Pengaturan Avatar”, gunakan “Cegah lutut menekuk terbalik” untuk mengaktifkan koreksi lutut. Atur sudut tekukan pada “Tekukan lutut minimum (derajat)” di rincian. “Aktifkan koreksi avatar” tersedia jika ada data koreksi yang sesuai. Koreksi yang tumpang tindih dengan penyesuaian anggota tubuh otomatis mungkin tidak dapat digunakan bersamaan.

## Pose asli, gerakan sendiri, dan prasetel

Pengaturan ini berada di “Pengaturan Avatar”.

- “Gunakan pose berdiri, jongkok, dan tengkurap asli”: mengimpor pose yang sudah diatur pada avatar secara otomatis.
- “Gunakan gerakan sendiri”: buka pengaturan impor untuk menentukan gerakan secara terpisah bagi setiap jenis, seperti berdiri, jongkok (Crouching), dan tengkurap (Prone). Pada umumnya, satu jenis menggunakan satu gerakan.
- “Integrasikan kunci pelacakan dengan alat lain”: aktifkan jika fungsi penguncian tidak bekerja dengan baik saat digunakan bersama alat lain.

Pengaturan impor untuk menentukan gerakan sendiri merupakan fitur terpisah dari pengaturan impor otomatis pose asli.

Prasetel berada di bagian paling bawah tab. Cari berdasarkan nama dan gunakan “Terapkan prasetel ini” untuk mengganti pengaturan pose. Fitur ini terutama digunakan untuk pengaturan awal saat pemasangan dan mengganti pengaturan saat ini. Simpan penyesuaian yang sudah dibuat sebelum beralih.

Saat menampilkan daftar prasetel, hanya nama dan informasi avatar yang didukung yang dimuat. Animasi penyesuaian dimuat saat prasetel terpilih diterapkan. Bahkan saat daftar dibuat pertama kali, animasi untuk semua avatar tidak dimuat lebih dahulu.

Daftar dibagi menjadi “Avatar kompatibel”, “Prasetel pengguna”, dan “Prasetel resmi Unisaki Studio”. “Sembunyikan prasetel bawaan” merupakan pengaturan tampilan bersama dalam proyek yang sama. Daftar diperbarui saat prasetel ditambah, diedit, atau dihapus. Jika tampilannya tidak berubah, tekan “Muat ulang”.

## Prabuild dan peringatan

Jika “Gunakan pose berdiri, jongkok, dan tengkurap asli” ON, gerakan dideteksi dari Base avatar setiap kali data dihasilkan. Hasil deteksi tidak ditulis kembali ke pengaturan impor manual, sehingga daftar tetap tidak ditentukan jika hanya impor otomatis yang digunakan. Perubahan gerakan Base dan status ON/OFF impor otomatis diterapkan pada pembuatan berikutnya.

Impor otomatis saat unggah diterapkan pada salinan untuk build. Objek asli dalam Scene, pengaturan impor, serta Controller dan gerakan sumber tidak diubah. Jika sudah ada prabuild dan input tidak berubah, hasilnya digunakan kembali.

Untuk jenis dengan pilihan manual, pengaturan manual diprioritaskan, termasuk rotasi, animasi penyesuaian, dan posisi pemutaran. Pengaturan dengan “Gunakan” OFF atau gerakan yang sebelumnya ditentukan telah dilepas akan menghentikan impor otomatis untuk jenis tersebut. Gunakan “Hapus pilihan manual” untuk menghapus pengaturan dan penyesuaian jenis tersebut; jika impor otomatis ON, deteksi diulang pada pembuatan berikutnya. Tindakan ini dapat dibatalkan dengan Undo. Jika terdapat beberapa pengaturan lama untuk jenis yang sama, pengaturan aktif yang dipilih di Inspector diprioritaskan.

Pengaturan yang ditambahkan ke daftar oleh impor otomatis versi lama tidak dihapus secara otomatis karena tidak dapat dibedakan dari pengaturan yang didaftarkan atau disesuaikan secara manual. Gunakan “Hapus pilihan manual” hanya pada jenis yang ingin dikembalikan ke impor otomatis. Jika beberapa produk dipasang pada avatar yang sama, pengaturan manual dari produk lain juga memengaruhi pose asli bersama.

Prabuild menggunakan kembali data yang sudah dihasilkan untuk mempercepat build avatar. Anda dapat membuat atau memperbaruinya dengan “Perbarui prabuild”. Data yang diperlukan juga dihasilkan saat build avatar biasa, jadi Anda tidak perlu membuat prabuild secara manual setiap kali.

Saat pengaturan berubah dan pembaruan diperlukan, “Periksa pengaturan” di bagian atas Inspector dan tanda di Hierarchy memberi tahu Anda. Setelah mengubah pose atau pengaturan IK, memperbarui prabuild dapat mengurangi waktu build berikutnya.

“Tampilan peringatan” berada dalam panel “Periksa pengaturan”. Anda dapat menyembunyikan setiap jenis peringatan atau tanda di Hierarchy. Menyembunyikannya tidak menyelesaikan masalah pengaturan atau memperbarui data yang dihasilkan. Kesalahan dan proses pembuatan yang diperlukan tetap berlaku.

## Catatan penggunaan

- Ditujukan untuk pelacakan 3 titik dan penggunaan desktop. Penguncian pose selama pelacakan seluruh tubuh tidak didukung.
- Bergantung pada proporsi tubuh dan modifikasi, kontak tangan dan kaki dengan lantai serta tampilan pakaian mungkin memerlukan penyesuaian tambahan.
- Kombinasi dengan layer Base khusus atau alat pengubah pose lain mungkin tidak bekerja sesuai harapan.
- Perpindahan dan rotasi Mode foto serta Tinggi lantai mengoreksi pose alat dan gerakan bawaan maupun khusus saat tidak ada pose yang dipilih. Periksa gerakan bawaan yang diganti dalam VRChat serta perilaku sudut pandang pada perangkat sebenarnya.

## Ketentuan penggunaan dan dukungan

Kode sumber dan sumber daya repositori ini bukan sumber terbuka. Penggunaannya memerlukan pembelian produk secara sah dan persetujuan atas [ketentuan penggunaan yang disertakan](../../../%E3%82%86%E3%81%AB%E3%81%95%E3%81%8D%E3%83%9D%E3%83%BC%E3%82%BA%E3%82%B7%E3%82%B9%E3%83%86%E3%83%A0%E5%88%A9%E7%94%A8%E8%A6%8F%E7%B4%84%EF%BC%88%E6%97%A5%E6%9C%AC%E8%AA%9E%E7%89%88%EF%BC%89.pdf).

- [Panduan Kawaii Posing Tool](https://docs.unisakistudio.jp/kawaiiposing/manual/)
- [Panduan Cool Posing Tool](https://docs.unisakistudio.jp/coolposing/manual/)
- [Hubungi kami](https://help.unisakistudio.jp/)

Saat melaporkan masalah, sertakan versi produk dan Unisaki Posing System, versi Unity, VRChat SDK, Modular Avatar, dan NDMF, avatar yang digunakan, langkah untuk mengulangi masalah, serta rincian kesalahan Console.
