# CLAUDE.md

Đồ án **Hệ thống Quản lý Bán hàng & Kho tích hợp AI**: ASP.NET Core Web API + SQL Server + EF Core (code-first), .NET 8 (`global.json` ghim SDK 8.0.404). Blazor chỉ thêm ở Tuần 7. Brief đầy đủ: `HUONG_DAN_CLAUDE_CODE.md`.

## Quy tắc làm việc
- Trả lời bằng tiếng Việt. **Comment trong code bằng tiếng Anh.** Tài liệu `.md` bằng tiếng Việt có dấu.
- Việc đụng nhiều hơn 1 file: trình bày kế hoạch trước, đợi duyệt.
- Không thêm package/tính năng ngoài yêu cầu. Version EF Core cùng major với `<TargetFramework>` (net8.0 → 8.0.x).
- Không ghi secret vào file được commit, không in secret ra chat. Không tự `git commit`/`push`.
- Prompt bài học mâu thuẫn với khế ước dưới đây → làm theo khế ước và nói rõ chỗ lệch.

## Khế ước dự án (bất khả xâm phạm)
| Hạng mục | Quy ước |
|---|---|
| Project | Đúng 4: `SalesInventory.Domain`, `.Application`, `.Infrastructure`, `.API`. `SalesInventory.Web` chỉ tạo ở Tuần 7. Không có project `Contracts`. |
| Tiền tệ | Luôn `decimal`, SQL `decimal(18,2)`. Giá nhập `PurchasePrice`, giá bán `SalePrice`, giá vốn `AverageCost`. **Cấm** `CostPrice`, `Price`, `double`, `float`. |
| Role | Đúng 3 chuỗi, phân biệt hoa/thường: `"Admin"`, `"WarehouseManager"`, `"SalesStaff"`. |
| Ngưỡng tồn thấp | `ReorderLevel` (int, mặc định 0). Cấm `LowStockThreshold`, `MinStockLevel`. |
| Nhật ký kho | Entity duy nhất `StockMovement`: `Id`, `ProductId`, `MovementType` (`StockMovementType { Import, Sale, Adjustment }`), `Quantity` (+ nhập, − xuất), `QuantityAfter`, `ReferenceType`, `ReferenceId?`, `Reason?`, `CreatedAtUtc`. Hủy đơn = dòng `Adjustment` đảo chiều, `Reason` là hằng số Domain (`CancelImport`, `CancelSale`). Không sửa/xóa dòng cũ. |
| Tồn kho | `Products.StockQuantity` là **cache**, `StockMovement` là **nguồn sự thật**. Mỗi biến động cập nhật `StockQuantity` + ghi `QuantityAfter` **trong cùng một transaction** (`docs/database-design.md` mục 4). |
| Phiếu nhập | Chỉ cộng tồn khi `Status = Approved`, không cộng lúc Draft. |
| Mã lỗi HTTP | Vi phạm nghiệp vụ / vượt tồn → **409**; không tìm thấy → **404**; input sai → **400**. |
| Blazor (Tuần 7) | Một named `HttpClient` `"ApiClient"` + typed client `IApiClient`. |
| Tài liệu | `docs/SRS.md`, `docs/UseCases.md`, `docs/database-design.md`, `docs/architecture.md`, `docs/BACKLOG.md`, `docs/PLAN.md`. |

## Kiến trúc và ProjectReference
```
API ──► Application ──► Domain ◄── Infrastructure
 └────────────────────► Infrastructure ──► Application
```
| Project | Tham chiếu | Chứa |
|---|---|---|
| Domain | *(không có, kể cả NuGet)* | Entity (`Domain/Entities`), enum, quy tắc lõi. Không `using Microsoft.EntityFrameworkCore`, không `[JsonIgnore]`/`[ConcurrencyCheck]`. |
| Application | Domain | Interface, DTO, service/use case, validation. |
| Infrastructure | Application, Domain | `Persistence/AppDbContext`, Fluent API, `Persistence/Migrations`, `DependencyInjection.AddInfrastructure()`. Package `EntityFrameworkCore.SqlServer`. |
| API | Application, Infrastructure | Controller, `Program.cs`, `appsettings.json`, Swagger. Package `EntityFrameworkCore.Design`. |

Nợ kỹ thuật đã biết (Services/DTO còn ở API, controller trả entity): `docs/solution-structure.md`.

**Health check:** `GET /api/health` (`HealthController`) trả 200 `{ "status": "ok" }`, không chạm database. Khi thêm xác thực ở Tuần 4, endpoint này **phải giữ `[AllowAnonymous]`** (dùng cho kiểm tra container ở Tuần 10).

## Lệnh
```
dotnet build
dotnet test
dotnet run --project src\SalesInventory.API --launch-profile https

dotnet ef migrations add <Ten> --project src\SalesInventory.Infrastructure --startup-project src\SalesInventory.API --output-dir Persistence\Migrations
dotnet ef database update     --project src\SalesInventory.Infrastructure --startup-project src\SalesInventory.API

dotnet user-secrets set "ConnectionStrings:DefaultConnection" "<chuỗi kết nối>" --project src\SalesInventory.API
```
- Connection string: khóa `ConnectionStrings:DefaultConnection`; `appsettings.json` để rỗng, giá trị thật trong user-secrets (dev) hoặc biến môi trường `ConnectionStrings__DefaultConnection`.
- Trong cmd dùng **một** dấu `\` (`(localdb)\MSSQLLocalDB`); hai dấu chỉ dùng trong file JSON.
- Tắt app đang chạy trước khi `dotnet build` (file `.exe` bị khóa).
