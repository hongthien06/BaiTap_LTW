# Bài tập LTW — Landing page bán sách "Nhà Giả Kim"

Website bán một đầu sách: landing page công khai + trang quản trị.

**Stack:** ASP.NET Core 9 Web API (C#) · SQL Server (LocalDB) + EF Core 9 · ReactJS 19 + Vite + TypeScript + Tailwind + Ant Design

## Cách 1 — Docker (nhanh nhất, không cần cài gì ngoài Docker)

```bash
cp .env.example .env     # mở ra đổi mật khẩu
docker compose up -d --build
```

Ba container: `sqlserver` → `api` → `frontend` (nginx phục vụ bản build React + proxy `/api`).

| Địa chỉ | Nội dung |
|---|---|
| http://localhost:8080 | Landing page |
| http://localhost:8080/admin/login | Trang quản trị |
| http://localhost:8080/swagger | Swagger |

Tài khoản admin: `admin@nhagiakim.local` / mật khẩu đặt ở `SEED_ADMIN_PASSWORD` trong `.env`.

Mọi cổng chỉ nghe ở `127.0.0.1` — máy khác trong mạng không vào được.

Dừng: `docker compose down` (giữ dữ liệu) hoặc `docker compose down -v` (xoá sạch).

Chi tiết kiến trúc, secret, giới hạn: [docs/06-docker.md](docs/06-docker.md).

## Cách 2 — Chạy trực tiếp trên máy (để phát triển)

Cần .NET SDK 9, Node 22 và SQL Server LocalDB.

### 1. Chuẩn bị secret (chỉ làm một lần)

```bash
cd backend/NhaGiaKim.Api
dotnet user-secrets set "Jwt:Key" "<chuoi-ngau-nhien-it-nhat-32-ky-tu>"
dotnet user-secrets set "Seed:AdminPassword" "<mat-khau-admin>"
```

> Secret **không** nằm trong `appsettings.json` và không được commit. Xem `appsettings.Development.example.json`.

### 2. Tạo database

```bash
dotnet ef database update -p backend/NhaGiaKim.Infrastructure -s backend/NhaGiaKim.Api
```

Dùng SQL Server LocalDB (`(localdb)\MSSQLLocalDB`, database `NhaGiaKim`). Đổi chuỗi kết nối ở `appsettings.json` nếu dùng SQL Server khác.

### 3. Chạy backend

```bash
ASPNETCORE_ENVIRONMENT=Development dotnet run --project backend/NhaGiaKim.Api \
  --urls http://localhost:5080 --no-launch-profile
```

- API: http://localhost:5080
- Swagger: http://localhost:5080/swagger
- Lần chạy đầu sẽ **seed** sách mẫu + tài khoản admin `admin@nhagiakim.local`.

### 4. Chạy frontend

```bash
cd frontend
npm install
npm run dev
```

- Landing page: http://localhost:5173
- Trang quản trị: http://localhost:5173/admin/login

Vite proxy `/api` và `/uploads` sang `http://localhost:5080` nên khi dev không vướng CORS.

## Kiểm thử

```bash
# Cong tu dong - KHONG can server chay (phai tat API truoc vi no khoa file DLL)
bash scripts/gates.sh                 # build, format, 107 test BE, lint, typecheck, 25 test FE, build FE

# Can ca hai server dang chay
bash scripts/smoke.sh                 # 18 kiem chung tren SQL Server that
npm --prefix frontend run test:e2e    # 12 test Playwright

# Chay le
dotnet test BaiTapLTW.sln             # 107 test backend
npm --prefix frontend run test:run    # 25 test frontend
```

Tổng **144 kiểm chứng tự động**: 107 backend (unit + tích hợp) · 25 frontend · 12 E2E.
`scripts/smoke.sh` chạy thêm 18 kiểm chứng trên SQL Server thật — **bắt buộc** trước khi nộp,
vì test tích hợp dùng SQLite nên không đại diện được cho SQL Server.

## Tài liệu

| File | Nội dung | Bước trong quy trình |
|---|---|---|
| [docs/00-workflow.md](docs/00-workflow.md) | Quy trình làm bài, phân tầng, automated gates, Definition of Done | Khung chung |
| [docs/01-spec.md](docs/01-spec.md) | Yêu cầu chức năng/phi chức năng + **30 Acceptance Criteria** | Bước 1 |
| [docs/02-test-design.md](docs/02-test-design.md) | Test case + bảng truy vết AC → test → bằng chứng | Bước 2 |
| [docs/03-plan.md](docs/03-plan.md) | Kiến trúc, cấu trúc thư mục, schema DB, API, 7 giai đoạn, rủi ro | Bước 3 |
| [docs/04-work-packages.md](docs/04-work-packages.md) | Chia work package, DAG, ownership file, chống xung đột | P1–P2 |
| [docs/05-review-findings.md](docs/05-review-findings.md) | Findings của reviewer độc lập và cách xử lý từng cái | Bước 7–8 |
| [docs/06-docker.md](docs/06-docker.md) | Đóng gói Docker: kiến trúc, secret, lệnh hay dùng, giới hạn | Triển khai |
| [docs/evidence/](docs/evidence/) | Raw output kiểm chứng | Bước 9 |
| [contracts/README.md](contracts/README.md) | Contract API đã đóng băng (`openapi.json`) | P2 |

Quy trình gốc: `multi_agent_coding_workflow.pdf`

## Cấu trúc

```
backend/
  NhaGiaKim.Domain/           entities + enums, không phụ thuộc gì
  NhaGiaKim.Application/      service, DTO, validator, business rule thuần
  NhaGiaKim.Infrastructure/   EF Core, SQL Server, BCrypt, JWT, file storage
  NhaGiaKim.Api/              controller, middleware, DI, Swagger
  NhaGiaKim.Tests/            65 unit + 27 integration
frontend/src/
  api/                        axios client, endpoint, type khớp contract
  components/landing/         8 section của landing page
  pages/admin/                login, đơn hàng, đánh giá, sách, cấu hình, tài khoản
contracts/openapi.json        contract sinh từ Swagger
scripts/gates.sh              cổng tự động (bước 6)
scripts/smoke.sh              18 kiểm chứng trên SQL Server thật
docker-compose.yml            3 container: sqlserver -> api -> frontend (nginx)
backend/Dockerfile            build .NET nhiều tầng, chạy bằng user không phải root
frontend/Dockerfile           build Vite -> nginx phục vụ tĩnh + proxy /api
```

## Nguyên tắc cốt lõi

> Không tin báo cáo "đã xong". Chỉ tin bằng chứng kiểm chứng độc lập được.

## Dọn dữ liệu test trước khi nộp

Smoke test và E2E tạo đơn hàng, đánh giá, tài khoản thật trong DB:

```bash
sqlcmd -S "(localdb)\MSSQLLocalDB" -d NhaGiaKim -i scripts/clean-test-data.sql
```

## Việc còn lại

- [ ] Code-splitting cho bundle frontend (hiện 1.28 MB, gzip 409 KB)
- [ ] Ảnh thật cho sách / tác giả / báo chí (hiện là SVG placeholder)
- [ ] Rate limit sau reverse proxy và token trong localStorage — xem phần
      "Minor chưa sửa, có lý do" ở [docs/05-review-findings.md](docs/05-review-findings.md)
