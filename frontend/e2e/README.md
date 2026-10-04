# E2E (Playwright)

## Chạy

Cần **cả hai server đang chạy** trước:

```bash
# cửa sổ 1 - API
ASPNETCORE_ENVIRONMENT=Development dotnet run --project backend/NhaGiaKim.Api \
  --urls http://localhost:5080 --no-launch-profile

# cửa sổ 2 - web
npm --prefix frontend run dev

# cửa sổ 3 - test
npm --prefix frontend run test:e2e
```

## Project

| Project | Viewport | Chạy gì |
|---|---|---|
| `desktop` | 1280×720 | Toàn bộ: đặt hàng, admin, responsive |
| `mobile-360` | 360×740 | Chỉ `responsive.spec.ts` (AC-6) |

## Lưu ý: rate limit

`POST /api/orders` bị giới hạn **5 request/phút/IP** (NFR-2). Chạy suite quá 5 lần trong
một phút sẽ khiến test đặt hàng fail với lỗi "Bạn thao tác quá nhanh" — đó là **rate limit
hoạt động đúng**, không phải lỗi code.

Muốn chạy lặp nhiều lần, khởi động API với giới hạn nới ra:

```bash
RateLimit__PublicWritePermitLimit=1000 ASPNETCORE_ENVIRONMENT=Development \
  dotnet run --project backend/NhaGiaKim.Api --urls http://localhost:5080 --no-launch-profile
```

## Dữ liệu

Test chạy trên **database thật** (`NhaGiaKim` trên LocalDB) và **có tạo đơn hàng thật**.
Đây là đánh đổi có chủ ý: muốn kiểm chứng đúng luồng end-to-end thì không mock.
Đơn do test tạo có tên `Nguyen Van E2E`, SĐT `0977000111` — xoá được bằng:

```sql
DELETE FROM Orders WHERE Phone = '0977000111';
```
