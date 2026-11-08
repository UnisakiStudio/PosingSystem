# Unisaki Posing System

Hệ thống tư thế dùng chung cho Kawaii Posing Tool, Cool Posing Tool và các công cụ liên quan. Hệ thống sử dụng Modular Avatar và NDMF để tạo menu cùng hoạt ảnh cần thiết khi tải avatar lên.

README này dành cho v3.2.0 đang được phát triển. Chưa xác định ngày phát hành chính thức. Kiểm tra phiên bản đã cài ở đầu Inspector của công cụ tư thế. Xem thêm [tính năng mới và lịch sử thay đổi](CHANGELOG.md).

## Ngôn ngữ hiển thị và tài liệu ngoại tuyến

Chọn ngôn ngữ hiển thị bằng “Language / 言語” ở đầu Inspector. Một lựa chọn áp dụng cho cả Inspector và menu VRChat được tạo. Tải avatar lên lại để thay đổi ngôn ngữ trong trò chơi.

Nếu chưa chọn, hệ thống dùng ngôn ngữ máy tính; ngôn ngữ không được hỗ trợ sẽ dùng tiếng Anh. Hỗ trợ Windows, macOS và Linux. Ngôn ngữ chọn gần nhất được lưu trong EditorPrefs và dùng cho các dự án khác của cùng người dùng hệ điều hành trên máy này. Lựa chọn rõ ràng đã lưu trên avatar được giữ nguyên. Không đồng bộ sang máy khác. Cách lưu giấy phép không thay đổi.

Bạn có thể chọn README, lịch sử thay đổi và hướng dẫn từ [danh sách ngôn ngữ](../index.html) đi kèm. “Hướng dẫn” và “Lịch sử thay đổi” trong Inspector cũng theo ngôn ngữ đã chọn. Điều khoản gốc và bản dịch tham khảo vẫn được cung cấp riêng như trước.

## Chuyển tư thế trong chế độ máy tính

Giảm độ trễ trước khi tư thế đã chọn được áp dụng khi chuyển giữa đứng, khom người và ngồi trong chế độ máy tính. Có hiệu lực trong bản dựng thường và dữ liệu tạo trước, không cần lệnh riêng hay thiết lập bổ sung. Chế độ VR và trạng thái đặc biệt giữ cách xử lý cũ. Hãy cập nhật dữ liệu tạo trước phiên bản cũ rồi mới tải lên.

## Cài đặt và nâng cấp

Làm theo tệp mua hàng và hướng dẫn cài đặt của từng sản phẩm để thêm công cụ tư thế qua VCC hoặc ALCOM. Hệ thống này được cài như gói phụ thuộc của công cụ tư thế.

Yêu cầu cho v3.2.0:

- Dự án avatar VRChat dùng Unity 2022.3
- VRChat Avatars SDK 3.10.4 trở lên
- Modular Avatar dòng 1.x, phiên bản 1.15.1 trở lên
- NDMF (Non-Destructive Modular Framework) dòng 1.x, phiên bản 1.14.0 trở lên

Nếu dùng Kawaii Posing Tool, cập nhật sản phẩm lên 3.2.0 bằng ALCOM / VCC và kiểm tra Unisaki Posing System cũng là 3.2.0. Nếu dùng dữ liệu tạo trước, nhấn “Cập nhật dữ liệu tạo trước”, lưu Scene rồi tải avatar lên.

Cool Posing Tool 1.2.0 dùng hệ thống chung 3.2.0. Cập nhật sản phẩm và hệ thống chung lên các phiên bản tương ứng.

### Dành cho người từng dùng bản 8bit

Việc thêm mới được thống nhất vào Prefab thường. Tắt các mục không cần trong “Tính năng” để giảm mức sử dụng. Nếu chỉ chọn tư thế, mức sử dụng cơ bản là 8 bit.

Prefab 8bit cũ vẫn nằm trong “Editor/Legacy” của từng sản phẩm để giữ tham chiếu của avatar đã đặt và thiết lập sẵn. Không cần xóa hoặc thay Prefab để tiếp tục dùng bản 8bit hiện có. Quy trình chọn bản 8bit từ menu thêm mới của Unity đã được bỏ.

## Đọc màn hình thiết lập

Chọn “可愛いポーズ” (Kawaii Posing), “カッコいいポーズ” (Cool Posing) hoặc đối tượng tương ứng bên trong avatar trong Hierarchy để thiết lập.

| Thẻ | Nội dung thiết lập |
| --- | --- |
| Tính năng | Mục thêm vào menu trò chơi, tiện ích, ảnh biểu tượng và mức dùng ExpressionParameters |
| Tư thế | Chọn, thêm, đổi thứ tự tư thế, xem trước trong Scene, chi tiết từng tư thế và hiệu chỉnh hoạt ảnh |
| Thiết lập avatar | Hiệu chỉnh theo vóc dáng, đầu gối, nhập tư thế gốc hoặc chuyển động riêng và thiết lập sẵn |

Với mục có thiết lập chi tiết, nhấn “Mở thiết lập” để mở phần thiết lập bên dưới. Thiết lập thở cũng nằm trong “Tính năng”.

Mở hướng dẫn, liên hệ hỗ trợ và lịch sử thay đổi từ “Trợ giúp / lịch sử thay đổi” ở trên cùng. Bạn có thể chuyển giữa lịch sử của sản phẩm và Unisaki Posing System.

## Tính năng và mức dùng ExpressionParameters

Các mục chọn trong “Tính năng > Tính năng menu” xuất hiện trong menu trò chơi. Ví dụ, chọn “Lật đối xứng” cho phép bật/tắt lật trong trò chơi. Chỉ chọn trong Inspector không khiến mọi tư thế luôn bị lật.

| Tính năng | Số bit dùng thêm |
| --- | ---: |
| Lật đối xứng | 1 bit |
| Tắt cử chỉ | 1 bit |
| Thêm hoạt ảnh thở | 0 bit |
| Thở (ON/OFF trong trò chơi) | 1 bit |
| Độ cao chân | 8 bit |
| Độ cao sàn | 8 bit |
| Chế độ chụp (chỉ màn hình của bạn) | 0 bit |
| Đồng bộ chế độ chụp | 16 bit |
| Giữ đầu hiển thị trong chế độ chụp | 0 bit |
| Tắt đung đưa khi đứng yên của VRChat | 0 bit |
| Giảm méo tư thế do xoay đầu | 1 bit (dùng chung với Lật người) |
| Lật người | 10 bit (trong đó 1 bit dùng chung với giảm méo tư thế) |

Lật người dùng 1 bit cho ON/OFF, 8 bit cho độ nghiêng đầu và 1 bit cho khóa cổ. Nếu đã bật giảm méo tư thế, thêm Lật người chỉ tăng 9 bit. Phần dùng chung không bị tính hai lần, và số hiển thị cập nhật khi đổi ô chọn.

Tổng hiển thị chỉ là phần của các công cụ tư thế đã cài trên avatar đó, không phải tổng toàn avatar gồm trang phục và công cụ khác. Thiết lập chung cũng không bị tính hai lần khi dùng nhiều công cụ tư thế. Cấu hình có số tư thế vượt phạm vi đánh số cơ bản sẽ cần thêm bit để chọn tư thế.

### Độ cao chân và Độ cao sàn

- “Độ cao chân”: điều chỉnh độ cao phía chân trong khi giữ vị trí đầu. Dùng để đặt chân khớp với sàn hoặc giường.
- “Độ cao sàn”: di chuyển toàn thân, gồm cả đầu, lên xuống. 50% là độ cao gốc, 0% thấp hơn 3m và 100% cao hơn 3m. Giá trị ban đầu là 50%.

Cả hai nằm trong “Thiết lập” trong trò chơi, với “Độ cao sàn” ngay sau “Độ cao chân”. Di chuyển Độ cao sàn khỏi giữa để lên xuống; đưa về gần 50% để trở lại độ cao gốc và kết thúc hiệu chỉnh. Không có công tắc bật/tắt nổi riêng. Góc nhìn được hiệu chỉnh khi bắt đầu nổi và trong lúc điều khiển Độ cao sàn.

Độ cao chân đã đổi cách tạo dữ liệu để không lặp lại chuyển động toàn thân của hoạt ảnh thường chỉ để chỉnh độ cao. Điều này giảm dữ liệu hoạt ảnh được tạo, nhưng không có nghĩa mọi avatar đều giảm dung lượng còn 1/3.

### Chế độ chụp

1. Bật “Tính năng > Tính năng menu > Chế độ chụp”.
2. Trong VRChat, mở “Chế độ chụp” và chọn tư thế bằng “Chọn tư thế”.
3. Dùng “Khoảng cách” để tiến tối đa 3m và “Hướng” để xoay. Khi khoảng cách lớn hơn 0, avatar sẽ quay về phía bạn.
4. Bật “Khóa chân” khi cần. Khóa chân không bị ép bật lúc bắt đầu, nên có thể chỉnh vị trí trước khi khóa. Nếu đã thêm “Tắt cử chỉ” vào menu, bạn cũng có thể đổi hình bàn tay trên cùng màn hình.
5. “Thoát chụp” đặt lại khoảng cách và hướng, đồng thời mở khóa chân.

Di chuyển và xoay trong Chế độ chụp dùng 0 bit nếu chỉ hiển thị trên màn hình của bạn. Để người khác cũng thấy vị trí và hướng, bật “Tiện ích > Đồng bộ chế độ chụp”. Khoảng cách và hướng dùng tổng 16 bit.

Nếu đầu avatar của bạn biến mất khi ở gần, bật “Tiện ích > Giữ đầu hiển thị trong chế độ chụp”. Chỉ áp dụng trên màn hình của bạn khi chụp và trở về hiển thị thường sau khi kết thúc.

### Nhảy để đổi tư thế nằm

Khi chọn cả nằm ngửa và nằm sấp, bạn có thể nhảy tại chỗ để chuyển đổi. Nếu chỉ chọn một phía, tư thế đó được giữ. Nếu không muốn đổi bằng nhảy, tắt “Nhảy để đổi tư thế nằm” trong “Tính năng → Tiện ích” của Unity. Không dùng thêm tham số đồng bộ: 0 bit. Với avatar dùng nhiều công cụ tư thế, tắt ở một công cụ sẽ áp dụng cho toàn avatar.

### Lật người

Ở tư thế nằm được hỗ trợ, nghiêng đầu sang trái/phải sẽ khiến cơ thể lật người mượt mà. Tính năng không tự động hỗ trợ mọi tư thế nằm.

1. Bật “Tính năng > Tính năng menu > Lật người”.
2. Trong “Tư thế”, mở “Chi tiết” của tư thế nằm muốn dùng và bật “Bật lật người”.
3. Đặt hoạt ảnh nằm nghiêng trong “Tư thế lật người sang bên”. Công cụ tạo chuyển động lật trái/phải từ tư thế gốc và nằm nghiêng. Điều chỉnh “Cường độ hiệu chỉnh sàn” khi cần.
4. Trong VRChat, chọn tư thế được hỗ trợ rồi bật “Lật người”. Hãy mở khóa cổ khi dùng.

Với tư thế dùng Lật người, độ nghiêng đầu quyết định vị trí phát hoạt ảnh nên thiết lập MotionTime thông thường không dùng được. Nếu dùng hoạt ảnh lật người hoàn chỉnh, đặt vào ô hoạt ảnh thường và để trống “Tư thế lật người sang bên”. Chuẩn bị hoạt ảnh có vị trí phát 0 là bên trái, 0.5 nằm ngửa và 1 bên phải.

### Thở và đung đưa mặc định

Trong “Tính năng > Tiện ích > Thêm hoạt ảnh thở”, ngực và các bộ phận khác chuyển động theo nhịp thở. Qua “Mở thiết lập”, chọn “Không, Nhẹ, Vừa, Mạnh, Nhanh, Tùy chỉnh” cho từng loại tư thế. “Nhanh” là nhịp thở nhanh. “Tùy chỉnh” cho phép chỉ định hoạt ảnh thở riêng.

Chỉ hoạt ảnh thở dùng 0 bit. Để dừng và tiếp tục thở trong VRChat, bật cả “Tính năng menu > Thở”. Đồng bộ ON/OFF dùng 1 bit. Thay đổi cường độ không tăng số bit.

Cổ, vai, tay và các phần khác cũng chuyển động theo ngực để hạn chế đầu và bàn tay dịch chuyển. Đây không phải khóa vị trí hoàn toàn, vì vậy hãy chọn cường độ theo vóc dáng và tư thế.

“Tiện ích > Tắt đung đưa khi đứng yên của VRChat” giảm chuyển động đung đưa mặc định của VRChat khi đứng yên. Mặc định tắt và vẫn giữ hoạt ảnh thở của công cụ tư thế cùng chuyển động tùy chỉnh khác. Có thể dùng mà không bật hoạt ảnh thở và không tăng số bit. Thiết lập này không dừng đung đưa do nguyên nhân khác ngoài chuyển động chờ mặc định.

Nếu dùng nhiều công cụ tư thế, việc tắt đung đưa mặc định ở một công cụ áp dụng cho toàn avatar. Muốn khôi phục, tắt thiết lập này ở tất cả công cụ rồi tải avatar lên lại.

### Giảm méo tư thế do xoay đầu

Dùng khi tư thế bị méo lúc mở khóa cổ vì hướng cổ khác tư thế gốc. Thiết lập nằm trong “Tính năng > Tiện ích” và không thêm menu riêng trong trò chơi. Khi khóa cổ, hướng của tư thế gốc được dùng. Tư thế nằm ngửa được loại khỏi hiệu chỉnh để giữ hướng nhìn lên gốc.

### Ảnh biểu tượng menu

- “Tạo biểu tượng tư thế”: tạo ảnh avatar trong từng tư thế để hiển thị ở menu. Giúp nhận biết tư thế nhưng tăng thời gian dựng do phải tạo ảnh.
- “Giảm kích thước biểu tượng”: giảm độ phân giải ảnh để giảm dung lượng avatar.

Bản dựng Android không tạo biểu tượng tư thế. Sau khi đổi trang phục hoặc bộ phận khác, dùng “Chụp lại biểu tượng”. Ảnh cho danh sách tư thế được cập nhật bằng “Cập nhật ảnh thu nhỏ tư thế”. Đây là thao tác mất thời gian, nên đọc hộp thoại xác nhận trước khi chạy.

## Thêm, sắp xếp và xem trước tư thế

Chọn loại tư thế ở bên trái “Tư thế”. Thẻ kính lúp trên cùng cho phép tìm tên trên mọi loại. Mở “Chi tiết” để thiết lập hoạt ảnh, biểu tượng, MotionTime và các mục khác cùng lúc.

Thêm nhiều hoạt ảnh cùng lúc bằng cách kéo thả vào vùng thêm, hoặc chọn trong Project rồi nhấn “Nhập clip đang chọn trong Project”. Clip trùng nhau được loại bỏ.

Để đổi thứ tự, chọn vị trí đích bằng “Thứ tự” trong “Chi tiết”. Cũng có thể dùng nút lên/xuống. Trạng thái mở/đóng chi tiết được giữ sau khi di chuyển.

Nhấn “Xem trước” ở từng tư thế để kiểm tra trong Scene mà không mở cửa sổ hiệu chỉnh. Khi kết thúc xem trước, hiển thị gốc được khôi phục. Xem trước đơn giản không chạy quy trình dựng MA hay NDMF, nên có thể khác kết quả cuối cùng của trang phục hoặc FloorAdjuster được thêm lúc dựng. Hãy kiểm tra trang phục và tiếp đất sau khi dựng avatar.

## Hiệu chỉnh hoạt ảnh và hiệu chỉnh chi tự động (IK)

Mở “Hiệu chỉnh hoạt ảnh” trong “Tư thế” để chỉnh chi tiết thân và các chi trong khi xem trước. Khi xong, lưu hoạt ảnh hiệu chỉnh bằng thao tác lưu trong cửa sổ đó.

Với sản phẩm và tư thế hỗ trợ hiệu chỉnh chi tự động, vị trí chi được điều chỉnh theo vóc dáng avatar. Dùng “Bật hiệu chỉnh chi tự động” trong màn hình hiệu chỉnh hoạt ảnh để bật/tắt toàn bộ, và “Bật IK tư thế này” cho từng tư thế. Tư thế không có hiệu chỉnh tự động sẽ hiển thị thông báo tương ứng.

Để hoàn thiện thủ công dựa trên kết quả tự động, nhấn “Ghi IK vào hiệu chỉnh”. Kết quả IK lúc đó được ghi vào hoạt ảnh hiệu chỉnh mới và IK của tư thế đó trên avatar này được tắt. Hoạt ảnh gốc được giữ. Nút dùng được khi hiệu chỉnh tự động toàn bộ và tư thế đó đều bật, đồng thời có thiết lập IK áp dụng được.

Trong “Thiết lập avatar”, dùng “Ngăn đầu gối gập ngược” để bật/tắt hiệu chỉnh gối. Đặt góc gập trong “Độ gập đầu gối tối thiểu (độ)” ở phần chi tiết. “Bật hiệu chỉnh avatar” dùng được khi có dữ liệu hiệu chỉnh phù hợp. Một số hiệu chỉnh chồng lấn với hiệu chỉnh chi tự động có thể không dùng chung được.

## Tư thế gốc, chuyển động riêng và thiết lập sẵn

Các mục này nằm trong “Thiết lập avatar”.

- “Dùng tư thế đứng, ngồi xổm và nằm sấp gốc”: tự động nhập tư thế vốn có trên avatar.
- “Dùng chuyển động riêng”: mở thiết lập nhập và chỉ định chuyển động riêng cho từng loại như đứng, ngồi xổm (Crouching) và nằm sấp (Prone). Thông thường mỗi loại dùng một chuyển động.
- “Tích hợp khóa theo dõi với công cụ khác”: bật nếu tính năng khóa không hoạt động tốt khi dùng với công cụ khác.

Thiết lập nhập để tự chọn chuyển động là tính năng tách biệt với tự động nhập tư thế gốc.

Thiết lập sẵn nằm ở cuối thẻ. Tìm theo tên rồi dùng “Áp dụng thiết lập sẵn này” để đổi thiết lập tư thế. Chủ yếu dùng để thiết lập ban đầu khi cài và thay thế cấu hình hiện tại, nên hãy lưu nội dung đã hiệu chỉnh trước khi chuyển.

Khi hiển thị danh sách thiết lập sẵn, chỉ tên và thông tin avatar tương thích được nạp. Hoạt ảnh hiệu chỉnh được nạp khi áp dụng thiết lập sẵn đã chọn. Ngay lần tạo danh sách đầu tiên cũng không nạp trước hoạt ảnh của mọi avatar.

Danh sách chia thành “Avatar tương thích”, “Thiết lập sẵn của người dùng” và “Thiết lập sẵn chính thức Unisaki Studio”. “Ẩn thiết lập sẵn đi kèm” là tùy chọn hiển thị chung trong cùng dự án. Danh sách cập nhật khi thêm, sửa hoặc xóa thiết lập sẵn. Nếu hiển thị chưa thay đổi, nhấn “Tải lại”.

## Tạo trước và cảnh báo

Khi “Dùng tư thế đứng, ngồi xổm và nằm sấp gốc” ON, chuyển động được phát hiện từ Base của avatar mỗi lần tạo dữ liệu. Kết quả không được ghi lại vào thiết lập nhập thủ công, nên danh sách vẫn chưa chỉ định nếu chỉ dùng nhập tự động. Thay đổi chuyển động Base và ON/OFF nhập tự động có hiệu lực ở lần tạo sau.

Nhập tự động lúc tải lên áp dụng cho bản sao dùng để dựng. Không sửa đối tượng gốc trong Scene, thiết lập nhập, Controller hay chuyển động nguồn. Nếu đã tạo trước và đầu vào không đổi, kết quả được tái sử dụng.

Với loại có lựa chọn thủ công, thiết lập thủ công được ưu tiên, gồm xoay, hoạt ảnh hiệu chỉnh và vị trí phát. Thiết lập có “Dùng” OFF hoặc chuyển động đã chỉ định bị gỡ sẽ ngừng nhập tự động cho loại đó. “Xóa lựa chọn thủ công” gỡ thiết lập và hiệu chỉnh của loại ấy; nếu nhập tự động ON, sẽ phát hiện lại ở lần tạo sau. Có thể hoàn tác bằng Undo. Nếu một loại có nhiều thiết lập cũ, thiết lập hợp lệ đang chọn trong Inspector được ưu tiên.

Thiết lập do nhập tự động phiên bản cũ thêm vào danh sách không bị tự xóa vì không thể phân biệt với mục đã đăng ký/hiệu chỉnh thủ công. Chỉ dùng “Xóa lựa chọn thủ công” cho loại muốn trở lại nhập tự động. Khi nhiều sản phẩm cài trên cùng avatar, thiết lập thủ công từ sản phẩm khác cũng áp dụng cho tư thế gốc dùng chung.

Tạo trước tái sử dụng dữ liệu đã sinh để giảm thời gian dựng avatar. Có thể tạo hoặc cập nhật bằng “Cập nhật dữ liệu tạo trước”. Bản dựng avatar thông thường cũng tạo dữ liệu cần thiết, nên không phải tạo trước thủ công mỗi lần.

Khi thay đổi thiết lập khiến cần cập nhật, “Kiểm tra thiết lập” ở đầu Inspector và dấu trong Hierarchy sẽ thông báo. Cập nhật dữ liệu tạo trước sau khi đổi tư thế hoặc IK giúp giảm thời gian dựng các lần tiếp theo.

“Thiết lập hiển thị cảnh báo” nằm trong khung “Kiểm tra thiết lập”. Có thể ẩn từng loại cảnh báo hoặc dấu Hierarchy. Ẩn không giải quyết vấn đề thiết lập hay cập nhật dữ liệu được tạo. Lỗi và xử lý tạo dữ liệu cần thiết vẫn giữ nguyên.

## Lưu ý sử dụng

- Dành cho theo dõi 3 điểm và chế độ máy tính. Không hỗ trợ khóa tư thế khi đang theo dõi toàn thân.
- Tùy vóc dáng và nội dung chỉnh sửa, tiếp đất của tay/chân hoặc hiển thị trang phục có thể cần hiệu chỉnh thêm.
- Kết hợp với layer Base đặc biệt hoặc công cụ đổi tư thế khác có thể không cho kết quả mong muốn.
- Di chuyển/xoay của Chế độ chụp và Độ cao sàn hiệu chỉnh tư thế công cụ cùng chuyển động mặc định/tùy chỉnh khi chưa chọn tư thế. Hãy kiểm tra chuyển động mặc định bị thay trong VRChat và hành vi góc nhìn trên thiết bị thực.

## Điều khoản sử dụng và hỗ trợ

Mã nguồn và tài nguyên của kho này không phải mã nguồn mở. Việc sử dụng yêu cầu mua sản phẩm hợp lệ và đồng ý với [điều khoản sử dụng đi kèm](../../../%E3%82%86%E3%81%AB%E3%81%95%E3%81%8D%E3%83%9D%E3%83%BC%E3%82%BA%E3%82%B7%E3%82%B9%E3%83%86%E3%83%A0%E5%88%A9%E7%94%A8%E8%A6%8F%E7%B4%84%EF%BC%88%E6%97%A5%E6%9C%AC%E8%AA%9E%E7%89%88%EF%BC%89.pdf).

- [Hướng dẫn Kawaii Posing Tool](https://docs.unisakistudio.jp/kawaiiposing/manual/)
- [Hướng dẫn Cool Posing Tool](https://docs.unisakistudio.jp/coolposing/manual/)
- [Liên hệ](https://help.unisakistudio.jp/)

Khi báo lỗi, hãy cung cấp phiên bản sản phẩm và Unisaki Posing System, phiên bản Unity, VRChat SDK, Modular Avatar và NDMF, avatar đang dùng, các bước tái hiện và nội dung lỗi Console.
