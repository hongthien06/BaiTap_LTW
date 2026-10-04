# Evidence — Bước 9 (Independent Verification)

Ngày chạy: **2026-10-04**. Mọi dòng dưới đây đều là **raw output**, không phải mô tả.

## 1. Automated Gates (bước 6)

`bash scripts/gates.sh` — 7/7 cổng xanh:

```
=== [1/7] dotnet build ===      Build succeeded. 0 Warning(s) 0 Error(s)   (-warnaserror)
=== [2/7] dotnet format ===     --verify-no-changes: không có thay đổi
=== [3/7] dotnet test ===       Passed! Failed: 0, Passed: 92, Skipped: 0, Total: 92
=== [4/7] frontend typecheck == tsc -b --noEmit: không lỗi
=== [5/7] frontend lint ===     oxlint: 0 error, 3 warning (không chặn)
=== [6/7] frontend test ===     Test Files 4 passed (4) | Tests 18 passed (18)
=== [7/7] frontend build ===    ✓ built in 10.11s — dist/index.html + css 19.16 kB + js 1,285.24 kB
=== GATES PASSED ===
```

**Tổng 110 test tự động, 0 fail, 0 skip.**

## 2. Kiểm chứng runtime qua HTTP (backend trực tiếp, cổng 5080)

| AC | Kiểm chứng | Raw output | Pass |
|---|---|---|---|
| AC-1 | `GET /api/public/landing` | `HTTP 200` + đủ `book`, `author`, `pressQuotes`, `review`, `ratingSummary`, `settings` | ✅ |
| AC-7 | `POST /api/orders` hợp lệ | `HTTP 201` `{"orderCode":"NGK-20261004-8AMZ",...}` | ✅ |
| AC-8 | SĐT `901234567` | `HTTP 400` + `errors.Phone` | ✅ |
| AC-10 | Giá server tính | Sách 89.000 → giảm 69.000; đặt 2 cuốn → `unitPrice 69000.00`, `totalPrice 138000.00` | ✅ |
| AC-11 | Rate limit | Request thứ 6/phút → `HTTP 429` | ✅ |
| AC-13 | `POST /api/feedbacks` | `HTTP 201` + "sẽ hiển thị sau khi được duyệt" | ✅ |
| AC-14 | Feedback chưa duyệt | `ratingSummary {average: 0, count: 0}`, `feedbacks []` | ✅ |
| AC-15 | `rating = 6` | `HTTP 400` | ✅ |
| AC-17 | Login đúng | `HTTP 200`, JWT 604 ký tự | ✅ |
| AC-18 | Sai mật khẩu / email lạ | **Cả hai** `HTTP 401` + cùng message "Email hoac mat khau khong dung." | ✅ |
| AC-19 | Không token | `HTTP 401` | ✅ |
| AC-19 | Token sửa chữ ký | `HTTP 401` | ✅ |
| AC-20 | Staff `PUT /api/admin/book` → `403`; Staff `GET /api/admin/orders` → `200` | | ✅ |
| AC-24 | Phân trang | `{"items":[...],"totalCount":1,"page":1,"pageSize":10,"totalPages":1}` | ✅ |
| AC-26 | `New → Confirmed` | `HTTP 200` | ✅ |
| AC-27 | `Confirmed → New` và `Confirmed → Completed` | Cả hai `HTTP 400` | ✅ |
| AC-28 | Duyệt feedback | Sau approve: `ratingSummary {average: 5, count: 1}` | ✅ |
| AC-29 | Upload `MZ…` đặt tên `.jpg` | `HTTP 400` | ✅ |
| AC-22 | Seed admin | `PasswordHash` dạng BCrypt (`$2…`), mật khẩu lấy từ user-secrets | ✅ |

## 3. Kiểm chứng end-to-end qua Vite proxy (frontend 5173 → API 5080 → SQL Server)

```
### Vite phục vụ trang HTML
HTTP 200
    <title>Nhà Giả Kim - Đặt mua sách</title>
    <div id="root"></div>

### Proxy /api/public/landing qua Vite
HTTP 200

### Đặt hàng qua proxy
{"orderCode":"NGK-20261004-UC9A","unitPrice":69000.00,"totalPrice":207000.00}
HTTP 201

### Đơn vừa tạo trong SQL Server
OrderCode          CustomerName     Quantity UnitPrice TotalPrice Status
------------------ ---------------- -------- --------- ---------- ------
NGK-20261004-UC9A  Khach Qua Proxy  3        69000.00  207000.00  0
```

Dữ liệu khớp: 3 × 69.000 = 207.000, trạng thái `0` = `New`. Giá do **server** tính, client không gửi giá.

## 4. Database thật

```
sqlcmd -S "(localdb)\MSSQLLocalDB" -d NhaGiaKim -Q "SELECT name FROM sys.tables"

__EFMigrationsHistory
AppUsers      Authors       BookImages    Books
ContentReviews Feedbacks    Orders        PressQuotes
Roles         SiteSettings
```

Migration: `20261004043055_InitialCreate`, áp dụng thành công trên DB sạch.

## 5. Chưa đạt Definition of Done

| Hạng mục | Lý do | Ảnh hưởng |
|---|---|---|
| **AC-6** (responsive 360px) | Chưa có Playwright, chưa chạy trên trình duyệt thật | Không khẳng định được layout đúng ở mobile |
| **AC-12** (màn hình cảm ơn + không gửi lại khi F5) | Code đã `reset()` sau 201 và render màn hình cảm ơn, nhưng **chưa có test tự động** | Có thể hồi quy mà không ai biết |
| Bước 7 (Code Review) | Chưa chạy reviewer context sạch | Chưa có finding nào được ghi nhận |
| Bước 8 (Challenge) | Chưa có reviewer độc lập tấn công | Rủi ro còn lỗ hổng chưa phát hiện |
| UI admin cho tác giả / báo chí / review | API đã xong và có test, UI chưa dựng | Phải sửa qua Swagger |
| Form tạo/sửa tài khoản | API đã xong và có test, UI mới ở mức xem danh sách | Phải tạo user qua Swagger |

> Theo quy tắc của quy trình: **những mục trên chưa Done.** Phần backend và phần landing page
> đã có đủ bằng chứng; phần còn lại phải nói rõ là chưa, không được gộp vào "đã xong".

## 6. Lệnh tái hiện toàn bộ

```bash
cd backend/NhaGiaKim.Api
dotnet user-secrets set "Jwt:Key" "<chuoi-ngau-nhien-32-ky-tu-tro-len>"
dotnet user-secrets set "Seed:AdminPassword" "<mat-khau-admin>"
cd ../..

dotnet ef database update -p backend/NhaGiaKim.Infrastructure -s backend/NhaGiaKim.Api
bash scripts/gates.sh

ASPNETCORE_ENVIRONMENT=Development dotnet run --project backend/NhaGiaKim.Api \
  --urls http://localhost:5080 --no-launch-profile &
npm --prefix frontend run dev
```
