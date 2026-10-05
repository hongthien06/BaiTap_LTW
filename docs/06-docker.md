# 06 — Chạy bằng Docker (ở máy cá nhân)

Ba container: **SQL Server** → **API** → **frontend (nginx)**. Không cần cài .NET, Node hay SQL Server trên máy — chỉ cần Docker.

> **Phạm vi:** stack này dùng để chạy và demo ở máy cá nhân. Không nhắm tới triển khai thật,
> nên không có HTTPS, không có cấu hình reverse proxy nhiều tầng, và SQL Server dùng bản
> Developer (miễn phí cho học tập, không được dùng cho production).

## Chạy

```bash
cp .env.example .env     # rồi mở ra đổi mật khẩu
docker compose up -d --build
```

Lần đầu mất vài phút vì phải tải image SQL Server (~2.3 GB) và build.

| Địa chỉ | Nội dung |
|---|---|
| http://localhost:8080 | Landing page |
| http://localhost:8080/admin/login | Trang quản trị |
| http://localhost:8080/swagger | Swagger |
| http://localhost:5080/health | Healthcheck của API |

Tài khoản admin được seed lần đầu: `admin@nhagiakim.local`, mật khẩu lấy từ `SEED_ADMIN_PASSWORD` trong `.env`.

## Lệnh hay dùng

```bash
docker compose ps                    # trạng thái 3 container
docker compose logs -f api           # xem log API
docker compose restart api           # khởi động lại riêng API
docker compose up -d --build api     # build lại API sau khi sửa code backend
docker compose up -d --build frontend  # build lại frontend sau khi sửa code
docker compose down                  # dừng, GIỮ dữ liệu
docker compose down -v               # dừng và XOÁ SẠCH database + file tải lên
```

Truy vấn thẳng database trong container:

```bash
docker compose exec sqlserver /opt/mssql-tools18/bin/sqlcmd \
  -S localhost -U sa -P "$MSSQL_SA_PASSWORD" -C -d NhaGiaKim \
  -Q "SELECT OrderCode, CustomerName, TotalPrice, Status FROM Orders"
```

> **Dùng Git Bash trên Windows:** thêm `MSYS_NO_PATHCONV=1` vào đầu lệnh. Không có nó, Git Bash
> sẽ biến `/opt/mssql-tools18/...` thành đường dẫn Windows và báo "no such file or directory".

## Chỉ nghe ở máy mình

Mọi cổng đều gắn vào `127.0.0.1`:

```yaml
ports:
  - "127.0.0.1:8080:80"
```

Nghĩa là **máy khác trong cùng wifi không truy cập được**. Muốn demo cho máy khác thì bỏ tiền tố
`127.0.0.1:` trong `docker-compose.yml` — nhưng nhớ là lúc đó SQL Server cũng mở ra theo, nên
hãy đổi `MSSQL_SA_PASSWORD` thành thứ tử tế trước.

Compose cũng **không đặt `restart: unless-stopped`**: đây là máy cá nhân, không phải server, nên
container không tự bật lại mỗi lần mở Docker Desktop.

## Kiến trúc

```
                  127.0.0.1:8080
                        │
                 ┌──────▼──────┐
                 │  frontend   │  nginx: phục vụ file tĩnh của SPA
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

### Vì sao cho frontend proxy `/api` thay vì gọi thẳng API

Để SPA và API **cùng một origin**. Ba cái lợi:

1. **Không cần CORS** — không phải bận tâm whitelist origin khi đổi cổng.
2. **Thuộc tính `download` của nút "Tải bản review" mới có tác dụng.** Khác origin thì trình duyệt
   **âm thầm bỏ qua** `download` và mở PDF trong tab mới — đúng lỗi m18 reviewer đã nêu.
3. Frontend không cần biết địa chỉ API lúc build.

### Thứ tự khởi động

`depends_on` thường **chỉ chờ container chạy, không chờ service sẵn sàng** — SQL Server mở cổng
trước khi nhận query hàng chục giây. Nên compose dùng `condition: service_healthy`:

- `sqlserver` healthy khi `SELECT 1` chạy được
- `api` healthy khi `/health` trả 200 (có kiểm tra cả kết nối `DbContext`)
- `frontend` chỉ khởi động sau khi `api` healthy

Ngoài ra EF Core đang bật `EnableRetryOnFailure(3)` nên vẫn chịu được nếu database chập chờn.

## Secret

Không có secret nào nằm trong image hay trong git:

| Biến | Dùng để |
|---|---|
| `MSSQL_SA_PASSWORD` | Mật khẩu SA của SQL Server |
| `JWT_KEY` | Khoá ký JWT (API từ chối khởi động nếu dưới 32 ký tự) |
| `SEED_ADMIN_PASSWORD` | Mật khẩu admin seed lần đầu |
| `SWAGGER_ENABLED` | Bật Swagger trong container để demo |

`.env` nằm trong `.gitignore`. Compose dùng cú pháp `${BIEN:?thông báo}` nên **thiếu biến là dừng
ngay** kèm thông báo, không âm thầm chạy với giá trị rỗng.

Sinh khoá JWT ngẫu nhiên: `openssl rand -base64 48`

## Dữ liệu sống qua lần khởi động lại

Đã kiểm chứng: tạo đơn hàng → `docker compose down` → `docker compose up -d` → đơn hàng vẫn còn,
vì database nằm trong volume `mssql-data` chứ không nằm trong container. Chỉ `docker compose down -v`
mới xoá.

## Một điều nên biết về rate limit trong Docker

API giới hạn 5 request/phút/IP cho `POST /api/orders` và `POST /api/feedbacks`. Khi đi qua nginx,
API thấy **IP của container nginx** chứ không phải IP thật của khách — nên giới hạn đó bị **dùng
chung cho mọi người** truy cập qua cổng 8080.

Chạy ở máy mình thì không ảnh hưởng gì (chỉ có một người dùng). Chỉ cần nhớ khi demo cho nhiều
người cùng lúc: đặt `RateLimit__PublicWritePermitLimit` cao hơn trong `docker-compose.yml`.

Cách sửa triệt để (`UseForwardedHeaders` kèm `KnownProxies`) chưa làm — xem m14 trong
[05-review-findings.md](05-review-findings.md) để biết vì sao bật bừa lại nguy hiểm hơn.

## Ba container, không phải hai

Frontend **có** trong Docker — là container `frontend` (image `nhagiakim-frontend`), chạy nginx
phục vụ bản build tĩnh của React. Kiểm tra:

```bash
docker compose ps
docker compose exec frontend ls /usr/share/nginx/html
#   assets/  img/  favicon.svg  icons.svg  index.html  50x.html
```

Khác với lúc dev: không có Vite dev server trong container. React đã được `npm run build` thành
file tĩnh ở bước build image, nginx chỉ việc phục vụ. Sửa code frontend thì phải build lại:

```bash
docker compose up -d --build frontend
```

Muốn sửa tới đâu thấy ngay tới đó thì chạy Vite trên máy (`npm --prefix frontend run dev`) và
trỏ vào API trong Docker — xem "Cách 2" ở [README](../README.md).

## Chạy test khi đang dùng Docker

`scripts/gates.sh` chạy trên máy thật, cần .NET SDK và Node — Docker không thay thế được.
Còn smoke và E2E thì chỉ cần đúng địa chỉ:

```bash
SMOKE_BASE_URL=http://localhost:5080 bash scripts/smoke.sh
E2E_BASE_URL=http://localhost:8080 npm --prefix frontend run test:e2e
```

## Bằng chứng kiểm chứng

Chạy ngày 2026-10-04 trên stack Docker với database **hoàn toàn trống**:

```
docker compose ps
  SERVICE     STATUS                  PORTS
  sqlserver   Up (healthy)            127.0.0.1:1433->1433/tcp
  api         Up (healthy)            127.0.0.1:5080->8080/tcp
  frontend    Up (healthy)            127.0.0.1:8080->80/tcp

Chạy thử từ một bản clone mới trên cổng khác (8081/5081/1434): build và khởi động được,
3 container healthy, landing và admin đều 200.

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
| `nhagiakim-frontend` | 75.6 MB |
| `nhagiakim-api` | 366 MB |
| `mcr.microsoft.com/mssql/server:2022-latest` | 2.34 GB |
