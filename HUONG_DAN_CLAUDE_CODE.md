# BRIEF ĐỒ ÁN & HƯỚNG DẪN CHO CLAUDE CODE

**Đồ án:** Hệ thống Quản lý Bán hàng & Kho tích hợp AI (ASP.NET Core Web API + Blazor + SQL Server + EF Core, toàn bộ C#)
**Nguồn:** tổng hợp từ Bài 1 → Bài 13 của khóa "Đồ án tốt nghiệp .NET với Claude Code" (LearningVN). Cập nhật 2026-11-11.
**Trạng thái khóa học:** xong Tuần 1 và Tuần 2 (12/60 bài), đang ở đầu Tuần 3, Bài 13 (EF Core + DbContext).

---

## 0. Cách dùng file này

1. Chép file vào dự án: `copy HUONG_DAN_CLAUDE_CODE.md docs\` (hoặc đặt ở thư mục gốc repo).
2. Mở cmd tại thư mục gốc repo, chạy `claude`.
3. Nhấn **Shift+Tab** để vào Plan mode, dán **Prompt A** (mục 8). Đọc báo cáo, sửa nếu cần.
4. Thoát Plan mode, dán **Prompt B** (đồng bộ nền tảng), rồi **Prompt C** (Bài 13), rồi **Prompt D** (kiểm chứng).
5. Với các bài sau, dùng **Prompt E** (khung bọc prompt của bài).
6. Đưa mục 1, 2, 3 vào `CLAUDE.md` (Prompt B làm việc này) để Claude tự đọc mỗi phiên.

### Quy tắc cho Claude Code khi đọc file này
- Mục 1, 2, 3 và 5 là **luật của dự án**. Nếu prompt của một bài học mâu thuẫn với chúng, làm theo file này và nói rõ chỗ lệch.
- Trả lời bằng tiếng Việt. Comment trong code bằng **tiếng Anh**. Tài liệu `.md` bằng tiếng Việt có dấu.
- Việc đụng nhiều hơn 1 file: trình bày kế hoạch trước, đợi duyệt rồi mới sửa.
- Không thêm package hoặc tính năng ngoài yêu cầu của prompt.
- Không ghi secret (connection string thật, `ANTHROPIC_API_KEY`) vào file được commit. Không in secret ra chat.
- Sau mỗi thay đổi: chạy `dotnet build`, rồi tóm tắt file nào đổi và vai trò từng thay đổi.
- Không tự `git commit` / `git push` nếu người dùng chưa yêu cầu.
- Không hard-code ID model AI; chỉ gọi theo tầng (Opus/Sonnet/Haiku) và đổi bằng `/model`.

---

## 1. Bối cảnh sản phẩm

- **Module:** sản phẩm, danh mục, nhà cung cấp, khách hàng, đơn nhập, đơn bán, tồn kho, báo cáo, trợ lý AI bán hàng (chatbot, Tuần 9).
- **3 vai trò người dùng:** Admin, Quản lý kho, Nhân viên bán hàng. Trợ lý AI là một tính năng, không phải vai trò.
- **Stack:** ASP.NET Core Web API, Blazor (Tuần 7), SQL Server, EF Core code-first.
- **Mục tiêu bảo vệ:** người học phải giải thích được từng dòng code trước hội đồng. "AI viết hộ" không phải câu trả lời.

---

## 2. KHẾ ƯỚC DỰ ÁN (bất khả xâm phạm, mọi bài dùng y hệt)

Khóa học yêu cầu dán khối này vào `CLAUDE.md` và vào phần Ràng buộc của mọi prompt tạo entity/endpoint.

| Hạng mục | Quy ước bắt buộc |
|---|---|
| Tên 4 project | `SalesInventory.Domain`, `SalesInventory.Application`, `SalesInventory.Infrastructure`, `SalesInventory.API`. Project Blazor `SalesInventory.Web` chỉ thêm ở Tuần 7, **chưa tạo trước**. |
| Tiền tệ | Luôn `decimal`, cột SQL `decimal(18,2)`. Giá nhập = `PurchasePrice`, giá bán = `SalePrice`. **Cấm** `CostPrice`, `Price`, `double`, `float`. |
| Vai trò (role) | Đúng 3 chuỗi, phân biệt hoa/thường: `"Admin"`, `"WarehouseManager"`, `"SalesStaff"`. **Cấm** Kho/BanHang/Warehouse/Sales. |
| Ngưỡng tồn thấp | Cột `ReorderLevel` (int, mặc định 0). **Cấm** tạo thêm `LowStockThreshold`. |
| Nhật ký kho | Entity **duy nhất** `StockMovement`: `Id` (int), `ProductId` (int, FK), `MovementType` (enum `StockMovementType { Import, Sale, Adjustment }`), `Quantity` (int, **dương khi nhập, âm khi xuất**), `QuantityAfter` (int, tồn ngay sau biến động), `ReferenceType` (string, ví dụ "PurchaseOrder"/"SalesOrder"), `ReferenceId` (int, nullable), `Reason` (string, nullable), `CreatedAtUtc` (DateTime, UTC). Không định nghĩa lại kiểu khác. |
| Tồn kho phiếu nhập | Chỉ cộng tồn khi phiếu được **duyệt** (`Status = Approved`). **Không** cộng lúc tạo phiếu (Draft). |
| Mã lỗi HTTP | Bán/xuất vượt tồn hoặc vi phạm nghiệp vụ → **409**. Không tìm thấy → **404**. Input sai → **400**. Nhất quán toàn hệ thống. |
| Blazor gọi API | Đúng **một** named `HttpClient` tên `"ApiClient"` + một typed client `IApiClient`; trang nào cũng `@inject IApiClient` (thiết lập Tuần 7, Bài 38). |
| Tên file tài liệu | `docs/SRS.md`, `docs/UseCases.md`, `docs/database-design.md`, `docs/architecture.md`, `docs/BACKLOG.md`, `docs/PLAN.md` (phân biệt hoa/thường). |

---

## 3. Kiến trúc và nơi đặt code

Quy tắc phụ thuộc: mọi mũi tên hướng vào **Domain**. Domain không tham chiếu project nào.

```
API ──► Application ──► Domain ◄── Infrastructure
 └────────────────────► Infrastructure ──► Application
```

| Project | ProjectReference tới | Chứa gì |
|---|---|---|
| `SalesInventory.Domain` | (không có) | Entity (`Category`, `Product`, `StockMovement`…), enum, quy tắc nghiệp vụ lõi. **Không** `using Microsoft.EntityFrameworkCore`. |
| `SalesInventory.Application` | Domain | Interface (`IProductRepository`, `IProductService`…), DTO, service/use case, validation. |
| `SalesInventory.Infrastructure` | Application, Domain | `AppDbContext`, cấu hình Fluent API, Repository, Migrations, seed, đăng ký DI cho tầng dữ liệu. |
| `SalesInventory.API` | Application, Infrastructure | Controller, `Program.cs`, `appsettings.json`, Swagger, xác thực. |

Bố cục thư mục (Bài 11): file `SalesInventory.sln` ở gốc, 4 project nằm trong `src/`, tài liệu trong `docs/`.

**Việc EF Core thuộc project nào:**
- `Microsoft.EntityFrameworkCore.SqlServer`: Infrastructure.
- `Microsoft.EntityFrameworkCore.Design`: project khởi động là API (bắt buộc cho `dotnet ef`). Nếu tool báo thiếu thì thêm cả vào Infrastructure.
- Version package EF Core phải **cùng major** với `<TargetFramework>` (ví dụ `net10.0` thì EF Core 10.x). Kiểm bằng `dotnet --version` và file `.csproj` trước khi `dotnet add package`.
- `AppDbContext` nằm ở Infrastructure vì Domain phải độc lập công nghệ lưu trữ (đổi SQL Server sang PostgreSQL thì Domain không đổi).
- Connection string: khóa `ConnectionStrings:DefaultConnection`. `appsettings.json` để trống/placeholder, giá trị thật nằm trong `dotnet user-secrets` của project API.
- Đăng ký DI gọn: `services.AddInfrastructure(configuration)` trong `Infrastructure/DependencyInjection.cs`, `Program.cs` chỉ gọi một dòng. (Bài 13 viết `AddDbContext` thẳng trong `Program.cs`; cách nào cũng đạt tiêu chí, cách này sạch hơn cho Bài 16.)

---

## 4. Kiến thức cốt lõi rút ra từ Bài 1 → 13

### 4.1 Cách làm việc với Claude Code (Bài 1, 3, 6)
- **Vibe coding** = ra lệnh bằng prompt, nhưng vẫn phải đọc, kiểm chứng và hiểu.
- **Công thức prompt M-N-R-T:** Mục tiêu (muốn gì, một câu) + Ngữ cảnh (dự án, công nghệ, file liên quan) + Ràng buộc (giới hạn, quy ước, không được đụng gì) + Tiêu chí chấp nhận (cách biết đã xong và đúng).
- **Plan mode:** Shift+Tab. Claude chỉ đọc và lập kế hoạch, không sửa file. Dùng cho việc lớn, đụng nhiều file, hoặc phân tích/thiết kế.
- **Đọc diff trước khi chấp nhận:** đúng file chưa? có xóa nhầm dòng quan trọng không? có package lạ không? có hard-code secret không?
- **Quy trình kiểm chứng 4 bước:** (1) đọc diff, (2) `dotnet build`, (3) `dotnet run` + mở Swagger thử endpoint, (4) đối chiếu dữ liệu thật trong SQL Server (SSMS hoặc Azure Data Studio). "Build xanh" chỉ chứng minh biên dịch được, không chứng minh nghiệp vụ đúng.
- **Lệnh trong Claude Code:** `/init` (tạo `CLAUDE.md`), `/model` (đổi tầng model), `/login`, `/clear` (khi đổi sang việc khác hẳn để bớt nhiễu ngữ cảnh). `claude doctor` để chẩn đoán cài đặt.
- **`CLAUDE.md`** ở gốc repo là "ràng buộc bền vững": Claude đọc mỗi phiên, nên đặt khế ước, lệnh build/run, quy ước comment tiếng Anh vào đó.

### 4.2 Môi trường, Git, bảo mật (Bài 2, 4, 5)
- Môi trường: .NET SDK (nên bản LTS), SQL Server (LocalDB/Express/Developer/Docker), SSMS hoặc Azure Data Studio, Git, Claude Code. Kiểm: `dotnet --version`, `dotnet --list-sdks`, `claude --version`.
- Swagger ở `https://localhost:PORT/swagger` (PORT đọc từ log `dotnet run`, đừng đoán). Báo chứng chỉ không tin cậy: `dotnet dev-certs https --trust`.
- Git: nhánh `main`, `.gitignore` chuẩn .NET (bỏ `bin/`, `obj/`, `.user`, `.env`, `appsettings.*.Local.json`), commit có thông điệp rõ, push lên GitHub. Kiểm bằng `git status`, `git log --oneline`, `git remote -v`.
- **Secret** (`ANTHROPIC_API_KEY`, connection string thật): để trong `dotnet user-secrets` hoặc `.env` đã `.gitignore`. Không dán vào chat, log, ảnh chụp. Lỡ push lên Git thì coi như đã lộ và tạo key mới ngay.

### 4.3 Chuỗi tài liệu Tuần 2 (Bài 7 → 12), mỗi bước làm đầu vào cho bước sau
| Bài | Sản phẩm | Ghi chú |
|---|---|---|
| 7 | `docs/SRS.md` | FR có mã (FR-01…), yêu cầu phi chức năng, phạm vi trong/ngoài. SRS nói **làm gì**, chưa nói **làm thế nào**. |
| 8 | `docs/UseCases.md` | 3 actor, ≥12 use case, user story INVEST, tiêu chí Given/When/Then, sơ đồ Mermaid. |
| 9 | `docs/database-design.md` | Bảng, khóa chính/ngoại, ERD Mermaid. N:M tách bằng bảng trung gian. Tồn kho suy ra từ nhật ký, có thể cache cột `StockQuantity`. |
| 10 | `docs/architecture.md`, `docs/adr/0001-kien-truc-du-an.md` | Chọn 4 lớp, tránh over-engineering. |
| 11 | solution 4 project + `docs/solution-structure.md` | ProjectReference đúng chiều, không vòng. Phải **đọc `.csproj`**, vì build xanh không chứng minh đúng chiều. |
| 12 | `docs/BACKLOG.md`, `docs/PLAN.md` | MoSCoW, story point, ~8 tuần, tuần cuối dành cho kiểm thử và bảo vệ. CRUD lõi là Must, AI là Should/Could. |

---

## 5. Chỗ các bài học mâu thuẫn nhau và cách xử lý (làm theo cột phải)

| # | Bài học nói | Làm theo |
|---|---|---|
| 1 | Bài 4 tạo **một** project `SalesInventory.Api`. Bài 11 và Khế ước dùng **4** project, API viết hoa `SalesInventory.API`. | Dùng 4 project. Nếu repo còn project `SalesInventory.Api` cũ, hợp nhất/đổi tên về `SalesInventory.API`, không giữ hai API project. Cẩn thận đổi hoa/thường trên Windows (`git mv` hai bước). |
| 2 | Bài 13 bảo tạo thư mục `Data/` và `Models/` "trong dự án Web API", namespace `SalesInventory.Api.*`. | Bỏ cách đó. Entity vào **Domain**, `AppDbContext` vào **Infrastructure**, namespace theo tên project mới (mục 3). |
| 3 | Code mẫu Bài 13 và Bài 3 dùng `Product.Price`. | Dùng `PurchasePrice` và `SalePrice`, cả hai `decimal(18,2)`. Không có `Price`. |
| 4 | Bài 9 đặt tên bảng nhật ký là `InventoryTransactions`. | Dùng entity duy nhất `StockMovement` theo Khế ước. Cập nhật lại `docs/database-design.md` nếu đang ghi tên cũ. |
| 5 | Bài 9 trỏ tới `docs/use-cases.md`, nhưng Bài 8 tạo `docs/UseCases.md`. | Dùng `docs/UseCases.md`. |
| 6 | Bài 7 liệt kê actor "Nhân viên bán hàng, Thủ kho, Quản lý, Trợ lý AI". | Chỉ 3 role chuẩn của Khế ước. Trợ lý AI là tính năng. |
| 7 | Bài 13 gõ connection string với `\\` (`(localdb)\\MSSQLLocalDB`). | Trong **cmd** dùng **một** dấu `\`: `(localdb)\MSSQLLocalDB`. Hai dấu chỉ đúng khi viết trong file JSON. |
| 8 | Bài 13 chạy `dotnet ef ...` và `dotnet user-secrets ...` như thể chỉ có một project. | Với nhiều project phải thêm `--project src\SalesInventory.Infrastructure --startup-project src\SalesInventory.API` (ef) và `--project src\SalesInventory.API` (user-secrets). |
| 9 | Đoạn "Bài kế tiếp" ở cuối vài bài nói sai nội dung bài sau (ví dụ Bài 4 nói Bài 5 là EF Core). | Bỏ qua. Nội dung thật theo mục lục khóa học. |
| 10 | Bài 13 đặt hai entity tối thiểu `Category` và `Product(Id, Name, Price, StockQuantity, CategoryId)`. | Giữ phạm vi tối thiểu (2 entity) nhưng sửa cột theo Khế ước (xem Prompt C). Mô hình đầy đủ để dành cho Bài 14. |

> Điểm 10 giữ `StockQuantity` như tồn hiện tại (cache) trên `Product`. Đây là lựa chọn của file này, dựa trên ghi chú ở Bài 9 ("có thể cache một cột StockQuantity"). Các bài sau trong khóa có thể chốt khác, nên đối chiếu khi tới Bài 14 và Tuần 6. Nếu giữ, mỗi biến động kho phải cập nhật `StockQuantity` và ghi `StockMovement.QuantityAfter` **trong cùng một transaction**.

---

## 6. Bước 0: hiện trạng cần xác minh trước khi code tiếp

Claude Code kiểm tra và báo cáo (chỉ đọc):

1. `dotnet --version`, `dotnet --list-sdks`, `<TargetFramework>` của từng `.csproj`.
2. Cây thư mục solution và **ProjectReference thực tế** của 4 project. Có đúng chiều mục 3 không? Có còn project `SalesInventory.Api` cũ không?
3. `Program.cs`: có Swagger, `HealthController` (`GET /api/health` → `{ "status": "ok" }`) không?
4. Package đang có ở từng project (`dotnet list package`).
5. Git: `git status`, `git log --oneline`, `git remote -v`. `.gitignore` có chặn `bin/`, `obj/`, secret không?
6. `docs/` có đủ các file ở mục 2 với đúng tên không? `docs/database-design.md` có chỗ nào lệch Khế ước (mục 5, dòng 3 và 4) không?
7. `CLAUDE.md` có tồn tại và có chứa Khế ước không?
8. SQL Server: LocalDB (`sqllocaldb info`) hoặc instance đang dùng có chạy không? `dotnet tool list -g` có `dotnet-ef` không?

---

## 7. Lộ trình

### 7.1 Tuần 3: Nền tảng dữ liệu với EF Core & SQL Server
> Bài 13 đọc kỹ ở trên. Bài 14–18 chỉ dựa vào **tên bài** trong mục lục, chưa đọc nội dung; hướng đi dưới đây là đề xuất, đối chiếu lại khi học tới.

| Bài | Nội dung | Hướng đi |
|---|---|---|
| 13 | EF Core + DbContext + SQL Server | `Category` + `Product` tối thiểu, `AppDbContext` ở Infrastructure, connection string qua user-secrets, DI, migration `InitialCreate`, `database update`. |
| 14 | Models và quan hệ | Đủ entity theo `docs/database-design.md` đã sửa theo Khế ước: Category, Product, Supplier, Customer, PurchaseOrder(+Item), SalesOrder(+Item), StockMovement. Cấu hình Fluent API, `decimal(18,2)`, index/unique. |
| 15 | Migrations và seed | Migration có tên rõ, seed dữ liệu mẫu chạy lặp lại không nhân đôi. |
| 16 | Repository và Service | Interface ở Application, hiện thực ở Infrastructure, đăng ký DI. |
| 17 | CRUD Danh mục và Sản phẩm | Controller + DTO + validation, lỗi 400/404/409 đúng Khế ước. |
| 18 | Kiểm chứng Swagger + SQL Server | Thử từng endpoint, đối chiếu bảng trong SQL Server. |

### 7.2 Các tuần sau (chỉ biết tên tuần)
- Tuần 4: Xác thực & phân quyền (3 role chuẩn ở mục 2).
- Tuần 5: Nghiệp vụ nhập kho & danh mục (cộng tồn khi `Approved`).
- Tuần 6: Nghiệp vụ bán hàng & tồn kho (409 khi vượt tồn, ghi `StockMovement`).
- Tuần 7: Giao diện Blazor (tạo `SalesInventory.Web`, `ApiClient`).
- Tuần 8: Tính năng nâng cao & báo cáo.
- Tuần 9: Trợ lý AI chatbot hỗ trợ bán hàng.
- Tuần 10: Kiểm thử, triển khai, bảo vệ đồ án.

---

## 8. PROMPT SẴN DÙNG (dán vào Claude Code trong cmd)

### Prompt A: Khảo sát hiện trạng (bật Plan mode trước bằng Shift+Tab)

```
Đọc @HUONG_DAN_CLAUDE_CODE.md (mục 1, 2, 3, 5, 6). Bạn đang ở Plan mode: chỉ ĐỌC và BÁO CÁO. KHÔNG sửa file, KHÔNG cài package, KHÔNG chạy migration. Chỉ được chạy lệnh chỉ-đọc (dotnet --version, dotnet --list-sdks, dotnet list package, git status, git log, dir).

MỤC TIÊU: khảo sát hiện trạng repo và đối chiếu với Khế ước + kiến trúc trong file brief.
NGỮ CẢNH: đồ án Hệ thống Quản lý Bán hàng & Kho, đã xong Bài 1-12, chuẩn bị làm Bài 13 (EF Core + DbContext).
RÀNG BUỘC: làm đủ 8 mục ở mục 6 của brief; không đề xuất code ở bước này.
TIÊU CHÍ CHẤP NHẬN: báo cáo gồm (1) cây thư mục và ProjectReference thực tế, (2) bảng ĐÚNG / LỆCH so với Khế ước và kiến trúc, (3) danh sách việc cần sửa theo thứ tự ưu tiên, (4) rủi ro nếu bỏ qua từng việc. Nêu rõ cái nào bạn không kiểm tra được.
```

### Prompt B: Đồng bộ nền tảng (chỉ chạy sau khi đã duyệt báo cáo A)

```
Dựa trên báo cáo vừa rồi và @HUONG_DAN_CLAUDE_CODE.md, hãy sửa CHỈ những chỗ lệch tôi đã duyệt. Trước khi sửa, trình bày kế hoạch từng bước để tôi duyệt.

MỤC TIÊU: đưa repo về đúng Khế ước và kiến trúc 4 project, chuẩn bị cho Bài 13.
RÀNG BUỘC:
- Cập nhật (hoặc tạo) CLAUDE.md ở gốc repo, ngắn gọn, gồm: mô tả dự án, Khế ước (mục 2), kiến trúc và ProjectReference (mục 3), lệnh build/run, quy ước "comment code bằng tiếng Anh".
- Đổi tên file docs sai tên về đúng tên ở mục 2. Cập nhật docs/database-design.md nếu còn Price/CostPrice/InventoryTransactions/LowStockThreshold so với Khế ước.
- Nếu còn project SalesInventory.Api cũ thì hợp nhất về SalesInventory.API, không để hai API project.
- Không thêm tính năng, không thêm package ngoài việc đồng bộ này.
TIÊU CHÍ CHẤP NHẬN: dotnet build thành công; ProjectReference đúng chiều mục 3; Domain không có ProjectReference; docs đúng tên; CLAUDE.md có Khế ước; git status không chứa secret. Tóm tắt từng file đã đổi và lý do.
```

### Prompt C: Bài 13 (EF Core + DbContext + SQL Server), bật Plan mode trước, duyệt kế hoạch rồi mới thực thi

```
Làm Bài 13 cho dự án theo @HUONG_DAN_CLAUDE_CODE.md. Nếu prompt/ví dụ của khóa học mâu thuẫn với Khế ước hoặc kiến trúc, làm theo Khế ước và nói rõ chỗ lệch. Trước khi sửa file, trình bày kế hoạch (file nào tạo/sửa, lệnh nào chạy) để tôi duyệt.

MỤC TIÊU: tích hợp Entity Framework Core với SQL Server, tạo AppDbContext, và tạo database bằng migration đầu tiên.

NGỮ CẢNH: solution 4 project (Domain, Application, Infrastructure, API) đã dựng và build được. Chỉ làm phạm vi tối thiểu của Bài 13; mô hình đầy đủ để dành cho Bài 14.

YÊU CẦU:
1. Kiểm <TargetFramework>, rồi cài package EF Core CÙNG major version với nó:
   - Microsoft.EntityFrameworkCore.SqlServer vào SalesInventory.Infrastructure.
   - Microsoft.EntityFrameworkCore.Design vào SalesInventory.API (project khởi động). Chỉ thêm vào Infrastructure nếu dotnet ef báo thiếu.
2. Domain (không dùng EF Core): entity Category (Id, Name, Description) và Product (Id, Name, PurchasePrice, SalePrice, StockQuantity, ReorderLevel mặc định 0, CategoryId, navigation tới Category). Category có ICollection<Product>.
3. Infrastructure: AppDbContext kế thừa DbContext, constructor nhận DbContextOptions<AppDbContext>, DbSet<Category> và DbSet<Product>. Cấu hình quan hệ 1-N Category-Product bằng Fluent API (hoặc IEntityTypeConfiguration). PurchasePrice và SalePrice dùng decimal(18,2). Tên Name bắt buộc, có độ dài tối đa hợp lý.
4. Đăng ký DI: extension AddInfrastructure(IConfiguration) trong Infrastructure/DependencyInjection.cs gọi AddDbContext + UseSqlServer với khóa ConnectionStrings:DefaultConnection; Program.cs của API gọi một dòng.
5. Connection string: appsettings.json chỉ để placeholder/rỗng. Giá trị thật đặt bằng dotnet user-secrets cho project API. Trên cmd dùng MỘT dấu "\" (ví dụ Server=(localdb)\MSSQLLocalDB;Database=SalesInventoryDb;Trusted_Connection=True;TrustServerCertificate=True). Chỉ cho tôi lệnh để TÔI tự chạy phần đặt secret; không ghi giá trị thật vào file nào bị commit.
6. Migration (nhớ --project và --startup-project vì có nhiều project):
   dotnet ef migrations add InitialCreate --project src\SalesInventory.Infrastructure --startup-project src\SalesInventory.API
   dotnet ef database update --project src\SalesInventory.Infrastructure --startup-project src\SalesInventory.API
   Nếu chưa có dotnet-ef thì hướng dẫn tôi cài (dotnet tool install --global dotnet-ef).
RÀNG BUỘC: tuân thủ Khế ước (không Price/CostPrice/double/float); comment code bằng tiếng Anh; không thêm package hay entity ngoài yêu cầu; không commit secret.

TIÊU CHÍ CHẤP NHẬN:
- dotnet build PASS, 0 error; Domain không tham chiếu EF Core hay project nào.
- Migration InitialCreate được tạo trong Infrastructure; database update chạy thành công.
- Trong SQL Server database SalesInventoryDb có bảng Categories, Products, __EFMigrationsHistory; cột giá là decimal(18,2).
- dotnet run khởi động được, mở /swagger vẫn thấy GET /api/health.
Sau cùng giải thích ngắn gọn vai trò từng file bạn tạo/sửa.
```

### Prompt D: Kiểm chứng (Claude chạy và báo cáo; người học tự soi SQL Server bằng SSMS/Azure Data Studio)

```
Kiểm chứng Bài 13 và báo cáo kết quả, chưa sửa gì. Chạy: dotnet build; dotnet list package (từng project); đọc từng .csproj để xác nhận ProjectReference; xác nhận appsettings.json không chứa mật khẩu; git status để chắc không có secret trong thay đổi chờ commit. Cho tôi bảng PASS/FAIL cho từng tiêu chí chấp nhận của Prompt C, kèm bằng chứng (đoạn output ngắn). Với FAIL, nêu nguyên nhân gốc và đề xuất sửa, đừng tự sửa.
```

### Prompt E: Khung bọc cho mọi bài sau (dán prompt của bài vào chỗ `<<< >>>`)

```
Đọc @HUONG_DAN_CLAUDE_CODE.md và CLAUDE.md. Trước khi làm, đối chiếu prompt dưới đây với Khế ước (mục 2) và kiến trúc (mục 3). Nếu có chỗ lệch (tên cột, tên role, mã lỗi HTTP, vị trí đặt code), làm theo Khế ước/kiến trúc và liệt kê chỗ lệch ở đầu câu trả lời. Việc đụng nhiều file: trình bày kế hoạch trước, đợi tôi duyệt. Sau khi xong: dotnet build, tóm tắt file đã đổi, và nói rõ tôi cần tự kiểm chứng gì (Swagger, SQL Server).

<<< dán prompt của bài học vào đây >>>
```

---

## 9. Checklist tự kiểm và câu hỏi hội đồng (Bài 13)

**Tự kiểm sau Bài 13**
- [ ] `dotnet build` xanh, 0 error.
- [ ] `Domain.csproj` không có `ProjectReference`; không có `using Microsoft.EntityFrameworkCore` trong Domain.
- [ ] `appsettings.json` không có mật khẩu; secret nằm trong user-secrets; `git status` sạch secret.
- [ ] Có migration `InitialCreate` trong Infrastructure; `database update` chạy được.
- [ ] SQL Server có `Categories`, `Products`, `__EFMigrationsHistory`; cột tiền là `decimal(18,2)`.
- [ ] Swagger vẫn lên và `GET /api/health` trả 200.

**Câu hỏi hội đồng nên trả lời được**
- EF Core là gì, `DbContext` là gì, `DbSet` ánh xạ sang cái gì? (ORM; phiên làm việc tới database; mỗi `DbSet` ứng với một bảng.)
- Vì sao `AppDbContext` đặt ở Infrastructure chứ không phải Domain? (Domain độc lập công nghệ lưu trữ.)
- `migrations add` khác `database update` thế nào? (add chỉ sinh code mô tả thay đổi; update mới tạo/sửa bảng thật.)
- `__EFMigrationsHistory` để làm gì? (EF nhớ đã áp migration nào.)
- Vì sao không để mật khẩu trong `appsettings.json`? (Commit lên Git là lộ vĩnh viễn trong lịch sử; dùng user-secrets khi dev, biến môi trường khi deploy.)
- Vì sao tiền dùng `decimal(18,2)` mà không dùng `double`? (Tránh sai số làm tròn nhị phân.)
