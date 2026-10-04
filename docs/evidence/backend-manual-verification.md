# Bằng chứng kiểm chứng backend (bước 9 — raw output)

Môi trường: .NET 9, SQL Server LocalDB `(localdb)\MSSQLLocalDB`, DB `NhaGiaKim`, API tại `http://localhost:5080`.
Ngày chạy: 2026-10-04. Toàn bộ output dưới đây là **raw output của curl**, không phải mô tả.

| AC | Kiểm chứng | Kết quả | Pass |
|---|---|---|---|
| AC-1 | `GET /api/public/landing` | `HTTP 200` + object có `book`, `author`, `pressQuotes`, `review`, `ratingSummary`, `settings` | ✅ |
| AC-7 | `POST /api/orders` payload hợp lệ | `HTTP 201` + `{"orderCode":"NGK-20261004-8AMZ",...}` | ✅ |
| AC-8 | `POST /api/orders` SĐT `901234567` | `HTTP 400` + `errors.Phone` | ✅ |
| AC-10 | Giá do server tính | Sách giá 89.000, giảm còn 69.000; đặt 2 cuốn → `unitPrice: 69000.00`, `totalPrice: 138000.00`. Client không gửi giá, server tự tính | ✅ |
| AC-11 | Rate limit `POST /api/orders` | Request thứ 6 trong 1 phút → `HTTP 429` (các request sau cũng 429) | ✅ |
| AC-13 | `POST /api/feedbacks` | `HTTP 201` + "Đánh giá sẽ hiển thị sau khi được duyệt" | ✅ |
| AC-14 | Feedback chưa duyệt không lên landing | `ratingSummary = {average: 0, count: 0}`, `feedbacks = []` | ✅ |
| AC-15 | `rating = 6` | `HTTP 400` | ✅ |
| AC-17 | Login đúng | `HTTP 200`, JWT dài 604 ký tự, có claim role | ✅ |
| AC-18 | Login sai mật khẩu / email không tồn tại | **Cả hai** trả `HTTP 401` + cùng message "Email hoac mat khau khong dung." → không lộ email có tồn tại | ✅ |
| AC-19 | `GET /api/admin/orders` không token | `HTTP 401` | ✅ |
| AC-20 | Staff `PUT /api/admin/book` | `HTTP 403`; cùng token đó `GET /api/admin/orders` → `HTTP 200` | ✅ |
| AC-24 | Phân trang `?page=1&pageSize=10` | `{"items":[...],"totalCount":1,"page":1,"pageSize":10,"totalPages":1}` | ✅ |
| AC-26 | `New → Confirmed` | `HTTP 200` | ✅ |
| AC-27 | `Confirmed → New` (lùi) và `Confirmed → Completed` (nhảy cóc) | Cả hai `HTTP 400` + "Khong the chuyen tu Confirmed sang New." | ✅ |
| AC-28 | Duyệt feedback → landing tính lại điểm | Sau approve: `ratingSummary = {average: 5, count: 1}` | ✅ |
| AC-29 | Upload file `MZ...` đặt tên `.jpg` | `HTTP 400` (magic bytes không khớp) | ✅ |
| — | JWT bị sửa chữ ký | `HTTP 401` | ✅ |
| AC-22 | Seed admin | DB tạo user `admin@nhagiakim.local` với `PasswordHash` BCrypt; mật khẩu lấy từ user-secrets, không có trong source | ✅ |

## Còn thiếu bằng chứng (chưa Done)

| AC | Lý do |
|---|---|
| AC-2, 3, 4, 5, 6, 12, 16, 21 | Thuộc frontend — chưa build xong |
| AC-9 | Biên số lượng 0/1/99/100 — cần unit test, chưa chạy |
| AC-23, 25, 30 | Admin CRUD phản chiếu lên landing — mới verify gián tiếp qua AC-28 |

## Lệnh tái hiện

```bash
# 1. Chuẩn bị secret (một lần)
cd backend/NhaGiaKim.Api
dotnet user-secrets set "Jwt:Key" "<chuoi-ngau-nhien-32-ky-tu-tro-len>"
dotnet user-secrets set "Seed:AdminPassword" "<mat-khau-admin>"

# 2. Tạo DB
dotnet ef database update -p backend/NhaGiaKim.Infrastructure -s backend/NhaGiaKim.Api

# 3. Chạy API
ASPNETCORE_ENVIRONMENT=Development dotnet run --project backend/NhaGiaKim.Api \
  --urls http://localhost:5080 --no-launch-profile
```

> **Lưu ý quy trình:** đây là verification thủ công bằng raw output. Theo bước 6 (Automated Gates),
> các kiểm chứng này phải được chuyển thành test tự động trong `NhaGiaKim.Tests` thì mới tính là
> gate xanh bền vững — kiểm thử thủ công không lặp lại được trong CI.
