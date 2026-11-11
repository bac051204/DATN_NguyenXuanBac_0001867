# Backlog tính năng
# Hệ thống Quản lý Bán hàng & Kho

| Thông tin | Giá trị |
|---|---|
| Phiên bản | 1.0 |
| Ngày lập | 07/11/2026 |
| Tài liệu gốc | [SRS.md](SRS.md) phiên bản 1.1, [UseCases.md](UseCases.md) phiên bản 1.0 |
| Tổng số mục | 77 mục, 206 story point |

Backlog chỉ gồm các chức năng và yêu cầu phi chức năng đã có trong SRS. Mỗi mục dẫn chiếu tới mã yêu cầu (FR/NFR) và use case (UC) tương ứng; cột "Ghi chú" nêu user story gốc trong UseCases.md (US-xx) hoặc tiêu chí chấp nhận trong SRS (AC-xx) nếu có.

---

## 1. Quy ước

### 1.1. Vai trò

Dùng 3 actor của [UseCases.md](UseCases.md) mục 0: **Admin** (gồm quyền Quản trị viên và Quản lý cửa hàng của SRS), **Quản lý kho** (Nhân viên kho) và **Nhân viên bán hàng**. "Người dùng" nghĩa là mọi vai trò.

### 1.2. Độ ưu tiên MoSCoW

| Mức | Ý nghĩa | Quy tắc gán |
|---|---|---|
| **Must** | Bắt buộc cho bản nghiệm thu | Yêu cầu SRS ưu tiên **Cao** (trừ module Trợ lý AI) và các NFR bảo mật, toàn vẹn dữ liệu, khả năng kiểm chứng |
| **Should** | Nên có, làm sau khi xong Must | Yêu cầu SRS ưu tiên **TB**; phần lõi của Trợ lý AI |
| **Could** | Có thì tốt, bỏ được nếu thiếu thời gian | Tiện ích ngoài luồng nghiệp vụ chính; phần mở rộng của Trợ lý AI |

Các điểm lệch so với độ ưu tiên trong SRS được liệt kê ở [mục 4](#4-điểm-lệch-so-với-độ-ưu-tiên-trong-srs).

### 1.3. Story point

Thang tương đối **1 – 2 – 3 – 5 – 8**, so với mốc: 1 điểm ≈ một thay đổi nhỏ trên một tầng (ví dụ thêm ràng buộc kiểm tra); 3 điểm ≈ một chức năng CRUD đầy đủ API + giao diện + kiểm thử đơn vị; 8 điểm ≈ chức năng có giao dịch, xử lý đồng thời hoặc tích hợp dịch vụ ngoài.

### 1.4. Mã mục

| Tiền tố | Module |
|---|---|
| PLT | Nền tảng (xác thực, phân quyền, cấu hình, logging) |
| CAT | Danh mục |
| PRD | Sản phẩm |
| SUP | Nhà cung cấp |
| CUS | Khách hàng |
| PUR | Đơn nhập |
| SAL | Đơn bán |
| INV | Tồn kho |
| RPT | Báo cáo |
| AI | Trợ lý AI bán hàng |

---

## 2. Backlog theo module

### 2.1. Nền tảng (PLT)

| Mã | User story | FR / UC | MoSCoW | SP | Ghi chú |
|---|---|---|---|---|---|
| PLT-01 | Là **người dùng**, tôi muốn đăng nhập bằng tên đăng nhập và mật khẩu, để sử dụng hệ thống với đúng quyền của mình. | FR-01 · UC-01 | Must | 3 | JWT 60 phút + refresh token 7 ngày; AC-01 |
| PLT-02 | Là **Admin**, tôi muốn tài khoản bị khóa 15 phút sau 5 lần sai mật khẩu liên tiếp, để chống dò mật khẩu. | FR-02 · UC-01 | Must | 2 | AC-01 |
| PLT-03 | Là **Admin**, tôi muốn tạo, sửa tài khoản và gán đúng một vai trò, để mỗi nhân viên chỉ dùng các chức năng thuộc công việc của mình. | FR-03 · UC-02 | Must | 3 | US-AD-01 |
| PLT-04 | Là **Admin**, tôi muốn vô hiệu hóa tài khoản và từ chối mọi token hiện có của tài khoản đó, để nhân viên đã nghỉ không còn truy cập được. | FR-03 · UC-02 | Must | 2 | US-AD-02 |
| PLT-05 | Là **Admin**, tôi muốn mọi API kiểm tra quyền theo ma trận vai trò (trái quyền trả HTTP 403) và giao diện ẩn chức năng không có quyền, để dữ liệu nhạy cảm được bảo vệ ở phía server. | FR-04, NFR-03 · UC-02 | Must | 5 | AC-02 |
| PLT-06 | Là **người dùng**, tôi muốn tự đổi mật khẩu khi nhập đúng mật khẩu hiện tại, để chủ động bảo vệ tài khoản. | FR-05 · UC-02 | Should | 2 | Mật khẩu ≥ 8 ký tự, có hoa, thường, số |
| PLT-07 | Là **người dùng**, tôi muốn đăng xuất và refresh token bị thu hồi, để người khác không dùng lại được phiên của tôi. | FR-06 · UC-01 | Should | 1 | |
| PLT-08 | Là **Admin**, tôi muốn mật khẩu được băm có salt và các bí mật (chuỗi kết nối, khóa ký JWT, khóa API LLM) nằm ngoài mã nguồn, để lộ mã nguồn không làm lộ thông tin đăng nhập. | NFR-01, NFR-05 | Must | 2 | User Secrets / biến môi trường |
| PLT-09 | Là **người dùng**, tôi muốn mọi dữ liệu nhập được kiểm tra ở server, lỗi hiển thị ngay cạnh trường và lỗi bất ngờ trả về ProblemDetails không lộ stack trace, để biết chính xác cần sửa gì. | NFR-04, NFR-15, NFR-19 | Must | 3 | |
| PLT-10 | Là **Admin**, tôi muốn có nhật ký kiểm toán cho đăng nhập, thao tác tài khoản, đổi giá bán, hủy đơn và duyệt điều chỉnh tồn kho, để truy được trách nhiệm khi có sai sót. | NFR-06 | Should | 3 | |
| PLT-11 | Là **Admin**, tôi muốn hệ thống ghi log ứng dụng có cấu trúc (mức log, mã truy vết request) cho mọi lỗi phía server, để chẩn đoán sự cố khi phía client chỉ nhận ProblemDetails. | NFR-15 | Should | 2 | Không ghi mật khẩu vào log (NFR-01) |
| PLT-12 | Là **giảng viên hướng dẫn / hội đồng**, tôi muốn Swagger mô tả đầy đủ mọi endpoint và gọi thử được với JWT, để kiểm chứng API độc lập với giao diện. | NFR-21 | Must | 1 | |
| PLT-13 | Là **nhóm phát triển**, tôi muốn lược đồ CSDL được quản lý bằng EF Core Migrations kèm dữ liệu mẫu, để triển khai và demo lặp lại được. | NFR-23, GD-05 | Must | 2 | |
| PLT-14 | Là **Admin**, tôi muốn cấu hình thông tin cửa hàng (tên, địa chỉ, số điện thoại), để hóa đơn in ra đúng thông tin cửa hàng. | FR-BAN-21 | Should | 2 | Dữ liệu đầu vào cho SAL-15 |
| PLT-15 | Là **Admin**, tôi muốn CSDL được sao lưu tự động hằng ngày và giữ 7 bản gần nhất, để khôi phục được khi có sự cố. | NFR-14 | Could | 2 | |
| PLT-16 | Là **người dùng**, tôi muốn giao diện bằng tiếng Việt, tiền tệ dạng `1.234.567 ₫` và ngày dạng `dd/MM/yyyy`, để đọc số liệu quen thuộc. | NFR-16 | Must | 1 | |

### 2.2. Danh mục (CAT)

| Mã | User story | FR / UC | MoSCoW | SP | Ghi chú |
|---|---|---|---|---|---|
| CAT-01 | Là **Admin**, tôi muốn tạo danh mục (tên 2–100 ký tự, không trùng trong cùng danh mục cha, tối đa 2 cấp), để tổ chức sản phẩm theo nhóm. | FR-07 · UC-03 | Must | 2 | US-AD-03 |
| CAT-02 | Là **Admin**, tôi muốn sửa tên, mô tả, danh mục cha và được chặn nếu tạo ra cấu trúc quá 2 cấp hoặc vòng lặp, để cây danh mục luôn hợp lệ. | FR-08 · UC-03 | Must | 2 | US-AD-03 |
| CAT-03 | Là **Admin**, tôi muốn được chặn khi xóa danh mục còn sản phẩm hoặc danh mục con, kèm số lượng đang liên kết, để không làm mất liên kết dữ liệu. | FR-09 · UC-03 | Must | 1 | US-AD-03 |
| CAT-04 | Là **Admin**, tôi muốn xem danh mục dạng cây kèm số sản phẩm đang kinh doanh, để nắm nhanh cơ cấu hàng hóa. | FR-10 · UC-03 | Should | 2 | |

### 2.3. Sản phẩm (PRD)

| Mã | User story | FR / UC | MoSCoW | SP | Ghi chú |
|---|---|---|---|---|---|
| PRD-01 | Là **Admin**, tôi muốn thêm sản phẩm với SKU duy nhất, danh mục, đơn vị tính, giá bán chưa thuế, thuế suất, mã vạch và ngưỡng tồn tối thiểu, để sản phẩm có thể được nhập kho và bán. | FR-11 · UC-04 | Must | 3 | US-AD-04; tồn ban đầu = 0 |
| PRD-02 | Là **Admin**, tôi muốn tải ảnh sản phẩm (JPG/PNG, ≤ 2 MB), để nhân viên nhận diện đúng hàng hóa. | FR-11 · UC-04 | Must | 2 | |
| PRD-03 | Là **Admin**, tôi muốn sửa thông tin sản phẩm (trừ SKU) và mỗi lần đổi giá được lưu lịch sử, để kiểm soát ai đổi giá, khi nào. | FR-12 · UC-04 | Must | 3 | US-AD-05 |
| PRD-04 | Là **Admin**, tôi muốn chuyển sản phẩm sang "Ngừng kinh doanh" và không xóa được sản phẩm đã có giao dịch, để lịch sử giao dịch luôn nguyên vẹn. | FR-13 · UC-04 | Must | 2 | US-AD-06; AC-03 |
| PRD-05 | Là **người dùng**, tôi muốn tìm sản phẩm theo SKU, mã vạch hoặc một phần tên (không phân biệt hoa thường, dấu), lọc theo danh mục, trạng thái, khoảng giá, để tìm hàng nhanh. | FR-14, NFR-08 · UC-05 | Must | 5 | Phân trang 20 dòng; ≤ 500 ms; AC-03 |
| PRD-06 | Là **Admin**, tôi muốn giá vốn chỉ hiển thị cho Admin, để nhân viên không biết biên lợi nhuận của cửa hàng. | FR-15 · UC-05 | Should | 2 | AC-02 |

### 2.4. Nhà cung cấp (SUP)

| Mã | User story | FR / UC | MoSCoW | SP | Ghi chú |
|---|---|---|---|---|---|
| SUP-01 | Là **Quản lý kho**, tôi muốn lưu nhà cung cấp với mã số thuế, số điện thoại, email được kiểm tra định dạng, để chọn nhanh khi lập đơn nhập. | FR-16 · UC-06 | Must | 2 | US-QK-01 |
| SUP-02 | Là **Quản lý kho**, tôi muốn sửa thông tin và chuyển nhà cung cấp sang "Ngừng hợp tác", để không lập nhầm đơn nhập với nhà cung cấp cũ. | FR-17 · UC-06 | Must | 2 | US-QK-02 |
| SUP-03 | Là **Quản lý kho**, tôi muốn được chặn khi xóa nhà cung cấp đã có đơn nhập, để giữ nguyên lịch sử nhập hàng. | FR-19 · UC-06 | Must | 1 | US-QK-02 |
| SUP-04 | Là **Quản lý kho**, tôi muốn xem lịch sử đơn nhập và tổng giá trị nhập của một nhà cung cấp theo khoảng thời gian, để đánh giá mức độ hợp tác. | FR-18 · UC-06 | Should | 3 | |

### 2.5. Khách hàng (CUS)

| Mã | User story | FR / UC | MoSCoW | SP | Ghi chú |
|---|---|---|---|---|---|
| CUS-01 | Là **Nhân viên bán hàng**, tôi muốn tạo và sửa khách hàng với số điện thoại duy nhất, để lưu thông tin khách quen. | FR-20 · UC-07 | Must | 2 | |
| CUS-02 | Là **Nhân viên bán hàng**, tôi muốn tìm khách theo số điện thoại hoặc tên và tạo nhanh khách mới ngay trên màn hình bán hàng, để không làm khách phải chờ. | FR-21 · UC-07 | Must | 3 | US-BH-08 |
| CUS-03 | Là **Nhân viên bán hàng**, tôi muốn xem lịch sử mua của khách (danh sách đơn, tổng số đơn, tổng chi tiêu, ngày mua gần nhất), để tư vấn phù hợp thói quen mua. | FR-22 · UC-07 | Must | 3 | US-BH-09 |
| CUS-04 | Là **Nhân viên bán hàng**, tôi muốn bán cho khách không cung cấp thông tin và đơn được ghi nhận là "Khách lẻ", để không bắt buộc hỏi thông tin khách. | FR-23 · UC-07 | Should | 1 | |

### 2.6. Đơn nhập (PUR)

| Mã | User story | FR / UC | MoSCoW | SP | Ghi chú |
|---|---|---|---|---|---|
| PUR-01 | Là **Quản lý kho**, tôi muốn lập đơn nhập nháp gồm nhiều dòng hàng với số lượng và giá nhập, hệ thống tự sinh mã `PN-yyyyMMdd-xxxx` và tính tổng tiền, để ghi nhận chính xác lô hàng. | FR-24 · UC-08 | Must | 3 | US-QK-03 |
| PUR-02 | Là **Quản lý kho**, tôi muốn sửa hoặc hủy đơn nhập khi còn Nháp và không sửa được đơn đã xác nhận, để chứng từ đã chốt không bị thay đổi. | FR-25 · UC-08 | Must | 2 | AC-04 |
| PUR-03 | Là **Quản lý kho**, tôi muốn xác nhận đơn nhập để hệ thống cộng tồn, tính lại giá vốn bình quân gia quyền và ghi lịch sử kho trong một giao dịch, để tồn kho và giá vốn luôn đúng. | FR-26, NFR-13 · UC-08 | Must | 5 | US-QK-04; AC-04 |
| PUR-04 | Là **Admin**, tôi muốn hủy đơn nhập đã xác nhận (bị từ chối nếu tồn hiện tại không đủ để trừ), để sửa sai sót nhập hàng. | FR-27 · UC-09 | Should | 3 | US-AD-12 |
| PUR-05 | Là **Quản lý kho**, tôi muốn tra cứu đơn nhập theo mã, nhà cung cấp, trạng thái, khoảng ngày và in phiếu nhập, để đối chiếu với nhà cung cấp. | FR-28 · UC-08 | Should | 3 | |

### 2.7. Đơn bán (SAL)

| Mã | User story | FR / UC | MoSCoW | SP | Ghi chú |
|---|---|---|---|---|---|
| SAL-01 | Là **Nhân viên bán hàng**, tôi muốn tạo đơn bán nháp với mã `HD-yyyyMMdd-xxxx` không trùng kể cả khi nhiều người tạo cùng lúc, để mỗi hóa đơn được định danh duy nhất. | FR-BAN-01 · UC-10 | Must | 3 | US-BH-01 |
| SAL-02 | Là **Nhân viên bán hàng**, tôi muốn gắn hoặc đổi khách hàng của đơn khi đơn còn Nháp, để đơn được ghi vào đúng lịch sử mua của khách. | FR-BAN-02 · UC-10 | Must | 2 | |
| SAL-03 | Là **Nhân viên bán hàng**, tôi muốn mở song song tối đa 5 đơn nháp và chuyển qua lại không mất dữ liệu, để phục vụ khách quay lại lấy thêm hàng. | FR-BAN-03 · UC-10 | Could | 3 | Hạ từ TB, xem mục 4 |
| SAL-04 | Là **Admin**, tôi muốn đơn nháp không thao tác trong 24 giờ tự chuyển "Đã hủy" với lý do "Hết hạn", để danh sách đơn không tồn đọng. | FR-BAN-04 · UC-10 | Could | 3 | Hạ từ TB, xem mục 4; cần tác vụ nền |
| SAL-05 | Là **Nhân viên bán hàng**, tôi muốn thêm sản phẩm bằng tìm kiếm hoặc quét mã vạch (quét trùng thì tăng số lượng), để lập đơn nhanh và không gõ nhầm. | FR-BAN-05 · UC-10 | Must | 3 | US-BH-01 |
| SAL-06 | Là **Nhân viên bán hàng**, tôi muốn sửa số lượng (1–9.999) hoặc xóa dòng và thấy tổng tiền cập nhật ngay, để điều chỉnh đơn theo yêu cầu khách. | FR-BAN-06, FR-BAN-07 · UC-10 | Must | 2 | 1–100 dòng/đơn; ≤ 300 ms |
| SAL-07 | Là **Admin**, tôi muốn đơn giá và thuế suất được chốt khi đơn hoàn tất, để đổi giá sau này không làm sai hóa đơn cũ. | FR-BAN-08 · UC-10 | Must | 2 | US-AD-05 |
| SAL-08 | Là **Nhân viên bán hàng**, tôi muốn bị chặn khi số lượng vượt tồn kho và thấy số tồn còn lại, để không hứa bán hàng đã hết. | FR-BAN-09 · UC-10 | Must | 2 | US-BH-02; AC-05 |
| SAL-09 | Là **Nhân viên bán hàng**, tôi muốn thấy nhãn "Sắp hết" khi tồn sau bán ≤ ngưỡng tối thiểu, để báo kho bổ sung kịp thời. | FR-BAN-10 · UC-10 | Should | 1 | US-BH-02 |
| SAL-10 | Là **Nhân viên bán hàng**, tôi muốn giảm giá từng dòng theo số tiền hoặc %, để áp dụng chính sách giá của cửa hàng. | FR-BAN-16 · UC-10 | Must | 2 | US-BH-03 |
| SAL-11 | Là **Nhân viên bán hàng**, tôi muốn giảm giá cả đơn được phân bổ tự động vào các dòng, và giảm giá vượt 10% phải có Admin xác nhận bằng mật khẩu, để chiết khấu lớn luôn được kiểm soát. | FR-BAN-17 · UC-10 | Must | 5 | US-BH-03; AC-05 |
| SAL-12 | Là **Nhân viên bán hàng**, tôi muốn thấy tổng chưa thuế, tiền thuế theo từng mức thuế suất và tổng thanh toán, để báo đúng số tiền cho khách. | FR-BAN-18, FR-BAN-19 · UC-10 | Must | 5 | US-BH-04 |
| SAL-13 | Là **Nhân viên bán hàng**, tôi muốn chọn thanh toán tiền mặt hoặc chuyển khoản và thấy ngay tiền thừa, để trả lại tiền chính xác. | FR-BAN-20 · UC-11 | Must | 2 | US-BH-05; AC-05 |
| SAL-14 | Là **Nhân viên bán hàng**, tôi muốn khi hoàn tất đơn, hệ thống kiểm tra lại tồn mọi dòng, trừ tồn, ghi lịch sử kho và lưu giá vốn trong một giao dịch an toàn khi bán đồng thời, để tồn kho không bao giờ sai hoặc âm. | FR-BAN-11 – FR-BAN-14 · UC-11 | Must | 8 | US-BH-06; AC-05 |
| SAL-15 | Là **Nhân viên bán hàng**, tôi muốn in hóa đơn khổ 80 mm hoặc A4 ngay sau khi thanh toán, để giao cho khách làm chứng từ. | FR-BAN-21 · UC-11 | Must | 3 | US-BH-07 |
| SAL-16 | Là **Admin**, tôi muốn hủy đơn bán đã hoàn tất kèm lý do, để sửa sai sót mà tồn kho và doanh thu vẫn chính xác. | FR-BAN-15 · UC-12 | Must | 3 | US-AD-07; AC-06 |
| SAL-17 | Là **Nhân viên bán hàng**, tôi muốn tra cứu các đơn mình đã lập và in lại hóa đơn, để phục vụ khách cần chứng từ. | FR-BAN-22 · UC-13 | Should | 2 | US-BH-12 |
| SAL-18 | Là **Nhân viên bán hàng**, tôi muốn thao tác màn hình bán hàng hoàn toàn bằng bàn phím, để lập đơn nhanh trong giờ cao điểm. | NFR-17 · UC-10 | Should | 2 | |

### 2.8. Tồn kho (INV)

| Mã | User story | FR / UC | MoSCoW | SP | Ghi chú |
|---|---|---|---|---|---|
| INV-01 | Là **Quản lý kho**, tôi muốn xem danh sách tồn kho và lọc theo danh mục, trạng thái Hết hàng / Dưới ngưỡng / Bình thường, để biết ngay mặt hàng cần xử lý. | FR-37 · UC-14 | Must | 3 | US-QK-05; giá vốn theo quyền |
| INV-02 | Là **Quản lý kho**, tôi muốn được cảnh báo trên trang tổng quan khi tồn ≤ ngưỡng, kèm số lượng gợi ý nhập, để đặt hàng kịp thời. | FR-38 · UC-14 | Must | 2 | US-QK-06; AC-07 |
| INV-03 | Là **Quản lý kho**, tôi muốn xem lịch sử biến động kho của từng sản phẩm và lịch sử này không thể sửa hoặc xóa, để truy nguyên nhân khi tồn kho bất thường. | FR-39 · UC-14 | Must | 3 | US-QK-08; AC-07 |
| INV-04 | Là **Quản lý kho**, tôi muốn lập phiếu kiểm kê với số lượng thực tế và hệ thống tự tính chênh lệch, để gửi Admin duyệt. | FR-40 · UC-15 | Should | 3 | US-QK-07 |
| INV-05 | Là **Admin**, tôi muốn duyệt phiếu kiểm kê để cập nhật tồn kho theo thực tế và ghi lịch sử "Điều chỉnh", để tồn kho khớp thực tế và có người chịu trách nhiệm. | FR-41 · UC-16 | Should | 3 | US-AD-08; AC-07 |

### 2.9. Báo cáo (RPT)

| Mã | User story | FR / UC | MoSCoW | SP | Ghi chú |
|---|---|---|---|---|---|
| RPT-01 | Là **Admin**, tôi muốn xem doanh thu, giảm giá, thuế, giá vốn và lợi nhuận gộp theo ngày/tuần/tháng dạng bảng và biểu đồ cột, để đánh giá hiệu quả kinh doanh. | FR-42, NFR-09 · UC-17 | Must | 5 | US-AD-09; AC-08 |
| RPT-02 | Là **Admin**, tôi muốn xem top N sản phẩm bán chạy theo số lượng hoặc doanh thu, để quyết định nhập hàng. | FR-43 · UC-17 | Must | 2 | |
| RPT-03 | Là **Admin**, tôi muốn xem báo cáo tồn kho hiện tại (số mặt hàng, số lượng, giá trị tồn) nhóm theo danh mục, để biết vốn đang nằm trong kho. | FR-44 · UC-17 | Must | 2 | Quản lý kho xem không có giá vốn |
| RPT-04 | Là **Nhân viên bán hàng**, tôi muốn xem doanh thu các đơn mình đã bán trong khoảng thời gian chọn, để tự theo dõi kết quả làm việc. | FR-45 · UC-17 | Should | 2 | US-BH-13; Admin xem mọi nhân viên |
| RPT-05 | Là **Admin**, tôi muốn xuất mọi báo cáo ra file Excel khớp số liệu trên màn hình, để lưu trữ và gửi kế toán. | FR-46 · UC-17 | Should | 3 | US-AD-10; AC-08 |
| RPT-06 | Là **Admin**, tôi muốn trang tổng quan hiển thị doanh thu, số đơn hôm nay, so sánh với hôm qua và số sản phẩm dưới ngưỡng, để nắm tình hình trong một màn hình. | FR-47 · UC-17 | Should | 3 | |

### 2.10. Trợ lý AI bán hàng (AI)

| Mã | User story | FR / UC | MoSCoW | SP | Ghi chú |
|---|---|---|---|---|---|
| AI-01 | Là **Nhân viên bán hàng**, tôi muốn mở khung chat từ mọi màn hình và hỏi bằng tiếng Việt về giá, tồn kho, mô tả sản phẩm, trợ lý trả lời dựa trên dữ liệu thật của cửa hàng, để trả lời khách nhanh. | FR-48, NFR-11 · UC-18 | Should | 8 | US-BH-10; tool calling; AC-09 |
| AI-02 | Là **Admin**, tôi muốn trợ lý không tiết lộ giá vốn, lợi nhuận, dữ liệu khách hàng cho vai trò không có quyền, để trợ lý không thành lỗ hổng phân quyền. | FR-50 · UC-18 | Should | 3 | US-BH-10; AC-09 |
| AI-03 | Là **Nhân viên bán hàng**, tôi muốn trợ lý nói rõ khi không có thông tin thay vì bịa tên, giá, số lượng, để không báo sai cho khách. | FR-51 · UC-18 | Should | 2 | US-BH-10; AC-09 |
| AI-04 | Là **Nhân viên bán hàng**, tôi muốn thấy thông báo lỗi thân thiện trong ≤ 15 giây khi dịch vụ LLM lỗi và các chức năng khác vẫn hoạt động, để việc bán hàng không bị gián đoạn. | FR-54 · UC-18 | Should | 2 | AC-09 |
| AI-05 | Là **Admin**, tôi muốn cấu hình khóa API và tên mô hình LLM (khóa không hiển thị lại dạng rõ), nhà cung cấp LLM được trừu tượng qua interface, để đổi nhà cung cấp mà không sửa nghiệp vụ. | FR-53, NFR-05, NFR-24 · UC-20 | Should | 2 | |
| AI-06 | Là **Nhân viên bán hàng**, tôi muốn trợ lý gợi ý tối đa 5 sản phẩm đang kinh doanh, còn hàng phù hợp nhu cầu khách, kèm tên, giá, tồn, để tư vấn nhanh hơn. | FR-49 · UC-18 | Should | 5 | US-BH-11; AC-09 |
| AI-07 | Là **Nhân viên bán hàng**, tôi muốn bấm "Thêm vào đơn" bên cạnh sản phẩm được gợi ý, để đưa sản phẩm vào đơn đang mở bằng một thao tác. | FR-52 · UC-19 | Could | 2 | US-BH-11; trợ lý không tự ghi dữ liệu |
| AI-08 | Là **người dùng**, tôi muốn xem lại lịch sử hội thoại của mình trong 30 ngày, để tra lại câu trả lời trước đó. | FR-53 · UC-18 | Could | 3 | |
| AI-09 | Là **Admin**, tôi muốn giới hạn số câu hỏi mỗi người dùng mỗi ngày, để kiểm soát chi phí dịch vụ LLM. | FR-53 · UC-20 | Could | 2 | US-AD-11 |

---

## 3. Tổng hợp

| Module | Số mục | Must (SP) | Should (SP) | Could (SP) | Tổng SP |
|---|---|---|---|---|---|
| Nền tảng (PLT) | 16 | 24 | 10 | 2 | 36 |
| Danh mục (CAT) | 4 | 5 | 2 | 0 | 7 |
| Sản phẩm (PRD) | 6 | 15 | 2 | 0 | 17 |
| Nhà cung cấp (SUP) | 4 | 5 | 3 | 0 | 8 |
| Khách hàng (CUS) | 4 | 8 | 1 | 0 | 9 |
| Đơn nhập (PUR) | 5 | 10 | 6 | 0 | 16 |
| Đơn bán (SAL) | 18 | 42 | 5 | 6 | 53 |
| Tồn kho (INV) | 5 | 8 | 6 | 0 | 14 |
| Báo cáo (RPT) | 6 | 9 | 8 | 0 | 17 |
| Trợ lý AI (AI) | 9 | 0 | 22 | 7 | 29 |
| **Tổng** | **77** | **126** | **65** | **15** | **206** |

Số mục theo mức: Must 46 · Should 25 · Could 6.

---

## 4. Điểm lệch so với độ ưu tiên trong SRS

| Mục | Ưu tiên SRS | Ưu tiên backlog | Lý do |
|---|---|---|---|
| AI-01, AI-02, AI-03, AI-04, AI-06 (FR-48 – FR-51, FR-54) | Cao | Should | Theo yêu cầu lập backlog: chức năng CRUD và nghiệp vụ lõi là Must, Trợ lý AI là Should/Could. Khi đã làm AI thì AI-02, AI-03, AI-04 là điều kiện bắt buộc đi kèm AI-01. |
| SAL-03 (FR-BAN-03) | TB | Could | Tiện ích thao tác, không ảnh hưởng tính đúng đắn của luồng bán. |
| SAL-04 (FR-BAN-04) | TB | Could | Đơn nháp không giữ chỗ tồn kho (FR-BAN-04), nên đơn nháp tồn đọng không làm sai số liệu. |
| AI-07, AI-08, AI-09 (FR-52, FR-53) | TB | Could | Phần mở rộng của Trợ lý AI. |
| PLT-11, PLT-14 | Không có FR riêng | Should | Logging hỗ trợ NFR-15; cấu hình cửa hàng là dữ liệu đầu vào của hóa đơn (FR-BAN-21). |

---

## 5. Ma trận truy vết FR → mục backlog

| FR | Mục backlog | FR | Mục backlog |
|---|---|---|---|
| FR-01 | PLT-01 | FR-BAN-09 | SAL-08 |
| FR-02 | PLT-02 | FR-BAN-10 | SAL-09 |
| FR-03 | PLT-03, PLT-04 | FR-BAN-11 – 14 | SAL-14 |
| FR-04 | PLT-05 | FR-BAN-15 | SAL-16 |
| FR-05 | PLT-06 | FR-BAN-16 | SAL-10 |
| FR-06 | PLT-07 | FR-BAN-17 | SAL-11 |
| FR-07 | CAT-01 | FR-BAN-18, 19 | SAL-12 |
| FR-08 | CAT-02 | FR-BAN-20 | SAL-13 |
| FR-09 | CAT-03 | FR-BAN-21 | SAL-15, PLT-14 |
| FR-10 | CAT-04 | FR-BAN-22 | SAL-17 |
| FR-11 | PRD-01, PRD-02 | FR-37 | INV-01 |
| FR-12 | PRD-03 | FR-38 | INV-02 |
| FR-13 | PRD-04 | FR-39 | INV-03 |
| FR-14 | PRD-05 | FR-40 | INV-04 |
| FR-15 | PRD-06 | FR-41 | INV-05 |
| FR-16 | SUP-01 | FR-42 | RPT-01 |
| FR-17 | SUP-02 | FR-43 | RPT-02 |
| FR-18 | SUP-04 | FR-44 | RPT-03 |
| FR-19 | SUP-03 | FR-45 | RPT-04 |
| FR-20 | CUS-01 | FR-46 | RPT-05 |
| FR-21 | CUS-02 | FR-47 | RPT-06 |
| FR-22 | CUS-03 | FR-48 | AI-01 |
| FR-23 | CUS-04 | FR-49 | AI-06 |
| FR-24 | PUR-01 | FR-50 | AI-02 |
| FR-25 | PUR-02 | FR-51 | AI-03 |
| FR-26 | PUR-03 | FR-52 | AI-07 |
| FR-27 | PUR-04 | FR-53 | AI-05, AI-08, AI-09 |
| FR-28 | PUR-05 | FR-54 | AI-04 |
| FR-BAN-01 | SAL-01 | | |
| FR-BAN-02 | SAL-02 | | |
| FR-BAN-03 | SAL-03 | | |
| FR-BAN-04 | SAL-04 | | |
| FR-BAN-05 | SAL-05 | | |
| FR-BAN-06, 07 | SAL-06 | | |
| FR-BAN-08 | SAL-07 | | |

Toàn bộ 68 FR của SRS đều có ít nhất một mục backlog. Các NFR có mục riêng: NFR-01, 03, 04, 05, 06, 08, 09, 11, 13, 14, 15, 16, 17, 19, 21, 23, 24. Các NFR còn lại (hiệu năng chung, khả dụng, trình duyệt, độ bao phủ kiểm thử) là tiêu chí kiểm thử chung, kiểm chứng ở giai đoạn kiểm thử thay vì tách thành mục riêng.

---

## 6. Ngoài phạm vi

Các nội dung sau **không** có trong backlog vì SRS mục 2.2 loại trừ:

1. Bán hàng trực tuyến, giỏ hàng online cho khách hàng cuối.
2. Tích hợp cổng thanh toán trực tuyến, xử lý thẻ ngân hàng.
3. Nhiều kho / nhiều chi nhánh.
4. Kế toán tổng hợp, sổ cái, hóa đơn điện tử theo quy định thuế.
5. Quản lý nhân sự, chấm công, tính lương.
6. Ứng dụng di động native.
7. Tích hợp phần cứng ngoài máy in hóa đơn qua trình duyệt (máy quét mã vạch chỉ ở chế độ giả lập bàn phím).
8. Khách hàng thân thiết, tích điểm, khuyến mãi phức tạp.
