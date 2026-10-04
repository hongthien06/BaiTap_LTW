# 06 — Chạy bằng Docker

Ba container: **SQL Server** → **API** → **web (nginx)**. Không cần cài .NET, Node hay SQL Server trên máy — chỉ cần Docker.

## Chạy

```bash
cp .env.example .env     # rồi mở ra đổi mật khẩu
docker compose up -d --build
```

Lần đầu mất vài phút vì phải tải image SQL Server (~1.5 GB) và build.

| Địa chỉ | Nội dung |
|---|---|
| http://localhost:8080 | Landing page |
| http://localhost:8080/admin/login | Trang quản trị |
| http://localhost:8080/swagger | Swagger (qua nginx) |
| http://localhost:5080/health | Healthcheck của API |

Tài khoản admin được seed lần đầu: `admin@nhagiakim.local`, mật khẩu lấy từ `SEED_ADMIN_PASSWORD` trong `.env`.

## Lệnh hay dùng

```bash
docker compose ps                    # trạng thái 3 container
docker compose logs -f api           # xem log API
docker compose logs -f sqlserver     # xem log SQL Server
docker compose restart api           # khởi động lại riêng API
docker compose down                  # dừng, GIỮ dữ liệu
docker compose down -v               # dừng và XOÁ SẠCH database + file tải lên
docker compose up -d --build api     # build lại riêng API sau khi sửa code
```

Truy vấn thẳng database trong container:

```bash
docker compose exec sqlserver /opt/mssql-tools18/bin/sqlcmd \
  -S localhost -U sa -P "$MSSQL_SA_PASSWORD" -C -d NhaGiaKim \
  -Q "SELECT OrderCode, CustomerName, TotalPrice, Status FROM Orders"
```

## Kiến trúc

```
                     localhost:8080
                           │
                    ┌──────▼──────┐
                    │  web        │  nginx: phục vụ file tĩnh của SPA
                    │  (nginx)    │  + proxy /api, /uploads, /swagger
                    └──────┬──────┘
                           │  http://api:8080
                    ┌──────▼──────┐
                    │  api        │  ASP.NET Core 9, chạy bằng user không phải root
                    │             │  tự migrate + seed khi khởi động
                    └──────┬──────┘
                           │  Server=sqlserver,1433
                    ┌──────▼──────┐
                    │  sqlserver  │  SQL Server 2022 Developer
                    └─────────────┘

volume mssql-data   dữ liệu database, sống qua `docker compose down`
volume uploads      file admin tải lên (ảnh, PDF review)
```

### Vì sao cho web proxy `/api` thay vì gọi thẳng API

Để SPA và API **cùng một origin**. Ba cái lợi:

1. **Không cần CORS** — không phải bận tâm whitelist origin khi đổi cổng hay đổi domain.
2. **Thuộc tính `download` của nút "Tải bản review" mới có tác dụng.** Khác origin thì trình duyệt **âm thầm bỏ qua** `download` và mở PDF trong tab mới — đúng là lỗi m18 mà reviewer đã nêu.
3. Frontend không cần biết địa chỉ API lúc build, nên **một image dùng được cho mọi môi trường**.

### Thứ tự khởi động

`depends_on` thường **chỉ chờ container chạy, không chờ service sẵn sàng** — SQL Server mở cổng trước khi nhận query hàng chục giây. Nên compose dùng `condition: service_healthy`:

- `sqlserver` healthy khi `SELECT 1` chạy được
- `api` healthy khi `/health` trả 200 (có kiểm tra cả kết nối `DbContext`)
- `web` chỉ khởi động sau khi `api` healthy

Ngoài ra EF Core đang bật `EnableRetryOnFailure(3)` nên vẫn chịu được nếu database chập chờn.

## Secret

Không có secret nào nằm trong image hay trong git:

| Biến | Dùng để | Nguồn |
|---|---|---|
| `MSSQL_SA_PASSWORD` | Mật khẩu SA của SQL Server | `.env` |
| `JWT_KEY` | Khoá ký JWT (API từ chối khởi động nếu dưới 32 ký tự) | `.env` |
| `SEED_ADMIN_PASSWORD` | Mật khẩu admin seed lần đầu | `.env` |

`.env` nằm trong `.gitignore`. Compose dùng cú pháp `${BIEN:?thong bao}` nên **thiếu biến là dừng ngay** kèm thông báo, không âm thầm chạy với giá trị rỗng.

Sinh khoá JWT ngẫu nhiên:

```bash
openssl rand -base64 48
```

## Vài điểm về bảo mật trong image

- API chạy bằng `USER $APP_UID`, **không phải root**.
- `.dockerignore` loại `appsettings.Local.json`, `appsettings.Production.json`, `.env`, `*.pfx`, `logs/`, và thư mục `uploads` của môi trường dev.
- nginx đặt `X-Content-Type-Options: nosniff`, `X-Frame-Options: DENY`, `Referrer-Policy`.
- `client_max_body_size 25m` — lớn hơn giới hạn 20 MB của file PDF để chính API trả lỗi 400 có nội dung đọc được, thay vì nginx cắt ngang bằng 413.

## Giới hạn đã biết

- **Chưa có HTTPS.** Dùng thật thì đặt sau một reverse proxy có TLS (Caddy, Traefik, nginx có cert).
- **`X-Forwarded-For` chưa được tin.** nginx có gửi header này nhưng API **cố ý không bật** `UseForwardedHeaders`: bật mà không khai báo `KnownProxies` sẽ cho phép client tự giả IP và qua mặt rate limit. Hệ quả hiện tại: rate limit 5 request/phút tính theo IP của **container nginx**, tức dùng chung cho mọi khách. Xem m14 trong [05-review-findings.md](05-review-findings.md).
- **SQL Server dùng `MSSQL_PID: Developer`** — miễn phí cho phát triển và kiểm thử, **không được dùng cho production**.
- Image SQL Server không chạy trên ARM (Apple Silicon) nếu không bật emulation; cân nhắc Azure SQL Edge nếu dùng máy đó.

## Chạy test khi đang dùng Docker

`scripts/gates.sh` chạy trên máy thật, cần .NET SDK và Node — Docker không thay thế được việc đó.
Còn `smoke.sh` và E2E thì chỉ cần địa chỉ:

```bash
SMOKE_BASE_URL=http://localhost:5080 bash scripts/smoke.sh
E2E_BASE_URL=http://localhost:8080 npm --prefix frontend run test:e2e
```

## Bằng chứng kiểm chứng

Chạy ngày 2026-10-04 trên stack Docker với database **hoàn toàn trống**:

```
docker compose ps
  sqlserver   Up (healthy)
  api         Up (healthy)
  web         Up (healthy)

Qua nginx cổng 8080
  /                        -> HTTP 200
  /api/public/landing      -> HTTP 200
  /admin/login             -> HTTP 200
  /swagger/index.html      -> HTTP 200
  /swagger/v1/swagger.json -> HTTP 200
  /img/book-mockup.svg     -> HTTP 200

scripts/smoke.sh   -> PASS: 18   FAIL: 0
playwright         -> 12 passed (E2E_BASE_URL=http://localhost:8080)
```

Migration và seed chạy đúng từ database rỗng: landing trả về sách "Nha Gia Kim" giá 89.000 giảm
còn 69.000, 3 trích dẫn báo chí, 8 setting.

Kích thước image:

| Image | Dung lượng |
|---|---|
| `nhagiakim-web` | 75.6 MB |
| `nhagiakim-api` | 366 MB |
| `mcr.microsoft.com/mssql/server:2022-latest` | 2.34 GB |
