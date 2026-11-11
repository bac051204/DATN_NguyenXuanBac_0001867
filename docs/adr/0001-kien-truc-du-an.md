# ADR 0001: Kiến trúc phân lớp cho hệ thống Quản lý Bán hàng & Kho

| Thông tin | Giá trị |
|---|---|
| Trạng thái | Đã chấp nhận |
| Ngày | 28/10/2026 |
| Tài liệu liên quan | [architecture.md](../architecture.md), [SRS.md](../SRS.md) |

## Bối cảnh

- Đồ án tốt nghiệp do 1–2 sinh viên thực hiện trong thời gian có hạn, dùng ASP.NET Core Web API, Blazor, EF Core và SQL Server.
- Gồm 8 module nghiệp vụ (danh mục, sản phẩm, nhà cung cấp, khách hàng, nhập hàng, bán hàng, tồn kho, báo cáo) và một trợ lý AI bán hàng gọi dịch vụ LLM bên ngoài.
- Phần lớn module là CRUD. Nghiệp vụ quan trọng tập trung ở bán hàng, nhập hàng và tồn kho: trừ/cộng tồn trong cùng một giao dịch, hủy đơn hoàn kho, tính tổng tiền.
- Yêu cầu: kiểm thử được lớp nghiệp vụ, dễ bảo trì, dễ giải thích trước hội đồng, tránh thiết kế thừa.
- Mã ban đầu là một project duy nhất (`Controllers → Services → AppDbContext`). Service đã được kiểm thử trên SQLite in-memory.

## Quyết định

Áp dụng **kiến trúc phân lớp theo hướng Clean Architecture, ở mức giản lược**:

- 4 lớp, mỗi lớp là một project riêng: **Domain**, **Application**, **Infrastructure**, **API** (`SalesInventory.API`). DTO nằm ở Application; **không** có project `Contracts`. Project Blazor (**Web**) chỉ thêm ở Tuần 7; nếu khi đó Blazor bị lặp model thì xem xét lại.
- Phụ thuộc chỉ hướng vào trong: Api/Infrastructure → Application → Domain. **Domain không tham chiếu project hay gói hạ tầng nào.**
- Application khai báo interface cho những thứ bên ngoài (`IAppDbContext`, `ILlmClient`, `ICurrentUser`), Infrastructure cài đặt các interface này.
- `AppDbContext` nằm ở Infrastructure. **Không dùng Repository chung** (`IRepository<T>`), vì `DbSet<T>` và `DbContext` đã đóng vai trò Repository và Unit of Work.
- Không dùng MediatR, CQRS hay Domain Event.

Chi tiết cấu trúc và chiều phụ thuộc xem [architecture.md](../architecture.md).

## Các phương án đã cân nhắc

| Phương án | Ưu điểm | Nhược điểm | Kết luận |
|---|---|---|---|
| **A. Layered cổ điển** (Presentation → Business → Data) | Đơn giản, gần với mã hiện có, hội đồng quen thuộc | Nghiệp vụ phụ thuộc trực tiếp vào tầng dữ liệu; quy tắc cốt lõi khó kiểm thử độc lập với CSDL | Không chọn |
| **B. Clean Architecture đầy đủ** (Repository cho mọi thực thể, MediatR, CQRS) | Tách biệt triệt để, dễ thay hạ tầng | Nhiều code thủ tục cho ~20 bảng chủ yếu là CRUD; Repository bọc EF Core là trừu tượng thừa; tốn thời gian, khó bảo vệ lý do dùng | Không chọn |
| **C. Clean giản lược** (phương án được chọn) | Domain độc lập và kiểm thử thuần được; chỉ thêm interface ở ranh giới thật sự cần (CSDL, LLM); chi phí chuyển đổi từ mã hiện có thấp | Application vẫn tham chiếu gói EF Core cơ bản | **Chọn** |

## Hệ quả

**Tốt:**

- Quy tắc nghiệp vụ cốt lõi (`Product.DecreaseStock`, `SalesOrder.Cancel`) được kiểm thử bằng unit test thuần, không cần CSDL.
- Service ở Application được kiểm thử trên SQLite in-memory, nên kiểm tra được cả truy vấn EF lẫn nghiệp vụ. Trợ lý AI được kiểm thử bằng `ILlmClient` giả, không gọi LLM thật.
- Đổi nhà cung cấp LLM hoặc chỉnh cấu hình CSDL chỉ ảnh hưởng đến Infrastructure.
- Trình biên dịch ép chiều phụ thuộc thông qua tham chiếu giữa các project. Viết sai chiều sẽ không build được.

**Đánh đổi:**

- Application phụ thuộc gói `Microsoft.EntityFrameworkCore` (để dùng `DbSet`, `Include`, `ToListAsync`). Đây là đánh đổi có chủ đích: Application không phụ thuộc SQL Server, nhưng đổi sang ORM khác sẽ phải sửa Application.
- Số project tăng từ 1 lên 6, phải di chuyển mã và Migrations sang project mới.
- Thực thể phải bỏ các thuộc tính như `[JsonIgnore]`, `[Required]`. Ràng buộc chuyển sang cấu hình Fluent API, nên có thêm một ít code cấu hình.
- Nếu sau này báo cáo cần SQL tối ưu, sẽ thêm interface truy vấn chuyên biệt (`IReportQueries`) thay vì Repository chung.
