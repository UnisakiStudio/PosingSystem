# Lịch sử thay đổi

## [3.2.0] - Chưa phát hành

### Đã thêm

- Inspector, thao tác chỉnh sửa, hộp thoại cài đặt, tên tư thế chuẩn và menu VRChat được tạo nay hiển thị bằng 30 ngôn ngữ.
- Thống nhất lựa chọn ngôn ngữ cho Inspector và menu trò chơi. Ban đầu dùng ngôn ngữ máy tính; lựa chọn gần nhất cũng dùng trong các dự án khác của cùng người dùng hệ điều hành.
- Có thể mở README, lịch sử thay đổi và hướng dẫn ngoại tuyến bằng ngôn ngữ đã chọn.
- Cung cấp tên hiển thị của 128 tư thế chuẩn và mô tả từng tính năng theo thuật ngữ hiện có.

### Tương thích

- Giữ thiết lập avatar hiện có, tên tùy chỉnh, tham chiếu tài nguyên, mã định danh tham số và phương thức lưu giấy phép.

## [3.1.2] - 2026-11-09

- Sửa lỗi khiến mặt vẫn hướng lên sau khi đứng dậy từ nằm sấp khi bật “Giảm méo tư thế do xoay đầu”.
- Có thể bật/tắt khóa đầu qua “Thiết lập → Tự khóa đầu khi nằm ngửa” trong menu EX ngay cả khi “Độ cao chân” tắt. Mục bị mất trên avatar ở 3.1.0 hoặc 3.1.1 cũng trở lại sau cập nhật và dựng.
- “Cập nhật dữ liệu tạo trước” hoạt động ngay cả khi bỏ chọn đối tượng avatar chính trong Unity.
- Sửa lỗi có thể ngăn tạo tư thế sau cập nhật nếu Controller tư thế chung phiên bản cũ còn trong Assets. Thiết lập Controller do bạn tự sửa cũng được giữ.
- Khi công cụ hoặc đối tượng cha tắt khiến không dùng được Animator tư thế chung, tên đối tượng liên quan được hiển thị. Có thể mở vị trí đó từ “Kiểm tra thiết lập → Chọn đối tượng”.
- Khi dữ liệu tạo trước báo có thay đổi thiết lập, có thể xem khác biệt so với lần trước bằng “Chi tiết → Xem thiết lập thay đổi từ lần tạo trước”.
- Khi lỗi mã lệnh chặn dựng (PS050), hiển thị tối đa 5 lỗi gốc Unity phát hiện. Qua “Hỏi về lỗi này”, bạn có thể xem, sửa rồi gửi bản nháp có phiên bản Unity, công cụ được hỗ trợ và mã lỗi.

Nếu từng dùng tạo trước, sau cập nhật hãy nhấn “Cập nhật dữ liệu tạo trước”, lưu Scene rồi dựng và tải avatar lên lại.

## [3.1.1] - 2026-11-08

- “Độ cao sàn” và “Lật người” đã bật nay xuất hiện trong “Thiết lập” của menu EX VRChat ngay cả khi “Lật đối xứng” tắt.
- Dù đã đổi thứ tự, xóa hoặc đổi tên tư thế ở bản cũ, chuyển động lật người đúng vẫn được kế thừa cho “Duỗi chân”, “Thư thái” và “Co một gối”. Cũng kế thừa sau Unpack hoàn toàn nếu nhận diện được tư thế gốc. Giữ tư thế tự tạo, Lật người bạn tự tắt và chuyển động nghiêng thay thế.
- Sửa lỗi avatar thoáng nằm sấp khi được chọn trong VRChat. Cũng sửa lỗi thoáng nằm ngửa khi nhảy mà chỉ chọn nằm sấp. Có thể tắt đổi bằng nhảy qua “Tính năng → Tiện ích → Nhảy để đổi tư thế nằm”. Mặc định bật.
- Nếu tham chiếu Animator Controller tư thế chung bị mất sau cập nhật và có thể khôi phục, hiển thị cảnh báo rồi tạo lại từ thiết lập. Giữ cả tham chiếu Controller dùng chung bởi nhiều avatar.
- Nếu dự án có lỗi biên dịch, dừng ngay đầu quá trình dựng và giải thích nguyên nhân. Hoãn tự dọn Controller được tạo khi lỗi mã lệnh ngăn kiểm tra tham chiếu.
- Ưu tiên Prefab chính thức khi thêm công cụ dù dự án có Prefab cùng tên sản phẩm. Cũng tiếp tục tạo trước nếu Controller chung chỉ khác ký tự xuống dòng.
- Write Defaults của layer FX do công cụ tư thế thêm, như Độ cao sàn, nay theo thiết lập avatar.

Nếu từng dùng tạo trước, sau cập nhật hãy nhấn “Cập nhật dữ liệu tạo trước” trên công cụ trong Scene, lưu Scene rồi dựng và tải avatar lên lại.

## [3.1.0] - 2026-11-07

- Hỗ trợ hiệu chỉnh chi tự động (IK) theo vóc dáng cho tư thế tương thích, hiệu chỉnh riêng avatar và ngăn đầu gối gập ngược.
- Điều chỉnh vị trí và hướng avatar bằng Chế độ chụp. Có thể chọn đồng bộ vị trí/hướng và hiển thị đầu mình khi chụp.
- “Độ cao sàn” di chuyển toàn thân lên xuống tối đa 3m, dùng riêng với “Độ cao chân”.
- Thêm hoạt ảnh thở, thiết lập ngừng đung đưa mặc định khi chờ và giảm méo tư thế do hướng cổ.
- Kiểm tra tính năng dùng và mức tham số trong Inspector. Hỗ trợ tìm tư thế, thêm hàng loạt, sắp xếp và xem trước đơn giản trong Scene.
- Tăng tốc chuyển đứng, khom và ngồi trong chế độ máy tính.
- Cải thiện cảnh báo tạo trước sau đổi thiết lập, tải khi cuộn Inspector và dừng lâu lúc dọn kết quả được tạo.
- Sửa lỗi khi nhập tự động ON khiến dữ liệu tạo trước không đổi thiết lập vẫn báo cần cập nhật và tạo lại khi tải lên. Kết quả nhập áp dụng cho avatar được tạo, không đổi thiết lập nhập đối tượng gốc.
- Thêm Lật người theo nghiêng đầu và thiết lập chuyển động tương thích từng tư thế.
- Cải thiện kế thừa Lật người từ thiết lập sẵn cũ, nhập chuyển động riêng và quản lý tài nguyên được tạo.

## [3.1.0-beta.2] - Beta mở (2026-11-05)

- Thêm chiều dài cố định và chênh lệch cao (Y)/trước sau (Z) từ Head đến Viewpoint làm cơ sở chiều dài vị trí IK.
- Cảnh báo Inspector cũng hiển thị bên trái tên đối tượng Hierarchy. Hỗ trợ tạo trước chưa chạy, thay đổi thiết lập và cảnh báo đang hiển thị từ nhập chuyển động hoặc thao tác thiết lập sẵn.
- Sửa lỗi tự dọn kết quả, như khi mở Scene, nạp Controller từ Scene khác khiến Unity dừng lâu.
- Sửa lịch sử tạo trước bị mất khi thay danh sách tư thế hoặc tính năng. Nay báo thiết lập đã đổi sau tạo trước thay vì chưa chạy, và Hierarchy cũng báo cần cập nhật.
- Dùng chung kết quả so sánh tạo trước cho Inspector/Hierarchy để giảm tải cuộn khi thiết lập và tài nguyên tham chiếu không đổi. So sánh lúc dựng vẫn chạy mỗi lần như trước.

## [3.1.0-beta.1] - Beta mở (2026-11-04)

- Đổi chuyển tư thế thường trong chế độ máy tính sang một lần đánh giá. Có hiệu lực cho dựng thường và tạo trước; VR và điều kiện đặc biệt giữ đường xử lý cũ.
- Nhận diện dữ liệu tạo trước từ alpha.3 trở về trước là cần cập nhật.
- Giữ góc nhìn khi đổi Độ cao sàn lúc chụp, rồi trở về Độ cao sàn mới nhất khi kết thúc chụp.

- Đổi nhập tự động thành chỉ phát hiện lúc tạo; tạo trước/dựng không thêm hoặc sửa thiết lập nhập thủ công.
- Áp dụng ON/OFF nhập tự động, thay Base và xóa thiết lập thủ công khi tạo lại. Giữ lựa chọn và hiệu chỉnh thủ công dùng chung nhiều sản phẩm, trả chuyển động nhập cũ về tư thế mặc định.
- Quản lý Controller tạo trước trong 1 tệp mỗi avatar theo GUID Scene. Tách tham chiếu dùng chung từ bản sao, Scene khác hoặc Prefab trước cập nhật.
- Dùng quản lý tài nguyên tạm NDMF lúc dựng, không tăng tệp tạo ra cho chỉnh sửa.
- Kiểm tra kết quả khi mở/lưu Scene. Giữ khi có chỉnh sửa chưa lưu, Undo/Redo hoặc tham chiếu ngoài; chỉ đưa tệp không dùng có chủ sở hữu rõ vào thùng rác.
- Áp dụng xoay, hoạt ảnh hiệu chỉnh và vị trí phát của chuyển động nhập. Thống nhất xử lý xoay, RootT/RootQ và hiệu chỉnh Humanoid với tư thế thường.
- Sao chép hiệu chỉnh BlendTree lồng riêng cho đích nhập, giữ chuyển động gốc và thiết lập hòa trộn đi bộ.
- Vị trí phát “chỉ đứng” được triển khai thành tư thế tĩnh theo tốc độ khung hình gốc mà không dừng phát phía đi bộ.

## [3.1.0-alpha.2] - Bản thử kín (2026-10-26)

### Sửa và thay đổi

- Áp dụng hiệu chỉnh Chế độ chụp và Độ cao sàn cả cho chuyển động mặc định/tùy chỉnh khi chưa chọn tư thế công cụ.
- Mở rộng Độ cao sàn lên xuống 3m. 50% là độ cao gốc và giá trị ban đầu, 0% thấp hơn 3m, 100% cao hơn 3m.
- Sửa ghi đè “chỉ đứng” làm thay BlendTree nguồn nhập.
- Dùng hiệu chỉnh thủ công cho tư thế không có quy tắc IK và giảm cảnh báo tạo không cần thiết.
- Sửa nhầm lẫn bản nháp hiệu chỉnh tư thế trùng tên và bỏ sót thay chuyển động trong cùng tài nguyên.
- Sửa số tư thế ban đầu không hợp lệ chọn nhầm tư thế khác.
- Hiển thị hướng dẫn thay vì ngoại lệ khi nhập AnimatorOverrideController không hỗ trợ.
- Giữ Lật người OFF rõ ràng khi áp dụng thiết lập sẵn cũ.
- Cải thiện độ rõ biểu tượng tính năng trên giao diện Editor sáng.

Đang tiếp tục kiểm tra góc nhìn và kết quả thay chuyển động mặc định trong ứng dụng VRChat.

## [3.1.0-alpha.1] - Trước công bố

### Đã thêm

- Thêm ON/OFF từng tính năng và hiển thị mức dùng ExpressionParameters.
- Thêm Chế độ chụp di chuyển avatar về trước và xoay.
- Thêm lựa chọn đồng bộ Chế độ chụp: chỉ cục bộ dùng 0 bit, đồng bộ dùng 16 bit.
- Thêm thiết lập giữ đầu avatar mình hiển thị trong Chế độ chụp.
- Thêm “Độ cao sàn” di chuyển toàn thân gồm đầu lên xuống 3m. 50% là độ cao gốc và ban đầu, 0% thấp hơn 3m, 100% cao hơn 3m.
- Thêm Lật người theo nghiêng đầu và thiết lập hoạt ảnh tương thích từng tư thế.
- Thêm hoạt ảnh thở cùng các mức Nhẹ, Vừa, Mạnh, Nhanh và Tùy chỉnh.
- Thêm lựa chọn riêng cho hoạt ảnh thở và menu ON/OFF trong trò chơi. Chỉ chuyển động dùng 0 bit; đồng bộ ON/OFF dùng 1 bit.
- Thêm thiết lập ngừng đung đưa từ chuyển động chờ mặc định VRChat.
- Thêm thiết lập giảm méo tư thế do khác hướng cổ.
- Thêm hiệu chỉnh chi tự động (IK) khớp tư thế tương thích theo vóc dáng và ON/OFF toàn avatar/từng tư thế.
- Thêm “Ghi IK vào hiệu chỉnh” để lưu kết quả IK thành hoạt ảnh hiệu chỉnh.
- Thêm hiệu chỉnh tư thế riêng avatar, ngăn đầu gối gập ngược và góc gập tối thiểu.
- Thêm xem trước đơn giản từ danh sách tư thế để kiểm tra trong Scene.
- Thêm thẻ loại tư thế, tìm xuyên loại, thêm nhiều hoạt ảnh cùng lúc và chỉ định vị trí đích trong thứ tự.
- Thêm hiển thị lịch sử thay đổi sản phẩm/hệ thống chung trong Inspector.

### Đã thay đổi

- Tăng tốc nạp thiết lập sẵn và giảm chờ khi thêm công cụ hoặc hiển thị danh sách.
- Tự cập nhật danh sách khi thêm, sửa hoặc xóa thiết lập sẵn.
- Sắp Inspector thành 3 thẻ “Tính năng”, “Tư thế” và “Thiết lập avatar”.
- Thêm mô tả, biểu tượng và thống nhất cách mở/đóng thiết lập chi tiết.
- Chuyển chi tiết thở vào thiết lập tính năng, thiết lập sẵn xuống cuối Thiết lập avatar.
- Tách chỉ định chuyển động riêng và tự nhập tư thế gốc thành thiết lập riêng.
- Giảm lặp hoạt ảnh toàn thân tạo cho Độ cao chân.
- Đổi cập nhật ảnh tư thế thành nút có hộp thoại xác nhận.
- Gom tùy chọn hiển thị cảnh báo trong “Kiểm tra thiết lập”.
- Bỏ thêm mới Prefab riêng 8bit, thống nhất lựa chọn tính năng trên Prefab thường. Giữ tham chiếu hiện có.
- Đổi phiên bản VRChat Avatars SDK tối thiểu thành 3.10.4.

### Đã sửa

- Đưa cả chuyển động mặc định/tùy chỉnh khi chưa chọn tư thế công cụ vào hiệu chỉnh Chế độ chụp và Độ cao sàn.
- Sửa ghi đè “chỉ đứng” làm thay BlendTree nguồn nhập.
- Dùng hiệu chỉnh thủ công cho tư thế không có quy tắc IK hợp lệ và giảm cảnh báo tạo IK không cần thiết.
- Sửa hiệu chỉnh tư thế trùng tên bị lẫn và thay chuyển động trong cùng tài nguyên không có hiệu lực khi tạo lại.
- Ngăn chọn ban đầu khi số của tư thế ban đầu đã tắt được dùng lại cho tư thế khác.
- Hiển thị hướng dẫn thay ngoại lệ khi nhập tư thế gốc gặp AnimatorOverrideController không hỗ trợ.
- Giữ thiết lập Lật người được đặt OFF rõ ràng khi áp dụng thiết lập sẵn cũ.
- Cải thiện nền biểu tượng Inspector để nhận biết tính năng trên giao diện Editor sáng.

- Sửa biểu tượng “Lật người”, “Độ cao sàn” và “Chế độ chụp” không xuất hiện lúc thêm vào avatar.
- Sửa mất thiết lập Lật người của tư thế hỗ trợ khi áp dụng thiết lập sẵn cũ.
- Sửa nhập tên bị chuyển sang tư thế khác khi mở chi tiết khác trong lúc sửa tên.
- Sửa chi tiết bị đóng khi sắp xếp tư thế và xử lý Undo/Redo thay đổi thiết lập.
- Sửa avatar gốc chồng trong xem trước và vị trí đặt bị cộng hai lần.
- Sửa lỗi xem trước avatar thiếu thành phần.
- Sửa hướng/vị trí chi IK lệch do khác hướng và tỷ lệ xương avatar.
- Sửa cảnh báo hoặc trạng thái tạo trước không cập nhật ngay sau đổi thiết lập.
- Sửa tái dùng dữ liệu tạo trước cũ sau đổi IK.
- Sửa lỗi dựng do tạo tính năng bổ sung thất bại ở một số cấu hình.

## [3.0.16] - 2026-09-14
- Cập nhật phiên bản phụ thuộc NDMF tối thiểu lên 1.14.0 vì sử dụng API `VirtualAnimatorController.SetParameter`.

## [3.0.15] - 2026-09-13
- Sửa vị trí cơ thể hoặc góc gập gối thay đổi lớn khi bật/tắt khóa cổ tư thế nằm ngửa trên avatar kết hợp thay tỷ lệ Hips với FloorAdjuster.

## [3.0.14] - 2026-09-09
- Sửa mục menu “Tắt cử chỉ” của công cụ thứ 2 trở đi bị mất khi cài nhiều công cụ tư thế trên cùng avatar.
- Sửa layer USSPS_Locomotion đôi khi trở về Standing dù đã chỉ định tư thế như SleepDown.
- Sửa lệch độ cao/vị trí X/Z của tư thế khi dùng FloorAdjuster và hỗ trợ thứ tự xử lý của bản MA/AAO.
- Khi AnimatorController tạo trước trong Generated bị xóa, tự khôi phục về PosingSystem_Locomotion mặc định.
- Sửa lỗi dựng khi đã xóa mọi tư thế đứng.
- Hiển thị danh sách thiết lập sẵn theo tên avatar.

## [3.0.13] - 2026-09-04
- Sửa tự chuyển vào chính trạng thái đứng khiến khôi phục tư thế thiếu ổn định khi đứng lại ngay sau khi bắt đầu ngồi xổm trong VR.

## [3.0.12] - 2026-09-04
- Sửa nhận nhầm thiết lập đã đổi khi dựng bằng NDMF với công cụ chưa tạo trước.
- Sửa thêm tài nguyên được tạo vào AnimatorController tạm trong lúc dựng NDMF gây lỗi `AddAssetToSameFile`.

## [3.0.11] - 2026-08-31
- Sửa Scene của avatar tạo trước trở nên không hợp lệ ở NDMF 1.14.7 trở lên khiến xử lý thất bại.
- Sửa màn hình lỗi NDMF tham chiếu LocalizationAsset đã hủy gây `MissingReferenceException`.

## [3.0.10] - 2026-08-29
- Trì hoãn nạp tài nguyên định nghĩa thiết lập sẵn tới khi mở UI chọn; dùng lại cache nếu danh mục GUID không đổi.
- Chỉ đánh giá lại biểu tượng cảnh báo/lỗi Hierarchy khi có thay đổi và dùng chung cache tìm tài nguyên menu sản phẩm. Giảm Editor bị đứng mỗi vài giây ở Scene có nhiều công cụ tư thế.
- Sửa `ArgumentException` khi đăng ký giá trị tham số trùng trong tạo trước nhiều công cụ trên cùng avatar.

## [3.0.9] - 2026-08-26
- Nếu không lấy được VRCAvatarDescriptor từ PosingSystem khi chuyển đổi, ngăn NullReferenceException và chỉ dẫn log dựng cần để điều tra.
- Sửa lần tạo trước/dựng sau tái dùng xem trước tham chiếu Avatar tạm đã bị NDMF tạo/xóa gây `Avatar is null`. Tách chuyển đổi khỏi cache xem trước và ngăn trùng tên tài nguyên tạm NDMF giữa các bản xem trước.
- Sửa tham chiếu xem trước của Prefab 可愛いポーズ (Kawaii Posing) không lưu trong Scene khiến avatar xem trước ẩn tăng mãi. Loại xem trước khỏi lưu Scene và thu hồi cả bản cũ khi nạp Scene, chuyển Play hoặc nạp lại mã lệnh.

## [3.0.8] - 2026-08-18
- Sửa Unity Assertion trên chuyển động tư thế tạo trước lúc FloorAdjuster hoặc công cụ tương tự hiệu chỉnh lại độ cao, có thể làm hỏng dữ liệu tư thế.
- Cải thiện an toàn để đo chiều cao không can thiệp AnimationMode avatar gốc hoặc công cụ khác.

## [3.0.7] - 2026-08-12
- Sửa lệch góc nhìn/gập gối bất thường khi nằm ngửa lúc dùng công cụ độ cao như FloorAdjuster.
- Sửa lỗi khi PosingSystem nằm ngoài avatar (cảm ơn PancakeTorto1se).
- Sửa tiếp tục xử lý gây lỗi sau khi tải avatar lên thất bại.
- Sửa đôi khi nhận nhầm thiết lập đã đổi ngay sau tạo trước.
- Cải thiện tự nhập chuyển động di chuyển.
- Thêm cảnh báo cho một số avatar dùng rig đặc biệt.
- Thêm hộp thoại xác nhận thay đổi và lật đối xứng giá trị hiệu chỉnh trong cửa sổ Hiệu chỉnh hoạt ảnh (cảm ơn kraihd).
- Thêm hướng dẫn khi công cụ khác lỗi và kiểm tra các vị trí dễ lỗi.

## [3.0.6] - 2026-03-17
- Sửa tư thế bị rối trên một số avatar vốn có điều khiển tư thế riêng.

## [3.0.5] - 2026-01-31
- Sửa Unity bị sập với một số ExpressionMenus.
- Sửa vấn đề avatar khi dùng Prefab tích hợp ごろ寝EX (cảm ơn あずき).
- Sửa góc xoay bất thường khi ghép hoạt ảnh hiệu chỉnh.

## [3.0.4] - 2026-01-11
- Cải thiện một phần hành vi avatar khi đặt trong Prefabs.
- Thêm chức năng xóa giấy phép.
- Sửa nhiều tư thế có cổ bất thường.
- Giảm nhẹ độ nặng khi mở Inspector.
- Tạo trước cũng tạo biểu tượng. Đổi chữ nút cập nhật avatar.
- Ẩn cảnh báo nếu chưa từng tạo trước, vì có vẻ nhiều người không dùng tạo trước.
- Xóa tài nguyên không cần.
- Sửa không hiệu chỉnh được ngón tay.

## [3.0.3] - 2026-01-04
- Sửa áp dụng thiết lập sẵn khác mục đã chọn (cảm ơn あずき).

## [3.0.2] - 2026-01-02
- Hỗ trợ giao diện tối Unity.
- Sửa tư thế sai khi xoay trong Hiệu chỉnh hoạt ảnh.
- Đổi kích thước biểu tượng menu, bật nén và thêm tùy chọn biểu tượng nhỏ hơn.
- Thêm lại tích hợp theo dõi và tự nhập chuyển động avatar từng bị bỏ ở 3.0.0.

## [3.0.1] - 2025-12-29
- Sửa tính năng thiết lập chuyển động mặc định.
- Sửa tư thế avatar tự trở nên bất thường.

## [3.0.0] - 2025-12-28
- Thêm tính năng tạo trước.
- Thêm Hiệu chỉnh hoạt ảnh.
- Thêm thiết lập sẵn.

## [2.2.5] - 2025-05-08
- Giải quyết tương thích với một số công cụ.

## [2.2.4] - 2025-04-04
- Sửa tư thế không đổi ở một số avatar và phiên bản ModularAvatar.

## [2.2.3] - 2025-03-13
- Sửa chuyển động bất thường ở một số avatar, như nhảy của 森羅.
- Sửa giá trị ban đầu Tắt cử chỉ đôi khi sai.

## [2.2.2] - 2025-03-06
- Sửa lỗi ở một số môi trường.

## [2.2.1] - 2025-02-05
- Sửa Unity bị sập.
- Thêm phiên bản vào tên công cụ hiển thị trong NDMF Console.

## [2.2.0] - 2025-02-02
- Thêm thiết lập tắt cử chỉ và dùng hình bàn tay trong hoạt ảnh.

## [2.1.6] - 2025-02-02
- Sửa tư thế hỏng khi dùng cùng một số công cụ.
- Sửa lỗi do một số thiết lập.

## [2.1.5] - 2024-12-10
- Giải quyết xung đột với một số công cụ.
- Thêm nút xóa bản Unitypackage.

## [2.1.4] - 2024-12-08
- Sửa lỗi ở một số avatar.
- Sửa không dùng được Khóa chân do xung đột công cụ/avatar.
- Sửa tư thế avatar trở lại mặc định VRC trong chế độ máy tính.

## [2.1.3] - 2024-12-01
- Sửa không đổi được tư thế khi dùng với avatar/công cụ sử dụng sai kiểu VRMode.
- Thêm hiển thị NDMF-Console khi công cụ xung đột do khác kiểu tham số.

## [2.1.2] - 2024-12-01
- Thêm giải thích trong NDMF-Console vì lỗi công cụ khác gây lỗi Unisaki Posing System khi dựng avatar.
- Sửa lỗi dựng ở một số avatar.
- Cải thiện hành vi ngay sau nạp avatar.
- Bỏ xử lý di chuyển gốc avatar vì có thể khiến không hoạt động trên một số avatar.

## [2.1.1] - 2024-11-20
- Sửa vấn đề MergeAnimator trong PosingOverride.

## [2.1.0] - 2024-11-20
- Giải quyết chân/hông chìm xuống đất trong chế độ máy tính để mọi tư thế hiển thị đẹp.
- Thêm đổi tư thế bỏ qua độ cao đầu.
- Thêm chế độ không tạo biểu tượng để tải avatar Quest lên.
- Thêm một số cảnh báo khi avatar có vấn đề.
- Giải quyết xung đột khóa với AFK hoặc công cụ khóa tay khác.
- Thêm tự khóa cổ khi nằm ngửa.

## [2.0.0] - 2024-11-12
- Phát hành bản hỗ trợ VPM/VCC.
- Đổi tên hiển thị công cụ khi lỗi #1.
- Thêm điều khoản sử dụng #5.
